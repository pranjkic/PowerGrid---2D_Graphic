using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using PZ2.Repository;
using PZ2.Model;
using Point = System.Windows.Point;


namespace PZ2.Presentation
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Variables
        public List<Tuple<DrawingShape, DrawingShape>> allLinesForDraw = new List<Tuple<DrawingShape, DrawingShape>>();
        public List<Tuple<DrawingShape, DrawingShape>> notDrawnLines = new List<Tuple<DrawingShape, DrawingShape>>();
        public List<Tuple<DrawingShape, DrawingShape>> drawnLines = new List<Tuple<DrawingShape, DrawingShape>>();        
        public Dictionary<ulong, DrawingShape> allShapes = new Dictionary<ulong, DrawingShape>();
        public Dictionary<string, LineEntity> allLines = new Dictionary<string, LineEntity>();
        public List<Tuple<double, LineEntity>> sorted = new List<Tuple<double, LineEntity>>();
        public List<Tuple<DrawingShape, DrawingShape>> allLineSegments = new List<Tuple<DrawingShape, DrawingShape>>();
        const int rowDimension = 120, columnDimension = 235, rowCount = rowDimension + 1, columnCount = columnDimension + 1;
        int[,] grid = new int[rowCount, columnCount], bfsMatrix = new int[rowCount + 2, columnCount + 2];
        double scalingFactor = 6;
        public static int cntr = 0, bfscnt = 0;
        string array2 = "";
        #endregion

        public MainWindow()
        {
            InitializeComponent();

            NetworkModelRepository.LoadModel("Geographic.xml");
            TransformNetworkModelObjectsToShapes();            
            IsEmptyCheck();
            SortLineByDistance();
            InitializeMatrix();
            //Linije bez preseka iscrtane su pomocu BFS algoritma.
            StartDrawingLines();
            //Linije za koje putanja nije pronadjena, iscrtane su u drugom krugu.
            DrawTheRestLines();
            DrawIntersection();
            DrawShapes();
        }

        #region MainFunctions
        public void TransformNetworkModelObjectsToShapes()
        {
            foreach (SubstationEntity se in NetworkModel.substationEntities)
            {
                int scaledX = (int)(((se.X - MinMaxCoordCheck.minX) / (MinMaxCoordCheck.maxX - MinMaxCoordCheck.minX)) * columnDimension);
                int scaledY = rowDimension - (int)(((se.Y - MinMaxCoordCheck.minY) / (MinMaxCoordCheck.maxY - MinMaxCoordCheck.minY)) * rowDimension);

                Ellipse ellipse = new Ellipse() { Width = 5, Height = 5, Fill = (SolidColorBrush)new BrushConverter().ConvertFromString("Red"), Uid = se.Id.ToString() };
                Canvas.SetTop(ellipse, scaledY);
                Canvas.SetLeft(ellipse, scaledX);
                DrawingShape drawingShape = new DrawingShape() { Shape = ellipse, ShapeId = se.Id, X = se.X, Y = se.Y, Row = scaledY, Column = scaledX, ShapeName = se.Name, SwitchStatus = null };
                allShapes.Add(se.Id, drawingShape);
            }

            foreach (NodeEntity ne in NetworkModel.nodeEntities)
            {
                int scaledX = (int)(((ne.X - MinMaxCoordCheck.minX) / (MinMaxCoordCheck.maxX - MinMaxCoordCheck.minX)) * columnDimension);
                int scaledY = rowDimension - (int)(((ne.Y - MinMaxCoordCheck.minY) / (MinMaxCoordCheck.maxY - MinMaxCoordCheck.minY)) * rowDimension);

                Ellipse ellipse = new Ellipse() { Width = 5, Height = 5, Fill = (SolidColorBrush)new BrushConverter().ConvertFromString("Yellow"), Uid = ne.Id.ToString() };
                Canvas.SetTop(ellipse, scaledY);
                Canvas.SetLeft(ellipse, scaledX);
                DrawingShape drawingShape = new DrawingShape() { Shape = ellipse, ShapeId = ne.Id, X = ne.X, Y = ne.Y, Row = scaledY, Column = scaledX, ShapeName = ne.Name, SwitchStatus = null };
                allShapes.Add(ne.Id, drawingShape);
            }

            foreach (SwitchEntity se in NetworkModel.switchEntities)
            {
                int scaledX = (int)(((se.X - MinMaxCoordCheck.minX) / (MinMaxCoordCheck.maxX - MinMaxCoordCheck.minX)) * columnDimension);
                int scaledY = rowDimension - (int)(((se.Y - MinMaxCoordCheck.minY) / (MinMaxCoordCheck.maxY - MinMaxCoordCheck.minY)) * rowDimension);

                Ellipse ellipse = new Ellipse() { Width = 5, Height = 5, Fill = (SolidColorBrush)new BrushConverter().ConvertFromString("Green"), Uid = se.Id.ToString() };
                Canvas.SetTop(ellipse, scaledY);
                Canvas.SetLeft(ellipse, scaledX);
                DrawingShape drawingShape = new DrawingShape() { Shape = ellipse, ShapeId = se.Id, X = se.X, Y = se.Y, Row = scaledY, Column = scaledX, ShapeName = se.Name, SwitchStatus = se.Status };
                allShapes.Add(se.Id, drawingShape);
            }

        }

        public void IsEmptyCheck()
        {
            InitializeGrid();
            
            foreach (DrawingShape drawingShape in allShapes.Values)
            {
                switch (grid[drawingShape.Row, drawingShape.Column])
                {
                    case -1:
                        putShapeInEmptyPlace(drawingShape, 0, 0);
                        break;
                    case 1:
                        for (int i = 0; i < 15; i++)
                        {
                            int row = drawingShape.Row, column = drawingShape.Column;

                            try
                            {
                                //1
                                if (grid[drawingShape.Row - i, drawingShape.Column] == -1)
                                    putShapeInEmptyPlace(drawingShape, -i, 0);
                                //2
                                else if (grid[drawingShape.Row - i, drawingShape.Column - i] == -1)
                                    putShapeInEmptyPlace(drawingShape, -i, -i);
                                //3
                                else if (grid[drawingShape.Row, drawingShape.Column - i] == -1)
                                    putShapeInEmptyPlace(drawingShape, 0, -i);
                                //4
                                else if (grid[drawingShape.Row + i, drawingShape.Column - i] == -1)
                                    putShapeInEmptyPlace(drawingShape, i, -i);
                                //5
                                else if (grid[drawingShape.Row + i, drawingShape.Column] == -1)
                                    putShapeInEmptyPlace(drawingShape, i, 0);
                                //6
                                else if (grid[drawingShape.Row + i, drawingShape.Column + i] == -1)
                                    putShapeInEmptyPlace(drawingShape, i, i);
                                //7
                                else if (grid[drawingShape.Row, drawingShape.Column + i] == -1)
                                    putShapeInEmptyPlace(drawingShape, 0, i);
                                //8
                                else if (grid[drawingShape.Row - i, drawingShape.Column + i] == -1)                                
                                    putShapeInEmptyPlace(drawingShape, -i, i);                                
                                else
                                    continue;
                                break;
                            }
                            catch
                            {
                                try
                                {
                                    int type = RowColumnCheck(row, column);
                                    switch (type)
                                    {
                                        case 1:
                                            for (int j = 0; j < 10; j++)
                                            {
                                                if (grid[drawingShape.Row + i, drawingShape.Column] == -1)
                                                {
                                                    putShapeInEmptyPlace(drawingShape, i, 0);
                                                    break;
                                                }
                                                else if (grid[drawingShape.Row + i, drawingShape.Column + i] == -1)
                                                {
                                                    putShapeInEmptyPlace(drawingShape, i, i);
                                                    break;
                                                }
                                                else if (grid[drawingShape.Row, drawingShape.Column + i] == -1)
                                                {
                                                    putShapeInEmptyPlace(drawingShape, 0, i);
                                                    break;
                                                }
                                                else
                                                    continue;
                                            }
                                            break;
                                        case 2:
                                            for (int j = 0; j < 10; j++)
                                            {
                                                if (grid[drawingShape.Row - i, drawingShape.Column] == -1)
                                                {
                                                    putShapeInEmptyPlace(drawingShape, -i, 0);
                                                    break;
                                                }
                                                else if (grid[drawingShape.Row, drawingShape.Column + i] == -1)
                                                {
                                                    putShapeInEmptyPlace(drawingShape, 0, i);
                                                    break;
                                                }
                                                else if (grid[drawingShape.Row - i, drawingShape.Column + i] == -1)
                                                {
                                                    putShapeInEmptyPlace(drawingShape, -i, i);
                                                    break;
                                                }
                                                else
                                                    continue;
                                            }
                                            break;
                                        case 3:
                                            for (int j = 0; j < 10; j++)
                                            {
                                                if (grid[drawingShape.Row, drawingShape.Column - i] == -1)
                                                {
                                                    putShapeInEmptyPlace(drawingShape, 0, -i);
                                                    break;
                                                }
                                                else if (grid[drawingShape.Row + i, drawingShape.Column - i] == -1)
                                                {
                                                    putShapeInEmptyPlace(drawingShape, i, -i);
                                                    break;
                                                }
                                                else if (grid[drawingShape.Row + i, drawingShape.Column] == -1)
                                                {
                                                    putShapeInEmptyPlace(drawingShape, i, 0);
                                                    break;
                                                }
                                                else
                                                    continue;
                                            }
                                            break;
                                        case 4:
                                            for (int j = 0; j < 10; j++)
                                            {
                                                //1
                                                if (grid[drawingShape.Row - i, drawingShape.Column] == -1)
                                                {
                                                    putShapeInEmptyPlace(drawingShape, -i, 0);
                                                    break;
                                                }
                                                //2
                                                else if (grid[drawingShape.Row - i, drawingShape.Column - i] == -1)
                                                {
                                                    putShapeInEmptyPlace(drawingShape, -i, -i);
                                                    break;
                                                }
                                                //3
                                                else if (grid[drawingShape.Row, drawingShape.Column - i] == -1)
                                                {
                                                    putShapeInEmptyPlace(drawingShape, 0, -i);
                                                    break;
                                                }
                                                else
                                                    continue;
                                            }
                                            break;
                                        case 5:
                                            for (int j = 0; j < 10; j++)
                                            {
                                                //3
                                                if (grid[drawingShape.Row, drawingShape.Column - i] == -1)
                                                {
                                                    putShapeInEmptyPlace(drawingShape, 0, -i);
                                                    break;
                                                }
                                                //4
                                                else if (grid[drawingShape.Row + i, drawingShape.Column - i] == -1)
                                                {
                                                    putShapeInEmptyPlace(drawingShape, i, -i);
                                                    break;
                                                }
                                                //5
                                                else if (grid[drawingShape.Row + i, drawingShape.Column] == -1)
                                                {
                                                    putShapeInEmptyPlace(drawingShape, i, 0);
                                                    break;
                                                }
                                                //6
                                                else if (grid[drawingShape.Row + i, drawingShape.Column + i] == -1)
                                                {
                                                    putShapeInEmptyPlace(drawingShape, i, i);
                                                    break;
                                                }
                                                //7
                                                else if (grid[drawingShape.Row, drawingShape.Column + i] == -1)
                                                {
                                                    putShapeInEmptyPlace(drawingShape, 0, i);
                                                    break;
                                                }
                                                else
                                                    continue;
                                            }
                                            break;
                                        case 6:
                                            for (int j = 0; j < 10; j++)
                                            {
                                                //1
                                                if (grid[drawingShape.Row - i, drawingShape.Column] == -1)
                                                {
                                                    putShapeInEmptyPlace(drawingShape, -i, 0);
                                                    row = drawingShape.Row - i;
                                                    column = drawingShape.Column;
                                                }
                                                //2
                                                else if (grid[drawingShape.Row - i, drawingShape.Column - i] == -1)
                                                {
                                                    putShapeInEmptyPlace(drawingShape, -i, -i);
                                                    row = drawingShape.Row - i;
                                                    column = drawingShape.Column - i;
                                                }
                                                //3
                                                else if (grid[drawingShape.Row, drawingShape.Column - i] == -1)
                                                {
                                                    putShapeInEmptyPlace(drawingShape, 0, -i);
                                                    row = drawingShape.Row;
                                                    column = drawingShape.Column - i;
                                                }
                                                //7
                                                else if (grid[drawingShape.Row, drawingShape.Column + i] == -1)
                                                {
                                                    putShapeInEmptyPlace(drawingShape, 0, i);
                                                    row = drawingShape.Row;
                                                    column = drawingShape.Column + i;
                                                }
                                                //8
                                                else if (grid[drawingShape.Row - i, drawingShape.Column + i] == -1)
                                                {
                                                    putShapeInEmptyPlace(drawingShape, -i, i);
                                                    row = drawingShape.Row - i;
                                                    column = drawingShape.Column + i;
                                                }
                                                else
                                                    continue;
                                            }
                                            break;
                                        case 7:
                                            for (int j = 0; j < 10; j++)
                                            {
                                                //1
                                                if (grid[drawingShape.Row - i, drawingShape.Column] == -1)
                                                {
                                                    putShapeInEmptyPlace(drawingShape, -i, 0);
                                                    break;
                                                }
                                                //5
                                                else if (grid[drawingShape.Row + i, drawingShape.Column] == -1)
                                                {
                                                    putShapeInEmptyPlace(drawingShape, i, 0);
                                                    break;
                                                }
                                                //6
                                                else if (grid[drawingShape.Row + i, drawingShape.Column + i] == -1)
                                                {
                                                    putShapeInEmptyPlace(drawingShape, i, i);
                                                    break;
                                                }
                                                //7
                                                else if (grid[drawingShape.Row, drawingShape.Column + i] == -1)
                                                {
                                                    putShapeInEmptyPlace(drawingShape, 0, i);
                                                    break;
                                                }
                                                //8
                                                else if (grid[drawingShape.Row - i, drawingShape.Column + i] == -1)
                                                {
                                                    putShapeInEmptyPlace(drawingShape, -i, i);
                                                    break;
                                                }

                                                else
                                                    continue;
                                            }
                                            break;
                                        case 8:
                                            for (int j = 0; j < 10; j++)
                                            {
                                                //1
                                                if (grid[drawingShape.Row - i, drawingShape.Column] == -1)
                                                {
                                                    putShapeInEmptyPlace(drawingShape, -i, 0);
                                                    break;
                                                }
                                                //2
                                                else if (grid[drawingShape.Row - i, drawingShape.Column - i] == -1)
                                                {
                                                    putShapeInEmptyPlace(drawingShape, -i, -i);
                                                    break;
                                                }
                                                //3
                                                else if (grid[drawingShape.Row, drawingShape.Column - i] == -1)
                                                {
                                                    putShapeInEmptyPlace(drawingShape, 0, -i);
                                                    break;
                                                }
                                                //4
                                                else if (grid[drawingShape.Row + i, drawingShape.Column - i] == -1)
                                                {
                                                    putShapeInEmptyPlace(drawingShape, i, -i);
                                                    break;
                                                }
                                                //5
                                                else if (grid[drawingShape.Row + i, drawingShape.Column] == -1)
                                                {
                                                    putShapeInEmptyPlace(drawingShape, i, 0);
                                                    break;
                                                }
                                                else
                                                    continue;
                                            }
                                            break;
                                    }
                                }
                                catch { }
                            }
                        }
                        break;
                }
            }
        }

        public void SortLineByDistance()
        {

            DrawingShape first1 = null, first2 = null, second1 = null, second2 = null;
            Tuple<double, LineEntity> temp = new Tuple<double, LineEntity>(-1, null);

            foreach (LineEntity lineEntity in NetworkModel.vertices)
            {
                try
                {
                    var v1 = allShapes[lineEntity.FirstEnd];
                    var v2 = allShapes[lineEntity.SecondEnd];
                    sorted.Add(new Tuple<double, LineEntity>(-1, lineEntity));
                }
                catch { }
            }

            for (int i = 0; i < sorted.Count() - 1; i++)
            {
                first1 = allShapes[sorted[i].Item2.FirstEnd];
                first2 = allShapes[sorted[i].Item2.SecondEnd];
                double firstDistance = GetDistance(new Point(first1.Row, first1.Column), new Point(first2.Row, first2.Column));

                sorted[i] = new Tuple<double, LineEntity>(firstDistance, sorted[i].Item2);

                for (int j = i + 1; j < sorted.Count(); j++)
                {
                    second1 = allShapes[sorted[j].Item2.FirstEnd];
                    second2 = allShapes[sorted[j].Item2.SecondEnd];

                    double secondDistance = GetDistance(new Point(second1.Row, second1.Column), new Point(second2.Row, second2.Column));

                    sorted[j] = new Tuple<double, LineEntity>(secondDistance, sorted[j].Item2);

                    if (sorted[j].Item1 < sorted[i].Item1)
                    {
                        temp = sorted[i];
                        sorted[i] = sorted[j];
                        sorted[j] = temp;
                    }
                }
            }

            DrawingShape first = null, second = null;
            for (int i = 0; i < sorted.Count(); i++)
            {
                if (sorted[i].Item1 == 0)
                    continue;
                try
                {
                    first = allShapes[sorted[i].Item2.FirstEnd];
                    second = allShapes[sorted[i].Item2.SecondEnd];
                    first.LineId = sorted[i].Item2.Id;
                    second.LineId = sorted[i].Item2.Id;
                    first.LineName = sorted[i].Item2.Name;
                    second.LineName = sorted[i].Item2.Name;
                    CheckIfLineExists(first.ShapeId, second.ShapeId, first, second, allLinesForDraw);
                    allLinesForDraw.Add(new Tuple<DrawingShape, DrawingShape>(first, second));
                }
                catch { }
            }
        }
        
        public void InitializeMatrix()
        {
            for (int i = 0; i < rowCount + 2; i++)
            {
                for (int j = 0; j < columnCount + 2; j++)
                {
                    if (i == 0)
                    {
                        bfsMatrix[i, j] = 1;
                        continue;
                    }
                    else if (i == rowCount + 1)
                    {
                        bfsMatrix[i, j] = 1;
                        continue;
                    }
                    if (j == 0)
                    {
                        bfsMatrix[i, j] = 1;
                        continue;
                    }
                    else if (j == columnCount + 1)
                    {
                        bfsMatrix[i, j] = 1;
                        continue;
                    }
                    else
                    {
                        bfsMatrix[i, j] = -1;
                    }
                }
            }
        }

        public void StartDrawingLines()
        {
            for (int i = 0; i < allLinesForDraw.Count(); i++)
            {
                if (!BFS(allLinesForDraw[i].Item1, allLinesForDraw[i].Item2))
                {
                    cntr++;
                    notDrawnLines.Add(new Tuple<DrawingShape, DrawingShape>(allLinesForDraw[i].Item1, allLinesForDraw[i].Item2));
                }
            }
        }

        public bool BFS(DrawingShape start, DrawingShape end)
        {
            Node _root = new Node() { ShapeId = end.ShapeId, Row = start.Row + 1, Column = start.Column + 1, PrewiousColumn = -1, PrewiousRow = -1 };
            Node _required = new Node() { ShapeId = end.ShapeId, Row = end.Row, Column = end.Column };
            List<Node> allNodes = new List<Node>();
            List<Tuple<int, int>> allDotsForLine = new List<Tuple<int, int>>();
            bfscnt++;

            Queue<Node> _searchQueue = new Queue<Node>();

            Node _current = _root;
            allNodes.Add(_current);
            _searchQueue.Enqueue(_root);
            while (_searchQueue.Count() != 0)
            {
                _current = _searchQueue.Dequeue();
                if (_current.Row == (_required.Row + 1) && _current.Column == (_required.Column + 1))
                {
                    while (_current.Row != -1 && _current.Column != -1)
                    {
                        allDotsForLine.Add(new Tuple<int, int>(_current.Row - 1, _current.Column - 1));

                        bfsMatrix[_current.Row, _current.Column] = 1;
                        if (_current.PrewiousRow == -1 && _current.PrewiousRow == -1)
                        {
                            break;
                        }
                        foreach (Node node in allNodes)
                        {
                            if (node.Row == _current.PrewiousRow && node.Column == _current.PrewiousColumn)
                            {
                                _current = node;
                                break;
                            }
                        }
                    }
                    bfsMatrix[_current.Row, _current.Column] = 1;

                    string uid = Guid.NewGuid().ToString();
                    List<Line> allLiesForIteration = new List<Line>();
                    LineEntity lineEntity = new LineEntity() { FirstEnd = start.ShapeId, SecondEnd = end.ShapeId };
                    for (int i = 0; i < allDotsForLine.Count() - 1; i++)
                    {
                        Line line = new Line() { X1 = allDotsForLine[i].Item2 * scalingFactor + 2.5, Y1 = allDotsForLine[i].Item1 * scalingFactor + 2.5, X2 = allDotsForLine[i + 1].Item2 * scalingFactor + 2.5, Y2 = allDotsForLine[i + 1].Item1 * scalingFactor + 2.5, Stroke = new SolidColorBrush(Colors.Black), StrokeThickness = 1, Uid = uid };
                        line.ToolTip = "ID: " + start.LineId + "\nName: " + start.LineName;
                        allLiesForIteration.Add(line);

                        if (i == 0)
                            allLines.Add(uid, lineEntity);
                    }

                    Point startPoint = new Point(allLiesForIteration[0].X1, allLiesForIteration[0].Y1), endPoint = new Point(allLiesForIteration[0].X2, allLiesForIteration[0].Y2);
                    if (allLiesForIteration.Count > 1)
                    {
                        int i = 0;
                        int cnt = allLiesForIteration.Count;
                        while (cnt > 0)
                        {
                            if (i < allLiesForIteration.Count - 1)
                            {
                                if (allLiesForIteration[i].Y1 == allLiesForIteration[i + 1].Y2 || allLiesForIteration[i].X1 == allLiesForIteration[i + 1].X2)
                                {
                                    endPoint.X = allLiesForIteration[i + 1].X2;
                                    endPoint.Y = allLiesForIteration[i + 1].Y2;
                                    i++;
                                    cnt--;
                                    continue;
                                }
                                else
                                {
                                    Line line = new Line() { X1 = startPoint.X, Y1 = startPoint.Y, X2 = endPoint.X, Y2 = endPoint.Y, Stroke = new SolidColorBrush(Colors.Black), StrokeThickness = 1, Uid = uid };
                                    line.ToolTip = "ID: " + start.LineId + "\nName: " + start.LineName;
                                    myCanvas.Children.Add(line);
                                    allLineSegments.Add(new Tuple<DrawingShape, DrawingShape>(new DrawingShape() { Row = (int)line.X1, Column = (int)line.Y1 }, new DrawingShape() { Row = (int)line.X2, Column = (int)line.Y2 }));
                                    startPoint.X = allLiesForIteration[i + 1].X1;
                                    startPoint.Y = allLiesForIteration[i + 1].Y1;
                                    endPoint.X = allLiesForIteration[i + 1].X2;
                                    endPoint.Y = allLiesForIteration[i + 1].Y2;
                                }
                            }
                            else
                            {
                                endPoint.X = allLiesForIteration[i].X2;
                                endPoint.Y = allLiesForIteration[i].Y2;
                                Line line = new Line() { X1 = startPoint.X, Y1 = startPoint.Y, X2 = endPoint.X, Y2 = endPoint.Y, Stroke = new SolidColorBrush(Colors.Black), StrokeThickness = 1, Uid = uid };
                                line.ToolTip = "ID: " + start.LineId + "\nName: " + start.LineName;
                                myCanvas.Children.Add(line);
                                allLineSegments.Add(new Tuple<DrawingShape, DrawingShape>(new DrawingShape() { Row = (int)line.X1, Column = (int)line.Y1 }, new DrawingShape() { Row = (int)line.X2, Column = (int)line.Y2 }));
                            }

                            i++;
                            cnt--;
                        }
                    }
                    else
                    {
                        Line line = new Line() { X1 = startPoint.X, Y1 = startPoint.Y, X2 = endPoint.X, Y2 = endPoint.Y, Stroke = new SolidColorBrush(Colors.Black), StrokeThickness = 1, Uid = uid };
                        line.ToolTip = "ID: " + start.LineId + "\nName: " + start.LineName;
                        myCanvas.Children.Add(line);
                        allLineSegments.Add(new Tuple<DrawingShape, DrawingShape>(new DrawingShape() { Row = (int)line.X1, Column = (int)line.Y1 }, new DrawingShape() { Row = (int)line.X2, Column = (int)line.Y2 }));
                    }
                    for (int i = 1; i < rowCount + 2; i++)
                    {
                        for (int j = 1; j < columnCount + 2; j++)
                        {
                            if (bfsMatrix[i, j] == 2)
                            {
                                bfsMatrix[i, j] = -1;
                            }
                        }
                    }
                    drawnLines.Add(new Tuple<DrawingShape, DrawingShape>(start, end));
                    return true;
                }
                else
                {
                    if (bfsMatrix[_current.Row - 1, _current.Column] == -1)
                    {
                        Node above = new Node() { Row = _current.Row - 1, Column = _current.Column, PrewiousRow = _current.Row, PrewiousColumn = _current.Column };
                        _searchQueue.Enqueue(above);
                        allNodes.Add(above);
                        bfsMatrix[_current.Row - 1, _current.Column] = 2;
                    }
                    if (bfsMatrix[_current.Row + 1, _current.Column] == -1)
                    {
                        Node under = new Node() { Row = _current.Row + 1, Column = _current.Column, PrewiousRow = _current.Row, PrewiousColumn = _current.Column };
                        _searchQueue.Enqueue(under);
                        allNodes.Add(under);
                        bfsMatrix[_current.Row + 1, _current.Column] = 2;
                    }
                    if (bfsMatrix[_current.Row, _current.Column - 1] == -1)
                    {
                        Node left = new Node() { Row = _current.Row, Column = _current.Column - 1, PrewiousRow = _current.Row, PrewiousColumn = _current.Column };
                        _searchQueue.Enqueue(left);
                        allNodes.Add(left);
                        bfsMatrix[_current.Row, _current.Column - 1] = 2;
                    }
                    if (bfsMatrix[_current.Row, _current.Column + 1] == -1)
                    {
                        Node right = new Node() { Row = _current.Row, Column = _current.Column + 1, PrewiousRow = _current.Row, PrewiousColumn = _current.Column };
                        _searchQueue.Enqueue(right);
                        allNodes.Add(right);
                        bfsMatrix[right.Row, right.Column] = 2;
                    }
                    bfsMatrix[_current.Row, _current.Column] = 2;

                }
            }
            for (int i = 1; i < rowCount + 2; i++)
            {
                for (int j = 1; j < columnCount + 2; j++)
                {
                    if (bfsMatrix[i, j] == 2)
                    {
                        bfsMatrix[i, j] = -1;
                    }
                }
            }
            return false;
        }

        public void DrawTheRestLines()
        {
            foreach (Tuple<DrawingShape, DrawingShape> newLine in notDrawnLines)
            {
                if (newLine.Item1.Column == newLine.Item2.Column || newLine.Item1.Row == newLine.Item2.Row)
                {
                    Line line = new Line() { Stroke = new SolidColorBrush(Colors.Black), StrokeThickness = 1, X1 = newLine.Item1.Column * scalingFactor + 2.5, Y1 = newLine.Item1.Row * scalingFactor + 2.5, X2 = newLine.Item2.Column * scalingFactor + 2.5, Y2 = newLine.Item2.Row * scalingFactor + 2.5, Uid = Guid.NewGuid().ToString() };
                    drawnLines.Add(new Tuple<DrawingShape, DrawingShape>(newLine.Item1, newLine.Item2));
                    allLines.Add(line.Uid, new LineEntity() { FirstEnd = newLine.Item1.ShapeId, SecondEnd = newLine.Item2.ShapeId });
                    myCanvas.Children.Add(line);
                    allLineSegments.Add(new Tuple<DrawingShape, DrawingShape>(new DrawingShape() { Row = (int)line.X1, Column = (int)line.Y1 }, new DrawingShape() { Row = (int)line.X2, Column = (int)line.Y2 }));
                }
                else
                {
                    DrawingShape drawingShape = new DrawingShape() { Column = newLine.Item1.Column, Row = newLine.Item2.Row };
                    Line line1 = new Line() { Stroke = new SolidColorBrush(Colors.Black), StrokeThickness = 1, X1 = newLine.Item1.Column * scalingFactor + 2.5, Y1 = newLine.Item1.Row * scalingFactor + 2.5, X2 = newLine.Item1.Column * scalingFactor + 2.5, Y2 = newLine.Item2.Row * scalingFactor + 2.5, Uid = Guid.NewGuid().ToString() };
                    Line line2 = new Line() { Stroke = new SolidColorBrush(Colors.Black), StrokeThickness = 1, X1 = newLine.Item1.Column * scalingFactor + 2.5, Y1 = newLine.Item2.Row * scalingFactor + 2.5, X2 = newLine.Item2.Column * scalingFactor + 2.5, Y2 = newLine.Item2.Row * scalingFactor + 2.5, Uid = line1.Uid };
                    drawnLines.Add(new Tuple<DrawingShape, DrawingShape>(newLine.Item1, newLine.Item2));
                    allLines.Add(line1.Uid, new LineEntity() { FirstEnd = newLine.Item1.ShapeId, SecondEnd = newLine.Item2.ShapeId });
                    myCanvas.Children.Add(line1);
                    myCanvas.Children.Add(line2);
                    allLineSegments.Add(new Tuple<DrawingShape, DrawingShape>(new DrawingShape() { Row = (int)line1.X1, Column = (int)line1.Y1 }, new DrawingShape() { Row = (int)line1.X2, Column = (int)line1.Y2 }));
                    allLineSegments.Add(new Tuple<DrawingShape, DrawingShape>(new DrawingShape() { Row = (int)line2.X1, Column = (int)line2.Y1 }, new DrawingShape() { Row = (int)line2.X2, Column = (int)line2.Y2 }));
                }
            }
        }

        public void DrawShapes()
        {
            foreach (DrawingShape drawingShape in allShapes.Values)
            {
                Ellipse ellipse = drawingShape.Shape as Ellipse;
                ellipse.ToolTip = "ID: " + drawingShape.ShapeId + "\nName: " + drawingShape.ShapeName;
                if (drawingShape.SwitchStatus != null)
                    ellipse.ToolTip += "\nStatus: " + drawingShape.SwitchStatus;
                Canvas.SetTop(ellipse, drawingShape.Row * scalingFactor);
                Canvas.SetLeft(ellipse, drawingShape.Column * scalingFactor);
                myCanvas.Children.Add(ellipse);
            }
        }

        public void DrawIntersection()
        {
            for (int i = 0; i < allLineSegments.Count; i++)
            {
                for (int j = 0; j < allLineSegments.Count; j++)
                {
                    if (allLineSegments[i].Item1.Column == allLineSegments[i].Item2.Column)
                    {
                        if (allLineSegments[j].Item1.Row == allLineSegments[j].Item2.Row)
                        {
                            if ((allLineSegments[j].Item1.Column < allLineSegments[i].Item1.Column && allLineSegments[j].Item2.Column > allLineSegments[i].Item1.Column) ||
                               (allLineSegments[j].Item1.Column > allLineSegments[i].Item1.Column && allLineSegments[j].Item2.Column < allLineSegments[i].Item1.Column))
                            {
                                if ((allLineSegments[i].Item1.Row < allLineSegments[j].Item1.Row && allLineSegments[i].Item2.Row > allLineSegments[j].Item1.Row) ||
                                    (allLineSegments[i].Item1.Row > allLineSegments[j].Item1.Row && allLineSegments[i].Item2.Row < allLineSegments[j].Item1.Row))
                                {
                                    Rectangle r = new Rectangle() { Height = 1, Width = 1, Fill = new SolidColorBrush(Colors.Purple) };
                                    Canvas.SetTop(r, allLineSegments[i].Item1.Column);
                                    Canvas.SetLeft(r, allLineSegments[j].Item1.Row);
                                    myCanvas.Children.Add(r);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void MyCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            List<UIElement> all = new List<UIElement>();

            var mouseWasDownOn = e.Source as FrameworkElement;
            if (mouseWasDownOn.GetType().Name == "Ellipse")
            {
                UIElement selectedElement = mouseWasDownOn as UIElement;

                foreach (UIElement shape in myCanvas.Children)
                {
                    if (shape.Uid == selectedElement.Uid)
                        all.Add(selectedElement);
                }

                if (all.Count == 1)
                {
                    Shape selectedShape = selectedElement as Shape;
                    DrawingShape selectedDrawingShape = allShapes[ulong.Parse(selectedShape.Uid)];

                    Ellipse el = new Ellipse() { Height = 50, Width = 50, Fill = selectedShape.Fill, Uid = selectedShape.Uid, ToolTip = selectedShape.ToolTip };
                    Canvas.SetTop(el, selectedDrawingShape.Row * scalingFactor - 22.5);
                    Canvas.SetLeft(el, selectedDrawingShape.Column * scalingFactor - 22.5);
                    myCanvas.Children.Add(el);
                    return;
                }
                else
                {
                    foreach (UIElement uie in all)
                    {
                        if ((uie as Shape).Height == 50)
                        {
                            myCanvas.Children.Remove(uie);
                            return;
                        }
                    }
                }
            }
        }

        private void MyCanvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var mouseWasDownOn = e.Source as FrameworkElement;
            if (mouseWasDownOn.GetType().Name == "Line")
            {
                Line line = mouseWasDownOn as Line;
                DrawingShape drawingShape1 = allShapes[allLines[line.Uid].FirstEnd];
                DrawingShape drawingShape2 = allShapes[allLines[line.Uid].SecondEnd];

                foreach (Shape s in myCanvas.Children)
                {
                    if (s.Uid == drawingShape1.ShapeId.ToString())
                    {
                        s.Fill = new SolidColorBrush(Colors.Blue);
                    }
                    if (s.Uid == drawingShape2.ShapeId.ToString())
                    {
                        s.Fill = new SolidColorBrush(Colors.Blue);
                    }
                }
            }
        }
        #endregion

        #region HelpFunctions
        public static double GetDistance(Point point1, Point point2)
        {
            var xDist = point1.X - point2.X;
            var yDist = point1.Y - point2.Y;
            var nDist = Math.Sqrt(xDist * xDist + yDist * yDist);
            return nDist;
        }
                     
        public void InitializeGrid()
        {
            for (int i = 0; i < rowCount; i++)
            {
                for (int j = 0; j < columnCount; j++)
                {
                    grid[i, j] = -1;
                }
            }
        }

        public int RowColumnCheck(int row, int column)
        {
            if (row < 0 && column < 0)
                return 1;
            else if (row > columnCount && column > 0)
                return 2;
            else if (row < 0 && column > columnCount)
                return 3;
            else if (row > columnCount && column > columnCount)
                return 4;
            else if (row < 0)
                return 5;
            else if (row > columnCount)
                return 6;
            else if (column < 0)
                return 7;
            else if (column > columnCount)
                return 8;
            return 0;
        }
           
        public void CheckIfLineExists(ulong first, ulong second, DrawingShape shape1, DrawingShape shape2, List<Tuple<DrawingShape, DrawingShape>> list)
        {
            if (list.Count == 0)
            {
                return;
            }
            foreach (Tuple<DrawingShape, DrawingShape> line in list)
            {
                if (line.Item1.ShapeId == first && line.Item2.ShapeId == second || line.Item1.ShapeId == second && line.Item2.ShapeId == first)
                    throw new Exception();
            }
        }
         
        public void putShapeInEmptyPlace(DrawingShape drawingShape, int row, int column)
        {
            grid[drawingShape.Row + row, drawingShape.Column + column] = 1;
            allShapes[drawingShape.ShapeId].Row = drawingShape.Row+row;
            allShapes[drawingShape.ShapeId].Column = drawingShape.Column+column;
        }
        #endregion
                
        #region Scroll

        private void scrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            Point mouseAtImage = e.GetPosition(myCanvas);
            Point mouseAtScrollViewer = e.GetPosition(scrollViewer);

            ScaleTransform st = myCanvas.LayoutTransform as ScaleTransform;
            if (st == null)
            {
                st = new ScaleTransform();
                myCanvas.LayoutTransform = st;
            }

            if (e.Delta > 0)
            {
                st.ScaleX = st.ScaleY = st.ScaleX * 1.25;
                if (st.ScaleX > 64) st.ScaleX = st.ScaleY = 64;
            }
            else
            {
                st.ScaleX = st.ScaleY = st.ScaleX / 1.25;
                if (st.ScaleX < 1) st.ScaleX = st.ScaleY = 1;
            }
            #region [this step is critical for offset]
            scrollViewer.ScrollToHorizontalOffset(0);
            scrollViewer.ScrollToVerticalOffset(0);
            this.UpdateLayout();
            #endregion

            Vector offset = myCanvas.TranslatePoint(mouseAtImage, scrollViewer) - mouseAtScrollViewer;
            scrollViewer.ScrollToHorizontalOffset(offset.X);
            scrollViewer.ScrollToVerticalOffset(offset.Y);
            this.UpdateLayout();

            e.Handled = true;
        }
        #endregion
        
    }
}
