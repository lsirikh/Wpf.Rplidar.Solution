﻿using Caliburn.Micro;
using RPLidarA1;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Wpf.Rplidar.Solution.Models;
using Wpf.Rplidar.Solution.Services;
using Wpf.Rplidar.Solution.Utils;
using static System.Net.Mime.MediaTypeNames;

namespace Wpf.Rplidar.Solution.ViewModels
{
    public class ShellViewModel : Conductor<IScreen>
    {
        #region - Ctors -
        public ShellViewModel(IEventAggregator eventAggregator
                                , VisualViewModel visualViewModel
                                , LidarService lidarService
                                , TcpServerService tcpServerService)
        {
            _eventAggregator = eventAggregator;
            _lidarService = lidarService;
            _tcpServerService = tcpServerService;
            VisualViewModel = visualViewModel;
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

            FindComport();
            
            _tcpServerService.Message += _tcpServerService_Message;
            _lidarService.Message += _lidarService_Message;
            VisualViewModel.ActivateAsync();

            _ = Task.Run(async () =>
            {

                await Task.Delay(5000);
                IpAddress = IPAddress.Any.ToString();
                Port = 15000;
                NotifyOfPropertyChange(() => IpAddress);
                _tcpServerService.TcpInitialize(IpAddress, Port);
            });
            return base.OnActivateAsync(cancellationToken);
        }

        private void FindComport()
        {
            string[] ports = SerialPort.GetPortNames();
            SerialPorts = new List<string>();
            foreach (var item in ports)
            {
                SerialPorts.Add(item);
            }

            NotifyOfPropertyChange(() => SerialPorts);
        }

        protected override Task OnDeactivateAsync(bool close, CancellationToken cancellationToken)
        {
            ClickToDisconnect();
            _eventAggregator.Unsubscribe(this);
            VisualViewModel.DeactivateAsync(true);

            
            Thread.Sleep(500);
            return base.OnDeactivateAsync(close, cancellationToken);
        }
        #endregion
        #region - Binding Methods -
        #endregion
        #region - Processes -
        private void _lidarService_Message(string msg, string status)
        {
            AddLogMessage(msg, status);
        }

        private void _tcpServerService_Message(string msg, string status)
        {
            AddLogMessage(msg, status);
        }

        private void AddLogMessage(string msg, string status)
        {
            System.Windows.Application.Current?.Dispatcher?.Invoke(() =>
            {
                // Your GUI update code here.
                // For example, update a TextBlock's Text property:
                int id = LogProvider?.Count() == null ? 0 : LogProvider.Count() + 1;
                LogProvider.Add(new LogViewModel(new LogModel(id, DateTime.Now, EnumLogType.INFO, msg)));
                Status = status;
            });
        }


        public void ClickToConnect()
        {
            if(SelectedPort != null) 
            {
                
                _lidarService.InitSerial(SelectedPort);
            }
        }

        public void ClickToDisconnect()
        {
            //_lidarService.Message -= _lidarService_Message;
            _lidarService.UninitSerial();
            _tcpServerService.Stop();
        }
        #endregion
        #region - IHanldes -
        #endregion
        #region - Properties -
        public ObservableCollection<LogViewModel> LogProvider { get; set; }
        public List<string> SerialPorts { get; set; }
        public VisualViewModel VisualViewModel { get; }

        public string SelectedPort
        {
            get { return _selectedPort; }
            set 
            { 
                _selectedPort = value; 
                NotifyOfPropertyChange(() => SelectedPort);
            }
        }

        private string _status;

        public string Status
        {
            get { return _status; }
            set 
            { 
                _status = value;
                NotifyOfPropertyChange(() => Status);
            }
        }

        private int _port;

        public int Port
        {
            get { return _port; }
            set 
            { 
                _port = value;
                NotifyOfPropertyChange(() => Port);
            }
        }


        public string IpAddress { get; set; }

        #endregion
        #region - Attributes -
        private IEventAggregator _eventAggregator;
        private string _selectedPort;
        private LidarService _lidarService;
        private TcpServerService _tcpServerService;
        #endregion
    }
}
