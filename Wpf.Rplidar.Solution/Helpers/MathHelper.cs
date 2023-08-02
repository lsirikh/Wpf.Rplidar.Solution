using System.Windows;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Wpf.Rplidar.Solution.Helpers
{
    /****************************************************************************
        Purpose      :                                                           
        Created By   : GHLee                                                
        Created On   : 8/2/2023 7:59:08 PM                                                    
        Department   : SW Team                                                   
        Company      : Sensorway Co., Ltd.                                       
        Email        : lsirikh@naver.com                                         
     ****************************************************************************/

    public static class MathHelper
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
        public static Point RotatePoint(Point point, double theta)
        {
            double radian = theta * Math.PI / 180; // Convert degree to radian
            double cosTheta = Math.Cos(radian);
            double sinTheta = Math.Sin(radian);
            double x = point.X * cosTheta - point.Y * sinTheta;
            double y = point.X * sinTheta + point.Y * cosTheta;
            return new Point(x, y);
        }

        public static Point RotatePointAroundPivot(Point point, Point pivot, double theta)
        {
            // Translate point back to origin
            point.X -= pivot.X;
            point.Y -= pivot.Y;

            // Rotate point
            double radian = theta * Math.PI / 180; // Convert degree to radian
            double cosTheta = Math.Cos(radian);
            double sinTheta = Math.Sin(radian);
            double xNew = point.X * cosTheta - point.Y * sinTheta;
            double yNew = point.X * sinTheta + point.Y * cosTheta;

            // Translate point back
            Point newPoint = new Point(xNew + pivot.X, yNew + pivot.Y);
            return newPoint;
        }

        public static (double Width, double Height) CalculateWidthAndHeight(List<Point> points, double divedOffset )
        {
            try
            {
                if (points != null && points.Count < 4)
                {
                    throw new ArgumentException("Exactly 4 points are required", nameof(points));
                }


                double minX = points.Min(point => point.X / divedOffset);
                double maxX = points.Max(point => point.X / divedOffset);

                double minY = points.Min(point => point.Y / divedOffset);
                double maxY = points.Max(point => point.Y / divedOffset);

                double width = maxX - minX;
                double height = maxY - minY;

                return (width, height);
            }
            catch (ArgumentException ex)
            {
                Debug.WriteLine($"Raised Exception in {nameof(CalculateWidthAndHeight)} :" + ex.Message);
                return (0.0, 0.0);
            }

        }

        public static Point MapToResolution(double canvasWidth, double canvasHeight, double pointX, double pointY)
        {
            // Calculate the ratio of the old size to the new size
            double widthRatio = 1920 / canvasWidth;
            double heightRatio = 1080 / canvasHeight;

            // Multiply the original coordinates by the ratio to get the new coordinates
            double newX = pointX * widthRatio;
            double newY = pointY * heightRatio;

            return new Point(newX, newY);
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
