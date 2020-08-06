using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PZ2.Model
{
    public static class NetworkModel
    {
        public static List<SubstationEntity> substationEntities { get; set; } = new List<SubstationEntity>();
        public static List<NodeEntity> nodeEntities { get; set; } = new List<NodeEntity>();
        public static List<SwitchEntity> switchEntities { get; set; } = new List<SwitchEntity>();
        public static List<LineEntity> vertices { get; set; } = new List<LineEntity>();
    }
}
