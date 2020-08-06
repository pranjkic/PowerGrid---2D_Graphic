using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PZ2.Model
{
    public class Node
    {
        public ulong ShapeId { get; set; }
        public int Row { get; set; }
        public int Column { get; set; }
        public int PrewiousRow { get; set; }
        public int PrewiousColumn { get; set; }
    }
}
