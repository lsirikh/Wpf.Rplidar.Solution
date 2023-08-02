using Caliburn.Micro;
using Newtonsoft.Json;
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
using Wpf.Libaries.ServerService.Services;
using Wpf.Rplidar.Solution.Controls;
using Wpf.Rplidar.Solution.Helpers;
using Wpf.Rplidar.Solution.Models;
using Wpf.Rplidar.Solution.Models.Messages;
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
        , IHandle<SetupMessageRefresh>
    {
        #region - Ctors -
        public VisualViewModel(IEventAggregator eventAggregator
                                , LidarService lidarService
                                , TcpServerService tcpServerService
                                , ConnectedComponentFinder connectedComponentFinder
                                , SetupModel setupModel)
                                 : base(eventAggregator)
        {
            _lidarService = lidarService;
            _tcpServerService = tcpServerService;
            _setupModel = setupModel;
            _connectedComponentFinder = connectedComponentFinder;
            locker = new object();
        }
        #endregion
        #region - Implementation of Interface -
        #endregion
        #region - Overrides -
        protected override Task OnActivateAsync(CancellationToken cancellationToken)
        {
            Scale = 100;


            _lidarService.TransferPoints += _lidarService_TransferPoints;
            //_lidarService.TransferGroup += _lidarService_TransferGroup;
            Points = new List<Point>();

            BoundaryPoints = CreateBoundary(_setupModel.BoundaryPoints);
            return base.OnActivateAsync(cancellationToken);
        }



        private PointCollection CreateBoundary(PointCollection boundaryPoints)
        {
            try
            {
                var pCollection = new PointCollection(boundaryPoints);
                (RelativeWidth, RelativeHeight) = MathHelper.CalculateWidthAndHeight(boundaryPoints.ToList<Point>(), DivideOffset);
                CreatePathGeometry(boundaryPoints.ToList<Point>());

                var point = pCollection.FirstOrDefault();
                pCollection.Add(point);
                IsCompleted = true;

                return pCollection;
            }
            catch
            {
                return null;
            }

        }

        protected override void OnViewAttached(object view, object context)
        {
            ZoomAndPanControl = (view as VisualView).ZoomAndPanControl;
            Canvas = (view as VisualView).canvas;
            base.OnViewAttached(view, context);
        }

        protected override Task OnDeactivateAsync(bool close, CancellationToken cancellationToken)
        {
            Canvas.Dispose();
            _lidarService.TransferPoints -= _lidarService_TransferPoints;
            //_lidarService.TransferGroup -= _lidarService_TransferGroup;
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
                if (!_isBoundarySet) return;
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
                else if (mouseHandlingMode == MouseHandlingMode.AddBoundary)
                {


                    if (IsCompleted)
                    {
                        ClickToClearBoundary();
                    }

                    var ePoint = new EllipseViewModel();
                    ePoint.X = origContentMouseDownPoint.X - ePoint.EllipseWidth / 2;
                    ePoint.Y = origContentMouseDownPoint.Y - ePoint.EllipseHeight / 2;
                    Ellipses.Add(ePoint);

                    BoundaryPoints.Add(origContentMouseDownPoint);

                    if (BoundaryPoints.Count > 3)
                    {
                        BoundaryPoints = CreateBoundary(BoundaryPoints);
                        Ellipses.Clear();

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
            if (ZoomAndPanControl.ContentScale < 0.1)
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


        public void ClickToSetBoundary()
        {
            _isBoundarySet = true;
        }

        public void ClickToClearBoundary()
        {
            (RelativeWidth, RelativeHeight) = (0.0d, 0.0d);
            BoundaryPoints.Clear();
            BoundaryPoints = new PointCollection();
            PathGeometry?.Clear();
            IsCompleted = false;
        }

        public void ClickToSetHighPosition()
        {
            SensorLocation = true;
        }

        public void ClickToSetLowPosition()
        {
            SensorLocation = false;
        }
        #endregion
        #region - Processes -
        private void _lidarService_TransferPoints(object sender, EventArgs e)
        {
            if (!(e is PointListArgs args)) return;

            try
            {
                ValidCount = 0;
                if (Points.Count > 0) return;


                //foreach (Measure measure in args.Measures)
                //{
                //    //중점을 기준으로 (x,y) 수평이동
                //    var x = (measure.X / 10) + (Width / 2);
                //    var y = Height / 2 - (measure.Y / 10);

                //    var point = MathHelper.RotatePointAroundPivot(new Point(x, y), new Point(Width / 2, Height / 2), OffsetAngle);

                //    Points.Add(point);
                //    _ = CheckPointTask(point);
                //}

                foreach (var point in args.Points)
                {
                    Points.Add(point);
                    
                }

                _ = CheckPointTask(args.Points);


            }
            catch (Exception)
            {
                throw;
            }

        }

        private Task CheckPointTask(List<Point> pList)
        {
            lock (locker)
            {
                return Task.Run(() =>
                {
                    try
                    {

                        Application.Current?.Dispatcher?.Invoke(() =>
                        {


                            //var testPoint = new Point(point.X / DivideOffset, point.Y / DivideOffset);
                            points.Clear();
                            foreach (var point in pList)
                            {
                                if (PathGeometry != null && PathGeometry.FillContains(point))
                                {
                                    // Transfer Service
                                    var originPoint = BoundaryPoints.FirstOrDefault();  // 첫 번째 경계 점
                                    Point newPoint = new Point(((point.X - originPoint.X) / DivideOffset), ((point.Y - originPoint.Y) / DivideOffset));

                                    points.Add(newPoint);  // 점을 목록에 추가


                                }


                            }
                            // 연결된 구성 요소를 찾음
                            var connectedComponents = _connectedComponentFinder.GetConnectedComponents(points, 5);  // 임계값은 20
                            int count = 1;
                            foreach (var connectedComponent in connectedComponents)
                            {
                                // 각 연결된 구성 요소를 처리합니다. 필요한 코드를 여기에 추가하세요.
                                Debug.WriteLine($"{count++}/{connectedComponents.Count} : ({connectedComponent.Average(entity => entity.X)}, {connectedComponent.Average(entity => entity.Y)}({connectedComponent.Count})");
                                
                                //_tcpServerService.SendRequest(JsonConvert.SerializeObject(model));
                            }

                        });
                        //Task.Run(() =>
                        //{
                        //    var connectedComponents = _connectedComponentFinder.GetConnectedComponents(points, 20);  // 임계값은 20
                        //    int count = 0;
                        //    foreach (var connectedComponent in connectedComponents)
                        //    {
                        //        // 각 연결된 구성 요소를 처리합니다. 필요한 코드를 여기에 추가하세요.
                        //        Debug.WriteLine($"{count++}/{connectedComponents.Count} : {connectedComponent.Count()}");
                        //    }
                        //});
                    }
                    catch (Exception ex)
                    {

                        Debug.WriteLine($"Raised Exception in {nameof(CheckPointTask)} : {ex.Message}");
                    }
                });
            }
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


        

       

        List<Point> points = new List<Point>();  // 점들의 목록

        

       

        public void CreatePathGeometry(List<Point> points)
        {
            try
            {
                PathGeometry = new PathGeometry();
                if (points == null || points.Count != 4)
                {
                    throw new ArgumentException("Exactly 4 points are required", nameof(points));
                }


                PathFigure pathFigure = new PathFigure();
                pathFigure.StartPoint = points[0];
                pathFigure.Segments.Add(new LineSegment(points[1], true));
                pathFigure.Segments.Add(new LineSegment(points[2], true));
                pathFigure.Segments.Add(new LineSegment(points[3], true));
                pathFigure.IsClosed = true;
                PathGeometry.Figures.Add(pathFigure);
            }
            catch (ArgumentException ex)
            {
                Debug.WriteLine($"Raised Exception in {nameof(CreatePathGeometry)} :" + ex.Message);
            }

        }

        #endregion
        #region - IHanldes -
        public Task HandleAsync(SetupMessageRefresh message, CancellationToken cancellationToken)
        {
            BoundaryPoints = CreateBoundary(_setupModel.BoundaryPoints);
            NotifyOfPropertyChange(() => BoundaryPoints);
            Refresh();
            return Task.CompletedTask;
        }
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

        public double Width
        {
            get { return _setupModel.Width; }
            set
            {
                _setupModel.Width = value;
                NotifyOfPropertyChange(() => Width);
            }
        }

        public double Height
        {
            get { return _setupModel.Height; }
            set
            {
                _setupModel.Height = value;
                NotifyOfPropertyChange(() => Height);
            }
        }

        public double XOffset
        {
            get { return _setupModel.XOffset; }
            set
            {
                _setupModel.XOffset = value;
                NotifyOfPropertyChange(() => XOffset);
            }
        }

        public double YOffset
        {
            get { return _setupModel.YOffset; }
            set
            {
                _setupModel.YOffset = value;
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

        public double OffsetAngle
        {
            get { return _setupModel.OffsetAngle; }
            set
            {
                _setupModel.OffsetAngle = value;
                NotifyOfPropertyChange(() => OffsetAngle);
            }
        }


        public ZoomAndPanControl ZoomAndPanControl { get; internal set; }
        public DrawingCanvas Canvas { get; internal set; }

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


        public PointCollection BoundaryPoints
        {
            get { return _setupModel.BoundaryPoints; }
            set
            {
                _setupModel.BoundaryPoints = value;
                NotifyOfPropertyChange(() => BoundaryPoints);
            }
        }

        public double DivideOffset => _setupModel.DivideOffset;

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

        public bool SensorLocation
        {
            get { return _setupModel.SensorLocation; }
            set
            {
                _setupModel.SensorLocation = value;
                NotifyOfPropertyChange(() => SensorLocation);
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

        public Thread ReceiveThread { get; private set; }
        public Thread ProcessingThread { get; private set; }
        #endregion
        #region - Attributes -
        private LidarService _lidarService;
        private TcpServerService _tcpServerService;
        private SetupModel _setupModel;
        private ConnectedComponentFinder _connectedComponentFinder;
        private StreamGeometry geometry;
        private object locker;

        private double _contentScale = 1.0;
        private MouseHandlingMode mouseHandlingMode;
        private MouseButton mouseButtonDown;
        private Point origZoomAndPanControlMouseDownPoint;
        private Point origContentMouseDownPoint;
        private bool _isBoundarySet;
        #endregion
    }
}
