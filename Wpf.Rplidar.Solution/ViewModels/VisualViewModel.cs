using Caliburn.Micro;
using RPLidarA1;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
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
            return Task.Run(() =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Points = measures;

                    ////Points = measures;
                    //foreach (var item in measures)
                    //{
                    //    Points.Add(item);
                    //}

                    NotifyOfPropertyChange(() => Points);
                });
                //await Task.Delay(10);

            });

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
        #endregion
        #region - IHanldes -
        #endregion
        #region - Properties -
        public List<Measure> Points { get; set; } 
        #endregion
        #region - Attributes -
        private LidarService _lidarService;
        private object locker;
        #endregion
    }
}
