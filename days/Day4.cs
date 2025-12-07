namespace AoC2025;

public class Day4
{
	public (string, string) Run(List<string> lines)
	{
		char[,] grid = Utils.ToCharArray(lines);

		return (GetFreeRollCount(grid).ToString(), RemoveFreeRollsOptimized(grid).ToString());
	}

	int GetFreeRollCount(char[,] grid)
	{
		// loop all positions
		// check all adjacent positions for rolls of paper
		int freeRolls = 0;
		grid.ForEach<char>((value, coords) =>
		{
			if (value == '@' && GetNeighbourRollCount(grid, coords) < 4) freeRolls++;
		});
		
		return freeRolls;
	}
	
	List<int[]> GetFreeRolls(char[,] grid)
	{
		List<int[]> freeRolls = new List<int[]>();
		grid.ForEach<char>((value, coords) =>
		{
			if (value == '@' && GetNeighbourRollCount(grid, coords) < 4) freeRolls.Add(coords);
		});
		return freeRolls;
	}
	
	int GetNeighbourRollCount(char[,] grid, int[] pos)
	{
		/*
		int total = 0;
		for (int i = -1; i <= 1; i++) { 
			for (int j = -1; j <= 1; j++)
			{
				if ((i == 0 && j == 0)) continue; // (i != 0 && j != 0)
				var value = grid.TryGetValue<char>(pos[0] + i, pos[1] + j, ' ');
				if (value == '@') total++;
			}
		}

		return total;
		*/
		return Utils.GetNeighbourValues<char>(grid, pos).Count(c => c == '@');
	}

	int RemoveFreeRolls(char[,] grid)
	{
		int totalRemoved = 0;
		int removed = -1;
		while (removed != 0)
		{
			List<int[]> free = GetFreeRolls(grid);
			free.ForEach(pos => grid[pos[0], pos[1]] = '.');
			removed = free.Count;
			totalRemoved += removed;
			Console.WriteLine("iter");
		}
		return totalRemoved;
	}
	
	// doesn't loop whole grid to find new free rolls, but starts from recently freed rolls instead
	int RemoveFreeRollsOptimized(char[,] grid)
	{
		List<int[]> free = GetFreeRolls(grid);

		int totalRemoved = 0;
		int removed = -1;
		while (free.Count > 0)
		{
			free.ForEach(pos => grid[pos[0], pos[1]] = '.');
			totalRemoved += free.Count;
			
			// continue with neighbours of newly freed rolls
			free = free
				.SelectMany(pos => Utils.GetNeighbours<char>(grid, pos))
				.GroupBy(x => String.Join(",", x))
				.Select(x => x.First().ToArray())
				.Where(pos => grid[pos[0], pos[1]] == '@' && GetNeighbourRollCount(grid, pos) < 4)
				.ToList();	
		}
		return totalRemoved;
	}
}