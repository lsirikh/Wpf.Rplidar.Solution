using RPLidarA1;
using System.Collections.Generic;
using System.Diagnostics;
using System;
using System.Windows.Threading;
using System.Net;
using System.Threading.Tasks;
using System.Timers;
using System.Threading;
using System.Windows.Controls;
using System.Collections.Concurrent;
using System.Collections;
using System.Text.RegularExpressions;
using Wpf.Rplidar.Solution.Utils;
using Wpf.Rplidar.Solution.Models;
using System.Windows.Media;
using System.Windows;
using Wpf.Rplidar.Solution.Helpers;

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
        public LidarService(SetupModel setupModel
                            , LogService logService)
        {
            _setupModel = setupModel;
            _logService = logService;
        }
        #endregion
        #region - Implementation of Interface -
        #endregion
        #region - Overrides -
        #endregion
        #region - Binding Methods -
        #endregion
        #region - Processes -
        public async void InitSerial(string comport)
        {

            cts = new CancellationTokenSource();

            // 시리얼 포트 설정
            string portName = comport; // 연결할 COM 포트 이름

            // SerialPort 객체 생성 및 설정
            rplidar = new RPLidarA1class();

            try
            {
                var msg = "RPlidar Sensor의 COM 포트 연결을 시도합니다.";
                Message?.Invoke(msg, "연결 시도");

                // 시리얼 포트 열기
                bool result = rplidar.ConnectSerial(portName); //Open Serial Port COM1
                
                if (!result) throw new Exception("RPlidar Sensor의 COM 포트 연결을 실패하였습니다.");

                msg = "RPlidar Sensor의 COM 포트 연결을 성공하였습니다.";
                Message?.Invoke(msg, "연결 성공");
                IsStarted = true;
                string snum = rplidar.SerialNum(); //Get RPLidar Info

                if (!rplidar.BoostScan()) //Start BoostScan
                {
                    rplidar.CloseSerial(); //On scan error close serial port
                    IsStarted = false;
                    return;
                }

                await TransferLidarPoints();

                msg = $"RPlidar Sensor Info : {snum}";
                Message?.Invoke(msg, "라이다 동작 중");
            }
           
            catch (Exception ex)
            {
                var msg = $"Lidar 셋업 중 오류가 발생했습니다: " + ex.Message;
                Message?.Invoke(msg, "오류 발생");
            }
        }

        private Task TransferLidarPoints()
        {
            try
            {
                // Lidar 데이터를 읽는 스레드 생성
                CaptureThread = new Thread(() =>
                {
                    while (!cts.IsCancellationRequested)
                    {
                        lock (rplidar.Measure_List)
                        {
                            List<Measure> measureList = new List<Measure>();
                            foreach (Measure m in rplidar.Measure_List) //Copy Measure Measures
                            {
                                if (!(m.distance > 150)) continue;
                                measureList.Add(m);
                            }
                            lidarDataQueue.Enqueue(measureList); // 큐에 Lidar 데이터 추가

                            rplidar.Measure_List.Clear(); //Clear original Measures
                        }
                        Thread.Sleep(150);
                    }
                    var msg = $"{nameof(CaptureThread)} was finished";
                    _logService.Info(msg);
                });
                CaptureThread.Start();

                // Lidar 데이터를 처리하는 스레드 생성
                ProcessingThread = new Thread(() =>
                {
                    while (!cts.IsCancellationRequested)
                    {
                        if (lidarDataQueue.TryDequeue(out var measures))
                        {

                            // Lidar 데이터 처리
                            List<Point> pointList = new List<Point>();
                            foreach (Measure measure in measures)
                            {
                                //중점을 기준으로 (x,y) 수평이동
                                var x = (measure.X / 10) + (Width / 2);
                                var y = Height / 2 - (measure.Y / 10);

                                var point = MathHelper
                                .RotatePointAroundPivot(new Point(x, y), new Point(Width / 2, Height / 2), OffsetAngle);
                                pointList.Add(point);
                            }

                            pointDataQueue.Enqueue(pointList); // 큐에 Lidar 데이터 추가
                        }
                    }
                    var msg = $"{nameof(ProcessingThread)} was finished";
                    _logService.Info(msg);
                });
                ProcessingThread.Start();

                TransferThread = new Thread(() =>
                {
                    while (!cts.IsCancellationRequested)
                    {
                        if (pointDataQueue.TryDequeue(out var points))
                        {
                            // Lidar 데이터 처리
                            // ...
                            //SendPoints?.Invoke(measures);

                            TransferPoints?.Invoke(this, new PointListArgs(points));


                        }
                    }
                    var msg = $"{nameof(TransferThread)} was finished";
                    _logService.Info(msg);
                });
                TransferThread.Start();
            }
            catch (TaskCanceledException ex)
            {
                var msg = $"라이다 데이터 수신 작업 취소: " + ex.Message;
                Message?.Invoke(msg, "오류 발생");

                rplidar?.Stop_Scan(); //Stop Scan Thread
                rplidar?.CloseSerial(); //Close serial port
            }
            catch (Exception ex)
            {
                Message?.Invoke("라이다 데이터 수신 작업 오류 발생 :" + ex.Message, "오류 발생");

                rplidar?.Stop_Scan(); //Stop Scan Thread
                rplidar?.CloseSerial(); //Close serial port
            }

            return Task.CompletedTask;
        }

        public async void UninitSerial()
        {
            try
            {
                // 시리얼 포트 닫기
                if (cts != null && !cts.IsCancellationRequested)
                    cts?.Cancel();
                cts?.Dispose();
                await Task.Delay(200);

                if (IsStarted)
                {
                    rplidar?.Stop_Scan(); //Stop Scan Thread
                    rplidar?.CloseSerial(); //Close serial port
                    IsStarted = false;
                }

                Message?.Invoke("Lidar를 정상적으로 종료합니다.", "라이다 종료");
            }
            catch(Exception ex)
            {
                Message?.Invoke($"Lidar 종료 중 오류가 발생했습니다.({ex.Message})", "오류 발생");
            }

        }

       
        #endregion
        #region - IHanldes -
        #endregion
        #region - Properties -
        public Thread CaptureThread { get; private set; }
        public Thread ProcessingThread { get; private set; }
        public Thread TransferThread { get; private set; }
        private ConcurrentQueue<List<Measure>> lidarDataQueue = new ConcurrentQueue<List<Measure>>();
        private ConcurrentQueue<List<Point>> pointDataQueue = new ConcurrentQueue<List<Point>>();
        public double Width => _setupModel.Width;
        public double Height => _setupModel.Height;
        public double OffsetAngle => _setupModel.OffsetAngle;
        public bool SensorLocation=> _setupModel.SensorLocation;
        public bool IsStarted { get; set; } 
        #endregion
        #region - Attributes -
        private RPLidarA1class rplidar;
        private System.Timers.Timer timer;

        public delegate void EventDele(string msg, string status);
        public event EventDele Message;

        public event EventHandler TransferPoints;
        public event EventHandler TransferGroup;

        public CancellationTokenSource cts;
        private SetupModel _setupModel;
        private LogService _logService;
        #endregion
    }

    
}
