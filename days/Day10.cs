using System.Text.RegularExpressions;

namespace AoC2025;

public class Day10
{
	public (string, string) Run(List<string> lines)
	{
		// parse machines
		lines = lines.Where(l => !l.StartsWith('#')).ToList();
		List<Machine> machines = lines.Select(CreateMachine).ToList();

		int part1 = 0;
		ParallelOptions options = new ParallelOptions { MaxDegreeOfParallelism = 8 };
		Parallel.ForEach(machines, options, m => {
			Interlocked.Add(ref part1, GetMinLightButtonClicks(m)); 
		});
		
		int part2 = 0;
		
		// Parallel.ForEach(machines, m => {
			// Interlocked.Add(ref part2, GetMinJoltageButtonClicks(m));
		// });

		foreach (Machine m in machines)
		{
			int result = GetMinJoltageButtonClicksViaEquations(m);
			Console.WriteLine($"-> {string.Join(',', m.JoltageReqs)}: {result}");
			part2 += result;
		}
		
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
		machine.Buttons.ForEach(btn => queue.Enqueue((1, btn, new bool[machine.JoltageReqs.Count])));

		while (queue.Count > 0)
		{
			(int depth, List<short> button, bool[] lights) next = queue.Dequeue();
			int hash = LightHash(next.lights);
			
			// skip if we already visited this lights combo
			if (visited.TryGetValue(hash, out int prevDepth) && prevDepth < next.depth - 1) continue;

			// press button
			next.button.ForEach(btn => next.lights[btn] = !next.lights[btn]);
			if (next.lights.SequenceEqual(machine.LightTargets)) {
				return next.depth;
			}

			foreach (List<short> btn in machine.Buttons)
			{
				if (btn.SequenceEqual(next.button)) continue; // don't press same button twice in a row
				queue.Enqueue((next.depth + 1, btn, (bool[])next.lights.Clone()));
			}

			if (!visited.ContainsKey(hash))
				visited.Add(hash, next.depth);
		}

		return -1;
	}

	// BFS solution: way too slow and memory intensive to ever complete :(
	int GetMinJoltageButtonClicks(Machine machine)
	{
		// loop ALL combinations, BFS style
		// so we know that first result is optimal one

		Dictionary<int, short> visited = new();

		Queue<(short depth, short button, short[] joltages)> queue = new();
		machine.Buttons.ForEach(btn => queue.Enqueue((1, (short)machine.Buttons.IndexOf(btn), new short[machine.JoltageReqs.Count])));

		while (queue.Count > 0)
		{
			(short depth, short button, short[] joltages) next = queue.Dequeue();

			// skip if we already visited this joltages combo
			if (visited.TryGetValue(JoltageHash(next.joltages), out short prevDepth) && prevDepth < next.depth - 1) continue;

			// press button
			machine.Buttons[next.button].ForEach(btn => next.joltages[btn]++);

			// have we found it??
			if (next.joltages.SequenceEqual(machine.JoltageReqs))
			{
				Console.WriteLine($"{next.depth}: {String.Join(',', next.joltages)}");
				return next.depth;
			}

			bool tooFar = false;
			foreach (List<short> btn in machine.Buttons)
				if (next.joltages.Select((jolt, i) => jolt > machine.JoltageReqs[i] && btn.Contains((short)i)).Any(el => el))
					tooFar = true;
			if (tooFar) continue;

			if (visited.ContainsKey(JoltageHash(next.joltages)))
				continue;

			foreach (List<short> btn in machine.Buttons)
			{
				// filter: stop here if any voltage is over the required setting
				if (next.joltages.Select((jolt, i) => jolt >= machine.JoltageReqs[i] && btn.Contains((short)i)).Any(el => el)) continue;
				queue.Enqueue(((short)(next.depth + 1), (short)machine.Buttons.IndexOf(btn), (short[])next.joltages.Clone()));
			}

			visited.Add(JoltageHash(next.joltages), next.depth);
		}

		return -1;
	}

	int LightHash(bool[] lights)
	{
		int hc = lights.Length;
		foreach (bool val in lights)
		{
			hc = unchecked(hc * 10 + (val?2:1));
		}
		return hc;
		// return lights.Select((l, i) => l ? (int)Math.Pow(10, i) : 0).Sum();
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

	int GetMinJoltageButtonClicksViaEquations(Machine machine)
	{
		int minResult = machine.JoltageReqs.Max(); 
		int buttonPressesGuess = machine.JoltageReqs.Max(); 

		// get equations, as int arrays
		int rowCount = Math.Max(machine.Buttons.Count, machine.JoltageReqs.Count);
		List<int[]> equations = GetEquations(machine, rowCount + 1);

		int missingEqs = rowCount - equations.Count;
		if (missingEqs > 0)
		{
			Console.WriteLine("Missing equations: " + missingEqs);
			
			// Add requirement/equation that all buttons must add up to the total
			// which we will guess, and try different values
			int[] totalRow = Enumerable.Range(0, rowCount+1).Select(indx => 1).ToArray();
			totalRow[^1] = buttonPressesGuess;
			equations.Add(totalRow);
			
			// For the other missing equations: make a guess for a single button (per missing equation)
			for (int i = 0; i < missingEqs-1; i++)
			{
				int[] newRow = Enumerable.Range(0, rowCount+1).Select(indx => indx == i ? 1 : 0).ToArray();
				equations.Add(newRow);
			}

			// get combinations
			List<int[]> combos = new List<int[]>();
			Utils.GenerateCombinationsHelper(new int[missingEqs-1], 0, 0, machine.JoltageReqs.Max(), missingEqs-1, combos);
			
			while (buttonPressesGuess < machine.JoltageReqs.Sum(j=>j)) // TODO how many is enough???
			{
				// Console.WriteLine(string.Join(',', machine.JoltageReqs) + " -- end guess: " + buttonPressesGuess);
				// update 'total' equation with new total guess
				equations[equations.Count - 1 - (missingEqs - 1)][^1] = buttonPressesGuess;

				if (missingEqs == 1)
				{
					// we only need to loop over our total button presses guess
					if (TrySolve(equations, minResult, machine, out int result))
						return result;
				}
				else
				{
					// now loop combinations
					for (int i = 1; i < combos.Count; i++)
					{
						for (int j = 0; j < missingEqs-1; j++)
						{
							equations[equations.Count-1-j][^1] = combos[i][j];
							if (TrySolve(equations, minResult, machine, out int result))
							{
								// TODO result or buttonPressGuesses?
								return result;
							}
						}
					}
				}
				
				buttonPressesGuess++;
			}
		}
		else if (missingEqs < 0)
		{
			Console.WriteLine(" ERROR ");
			if (TrySolve(equations, minResult, machine, out int result)) return result;
		}
		else if (missingEqs == 0)
		{
			if (TrySolve(equations, minResult, machine, out int result)) return result;
		}

		return - 1;
	}

	// Build equations based on buttons upping the joltages
	// b1 + b2 + b3 + b4 = joltage
	// per joltage req. Coefficient 1 per button that influences the joltage
	List<int[]> GetEquations(Machine machine, int cols)
	{
		List<int[]> equations = new List<int[]>();
		for (int i = 0; i < machine.JoltageReqs.Count; i++)
		{
			int[] equation = new int[cols];

			// for each button upping this joltage
			for (int j = 0; j < machine.Buttons.Count; j++)
			{
				if (machine.Buttons[j].Contains((short)i))
				{
					equation[j] = 1;
				}
			}
			equation[cols - 1] = machine.JoltageReqs[i];

			// make sure not to add duplicate equations (they are useless)
			if (!equations.Any(eq => eq.SequenceEqual(equation)))
				equations.Add(equation);
		}
		return equations;
	}

	// modified to extrac total of all vars added up
	bool TrySolve(List<int[]> equations, int minResult, Machine machine, out int result)
	{
		result = 0;

		// build matrix with equations and run it
		int rowCount = Math.Max(machine.Buttons.Count, machine.JoltageReqs.Count);

		// last equation => a+b+c+d+...n = X (we loop until we find a valid system of equations)
		double[,] eqMatrix = new double[rowCount, rowCount + 1];

		// step 1: main equations of buttons mapping to joltage reqs
		for (int i = 0; i < equations.Count; i++)
		{
			for (int j = 0; j < equations[i].Length; j++)
			{
				eqMatrix[i, j] = equations[i][j];
			}
		}


		/*
		 // TODO re-enable this
		// missing lines
		// amount: buttons - (joltageReqs + 1)
		// for each, choose a random button, try all values starting from 0?
		for (int i = 0; i < buttonVals.Count; i++)
		{
			eqMatrix[machine.JoltageReqs.Count + i, i+1] = 1;
			eqMatrix[machine.JoltageReqs.Count + i, machine.Buttons.Count] = buttonVals[i];
		}
		*/
		/*
		if (totalBtnPrssGuess > 0)
		{
			// last line: all buttons together must add up to the button clicks
			// we set this value ourselves (starting with minimum), and increment until first valid one found
			for (int i = 0; i < machine.Buttons.Count; i++)
			{
				eqMatrix[eqMatrix.GetLength(0) - 1, i] = 1;
			}
			eqMatrix[eqMatrix.GetLength(0) - 1, eqMatrix.GetLength(1) - 1] = totalBtnPrssGuess;
		}
		*/

		// Utils.PrintArray(eqMatrix);
		//Console.WriteLine("--");

		var solver = new GaussianEliminationSolver(eqMatrix);

		// Solve the system of equations.
		// solver.PrintSteps();
		double[] solution = solver.SolveSystem();

		eqMatrix = solver.Matrix;

		//Utils.PrintArray(eqMatrix);
		/*
		// Print the solution.
		Console.WriteLine("Solution:");
		for (int i = 0; i < solution.Length; i++)
		{
			Console.WriteLine($"x{i + 1} = {solution[i]}");
		}
		Console.WriteLine();
		*/


		result = (int)Math.Round(solution.Sum());

		int bottomRows = Math.Abs(machine.JoltageReqs.Count - machine.Buttons.Count);
		if (bottomRows == 0) return true; // no 'missing rows', should always get a valid result

		for (int i = 0; i < bottomRows; i++)
		{
			int row = eqMatrix.GetLength(0) - 1 - i;
			// check full row
			// must either be ALL zeros
			// OR 1 on pivot (ignore last value / solution of equation)
			bool allZero = Enumerable.Range(0, eqMatrix.GetLength(1)-1).All(col => Math.Abs(eqMatrix[row, col]) <= 0.00001);
			bool pivot1 = Math.Abs(eqMatrix[row, row] - 1) <= 0.00001;
			if (!allZero && !pivot1) return false;
		}
		
		// TODO should all solutions be positive now?
		// test for all examples
		
		return result >= minResult && solution.All(val => val >= -0.00001);
	}
}




// 
// Represents a system of linear equations that can be solved using Gaussian elimination.
// The class provides methods to solve the system and print the steps in detail.
// 
public class GaussianEliminationSolver
{
	private double[,] matrix;
	private int rowCount;
	private int columnCount;
        
	public double[,] Matrix => matrix;
        
	// <summary>
	// Constructs a new GaussianEliminationSolver with the provided matrix.
	//
	// Parameters:
	// - inputMatrix: The matrix representing the system of linear equations.
	// </summary>
	public GaussianEliminationSolver(double[,] inputMatrix)
	{
		matrix = inputMatrix;
		rowCount = matrix.GetLength(0);
		columnCount = matrix.GetLength(1);
	}
    
	// <summary>
	// Solves the system of linear equations using Gaussian elimination.
	//
	// Returns:
	// - An array of doubles representing the solution to the system of equations.
	// </summary>
	public double[] SolveSystem()
	{
		// Perform Gaussian elimination.
		for (int pivotRow = 0; pivotRow < rowCount; pivotRow++)
		{
			// Find the pivot element.
			int pivotColumn = pivotRow;
			double pivotElement = matrix[pivotRow, pivotColumn];


			// check for zero coefficients
			if (pivotElement == 0)
			{
				// find non-zero coefficient
				int swapRow = pivotColumn + 1;
				for (; swapRow < rowCount; swapRow++)
					if (matrix[swapRow, pivotColumn] != 0)
						break;
	            
				if (swapRow < rowCount && (matrix[swapRow, pivotColumn] != 0)) // found a non-zero coefficient?
				{
					// Console.WriteLine($"non zero: {pivotColumn}, swapping with {swapRow}");
						
					// yes, then swap it with the above
					double[] tmp = new double[rowCount + 1];
					for (int i = 0; i < rowCount + 1; i++)
					{
						tmp[i] = matrix[swapRow, i];
						matrix[swapRow, i] = matrix[pivotColumn, i];
						matrix[pivotColumn, i] = tmp[i];
					}
				}
			}
            
			pivotElement = matrix[pivotRow, pivotColumn];
			if (pivotElement == 0) continue;
            
			// Scale the pivot row.
			for (int j = pivotColumn; j < columnCount; j++)
			{
				matrix[pivotRow, j] /= pivotElement;
			}

			// Eliminate other rows.
			for (int i = 0; i < rowCount; i++)
			{
				if (i == pivotRow) continue;
				double factor = matrix[i, pivotColumn];
				// if (factor == 0) continue;

				for (int j = pivotColumn; j < columnCount; j++)
				{
					matrix[i, j] -= factor * matrix[pivotRow, j];
				}
			}
		}

		// Back substitution to find the solution.
		double[] solution = new double[rowCount];
		solution[rowCount - 1] = matrix[rowCount - 1, columnCount - 1];

		for (int i = rowCount - 2; i >= 0; i--)
		{
			double sum = 0;

			for (int j = i + 1; j < columnCount - 1; j++)
			{
				sum += matrix[i, j] * solution[j];
			}

			solution[i] = matrix[i, columnCount - 1] - sum;
		}

		return solution;
	}

	// <summary>
	// Prints the steps of Gaussian elimination in detail.
	// </summary>
	public void PrintSteps()
	{
		Console.WriteLine("Gaussian Elimination Steps:");

		for (int pivotRow = 0; pivotRow < rowCount; pivotRow++)
		{
			Console.WriteLine($"Step {pivotRow + 1}:");

			// Print the current matrix.
			for (int i = 0; i < rowCount; i++)
			{
				for (int j = 0; j < columnCount; j++)
				{
					Console.Write($"{matrix[i, j],-10}");
				}

				Console.WriteLine();
			}

			Console.WriteLine();

			int pivotColumn = pivotRow;
			// Find the pivot element.
			double pivotElement = matrix[pivotRow, pivotColumn];
            

			// check for zero coefficients
			if (pivotElement == 0)
			{
				// find non-zero coefficient
				int swapRow = pivotColumn + 1;
				for (; swapRow < rowCount; swapRow++)
					if (matrix[swapRow, pivotColumn] != 0)
						break;
	            
				if (swapRow < rowCount && (matrix[swapRow, pivotColumn] != 0)) // found a non-zero coefficient?
				{
					Console.WriteLine($"non zero: {pivotColumn}, swapping with {swapRow}");
						
					// yes, then swap it with the above
					double[] tmp = new double[rowCount + 1];
					for (int i = 0; i < rowCount + 1; i++)
					{
						tmp[i] = matrix[swapRow, i];
						matrix[swapRow, i] = matrix[pivotColumn, i];
						matrix[pivotColumn, i] = tmp[i];
					}
				}
			}
            
			pivotElement = matrix[pivotRow, pivotColumn];
			if (pivotElement == 0) continue;

			// Scale the pivot row.
			for (int j = pivotColumn; j < columnCount; j++)
			{
				matrix[pivotRow, j] /= pivotElement;
			}
            
			// Eliminate other rows.
			for (int i = 0; i < rowCount; i++)
			{
				if (i == pivotRow) continue;
				double factor = matrix[i, pivotColumn];
				//if (matrix[i, i] != 0 && (matrix[i, i] - factor * matrix[pivotRow, i]) == 0) continue;

				for (int j = pivotColumn; j < columnCount; j++)
				{
					matrix[i, j] -= factor * matrix[pivotRow, j];
				}
			}
		}

		// Print the current matrix.
		Console.WriteLine($"Result:");
		for (int i = 0; i < rowCount; i++)
		{
			for (int j = 0; j < columnCount; j++)
			{
				Console.Write($"{matrix[i, j],-10}");
			}

			Console.WriteLine();
		}
        
		Console.WriteLine("Gaussian Elimination Completed.");
	}
}
    
// TODO REMOVE
// Gaussian Elimination based on https://www.codeproject.com/tips/388179/linear-equation-solver-gaussian-elimination-csharp
public static class LinearEquationSolver
{
	/// <summary>Computes the solution of a linear equation system.</summary>
	/// <param name="M">
	/// The system of linear equations as an augmented matrix[row, col] where (rows == cols + 1).
	/// It will contain the solution in "row canonical form" if the function returns "true".
	/// </param>
	/// <returns>Returns whether the matrix has a unique solution or not.</returns>
	public static bool Solve(float[,] M)
	{
		// input checks
		int rowCount = M.GetUpperBound(0) + 1;
		if (M == null || M.Length != rowCount * (rowCount + 1))
			throw new ArgumentException("The algorithm must be provided with a (n x n+1) matrix.");
		if (rowCount < 1)
			throw new ArgumentException("The matrix must at least have one row.");

		// pivoting
		for (int col = 0; col + 1 < rowCount; col++)
		{
			// check for zero coefficients
			if (M[col, col] == 0)
			{
				// find non-zero coefficient
				int swapRow = col + 1;
				for (; swapRow < rowCount; swapRow++)
					if (M[swapRow, col] != 0)
						break;

				// TODO FIX THIS
				//if (swapRow >= rowCount) continue;
				if (swapRow < rowCount && (M[swapRow, col] != 0)) // found a non-zero coefficient?
				{
					Console.WriteLine($"non zero: {col}, swapping with {swapRow}");
						
					// yes, then swap it with the above
					float[] tmp = new float[rowCount + 1];
					for (int i = 0; i < rowCount + 1; i++)
					{
						tmp[i] = M[swapRow, i];
						M[swapRow, i] = M[col, i];
						M[col, i] = tmp[i];
					}
				}
				//else return false; // no, then the matrix has no unique solution
			}
		}

		Utils.PrintArray(M);
		Console.WriteLine("--");
			
		// elimination
		for (int sourceRow = 0; sourceRow + 1 < rowCount; sourceRow++)
		{
			for (int destRow = sourceRow + 1; destRow < rowCount; destRow++)
			{
				float df = M[sourceRow, sourceRow];
				if (df == 0) continue;
				float sf = M[destRow, sourceRow];
				if (sf == 0) continue;
					
				Console.WriteLine($"eliminating {sourceRow}:{destRow}:{df}:{sf}");
				for (int i = 0; i < rowCount + 1; i++)
					M[destRow, i] = M[destRow, i] * df - M[sourceRow, i] * sf;
			}
		}
			
		Utils.PrintArray(M);
		Console.WriteLine("--");

		// back-insertion
		for (int row = rowCount - 1; row >= 0; row--)
		{
			float f = M[row,row];
			if (f == 0) return false;

			for (int i = 0; i < rowCount + 1; i++) M[row, i] /= f;
			for (int destRow = 0; destRow < row; destRow++)
			{
				M[destRow, rowCount] -= M[destRow, row] * M[row, rowCount]; 
				M[destRow, row] = 0;
			}
		}
		return true;
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
		Buttons = buttons; //OrderByDescending(b => b.Count).ToList();
		JoltageReqs = joltageReqs;
	}
}