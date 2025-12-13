using System.Text.RegularExpressions;

namespace AoC2025;

public class Day11
{
	Dictionary<string, List<string>> _deviceMap = new ();
	
	public (string, string) Run(List<string> lines)
	{
		ParseInput(lines, 1);
		long part1 = GetNrPaths("you", "out", new(), new (), false);
		
		ParseInput(lines, 2);
		long part2 = GetNrPaths("svr", "out", new(), new (), true);
		
		return (part1.ToString(), part2.ToString());
	}

	void ParseInput(List<string> lines, int part)
	{
		if (part == 1)
		{
			// test input: take 10 first lines for part 1
			if (lines.Count < 25) {
				BuildDeviceMap(lines.Take(10));		
			}
			else {
				BuildDeviceMap(lines);
			}			
		}
		else if (part == 2 && lines.Count < 25)
		{
			// test input: take 13 last lines for part 2
			_deviceMap.Clear();
			BuildDeviceMap(lines.TakeLast(13));
		}

	}

	void BuildDeviceMap(IEnumerable<string> lines)
	{
		lines.Select(line => line.Split(':')).ToList()
			.ForEach(parts => _deviceMap[parts[0]] = Regex.Matches(parts[1], "[a-z]+").Select(m => m.Value).ToList());			
	}

	long GetNrPaths(string current, string target, HashSet<string> visited, Dictionary<(string, string), long> pathCache, bool passDacFft)
	{
		long nrPaths = 0;

		visited.Add(current);
		
		List<string> outputs = _deviceMap[current];
		foreach (string output in outputs)
		{
			if (visited.Contains(output)) continue;
			
			if (output == target) {
				if (!passDacFft || (visited.Contains("dac") && visited.Contains("fft")))
					nrPaths++;
				continue;
			}
			
			if (passDacFft && visited.Contains("dac") && !visited.Contains("fft") && pathCache.TryGetValue(("fft", output), out long fftVal)) {
				nrPaths += fftVal;
				continue;
			}
			if (passDacFft && visited.Contains("fft") && !visited.Contains("dac") && pathCache.TryGetValue(("dac", output), out long dacVal)) {
				nrPaths += dacVal;
				continue;
			}
			if (passDacFft && !visited.Contains("dac") && !visited.Contains("fft") && pathCache.TryGetValue(("dacfft", output), out long dacFftVal)) {
				nrPaths += dacFftVal;
				continue;
			}
			if (passDacFft && visited.Contains("dac") && visited.Contains("fft") && pathCache.TryGetValue(("", output), out long noneVal)) {
				nrPaths += noneVal;
				continue;
			}
			
			nrPaths += GetNrPaths(output, target, visited, pathCache, passDacFft);
		}
		
		visited.Remove(current);
		
		// cache
		if (!visited.Contains("fft") && !visited.Contains("dac") && !pathCache.ContainsKey(("dacfft", current)))
			pathCache.Add(("dacfft", current), nrPaths); 
		else if (!visited.Contains("dac") && !pathCache.ContainsKey(("dac", current)))
			pathCache.Add(("dac", current), nrPaths); 
		else if (!visited.Contains("fft") && !pathCache.ContainsKey(("fft", current)))
			pathCache.Add(("fft", current), nrPaths); 
		else if (visited.Contains("fft") && visited.Contains("dac") && !pathCache.ContainsKey(("", current)))
			pathCache.Add(("", current), nrPaths); 
		
		return nrPaths;
	}
}