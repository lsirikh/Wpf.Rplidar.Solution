using System.Diagnostics;
using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Net.Sockets;
using Wpf.Libaries.ServerService.Utils;
using Wpf.Libaries.ServerService.Models;

namespace Wpf.Libaries.ServerService.Base
{
    /****************************************************************************
        Purpose      :                                                           
        Created By   : GHLee                                                
        Created On   : 7/6/2023 11:35:21 AM                                                    
        Department   : SW Team                                                   
        Company      : Sensorway Co., Ltd.                                       
        Email        : lsirikh@naver.com                                         
     ****************************************************************************/

    public class TcpAcceptedClient : TcpSocket
    {

        #region - Ctors -
        #endregion
        #region - Implementation of Interface -
        #endregion
        #region - Overrides -
        public override void InitSocket(ITcpServerModel model = default)
        {
            try
            {
                sb = new StringBuilder();
                //Timer 초기화
                InitTimer();

                
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Raised Exception in {nameof(InitSocket)} : {ex.Message}");
            }
        }

        public void CreateSocket(Socket socketClient)
        {
            try
            {
                //전달받은 소켓 전역으로 활용
                Socket = socketClient;

                Mode = Utils.EnumTcpMode.INACTIVE;
                //HeartBeatExpireTime = DateTime.Now + TimeSpan.FromSeconds(20);
                SetTimerInterval(1000);
                SetTimerStart();

                Socket.LingerState = new LingerOption(true, 1);
                Socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

                // 서버에 보낼 객체를 만든다.
                var _receiveEvent = new SocketAsyncEventArgs();
                //보낼 데이터를 설정하고
                _receiveEvent.UserToken = Socket;

                //ID 설정
                ClientAddress = $"{((IPEndPoint)Socket.RemoteEndPoint)?.Address.ToString()}:{((IPEndPoint)Socket.RemoteEndPoint)?.Port.ToString()}";

                //데이터 길이 세팅
                _receiveEvent.SetBuffer(new byte[PACKET_BYTE], 0, PACKET_BYTE);

                Mode = Utils.EnumTcpMode.DISCONNECTED;
                //받음 완료 이벤트 연결
                _receiveEvent.Completed += new EventHandler<SocketAsyncEventArgs>(Recieve_Completed);

                //클라이언트 연결 후, 호출한 서버클래스 내부 작업 요청
                Connected?.Invoke(this, new TcpEventArgs(socketClient.RemoteEndPoint as IPEndPoint));

                Mode = Utils.EnumTcpMode.CONNECTED;

                //받음 보냄
                Socket.ReceiveAsync(_receiveEvent);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Raised Exception in {nameof(CreateSocket)} : {ex.Message}");
            }
        }

        private void Recieve_Completed(object sender, SocketAsyncEventArgs e)
        {
            //서버에서 넘어온 정보
            Socket socketClient = (Socket)sender;

            
            try
            {
                // 접속이 연결되어 있으면...
                if (socketClient.Connected && e.BytesTransferred > 0)
                {

                    var remoteEP = (IPEndPoint)socketClient?.RemoteEndPoint;

                    // 수신 데이터는 e.Buffer에 있다.
                    byte[] data = e.Buffer;


                    // 메모리 버퍼를 초기화 한다.
                    e.SetBuffer(new byte[PACKET_BYTE], 0, PACKET_BYTE);

                    // 데이터를 string으로 변환한다.
                    string msg = Encoding.UTF8.GetString(data);

                    // StringBuilder에 추가한다.
                    sb.Append(msg.Trim('\0').Replace(Environment.NewLine, string.Empty));

                    //수신시 확인
                    Received?.Invoke(this, new TcpReceiveEventArgs(sb.ToString(), remoteEP));

                    // StringBuilder의 내용을 비운다.
                    sb.Clear();

                    // 메시지가 오면 이벤트를 발생시킨다. (IOCP로 넣는 것)
                    socketClient.ReceiveAsync(e);
                }
                else
                {
                    CloseSocket();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Raised Exception in {nameof(Recieve_Completed)} : {ex.Message}");
            }

        }

        public override Task SendRequest(string msg, IPEndPoint selectedIp = null)
        {
            return Task.Run(() =>
            {
                try
                {
                    //3rd Party에게 송신할 객체를 생성
                    SocketAsyncEventArgs sendArgs = new SocketAsyncEventArgs();

                    sendArgs.RemoteEndPoint = selectedIp != null ? selectedIp : null;

                    //보낼 메시지 내용 byte[]로 변환
                    byte[] sendData = Encoding.UTF8.GetBytes(msg);

                    sendArgs.SetBuffer(sendData, 0, sendData.Length);
                    Socket.SendAsync(sendArgs);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Raised Exception in {nameof(SendRequest)} : {ex.Message}");
                }
            });
        }

        protected override void ConnectionTick(object sender, ElapsedEventArgs e)
        {

            //if ((DateTime.Now - HeartBeatExpireTime) > TimeSpan.Zero)
            //{
            //    Debug.WriteLine($"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ff")}]****Heartbeat time was expired!****");
            //    Debug.WriteLine($"heartbeat : {HeartBeatExpireTime}");
            //    CloseSocket();
            //}
        }

        public override void CloseSocket()
        {
            try
            {
                Debug.WriteLine("TcpAcceptedClient CloseSocket was called");

                if (Socket != null && Mode != 0)
                {
                    /*if (_receiveEvent != null)
                        _receiveEvent.Completed -= new EventHandler<SocketAsyncEventArgs>(Recieve_Completed);*/

                    
                    DisposeTimer();
                    Disconnected?.Invoke(this, new TcpEventArgs());

                    if (Socket.Connected)
                    {
                        //Socket AsyncEvent for Disconnection
                        SocketAsyncEventArgs disconnectEvent = new SocketAsyncEventArgs();
                        Socket.DisconnectAsync(disconnectEvent);

                        //When Complete Disconnection from Remote EndPoint Call a callback function
                        disconnectEvent.Completed += new EventHandler<SocketAsyncEventArgs>(Disconnect_Complete);
                    }
                    else
                    {
                        Disconnect_Process();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Raised Exception in {nameof(CloseSocket)} : {ex.Message}");
            }
        }


        #endregion
        #region - Binding Methods -
        #endregion
        #region - Processes -
        public void UpdateHeartBeat(DateTime dateTime)
        {
            HeartBeatExpireTime = dateTime;
            Debug.WriteLine($"<<<<<<<<<<<<UpdateHeartBeat : {HeartBeatExpireTime.ToString("yyyy-MM-dd HH:mm:ss.ff")}>>>>>>>>>>>>>");
        }

        private void Disconnect_Complete(object sender, SocketAsyncEventArgs e)
        {
            try
            {
                if (!((Socket)sender).Connected)
                {
                    Disconnect_Process();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Raised Exception in {nameof(Disconnect_Complete)} : {ex.Message}");
            }
        }

        private void Disconnect_Process()
        {
            Mode = EnumTcpMode.DISCONNECTED;
            Debug.WriteLine($"{nameof(TcpAcceptedClient)} socket({Socket.GetHashCode()}) was disconnected in {nameof(Disconnect_Complete)}");
            //Socket Close to finish using socket
            Mode = EnumTcpMode.INACTIVE;
            Socket?.Close();
            Debug.WriteLine($"{nameof(TcpAcceptedClient)} socket({Socket.GetHashCode()}) was closed in {nameof(Disconnect_Complete)}");
            //Socket Dispose to release resources
            Mode = EnumTcpMode.NONE;
            Socket?.Dispose();
            Debug.WriteLine($"{nameof(TcpAcceptedClient)} socket({Socket.GetHashCode()}) was disposed in {nameof(Disconnect_Complete)}");
        }
        #endregion
        #region - IHanldes -
        #endregion
        #region - Properties -
        public string ClientAddress { get; set; }
        public DateTime HeartBeatExpireTime { get; set; }
        #endregion
        #region - Attributes -
        private IPEndPoint _remoteEP;

        public event EventHandler Connected;
        public event EventHandler Received;
        public event EventHandler Disconnected;
        #endregion
    }
}
