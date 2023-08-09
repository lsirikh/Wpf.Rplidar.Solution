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

        public LidarDataModel(double width, double height, double x, double y, int cur, int tot, int seq)
        {
            Width = width; 
            Height = height;
            X = x; 
            Y = y;
            CurrentGroup = cur;
            TotalGroup = tot;
            Sequence = seq;
        }

        [JsonProperty("width", Order = 0)]
        public double Width { get; set; }

        [JsonProperty("height", Order = 1)]
        public double Height { get; set; }

        [JsonProperty("x", Order = 2)]
        public double X { get; set; }

        [JsonProperty("y", Order = 3)]
        public double Y { get; set; }

        [JsonProperty("current_group", Order = 4)]
        public int CurrentGroup { get; set; }

        [JsonProperty("total_group", Order = 5)]
        public int TotalGroup { get; set; }

        [JsonProperty("sequence", Order = 6)]
        public int Sequence { get; set; }

    }
}
