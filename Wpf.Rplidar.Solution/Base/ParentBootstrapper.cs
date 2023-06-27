using Autofac;
using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Wpf.Rplidar.Solution.Base
{
    public class ParentBootstrapper<T> : BootstrapperBase
    {

        #region - Ctors -
        #endregion
        #region - Implementation of Interface -
        #endregion
        #region - Overrides -

        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            base.OnStartup(sender, e);
        }

        protected override void OnExit(object sender, EventArgs e)
        {
            base.OnExit(sender, e);
        }

        protected override void Configure()
        {
            var builder = new ContainerBuilder();



            RegisterBaseType(builder);
            builder.RegisterType<T>().SingleInstance();

            ConfigureContainer(builder);

            _container = builder.Build();
        }


        private void RegisterBaseType(ContainerBuilder builder)
        {
            builder.RegisterType<WindowManager>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<EventAggregator>().AsImplementedInterfaces().SingleInstance();

        }

        protected override object GetInstance(Type service, string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                if (Container.IsRegistered(service))
                    return Container.Resolve(service);
            }
            else
            {
                if (Container.IsRegisteredWithKey(key, service))
                    return Container.ResolveKeyed(key, service);
            }
            throw new Exception(string.Format("Could not locate any instances of contract {0}.", key ?? service.Name));

        }

        protected override IEnumerable<object> GetAllInstances(Type service)
        {
            return Container.Resolve(typeof(IEnumerable<>).MakeGenericType(service)) as IEnumerable<object>;
        }

        protected override void BuildUp(object instance)
        {
            Container.InjectProperties(instance);
        }

        protected virtual void ConfigureContainer(ContainerBuilder builder)
        {
            /// Reigster classes
        }
        #endregion
        #region - Binding Methods -
        #endregion
        #region - Processes -
        #endregion
        #region - IHanldes -
        #endregion
        #region - Properties -
        protected IContainer Container
        {
            get { return _container; }
        }
        #endregion
        #region - Attributes -
        private IContainer _container;
        #endregion
    }
}
