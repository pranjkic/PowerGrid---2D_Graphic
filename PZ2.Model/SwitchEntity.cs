using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PZ2.Model
{
    public class SwitchEntity
    {
        public UInt64 Id { get; set; }
        public string Name { get; set; }
        public string Status { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public int Row { get; set; }
        public int Column { get; set; }
    }
}
