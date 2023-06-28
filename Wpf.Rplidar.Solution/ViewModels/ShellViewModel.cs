using Caliburn.Micro;
using RPLidarA1;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Linq;
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
                                , LidarService lidarService)
        {
            _eventAggregator = eventAggregator;
            _lidarService = lidarService;
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
            _lidarService.Message += _lidarService_Message;
            VisualViewModel.ActivateAsync();
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

        protected override async Task OnDeactivateAsync(bool close, CancellationToken cancellationToken)
        {
            ClickToDisconnect();
            await Task.Delay(1000);
            _eventAggregator.Unsubscribe(this);
            await VisualViewModel.DeactivateAsync(true);
            await base.OnDeactivateAsync(close, cancellationToken);
        }
        #endregion
        #region - Binding Methods -
        #endregion
        #region - Processes -
        private void _lidarService_Message(string msg)
        {
            System.Windows.Application.Current?.Dispatcher?.Invoke(() =>
            {
                // Your GUI update code here.
                // For example, update a TextBlock's Text property:
                int id = LogProvider?.Count() == null ? 0 : LogProvider.Count() + 1;
                LogProvider.Add(new LogViewModel(new LogModel(id, DateTime.Now, EnumLogType.INFO, msg)));
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


        #endregion
        #region - Attributes -
        private IEventAggregator _eventAggregator;
        private string _selectedPort;
        private LidarService _lidarService;
        #endregion
    }
}
