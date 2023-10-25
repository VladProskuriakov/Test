using System;
using System.Collections.Generic;
using System.IO;

namespace Паросочетания
{
    class NonExistentVertexException : Exception
    {
        public NonExistentVertexException(string msg) : base(msg) { }
    }
    class NonExistentEdgeException : Exception
    {
        public NonExistentEdgeException(string msg) : base(msg) { }
    }

    class AddingExistingEdge : Exception
    {
        public AddingExistingEdge(string msg) : base(msg) { }
    }

    class AddingExistingVertex : Exception
    {
        public AddingExistingVertex(string msg) : base(msg) { }
    }

    class InvalidInputException : Exception
    {
        public InvalidInputException(string msg) : base(msg) { }
    }

    class NonBipartiteGraphException : Exception
    {
        public NonBipartiteGraphException(string msg) : base(msg) { }
    }

    class NonCompleteBipartiteGraphException : Exception
    {
        public NonCompleteBipartiteGraphException(string msg) : base(msg) { }
    }

    class Graph
    {
        bool orint; 
        List<Vertex> vertices; 

        public Graph()
        {
            orint = false;
            vertices = new List<Vertex>();
        }
        public Graph(int numOfVertex, bool _d)
        {
            vertices = new List<Vertex>();
            orint = _d;            
            for (int i = 0; i < numOfVertex; i++)
                AddVertex(i + 1);
        }
        
        public Graph(bool _d, List<Vertex> _vertices)
        {
            orint = _d;            
            vertices = new List<Vertex>();
            foreach (Vertex vertex in _vertices)
                AddVertex(vertex.num);           
            for (int i = 0; i < _vertices.Count; i++)
            {
                for (int j = i + 1; j < _vertices.Count; j++)
                {
                    Edge findedEdge = _vertices[i].findEdge(_vertices[j]);
                    if (findedEdge != null)
                        AddEdge(findedEdge.start.num, findedEdge.end.num, findedEdge.data);
                }
            }
        }

        public Graph(StreamReader f, int typeOfInput)
        {
            vertices = new List<Vertex>();            
            if (typeOfInput == 1) 
            {
                orint = false;
                ReadMatrix(f);
            }
            else 
                Read_edges(f);

            f.Close();
        }
                
        private void ReadMatrix(StreamReader f)
        {
            int N, M;             
            string[] size = f.ReadLine().Trim().Split(" ");
            if (!int.TryParse(size[0], out N) || !int.TryParse(size[1], out M)) throw new InvalidInputException("Ошибка | Некорректные входные данные");            
            for (int i = 0; i < N + M; i++)
                AddVertex(i + 1);            
            int[,] matrix = new int[N, M];
            for (int i = 0; i < N; i++)
            {
                string[] row = f.ReadLine().Trim().Split(" ");
                for (int j = 0; j < M; j++)
                {
                    int weight;
                    if (row.Length < j || !int.TryParse(row[j], out weight)) throw new InvalidInputException("Ошибка | Некорректные входные данные");
                    matrix[i, j] = weight;
                }
            }            
            for (int i = 0; i < N; i++)
            {
                for (int j = 0; j < M; j++)
                {
                    if (matrix[i, j] > 0)
                        AddEdge(i + 1, N + j + 1, matrix[i, j]);
                }
            }
        }

        
        private void Read_edges(StreamReader f)
        {            
            string[] graphParams = f.ReadLine().Trim().Split(" ");
            orint = graphParams[1] == "1" ? true : false;
            
            for (int i = 0; i < int.Parse(graphParams[0]); i++)
                AddVertex(i + 1);
            
            while (!f.EndOfStream)
            {
                string[] edge = f.ReadLine().Split(" ");
                AddEdge(int.Parse(edge[0]), int.Parse(edge[1]), int.Parse(edge[2]));
            }
        }

        /* Проверка двудольности */
        public bool GetGraphShares(ref List<Vertex> V1, ref List<Vertex> V2)
        {
            V1.Add(vertices[0]);
            foreach (Vertex vertex in vertices)
            {
                if (!V1.Contains(vertex) && !V2.Contains(vertex))
                {
                    if (!V1.Contains(vertex))
                        V1.Add(vertex);
                    else
                        V2.Add(vertex);
                }
                                
                if (vertex._edges.Count <= 0)
                    throw new NonBipartiteGraphException("Ошибка | Граф не является двудольным");

                foreach (Edge edge in vertex._edges)
                {                    
                    if (V1.Contains(edge.end) || V2.Contains(edge.end))
                    {                        
                        if (V1.Contains(vertex) && V1.Contains(edge.end) || V2.Contains(vertex) && V2.Contains(edge.end))
                            throw new NonBipartiteGraphException("Ошибка | Граф не является двудольным");
                    }
                    else 
                    {
                        if (V1.Contains(vertex))
                            V2.Add(edge.end);
                        else
                            V1.Add(edge.end);
                    }
                }
            }
            return true;
        }

        /* Жадный алгоритм */
        public List<Edge> SearchFirstMatching(List<Vertex> V1)
        {
            List<Vertex> usedVertex = new List<Vertex>();
            List<Edge> matching = new List<Edge>();
            foreach (var vertex in V1)
            {
                foreach (var edge in vertex._edges)
                    if (!usedVertex.Contains(edge.start) && !usedVertex.Contains(edge.end))
                    {
                        usedVertex.Add(edge.start);
                        usedVertex.Add(edge.end);
                        matching.Add(edge);
                    }
            }
            return matching;
        }
        
        /* Максимальное паросочетание (Волновой метод) */
        public List<Edge> SearchMax(ref List<Vertex> markedVertex)
        {            
            List<Vertex> V1 = new List<Vertex>();
            List<Vertex> V2 = new List<Vertex>();            
            GetGraphShares(ref V1, ref V2);            
            List<Edge> matching = SearchFirstMatching(V1);
                        
            bool unpairedVertex = true; // Непарная вершина
            while (matching.Count < V1.Count && unpairedVertex)
            {
                List<Vertex> queue = new List<Vertex>(); 
                markedVertex.Clear(); 
                List<Edge> chain = new List<Edge>(); 
                /* Поиск непарныя вершин */
                foreach (var vertex in V1)
                {
                    if (matching.Find(e => e.start == vertex) == null)
                    {
                        markedVertex.Add(vertex);
                        queue.Add(vertex);
                    }
                }
                while (queue.Count > 0)
                {
                    Vertex curVertex = queue[0]; 
                    queue.RemoveAt(0);                     
                    if (V1.Contains(curVertex))
                    {
                        foreach (var edge in curVertex._edges)
                        {
                            /* если ребро не входит в паросочетание и ведёт к неотмеченной вершине */
                            if (!matching.Contains(edge) && !markedVertex.Contains(edge.end))
                            {
                                chain.Add(edge);
                                queue.Add(edge.end);
                                markedVertex.Add(edge.end);
                            }
                        }
                    }                    
                    else
                    {
                        unpairedVertex = true;
                        foreach (var edge in curVertex._edges)
                        {
                            /* если ребро входит в паросочетание и ведёт к неотмеченной вершине */
                            if (matching.Find(e => e.start == edge.end && e.end == edge.start) != null && !markedVertex.Contains(edge.end))
                            {
                                unpairedVertex = false;
                                chain.Add(edge);
                                queue.Add(edge.end);
                                markedVertex.Add(edge.end);
                            }
                        }
                        /* если мы попали в непарную вершину, то найдена чередующаяся цепь */
                        if (unpairedVertex)
                        {
                            /* увеличиваем паросочетание вдоль цепи */
                            while (curVertex != null)
                            {
                                Edge edge = chain.Find(e => e.end == curVertex);
                                if (edge != null)
                                {
                                    Edge foundEdge = matching.Find(e => e == edge || e.start == edge.end && e.end == edge.start);
                                    if (foundEdge != null)
                                        matching.Remove(foundEdge);
                                    else
                                        matching.Add(edge);
                                    curVertex = edge.start;
                                    chain.Remove(edge);
                                }
                                else
                                    curVertex = null;
                            }
                            break;
                        }
                    }
                }
            }
            return matching;
        }
        
        /* Вывод паросочетания */
        public string PrintMatching(List<Edge> matching)
        {
            string matching_str = "M = { "; 
            foreach (Edge edge in matching)
                matching_str += "(" + edge.start.num + "," + edge.end.num + ")" + " ";
            matching_str += "}";
            return matching_str;
        }

        /* Вывод максимального паросочетания минимальной стоимости */
        public string PrintMaxMin(List<Edge> matching)
        {
            string matching_str = "M = { "; 
            string cost_str = "Сумма стоимостей рёбер, входящих в паросочетание:  "; 
            int cost = 0; 
            for (int i = 0; i < vertices.Count; i++)
            {
                for (int j = i + 1; j < vertices.Count; j++)
                {
                    Edge edge = vertices[i].findEdge(vertices[j]);
                    if (edge != null && matching.Find(e => e.start.num == edge.start.num && e.end.num == edge.end.num) != null)
                    {
                        matching_str += "(" + edge.start.num + "," + edge.end.num + ")" + " ";
                        cost_str += edge.data + " + ";
                        cost += edge.data;
                    }
                }
            }
            matching_str += "}";
            cost_str = cost_str.Substring(0, cost_str.Length - 2) + "= ";
            return matching_str + "\n" + cost_str + cost.ToString();
        }

        /* найти максимальное паросочетание минимальной стоимости */
        public List<Edge> SearchMaxMin()
        {            
            Graph graphCopy = new Graph(orint, vertices); //копия графа
           
            List<Vertex> V1 = new List<Vertex>();
            List<Vertex> V2 = new List<Vertex>();
            graphCopy.GetGraphShares(ref V1, ref V2);

            /* Проверка что граф является полным двудольным */
            foreach (Vertex v1 in V1)
                if (v1._edges.Count != V2.Count)
                    throw new NonCompleteBipartiteGraphException("Ошибка | Граф не является полным двудольным взвешенным графом");

            /* Горизонтальная редукция */
            foreach (Vertex v1 in V1)
            {
                int minCost = -1;
                Edge e1, e2;                
                foreach (Vertex v2 in V2)
                {
                    e1 = v1.findEdge(v2);
                    if (e1.data < minCost || minCost == -1)
                        minCost = e1.data;
                }                
                foreach (Vertex v2 in V2)
                {
                    e1 = v1.findEdge(v2);
                    e2 = v2.findEdge(v1);
                    e1.data = e2.data -= minCost;
                }
            }

            /* Проверка что у каждой вершины есть хотя бы одно отсутствующее ребро */
            List<Vertex> vertexForRedux = new List<Vertex>();
            foreach (Vertex vertex in V2)
            {
                if (vertex._edges.Find(e => e.data == 0) == null)
                    vertexForRedux.Add(vertex);
            }

            /* Вертикальная редукция */
            if (vertexForRedux.Count > 0)
            {
                foreach (Vertex v2 in vertexForRedux)
                {
                    int minCost = -1;
                    Edge e1, e2;
                    foreach (Vertex v1 in V1)
                    {
                        e2 = v2.findEdge(v1);
                        if (e2.data < minCost || minCost == -1)
                            minCost = e2.data;
                    }
                    foreach (Vertex v1 in V1)
                    {
                        e2 = v2.findEdge(v1);
                        e1 = v1.findEdge(v2);
                        e2.data = e1.data -= minCost;
                    }
                }
            }

            
            List<Edge> matching = new List<Edge>();            
            while (matching.Count < V1.Count)
            {
                /* Составление двудолного графа из нулей в матрице */
                Graph transformedGraph = new Graph(vertices.Count, false);                
                foreach (Vertex v1 in V1)
                {
                    foreach (Vertex v2 in V2)
                    {
                        Edge edge = v1.findEdge(v2);
                        if (edge.data == 0)
                            transformedGraph.AddEdge(v1.num, v2.num, edge.data);
                    }
                }
                
                List<Vertex> markedVertex = new List<Vertex>(); // отмеченные вершины, в ходе построения макс.паросочетания
                matching = transformedGraph.SearchMax(ref markedVertex);
                
                List<Vertex> markedVertexV1 = new List<Vertex>(); 
                List<Vertex> markedVertexV2 = new List<Vertex>(); 
                foreach (Vertex vertex in markedVertex)
                {
                    Vertex findedVertexV1 = V1.Find(v => v.num == vertex.num);
                    if (findedVertexV1 != null)
                        markedVertexV1.Add(findedVertexV1);
                    else
                        markedVertexV2.Add(V2.Find(v => v.num == vertex.num));
                }

                /* Диагональная редукция */
                int d = -1; 
                foreach (Vertex v1 in markedVertexV1)
                {
                    foreach (Vertex v2 in V2)
                    {
                        if (markedVertexV2.Contains(v2)) continue;
                        Edge e1 = v1.findEdge(v2);
                        if (e1.data < d || d == -1)
                            d = e1.data;
                    }
                }
                
                foreach (Vertex v1 in markedVertexV1)
                {
                    foreach (Vertex v2 in V2)
                    {
                        if (!markedVertexV2.Contains(v2))
                        {
                            Edge e1 = v1.findEdge(v2);
                            Edge e2 = v2.findEdge(v1);
                            e1.data = e2.data -= d;
                        }
                    }
                }
                
                foreach (Vertex v2 in markedVertexV2)
                {
                    foreach (Vertex v1 in V1)
                    {
                        if (!markedVertexV1.Contains(v1))
                        {
                            Edge e1 = v1.findEdge(v2);
                            Edge e2 = v2.findEdge(v1);
                            e1.data = e2.data += d;
                        }
                    }
                }
            }
            return matching;
        }

        
        public void AddVertex(int numOfVertex)
        {            
            if (numOfVertex <= 0) throw new InvalidInputException("Ошибка | Номер вершины не может быть равен нулю и содержать любые другие символы, кроме цифр");            
            if (vertices.Find(v => v.num == numOfVertex) != null)
                throw new AddingExistingVertex("Ошибка | Попытка добавить в граф существующую вершину");
            Vertex newVertex = new Vertex(numOfVertex);
            vertices.Add(newVertex);
        }
        
        public void AddEdge(int start, int end, int data)
        {            
            if (start <= 0 || end <= 0) throw new InvalidInputException("Ошибка | Номера вершин не могут быть равными нулю и содержать любые другие символы, кроме цифр");            
            if (data < 0) throw new InvalidInputException("Ошибка | Вес ребра должен быть неотрицательным числом");
            Vertex v1 = vertices.Find(v => v.num == start);
            Vertex v2 = vertices.Find(v => v.num == end);            
            if (v1 == null || v2 == null) throw new NonExistentVertexException("Ошибка | Одна из указанных вершин не принадлежит графу");
            v1.addEdge(data, v1, v2);
            if (!orint) v2.addEdge(data, v2, v1);
        }
        
        public void DelVertex(int numOfVertex)
        {            
            if (numOfVertex <= 0) throw new InvalidInputException("Ошибка | Номера вершин не могут быть равными нулю и содержать любые другие символы, кроме цифр");
            Vertex del_v = vertices.Find(del_v => del_v.num == numOfVertex);            
            if (del_v == null) throw new NonExistentVertexException("Ошибка | Попытка удалить вершину, не принадлежающую графу");            
            foreach (var vertex in vertices)
                if (vertex.findEdge(del_v) != null)
                    vertex.delEdge(del_v); ;            
            vertices.Remove(del_v);
        }
        
        public void DelEdge(int start, int end)
        {            
            if (start <= 0 || end <= 0)
                throw new InvalidInputException("Ошибка | Номера вершин не могут быть равными нулю и содержать любые другие символы, кроме цифр");
            Vertex v1 = vertices.Find(v => v.num == start);
            Vertex v2 = vertices.Find(v => v.num == end);            
            if (v1 == null || v2 == null)
                throw new NonExistentVertexException("Ошибка | Одна из указанных вершин не принадлежит графу");            
            v1.delEdge(v2);            
            if (!orint)
                v2.delEdge(v1);
        }

        /* перечисление вершин и перечисление рёбер */
        public void Show1()
        {          
            Console.WriteLine("V: ");
            foreach (var v in vertices)
                Console.Write(v.num + " ");
            
            Console.WriteLine("\nE: ");
            foreach (var v in vertices)
            {
                string temp = v.printEdge1();
                Console.WriteLine(temp);
            }

        }

        /* вывод в виде матрицы смежности */
        public void Show2()
        {
            for (int i = 0; i < vertices.Count; i++)
            {
                foreach (var vertex in vertices)
                {
                    Edge edge = vertices[i].findEdge(vertex);
                    if (edge == null)
                        Console.Write("{0,5}", 0 + " ");
                    else
                        Console.Write("{0,5}", edge.data + " ");
                }
                Console.WriteLine();
            }
        }

        /* вывод в виде списков смежности */
        public void Show3()
        {
            foreach (var v in vertices)
            {
                Console.Write(v.num + " ");
                string temp = v.printEdge2();
                Console.WriteLine(temp);                
            }
            Console.WriteLine();
        }

        /* вывод матрицы весов двудольного графа */
        public void Show4()
        {
            List<Vertex> V1 = new List<Vertex>();
            List<Vertex> V2 = new List<Vertex>();            
            GetGraphShares(ref V1, ref V2);            
            for (int i = 0; i < V2.Count - 1; i++)
            {
                for (int j = 0; j < V2.Count - i - 1; j++)
                {
                    if (V2[j].num > V2[j + 1].num)
                    {
                        Vertex temp = V2[j];
                        V2[j] = V2[j + 1];
                        V2[j + 1] = temp;
                    }
                }
            }            
            foreach (var vertexV1 in V1)
            {
                foreach (var vertexV2 in V2)
                {
                    Edge edge = vertexV1.findEdge(vertexV2);
                    if (edge != null)
                        Console.Write("{0,2}", edge.data + " ");
                    else
                        Console.Write("{0,2}", 0 + " ");

                }
                Console.WriteLine();
            }
        }
    }
    class Vertex
    {
        public int num; 
        public List<Edge> _edges; 

        public Vertex(int _num)
        {
            num = _num;
            _edges = new List<Edge>();
        }

        public Edge findEdge(Vertex v2)
        {
            foreach (Edge edge in _edges)
                if (edge.end == v2) return edge;
            return null;
        }

        public void addEdge(int data, Vertex v1, Vertex v2)
        {                     
            if (_edges.Find(e => e.end == v2) != null) throw new AddingExistingEdge("Ошибка | Попытка добавить в граф уже существующее ребро");
            Edge newEdge = new Edge(data, v1, v2);
            _edges.Add(newEdge);
        }
        
        public void delEdge(Vertex delV)
        {            
            if (findEdge(delV) == null) throw new NonExistentEdgeException("Ошибка | Попытка удалить ребро, не принадлежащее графу");            
            _edges.RemoveAll(edge => edge.end == delV);
        }

        /* Вывод рёбер */
        public string printEdge1()
        {
            string temp = "";
            foreach (Edge edge in _edges)
            {
                temp += Convert.ToString(num) + " " + Convert.ToString(edge.end.num) + " " + Convert.ToString(edge.data) + "\n";
            }
             return temp;     
        }

        public string printEdge2()
        {
            string temp = "";
            foreach (Edge edge in _edges)
            {
                temp += Convert.ToString(edge.end.num) + "(" + Convert.ToString(edge.data) + ")" + "\n";
            }
            return temp;                    
        }

    }
    class Edge
    {
        public int data; 
        public Vertex start; 
        public Vertex end; 

        public Edge(int _data, Vertex _start, Vertex _end)
        {
            data = _data;
            start = _start;
            end = _end;
        }
    }

    class Program
    {
        public static void Main()
        {
            Graph myGraph = null;
            string option; 

            /* ввод графа */
            do
            {
                Console.WriteLine("\n1 - Ввод данных из файла\n2 - Ручной ввод нового графа");
                Console.Write("\nВыберите способ ввода графа: ");
                option = Console.ReadLine();
                switch (option)
                {
                    case "0":
                        break;
                    case "1":
                        int typeOfInput;
                        Console.Write("Выберите в каком виде хранится граф в текстовом файле (1 - матрица двудольного графа, 2 - список рёбер): ");
                        int.TryParse(Console.ReadLine(), out typeOfInput);
                        try
                        {
                            myGraph = new Graph(new StreamReader("input.txt"), typeOfInput);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                            return;
                        }
                        break;
                    case "2":
                        Console.Write("Введите число вершин в графе: ");
                        int numOfVertex;
                        int.TryParse(Console.ReadLine(), out numOfVertex);
                        Console.Write("Является ли граф ориентированным ('0' - нет / '1' - да): ");
                        bool orint = false;
                        if (Console.ReadLine() == "1")
                            orint = true;
                        myGraph = new Graph(numOfVertex, orint);
                        break;
                    default:
                        Console.WriteLine("Неизвестное действие");
                        break;
                }
            }
            while (option != "0" && option != "1" && option != "2");

            /* Операции над графом */
            while (option != "0")
            {
                Console.WriteLine("\n1 - Добавить вершину\n2 - Удалить вершину\n3 - Добавить ребро\n4 - Удалить ребро\n5 - Вывести граф\n" +
                "6 - Найти max паросочетание\n7 - Найти max паросочетание min стоимости ");
                Console.Write("__________________\nВыберите действие: ");
                option = Console.ReadLine();
                switch (option)
                {
                    case "1":
                        {
                            int numOfVertex;
                            Console.Write("Введите номер вершины: ");
                            int.TryParse(Console.ReadLine(), out numOfVertex);
                            try
                            {
                                myGraph.AddVertex(numOfVertex);
                            }
                            catch (AddingExistingVertex ex)
                            {
                                Console.WriteLine(ex.Message);
                                break;
                            }
                            catch (InvalidInputException ex)
                            {
                                Console.WriteLine(ex.Message);
                                break;
                            }
                            Console.WriteLine("Вершина '" + numOfVertex + "' добавлена в граф");
                            break;
                        }
                    case "2":
                        {
                            int numOfVertex;
                            Console.Write("Введите номер вершины: ");
                            int.TryParse(Console.ReadLine(), out numOfVertex);
                            try
                            {
                                myGraph.DelVertex(numOfVertex);
                            }
                            catch (NonExistentVertexException ex)
                            {
                                Console.WriteLine(ex.Message);
                                break;
                            }
                            catch (InvalidInputException ex)
                            {
                                Console.WriteLine(ex.Message);
                                break;
                            }
                            Console.WriteLine("Вершина '" + numOfVertex + "' удалена из графа");
                            break;
                        }
                    case "3":
                        {
                            int start, end, weight;
                            Console.Write("Введите начальную вершину ребра: ");
                            int.TryParse(Console.ReadLine(), out start);
                            Console.Write("Введите конечную вершину ребра: ");
                            int.TryParse(Console.ReadLine(), out end);
                            Console.Write("Введите вес ребра: ");
                            int.TryParse(Console.ReadLine(), out weight);
                            try
                            {
                                myGraph.AddEdge(start, end, weight);
                            }
                            catch (AddingExistingEdge ex)
                            {
                                Console.WriteLine(ex.Message);
                                break;
                            }
                            catch (NonExistentVertexException ex)
                            {
                                Console.WriteLine(ex.Message);
                                break;
                            }
                            catch (InvalidInputException ex)
                            {
                                Console.WriteLine(ex.Message);
                                break;
                            }
                            Console.WriteLine("Ребро " + "(" + start + "," + end + ") добавлено в граф");
                            break;
                        }
                    case "4":
                        {
                            int start, end;
                            Console.Write("Введите начальную вершину ребра: ");
                            int.TryParse(Console.ReadLine(), out start);
                            Console.Write("Введите конечную вершину ребра: ");
                            int.TryParse(Console.ReadLine(), out end);
                            try
                            {
                                myGraph.DelEdge(start, end);
                            }
                            catch (NonExistentEdgeException ex)
                            {
                                Console.WriteLine(ex.Message);
                                break;
                            }
                            catch (NonExistentVertexException ex)
                            {
                                Console.WriteLine(ex.Message);
                                break;
                            }
                            catch (InvalidInputException ex)
                            {
                                Console.WriteLine(ex.Message);
                                break;
                            }
                            Console.WriteLine("Ребро " + "(" + start + "," + end + ") удалено из графа");
                            break;
                        }
                    case "5":
                        {
                            Console.Write("\n1 - Перечисление множеств\n2 - Матрица смежности\n3 - Списки смежности\n4 - Матрица весов двудольного графа\n" +
                                "_______________________\nВыберите способ вывода: ");
                            string type = Console.ReadLine();
                            Console.WriteLine("Граф G: ");
                            if (type == "1")
                                myGraph.Show1();
                            else if (type == "2")
                                myGraph.Show2();
                            else if (type == "3")
                                myGraph.Show3();
                            else if (type == "4")
                            {
                                try
                                {
                                    myGraph.Show4();
                                }
                                catch (NonBipartiteGraphException ex)
                                {
                                    Console.WriteLine(ex.Message);
                                }
                            }
                            else
                                Console.WriteLine("Ошибка | Выберите один из доступных способов вывода ");
                            break;
                        }
                    case "6":
                        {
                            List<Edge> matching;
                            List<Vertex> markedVertex = new List<Vertex>();
                            try
                            {
                                matching = myGraph.SearchMax(ref markedVertex);
                            }
                            catch (NonBipartiteGraphException ex)
                            {
                                Console.WriteLine(ex.Message);
                                break;
                            }
                            Console.WriteLine(myGraph.PrintMatching(matching));
                            break;
                        }
                    case "7":
                        {
                            List<Edge> matching;
                            try
                            {
                                matching = myGraph.SearchMaxMin();
                            }
                            catch (NonBipartiteGraphException ex)
                            {
                                Console.WriteLine(ex.Message);
                                break;
                            }
                            catch (NonCompleteBipartiteGraphException ex)
                            {
                                Console.WriteLine(ex.Message);
                                break;
                            }
                            Console.WriteLine(myGraph.PrintMaxMin(matching));
                            break;
                        }
                    default:
                        {
                            Console.WriteLine("Неизвестное действие");
                            break;
                        }
                }
            }
        }
    }
}
