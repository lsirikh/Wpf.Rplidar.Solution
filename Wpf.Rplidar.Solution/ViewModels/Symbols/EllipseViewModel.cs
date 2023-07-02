using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wpf.Rplidar.Solution.ViewModels.Symbols
{
    public class EllipseViewModel : Screen
    {

        private double x;
        public double X
        {
            get { return x; }
            set
            {
                x = value;
                NotifyOfPropertyChange(() => X);
            }
        }

        private double y;
        public double Y
        {
            get { return y; }
            set
            {
                y = value;
                NotifyOfPropertyChange(() => Y);
            }
        }

        private double _ellipseWidth = 5;
        public double EllipseWidth
        {
            get { return _ellipseWidth; }
            set
            {
                _ellipseWidth = value;
                NotifyOfPropertyChange(() => EllipseWidth);
            }
        }

        private double _ellipseHeight = 5;
        public double EllipseHeight
        {
            get { return _ellipseHeight; }
            set
            {
                _ellipseHeight = value;
                NotifyOfPropertyChange(() => EllipseHeight);
            }
        }
    }
}
