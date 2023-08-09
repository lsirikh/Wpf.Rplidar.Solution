using RPLidarA1;
using System.Collections.Generic;

namespace Wpf.Rplidar.Solution.Models
{
    /****************************************************************************
        Purpose      :                                                           
        Created By   : GHLee                                                
        Created On   : 7/25/2023 3:47:55 PM                                                    
        Department   : SW Team                                                   
        Company      : Sensorway Co., Ltd.                                       
        Email        : lsirikh@naver.com                                         
     ****************************************************************************/

    public class GroupModel
    {

        #region - Ctors -
        public GroupModel()
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
        #endregion
        #region - IHanldes -
        #endregion
        #region - Properties -
        public List<Measure> Measures { get; set; }
        public Measure Center { get; set; }
        #endregion
        #region - Attributes -
        #endregion
    }
}
