using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Wpf.Libaries.ServerService.Models;
using Wpf.Libaries.ServerService.Utils;

namespace Wpf.Libaries.ServerService.Base
{
    /****************************************************************************
        Purpose      :                                                           
        Created By   : GHLee                                                
        Created On   : 7/6/2023 11:20:17 AM                                                    
        Department   : SW Team                                                   
        Company      : Sensorway Co., Ltd.                                       
        Email        : lsirikh@naver.com                                         
     ****************************************************************************/

    public abstract class TcpSocket : TcpTaskTimer, ITcpSocket
    {

        #region - Ctors -
        public TcpSocket()
        {

        }
        #endregion
        #region - Implementation of Interface -
        #endregion
        #region - Overrides -
        public abstract void InitSocket(ITcpServerModel model = default);
        public abstract Task SendRequest(string msg, IPEndPoint selectedIp = null);
        public abstract void CloseSocket();
        #endregion
        #region - Binding Methods -
        #endregion
        #region - Processes -
        #endregion
        #region - IHanldes -
        #endregion
        #region - Properties -
        public Socket Socket { get; protected set; }
        public EnumTcpMode Mode { get; protected set; }
        #endregion
        #region - Attributes -
        protected StringBuilder sb;

        protected const int PACKET_BYTE = 1024 * 8;
        #endregion
    }
}
