using System.Collections.Generic;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace Wpf.Rplidar.Solution.Models
{
    /****************************************************************************
        Purpose      :                                                           
        Created By   : GHLee                                                
        Created On   : 7/4/2023 9:42:50 AM                                                    
        Department   : SW Team                                                   
        Company      : Sensorway Co., Ltd.                                       
        Email        : lsirikh@naver.com                                         
     ****************************************************************************/

    public class SetupModel
    {

        #region - Ctors -
        #endregion
        #region - Implementation of Interface -
        #endregion
        #region - Overrides -
        #endregion
        #region - Binding Methods -
        #endregion
        #region - Processes -
        #endregion
        #region - IHanldes -
        #endregion
        #region - Properties -
        public string IpAddress { get; set; }
        public int Port { get; set; }

        public double Width { get; set; }
        public double Height { get; set; }
        //public Measures<Point> BoundaryPoints { get; set; } = new Measures<Point>();
        public PointCollection BoundaryPoints { get; set; } = new PointCollection();
        public double OffsetAngle { get; set; }
        public double XOffset { get; set; }
        public double YOffset { get; set; }
        public double DivideOffset { get; set; }
        public bool SensorLocation { get; set; }
        #endregion
        #region - Attributes -
        #endregion
    }
}
