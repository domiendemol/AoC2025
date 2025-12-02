namespace AoC2025;

public class Day2
{
	public (string, string) Run(List<string> lines)
	{
		List<string> ranges = lines[0].Split(',').ToList();
		// Original:
		// long part1 = ranges.Sum(range => GetInvalids(range, true));
		// long part2 = ranges.Sum(range => GetInvalids(range, false));

		// With multithreading:
		long part1 = 0;
		Parallel.ForEach(ranges, (c, state, index) => {
			Interlocked.Add(ref part1, GetInvalids(c, true));  
		});
		long part2 = 0;
		Parallel.ForEach(ranges, (c, state, index) => {
			Interlocked.Add(ref part2, GetInvalids(c, false));  
		});
		
		return (part1.ToString(), part2.ToString());
	}

	long GetInvalids(string range, bool onlyTwo)
	{
		long result = 0;
		long[] parts = range.Split('-').Select(long.Parse).ToArray();

		for (long i = parts[0]; i <= parts[1]; i++)
		{
			string nr = i.ToString();
			int len = nr.Length;
			// loop nr of parts to check for
			for (int j = 2; j <= len; j++)
			{
				if (onlyTwo && j > 2) break;
				if (len % j != 0) continue;
				if (CheckInvalidOptimized(nr, j)) 
				{
					result += i;
					break;
				}
			}
		}
		return result;
	}

	// slightly optimized version checking chars instead of substrings
	bool CheckInvalidOptimized(string nr, int parts)
	{
		int partLength = nr.Length / parts;
		// loop and check all others
		for (int i = 1; i < parts; i++)
		{
			for (int k = 0; k < partLength; k++)
			{
				if (nr[k] != nr[i * partLength + k]) return false;
			}
		}
		return true;
	}
	
	// original version comparing substrings
	bool CheckInvalid(string nr, int parts)
	{
		// get first part/substring
		int partLength = nr.Length / parts;
		string first =  nr.Substring(0, partLength);
		// loop and check all others
		for (int i = 1; i < parts; i++)
		{
			string part = nr.Substring(i * partLength, partLength);
			if (!first.Equals(part)) return false;
		}
		return true;
	}
}