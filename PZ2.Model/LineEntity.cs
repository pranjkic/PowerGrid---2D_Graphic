using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PZ2.Model
{
    public class LineEntity
    {
        public ulong Id { get; set; }
        public string Name { get; set; }
        public bool IsUnderground { get; set; }
        public float R { get; set; }
        public string ConductorMaterial { get; set; }
        public string LineType { get; set; }
        public long ThermalConstantHeat { get; set; }
        public ulong FirstEnd { get; set; }
        public ulong SecondEnd { get; set; }
        public List<Point> Vertices { get; set; } = new List<Point>();
    }
}
