﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data; // for IValueConverter

namespace shopping_compare
{
	public class BestConverter : IValueConverter
	{
		/// <summary>
		/// Returns a string to display, based on the colorindex, such as "BEST PRICE PER UNIT".
		/// </summary>
		/// <param name="value"></param>
		/// <param name="targetType"></param>
		/// <param name="parameter"></param>
		/// <param name="culture"></param>
		/// <returns>A string of text to display onscreen based on the current item's colorIndex</returns>
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			// value is the ColorIndex of a CompareItem
			double colorIndex = (double)value;
			if (colorIndex == -1)
			{
				return "";
			}
			else if (colorIndex == 0)
			{
				return "BEST PRICE PER UNIT";
			}
			else if (colorIndex > 0 && colorIndex < 1)
			{
				if (colorIndex < .05) return "NEARLY BEST";
				return "";
			}
			else if (colorIndex == 1)
			{
				return "WORST PRICE PER UNIT";
			}
			else
			{
				throw new ArgumentOutOfRangeException();
			}
		}

		// Not implemented:
		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) { throw new NotImplementedException(); }
	}
}
