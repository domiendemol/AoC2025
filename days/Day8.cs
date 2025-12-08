using System.Text.RegularExpressions;

namespace AoC2025;

public class Day8
{
	struct Connection
	{
		public int Distance;
		public Vector3Int[] Points;
	}
	
	Dictionary<Vector3Int, int> _boxCircuitMap = new();
	Dictionary<int, List<Vector3Int>> _circuitBoxMap = new();
	
	public (string, string) Run(List<string> lines)
	{
		List<Vector3Int> boxes = lines.Select(l => Regex.Match(l, "([0-9]+),([0-9]+),([0-9]+)"))
			.Select(m => new Vector3Int(Convert.ToInt32(m.Groups[1].Value), Convert.ToInt32(m.Groups[2].Value), Convert.ToInt32(m.Groups[3].Value))).ToList();
		
		// to improve: this is by far the slowest part
		List<Connection> connections = boxes.SelectMany(p1 => boxes,
				(p1, p2) => new Connection { Distance = Dist(p1, p2), Points = new[] { p1, p2 } })
			.Where(x => x.Points[0] != x.Points[1] && x.Points[0].x <= x.Points[1].x)
			.OrderBy(x=> x.Distance)
			.ToList();
		
		// part 1
		Connect(boxes, 0, lines.Count < 25 ? 10 : 1000, connections);
		var longestCircuits = _circuitBoxMap.Select(p => p.Value).OrderByDescending(c => c.Count).Take(3);
		long part1 = longestCircuits.Aggregate(1, (x,y) => x * y.Count);
		
		// part 2
		(int, int) lastConnection = Connect(boxes, lines.Count < 25 ? 10 : 1000, int.MaxValue, connections);
		
		return (part1.ToString(), (lastConnection.Item1 * lastConnection.Item2).ToString());
	}

	(int, int) Connect(List<Vector3Int> boxes, int start, int target, List<Connection> connections)
	{
		int circuitNr = _circuitBoxMap.Count;
		
		int nrConn = start;
		(int, int) lastConn = (0,0);
		while (nrConn < target && _boxCircuitMap.Keys.Count < boxes.Count)
		{
			// find closest pairing
			Connection closest = connections[nrConn];

			if (_boxCircuitMap.ContainsKey(closest.Points[0]) && _boxCircuitMap.ContainsKey(closest.Points[1]))
			{
				if (_boxCircuitMap[closest.Points[0]] != _boxCircuitMap[closest.Points[1]]) {
					// set all of partition 1 to 0
					var part1Boxes = _circuitBoxMap[_boxCircuitMap[closest.Points[1]]];
					int circ = _boxCircuitMap[closest.Points[0]];
					part1Boxes.ForEach(p => _boxCircuitMap[p] = circ);
					_circuitBoxMap[ _boxCircuitMap[closest.Points[0]] ].AddRange(part1Boxes);			
				}
			}
			else if (_boxCircuitMap.ContainsKey(closest.Points[0])) {
				_boxCircuitMap[closest.Points[1]] = _boxCircuitMap[closest.Points[0]];
				_circuitBoxMap[_boxCircuitMap[closest.Points[0]]].Add(closest.Points[1]);
			}
			else if (_boxCircuitMap.ContainsKey(closest.Points[1])) {
				_boxCircuitMap[closest.Points[0]] = _boxCircuitMap[closest.Points[1]];
				_circuitBoxMap[_boxCircuitMap[closest.Points[1]]].Add(closest.Points[0]);				
			}
			else {
				// new partition
				_boxCircuitMap.Add(closest.Points[0], ++circuitNr);
				_boxCircuitMap.Add(closest.Points[1], circuitNr);
				_circuitBoxMap.Add(circuitNr, new List<Vector3Int>(closest.Points));
			}
			
			lastConn = (closest.Points[0].x, closest.Points[1].x);
			nrConn++;
		}
		
		return lastConn;
	}

	int Dist(Vector3Int a, Vector3Int b)
	{
		return (int) Math.Round(Math.Pow(a.x - b.x,2) + Math.Pow(a.y - b.y,2) + Math.Pow(a.z - b.z,2));
	}
}