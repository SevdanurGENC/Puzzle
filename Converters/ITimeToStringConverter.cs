/*
Copyright © 2012, Shai Raiten
All rights reserved.
http://blogs.microsoft.co.il/blogs/shair
 */
using System;
using System.Windows.Data;

namespace Puzzle15.Converters
{
    public class ITimeToStringConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var time = (DateTime) value;
            return string.Format("{0}", time.ToString("HH:mm:ss"));
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
