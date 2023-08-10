using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Drawing;
using System.IO.Ports;
using System.Collections.Generic;

namespace RPLidarA1
{
    public class RPLidarA1class
    {
        SerialPort seriale; //serialport object
        static string SYNC = "" + (char)0xA5 + (char)0x5A; //Sincronismo
        private string m_ricevuto; //Received string
        private int lbyte;
        int scalelvl;
        Measure measure;
        public List<Measure> Measure_List;
        string serialport;

        Thread Main_Scan;
        // Define the cancellation token.
        static CancellationTokenSource source;
        CancellationToken token; 

        public int measure_second;
        int ms;
        Timer timer;


        public string[] FindSerialPorts() //Find Serial Ports on PC
        {
            string[] porte = SerialPort.GetPortNames(); 
            return porte;
        }

        public bool ConnectSerial (string com) //Open Serial Port com 
        {
            serialport = com;
            int baudrate = 115200;
            seriale = new SerialPort(com, baudrate, Parity.None, 8, StopBits.One);
            seriale.ReadTimeout = 1000;
            seriale.ReadBufferSize = 10000;
            seriale.RtsEnable = true;

            try
            {
                seriale.Open();

            }
            catch
            {
                return false;
            }
            seriale.DtrEnable = true; //Stop Motor
            
            return true;
        }

        public void CloseSerial () //Close Serial Port
        {
            try
            {
                if (seriale.IsOpen)
                {
                    seriale.Close();
                    seriale.Dispose();
                    source.Dispose();
                }
            }
            catch
            {
            }
        }

        public void StopMotor ()
        {
            if (seriale!=null && Main_Scan != null && Main_Scan.IsAlive == true)
            {
                Stop_Scan();
                CloseSerial();
                ConnectSerial(serialport);
            }
        }

        public void Reset() //Send reset command
        {
            string inviare;
            inviare = "" + (char)0xA5 + (char)0x40;
            Writeserial(inviare);
            Thread.Sleep(5000);
        }

        public bool GetHealt()  //Retrive Healt
        {
            string inviare;
            int pos;
            inviare = "" + (char)0xA5 + (char)0x52;
            Writeserial(inviare);
            if (!ReadSerial1Shot()) return false;
            pos = m_ricevuto.IndexOf(SYNC);
            if (pos < 0) return false;
            if (m_ricevuto.Substring((pos + 3), 3) != "\0\0\0") return false;
            return true;
        }

        public string SerialNum() //Retrive Info from Lidar
        {
            string inviare;
            int pos;
            inviare = "" +(char) 0xA5 + (char)0x50;
            Writeserial(inviare);
            if (!ReadSerial1Shot()) return "";
            pos = m_ricevuto.IndexOf(SYNC);
            if (pos < 0) return "";
            string dato = "" + m_ricevuto.Substring(pos + 7);
 
            int d = (int)dato.ElementAt(0);
            string info = "Mod:" + d.ToString() + " Ver:";
            d = (int)dato.ElementAt(2);
            info += d.ToString() + ".";
            d = (int)dato.ElementAt(1);
            info += d.ToString() + " Hw:";
            d = (int)dato.ElementAt(3);
            info += d.ToString() + " Serial:";
            for (int x = 4; x < dato.Length; x++)
            {
                d = (byte)dato.ElementAt(x);
                info += d.ToString("X2") + " ";
            }
            return info;
        }

        private bool ReadSerial1Shot()  //Read single answer
        {
            if (seriale.IsOpen == false) return false;
            lbyte = 0;
            m_ricevuto = "";

            for (int x = 0; (x < 700) && (lbyte == 0); x++)
            {
                System.Threading.Thread.Sleep(10);
                try
                {
                    lbyte = seriale.BytesToRead;
                }
                catch
                {
                    return false;
                }
            }

            try
            {
                lbyte = seriale.BytesToRead;
            }
            catch
            {
                return false;
            }
            for (int z = 0; z < lbyte; z++)
            {
                m_ricevuto += (char)seriale.ReadByte();
            }
            return true;
        }

        private bool Writeserial(string s) //Write to serial port
        {
            if (seriale.IsOpen == false) return false;

            byte[] ss = new byte[200];
            for (int x = 0; x < s.Length; x++)
            {
                ss[x] = (byte)s.ElementAt(x);
            }

            try
            {
                seriale.Write(ss, 0, s.Length);
            }
            catch
            {
                return false;
            }
            return true;
        }

        public bool BoostScan()
        {
            string inviare;
            Measure_List = new List<Measure>();
            Measure_List.Clear();

            if (Main_Scan!=null && Main_Scan.IsAlive == true) return false; //Already scanning
            if (seriale.IsOpen == false) return false; //Port closed

            seriale.DtrEnable = false; //Start Motor
            if (!GetHealt()) // It's Alive ?
            {
                Reset(); //No, try Reset
                if (!GetHealt())
                {
                    CloseSerial(); //Bad, close serial port
                    return false;
                }
            }

            for (int j = 0; j < 5; j++)
            {
                //GET_RPLIDAR_CONFIG
                inviare = "" + (char)0xA5 + (char)0x84 + (char)0x24 + (char)0x75 + (char)0x00 + (char)0x00 + (char)0x00 + (char)0x02;
                for (int x = 0; x < 31; x++) inviare += (char)0x00;
                inviare += (char)0x72;
                Writeserial(inviare);
                ReadSerial1Shot();

                inviare = "" + (char)0xA5 + (char)0x5A + (char)0x05 + (char)0x00 + (char)0x00 + (char)0x00 + (char)0x20 + (char)0x75
                    + (char)0x00 + (char)0x00 + (char)0x00 + (char)0x84;

                if (m_ricevuto.IndexOf(inviare) >= 0)
                {
                    break;
                }
            }

            inviare = "" + (char)0xA5 + (char)0x82 + (char)0x05 + (char)0x02 + (char)0x00 + (char)0x00 + (char)0x00 + (char)0x00 + (char)0X20;
            Writeserial(inviare);

            measure_second = 0; //Initialize mesure/second variable
            ms = 0;
            timer = new Timer(second, null, 1000, 1000); //Start Timer
            source = new CancellationTokenSource();
            token = source.Token;
            Main_Scan = new Thread(ReadSerial); //Start Scan Thread
            Main_Scan.Start();

            return true;
        }

        private void second(object state)  // Event timer every second
        {
            lock ((object)measure_second)
            {
                measure_second = ms;
                ms = 0;
            }
        }

        private void ReadSerial()
        {
            try
            {
                lbyte = 0;
                int p;
                char c;
                m_ricevuto = "";
                p = 0;

                ReadSerial1Shot();

                p = m_ricevuto.IndexOf(SYNC);
                if (p < 0) Stop_Scan();
                m_ricevuto = m_ricevuto.Remove(p, 7);

                while (source.IsCancellationRequested == false)
                {
                    try
                    {
                        lbyte = seriale.BytesToRead;
                    }
                    catch
                    {
                        Stop_Scan();
                    }
                    if (lbyte == 0) continue;

                    for (int z = 0; z < lbyte; z++)
                    {
                        c = (char)seriale.ReadByte();
                        m_ricevuto += c;
                        if (m_ricevuto.Length == 136)
                        {
                            DecodeCabinBest(m_ricevuto);
                            m_ricevuto = m_ricevuto.Substring(132);
                            break;
                        }
                    }
                }
            }
            catch 
            {
            }
            
        }

        public void Stop_Scan()  //Stop Scan Thread
        {
            try
            {
                string inviare;
                timer?.Change(Timeout.Infinite, Timeout.Infinite);
                timer?.Dispose();

                if (Main_Scan != null && Main_Scan.IsAlive == true)
                {
                    source.Cancel();
                    while (Main_Scan.ThreadState == System.Threading.ThreadState.Stopped)
                    {
                        Thread.Sleep(100);
                    }
                }

                inviare = "" + (char)0xA5 + (char)0x25;
                Writeserial(inviare);
                seriale.DiscardInBuffer(); // clear read serial buffer
                seriale.DtrEnable = true; //Stop Motor
            }
            catch 
            {

                throw;
            }
            
        }

        private void DecodeCabinBest(string data)
        {
            float fstartangle, fstartanglenext, delta;
            int startangle, startanglenext;
            startangle = 0;
            int a, b, predict1, predict2, major, major2, k, dist_base, dist_base2, scalelvl2;
            byte mask;
            float f1;

            a = (int)data.ElementAt(0);
            b = (int)data.ElementAt(1);
            a >>= 4;
            b >>= 4;
            if ((a != 0x0A) || (b != 0x05)) //SYNC PRESENT ?
            {
                return;
            }


            startangle = (byte)data.ElementAt(2);
            startangle = startangle + RotateLeft((byte)data.ElementAt(3), 8);
            fstartangle = (float)startangle / 64;

            startanglenext = (byte)data.ElementAt(134);
            startanglenext = startanglenext + RotateLeft((byte)data.ElementAt(135), 8);
            fstartanglenext = (float)startanglenext / 64;
            if (fstartanglenext < fstartangle) fstartanglenext = fstartanglenext + 360;
            delta = (fstartanglenext - fstartangle) / 95;

            k = 0;

            data = data.Substring(0, 132);
            for (int x = 4; x < data.Length; x = x + 4)
            {
                if (x + 4 > data.Length)
                {
                    break;
                }

                if ((x + 4) < data.Length)
                {
                    mask = 0b00001111;
                    major2 = ((byte)data.ElementAt(x + 4 + 1)) & mask;
                    major2 = RotateLeft(major2, 8);
                    major2 = major2 + (byte)data.ElementAt(x + 4);
                }
                else major2 = 0; //// NON E' PROPRIO COSI'

                major2 = varbitscale_decode(major2);
                dist_base2 = major2;
                scalelvl2 = scalelvl;

                mask = 0b00001111;
                major = ((byte)data.ElementAt(x + 1)) & mask;
                major = RotateLeft(major, 8);
                major = major + (byte)data.ElementAt(x);

                mask = 0b00111111;
                predict1 = ((byte)data.ElementAt(x + 2)) & mask;
                predict1 = RotateLeft(predict1, 4);
                mask = 0b11110000;
                predict1 = predict1 + (RotateRight((((byte)data.ElementAt(x + 1)) & mask), 4));


                predict2 = RotateLeft((byte)data.ElementAt(x + 3), 2);
                mask = 0b11000000;
                predict2 = predict2 + RotateRight(((byte)data.ElementAt(x + 2)) & mask, 6);


                major = varbitscale_decode(major);
                dist_base = major;

                if ((major == 0) & (major2 != 0))
                {
                    major = major2;
                    dist_base = major2;
                    scalelvl = scalelvl2;
                }

                major = RotateLeft(major, 2);

                mask = 0b11111111;
                predict1 = predict1 & mask;
                if (predict1 >= 0xF0) predict1 = RotateLeft((dist_base), 2);
                else
                {
                    predict1 = RotateLeft(predict1, scalelvl);
                    predict1 = RotateLeft((predict1 + dist_base), 2);
                }

                mask = 0b11111111;
                predict2 = predict2 & mask;
                if (predict2 >= 0xF0) predict2 = RotateLeft((dist_base2), 2);
                else
                {
                    predict2 = RotateLeft(predict2, scalelvl2);
                    predict2 = RotateLeft((predict2 + dist_base2), 2);
                }

                f1 = (fstartangle + (delta * k));
                k++;
                if (f1 > 360)
                {
                    f1 = f1 - 360;
                }


                if (major != 0 && f1<=360)
                {
                    measure = new Measure();
                    measure.X= (int)(major * (System.Math.Sin((Math.PI / 180) * f1)));
                    measure.Y= (int)(major * (System.Math.Cos((Math.PI / 180) * f1)));
                    measure.angle = f1;
                    measure.distance = major;
                    lock (Measure_List)
                    {
                        Measure_List.Add(measure); //Add measure to list
                        ms++; //increment measure number
                    }
                }

                f1 = (fstartangle + (delta * k));
                k++;
                if (f1 > 360)
                {
                    f1 = f1 - 360;
                }


                if (predict1 != 0 && f1 <= 360)
                {
                    measure = new Measure();
                    measure.X = (int)(predict1 * (System.Math.Sin((Math.PI / 180) * f1)));
                    measure.Y = (int)(predict1 * (System.Math.Cos((Math.PI / 180) * f1)));
                    measure.angle = f1;
                    measure.distance = predict1;
                    lock (Measure_List)
                    {
                        Measure_List.Add(measure); //Add measure to list
                        ms++; //increment measure number
                    }
                }

                f1 = (fstartangle + (delta * k));
                k++;
                if (f1 > 360)
                {
                    f1 = f1 - 360;
                }


                if (predict2 != 0 && f1 <= 360)
                {
                    measure = new Measure();
                    measure.X = (int)(predict2 * (System.Math.Sin((Math.PI / 180) * f1)));
                    measure.Y = (int)(predict2 * (System.Math.Cos((Math.PI / 180) * f1)));
                    measure.angle = f1;
                    measure.distance = predict2;
                    lock (Measure_List)
                    {
                        Measure_List.Add(measure); //Add measure to list
                        ms++; //increment measure number
                    }
                }

                if (Measure_List.Count>100000) //Remove Mesure extra 100000 point
                {
                    Measure_List.RemoveRange(0, (Measure_List.Count-100000));
                }

            }
        }

        private static int RotateLeft(int value, int count) //Rotate bit Left
        {
            uint v;
            uint val = (uint)value;
            v = (uint)((val << count) | (val >> (32 - count)));
            uint mask;
            mask = 0b11111111111111110000000000000000;
            v = v & ~(mask);
            return (int)v;
        }

        private static int RotateRight(int value, int count) //Rotate bit Right
        {
            uint v;
            uint val = (uint)value;
            v = (uint)((value >> count) | (value << (32 - count)));
            uint mask;
            mask = 0b11111111111111110000000000000000;
            v = v & ~(mask);
            return (int)v;
        }

        private int varbitscale_decode(int scaled)
        {
            int[] VBS_SCALED_BASE = { 3328, 1792, 1280, 512, 0 };
            int[] VBS_SCALED_LVL = { 4, 3, 2, 1, 0 };
            int[] VBS_TARGET_BASE = { 14, 12, 11, 9, 0 };

            for (int i = 0; i < VBS_SCALED_BASE.Length; i++)
            {
                int remain = scaled - VBS_SCALED_BASE[i];
                if (remain >= 0)
                {
                    scalelvl = VBS_SCALED_LVL[i];
                    return (RotateLeft(0x01, VBS_TARGET_BASE[i]) + (RotateLeft(remain, VBS_SCALED_LVL[i])));
                }
            }
            scalelvl = 0;
            return 0;
        }

    }

    public class Measure : IEquatable<Measure>  //Measure Class
    {
        public float angle { get; set; }
        public int distance { get; set; }
        public double X { get; set; }
        public double Y { get; set; }

        bool IEquatable<Measure>.Equals(Measure other)
        {
            if (other == null) return false;
            return true;
        }
    }
}
