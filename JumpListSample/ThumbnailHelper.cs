// Copyright (c) 0x5BFA. All rights reserved.
// Licensed under the MIT License.

using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.IO;
using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using Windows.Win32.Graphics.GdiPlus;
using Windows.Win32.System.Com;
using Windows.Win32.UI.Shell;
using Windows.Win32.UI.WindowsAndMessaging;

namespace JumpListSample
{
	public unsafe static class ThumbnailHelper
	{
		private static (Guid Format, Guid Encorder)[]? GdiEncoders;

		public static byte[]? GetThumbnail(IShellItem* pShellItem, int size = 32)
		{
			using ComPtr<IShellItemImageFactory> pShellItemImageFactory = default;
			pShellItem->QueryInterface(IID.IID_IShellItemImageFactory, (void**)pShellItemImageFactory.GetAddressOf());
			if (pShellItemImageFactory.IsNull)
				return [];

			// Get HBITMAP
			HBITMAP hBitmap = default;
			HRESULT hr = pShellItemImageFactory.Get()->GetImage(new(size, size), SIIGBF.SIIGBF_ICONONLY, &hBitmap);
			if (hr.ThrowIfFailedOnDebug().Failed)
			{
				if (!hBitmap.IsNull) PInvoke.DeleteObject(hBitmap);
				return [];
			}

			// Retrieve BITMAP data
			BITMAP bmp = default;
			if (PInvoke.GetObject(hBitmap, sizeof(BITMAP), &bmp) is 0)
			{
				if (!hBitmap.IsNull) PInvoke.DeleteObject(hBitmap);
				return [];
			}

			// Allocate buffer for flipped pixel data
			byte* flippedBits = (byte*)NativeMemory.AllocZeroed((nuint)(bmp.bmWidthBytes * bmp.bmHeight));

			// Flip the image manually row by row
			for (int y = 0; y < bmp.bmHeight; y++)
			{
				Buffer.MemoryCopy(
					(byte*)bmp.bmBits + y * bmp.bmWidthBytes,
					flippedBits + (bmp.bmHeight - y - 1) * bmp.bmWidthBytes,
					bmp.bmWidthBytes,
					bmp.bmWidthBytes
				);
			}

			// Create GpBitmap from the flipped pixel data
			GpBitmap* gpBitmap = default;
			var status = PInvoke.GdipCreateBitmapFromScan0(bmp.bmWidth, bmp.bmHeight, bmp.bmWidthBytes, 2498570, flippedBits, &gpBitmap);
			if (status is not Status.Ok)
			{
				if (flippedBits is not null) NativeMemory.Free(flippedBits);
				if (!hBitmap.IsNull) PInvoke.DeleteObject(hBitmap);
				return [];
			}

			if (!ConvertGpBitmapToByteArray(gpBitmap, out var thumbnailData))
			{
				if (!hBitmap.IsNull) PInvoke.DeleteObject(hBitmap);
				return [];
			}

			if (flippedBits is not null) NativeMemory.Free(flippedBits);
			if (!hBitmap.IsNull) PInvoke.DeleteObject(hBitmap);

			return thumbnailData;
		}

		public static byte[] GetThumbnail(IShellLinkW* pShellLink, int size = 32)
		{
			HRESULT hr = default;
			int nIconIndex = 0;
			char* pwszIconLocation = null;
			HICON hIconLarge = default;
			HICON hIconSmall = default;
			GpBitmap* pGpBitmap = null;

			try
			{
				pwszIconLocation = (char*)NativeMemory.Alloc(256);

				hr = pShellLink->GetIconLocation(pwszIconLocation, 256, &nIconIndex);

				string path = new(pwszIconLocation);

				using ComPtr<IExtractIconW> pExtractIcon = default;
				pShellLink->QueryInterface(IID.IID_IExtractIconW, (void**)pExtractIcon.GetAddressOf());
				hr = pExtractIcon.Get()->Extract(pwszIconLocation, (uint)nIconIndex, &hIconLarge, &hIconSmall, (uint)size);

				var bitmap = System.Drawing.Bitmap.FromHicon(hIconSmall);
				using var ms = new MemoryStream();
				bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);

				return ms.ToArray();
			}
			finally
			{
				if (pwszIconLocation is not null) NativeMemory.Free(pwszIconLocation);
				if (!hIconLarge.IsNull) PInvoke.DestroyIcon(hIconLarge);
				if (!hIconSmall.IsNull) PInvoke.DestroyIcon(hIconSmall);
				if (pGpBitmap is not null) PInvoke.GdipDisposeImage((GpImage*)pGpBitmap);
			}
		}

		public static bool ConvertGpBitmapToByteArray(GpBitmap* gpBitmap, out byte[]? imageData)
		{
			imageData = null;
			byte* pRawThumbnailData = null;

			try
			{
				// Get an encoder for PNG
				Guid format = Guid.Empty;
				if (PInvoke.GdipGetImageRawFormat((GpImage*)gpBitmap, &format) is not Status.Ok)
					return false;

				Guid encoder = GetEncoderClsid(format);
				if (format == PInvoke.ImageFormatJPEG || encoder == Guid.Empty)
				{
					format = PInvoke.ImageFormatPNG;
					encoder = GetEncoderClsid(format);
				}

				using ComPtr<IStream> pStream = default;
				HRESULT hr = PInvoke.CreateStreamOnHGlobal(HGLOBAL.Null, true, pStream.GetAddressOf());
				if (hr.ThrowIfFailedOnDebug().Failed)
					return false;

				if (PInvoke.GdipSaveImageToStream((GpImage*)gpBitmap, pStream.Get(), &encoder, (EncoderParameters*)null) is not Status.Ok)
					return false;

				STATSTG stat = default;
				hr = pStream.Get()->Stat(&stat, (uint)STATFLAG.STATFLAG_NONAME);
				if (hr.ThrowIfFailedOnDebug().Failed)
					return false;

				ulong statSize = stat.cbSize & 0xFFFFFFFF;
				pRawThumbnailData = (byte*)NativeMemory.Alloc((nuint)statSize);
				pStream.Get()->Seek(0L, (System.IO.SeekOrigin)STREAM_SEEK.STREAM_SEEK_SET, null);
				hr = pStream.Get()->Read(pRawThumbnailData, (uint)statSize);
				if (hr.ThrowIfFailedOnDebug().Failed)
					return false;

				imageData = new ReadOnlySpan<byte>(pRawThumbnailData, (int)statSize / sizeof(byte)).ToArray();

				return true;
			}
			finally
			{
				if (pRawThumbnailData is not null) NativeMemory.Free(pRawThumbnailData);
			}

			Guid GetEncoderClsid(Guid format)
			{
				foreach ((Guid Format, Guid Encoder) in GetGdiEncoders())
					if (Format == format)
						return Encoder;

				return Guid.Empty;
			}

			(Guid Format, Guid Encorder)[] GetGdiEncoders()
			{
				if (GdiEncoders is not null)
					return GdiEncoders;

				if (PInvoke.GdipGetImageEncodersSize(out var numEncoders, out var size) is not Status.Ok)
					return [];

				ImageCodecInfo* pImageCodecInfo = (ImageCodecInfo*)NativeMemory.Alloc(size);

				if (PInvoke.GdipGetImageEncoders(numEncoders, size, pImageCodecInfo) is not Status.Ok)
					return [];

				ReadOnlySpan<ImageCodecInfo> codecs = new(pImageCodecInfo, (int)numEncoders);
				GdiEncoders = new (Guid Format, Guid Encoder)[codecs.Length];
				for (int index = 0; index < codecs.Length; index++)
					GdiEncoders[index] = (codecs[index].FormatID, codecs[index].Clsid);

				return GdiEncoders;
			}
		}

		public static BitmapImage? ToBitmap(this byte[]? data, int decodeSize = -1)
		{
			if (data is null)
			{
				return null;
			}

			try
			{
				using var ms = new MemoryStream(data);
				var image = new BitmapImage();
				if (decodeSize > 0)
				{
					image.DecodePixelWidth = decodeSize;
					image.DecodePixelHeight = decodeSize;
				}
				image.DecodePixelType = DecodePixelType.Logical;
				image.SetSource(ms.AsRandomAccessStream());
				return image;
			}
			catch (Exception)
			{
				return null;
			}
		}
	}
}
