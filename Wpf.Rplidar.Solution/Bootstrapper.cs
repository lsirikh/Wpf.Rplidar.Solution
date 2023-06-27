using Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Wpf.Rplidar.Solution.Base;
using Wpf.Rplidar.Solution.Services;
using Wpf.Rplidar.Solution.ViewModels;

namespace Wpf.Rplidar.Solution
{
    public class Bootstrapper : ParentBootstrapper<ShellViewModel>
    {
        #region - Ctors -
        public Bootstrapper()
        {
            Initialize();
        }
        #endregion
        #region - Implementation of Interface -
        #endregion
        #region - Overrides -
        protected override async void OnStartup(object sender, StartupEventArgs e)
        {
            base.OnStartup(sender, e);

            await DisplayRootViewForAsync<ShellViewModel>();
        }

        protected override void OnExit(object sender, EventArgs e)
        {
            base.OnExit(sender, e);
        }

        protected override void ConfigureContainer(ContainerBuilder builder)
        {
            try
            {
                base.ConfigureContainer(builder);

                builder.RegisterType<VisualViewModel>().SingleInstance();
                builder.RegisterType<LidarService>().SingleInstance();
            }
            catch (Exception)
            {

                throw;
            }
        }
        #endregion
        #region - Binding Methods -
        #endregion
        #region - Processes -
        #endregion
        #region - IHanldes -
        #endregion
        #region - Properties -
        #endregion
        #region - Attributes -
        #endregion
    }
}
