namespace AoC2025;

public class Day4
{
	public (string, string) Run(List<string> lines)
	{
		char[,] grid = Utils.ToCharArray(lines);

		return (GetFreeRollCount(grid).ToString(), RemoveFreeRolls(grid).ToString());
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
		}
		return totalRemoved;
	}
}