using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Wpf.Rplidar.Solution.Models
{
    public class LidarDataModel
    {

        public LidarDataModel()
        {
            
        }

        public LidarDataModel(double width, double height, double x, double y)
        {
            Width = width; 
            Height = height;
            X = x; 
            Y = y;
        }

        [JsonProperty("width", Order = 0)]
        public double Width { get; set; }
        [JsonProperty("height", Order = 0)]
        public double Height { get; set; }
        [JsonProperty("x", Order = 0)]
        public double X { get; set; }
        [JsonProperty("y", Order = 0)]
        public double Y { get; set; }

    }
}
