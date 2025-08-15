// Copyright (c) 0x5BFA. All rights reserved.
// Licensed under the MIT License.

using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.GdiPlus;
using Windows.Win32.System.Com;
using Windows.Win32.System.Com.StructuredStorage;
using Windows.Win32.UI.Shell;
using Windows.Win32.UI.Shell.Common;
using Windows.Win32.UI.Shell.PropertiesSystem;

namespace JumpListSample
{
	public unsafe partial class JumpListManager : IDisposable
	{
		IAutomaticDestinationList* _autoDestListPtr = default;
		ICustomDestinationList* _customDestListPtr = default;
		ICustomDestinationList2* _customDestList2Ptr = default;

		public static JumpListManager Initialize(string szAppId)
		{
			HRESULT hr = default;

			IAutomaticDestinationList* autoDestListPtr = default;
			ICustomDestinationList* customDestListPtr = default;
			ICustomDestinationList2* customDestList2Ptr = default;

			hr = PInvoke.CoCreateInstance(CLSID.CLSID_AutomaticDestinationList, null, CLSCTX.CLSCTX_INPROC_SERVER, IID.IID_IAutomaticDestinationList, (void**)&autoDestListPtr).ThrowOnFailure();
			hr = PInvoke.CoCreateInstance(CLSID.CLSID_DestinationList, null, CLSCTX.CLSCTX_INPROC_SERVER, IID.IID_ICustomDestinationList, (void**)&customDestListPtr).ThrowOnFailure();
			hr = PInvoke.CoCreateInstance(CLSID.CLSID_DestinationList, null, CLSCTX.CLSCTX_INPROC_SERVER, IID.IID_ICustomDestinationList2, (void**)&customDestList2Ptr).ThrowOnFailure();

			fixed (char* pwszAppId = szAppId)
			{
				// These internally convert the passed AMUID string to the corresponding CRC hash and initialize the path to the destination lists.
				hr = autoDestListPtr->Initialize(pwszAppId, default, default).ThrowOnFailure();
				hr = customDestListPtr->SetAppID(pwszAppId).ThrowOnFailure();
				hr = customDestList2Ptr->SetApplicationID(pwszAppId).ThrowOnFailure();
			}

			return new() { _autoDestListPtr = autoDestListPtr, _customDestListPtr = customDestListPtr, _customDestList2Ptr = customDestList2Ptr };
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

			return CreateCollectionFromIObjectCollection(pObjectCollection.Get());
		}

		public IEnumerable<BaseJumpListItem> EnumerateCustomDestinations()
		{
			HRESULT hr = default;

			uint dwCategoryCount = 0U;
			hr = _customDestList2Ptr->GetCategoryCount(&dwCategoryCount);

			List<BaseJumpListItem> items = [];

			for (uint dwCategoryIndex = 0U; dwCategoryIndex < dwCategoryCount; dwCategoryIndex++)
			{
				APPDESTCATEGORY category = default;
				char* pszCategoryName = null;

				try
				{
					// Get the category data (e.g., the type, the name, and the count of the destinations)
					hr = _customDestList2Ptr->GetCategory(dwCategoryIndex, GETCATFLAG.DEFAULT, &category).ThrowOnFailure();

					// Get the category name
					pszCategoryName = (char*)NativeMemory.AllocZeroed(256);
					PInvoke.SHLoadIndirectString(category.Anonymous.Name, pszCategoryName, 256, null);
					string categoryName = category.Type is APPDESTCATEGORYTYPE.TASKS ? "Tasks" : new(pszCategoryName);

					// Add the category header item
					items.Add(new JumpListSectionItem() { Text = categoryName });

					// Enumerate and add the destinations in the category to the list
					using ComPtr<IObjectCollection> pObjectCollection = default;
					hr = _customDestList2Ptr->EnumerateCategoryDestinations(dwCategoryIndex, IID.IID_IObjectCollection, (void**)pObjectCollection.GetAddressOf()).ThrowOnFailure();
					items.AddRange(CreateCollectionFromIObjectCollection(pObjectCollection.Get()));
				}
				finally
				{
					if (pszCategoryName is not null) NativeMemory.Free(pszCategoryName);
					if (category.Anonymous.Name.Value is not null) PInvoke.CoTaskMemFree(category.Anonymous.Name);
				}
			}

			return items;
		}

		private IEnumerable<JumpListItem> CreateCollectionFromIObjectCollection(IObjectCollection* pObjectCollection)
		{
			HRESULT hr = default;

			List<JumpListItem> items = [];

			hr = pObjectCollection->GetCount(out uint pcObjects);

			for (uint dwIndex = 0U; dwIndex < pcObjects; dwIndex++)
			{
				using ComPtr<IUnknown> pObj = default;
				hr = pObjectCollection->GetAt(dwIndex, IID.IID_IUnknown, (void**)pObj.GetAddressOf());
				items.Add(CreateItemFromIUnknown(pObj.Get()));
			}

			return items;
		}

		private JumpListItem CreateItemFromIUnknown(IUnknown* pObj)
		{
			HRESULT hr = default;
			using ComPtr<IShellItem> pShellItem = default;
			using ComPtr<IShellLinkW> pShellLink = default;

			BOOL pfIsPinned = default;
			_autoDestListPtr->IsPinned(pObj, &pfIsPinned);

			hr = pObj->QueryInterface(IID.IID_IShellItem, (void**)pShellItem.GetAddressOf());
			if (hr.Succeeded)
			{
				using ComHeapPtr<char> pwszName = default;
				hr = pShellItem.Get()->GetDisplayName(SIGDN.SIGDN_NORMALDISPLAY, (PWSTR*)pwszName.GetAddressOf());

				BitmapImage? bitmapImage = ThumbnailHelper.GetThumbnail(pShellItem.Get(), (int)(32 * App.Dpi)).ToBitmap();

				return new() { Icon = bitmapImage, Text = new string(pwszName.Get()), IsPinned = pfIsPinned };
			}
			else
			{
				hr = pObj->QueryInterface(IID.IID_IShellLinkW, (void**)pShellLink.GetAddressOf()).ThrowOnFailure();

				using ComPtr<IPropertyStore> pPropertyStore = default;
				PROPERTYKEY pKey = PInvoke.PKEY_Title;
				PROPVARIANT pVar = default;

				pShellLink.Get()->QueryInterface(IID.IID_IPropertyStore, (void**)pPropertyStore.GetAddressOf());
				hr = pPropertyStore.Get()->GetValue(&pKey, &pVar);

				var bitmapImage = ThumbnailHelper.GetThumbnail(pShellLink.Get())?.ToBitmap();

				return new() { Icon = bitmapImage, Text = new string(pVar.Anonymous.Anonymous.Anonymous.pwszVal), IsPinned = pfIsPinned };
			}
		}

		public void Dispose()
		{
			if (_autoDestListPtr is not null) ((IUnknown*)_autoDestListPtr)->Release();
			if (_customDestListPtr is not null) ((IUnknown*)_customDestListPtr)->Release();
			if (_customDestList2Ptr is not null) ((IUnknown*)_customDestList2Ptr)->Release();
		}
	}
}
