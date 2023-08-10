using Autofac;
using log4net;

namespace Wpf.Rplidar.Solution.Modules
{
    /****************************************************************************
        Purpose      :                                                           
        Created By   : GHLee                                                
        Created On   : 8/10/2023 9:28:56 AM                                                    
        Department   : SW Team                                                   
        Company      : Sensorway Co., Ltd.                                       
        Email        : lsirikh@naver.com                                         
     ****************************************************************************/

    public class Log4NetModule : Module
    {

        #region - Ctors -

        #endregion
        #region - Implementation of Interface -
        #endregion
        #region - Overrides -
        protected override void Load(ContainerBuilder builder)
        {
            // log4net 등록
            builder.Register(c => LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType))
                   .As<ILog>()
                   .SingleInstance(); // 싱글턴 스코프로 등록
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
