using RPLidarA1;
using System;
using System.Collections.Generic;
using System.Windows;
using Wpf.Rplidar.Solution.Models;

namespace Wpf.Rplidar.Solution.Utils
{
    /****************************************************************************
        Purpose      :                                                           
        Created By   : GHLee                                                
        Created On   : 7/24/2023 8:25:49 PM                                                    
        Department   : SW Team                                                   
        Company      : Sensorway Co., Ltd.                                       
        Email        : lsirikh@naver.com                                         
     ****************************************************************************/

    public class PointListArgs : EventArgs
    {

        #region - Ctors -
        public PointListArgs(List<Measure> list)
        {
            Measures = list;
        }

        public PointListArgs(List<Point> list)
        {
            Points = list;
        }
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
        public List<Measure> Measures { get; set; }
        public List<Point> Points { get; set; }
        #endregion
        #region - Attributes -
        #endregion
    }

    public class GroupListArgs : EventArgs
    {

        #region - Ctors -
        public GroupListArgs(List<GroupModel> list)
        {
            List = list;
        }
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
        public List<GroupModel> List { get; set; }
        #endregion
        #region - Attributes -
        #endregion
    }
}
