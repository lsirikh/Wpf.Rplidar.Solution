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
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Wpf.Rplidar.Solution.Models;
using Wpf.Rplidar.Solution.Services;
using Wpf.Rplidar.Solution.Utils;
using Wpf.Rplidar.Solution.Views;
using ZoomAndPan;

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
            LidarMaxLength = 6000;
            Width = 16000;
            Height = 16000;
            Scale = 1d;
            _lidarService.SendPoints += _lidarService_SendPoints;

            return base.OnActivateAsync(cancellationToken);
        }

        protected override void OnViewAttached(object view, object context)
        {
            ZoomAndPanControl = (view as VisualView).ZoomAndPanControl;
            base.OnViewAttached(view, context);
        }

        protected override Task OnDeactivateAsync(bool close, CancellationToken cancellationToken)
        {
            return base.OnDeactivateAsync(close, cancellationToken);
        }
        #endregion
        #region - Binding Methods -
        public void zoomAndPanControl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!(sender is ZoomAndPanControl zoomAndPanControl)) return;

            System.Windows.Controls.Canvas canvas = zoomAndPanControl.Content as System.Windows.Controls.Canvas;
            canvas.Focus();
            Keyboard.Focus(canvas);

            mouseButtonDown = e.ChangedButton;
            origZoomAndPanControlMouseDownPoint = e.GetPosition(ZoomAndPanControl);
            origContentMouseDownPoint = e.GetPosition(canvas);

            if ((Keyboard.Modifiers & ModifierKeys.Shift) != 0 &&
                (e.ChangedButton == MouseButton.Left ||
                 e.ChangedButton == MouseButton.Right))
            {
                // Shift + left- or right-down initiates zooming mode.
                mouseHandlingMode = MouseHandlingMode.Zooming;
            }
            else if (mouseButtonDown == MouseButton.Left)
            {
                // Just a plain old left-down initiates panning mode.
                mouseHandlingMode = MouseHandlingMode.Panning;
            }

            if (mouseHandlingMode != MouseHandlingMode.None)
            {
                // Capture the mouse so that we eventually receive the mouse up event.
                ZoomAndPanControl.CaptureMouse();
                e.Handled = true;
            }
        }

        /// <summary>
        /// Event raised on mouse up in the ZoomAndPanControl.
        /// </summary>
        public void zoomAndPanControl_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (mouseHandlingMode != MouseHandlingMode.None)
            {
                if (mouseHandlingMode == MouseHandlingMode.Zooming)
                {
                    if (mouseButtonDown == MouseButton.Left)
                    {
                        // Shift + left-click zooms in on the content.
                        ZoomIn();
                    }
                    else if (mouseButtonDown == MouseButton.Right)
                    {
                        // Shift + left-click zooms out from the content.
                        ZoomOut();
                    }
                }

                ZoomAndPanControl.ReleaseMouseCapture();
                mouseHandlingMode = MouseHandlingMode.None;
                e.Handled = true;
            }
        }

        /// <summary>
        /// Event raised on mouse move in the ZoomAndPanControl.
        /// </summary>
        public void zoomAndPanControl_MouseMove(object sender, MouseEventArgs e)
        {
            if (!(sender is ZoomAndPanControl zoomAndPanControl)) return;

            System.Windows.Controls.Canvas canvas = zoomAndPanControl.Content as System.Windows.Controls.Canvas;

            if (mouseHandlingMode == MouseHandlingMode.Panning)
            {
                //
                // The user is left-dragging the mouse.
                // Pan the viewport by the appropriate amount.
                //
                Point curContentMousePoint = e.GetPosition(canvas);
                Vector dragOffset = curContentMousePoint - origContentMouseDownPoint;

                ZoomAndPanControl.ContentOffsetY -= dragOffset.Y;
                ZoomAndPanControl.ContentOffsetX -= dragOffset.X;

                e.Handled = true;
            }
        }

        /// <summary>
        /// Event raised by rotating the mouse wheel
        /// </summary>
        public void zoomAndPanControl_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            e.Handled = true;

            if (e.Delta > 0)
            {
                ZoomIn();
            }
            else if (e.Delta < 0)
            {
                ZoomOut();
            }
        }

        /// <summary>
        /// The 'ZoomIn' command (bound to the plus key) was executed.
        /// </summary>
        private void ZoomIn_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ZoomIn();
        }

        /// <summary>
        /// The 'ZoomOut' command (bound to the minus key) was executed.
        /// </summary>
        private void ZoomOut_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ZoomOut();
        }

        /// <summary>
        /// Zoom the viewport out by a small increment.
        /// </summary>
        private void ZoomOut()
        {
            ZoomAndPanControl.ContentScale -= 0.05;
            Scale = ZoomAndPanControl.ContentScale;
        }

        /// <summary>
        /// Zoom the viewport in by a small increment.
        /// </summary>
        private void ZoomIn()
        {
            ZoomAndPanControl.ContentScale += 0.05;
            Scale = ZoomAndPanControl.ContentScale;
        }

        #endregion
        #region - Processes -
        private Task _lidarService_SendPoints(List<Measure> measures)
        {
            List<Point> points = new List<Point>();
            foreach (Measure measure in measures)
            {
                var x = measure.X + (Width / 2);
                var y = Height / 2 - measure.Y;

                points.Add(new Point(x, y));
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
            //            Debug.WriteLine($"θ:{item.angle}, L:{item.distance}, X:{item.X}, Y:{Height - item.Y}");
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

                for (int i = 0; i < points.Count; i++)
                {
                    Point point = RotatePointAroundPivot(points[i], new Point(Width/2, Height/2), -5);
                    if(i == 0)
                    {
                        //ctx.BeginFigure(new Point((point.X + XOffset) / Scale, (point.Y + YOffset) / Scale), true, false);
                        ctx.BeginFigure(new Point(point.X, point.Y), true, false);
                    }

                    //var newPoint = new Point((point.X + XOffset) / Scale, (point.Y + YOffset) / Scale);
                    var newPoint = new Point(point.X, point.Y);
                    ctx.LineTo(newPoint, true, false);
                }
                
            }
            geometry.Freeze();
            return geometry;
        }

        public Point RotatePoint(Point point, double theta)
        {
            double radian = theta * Math.PI / 180; // Convert degree to radian
            double cosTheta = Math.Cos(radian);
            double sinTheta = Math.Sin(radian);
            double x = point.X * cosTheta - point.Y * sinTheta;
            double y = point.X * sinTheta + point.Y * cosTheta;
            return new Point(x, y);
        }

        public Point RotatePointAroundPivot(Point point, Point pivot, double theta)
        {
            // Translate point back to origin
            point.X -= pivot.X;
            point.Y -= pivot.Y;

            // Rotate point
            double radian = theta * Math.PI / 180; // Convert degree to radian
            double cosTheta = Math.Cos(radian);
            double sinTheta = Math.Sin(radian);
            double xNew = point.X * cosTheta - point.Y * sinTheta;
            double yNew = point.X * sinTheta + point.Y * cosTheta;

            // Translate point back
            Point newPoint = new Point(xNew + pivot.X, yNew + pivot.Y);
            return newPoint;
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

        private double _width;

        public double Width
        {
            get { return _width; }
            set
            {
                _width = value;
                NotifyOfPropertyChange(() => Width);
            }
        }

        private double _height;

        public double Height
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

        private double _lidarMaxLength;

        public double LidarMaxLength
        {
            get { return _lidarMaxLength / Scale; }
            set 
            { 
                _lidarMaxLength = value;
                NotifyOfPropertyChange(() => LidarMaxLength);
            }
        }

        public ZoomAndPanControl ZoomAndPanControl { get; set; }

        public double ContentScale
        {
            get { return _contentScale; }
            set
            {
                _contentScale = value;
                NotifyOfPropertyChange(() => ContentScale);
            }
        }

        public double ContentOffsetX
        {
            get { return _contentOffsetX; }
            set
            {
                _contentOffsetX = value;
                NotifyOfPropertyChange(() => ContentOffsetX);
            }
        }

        public double ContentOffsetY
        {
            get { return _contentOffsetY; }
            set
            {
                _contentOffsetY = value;
                NotifyOfPropertyChange(() => ContentOffsetY);
            }
        }


        public void Pan(double deltaX, double deltaY)
        {
            ContentOffsetX += deltaX;
            ContentOffsetY += deltaY;
        }


        //public List<Measure> Points { get; set; } 
        #endregion
        #region - Attributes -
        private LidarService _lidarService;
        private StreamGeometry geometry;
        private object locker;

        private double _contentScale = 1.0;
        private double _contentOffsetX = 0.0;
        private double _contentOffsetY = 0.0;
        private MouseHandlingMode mouseHandlingMode;
        private MouseButton mouseButtonDown;
        private Point origZoomAndPanControlMouseDownPoint;
        private Point origContentMouseDownPoint;
        #endregion
    }
}
