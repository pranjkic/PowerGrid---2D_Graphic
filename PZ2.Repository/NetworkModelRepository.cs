using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using PZ2.Model;

namespace PZ2.Repository
{
    public static class NetworkModelRepository
    {
        public static void LoadModel(string path)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(path);
            XmlNodeList nodeList;

            nodeList = xmlDoc.DocumentElement.SelectNodes("/NetworkModel/Substations/SubstationEntity");
            foreach (XmlNode node in nodeList)
            {
                SubstationEntity substationEntity = new SubstationEntity();

                substationEntity.Id = UInt64.Parse(node.SelectSingleNode("Id").InnerText);
                substationEntity.Name = node.SelectSingleNode("Name").InnerText;
                substationEntity.X = double.Parse(node.SelectSingleNode("X").InnerText);
                substationEntity.Y = double.Parse(node.SelectSingleNode("Y").InnerText);

                double newX, newY;
                ToLatLonConverter.ToLatLon(substationEntity.X, substationEntity.Y, 34, out newX, out newY);
                substationEntity.X = newX;
                substationEntity.Y = newY;

                MinMaxCoordCheck.Check(newX, newY);
                NetworkModel.substationEntities.Add(substationEntity);
            }

            nodeList = xmlDoc.DocumentElement.SelectNodes("/NetworkModel/Nodes/NodeEntity");
            foreach (XmlNode node in nodeList)
            {
                NodeEntity nodeEntity = new NodeEntity();

                nodeEntity.Id = UInt64.Parse(node.SelectSingleNode("Id").InnerText);
                nodeEntity.Name = node.SelectSingleNode("Name").InnerText;
                nodeEntity.X = double.Parse(node.SelectSingleNode("X").InnerText);
                nodeEntity.Y = double.Parse(node.SelectSingleNode("Y").InnerText);

                double newX, newY;
                ToLatLonConverter.ToLatLon(nodeEntity.X, nodeEntity.Y, 34, out newX, out newY);
                nodeEntity.X = newX;
                nodeEntity.Y = newY;

                MinMaxCoordCheck.Check(newX, newY);
                NetworkModel.nodeEntities.Add(nodeEntity);
            }

            nodeList = xmlDoc.DocumentElement.SelectNodes("/NetworkModel/Switches/SwitchEntity");
            foreach (XmlNode node in nodeList)
            {
                SwitchEntity switchEntity = new SwitchEntity();

                switchEntity.Id = UInt64.Parse(node.SelectSingleNode("Id").InnerText);
                switchEntity.Name = node.SelectSingleNode("Name").InnerText;
                switchEntity.X = double.Parse(node.SelectSingleNode("X").InnerText);
                switchEntity.Y = double.Parse(node.SelectSingleNode("Y").InnerText);
                switchEntity.Status = node.SelectSingleNode("Status").InnerText;

                double newX, newY;
                ToLatLonConverter.ToLatLon(switchEntity.X, switchEntity.Y, 34, out newX, out newY);
                switchEntity.X = newX;
                switchEntity.Y = newY;

                MinMaxCoordCheck.Check(newX, newY);
                NetworkModel.switchEntities.Add(switchEntity);
            }

            nodeList = xmlDoc.DocumentElement.SelectNodes("/NetworkModel/Lines/LineEntity");
            foreach (XmlNode node in nodeList)
            {
                LineEntity lineEntity = new LineEntity();

                lineEntity.Id = ulong.Parse(node.SelectSingleNode("Id").InnerText);
                lineEntity.Name = node.SelectSingleNode("Name").InnerText;
                if (node.SelectSingleNode("IsUnderground").InnerText.Equals("true"))
                {
                    lineEntity.IsUnderground = true;
                }
                else
                {
                    lineEntity.IsUnderground = false;
                }
                lineEntity.R = float.Parse(node.SelectSingleNode("R").InnerText);
                lineEntity.ConductorMaterial = node.SelectSingleNode("ConductorMaterial").InnerText;
                lineEntity.LineType = node.SelectSingleNode("LineType").InnerText;
                lineEntity.ThermalConstantHeat = long.Parse(node.SelectSingleNode("ThermalConstantHeat").InnerText);
                lineEntity.FirstEnd = ulong.Parse(node.SelectSingleNode("FirstEnd").InnerText);
                lineEntity.SecondEnd = ulong.Parse(node.SelectSingleNode("SecondEnd").InnerText);

                foreach (XmlNode pointNode in node.ChildNodes[9].ChildNodes) // 9 posto je Vertices 9. node u jednom line objektu
                {
                    Point point = new Point();

                    point.X = double.Parse(pointNode.SelectSingleNode("X").InnerText);
                    point.Y = double.Parse(pointNode.SelectSingleNode("Y").InnerText);

                    double newX, newY;
                    ToLatLonConverter.ToLatLon(point.X, point.Y, 34, out newX, out newY);
                    point.X = newX;
                    point.Y = newY;

                    //NetworkModel.vertices.Add(lineEntity);
                }
                NetworkModel.vertices.Add(lineEntity);
            }
        }
    }
}
