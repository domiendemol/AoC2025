namespace AoC2025;

public class Day9
{
	public (string, string) Run(List<string> lines)
	{
		List<Vector2Int> redTiles = lines.Select(l => l.Split(",")).Select(list => new Vector2Int(int.Parse(list[0]), int.Parse(list[1]))).ToList();
		
		// part 1
		// manually loop all combinations, find largest square
		(long square, Vector2Int a, Vector2Int b) max = (0, redTiles[0], redTiles[0]);
		for (int i = 0; i < redTiles.Count; i++)
		{
			for (int j = i+1; j < redTiles.Count; j++)
			{
				long square = Square(redTiles[i], redTiles[j]);
				if (square > max.square)
				{
					max = (square, redTiles[i], redTiles[j]);
				}
			}
		}
		
		return (max.square.ToString(), Part2(redTiles).ToString());
	}

	long Part2(List<Vector2Int> redTiles)
	{
		// build lists of horizontal and vertical line segments
		// build maps of min and maximum occupied tiles, per row and column (x and y)
		List<(Vector2Int, Vector2Int)> horLines = new List<(Vector2Int, Vector2Int)>();
		List<(Vector2Int, Vector2Int)> verLines = new List<(Vector2Int, Vector2Int)>();
		Dictionary<int, int> minXMap = new Dictionary<int, int>();
		Dictionary<int, int> maxXMap = new Dictionary<int, int>();
		Dictionary<int, int> minYMap = new Dictionary<int, int>();
		Dictionary<int, int> maxYMap = new Dictionary<int, int>();
		
		for (int i = 0; i < redTiles.Count; i++)
		{
			int nextI = (i+1) % redTiles.Count;
			// horizontal line
			if (redTiles[i].y == redTiles[nextI].y) 
			{
				horLines.Add((redTiles[i], redTiles[nextI]));
				for (int k = Math.Min(redTiles[i].x, redTiles[nextI].x); k < Math.Max(redTiles[i].x, redTiles[nextI].x); k++)
				{
					if (!minYMap.ContainsKey(k) || redTiles[i].y < minYMap[k]) minYMap[k] = redTiles[i].y;
					if (!maxYMap.ContainsKey(k) || redTiles[i].y > maxYMap[k]) maxYMap[k] = redTiles[i].y;
				}
				if (!minXMap.ContainsKey(redTiles[i].y) || Math.Min(redTiles[i].x, redTiles[nextI].x) < minXMap[redTiles[i].y]) minXMap[redTiles[i].y] = Math.Min(redTiles[i].x, redTiles[nextI].x);
				if (!maxXMap.ContainsKey(redTiles[i].y) || Math.Max(redTiles[i].x, redTiles[nextI].x) > maxXMap[redTiles[i].y]) maxXMap[redTiles[i].y] = Math.Max(redTiles[i].x, redTiles[nextI].x);

			}
			// vertical line
			else 
			{
				verLines.Add((redTiles[i], redTiles[nextI]));
				for (int k = Math.Min(redTiles[i].y, redTiles[nextI].y); k < Math.Max(redTiles[i].y, redTiles[nextI].y); k++)
				{
					if (!minXMap.ContainsKey(k) || redTiles[i].x < minXMap[k]) minXMap[k] = redTiles[i].x;
					if (!maxXMap.ContainsKey(k) || redTiles[i].x > maxXMap[k]) maxXMap[k] = redTiles[i].x;
				}
				if (!minYMap.ContainsKey(redTiles[i].x) || Math.Min(redTiles[i].y, redTiles[nextI].y) < minYMap[redTiles[i].x]) minYMap[redTiles[i].x] = Math.Min(redTiles[i].y, redTiles[nextI].y);
				if (!maxYMap.ContainsKey(redTiles[i].x) || Math.Max(redTiles[i].y, redTiles[nextI].y) > maxYMap[redTiles[i].x]) maxYMap[redTiles[i].x] = Math.Max(redTiles[i].y, redTiles[nextI].y);
			} 
		}
		
		// we prepared all our data, now loop all tile combos and do intersection checks + min/max checks
		(long square, Vector2Int a, Vector2Int b) max = (0, redTiles[0], redTiles[0]);
		for (int i = 0; i < redTiles.Count; i++)
		{
			for (int j = i+1; j < redTiles.Count; j++)
			{
				long square = Square(redTiles[i], redTiles[j]);
				if (square > max.square)
				{
					Vector2Int tileA = redTiles[i];
					Vector2Int tileB = redTiles[j];
					// create 4 sides of the square
					(Vector2Int, Vector2Int) side1 = (tileA, new Vector2Int(tileA.x, tileB.y));
					(Vector2Int, Vector2Int) side2 = (new Vector2Int(tileA.x, tileB.y), tileB);
					(Vector2Int, Vector2Int) side3 = (tileB, new Vector2Int(tileB.x, tileA.y));
					(Vector2Int, Vector2Int) side4 = (new Vector2Int(tileB.x, tileA.y), tileA);
					// opposite tiles of the square
					Vector2Int tileC = new Vector2Int(tileA.x, tileB.y);
					Vector2Int tileD = new Vector2Int(tileB.x, tileA.y);
					
					// validate if opposite tiles are within min/max tiles per row and column
					if (minXMap[tileC.y] > tileC.x) continue;
					if (maxXMap[tileC.y] < tileC.x) continue;
					if (minYMap[tileC.x] > tileC.y) continue;
					if (maxYMap[tileC.x] < tileC.y) continue;
					
					if (minXMap[tileD.y] > tileD.x) continue;
					if (maxXMap[tileD.y] < tileD.x) continue;
					if (minYMap[tileD.x] > tileD.y) continue;
					if (maxYMap[tileD.x] < tileD.y) continue;
					
					// validate that the sides don't intersect with our lines
					if (!IntersectsWithLines(side1.Item1, side1.Item2, horLines, verLines)
					    && !IntersectsWithLines(side2.Item1, side2.Item2, horLines, verLines)
					    && !IntersectsWithLines(side3.Item1, side3.Item2, horLines, verLines)
					    && !IntersectsWithLines(side4.Item1, side4.Item2, horLines, verLines))
					{
						max = (square, tileA, tileB);
					}
				}
			}
		}
		
		// Console.WriteLine(max.a);
		// Console.WriteLine(max.b);
		return max.square;
	}
	
	long Square(Vector2Int a, Vector2Int b)
	{
		return (Math.Abs(a.x - b.x) + 1L) * (Math.Abs(a.y - b.y) + 1L);
	}

	bool IntersectsWithLines(Vector2Int a, Vector2Int b, List<(Vector2Int, Vector2Int)> horLines, List<(Vector2Int, Vector2Int)> verLines)
	{
		if (a.y == b.y)
		{
			foreach ((Vector2Int, Vector2Int) verLine in verLines)
			{
				if (Intersects(a, b, verLine.Item1, verLine.Item2)) return true;
			}
		}
		else
		{
			foreach ((Vector2Int, Vector2Int) horLine in horLines)
			{
				if (Intersects(horLine.Item1, horLine.Item2, a, b)) return true;
			}
		}
		return false;
	}

	// assuming horizontal/vertical
	bool Intersects(Vector2Int hor1, Vector2Int hor2, Vector2Int ver1, Vector2Int ver2)
	{
		if (hor1 == ver1 && hor2 == ver2) return false;
		if (hor1 == hor2 || ver1 == ver2) return false;

		// allow if one of the vertices match
		if (hor1 == ver1 || hor2 == ver1 || hor1 == ver2 || hor2 == ver2) return false;
		
		if ((Math.Min(hor1.x, hor2.x) < ver1.x && Math.Max(hor1.x, hor2.x) > ver1.x)
			&& (Math.Min(ver1.y, ver2.y) < hor1.y && Math.Max(ver1.y, ver2.y) > hor1.y)) return true;

		return false;
	}
}