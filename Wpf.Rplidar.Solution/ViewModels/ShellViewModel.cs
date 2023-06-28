using Caliburn.Micro;
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
            _lidarService.Message += _lidarService_Message;
            _lidarService.InitSerial();
            VisualViewModel.ActivateAsync();
            return base.OnActivateAsync(cancellationToken);
        }
        

        protected override Task OnDeactivateAsync(bool close, CancellationToken cancellationToken)
        {
            _eventAggregator.Unsubscribe(this);
            _lidarService.UninitSerial();
            VisualViewModel.DeactivateAsync(true);
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
        public VisualViewModel VisualViewModel { get; }
        
        #endregion
        #region - Attributes -
        private IEventAggregator _eventAggregator;
        private LidarService _lidarService;
        #endregion
    }
}
