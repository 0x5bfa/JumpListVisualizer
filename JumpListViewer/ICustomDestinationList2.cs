// Copyright (c) 0x5BFA. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Windows.Win32.Foundation;
using Windows.Win32.System.Com;

namespace JumpListViewer
{
	// https://github.com/GigabyteProductions/classicshell/blob/HEAD/src/ClassicStartMenu/ClassicStartMenuDLL/JumpLists.cpp#L397
	public unsafe partial struct ICustomDestinationList2
	{
#pragma warning disable CS0649 // Field 'field' is never assigned to, and will always have its default value 'value'
		private void** lpVtbl;
#pragma warning restore CS0649 // Field 'field' is never assigned to, and will always have its default value 'value'

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public HRESULT SetMinItems(uint dwMinItems)
			=> (HRESULT)((delegate* unmanaged[MemberFunction]<ICustomDestinationList2*, uint, int>)lpVtbl[3])(
				(ICustomDestinationList2*)Unsafe.AsPointer(ref this), dwMinItems);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public HRESULT SetApplicationID(PCWSTR pszAppID)
			=> (HRESULT)((delegate* unmanaged[MemberFunction]<ICustomDestinationList2*, PCWSTR, int>)lpVtbl[4])(
				(ICustomDestinationList2*)Unsafe.AsPointer(ref this), pszAppID);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public HRESULT GetSlotCount(uint* pdwSlotCount)
			=> (HRESULT)((delegate* unmanaged[MemberFunction]<ICustomDestinationList2*, uint*, int>)lpVtbl[5])(
				(ICustomDestinationList2*)Unsafe.AsPointer(ref this), pdwSlotCount);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public HRESULT GetCategoryCount(uint* pdwCategoryCount)
			=> (HRESULT)((delegate* unmanaged[MemberFunction]<ICustomDestinationList2*, uint*, int>)lpVtbl[6])(
				(ICustomDestinationList2*)Unsafe.AsPointer(ref this), pdwCategoryCount);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public HRESULT GetCategory(uint a1, GETCATFLAG dwFlags, APPDESTCATEGORY* pADC)
			=> (HRESULT)((delegate* unmanaged[MemberFunction]<ICustomDestinationList2*, uint, GETCATFLAG, APPDESTCATEGORY*, int>)lpVtbl[7])(
				(ICustomDestinationList2*)Unsafe.AsPointer(ref this), a1, dwFlags, pADC);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public HRESULT DeleteCategory(uint a1, int a2)
			=> (HRESULT)((delegate* unmanaged[MemberFunction]<ICustomDestinationList2*, int>)lpVtbl[8])(
				(ICustomDestinationList2*)Unsafe.AsPointer(ref this));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public HRESULT EnumerateCategoryDestinations(uint a1, Guid* a2, void** ppvObject)
			=> (HRESULT)((delegate* unmanaged[MemberFunction]<ICustomDestinationList2*, uint, Guid*, void**, int>)lpVtbl[9])(
				(ICustomDestinationList2*)Unsafe.AsPointer(ref this), a1, a2, ppvObject);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public HRESULT RemoveDestination(IUnknown* a1)
			=> (HRESULT)((delegate* unmanaged[MemberFunction]<ICustomDestinationList2*, int>)lpVtbl[10])
			((ICustomDestinationList2*)Unsafe.AsPointer(ref this));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public HRESULT HasListEx(int* a1, int* a2)
			=> (HRESULT)((delegate* unmanaged[MemberFunction]<ICustomDestinationList2*, int>)lpVtbl[11])
			((ICustomDestinationList2*)Unsafe.AsPointer(ref this));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public HRESULT ClearRemovedDestinations()
			=> (HRESULT)((delegate* unmanaged[MemberFunction]<ICustomDestinationList2*, int>)lpVtbl[12])
			((ICustomDestinationList2*)Unsafe.AsPointer(ref this));
	}

	[StructLayout(LayoutKind.Sequential)]
	public unsafe struct APPDESTCATEGORY
	{
		[StructLayout(LayoutKind.Explicit)]
		public struct _Anonymous_e__Union
		{
			[FieldOffset(0)]
			public PWSTR Name;

			[FieldOffset(0)]
			public int SubType;
		}

		public APPDESTCATEGORYTYPE Type;

		public _Anonymous_e__Union Anonymous;

		public int Count;

		public fixed int Padding[10];
	}

	public enum GETCATFLAG : uint
	{
		// 1 is the only valid value?
		DEFAULT = 1,
	}

	public enum APPDESTCATEGORYTYPE : uint
	{
		CUSTOM = 0,
		STANDARD = 1,
		TASKS = 2,    // Used @ explorer.exe!DestinationList::InsertTasks in Windows 7 (Build 7601)
	}
}
