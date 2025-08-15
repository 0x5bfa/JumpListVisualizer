// Copyright (c) 0x5BFA. All rights reserved.
// Licensed under the MIT License.

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;

namespace JumpListViewer
{
	public class JumpListItemTemplateSelector : DataTemplateSelector
	{
		public DataTemplate SectionItem { get; set; } = null!;

		public DataTemplate Item { get; set; } = null!;

		protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
		{
			if (item is JumpListSectionItem)
			{
				return SectionItem;
			}
			else if (item is JumpListItem)
			{
				return Item;
			}
			else
			{
				throw new ArgumentException($@"Type of ""{nameof(item)}"" is not a type expected.");
			}
		}
	}
}
