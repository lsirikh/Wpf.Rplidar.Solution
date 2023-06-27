using System;
using Wpf.Rplidar.Solution.Utils;

namespace Wpf.Rplidar.Solution.Models
{
    public interface ILogModel
    {
        DateTime CreatedTime { get; set; }
        int Id { get; set; }
        string Message { get; set; }
        EnumLogType Type { get; set; }
    }
}