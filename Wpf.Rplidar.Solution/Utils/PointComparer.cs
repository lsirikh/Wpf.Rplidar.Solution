using System.Collections.Generic;
using System.Windows;

namespace Wpf.Rplidar.Solution.Utils
{
    /****************************************************************************
        Purpose      : To compare the points allocated               
        Created By   : GHLee                                                
        Created On   : 7/31/2023 4:34:15 PM                                                    
        Department   : SW Team                                                   
        Company      : Sensorway Co., Ltd.                                       
        Email        : lsirikh@naver.com                                         
     ****************************************************************************/

    public class PointComparer : IEqualityComparer<Point>
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
        public bool Equals(Point x, Point y)
        {
            return x.X == y.X && x.Y == y.Y;
        }

        public int GetHashCode(Point obj)
        {
            int hash = 17;
            hash = hash * 31 + obj.X.GetHashCode();
            hash = hash * 31 + obj.Y.GetHashCode();
            return hash;
        }
        #endregion
        #region - IHanldes -
        #endregion
        #region - Properties -
        #endregion
        #region - Attributes -
        #endregion
    }
}
