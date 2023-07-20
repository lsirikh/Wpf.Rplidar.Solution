using System;
using System.Net;

namespace Wpf.Libaries.ServerService.Utils
{
    /****************************************************************************
        Purpose      :                                                           
        Created By   : GHLee                                                
        Created On   : 7/6/2023 1:38:59 PM                                                    
        Department   : SW Team                                                   
        Company      : Sensorway Co., Ltd.                                       
        Email        : lsirikh@naver.com                                         
     ****************************************************************************/

    public class TcpEventArgs :EventArgs
    {

        #region - Ctors -
        public TcpEventArgs(IPEndPoint endPoint = default)
        {
            EndPoint = endPoint;
        }

        #endregion
        #region - Implementation of Interface -
        #endregion
        #region - Overrides -
        #endregion
        #region - Binding Methods -
        #endregion
        #region - Processes -
        #endregion
        #region - IHanldes -
        #endregion
        #region - Properties -
        public IPEndPoint EndPoint { get; }
        #endregion
        #region - Attributes -
        #endregion
    }

    public class TcpReceiveEventArgs : TcpEventArgs
    {

        #region - Ctors -
        public TcpReceiveEventArgs(string msg = null, IPEndPoint endPoint = default) : base(endPoint)
        {
            Message = msg;
        }

        #endregion
        #region - Implementation of Interface -
        #endregion
        #region - Overrides -
        #endregion
        #region - Binding Methods -
        #endregion
        #region - Processes -
        #endregion
        #region - IHanldes -
        #endregion
        #region - Properties -
        public string Message { get; }
        #endregion
        #region - Attributes -
        #endregion
    }
}
