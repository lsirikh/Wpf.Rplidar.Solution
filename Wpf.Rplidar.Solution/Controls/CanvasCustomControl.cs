using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Wpf.Rplidar.Solution.Controls
{
    /// <summary>
    /// XAML 파일에서 이 사용자 지정 컨트롤을 사용하려면 1a 또는 1b단계를 수행한 다음 2단계를 수행하십시오.
    ///
    /// 1a단계) 현재 프로젝트에 있는 XAML 파일에서 이 사용자 지정 컨트롤 사용.
    /// 이 XmlNamespace 특성을 사용할 마크업 파일의 루트 요소에 이 특성을 
    /// 추가합니다.
    ///
    ///     xmlns:MyNamespace="clr-namespace:Wpf.Rplidar.Solution.Controls"
    ///
    ///
    /// 1b단계) 다른 프로젝트에 있는 XAML 파일에서 이 사용자 지정 컨트롤 사용.
    /// 이 XmlNamespace 특성을 사용할 마크업 파일의 루트 요소에 이 특성을 
    /// 추가합니다.
    ///
    ///     xmlns:MyNamespace="clr-namespace:Wpf.Rplidar.Solution.Controls;assembly=Wpf.Rplidar.Solution.Controls"
    ///
    /// 또한 XAML 파일이 있는 프로젝트의 프로젝트 참조를 이 프로젝트에 추가하고
    /// 다시 빌드하여 컴파일 오류를 방지해야 합니다.
    ///
    ///     솔루션 탐색기에서 대상 프로젝트를 마우스 오른쪽 단추로 클릭하고
    ///     [참조 추가]->[프로젝트]를 차례로 클릭한 다음 이 프로젝트를 찾아서 선택합니다.
    ///
    ///
    /// 2단계)
    /// 계속 진행하여 XAML 파일에서 컨트롤을 사용합니다.
    ///
    ///     <MyNamespace:CanvasCustomControl/>
    ///
    /// </summary>
    public class CanvasCustomControl : Control
    {
        static CanvasCustomControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CanvasCustomControl), new FrameworkPropertyMetadata(typeof(CanvasCustomControl)));

            _pen = new Pen(Brushes.Gray, 0);
            _pen.Freeze();
        }

        private static readonly Pen _pen;

        public List<Point> Points
        {
            get { return (List<Point>)GetValue(PointsProperty); }
            set { SetValue(PointsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Points.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PointsProperty =
            DependencyProperty.Register("Points", typeof(List<Point>), typeof(DrawingCanvas), new PropertyMetadata(new List<Point>()));



        public CanvasCustomControl()
        {
            var timer = new System.Timers.Timer(300);
            try
            {
                _ = Task.Run(() =>
                {
                    //var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(150));
                    //while (await timer.WaitForNextTickAsync())
                    //{
                    //    await Dispatcher.InvokeAsync(InvalidateVisual);
                    //}

                    timer.Elapsed += (sender, e) =>
                    {
                        Dispatcher.Invoke(() =>
                        {
                            InvalidateVisual();
                        });
                    };
                    timer.Start();

                });
            }
            catch (Exception)
            {
                timer.Stop();
                timer.Dispose();
            }

        }
        protected override void OnRender(DrawingContext dc)
        {
            try
            {
                //var count = 3800;
                foreach (var item in Points)
                {
                    dc.DrawEllipse(Brushes.Gray, _pen, new Point(item.X, item.Y), 2, 2);
                }
            }
            catch
            {
            }

        }
    }
}
