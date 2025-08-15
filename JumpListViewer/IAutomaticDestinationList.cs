// Copyright (c) 0x5BFA. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Runtime.CompilerServices;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.System.Com;

namespace JumpListViewer
{
	public unsafe partial struct IAutomaticDestinationList : IComIID
	{
#pragma warning disable CS0649 // Field 'field' is never assigned to, and will always have its default value 'value'
		private void** lpVtbl;
#pragma warning restore CS0649 // Field 'field' is never assigned to, and will always have its default value 'value'

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public HRESULT Initialize(PCWSTR szAppId, PCWSTR a2, PCWSTR a3)
			=> (HRESULT)((delegate* unmanaged[MemberFunction]<IAutomaticDestinationList*, PCWSTR, PCWSTR, PCWSTR, int>)lpVtbl[3])
				((IAutomaticDestinationList*)Unsafe.AsPointer(ref this), szAppId, a2, a3);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public HRESULT HasList(BOOL* pfHasList)
			=> (HRESULT)((delegate* unmanaged[MemberFunction]<IAutomaticDestinationList*, BOOL*, int>)lpVtbl[4])
				((IAutomaticDestinationList*)Unsafe.AsPointer(ref this), pfHasList);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public HRESULT GetList(DESTLISTTYPE dwListType, int iMaxCount, GETDESTLISTFLAGS dwFlags, Guid* riid, void** ppvObject)
			=> (HRESULT)((delegate* unmanaged[MemberFunction]<IAutomaticDestinationList*, DESTLISTTYPE, int, GETDESTLISTFLAGS, Guid*, void**, int>)lpVtbl[5])
				((IAutomaticDestinationList*)Unsafe.AsPointer(ref this), dwListType, iMaxCount, dwFlags, riid, ppvObject);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public HRESULT AddUsagePoint(IUnknown* pItem)
			=> (HRESULT)((delegate* unmanaged[MemberFunction]<IAutomaticDestinationList*, IUnknown*, int>)lpVtbl[6])
				((IAutomaticDestinationList*)Unsafe.AsPointer(ref this), pItem);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public HRESULT PinItem(IUnknown* pItem, int iPinIndex)
			=> (HRESULT)((delegate* unmanaged[MemberFunction]<IAutomaticDestinationList*, IUnknown*, int>)lpVtbl[7])
				((IAutomaticDestinationList*)Unsafe.AsPointer(ref this), pItem);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public HRESULT IsPinned(IUnknown* pObj, BOOL* fIsPinned)
			=> (HRESULT)((delegate* unmanaged[MemberFunction]<IAutomaticDestinationList*, IUnknown*, BOOL*, int>)lpVtbl[8])
				((IAutomaticDestinationList*)Unsafe.AsPointer(ref this), pObj, fIsPinned);

		[GuidRVAGen.Guid("E9C5EF8D-FD41-4F72-BA87-EB03BAD5817C")]
		public static partial ref readonly Guid Guid { get; }
	}

	public enum DESTLISTTYPE : uint
	{
		PINNED,
		RECENT,
		FREQUENT,
	}

	public enum GETDESTLISTFLAGS : uint
	{
		NONE,
		EXCLUDE_UNNAMED_DESTINATIONS,
	}
}
