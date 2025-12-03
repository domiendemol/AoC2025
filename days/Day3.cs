namespace AoC2025;

public class Day3
{
	public (string, string) Run(List<string> lines)
	{
		List<char[]> banks = lines.Select(line => line.ToCharArray()).ToList();
		int part1 = banks.Sum(bank=> GetJoltage(bank));
		long part2 = banks.Sum(bank=> GetJoltage(bank, 0, 1, 12));
		return (part1.ToString(), part2.ToString());
	}

	// part 1
	int GetJoltage(char[] bank)
	{
		// loop from 99 to 11: test if possible in our battery bank using indexes
		for (int i = 9; i > 0; i--)
		{
			int index = Array.FindIndex(bank, w => w == '0' + (char)i);
			if (index == -1) continue;

			for (int j = 9; j > 0; j--)
			{
				int index2 = Array.FindIndex(bank, index + 1, w => w == '0' + (char)j);
				if (index2 != -1)
				{
					Console.WriteLine($"{i}{j}");
					return i * 10 + j;
				}
			}
		}

		return 0;
	}
	
	// part 2: variable depth -> recursive
	long GetJoltage(char[] bank, int startIndex, int depth, int maxDepth)
	{
		for (int i = 9; i > 0; i--)
		{
			int index = Array.FindIndex(bank, startIndex,w => w == '0' + (char)i);
			if (index == -1) continue;

			// reached the end
			if (depth == maxDepth) return i;
			
			long next = GetJoltage(bank, index + 1, depth + 1, maxDepth);
			if (next == 0) continue;
			
			return (long)((i * Math.Pow(10, maxDepth-depth)) + next);
		}
		return 0;
	}
}