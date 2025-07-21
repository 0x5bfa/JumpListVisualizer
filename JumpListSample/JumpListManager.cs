// Copyright (c) 0x5BFA. All rights reserved.
// Licensed under the MIT License.

using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.GdiPlus;
using Windows.Win32.System.Com;
using Windows.Win32.UI.Shell;
using Windows.Win32.UI.Shell.Common;

namespace JumpListSample
{
	public unsafe partial class JumpListManager : IDisposable
	{
		IAutomaticDestinationList* _autoDestListPtr = default;
		ICustomDestinationList* _customDestListPtr = default;
		ICustomDestinationList2* _customDestList2Ptr = default;

		public static JumpListManager? Initialize(string szAppId)
		{
			HRESULT hr = default;

			IAutomaticDestinationList* autoDestListPtr = default;
			ICustomDestinationList* customDestListPtr = default;
			ICustomDestinationList2* customDestList2Ptr = default;

			hr = PInvoke.CoCreateInstance(CLSID.CLSID_AutomaticDestinationList, null, CLSCTX.CLSCTX_INPROC_SERVER, IID.IID_IAutomaticDestinationList, (void**)&autoDestListPtr);
			hr = PInvoke.CoCreateInstance(CLSID.CLSID_DestinationList, null, CLSCTX.CLSCTX_INPROC_SERVER, IID.IID_ICustomDestinationList, (void**)&customDestListPtr);
			hr = PInvoke.CoCreateInstance(CLSID.CLSID_DestinationList, null, CLSCTX.CLSCTX_INPROC_SERVER, IID.IID_ICustomDestinationList2, (void**)&customDestList2Ptr);

			fixed (char* pszAppId = szAppId)
			{
				hr = autoDestListPtr->Initialize(pszAppId, default, default);
				hr = customDestListPtr->SetAppID(pszAppId);
				hr = customDestList2Ptr->SetApplicationID(pszAppId);
			}

			return /*hr.Failed ? null : */new() { _autoDestListPtr = autoDestListPtr, _customDestListPtr = customDestListPtr, _customDestList2Ptr = customDestList2Ptr };
		}

		public bool HasListOf(DESTLISTTYPE type)
		{
			HRESULT hr = default;

			Guid IID_IObjectCollection = IObjectCollection.IID_Guid;
			using ComPtr<IObjectCollection> pObjectCollection = default;

			BOOL fHasList = default;
			hr = _autoDestListPtr->HasList(&fHasList);
			if ((bool)fHasList is false) return false;

			hr = _autoDestListPtr->GetList(type, 1, GETDESTLISTFLAGS.NONE, &IID_IObjectCollection, (void**)pObjectCollection.GetAddressOf());

			hr = pObjectCollection.Get()->GetCount(out uint pcObjects);

			return pcObjects is not 0U;
		}

		public IEnumerable<JumpListItem> EnumerateAutomaticDestinations(DESTLISTTYPE type, int count = 20)
		{
			HRESULT hr = default;

			Guid IID_IObjectCollection = IObjectCollection.IID_Guid;
			using ComPtr<IObjectCollection> pObjectCollection = default;

			hr = _autoDestListPtr->GetList(type, count, GETDESTLISTFLAGS.NONE, &IID_IObjectCollection, (void**)pObjectCollection.GetAddressOf());
			hr = pObjectCollection.Get()->GetCount(out uint pcObjects);

			List<JumpListItem> items = [];

			GdiplusStartupInput gpsi = default;
			gpsi.GdiplusVersion = 1;
			nuint dwGdiPlusToken;
			var status = PInvoke.GdiplusStartup(&dwGdiPlusToken, &gpsi, null);

			for (uint index = 0U; index < pcObjects; index++)
			{
				using ComPtr<IShellItem> pShellItem = default;

				hr = pObjectCollection.Get()->GetAt(index, IID.IID_IShellItem, (void**)pShellItem.GetAddressOf());

				PWSTR pszName = default;
				hr = pShellItem.Get()->GetDisplayName(SIGDN.SIGDN_NORMALDISPLAY, &pszName);

				int fIsPinned = default;

				_autoDestListPtr->IsPinned((IUnknown*)pShellItem.Get(), &fIsPinned);

				BitmapImage image;
				var imageAsByteArray = ThumbnailHelper.GetThumbnail(pShellItem, (int)(32 * App.Dpi));
				image = imageAsByteArray.ToBitmap()!;

				items.Add(new() { Icon = image, Text = new string(pszName), IsPinned = true });

				//System.Diagnostics.Debug.WriteLine($"Idx {index}: {pszName} ({fIsPinned})");
			}

			PInvoke.GdiplusShutdown(dwGdiPlusToken);

			return items;
		}

		public IEnumerable<JumpListItem> EnumerateCustomDestinations()
		{
			HRESULT hr = default;

			Guid IID_IObjectCollection = IObjectCollection.IID_Guid;
			using ComPtr<IObjectCollection> pObjectCollection = default;

			return [];

			hr = _customDestList2Ptr->EnumerateCategoryDestinations(0, &IID_IObjectCollection, (void**)pObjectCollection.GetAddressOf());
			hr = pObjectCollection.Get()->GetCount(out uint pcObjects);

			for (uint dwIndex = 0U; dwIndex < pcObjects; dwIndex++)
			{
				using ComPtr<IShellItem> pShellItem = default;
				PWSTR pszName = default;

				hr = pObjectCollection.Get()->GetAt(dwIndex, IID.IID_IShellItem, (void**)pShellItem.GetAddressOf());
				hr = pShellItem.Get()->GetDisplayName(SIGDN.SIGDN_NORMALDISPLAY, &pszName);

				System.Diagnostics.Debug.WriteLine($"Idx {dwIndex}: {pszName}");

				// Use: 507101CD-F6AD-46C8-8E20-EEB9E6BAC47F
			}

			return [];
		}

		public void Dispose()
		{
			if (_autoDestListPtr is not null)
				((IUnknown*)_autoDestListPtr)->Release();
		}
	}
}
