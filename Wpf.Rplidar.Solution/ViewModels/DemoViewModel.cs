using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Wpf.Rplidar.Solution.Models;

namespace Wpf.Rplidar.Solution.ViewModels
{
    public class DemoViewModel : Conductor<IScreen>
    {
        #region - Ctors -
        public DemoViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
        }
        #endregion
        #region - Implementation of Interface -
        #endregion
        #region - Overrides -
        protected override Task OnActivateAsync(CancellationToken cancellationToken)
        {
            Model = new DemoModel() { Id = 1, Name = "Sensorway", Phone = "02-957-6500", Address = "경기도 고양시 덕양구 통일로" };
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
        #endregion
        #region - IHanldes -
        #endregion
        #region - Properties -

        public DemoModel Model
        {
            get { return _model; }
            set
            {
                _model = value;
                NotifyOfPropertyChange(() => Model);
            }
        }

        #endregion
        #region - Attributes -
        private DemoModel _model;
        private IEventAggregator _eventAggregator;
        #endregion
    }
}
