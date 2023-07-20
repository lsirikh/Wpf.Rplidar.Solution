namespace Wpf.Libaries.ServerService.Models
{
    public interface ITcpServerModel
    {
        string IpAddress { get; set; }
        int Port { get; set; }
        int HeartBeat { get; set; }
    }
}