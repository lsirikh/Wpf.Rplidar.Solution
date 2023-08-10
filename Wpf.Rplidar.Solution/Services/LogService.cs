using log4net;
using System.Diagnostics;

namespace Wpf.Rplidar.Solution.Services
{
    /****************************************************************************
        Purpose      :                                                           
        Created By   : GHLee                                                
        Created On   : 8/10/2023 9:31:43 AM                                                    
        Department   : SW Team                                                   
        Company      : Sensorway Co., Ltd.                                       
        Email        : lsirikh@naver.com                                         
     ****************************************************************************/

    public class LogService
    {

        #region - Ctors -
        public LogService(ILog log)
        {
            _log = log;
        }
        #endregion
        #region - Implementation of Interface -
        #endregion
        #region - Overrides -
        #endregion
        #region - Binding Methods -
        #endregion
        #region - Processes -
        public void Init()
        {
            var msg = $"LogService 초기화";
            
            // 로그 작성
            _log.Info(msg);
            Debug.WriteLine(msg);
        }

        public void Info(string msg)
        {
            // 로그 작성
            _log.Info(msg);
            Debug.WriteLine(msg);
        }

        public void Warning(string msg)
        {
            // 로그 작성
            _log.Warn(msg);
            Debug.WriteLine(msg);
        }

        public void Error(string msg)
        {
            // 로그 작성
            _log.Error(msg);
            Debug.WriteLine(msg);
        }
        #endregion
        #region - IHanldes -
        #endregion
        #region - Properties -
        #endregion
        #region - Attributes -
        private readonly ILog _log;
        #endregion
    }
}
