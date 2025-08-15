// Copyright (c) 0x5BFA. All rights reserved.
// Licensed under the MIT License.

using Microsoft.UI.Xaml.Media.Imaging;

namespace JumpListViewer
{
	public class JumpListItem : BaseJumpListItem
	{
		public BitmapImage? Icon { get; set; }

		public bool IsPinned { get; set; }
	}
}
