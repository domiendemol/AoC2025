using System.Text.RegularExpressions;

namespace AoC2025;

public class Day5
{
	public (string, string) Run(List<string> lines)
	{
		List<long[]> freshRanges = lines.Where(l => l.Contains('-'))
			.Select(line => Regex.Matches(line, @$"^([0-9]+)-([0-9]+)")[0])
			.Select(m => new long[] { Convert.ToInt64(m.Groups[1].Value), Convert.ToInt64(m.Groups[2].Value) })
			.ToList();
		
		// part 1
		List<long> ingrIds = lines.Where(l => !l.Contains('-') && l.Length > 0).Select(line => Convert.ToInt64(line)).ToList();
		long part1 = ingrIds.Count(id => freshRanges.Any(r => id >= r[0] && id <= r[1]));

		// part 2: keep reducing until no more ranges overlap
		// then we can just count the range lengths
		int removedRanges = -1;
		while (removedRanges != 0)
			removedRanges = ReduceRanges(ref freshRanges);
		
		return (part1.ToString(), freshRanges.Sum(range => range[1]-range[0]+1).ToString());
	}

	// reduce list of ranges, return nr of ranges removed
	private int ReduceRanges(ref List<long[]> freshRanges)
	{
		List<long[]> reducedRanges = new List<long[]>();
		foreach (long[] range in freshRanges)
		{
			// find overlap
			long[] overlapping = reducedRanges.FirstOrDefault(r => (Math.Max(range[0], r[0]) <= Math.Min(range[1], r[1])), null);
			if (overlapping != null) {
				// merge
				overlapping[0] = Math.Min(range[0], overlapping[0]);
				overlapping[1] = Math.Max(range[1], overlapping[1]);
			}
			else {
				reducedRanges.Add(range);
			}
		}

		int reduction = freshRanges.Count - reducedRanges.Count;
		freshRanges = reducedRanges;
		return reduction;
	}
}