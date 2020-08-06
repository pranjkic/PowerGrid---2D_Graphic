using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PZ2.Presentation;
using PZ2.Repository;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            NetworkModelRepository.LoadModel("Geographic.xml");
            TransformNetworkModelObjectsToShapes();
            InitializeLineMatrix();
            IsEmptyCheck();
            DrawLines();
            DrawShapes();
            InitializeMatrix();
        }
    }
}
