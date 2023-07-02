namespace Wpf.Rplidar.Solution.Utils
{
    /****************************************************************************
        Purpose      :                                                           
        Created By   : GHLee                                                
        Created On   : 6/29/2023 6:52:06 PM                                                    
        Department   : SW Team                                                   
        Company      : Sensorway Co., Ltd.                                       
        Email        : lsirikh@naver.com                                         
     ****************************************************************************/

    /// <summary>
    /// Defines the current state of the mouse handling logic.
    /// </summary>
    public enum MouseHandlingMode
    {
        /// <summary>
        /// Not in any special mode.
        /// </summary>
        None,

        /// <summary>
        /// The user is left-dragging rectangles with the mouse.
        /// </summary>
        DraggingRectangles,

        /// <summary>
        /// The user is middle-mouse-button-dragging to pan the viewport.
        /// </summary>
        Panning,

        /// <summary>
        /// The user is holding down shift and left-clicking or right-clicking to zoom in or out.
        /// </summary>
        Zooming,

        /// <summary>
        /// The user is left-mouse-button draw Rectagle Boundary
        /// </summary>
        AddBoundary,
    }
}
