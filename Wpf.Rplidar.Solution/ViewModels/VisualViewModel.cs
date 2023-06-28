using Caliburn.Micro;
using RPLidarA1;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Wpf.Rplidar.Solution.Models;
using Wpf.Rplidar.Solution.Services;

namespace Wpf.Rplidar.Solution.ViewModels
{
    public class VisualViewModel : BaseViewModel
    {
        #region - Ctors -
        public VisualViewModel(IEventAggregator eventAggregator
                                , LidarService lidarService)
            : base(eventAggregator)
        {
            _lidarService = lidarService;
            locker = new object();
        }
        #endregion
        #region - Implementation of Interface -
        #endregion
        #region - Overrides -
        protected override Task OnActivateAsync(CancellationToken cancellationToken)
        {
            _lidarService.SendPoints += _lidarService_SendPoints;
            //List<Point> points = new List<Point>();
            //points.Add(new Point(100, 200));
            //points.Add(new Point(100, 300));
            //points.Add(new Point(200, 300));
            //points.Add(new Point(400, 600));
            //UpdatePoints(points);

            Scale = 10d;
            return base.OnActivateAsync(cancellationToken);
        }

        protected override Task OnDeactivateAsync(bool close, CancellationToken cancellationToken)
        {
            return base.OnDeactivateAsync(close, cancellationToken);
        }
        #endregion
        #region - Binding Methods -
        #endregion
        #region - Processes -
        private Task _lidarService_SendPoints(List<Measure> measures)
        {
            List<Point> points = new List<Point>();
            foreach (Measure measure in measures)
            {
                points.Add(new Point(measure.X, measure.Y));
            }

            UpdatePoints(points);

            return Task.CompletedTask;
            //return Task.Run(() => 
            //{
            //    lock (locker)
            //    {

            //        Debug.WriteLine($"=====Start=====");
            //        foreach (var item in measures)
            //        {
            //            Debug.WriteLine($"θ:{item.angle}, L:{item.distance}, X:{item.X}, Y:{item.Y}");
            //        }
            //        Debug.WriteLine($"=====End=====");
            //    }
            //});

        }

        private void UpdatePoints(List<Point> points)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Geometry = GenerateGeometry(points);
            });
        }

        private StreamGeometry GenerateGeometry(List<Point> points)
        {
            StreamGeometry geometry = new StreamGeometry();
            using (StreamGeometryContext ctx = geometry.Open())
            {
                ctx.BeginFigure(new Point((points.FirstOrDefault().X + XOffset) / Scale, (points.FirstOrDefault().Y + YOffset) / Scale), true, false);
                foreach (var point in points)
                {
                    var newPoint = new Point((point.X + XOffset) / Scale, (point.Y + YOffset) / Scale);
                    ctx.LineTo(newPoint, true, false);
                }
            }
            return geometry;
        }
        #endregion
        #region - IHanldes -
        #endregion
        #region - Properties -
        public StreamGeometry Geometry
        {
            get { return geometry; }
            set
            {
                if (geometry != value)
                {
                    geometry = value;
                    NotifyOfPropertyChange(() => Geometry);
                }
            }
        }

        private int _width;

        public int Width
        {
            get { return _width; }
            set
            {
                _width = value;
                NotifyOfPropertyChange(() => Width);
            }
        }

        private int _height;

        public int Height
        {
            get { return _height; }
            set
            {
                _height = value;
                NotifyOfPropertyChange(() => Height);
            }
        }

        private double _xOffset;

        public double XOffset
        {
            get { return _xOffset; }
            set
            {
                _xOffset = value;
                NotifyOfPropertyChange(() => XOffset);
            }
        }

        private double _yOffset;

        public double YOffset
        {
            get { return _yOffset; }
            set
            {
                _yOffset = value;
                NotifyOfPropertyChange(() => YOffset);
            }
        }


        private double _scale;

        public double Scale
        {
            get { return _scale; }
            set
            {
                _scale = value;
                NotifyOfPropertyChange(() => Scale);
            }
        }

        //public List<Measure> Points { get; set; } 
        #endregion
        #region - Attributes -
        private LidarService _lidarService;
        private StreamGeometry geometry;
        private object locker;
        #endregion
    }
}
