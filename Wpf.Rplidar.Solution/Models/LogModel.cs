using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wpf.Rplidar.Solution.Utils;

namespace Wpf.Rplidar.Solution.Models
{
    public class LogModel : ILogModel
    {
        public LogModel()
        {
                
        }

        public LogModel(int id, DateTime dateTime, EnumLogType type, string message)
        {
            Id = id;
            CreatedTime = dateTime;
            Type = type;
            Message = message;
        }
        public int Id { get; set; }
        public DateTime CreatedTime { get; set; }
        public EnumLogType Type { get; set; }
        public string Message { get; set; }
    }
}
