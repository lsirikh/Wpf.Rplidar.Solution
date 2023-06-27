using Caliburn.Micro;
using System.Threading;
using System.Threading.Tasks;

namespace Wpf.Rplidar.Solution.ViewModels
{
    public abstract class BaseViewModel : Screen
    {

        public BaseViewModel()
        {
            _eventAggregator = IoC.Get<IEventAggregator>();
        }

        public BaseViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
        }

        protected override Task OnActivateAsync(CancellationToken cancellationToken)
        {
            _eventAggregator.SubscribeOnUIThread(this);
            return base.OnActivateAsync(cancellationToken);
        }

        protected override Task OnDeactivateAsync(bool close, CancellationToken cancellationToken)
        {
            _eventAggregator.Unsubscribe(this);
            return base.OnDeactivateAsync(close, cancellationToken);
        }

        private IEventAggregator _eventAggregator;
    }
}