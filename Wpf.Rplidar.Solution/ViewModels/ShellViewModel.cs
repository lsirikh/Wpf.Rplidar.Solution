using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Wpf.Rplidar.Solution.ViewModels
{
    public class ShellViewModel : Conductor<IScreen>
    {
        #region - Ctors -
        public ShellViewModel(IEventAggregator eventAggregator
                              , DemoViewModel demoViewModel)
        {
            _eventAggregator = eventAggregator;
            DemoViewModel = demoViewModel;
        }
        #endregion
        #region - Implementation of Interface -
        #endregion
        #region - Overrides -
        protected override Task OnActivateAsync(CancellationToken cancellationToken)
        {
            _eventAggregator.SubscribeOnUIThread(this);
            DemoViewModel.ActivateAsync();
            return base.OnActivateAsync(cancellationToken);
        }

        protected override Task OnDeactivateAsync(bool close, CancellationToken cancellationToken)
        {
            _eventAggregator.Unsubscribe(this);
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
        public DemoViewModel DemoViewModel { get; }
        #endregion
        #region - Attributes -
        private IEventAggregator _eventAggregator;

        #endregion
    }
}
