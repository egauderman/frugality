﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel; // for ObservableCollection<CompareItem>
using System.ComponentModel; // for PropertyChangedEventArgs
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell; // for App Bar
using Microsoft.Phone.Tasks; // for MarketplaceReviewTask

namespace shopping_compare
{
	public partial class MainPage : PhoneApplicationPage
	{
		#region Data

		public ObservableCollection<CompareItem> CompareItems = new ObservableCollection<CompareItem>();

		private double _oldPricePerUnitRange = 0;

		// Application Bar buttons and data:
		private const double APP_BAR_OPACITY = 0.8;
		private ApplicationBarIconButton _addButton;
		private ApplicationBarIconButton _resetButton;
		private ApplicationBarIconButton _updateButton;
		private ApplicationBarMenuItem _aboutMenuItem;
		private ApplicationBarMenuItem _reviewMenuItem;

		#endregion Data

		public MainPage()
		{
			InitializeComponent();

			// AdBar properties:
			AdBar.ApplicationId = "924cbd72-28fe-44bf-9446-9b761635537b";
			AdBar.AdUnitId = "156684";
			AdBar.IsAutoRefreshEnabled = true;
			AdBar.IsAutoCollapseEnabled = true;
			//AdBar.ApplicationId = "test_client";
			//AdBar.AdUnitId = "TextAd";

			#region App Bar Creation

			// Note: I'm adding the app bar in code, not XAML, because you can dynamically change the app bar if you use code.
			ApplicationBar = new ApplicationBar();
			ApplicationBar.Mode = ApplicationBarMode.Default;
			ApplicationBar.Opacity = APP_BAR_OPACITY;
			ApplicationBar.IsVisible = true;
			ApplicationBar.IsMenuEnabled = true;

			// Button to add a new item:
			_addButton = new ApplicationBarIconButton();
			_addButton.IconUri = new Uri("/Images/app_bar_add.png", UriKind.Relative);
			_addButton.Text = "add item";
			_addButton.Click += _addButton_Click;
			ApplicationBar.Buttons.Add(_addButton);

			// Button to reset the list back to two blank items:
			_resetButton = new ApplicationBarIconButton();
			_resetButton.IconUri = new Uri("/Images/app_bar_reset.png", UriKind.Relative);
			_resetButton.Text = "reset";
			_resetButton.Click += _resetButton_Click;
			ApplicationBar.Buttons.Add(_resetButton);

			_updateButton = new ApplicationBarIconButton();
			_updateButton.IconUri = new Uri("/Images/app_bar_update.png", UriKind.Relative);
			_updateButton.Text = "update";
			_updateButton.Click += _updateButton_Click;

			// Link to About page:
			_aboutMenuItem = new ApplicationBarMenuItem();
			_aboutMenuItem.Text = "about";
			ApplicationBar.MenuItems.Add(_aboutMenuItem);
			_aboutMenuItem.Click += _aboutMenuItem_Click;

			// Link to Rate and Review:
			_reviewMenuItem = new ApplicationBarMenuItem();
			_reviewMenuItem.Text = "rate & review";
			ApplicationBar.MenuItems.Add(_reviewMenuItem);
			_reviewMenuItem.Click += _reviewMenuItem_Click;

			#endregion App Bar Creation

			CompareItemsItemsControl.DataContext = CompareItems;

			// Initially, 2 compare items (user can add more later):
			AddCompareItem();
			AddCompareItem();
		}

		/// <summary>
		/// Adds a new CompareItem to the list and subscribes UpdateColors to the CompareItem's PropertyChanged event
		/// </summary>
		public void AddCompareItem()
		{
			CompareItem temp = new CompareItem(CompareItems.Count+1);
			temp.PropertyChanged += UpdateColors;
			CompareItems.Add(temp);

			// Big-time hack to scroll to the bottom:
			CompareItemsScrollViewer.UpdateLayout(); // (required)
			CompareItemsScrollViewer.ScrollToVerticalOffset(double.MaxValue);
		}

		#region Event Handlers

		/// <summary>
		/// Iterates through the CompareItems and updates their ColorIndex values.  The parameter sender must be any CompareItem in the list.
		/// Note: this is called every time a property of any CompareItem changes; it only does anything if the property is PricePerUnit.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public void UpdateColors(object sender, PropertyChangedEventArgs e)
		{
			if(e.PropertyName == "PricePerUnit")
			{
				CompareItem currentCompareItem = (CompareItem)sender;



				#region First, set highestPricePerUnit and lowestPricePerUnit and pricePerUnitRange.
				double lowestPricePerUnit = double.MaxValue;
				double highestPricePerUnit = double.MinValue;
				foreach(CompareItem i in CompareItems)
				{
					if(i.Good)
					{
						if(i.PricePerUnit < lowestPricePerUnit)
						{
							lowestPricePerUnit = i.PricePerUnit;
						}
						if(i.PricePerUnit > highestPricePerUnit)
						{
							highestPricePerUnit = i.PricePerUnit;
						}
					}
				}
				if(lowestPricePerUnit == double.MaxValue || highestPricePerUnit == double.MinValue)
				{
					lowestPricePerUnit = -1;
					highestPricePerUnit = -1;
				}
				double pricePerUnitRange = highestPricePerUnit - lowestPricePerUnit;
				#endregion



				#region Then, update the colors.
				if (pricePerUnitRange == 0)
				{
					// If the pricePerUnitRange is zero, every CompareItem that's Good should have the same price per unit and be counted as Best.
					foreach (CompareItem i in CompareItems)
					{
						if (i.Good)
						{
							i.ColorIndex = 0;
						}
						else
						{
							i.ColorIndex = -1;
						}
					}
				}
				else
				{
					if (_oldPricePerUnitRange == pricePerUnitRange) // i.e. pricePerUnitRange hasn't changed
					{
						if (currentCompareItem.Good)
						{
							currentCompareItem.ColorIndex = (currentCompareItem.PricePerUnit - lowestPricePerUnit) / pricePerUnitRange;
						}
						else
						{
							currentCompareItem.ColorIndex = -1;
						}
					}
					else // i.e. pricePerUnitRange has changed
					{
						// Update the ColorIndex of each CompareItem, normalizing from 0 to 1 based on the item'output priceperunit.
						foreach (CompareItem i in CompareItems)
						{
							if (i.Good)
							{
								i.ColorIndex = (i.PricePerUnit - lowestPricePerUnit) / pricePerUnitRange;
							}
							else
							{
								i.ColorIndex = -1;
							}
						}
					}
				}
				#endregion



				// Set _oldPricePerUnitRange for next time.
				_oldPricePerUnitRange = pricePerUnitRange;
			}
		}

		private void TextBox_GotFocus(object sender, RoutedEventArgs e)
		{
			//ApplicationBar.Opacity = 1.0; // this causes the screen to sometimes not scroll up when the on-screen keyboard is brought up (due to, I believe, a multithreading race condition between this thread and whatever thread causes the screen to automatically scroll up when the on-screen keyboard is brought up.)

			if (!ApplicationBar.Buttons.Contains(_updateButton)) ApplicationBar.Buttons.Add(_updateButton);
			if (ApplicationBar.Buttons.Contains(_addButton)) ApplicationBar.Buttons.Remove(_addButton);
			if (ApplicationBar.Buttons.Contains(_resetButton)) ApplicationBar.Buttons.Remove(_resetButton);
			ApplicationBar.IsMenuEnabled = false;
		}

		private void TextBox_LostFocus(object sender, RoutedEventArgs e)
		{
			object newCurrentFocus = FocusManager.GetFocusedElement();
			if(newCurrentFocus is TextBox)
			{
				// Keep the update button on the app bar
			}
			else // if we've deselected the TextBox and focus is not on any TextBox
			{
				ApplicationBar.Opacity = APP_BAR_OPACITY;

				if (ApplicationBar.Buttons.Contains(_updateButton)) ApplicationBar.Buttons.Remove(_updateButton);
				if (!ApplicationBar.Buttons.Contains(_addButton)) ApplicationBar.Buttons.Add(_addButton);
				if (!ApplicationBar.Buttons.Contains(_resetButton)) ApplicationBar.Buttons.Add(_resetButton);
				ApplicationBar.IsMenuEnabled = true;
			}
		}

		#region App Bar button click handlers

		void _addButton_Click(object sender, EventArgs e)
		{
			AddCompareItem();
		}

		void _resetButton_Click(object sender, EventArgs e)
		{
			CompareItems.Clear();

			// Initially, 2 compare items (user can add more later):
			AddCompareItem();
			AddCompareItem();
		}

		void _updateButton_Click(object sender, EventArgs e)
		{
			TextBox currentFocus = FocusManager.GetFocusedElement() as TextBox;
			if(currentFocus == null)
			{
				throw new Exception("The item with focus is not a TextBox");
			}
			else // if we've clicked the update button while focused on a TextBox
			{
				// Set the focus to the entire MainPage, thereby removing the focus from currentFocus.  This also causes the LostFocus event to be raised (by currentFocus I believe).
				this.Focus();
			}
		}

		void _aboutMenuItem_Click(object sender, EventArgs e)
		{
			NavigationService.Navigate(new Uri("/AboutPage.xaml", UriKind.Relative));
		}

		void _reviewMenuItem_Click(object sender, EventArgs e)
		{
			MarketplaceReviewTask marketplaceReviewTask = new MarketplaceReviewTask();

			marketplaceReviewTask.Show();
		}

		#endregion App Bar button click handlers

		#endregion Event Handlers
	}
}