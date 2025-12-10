using System.Text.RegularExpressions;

namespace AoC2025;

public class Day10
{
	public (string, string) Run(List<string> lines)
	{
		// parse machines
		List<Machine> machines = lines.Select(CreateMachine).ToList();

		
		// TODO fewest total presses required to correctly configure all indicator lights
		// per machine
		
		int part1 = machines.Sum(m => GetOptimalButtonClicks(m));
		
		return (part1.ToString(), "");
	}

	Machine CreateMachine(string line)
	{
		// example: [.##.] (3) (1,3) (2) (2,3) (0,2) (0,1) {3,5,4,7}
		Match m1 = Regex.Match(line, @"\[([\.#]+)\]");
		MatchCollection m2 = Regex.Matches(line, @"\(([0-9,]+)\)");
		Match m3 = Regex.Match(line, @"{(.*)}");
		
		bool[] lightTargets = m1.Groups[1].Value.Select(c => c == '#').ToArray();
		List<List<int>> buttons = m2.Select(m => m.Groups[1].Value.Split(',').Select(int.Parse).ToList()).ToList();
		List<int> joltageReqs = m3.Groups[1].Value.Split(',').Select(int.Parse).ToList();
		
		return new Machine(lightTargets, buttons, joltageReqs);
	}

	int GetOptimalButtonClicks(Machine machine)
	{
		// loop ALL combinations, BFS style
		// so we know that first result is optimal one
		
		// TODO optimize with memoization
		Dictionary<int, int> visited = new();
		
		Queue<(int depth, List<int> button, bool[] lights)> queue = new();
		machine.Buttons.ForEach(btn => queue.Enqueue((1, btn, new bool[machine.Lights.Length])) );
		
		while (queue.Count > 0)
		{
			(int depth, List<int> button, bool[] lights) next = queue.Dequeue();
			if (visited.TryGetValue(LightHash(next.lights), out int prevDepth) && prevDepth < next.depth-1) continue;
			
			// Console.WriteLine($"{next.depth} {FormatLights(next.lights)} - {string.Join(',', next.button)}");
			
			// press button
			next.button.ForEach(btn => next.lights[btn] = !next.lights[btn]);
			// Console.WriteLine($"  {FormatLights(next.lights)} - {string.Join(',', next.button)}");
			if (next.lights.SequenceEqual(machine.LightTargets)){
				// Console.WriteLine($"{next.depth}: {String.Join(',', next.lights)}");
				return next.depth;
			}

			foreach (List<int> btn in machine.Buttons)
			{
				if (btn.SequenceEqual(next.button)) continue; // don't press same button twice in a row
				queue.Enqueue((next.depth + 1, btn, (bool[])next.lights.Clone()));
			}

			if (!visited.ContainsKey(LightHash(next.lights)))
				visited.Add(LightHash(next.lights), next.depth);
		}

		return -1;
	}

	string FormatLights(bool[] lights)
	{
		return $"[{string.Join("", lights.Select(l => l ? "#" : "."))}]";
	}

	int LightHash(bool[] lights)
	{
		return lights.Select((l, i) => l ? (int) Math.Pow(10, i) : 0).Sum();
	}
	
	class Machine
	{
		public bool[] Lights;
		public bool[] LightTargets;
		public List<List<int>> Buttons;
		public List<int> JoltageReqs;

		public Machine(bool[] lightTargets, List<List<int>> buttons, List<int> joltageReqs)
		{
			Lights = new bool[lightTargets.Length];
			LightTargets = lightTargets;
			Buttons = buttons;
			JoltageReqs = joltageReqs;
		}
	}
}