namespace AoC2024;

public class Day2
{
	public (string, string) Run(List<string> lines)
	{
		List<string> ranges = lines[0].Split(',').ToList();
		long part1 = ranges.Sum(range => GetInvalids(range, true));
		long part2 = ranges.Sum(range => GetInvalids(range, false));
		
		return (part1.ToString(), part2.ToString());
	}

	long GetInvalids(string range, bool onlyTwo)
	{
		long result = 0;
		long[] parts = range.Split('-').Select(part => long.Parse(part)).ToArray();

		for (long i = parts[0]; i <= parts[1]; i++)
		{
			string nr = i.ToString();
			int len = i.ToString().Length;
			// loop nr of parts to check for
			for (int j = 2; j <= len; j++)
			{
				if (onlyTwo && j > 2) break;
				if (len % j != 0) continue;
				if (CheckInvalid(nr, j)) 
				{
					result += i;
					break;
				}
			}

		}
		return result;
	}

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