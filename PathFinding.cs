namespace AoC2025;

public class PathFinding
{
    // assuming top left starting position => x horizontal, y vertical
    public static Vector2Int Up = new Vector2Int(0, -1);
    public static Vector2Int Down = new Vector2Int(0, 1);
    public static Vector2Int Right = new Vector2Int(1, 0);
    public static Vector2Int Left = new Vector2Int(-1, 0);
    public static Vector2Int[] directions = new[] { Left, Up, Down, Right };
    
    public class Node : IComparable<Node>
    {
        public char id;
        public Vector2Int pos;
        public List<Node> siblings = new List<Node>();
        public List<Node> prevNodes = new List<Node>();
        public bool visited;
        public int cost = Int32.MaxValue;
        
        public Node(Vector2Int pos, char id)
        {
            this.pos = pos;
            this.id = id;
        }

        public int CompareTo(Node? other)
        {
            if (other == null) return 1;
            return cost.CompareTo(other.cost);
        }

        public void Reset()
        {
            cost = int.MaxValue;
            prevNodes.Clear();
        }

        public override string ToString() => id.ToString();
        public override bool Equals(object? obj)
        {
            return id.Equals(((Node)obj).id);
        }
    }
    
    // BFS, return all equal cost paths
    public static List<List<Node>>? FindShortestPaths(List<Node> graph, Node start, Node end, Vector2Int[] directions)
    {
        if (start.Equals(end)) return null;
        graph.ForEach(n => n.Reset());
        
        Queue<Node> queue = new Queue<Node>();
        start.cost = 0;
        queue.Enqueue(start);
        
        while (queue.Count > 0)
        {
            Node node = queue.Dequeue();
            // check its neighbours
            List<(Node, Vector2Int)> possibleSteps = GetPossibleDirections(node, directions);
            foreach ((Node node, Vector2Int) nextStep in possibleSteps)
            {
                if (node.cost + 1 <= nextStep.node.cost) {
                    if (node.cost + 1 < nextStep.node.cost) {
                        nextStep.node.cost = node.cost + 1;
                        queue.Enqueue(nextStep.node);
                    }
                    if (!nextStep.node.prevNodes.Contains(node)) nextStep.node.prevNodes.Add(node);
                }
            }
            node.visited = true;
        }

        // List<Node> path = new List<Node>();
        // Node pathNode = end;
        // while (pathNode.prevNodes.Count > 0)
        // {
        //     path.Insert(0, pathNode);
        //     pathNode = pathNode.prevNodes[0];
        // }
        // // path.Insert(0, pathNode);
        
        return GetAllPaths(end, new List<Node>());
    }

    public static List<List<Node>> GetAllPaths(Node end, List<Node> pathSoFar)
    {
        List<List<Node>> pathList = new List<List<Node>>();

        Node pathNode = end; // pathSoFar.Count > 0 ? pathSoFar[0] : end;
        if (pathNode.prevNodes.Count > 0)
        {
            pathSoFar.Insert(0, pathNode);
            foreach (Node prev in pathNode.prevNodes) {
                pathList = pathList.Union(GetAllPaths(prev, new List<Node>(pathSoFar))).ToList();
            }
            // pathNode = pathNode.prevNodes[0];
            return pathList;
        }

        return new List<List<Node>>(){pathSoFar};
    }
    
    public static List<(Node, Vector2Int dir)> GetPossibleDirections(Node node, Vector2Int[] directions)   
    {
        List<(Node next, Vector2Int dir)> result = new();
        for (int i = 0; i < directions.Length; i++)
        {
            Vector2Int nextPos = node.pos + directions[i];
            Node next = node.siblings.FirstOrDefault(n => n.pos == node.pos + directions[i], null);
            if (next != null && next != node)
            {
                result.Add((next, nextPos));
            }
        }

        return result;
    }
}