using System.Text.RegularExpressions;

namespace AoC2025;

public class Day8
{
	class Connection
	{
		public int Distance;
		public Vector3Int[] Points;
	}
	
	public (string, string) Run(List<string> lines)
	{
		List<Vector3Int> boxes = lines.Select(l => Regex.Match(l, "([0-9]+),([0-9]+),([0-9]+)"))
			.Select(m => new Vector3Int(Convert.ToInt32(m.Groups[1].Value), Convert.ToInt32(m.Groups[2].Value), Convert.ToInt32(m.Groups[3].Value))).ToList();
		
		List<Connection> connections = boxes.SelectMany(p1 => boxes,
				(p1, p2) => new Connection { Distance = Dist(p1, p2), Points = new[] { p1, p2 } })
			.Where(x => x.Points[0] != x.Points[1] && x.Points[0].x <= x.Points[1].x)
			.OrderBy(x=> x.Distance)
			.ToList();
		
		// part 1
		Connect(boxes, lines.Count < 25 ? 10 : 1000, connections, out var circuitBoxMap);
		var longestCircuits = circuitBoxMap.Select(p => p.Value).OrderByDescending(c => c.Count).Take(3);
		long part1 = longestCircuits.Aggregate(1, (x,y) => x * y.Count);
		
		// TODO make part2 continue from part 1 calculations
		
		// return (part1.ToString(), "");
		
		
		// part 2
		(int, int) lastConnection = Connect(boxes, int.MaxValue, connections, out var circuitBoxMap2);
		
		return (part1.ToString(), (lastConnection.Item1 * lastConnection.Item2).ToString());
	}

	(int, int) Connect(List<Vector3Int> boxes, int target, List<Connection> connections, out Dictionary<int, List<Vector3Int>> circuits)
	{
		int circuitNr = 0;
		Dictionary<Vector3Int, int> boxCircuitMap = new();
		Dictionary<int, List<Vector3Int>> circuitBoxMap = new();
		
		int nrConn = 0;
		(int, int) lastConn = (0,0);
		while (nrConn < target && boxCircuitMap.Keys.Count < boxes.Count)
		{
			// find closest pairing
			Connection closest = connections[nrConn];
			// Console.WriteLine($"Connection {nrConn} = {closest.Points[0]}-{closest.Points[1]}: {minDistance}");

			if (boxCircuitMap.ContainsKey(closest.Points[0]) && boxCircuitMap.ContainsKey(closest.Points[1]))
			{
				if (boxCircuitMap[closest.Points[0]] != boxCircuitMap[closest.Points[1]]) {
					// set all of partition 1 to 0
					var part1Boxes = circuitBoxMap[boxCircuitMap[closest.Points[1]]];
					part1Boxes.ForEach(p => boxCircuitMap[p] = boxCircuitMap[closest.Points[0]]);
					circuitBoxMap[ boxCircuitMap[closest.Points[0]] ].AddRange(part1Boxes);			
				}
			}
			else if (boxCircuitMap.ContainsKey(closest.Points[0])) {
				boxCircuitMap[closest.Points[1]] = boxCircuitMap[closest.Points[0]];
				circuitBoxMap[boxCircuitMap[closest.Points[0]]].Add(closest.Points[1]);
			}
			else if (boxCircuitMap.ContainsKey(closest.Points[1])) {
				boxCircuitMap[closest.Points[0]] = boxCircuitMap[closest.Points[1]];
				circuitBoxMap[boxCircuitMap[closest.Points[1]]].Add(closest.Points[0]);				
			}
			else {
				// new partition
				boxCircuitMap.Add(closest.Points[0], ++circuitNr);
				boxCircuitMap.Add(closest.Points[1], circuitNr);
				circuitBoxMap.Add(circuitNr, new List<Vector3Int>(closest.Points));
			}
			
			lastConn = (closest.Points[0].x, closest.Points[1].x);
			nrConn++;
		}
		
		circuits = circuitBoxMap;
		return lastConn;
	}

	int Dist(Vector3Int a, Vector3Int b)
	{
		return (int) Math.Round(Math.Sqrt(Math.Pow(a.x - b.x,2) + Math.Pow(a.y - b.y,2) + Math.Pow(a.z - b.z,2)));
	}
}