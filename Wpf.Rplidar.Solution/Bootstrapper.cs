using Autofac;
using Caliburn.Micro;
using IniParser;
using IniParser.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Wpf.Libaries.ServerService.Models;
using Wpf.Libaries.ServerService.Services;
using Wpf.Rplidar.Solution.Base;
using Wpf.Rplidar.Solution.Models;
using Wpf.Rplidar.Solution.Modules;
using Wpf.Rplidar.Solution.Services;
using Wpf.Rplidar.Solution.Utils;
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
            var fileService = IoC.Get<FileService>();
            var setupModel = IoC.Get<SetupModel>();
            fileService.Init();
            fileService.CreateSetupModel(setupModel);
            var logService = IoC.Get<LogService>();
            logService.Init();

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
                builder.RegisterType<FileService>().SingleInstance();
                builder.RegisterType<ConnectedComponentFinder>().SingleInstance();

                var setupModel = new SetupModel();
                builder.RegisterInstance(setupModel).SingleInstance();

                builder.RegisterType<TcpServerService>().SingleInstance();

                // log4net 모듈 로드
                builder.RegisterModule(new Log4NetModule());

                builder.RegisterType<LogService>().SingleInstance();

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
