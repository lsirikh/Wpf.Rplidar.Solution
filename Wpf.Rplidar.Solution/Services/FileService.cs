using System.IO;
using System.Net.Sockets;
using System;
using System.Threading;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Wpf.Rplidar.Solution.Models;
using IniParser;
using Caliburn.Micro;
using IniParser.Model;
using System.Linq;
using System.Windows.Markup;
using System.Windows.Media;

namespace Wpf.Rplidar.Solution.Services
{
    /****************************************************************************
        Purpose      :                                                           
        Created By   : GHLee                                                
        Created On   : 7/4/2023 9:39:19 AM                                                    
        Department   : SW Team                                                   
        Company      : Sensorway Co., Ltd.                                       
        Email        : lsirikh@naver.com                                         
     ****************************************************************************/

    public class FileService: IDisposable
    {

        #region - Ctors -
        public FileService()
        {
            _locker = new ReaderWriterLock();
        }
        #endregion
        #region - Implementation of Interface -
        #endregion
        #region - Overrides -
        #endregion
        #region - Binding Methods -
        #endregion
        #region - Processes -
        public void Init()
        {
            string appDirectory = AppDomain.CurrentDomain.BaseDirectory;

            _folderPath = appDirectory + @"\Properties";

            CheckDir();
            CreateFile(CreateFilePath());
            
        }

        private void CheckDir()
        {
            try
            {
                //DirectoryInfo 객체 생성
                DirectoryInfo di = new DirectoryInfo(_folderPath);

                //폴더 없으면 생성
                if (di.Exists == false)
                {
                    di.Create();
                }
            }
            catch 
            {
                
            }

        }

        private string CreateFilePath()
        {
            var fileName = $"properties.ini";
            
            return _folderPath + @"\" + fileName;
        }

        private void CreateFile(string filePath)
        {
            try
            {
                FileInfo fi = new FileInfo(filePath);

                //폴더 없으면 생성
                if (fi.Exists == false)
                    CreateBlankIni(filePath);
            }
            catch 
            {
            }
        }

        private void CreateBlankIni(string filePath)
        {
            if (_parser == null)
                _parser = new FileIniDataParser();

            //Get the data
            _data = new IniData();

            //Add a new section and some keys
            _data.Sections.AddSection("Network");
            _data["Network"].AddKey("IpAddress", "0.0.0.0");
            _data["Network"].AddKey("Port", "15100");

            _data.Sections.AddSection("LidarSetting");
            _data["LidarSetting"].AddKey("Width", "1600");
            _data["LidarSetting"].AddKey("Height", "1600");
            _data["LidarSetting"].AddKey("OffsetAngle", "-5");
            _data["LidarSetting"].AddKey("XOffset", "0");
            _data["LidarSetting"].AddKey("YOffset", "0");
            _data["LidarSetting"].AddKey("DivideOffset", "4.0");
            _data["LidarSetting"].AddKey("SensorLocation", "True");

            _data["LidarSetting"].AddKey("Boundary", "");

            _parser.WriteFile(filePath, _data);
        }


        public void CreateSetupModel(SetupModel model)
        {
            if(_parser == null)
                _parser = new FileIniDataParser();

            
            _data = _parser.ReadFile(CreateFilePath());

            model.IpAddress = _data["Network"]["IpAddress"].ToString();
            model.Port = int.Parse(_data["Network"]["Port"].ToString());

            model.Width = double.Parse(_data["LidarSetting"]["Width"].ToString());
            model.Height = double.Parse(_data["LidarSetting"]["Height"].ToString());
            model.OffsetAngle = double.Parse(_data["LidarSetting"]["OffsetAngle"].ToString());
            model.XOffset = double.Parse(_data["LidarSetting"]["XOffset"].ToString());
            model.YOffset = double.Parse(_data["LidarSetting"]["YOffset"].ToString());
            model.DivideOffset = double.Parse(_data["LidarSetting"]["DivideOffset"].ToString());
            model.SensorLocation = bool.Parse(_data["LidarSetting"]["SensorLocation"].ToString());

            try
            {
                var pointString = _data["LidarSetting"]["Boundary"].ToString().Split(' ').ToList();

                model.BoundaryPoints.Clear();
                foreach (var item in pointString)
                {
                    var pArray = item.ToString().Split(',').ToArray();
                    var point = new Point(double.Parse(pArray[0]), double.Parse(pArray[1]));
                    model.BoundaryPoints.Add(point);
                }
            }
            catch
            {
            }
            
        }

        public void SaveSetupModel(SetupModel model)
        {
            _data["Network"]["IpAddress"] = model.IpAddress;
            _data["Network"]["Port"] = model.Port.ToString();


            _data["LidarSetting"]["Width"] = model.Width.ToString();
            _data["LidarSetting"]["Height"] = model.Height.ToString();
            _data["LidarSetting"]["OffsetAngle"] = model.OffsetAngle.ToString();
            _data["LidarSetting"]["XOffset"] = model.XOffset.ToString();
            _data["LidarSetting"]["YOffset"] = model.YOffset.ToString();
            _data["LidarSetting"]["DivideOffset"] = model.DivideOffset.ToString();
            _data["LidarSetting"]["SensorLocation"] = model.SensorLocation.ToString();

            var tempPoint = new PointCollection(model.BoundaryPoints);
            tempPoint.Remove(tempPoint.LastOrDefault());
            string pointsString = string.Join(" ", tempPoint.Select(p => $"{p.X},{p.Y}"));

            _data["LidarSetting"]["Boundary"] = pointsString;

            _parser.WriteFile(CreateFilePath(), _data);
        }

        public void Dispose()
        {
            _parser = null;
            _data = null;
            GC.Collect();
        }

        #endregion
        #region - IHanldes -
        #endregion
        #region - Properties -
        #endregion
        #region - Attributes -
        private ReaderWriterLock _locker;
        private string _folderPath;
        private FileIniDataParser _parser;
        private IniData _data;
        #endregion
    }
}
