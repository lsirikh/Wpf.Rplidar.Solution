using RPLidarA1;
using System.Collections.Generic;
using System.Diagnostics;
using System;
using System.Windows.Threading;
using System.Net;
using System.Threading.Tasks;

namespace Wpf.Rplidar.Solution.Services
{
    /****************************************************************************
        Purpose      :                                                           
        Created By   : GHLee                                                
        Created On   : 6/27/2023 11:32:21 PM                                                    
        Department   : SW Team                                                   
        Company      : Sensorway Co., Ltd.                                       
        Email        : lsirikh@naver.com                                         
     ****************************************************************************/

    public class LidarService
    {

        #region - Ctors -
        public LidarService()
        {

        }
        #endregion
        #region - Implementation of Interface -
        #endregion
        #region - Overrides -
        #endregion
        #region - Binding Methods -
        #endregion
        #region - Processes -
        
        public void InitSerial()
        {
            // 시리얼 포트 설정
            string portName = "COM4"; // 연결할 COM 포트 이름

            // SerialPort 객체 생성 및 설정
            rplidar = new RPLidarA1class();
            string[] porte = rplidar.FindSerialPorts(); //Find serial ports

            Debug.WriteLine("COM 포트 연결을 시도합니다.");
            Message?.Invoke("COM 포트 연결을 시도합니다.");

            try
            {
                // 시리얼 포트 열기
                bool result = rplidar.ConnectSerial(portName); //Open Serial Port COM1
                Debug.WriteLine("COM 포트 연결 성공");
                Message?.Invoke("COM 포트 연결 성공");

                // DispatcherTimer를 사용하여 데이터 수신 주기 설정
                timer = new DispatcherTimer();
                timer.Interval = TimeSpan.FromMilliseconds(100); // 100ms마다 데이터 수신
                timer.Tick += Timer_Tick;
                timer.Start();

                string snum = rplidar.SerialNum(); //Get RPLidar Info

                Debug.WriteLine($"Lidar Info : {snum}");
                Message?.Invoke($"Lidar Info : {snum}");

                if (!rplidar.BoostScan()) //Start BoostScan
                {
                    rplidar.CloseSerial(); //On scan error close serial port
                    return;
                }

            }
            catch (Exception ex)
            {
                Debug.WriteLine("Lidar 셋업 중 오류가 발생했습니다: " + ex.Message);
                Message?.Invoke("Lidar 셋업 중 오류가 발생했습니다: " + ex.Message);
            }
        }

        public void UninitSerial()
        {
            try
            {
                // 시리얼 포트 닫기
                timer.Stop();
                rplidar.Stop_Scan(); //Stop Scan Thread
                rplidar.CloseSerial(); //Close serial port
                Debug.WriteLine("Lidar를 정상적으로 종료합니다.");
                Message?.Invoke("Lidar를 정상적으로 종료합니다.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Lidar 종료 중 오류가 발생했습니다: " + ex.Message);
                Message?.Invoke("Lidar 종료 중 오류가 발생했습니다: " + ex.Message);
            }
            
        }

        private async void Timer_Tick(object sender, EventArgs e)
        {

            await Task.Run(() =>
            {
                // Lidar 데이터 수신 주기마다 실행되는 코드
                // Lidar 데이터 처리 및 표시 등을 수행할 수 있습니다.
                // 여기서는 간단히 수신된 데이터를 콘솔에 출력하는 예제입니다.
                List<Measure> measures = new List<Measure>(); //Measures list
                string sample_second = rplidar.measure_second.ToString();

                lock (rplidar.Measure_List)
                {
                    foreach (Measure m in rplidar.Measure_List) //Copy Measure List
                    {
                        //filter
                        if (!(m.distance > 50)) continue;

                        measures.Add(m);
                    }
                    SendPoints?.Invoke(measures);

                    rplidar.Measure_List.Clear(); //Clear original List
                }
            });
        }
        #endregion
        #region - IHanldes -
        #endregion
        #region - Properties -
        #endregion
        #region - Attributes -
        private DispatcherTimer timer;
        private RPLidarA1class rplidar;

        public delegate void EventDele(string msg);
        public event EventDele Message;

        public delegate Task SendDele(List<Measure> measures);
        public event SendDele SendPoints;
        #endregion
    }
}
