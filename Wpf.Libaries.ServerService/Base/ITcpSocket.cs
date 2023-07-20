using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Wpf.Libaries.ServerService.Models;
using Wpf.Libaries.ServerService.Utils;

namespace Wpf.Libaries.ServerService.Base
{
    public interface ITcpSocket : ITcpTaskTimer
    {
        EnumTcpMode Mode { get; }
        Socket Socket { get; }

        void InitSocket(ITcpServerModel model = default);
        void CloseSocket();
        Task SendRequest(string msg, IPEndPoint selectedIp = null);
    }
}