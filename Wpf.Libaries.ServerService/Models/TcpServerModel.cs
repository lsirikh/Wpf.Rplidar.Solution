namespace Wpf.Libaries.ServerService.Models
{
    /****************************************************************************
        Purpose      :                                                           
        Created By   : GHLee                                                
        Created On   : 7/6/2023 2:09:26 PM                                                    
        Department   : SW Team                                                   
        Company      : Sensorway Co., Ltd.                                       
        Email        : lsirikh@naver.com                                         
     ****************************************************************************/

    public class TcpServerModel : ITcpServerModel
    {

        #region - Ctors -
        public TcpServerModel()
        {

        }

        public TcpServerModel(string ipAddress, int port, int heartBeat)
        {
            IpAddress = ipAddress;
            Port = port;
            HeartBeat = heartBeat;
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
        public string IpAddress { get; set; }
        public int Port { get; set; }
        public int HeartBeat { get; set; }
        #endregion
        #region - Attributes -
        #endregion
    }
}
