// Copyright (c) 0x5BFA. All rights reserved.
// Licensed under the MIT License.

using JumpListViewer.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace JumpListViewer.Views
{
	public sealed partial class MainPage : Page
	{
		private MainPageViewModel ViewModel { get; } = new();

		public MainPage()
		{
			InitializeComponent();
		}

		private void ApplicationItemsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			ViewModel.EnumerateJumpListItems();
		}
	}
}
