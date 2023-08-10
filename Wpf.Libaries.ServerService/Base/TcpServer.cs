using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Text;
using System;
using Wpf.Libaries.ServerService.Utils;
using System.Reflection;
using Wpf.Libaries.ServerService.Models;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Wpf.Libaries.ServerService.Base
{
    /****************************************************************************
        Purpose      :                                                           
        Created By   : GHLee                                                
        Created On   : 7/6/2023 11:29:10 AM                                                    
        Department   : SW Team                                                   
        Company      : Sensorway Co., Ltd.                                       
        Email        : lsirikh@naver.com                                         
     ****************************************************************************/

    public abstract class TcpServer : TcpSocket
    {

        #region - Ctors -
        public TcpServer()
        {
            _locker = new object();
        }
        #endregion
        #region - Implementation of Interface -
        #endregion
        #region - Overrides -
        public override void InitSocket(ITcpServerModel model)
        {
            try
            {
                _model = model;
                sb = new StringBuilder();
                ClientCount = 0;
                ClientList = new List<TcpAcceptedClient>();
                IPEndPoint ipep = new IPEndPoint(IPAddress.Parse(_model.IpAddress), Convert.ToInt32(_model.Port));

                //Timer 초기화
                InitTimer();


                //Mode Prepared
                Mode = EnumTcpMode.CLOSED;

                CreateSocket(ipep);
                

            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Raised Exception in {nameof(InitSocket)} : {ex.Message}");
            }
        }

        public override Task SendRequest(string msg, IPEndPoint endPoint = null)
        {

            lock (_locker)
            {
                Task.Run(async () =>
                {
                    try
                    {
                        //연결된 클라이언트에게 메시지 브로드캐스팅
                        foreach (TcpAcceptedClient client in ClientList)
                        {
                            if (endPoint == null)
                            {
                                await client.SendRequest(msg);
                            }
                            else
                            {
                                if (client.Socket.RemoteEndPoint as IPEndPoint == endPoint)
                                    await client.SendRequest(msg, endPoint);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Raised Exception in {nameof(SendRequest)} : {ex.Message}");
                    }
                });
            }
            return Task.CompletedTask;

        }
        public override void CloseSocket()
        {
            try
            {
                foreach (var client in ClientList.ToList())
                {
                    if (client.Socket != null && client.Socket.Connected)
                    {
                        client.SendRequest($"Server was finished!", (IPEndPoint)client.Socket.RemoteEndPoint);
                        client.CloseSocket();

                        ClientDeactivate(client);
                    }
                }

                if (_hearingEvent != null)
                    _hearingEvent.Completed -= new EventHandler<SocketAsyncEventArgs>(Accept_Completed);

                if (Socket.Connected)
                {
                    Socket.Disconnect(false);
                    Debug.WriteLine($"Server socket({Socket.GetHashCode()}) was disconnected in {nameof(CloseSocket)}");
                }
                ///Timer Task Dispose
                DisposeTimer();

                ///Socket Closed
                Socket.Close();
                Debug.WriteLine($"{nameof(TcpServer)} socket({Socket.GetHashCode()}) was closed in {nameof(CloseSocket)}");

                //Mode Created
                Mode = EnumTcpMode.CLOSED;
                Debug.WriteLine($"{nameof(TcpServer)} socket({Socket.GetHashCode()}) was disposed in {nameof(CloseSocket)}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Raised Exception in {nameof(CloseSocket)} : {ex.Message}");
            }
        }
        protected void Accept_Completed(object sender, SocketAsyncEventArgs e)
        {
            try
            {
                TcpAcceptedClient cliUser = new TcpAcceptedClient();

                ClientActivate(cliUser);

                cliUser.CreateSocket(e.AcceptSocket);

                //클라이언트 리스트 등록
                ClientList.Add(cliUser);

                //Connected?.Invoke(this, new TcpEventArgs(e.AcceptSocket.RemoteEndPoint as IPEndPoint));

                Socket socketServer = (Socket)sender;
                e.AcceptSocket = null;
                socketServer.AcceptAsync(e);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Raised Exception in {nameof(Accept_Completed)} : {ex.Message}");
            }
        }

        private void Client_Connected(object sender, EventArgs e)
        {
            try
            {
                TcpAcceptedClient client = (TcpAcceptedClient)sender;
                Connected?.Invoke(this, new TcpEventArgs((IPEndPoint)client.Socket.RemoteEndPoint));
                
                Mode = EnumTcpMode.CONNECTED;
                //For Test
                //await client.SendRequest("Welcome!");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Raised Exception in {nameof(Client_Connected)} : {ex.Message}");
            }
        }

        private void Client_Received(object sender, EventArgs e)
        {
            var eventArgs = e as TcpReceiveEventArgs;
            Received?.Invoke(this, new TcpReceiveEventArgs(eventArgs.Message, eventArgs.EndPoint));
        }

        private void Client_Disconnected(object sender, EventArgs e)
        {
            try
            {
                var removeCli = (sender as TcpAcceptedClient);
                var endPoint = (sender as TcpAcceptedClient)?.Socket?.RemoteEndPoint;
                Debug.WriteLine($"EndPoint is {endPoint} in AcceptedClient_Disconnected of TcpServer");

                // 서버 연결 종료 이벤트 송신
                Disconnected?.Invoke(this, new TcpEventArgs(endPoint as IPEndPoint));
                // 클라이언트 리스트에서 해당 소켓 삭제
                ClientList.Remove(removeCli);

                ClientDeactivate(removeCli);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Rasied Exception in {nameof(Client_Disconnected)} : {ex.Message}");
            }
        }

        #endregion
        #region - Binding Methods -
        #endregion
        #region - Processes -
        private void CreateSocket(IPEndPoint ipep)
        {
            try
            {
                //소켓 생성
                Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                Socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                //ipep = new IPEndPoint(IPAddress.Parse(SetupModel.ServerIp), Convert.ToInt32(SetupModel.ServerPort));
                Socket.Bind(ipep);
                Socket.Listen(5);

                //연결요청 확인 이벤트
                _hearingEvent = new SocketAsyncEventArgs();

                //이벤트 RemoteEndPoint 설정
                _hearingEvent.RemoteEndPoint = ipep;

                //연결 완료 이벤트 연결
                _hearingEvent.Completed += new EventHandler<SocketAsyncEventArgs>(Accept_Completed);

                //TaskTimer Start!
                SetTimerStart();

                //Mode Created
                Mode = EnumTcpMode.CREATED;

                //서버 메시지 대기
                Socket.AcceptAsync(_hearingEvent);

               
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{ex.Message}");
            }
        }

        public void ClientActivate(TcpAcceptedClient client)
        {
            try
            {
                client.UpdateHeartBeat(DateTime.Now + TimeSpan.FromSeconds(_model.HeartBeat));
                client.InitSocket();
                //이벤트 연결
                client.Connected += Client_Connected;
                client.Received += Client_Received; ;
                client.Disconnected += Client_Disconnected; ;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public void ClientDeactivate(TcpAcceptedClient client)
        {
            try
            {
                client.Connected -= Client_Connected;
                client.Received -= Client_Received; ;
                client.Disconnected -= Client_Disconnected; ;
            }
            catch (Exception)
            {

                throw;
            }
        }
        #endregion
        #region - IHanldes -
        #endregion
        #region - Properties -
        public int ClientCount { get; set; }
        public List<TcpAcceptedClient> ClientList { get; set; }
        #endregion
        #region - Attributes -
        private int _port;
        private ITcpServerModel _model;

        public SocketAsyncEventArgs _hearingEvent;

        public event EventHandler Connected;
        public event EventHandler Received;
        public event EventHandler Disconnected;

        private object _locker;
        #endregion
    }
}
