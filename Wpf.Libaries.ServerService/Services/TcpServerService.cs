using System.Diagnostics;
using System.Timers;
using Wpf.Libaries.ServerService.Base;
using Wpf.Libaries.ServerService.Models;

namespace Wpf.Libaries.ServerService.Services
{
    /****************************************************************************
        Purpose      :                                                           
        Created By   : GHLee                                                
        Created On   : 7/6/2023 11:19:29 AM                                                    
        Department   : SW Team                                                   
        Company      : Sensorway Co., Ltd.                                       
        Email        : lsirikh@naver.com                                         
     ****************************************************************************/

    public class TcpServerService : TcpServer
    {

        #region - Ctors -
        public TcpServerService()
        {

        }
        #endregion
        #region - Implementation of Interface -
        #endregion
        #region - Overrides -
        protected override void ConnectionTick(object sender, ElapsedEventArgs e)
        {
            Debug.WriteLine($"Tick...");
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
