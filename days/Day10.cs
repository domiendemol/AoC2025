using System.Text.RegularExpressions;

namespace AoC2025;

public class Day10
{
	public (string, string) Run(List<string> lines)
	{
		// parse machines
		List<Machine> machines = lines.Select(CreateMachine).ToList();

		int part1 = 0;
		Parallel.ForEach(machines, m => {
			Interlocked.Add(ref part1, GetMinLightButtonClicks(m));  
		});
		
		int part2 = 0;
		/*
		ParallelOptions options = new ParallelOptions { MaxDegreeOfParallelism = 2 }; 
		Parallel.ForEach(machines, m => {
			Interlocked.Add(ref part2, GetMinJoltageButtonClicks(m));  
		});
		*/
		 part2 = machines.Sum(GetMinJoltageButtonClicks);
		
		return (part1.ToString(), part2.ToString());
	}

	Machine CreateMachine(string line)
	{
		// example: [.##.] (3) (1,3) (2) (2,3) (0,2) (0,1) {3,5,4,7}
		Match m1 = Regex.Match(line, @"\[([\.#]+)\]");
		MatchCollection m2 = Regex.Matches(line, @"\(([0-9,]+)\)");
		Match m3 = Regex.Match(line, @"{(.*)}");
		
		bool[] lightTargets = m1.Groups[1].Value.Select(c => c == '#').ToArray();
		List<List<short>> buttons = m2.Select(m => m.Groups[1].Value.Split(',').Select(short.Parse).ToList()).ToList();
		List<short> joltageReqs = m3.Groups[1].Value.Split(',').Select(short.Parse).ToList();
		
		return new Machine(lightTargets, buttons, joltageReqs);
	}

	int GetMinLightButtonClicks(Machine machine)
	{
		// loop ALL combinations, BFS style
		// so we know that first result is optimal one
		
		Dictionary<int, int> visited = new();
		
		Queue<(int depth, List<short> button, bool[] lights)> queue = new();
		machine.Buttons.ForEach(btn => queue.Enqueue((1, btn, new bool[machine.JoltageReqs.Count])) );
		
		while (queue.Count > 0)
		{
			(int depth, List<short> button, bool[] lights) next = queue.Dequeue();
			// skip if we already visited this lights combo
			if (visited.TryGetValue(LightHash(next.lights), out int prevDepth) && prevDepth < next.depth-1) continue;
			
			// Console.WriteLine($"{next.depth} {FormatLights(next.lights)} - {string.Join(',', next.button)}");
			
			// press button
			next.button.ForEach(btn => next.lights[btn] = !next.lights[btn]);
			// Console.WriteLine($"  {FormatLights(next.lights)} - {string.Join(',', next.button)}");
			if (next.lights.SequenceEqual(machine.LightTargets)) {
				// Console.WriteLine($"{next.depth}: {String.Join(',', next.lights)}");
				return next.depth;
			}

			foreach (List<short> btn in machine.Buttons)
			{
				if (btn.SequenceEqual(next.button)) continue; // don't press same button twice in a row
				queue.Enqueue((next.depth + 1, btn, (bool[]) next.lights.Clone()));
			}

			if (!visited.ContainsKey(LightHash(next.lights)))
				visited.Add(LightHash(next.lights), next.depth);
		}

		return -1;
	}

	int GetMinJoltageButtonClicks(Machine machine)
	{
		// loop ALL combinations, BFS style
		// so we know that first result is optimal one
		
		Dictionary<int, short> visited = new();
		Dictionary<int, short> minDepths = new();
		
		Queue<(short depth, short button, short[] joltages)> queue = new();
		machine.Buttons.ForEach(btn => queue.Enqueue((1, (short) machine.Buttons.IndexOf(btn), new short[machine.JoltageReqs.Count])) );
		
		while (queue.Count > 0)
		{
			(short depth, short button, short[] joltages) next = queue.Dequeue();
			
			// skip if we already visited this joltages combo
			if (visited.TryGetValue(JoltageHash(next.joltages), out short prevDepth) && prevDepth < next.depth-1) continue;
			
			// press button
			machine.Buttons[next.button].ForEach(btn => next.joltages[btn]++); 

			// have we found it??
			if (next.joltages.SequenceEqual(machine.JoltageReqs)) {
				Console.WriteLine($"{next.depth}: {String.Join(',', next.joltages)}");
				return next.depth;
			}

			bool tooFar = false;
			foreach (List<short> btn in machine.Buttons)
				if (next.joltages.Select((jolt, i) => jolt > machine.JoltageReqs[i] && btn.Contains((short) i)).Any(el => el)) tooFar = true;
			if (tooFar) continue;
			
			// optimization: try multiples of this combo and check if we have the remainder in our visited map
			int i = 0;
			short[] multJoltages = (short[]) next.joltages.Clone();
			while (!tooFar)
			{
				for (int j = 0; j < next.joltages.Length; j++)
				{
					multJoltages[j] += next.joltages[j];
					if (multJoltages[j] + next.joltages[j] > machine.JoltageReqs[j])
					{
						tooFar = true;
					}
				}
				i++;
			}
			// check in visited 
			
			short[] remainder = next.joltages.Select((jolt, indx) => (short) (machine.JoltageReqs[indx] - multJoltages[indx] + jolt)).ToArray();
			if (visited.ContainsKey(JoltageHash(remainder)))
			{
				short totalDepth = (short) (next.depth * i + visited[JoltageHash(remainder)]);
				// Console.WriteLine($"woop {next.depth} * {i} + {visited[JoltageHash(remainder)]} = {totalDepth}");
				// Console.WriteLine($" {string.Join(",", next.joltages)} -> {string.Join(",", next.joltages.Select((jolt, indx) => multJoltages[indx] - jolt).ToArray())}");
				// Console.WriteLine($" remainder: {string.Join(",", remainder)} -> {visited[JoltageHash(remainder)]}");
				// Console.WriteLine($" target: {string.Join(",", machine.JoltageReqs)}");
				
				// 
				if (minDepths.ContainsKey(next.depth) && minDepths.ContainsKey(next.depth - 1))
				{
					Console.WriteLine($" ({string.Join(",", machine.JoltageReqs)}): min depth found in minDepths: {minDepths[next.depth - 1]}");
					return minDepths[next.depth - 1];
				}
				if (!minDepths.TryGetValue(next.depth, out short val) || val > totalDepth) 
					minDepths[next.depth] = totalDepth;
			}

			if (visited.ContainsKey(JoltageHash(next.joltages)))
				continue;
			
			foreach (List<short> btn in machine.Buttons)
			{
				// filter: stop here if any voltage is over the required setting
				if (next.joltages.Select((jolt, i) => jolt >= machine.JoltageReqs[i] && btn.Contains((short) i)).Any(el => el)) continue;
				queue.Enqueue(((short) (next.depth + 1), (short) machine.Buttons.IndexOf(btn), (short[]) next.joltages.Clone()));
			}

			visited.Add(JoltageHash(next.joltages), next.depth);
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
	
	int JoltageHash(short[] joltages)
	{
		// string join is too slow
		// return string.Join("", joltages);
		int hc = joltages.Length;
		foreach (int val in joltages)
		{
			hc = unchecked(hc * 314159 + val);
		}
		return hc;
	}
	
	// Gaussian Elimination based on https://www.codeproject.com/tips/388179/linear-equation-solver-gaussian-elimination-csharp
    public static class LinearEquationSolver
    {
        /// <summary>Computes the solution of a linear equation system.</summary>
        /// <param name="M">
        /// The system of linear equations as an augmented matrix[row, col] where (rows == cols + 1).
        /// It will contain the solution in "row canonical form" if the function returns "true".
        /// </param>
        /// <returns>Returns whether the matrix has a unique solution or not.</returns>
        public static bool Solve(ref double[,] M)
        {
            int rowCount = M.GetLength(0);
            if (M == null || M.Length != rowCount * (rowCount + 1))
                throw new ArgumentException("The algorithm must be provided with a (n x n+1) matrix.");
            // pivoting
            for (int col = 0; col + 1 < rowCount; col++)
            {
                if (M[col, col] == 0) // check for zero coefficients
                {
                    // find non-zero coefficient
                    int swapRow = 0; // col + 1;
                    for (; swapRow < rowCount; swapRow++)
                        if (M[swapRow, col] != 0)
                            break;
                    if (swapRow < rowCount && M[swapRow, col] != 0) 
                    {
                        // found non-zeo coefficient, swap it with the above
                        double[] tmp = new double[rowCount + 1];
                        for (int i = 0; i < rowCount + 1; i++)
                        {
                            tmp[i] = M[swapRow, i];
                            M[swapRow, i] = M[col, i];
                            M[col, i] = tmp[i];
                        }
                    }
                    else return false; // no, then the matrix has no unique solution
                }
            }

            // elimination
            for (int sourceRow = 0; sourceRow + 1 < rowCount; sourceRow++)
            {
                for (int destRow = sourceRow + 1; destRow < rowCount; destRow++)
                {
                    double df = M[sourceRow, sourceRow];
                    double sf = M[destRow, sourceRow];
                    for (int i = 0; i < rowCount + 1; i++) {
                        M[destRow, i] = M[destRow, i] * df - M[sourceRow, i] * sf;
                    }
                }
            }

            // back-insertion
            for (int row = rowCount - 1; row >= 0; row--)
            {
                double f = M[row,row];
                if (f == 0) return false;

                for (int i = 0; i < rowCount + 1; i++) M[row, i] /= f;
                for (int destRow = 0; destRow < row; destRow++) {
                    M[destRow, rowCount] -= M[destRow, row] * M[row, rowCount]; M[destRow, row] = 0;
                }
            }
            return true;
            // TODO return something else
        }
    }
	
	class Machine
	{
		public bool[] LightTargets;
		public List<List<short>> Buttons;
		public List<short> JoltageReqs;

		public Machine(bool[] lightTargets, List<List<short>> buttons, List<short> joltageReqs)
		{
			LightTargets = lightTargets;
			Buttons = buttons.OrderByDescending(b => b.Count).ToList();
			JoltageReqs = joltageReqs;
		}
	}
}