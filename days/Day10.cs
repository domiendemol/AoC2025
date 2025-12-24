using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace AoC2025;

public class Day10
{
	public (string, string) Run(List<string> lines)
	{
		// New approach:
		// - check rank. If it matches buttons: proceed as normal
		// - if rank = nrbuttons - 1: try solve with a guessed sum presses row
		// - if not foudn result or rank even lower: try x free vars (with combos): let's try random values
		//		- keep trying until we have result
		// 
		// TODO 

		// Test1(); return ("test", "1");
		return RunInput(lines);
	}

	void Test1()
	{
		// for problem {28,78,54,63,56,39,23,66,68}: works
		// double[,] testMatrix =
		// {
		// 	{1, 0, 0, 0, 0, 0, 0, 0, 0, 0},
		// 	{0, 1, 0, 0, 0, 0, 0, 0, 0, 9},
		// 	{0, 0, 1, 0, 0, 0, 0, 0, 1, 20},
		// 	{0, 0, 0, 1, 0, 0, 0, 0, -3, -37},
		// 	{0, 0, 0, 0, 1, 0, 0, 0, 1, 29},
		// 	{0, 0, 0, 0, 0, 1, 0, 0, 1, 31},
		// 	{0, 0, 0, 0, 0, 0, 1, 0, -1, 1},
		// 	{0, 0, 0, 0, 0, 0, 0, 1, 4, 68},
		// 	{0, 0, 0, 0, 0, 0, 0, 0, 1, 13}
		// };
		
		double[,] testMatrix =
		{
			{0, 1, 0, 1, 0, 1, 0, 0, 0, 0, 1, 56},
			{0, 1, 1, 1, 1, 0, 0, 1, 0, 0, 0, 53},
			{1, 1, 0, 1, 1, 0, 0, 1, 1, 1, 1, 220},
			{0, 0, 1, 1, 0, 1, 0, 1, 1, 0, 0, 189},
			{1, 1, 1, 1, 1, 1, 0, 1, 0, 1, 1, 100},
			{1, 1, 1, 1, 0, 1, 0, 1, 0, 0, 1, 73},
			{1, 0, 1, 1, 0, 1, 0, 0, 0, 1, 0, 48},
			{1, 0, 1, 0, 0, 1, 1, 0, 0, 0, 0, 36},
			{0, 1, 1, 0, 0, 1, 1, 0, 1, 0, 1, 192},
			{0, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 21},
			
			{0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 12},			
			{0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 2},			
		};
		
		/*
		 * SUCCESS! Sum: 247
		   x1 = 9
		   x2 = 8
		   x3 = 1
		   x4 = 12
		   x5 = 25
		   x6 = 24
		   x7 = 2
		   x8 = 7
		   x9 = 145
		   x10 = 2
		   x11 = 12
		 */
		
		Console.WriteLine($"Rank: {CalculateRank(testMatrix)}");

		var solver = new GaussianEliminationSolver(testMatrix);

		// Solve the system of equations.
		solver.PrintSteps(true);
		// solver.SolveSystem(false);
		// double[] solution = solver.SolveSystem();

		testMatrix = solver.Matrix;

		Utils.PrintArray(testMatrix);

		
		// Print the solution.
		// Console.WriteLine("Solution:");
		// for (int i = 0; i < solution.Length; i++)
		// {
		// Console.Write($"{solution[i]} ");
		// }
		// Console.WriteLine();
		// part2 = (int)Math.Round(solution.Sum());
	}

	(string, string) RunInput(List<string> lines)
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
		Random rnd = new Random();
		
		// get equations, as int arrays
		int rowCount = Math.Max(machine.Buttons.Count, machine.JoltageReqs.Count);
		List<int[]> equations = GetEquations(machine, rowCount + 1);
		
		// get rank
		double[,] eqMatrix = BuildMatrix(equations);
		int rank = CalculateRank(eqMatrix);
		Console.WriteLine($"Rank: {rank}, buttons: {machine.Buttons.Count}, equations: {equations.Count}");

		Utils.PrintArray(eqMatrix);
		
		// int missingEqs = rowCount - equations.Count;
		int missingEqs = machine.Buttons.Count - rank;

		if (missingEqs < 0)
		{
			Console.WriteLine("AAAAAARRRR PANIC");
			return -1;
		}
		if (missingEqs == 0)
		{
			// ALL GOOD 
			if (TrySolve(eqMatrix, minResult, true, out int result, out List<int> freeVars, out double[,] matrix)) return result;
		}
		else if (missingEqs == 1)
		{
			Console.WriteLine("Missing equations: " + missingEqs);

			// Add requirement/equation that all buttons must add up to the total
			// which we will guess, and try different values
			int[] totalRow = Enumerable.Range(0, rowCount + 1).Select(indx => 1).ToArray();
			totalRow[^1] = buttonPressesGuess;
			equations.Add(totalRow);

			while (buttonPressesGuess <= machine.JoltageReqs.Sum(j => j)) 
			{
				// Console.WriteLine(string.Join(',', machine.JoltageReqs) + " -- end guess: " + buttonPressesGuess);
				// update 'total' equation with new total guess
				equations[equations.Count - 1 - (missingEqs - 1)][^1] = buttonPressesGuess;

				// we only need to loop over our total button presses guess
				if (TrySolve(BuildMatrix(equations), minResult, true, out int result, out List<int> freeVars, out double[,] matrix))
					return result;
				
				buttonPressesGuess++;
			}
		}
		
		//
		// missingEqs > 1 OR didn't work with total button guess
		//
		Console.WriteLine("Missing equations: " + missingEqs);
	
		// try solve, purely to get the free vars from it
		if (TrySolve(eqMatrix, minResult, true, out int resultTemp, out List<int> freeVarsM, out double[,] freeVarMatrix))
			return resultTemp;
		
		//  use the resulting RRE matrix or the original one to continue??
		//eqMatrix = freeVarMatrix;
		//  remove full 0 row(s)??
		//eqMatrix = Utils.TrimArray(7, eqMatrix);
		
		Utils.PrintArray(freeVarMatrix);
		Console.WriteLine("Free vars: " + string.Join(",", freeVarsM));
		
		// For the missing equations: make a guess for a single button (per missing equation)
		eqMatrix = Utils.ResizeArray(eqMatrix, eqMatrix.GetLength(0)+missingEqs, eqMatrix.GetLength(1));
		for (int i = 0; i < missingEqs; i++)
		{
			int var = i < freeVarsM.Count ? freeVarsM[i] : rnd.Next(0, machine.Buttons.Count);
			Console.WriteLine($"Free var {i}: {var}");
			for (int j = 0; j < eqMatrix.GetLength(1); j++)
			{
				eqMatrix[eqMatrix.GetLength(0) - 1 - i, j] = var == j ? 1 : 0;
			}
		}

		// get combinations
		List<int[]> combos = new List<int[]>();
		Utils.GenerateCombinationsHelper(new int[missingEqs], 0, 0, machine.JoltageReqs.Max(), missingEqs, combos); // TODO machines.Joltages.Max
		
		// TODO DO WE NEED TO TAKE THE MINIUM OF THE COMBINATION RESULTS??
		// or is the the first valid result always correct?
		
		// while loop needed? can we find the free variables?

		// loop combinations
		for (int i = 0; i < combos.Count; i++)
		{
			for (int j = 0; j < missingEqs; j++)
			{
				// equations[equations.Count-1-j][^1] = combos[i][j]; // TODO change matrix
				eqMatrix[eqMatrix.GetLength(0)-1-j, eqMatrix.GetLength(1)-1] = combos[i][j]; 
			}
			
			// Console.WriteLine($"Solution {string.Join(',', combos[i])}:");
			if (TrySolve(eqMatrix, minResult, true, out int result, out List<int> freeVars, out double[,] matr))
			{
				return result;
			}
			// if (combos[i][0] == 2 && combos[i][1] == 12) break;
			// if (combos[i][1] == 2 && combos[i][0] == 12) break;
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

	double[,] BuildMatrix(List<int[]> equations)
	{
		int colCount = equations[0].Length;
		int rowCount = equations.Count;
		
		// last equation => a+b+c+d+...n = X (we loop until we find a valid system of equations)
		double[,] eqMatrix = new double[rowCount, colCount];

		// main equations of buttons mapping to joltage reqs
		for (int i = 0; i < equations.Count; i++)
		{
			for (int j = 0; j < equations[i].Length; j++)
			{
				eqMatrix[i, j] = equations[i][j];
			}
		}

		return eqMatrix;
	}
	
	// modified to extract total of all vars added up
	bool TrySolve(double[,] inputMatrix, int minResult, bool reduceFully, out int result, out List<int> freeVars, out double[,] resultMatrix)
	{
		result = 0;

		// build matrix with equations and run it
		// int rowCount = Math.Max(machine.Buttons.Count, machine.JoltageReqs.Count);
		int rowCount = inputMatrix.GetLength(0);
		double[,] eqMatrix = inputMatrix.Clone() as double[,];
		
		// Utils.PrintArray(eqMatrix);
		// Console.WriteLine("--");

		var solver = new GaussianEliminationSolver(eqMatrix);

		// Solve the system of equations.
		// solver.PrintSteps();
		double[] solution = solver.SolveSystem(reduceFully);
		double[,] outputMatrix = solver.Matrix;
		resultMatrix = outputMatrix;

		// Utils.PrintArray(outputMatrix);
		
		// Print the solution.
		// Console.WriteLine($"Solution {equations[outputMatrix.GetLength(0)-1][outputMatrix.GetLength(1)-1]}:");
		// for (int i = 0; i < solution.Length; i++)
		// {
			// Console.Write($"{solution[i]} ");
		// }
		// Console.WriteLine();

		freeVars = new List<int>();
		result = (int) Math.Round(solution.Sum());

		int bottomRows = inputMatrix.GetLength(1) - rowCount - 1;

		for (int i = 0; i < outputMatrix.GetLength(1)-1; i++)
		{
			// check columns for non-one or zero values
			for (int j = 0; j < rowCount; j++)
			{
				if (outputMatrix[j, i] != 0 && outputMatrix[j, i] != 1 && !freeVars.Contains(i))
				{
					// potential free var, but we need to check one more thing:
					// it could be we have an answer, aka: on its row all values are zero except the pivot (1) and the value (an integer)
					if (!CheckValue(outputMatrix, i))
						freeVars.Add(i);
				}
			}
		}
		
		for (int i = 0; i < bottomRows; i++)
		{
			int row = outputMatrix.GetLength(0) - 1 - i;
			// check full row
			// must either be ALL zeros
			// OR 1 on pivot (ignore last value / solution of equation)
			bool allZero = Enumerable.Range(0, outputMatrix.GetLength(1)-1).All(col => Math.Abs(outputMatrix[row, col]) <= 0.00001);
			bool pivot1 = Math.Abs(outputMatrix[row, row] - 1) <= 0.00001;
			if (!allZero && !pivot1)
			{
				// TODO is the allzero row always the free var one????
				return false;
			}
		}
		
		bool valid =  result >= minResult && solution.All(val => val >= -0.00001);
		if (valid)
		{
			// Print the solution.
			Console.WriteLine($"Solution: ");
			for (int i = 0; i < solution.Length; i++)
			{
				Console.Write($"{solution[i]} ");
			}
			Console.WriteLine();
		}
		
		return valid;
	}

	// Check if we find an 'identity' row in our matrix
	// -> where only the pivot is 1 and the value, so we know the value of this variable
	bool CheckValue(double[,] matrix, int varIndex)
	{
		bool foundValueRow = false; // aka identity row?
		for (int row = 0; row < matrix.GetLength(0) && !foundValueRow; row++)
		{
			if (Math.Abs(matrix[row, varIndex] - 1) > 0.00001) continue;  // pivot must be one
			foundValueRow = true;
			for (int col = 0; col < matrix.GetLength(1); col++)	
			{
				if (col == matrix.GetLength(1) - 1 && matrix[row, col] <= 0) foundValueRow = false; // value must be positive
				if (col != varIndex && col != matrix.GetLength(1) - 1 && matrix[row, col] != 0) foundValueRow = false;
			}
		}
		return foundValueRow;
	}

	static int CalculateRank(double[,] matrix)
	{
		int rows = matrix.GetLength(0);
		int cols = matrix.GetLength(1);
		int rank = 0;

		// Create a copy to avoid modifying the original matrix
		double[,] mat = (double[,])matrix.Clone();
        
		// Small epsilon to handle floating point precision issues
		double epsilon = 1e-10;

		bool[] rowSelected = new bool[rows];

		for (int j = 0; j < cols; j++)
		{
			int i;
			for (i = 0; i < rows; i++)
			{
				if (!rowSelected[i] && Math.Abs(mat[i, j]) > epsilon)
					break;
			}

			if (i != rows)
			{
				rank++;
				rowSelected[i] = true;
                
				// Pivot element
				double pivot = mat[i, j];

				for (int p = 0; p < rows; p++)
				{
					if (p != i && Math.Abs(mat[p, j]) > epsilon)
					{
						double factor = mat[p, j] / pivot;
						for (int k = j; k < cols; k++)
						{
							mat[p, k] -= factor * mat[i, k];
						}
					}
				}
			}
		}

		return rank;
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
	public double[] SolveSystem(bool reduceFully = true)
	{
		// Perform Gaussian elimination.
		for (int pivotRow = 0; pivotRow < columnCount-1 && pivotRow < rowCount; pivotRow++) // rowCount -> columnCount - 1
		{
			// Find the pivot element.
			int pivotColumn = pivotRow;
			double pivotElement = matrix[pivotRow, pivotColumn];

			// check for zero coefficients
			if (Math.Abs(pivotElement) < 0.00001)
			{
				// find non-zero coefficient
				int swapRow = pivotColumn + 1;
				for (; swapRow < rowCount; swapRow++)
					if (matrix[swapRow, pivotColumn] != 0)
						break;
	            
				if (swapRow < rowCount && (matrix[swapRow, pivotColumn] != 0)) // found a non-zero coefficient?
				{
					// yes, then swap it with the above
					// Console.WriteLine($"non zero: {pivotColumn}, swapping row {pivotRow} with {swapRow}");
					double[] tmp = new double[columnCount];
					for (int i = 0; i < columnCount; i++)
					{
						tmp[i] = matrix[swapRow, i];
						matrix[swapRow, i] = matrix[pivotColumn, i];
						matrix[pivotColumn, i] = tmp[i];
					}
				}
			}
            
			pivotElement = matrix[pivotRow, pivotColumn];
			if (pivotColumn >= columnCount) continue;
			if (pivotRow >= rowCount) continue;
			if (Math.Abs(pivotElement) < 0.00001) continue;
            
			// Scale/normalize the pivot row.
			for (int j = pivotColumn; j < columnCount; j++)
			{
				matrix[pivotRow, j] /= pivotElement;
			}

			// Eliminate other rows.
			for (int i = reduceFully ? 0 : pivotRow+1; i < rowCount; i++)
			{
				if (i == pivotRow) continue;
				double factor = matrix[i, pivotColumn];
				if (i >= columnCount-1 && pivotRow == i-1) continue; // CHANGED 2025-12-18
				// if (factor == 0) continue;

				for (int j = pivotColumn; j < columnCount; j++)
				{
					matrix[i, j] -= factor * matrix[pivotRow, j];
				}
			}
		}

		// Back substitution to find the solution.
		double[] solution = new double[columnCount-1];
		solution[columnCount - 2] = matrix[rowCount - 1, columnCount - 1];

		for (int i = rowCount - 2; i >= 0; i--)
		{
			double sum = 0;

			for (int j = i + 1; j < columnCount - 1; j++)
			{
				sum += matrix[i, j] * solution[j];
			}

			solution[i] = (double)Math.Round((Decimal) (matrix[i, columnCount - 1] - sum), 3);
		}

		return solution;
	}

	// <summary>
	// Prints the steps of Gaussian elimination in detail.
	// </summary>
	public void PrintSteps(bool reduceFully = true)
	{
		Console.WriteLine("Gaussian Elimination Steps:");

		for (int pivotRow = 0; pivotRow < columnCount-1; pivotRow++) // CHANGED 2025-12-18 rowCount -> columnCount - 1
		{
			Console.WriteLine($"Step {pivotRow + 1}:");

			// Print the current matrix.
			for (int i = 0; i < rowCount; i++)
			{
				for (int j = 0; j < columnCount; j++)
				{
					Console.Write($"{matrix[i, j],-5}");
				}

				Console.WriteLine();
			}

			Console.WriteLine();

			int pivotColumn = pivotRow;
			if (pivotColumn >= columnCount) continue;
			if (pivotRow >= rowCount) continue;
			// Find the pivot element.
			double pivotElement = matrix[pivotRow, pivotColumn];
            

			// check for zero coefficients
			if (Math.Abs(pivotElement) < 0.00001)
			{
				// find non-zero coefficient
				int swapRow = pivotColumn + 1;
				for (; swapRow < rowCount; swapRow++)
					if (Math.Abs(matrix[swapRow, pivotColumn]) > 0.00001)
						break;
	            
				if (swapRow < rowCount && (matrix[swapRow, pivotColumn] != 0)) // found a non-zero coefficient?
				{
					Console.WriteLine($"non zero: {pivotColumn}, swapping with {swapRow}");
						
					// yes, then swap it with the above
					double[] tmp = new double[columnCount];
					for (int i = 0; i < columnCount; i++)
					{
						tmp[i] = matrix[swapRow, i];
						matrix[swapRow, i] = matrix[pivotColumn, i];
						matrix[pivotColumn, i] = tmp[i];
					}
				}
			}
            
			pivotElement = matrix[pivotRow, pivotColumn];
			if (Math.Abs(pivotElement) < 0.00001) continue;

			// Scale the pivot row.
			for (int j = pivotColumn; j < columnCount; j++)
			{
				matrix[pivotRow, j] /= pivotElement;
			}
            
			// Eliminate other rows.
			for (int i = reduceFully ? 0 : pivotRow + 1; i < rowCount; i++)
			{
				if (i == pivotRow) continue;
				if (i >= columnCount-1 && pivotRow == i-1) continue; // CHANGED 2025-12-18
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
				Console.Write($"{matrix[i, j],-5}");
			}

			Console.WriteLine();
		}
        
		Console.WriteLine("Gaussian Elimination Completed.");
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