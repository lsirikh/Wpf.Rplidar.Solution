using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wpf.Rplidar.Solution.Models;
using Wpf.Rplidar.Solution.Utils;

namespace Wpf.Rplidar.Solution.ViewModels
{
    public class LogViewModel : Screen
    {

        public LogViewModel(LogModel model)
        {

            _model = model;
        }

        private LogModel _model;

        public string CreatedTime 
        { 
            get => _model.CreatedTime.ToString("yyyy-MM-dd HH:mm:ss.ff");
            //set 
            //{
            //    _model.CreatedTime = DateTime.Parse(value);
            //    NotifyOfPropertyChange(() => CreatedTime);
            //}
        }
        public string Message 
        {
            get => _model.Message;
            set
            {
                _model.Message = value;
                NotifyOfPropertyChange(() => Message);
            }
        }
        public EnumLogType Type 
        {
            get => _model.Type;
            set
            {
                _model.Type = value;
                NotifyOfPropertyChange(() => Type);
            }
        }
    }
}
