using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PZ2.Model
{
    public class DrawingShape
    {
        public object Shape { get; set; }
        public string ShapeName { get; set; }
        public ulong ShapeId { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public int Row { get; set; }
        public int Column { get; set; }
        public string SwitchStatus { get; set; }
        public ulong LineId { get; set; }
        public string LineName { get; set; }
    }
}
