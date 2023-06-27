using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Wpf.Rplidar.Solution.Utils
{
    public class OffsetConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var key = "";

            if (parameter.ToString() == "X")
                key = "HORIZONTAL";
            else if (parameter.ToString() == "Y")
                key = "VERTICAL";
            else
                key = "NONE";

            double size = (double)value;

            if(key == "HORIZONTAL")
                return size + (1000)/2;
            else if(key == "VERTICAL")
                return size + (1000)/2;
            else 
                return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
