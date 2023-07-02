using Caliburn.Micro;
using RPLidarA1;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.Remoting.Contexts;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Wpf.Rplidar.Solution.Controls;
using Wpf.Rplidar.Solution.Models;
using Wpf.Rplidar.Solution.Services;
using Wpf.Rplidar.Solution.Utils;
using Wpf.Rplidar.Solution.ViewModels.Symbols;
using Wpf.Rplidar.Solution.Views;
using ZoomAndPan;
using static System.Net.Mime.MediaTypeNames;
using Application = System.Windows.Application;

namespace Wpf.Rplidar.Solution.ViewModels
{
    public class VisualViewModel : BaseViewModel
    {
        #region - Ctors -
        public VisualViewModel(IEventAggregator eventAggregator
                                , LidarService lidarService
                                , TcpServerService tcpServerService)
            : base(eventAggregator)
        {
            _lidarService = lidarService;
            _tcpServerService = tcpServerService;
            locker = new object();
        }
        #endregion
        #region - Implementation of Interface -
        #endregion
        #region - Overrides -
        protected override Task OnActivateAsync(CancellationToken cancellationToken)
        {
            LidarMaxLength = 6000;
            Width = 1600;
            Height = 1600;
            Scale = 100;

            OffsetAngle = -5d;
            _lidarService.SendPoints += _lidarService_SendPoints;
            Points = new List<Point>();
            BoundaryPoints = new PointCollection();

            DivideOffset = 4d;
            return base.OnActivateAsync(cancellationToken);
        }

        protected override void OnViewAttached(object view, object context)
        {
            ZoomAndPanControl = (view as VisualView).ZoomAndPanControl;
            Canvas = (view as VisualView).canvas;
            base.OnViewAttached(view, context);
        }

        protected override Task OnDeactivateAsync(bool close, CancellationToken cancellationToken)
        {
            return base.OnDeactivateAsync(close, cancellationToken);
        }
        #endregion
        #region - Binding Methods -
        public void scroller_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!((sender as ScrollViewer).Content is ZoomAndPanControl zoomAndPanControl)) return;

            Canvas.Focus();
            Keyboard.Focus(Canvas);

            mouseButtonDown = e.ChangedButton;
            origZoomAndPanControlMouseDownPoint = e.GetPosition(ZoomAndPanControl);
            origContentMouseDownPoint = e.GetPosition(Canvas);

            if ((Keyboard.Modifiers & ModifierKeys.Shift) != 0 &&
                (e.ChangedButton == MouseButton.Left ||
                 e.ChangedButton == MouseButton.Right))
            {
                // Shift + left- or right-down initiates zooming mode.
                mouseHandlingMode = MouseHandlingMode.Zooming;
            }
            else if (mouseButtonDown == MouseButton.Middle)
            {
                // Just a plain old left-down initiates panning mode.
                mouseHandlingMode = MouseHandlingMode.Panning;
            }
            else if (mouseButtonDown == MouseButton.Left)
            {
                // Just a plain old left-down initiates panning mode.
                mouseHandlingMode = MouseHandlingMode.AddBoundary;
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
        public void scroller_MouseUp(object sender, MouseButtonEventArgs e)
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
                else if(mouseHandlingMode == MouseHandlingMode.AddBoundary)
                {
                    if (IsCompleted)
                    {
                        BoundaryPoints = new PointCollection();
                        PathGeometry.Clear();
                    }

                    var ePoint = new EllipseViewModel();
                    ePoint.X = origContentMouseDownPoint.X - ePoint.EllipseWidth / 2;
                    ePoint.Y = origContentMouseDownPoint.Y - ePoint.EllipseHeight / 2;
                    Ellipses.Add(ePoint);

                    BoundaryPoints.Add(origContentMouseDownPoint);

                    if (BoundaryPoints.Count > 3)
                    {
                        (RelativeWidth, RelativeHeight)=CalculateWidthAndHeight(BoundaryPoints.ToList<Point>());
                        CreatePathGeometry(BoundaryPoints.ToList<Point>());
                        var point = _boundaryPoints.FirstOrDefault();
                        BoundaryPoints.Add(point);
                        BoundaryPoints = new PointCollection(BoundaryPoints);

                        Ellipses.Clear();
                        IsCompleted = true;
                    }
                    else
                    {
                        IsCompleted = false;
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
        public void scroller_MouseMove(object sender, MouseEventArgs e)
        {
            if (!((sender as ScrollViewer).Content is ZoomAndPanControl zoomAndPanControl)) return;


            if (mouseHandlingMode == MouseHandlingMode.Panning)
            {
                //
                // The user is left-dragging the mouse.
                // Pan the viewport by the appropriate amount.
                //
                Point curContentMousePoint = e.GetPosition(Canvas);
                Vector dragOffset = curContentMousePoint - origContentMouseDownPoint;

                ZoomAndPanControl.ContentOffsetX -= dragOffset.X;
                ZoomAndPanControl.ContentOffsetY -= dragOffset.Y;

                e.Handled = true;
            }
           
        }


        public void canvas_MouseMove(object sender, MouseEventArgs e)
        {
            Point curContentMousePoint = e.GetPosition(Canvas);

            CurrentX = (curContentMousePoint.X - Width / 2) / DivideOffset;
            CurrentY = (curContentMousePoint.Y - Height / 2) / DivideOffset;


            if (IsCompleted)
            {
                var rOrigin = BoundaryPoints.FirstOrDefault();
                rOrigin.X = curContentMousePoint.X;
            }
        }

        public void relativeScreen_MouseLeave(object sender, MouseEventArgs e)
        {

            RelativeX = 0d;
            RelativeY = 0d;
        }


        public void relativeScreen_MouseMove(object sender, MouseEventArgs e)
        {
            if (IsCompleted)
            {

                var polyLine = sender as Polyline;
                Point curContentMousePoint = e.GetPosition(polyLine);

                //if (PathGeometry.FillContains(curContentMousePoint))
                //    Debug.WriteLine($"Boundary에 포함된 포인트 입니다.~!!");

                var rOrigin = BoundaryPoints.FirstOrDefault();
                RelativeX = (curContentMousePoint.X - rOrigin.X) / DivideOffset;
                RelativeY = (curContentMousePoint.Y - rOrigin.Y) / DivideOffset;
            }
        }

        public void canvas_MouseLeave(object sender, MouseEventArgs e)
        {

            CurrentX = 0d;
            CurrentY = 0d;
        }

        /// <summary>
        /// Event raised by rotating the mouse wheel
        /// </summary>
        public void scroller_MouseWheel(object sender, MouseWheelEventArgs e)
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
            if(ZoomAndPanControl.ContentScale < 0.1)
            {
                ZoomAndPanControl.ContentScale -= 0.01;
            }
            else
            {
                ZoomAndPanControl.ContentScale -= 0.1;
            }
            Scale = ZoomAndPanControl.ContentScale * 100;
        }

        /// <summary>
        /// Zoom the viewport in by a small increment.
        /// </summary>
        private void ZoomIn()
        {
            if (ZoomAndPanControl.ContentScale < 0.1)
            {
                ZoomAndPanControl.ContentScale += 0.01;
            }
            else
            {
                ZoomAndPanControl.ContentScale += 0.1;
            }

            Scale = ZoomAndPanControl.ContentScale * 100;
        }

        #endregion
        #region - Processes -
        private Task _lidarService_SendPoints(List<Measure> measures)
        {
            try
            {
                ValidCount = 0;
                if (Points.Count > 0) return Task.CompletedTask;

                //measures = RemoveNoise(measures, 20, 2);

                foreach (Measure measure in measures)
                {
                    //var x = (measure.X + (EllipseWidth / 2));
                    //var y = (EllipseHeight / 2 - measure.Y);
                    var x = (measure.X / 10) + (Width / 2);
                    var y = Height / 2 - (measure.Y / 10);

                    var point = RotatePointAroundPivot(new Point(x, y), new Point(Width / 2, Height / 2), OffsetAngle);

                    _ = CheckPointTask(point);
                    Points.Add(point);
                }
            }
            catch (Exception)
            {
                throw;
            }
            return Task.CompletedTask;

            //UpdatePoints(points);
            #region Debugging 
            //return Task.Run(() =>
            //{
            //    lock (locker)
            //    {

            //        Debug.WriteLine($"=====Start=====");
            //        foreach (var item in measures)
            //        {
            //            Debug.WriteLine($"θ:{item.angle}, L:{item.distance}, X:{item.X}, Y:{EllipseHeight - item.Y}");
            //        }
            //        Debug.WriteLine($"=====End=====");
            //    }
            //});
            #endregion
        }

        public List<Measure> RemoveNoise(List<Measure> points, int range, int minPoints)
        {
            List<Measure> result = new List<Measure>();

            foreach (var point in points)
            {
                var nearbyPoints = points.Where(p => Math.Abs(p.X - point.X) <= range && Math.Abs(p.Y - point.Y) <= range);
                if (nearbyPoints.Count() >= minPoints)
                {
                    result.Add(point);
                }
            }

            return result;
        }
        

        private Task CheckPointTask(Point point)
        {
            return Task.Run(() =>
            {
                try
                {
                    Application.Current?.Dispatcher?.Invoke(() =>
                    {
                        //var testPoint = new Point(point.X / DivideOffset, point.Y / DivideOffset);
                        if (PathGeometry != null && PathGeometry.FillContains(point))
                        {
                            //Transfer Service
                            var originPoint = BoundaryPoints.FirstOrDefault();
                            var newPoint = new Point(((point.X - originPoint.X) / DivideOffset), ((point.Y - originPoint.Y) / DivideOffset));
                            var model = new LidarDataModel(RelativeWidth, RelativeHeight, newPoint.X, newPoint.Y);
                            _tcpServerService.SendAsync(model);
                            //Debug.WriteLine($"({(point.X - originPoint.X) / DivideOffset}, {(point.Y - originPoint.Y) / DivideOffset}) => 수집된 위치는 영역에 포함됩니다.!!");
                        }
                    });
                }
                catch (Exception ex)
                {

                    Debug.WriteLine($"Raised Exception in {nameof(CheckPointTask)} : {ex.Message}");
                }
            });
        }

        //private void UpdatePoints(List<Point> points)
        //{
        //    Application.Current.Dispatcher.Invoke(() =>
        //    {
        //        Geometry = GenerateGeometry(points);

        //    });
        //}


        //private StreamGeometry GenerateGeometry(List<Point> points)
        //{
        //    StreamGeometry geometry = new StreamGeometry();
        //    using (StreamGeometryContext ctx = geometry.Open())
        //    {

        //        for (int i = 0; i < points.Count; i++)
        //        {
        //            Point point = RotatePointAroundPivot(points[i], new Point(Width/2, Height/2), OffsetAngle);
        //            if(i == 0)
        //            {
        //                //ctx.BeginFigure(new Point((point.X + XOffset) / Scale, (point.Y + YOffset) / Scale), true, false);
        //                ctx.BeginFigure(new Point(point.X, point.Y), true, false);
        //            }

        //            //var newPoint = new Point((point.X + XOffset) / Scale, (point.Y + YOffset) / Scale);
        //            var newPoint = new Point(point.X, point.Y);
        //            ctx.LineTo(newPoint, true, false);
        //        }
                
        //    }
        //    geometry.Freeze();
        //    return geometry;
        //}

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

        public (double Width, double Height) CalculateWidthAndHeight(List<Point> points)
        {
            if (points == null || points.Count < 4)
            {
                throw new ArgumentException("At least 4 points are required", nameof(points));
            }

            double minX = points.Min(point => point.X / DivideOffset);
            double maxX = points.Max(point => point.X / DivideOffset);

            double minY = points.Min(point => point.Y / DivideOffset);
            double maxY = points.Max(point => point.Y / DivideOffset);

            double width = maxX - minX;
            double height = maxY - minY;

            return (width, height);
        }

        public Point MapToResolution(double canvasWidth, double canvasHeight, double pointX, double pointY)
        {
            // Calculate the ratio of the old size to the new size
            double widthRatio = 1920 / canvasWidth;
            double heightRatio = 1080 / canvasHeight;

            // Multiply the original coordinates by the ratio to get the new coordinates
            double newX = pointX * widthRatio;
            double newY = pointY * heightRatio;

            return new Point(newX, newY);
        }

        public void CreatePathGeometry(List<Point> points)
        {
            if (points == null || points.Count != 4)
            {
                throw new ArgumentException("Exactly 4 points are required", nameof(points));
            }

            PathGeometry = new PathGeometry();
            PathFigure pathFigure = new PathFigure();
            pathFigure.StartPoint = points[0];
            pathFigure.Segments.Add(new LineSegment(points[1], true));
            pathFigure.Segments.Add(new LineSegment(points[2], true));
            pathFigure.Segments.Add(new LineSegment(points[3], true));
            pathFigure.IsClosed = true;
            PathGeometry.Figures.Add(pathFigure);
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

        private double _offsetAngle;

        public double OffsetAngle
        {
            get { return _offsetAngle; }
            set 
            { 
                _offsetAngle = value;
                NotifyOfPropertyChange(() => OffsetAngle);
            }
        }


        public ZoomAndPanControl ZoomAndPanControl { get; set; }
        public DrawingCanvas Canvas { get; private set; }

        //public Canvas Canvas { get; private set; }

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
            get { return ZoomAndPanControl.ContentOffsetX; }
            
        }

        public double ContentOffsetY
        {
            get { return ZoomAndPanControl.ContentOffsetY; }
            
        }


        private List<Point> _points;

        public List<Point> Points
        {
            get { return _points; }
            set
            {
                _points = value;
                NotifyOfPropertyChange(() => Points);
            }
        }


        private double _currentX;

        public double CurrentX
        {
            get { return _currentX; }
            set 
            { 
                _currentX = value;
                NotifyOfPropertyChange(() => CurrentX);
            }
        }

        private double _currentY;

        public double CurrentY
        {
            get { return _currentY; }
            set 
            { 
                _currentY = value; 
                NotifyOfPropertyChange(() => CurrentY);
            }
        }

        private double _relativeX;

        public double RelativeX
        {
            get { return _relativeX; }
            set 
            {
                _relativeX = value;
                NotifyOfPropertyChange(() => RelativeX);
            }
        }

        private double _relativeY;

        public double RelativeY
        {
            get { return _relativeY; }
            set 
            {
                _relativeY = value;
                NotifyOfPropertyChange(() => RelativeY);
            }
        }



        private PointCollection _boundaryPoints;

        public PointCollection BoundaryPoints
        {
            get { return _boundaryPoints; }
            set 
            { 
                _boundaryPoints = value; 
                NotifyOfPropertyChange(() => BoundaryPoints);
            }
        }

        public double DivideOffset { get; private set; }

        private bool _isCompleted;

        public bool IsCompleted
        {
            get { return _isCompleted; }
            set 
            { 
                _isCompleted = value; 
                NotifyOfPropertyChange(() => IsCompleted);
            }
        }


        private double _relativeWidth;

        public double RelativeWidth
        {
            get { return _relativeWidth; }
            set 
            { 
                _relativeWidth = value; 
                NotifyOfPropertyChange(() => RelativeWidth);
            }
        }

        private double _relativeHeight;

        public double RelativeHeight
        {
            get { return _relativeHeight; }
            set 
            {
                _relativeHeight = value;
                NotifyOfPropertyChange(() => RelativeHeight);
            }
        }




        private ObservableCollection<EllipseViewModel> ellipses = new ObservableCollection<EllipseViewModel>();
        public ObservableCollection<EllipseViewModel> Ellipses
        {
            get { return ellipses; }
            set
            {
                ellipses = value;
                NotifyOfPropertyChange(() => Ellipses);
            }
        }

        public PathGeometry PathGeometry { get; private set; }
        public int ValidCount { get; private set; }
        #endregion
        #region - Attributes -
        private LidarService _lidarService;
        private TcpServerService _tcpServerService;
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
