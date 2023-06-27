﻿using Caliburn.Micro;
using RPLidarA1;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Wpf.Rplidar.Solution.Models;
using Wpf.Rplidar.Solution.Services;
using Wpf.Rplidar.Solution.Utils;

namespace Wpf.Rplidar.Solution.ViewModels
{
    public class ShellViewModel : Conductor<IScreen>
    {
        #region - Ctors -
        public ShellViewModel(IEventAggregator eventAggregator
                                , LidarService lidarService)
        {
            _eventAggregator = eventAggregator;
            _lidarService = lidarService;
        }
        #endregion
        #region - Implementation of Interface -
        #endregion
        #region - Overrides -
        protected override Task OnActivateAsync(CancellationToken cancellationToken)
        {
            LogProvider = new ObservableCollection<LogViewModel>();
            NotifyOfPropertyChange(()=> LogProvider);
            
            _eventAggregator.SubscribeOnUIThread(this);
            
            _lidarService.Message += _lidarService_Message;
            _lidarService.SendPoints += _lidarService_SendPoints;
            _lidarService.InitSerial();


            return base.OnActivateAsync(cancellationToken);
        }
        

        protected override Task OnDeactivateAsync(bool close, CancellationToken cancellationToken)
        {
            _eventAggregator.Unsubscribe(this);
            _lidarService.UninitSerial();
            return base.OnDeactivateAsync(close, cancellationToken);
        }
        #endregion
        #region - Binding Methods -
        #endregion
        #region - Processes -
        private void _lidarService_Message(string msg)
        {
            int id = LogProvider?.Count() == null ? 0 : LogProvider.Count() + 1;
            LogProvider.Add(new LogViewModel(new LogModel(id, DateTime.Now, EnumLogType.INFO, msg)));
        }
        #endregion
        #region - IHanldes -
        #endregion
        #region - Properties -
        public ObservableCollection<LogViewModel> LogProvider { get; set; }

        private Task _lidarService_SendPoints(List<Measure> measures)
        {
            Points = measures;

            ////Points = measures;
            //foreach (var item in measures)
            //{
            //    Points.Add(item);
            //}

            NotifyOfPropertyChange(() => Points);
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
        public List<Measure> Points { get; set; }
        #endregion
        #region - Attributes -
        private IEventAggregator _eventAggregator;
        private LidarService _lidarService;

        #endregion
    }
}