using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace AoC2025;

public class Day10
{
	public (string, string) Run(List<string> lines)
	{
		return RunInput(lines, true, false);
	}
	
	(string, string) RunInput(List<string> lines, bool multiThreading, bool printResults)
	{
		// parse machines
		lines = lines.Where(l => !l.StartsWith('#')).ToList();
		List<Machine> machines = lines.Select((line, index) => CreateMachine(line, index)).ToList();

		int part1 = 0;
		ParallelOptions options = new ParallelOptions { MaxDegreeOfParallelism = 8 };
		Parallel.ForEach(machines, options, m => {
			Interlocked.Add(ref part1, GetMinLightButtonClicks(m)); 
		});
		
		int part2 = 0;
		if (multiThreading) {
			Parallel.ForEach(machines, m => {
				int result = GetMinJoltageButtonClicksViaEquations(m);
				m.Result = result;
				Interlocked.Add(ref part2, result); 
			});
		}
		else {
			foreach (Machine m in machines) {
				int result = GetMinJoltageButtonClicksViaEquations(m);
				m.Result = result;
				part2 += result;
			}
		}

		if (printResults) {
			foreach (Machine m in machines) {
				Console.WriteLine($"{m.Nr}: {m.Result}");
			}
		}

		return (part1.ToString(), part2.ToString());
	}
	
	Machine CreateMachine(string line, int index)
	{
		// example: [.##.] (3) (1,3) (2) (2,3) (0,2) (0,1) {3,5,4,7}
		Match m1 = Regex.Match(line, @"\[([\.#]+)\]");
		MatchCollection m2 = Regex.Matches(line, @"\(([0-9,]+)\)");
		Match m3 = Regex.Match(line, @"{(.*)}");

		bool[] lightTargets = m1.Groups[1].Value.Select(c => c == '#').ToArray();
		List<List<short>> buttons = m2.Select(m => m.Groups[1].Value.Split(',').Select(short.Parse).ToList()).ToList();
		List<short> joltageReqs = m3.Groups[1].Value.Split(',').Select(short.Parse).ToList();

		return new Machine(index+1, lightTargets, buttons, joltageReqs);
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
				// Console.WriteLine($"{next.depth}: {String.Join(',', next.joltages)}");
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
		// string join was too slow
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
		// Console.WriteLine($"Rank: {rank}, buttons: {machine.Buttons.Count}, equations: {equations.Count}");

		// Utils.PrintArray(eqMatrix);
		
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
			if (TrySolve(eqMatrix, minResult, true, out int result)) return result;
		}
		else if (missingEqs == 1)
		{
			// Add requirement/equation that all buttons must add up to the total
			// which we will guess, and try different values
			int[] totalRow = Enumerable.Range(0, rowCount + 1).Select(_ => 1).ToArray();
			totalRow[^1] = buttonPressesGuess;
			equations.Add(totalRow);

			while (buttonPressesGuess <= machine.JoltageReqs.Sum(j => j)) 
			{
				// Console.WriteLine(string.Join(',', machine.JoltageReqs) + " -- end guess: " + buttonPressesGuess);
				// update 'total' equation with new total guess
				equations[equations.Count - 1 - (missingEqs - 1)][^1] = buttonPressesGuess;

				// we only need to loop over our total button presses guess
				if (TrySolve(BuildMatrix(equations), minResult, true, out int result))
					return result;
				
				buttonPressesGuess++;
			}
		}
		
		//
		// missingEqs > 1 OR didn't work with total button guess
		//
	
		// try solve, purely to get the free vars from it
		var freeVarsM = GetFreeVars(eqMatrix, true);
		
		List<int> buttonMaxes = Enumerable.Range(0, machine.Buttons.Count).Select(b => equations.Min(e => e[b] == 1 ? e[^1] : minResult)).ToList();
		
		// Utils.PrintArray(freeVarMatrix);
		// Console.WriteLine("Free vars: " + string.Join(",", freeVarsM));
		
		// For the missing equations: make a guess for a single button (per missing equation)
		eqMatrix = Utils.ResizeArray(eqMatrix, eqMatrix.GetLength(0)+missingEqs, eqMatrix.GetLength(1));
		
		// get free var combinations
		IList<IList<int>> freeVarCombos = Utils.DoPermute(freeVarsM.ToArray(), 0, freeVarsM.Count - 1, new List<IList<int>>());
		int freeVarComIndex = 0;
		
		// get free far value combinations
		List<int[]> combos = new List<int[]>();
		// the 4/5 reduction is an optimization by guessing/trying. The range could be calculated per machine, but is more complicated
		Utils.GenerateCombinationsHelper(new int[missingEqs], 0, 0, machine.JoltageReqs.Max() * 4/5, missingEqs, combos);
		
		int min = int.MaxValue;
		while (min == int.MaxValue && freeVarComIndex < freeVarCombos.Count)
		{
			IList<int> freeVarCombo = freeVarCombos[freeVarComIndex++];
			int[] freeVarMaxes = new int[missingEqs];
			
			for (int i = 0; i < missingEqs; i++)
			{
				int var = i < freeVarCombo.Count ? freeVarCombo[i] : NextRandom(rnd, machine.Buttons.Count, freeVarCombo); 
				freeVarMaxes[i] = buttonMaxes[var];
				for (int j = 0; j < eqMatrix.GetLength(1); j++) {
					eqMatrix[eqMatrix.GetLength(0) - 1 - i, j] = var == j ? 1 : 0;
				}
			}
			
			// loop value combinations
			for (int i = 0; i < combos.Count; i++)
			{
				bool skip = false;
				// update matrix with new freeVar values from combo
				for (int j = 0; j < missingEqs; j++) {
					eqMatrix[eqMatrix.GetLength(0)-1-j, eqMatrix.GetLength(1)-1] = combos[i][j];
					if (combos[i][j] > freeVarMaxes[j]) skip = true; // stop if we are over the max button amount
				}
				if (skip) continue;
				
				// Console.WriteLine($"Solution {string.Join(',', combos[i])}:");
				if (TrySolve(eqMatrix, minResult, true, out int result)) {
					if (result < min) min = result;
				}
			}
		}
		
		return min;
	}

	int NextRandom(Random rnd, int max, IList<int> exclude)
	{
		int next = exclude[0];
		while (exclude.Contains(next))
		{
			next = rnd.Next(0, max);
		}
		return next;
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
	
	List<int> GetFreeVars(double[,] inputMatrix, bool reduceFully)
	{
		// build matrix with equations and run it
		// int rowCount = Math.Max(machine.Buttons.Count, machine.JoltageReqs.Count);
		int rowCount = inputMatrix.GetLength(0);
		double[,] eqMatrix = inputMatrix.Clone() as double[,];

		// Utils.PrintArray(eqMatrix);
		// Console.WriteLine("--");

		var solver = new GaussianEliminationSolver(eqMatrix);

		// Solve the system of equations.
		solver.SolveSystem(reduceFully);
		double[,] outputMatrix = solver.Matrix;

		List<int> freeVars = new List<int>();
		
		for (int i = 0; i < outputMatrix.GetLength(1) - 1; i++)
		{
			// check columns for non-one or zero values
			int ones = 0;
			for (int j = 0; j < rowCount; j++)
			{
				if (i == j && outputMatrix[j, i] == 0 && !freeVars.Contains(i))
					freeVars.Add(i);
				else if (outputMatrix[j, i] != 0 && outputMatrix[j, i] != 1 && !freeVars.Contains(i))
				{
					// potential free var, but we need to check one more thing:
					// it could be we have an answer, aka: on its row all values are zero except the pivot (1) and the value (an integer)
					if (!CheckValue(outputMatrix, i))
						freeVars.Add(i);
				}
				else if (outputMatrix[j, i] == 1)
					ones++;
			}
			if (ones > 1 &&  !freeVars.Contains(i))
				freeVars.Add(i); // multiple ones per column, free var
		}
		
		return freeVars;
	}

	// modified to extract total of all vars added up
	bool TrySolve(double[,] inputMatrix, int minResult, bool reduceFully, out int result)
	{
		result = 0;

		// build matrix with equations and run it
		int rowCount = inputMatrix.GetLength(0);
		double[,] eqMatrix = (inputMatrix.Clone() as double[,])!;
		
		var solver = new GaussianEliminationSolver(eqMatrix);

		// Solve the system of equations.
		double[] solution = solver.SolveSystem(reduceFully);
		double[,] outputMatrix = solver.Matrix;

		// Utils.PrintArray(outputMatrix);
		
		// Console.WriteLine($"Solution {equations[outputMatrix.GetLength(0)-1][outputMatrix.GetLength(1)-1]}:");
		// for (int i = 0; i < solution.Length; i++)
		// {
			// Console.Write($"{solution[i]} ");
		// }
		// Console.WriteLine();

		result = (int) Math.Round(solution.Sum());

		int bottomRows = inputMatrix.GetLength(1) - rowCount - 1;
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
				return false;
			}
		}
		
		bool valid = result >= minResult && solution.All(val => val >= -0.00001 && Double.IsInteger(Math.Round(val, 3)));
		if (valid)
		{
			/*
			Console.WriteLine($"{solution.Sum()} - Solution: ");
			for (int i = 0; i < solution.Length; i++) {
				Console.Write($"{solution[i]} ");
			}
			Console.WriteLine();
			*/
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
		// last solution value is bottom right matrix val
		int lastVarRow = Math.Min(rowCount - 1, solution.Length - 1);
		solution[lastVarRow] = matrix[rowCount - 1, columnCount - 1];

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
	public int Nr;
	public int Result;
	public bool[] LightTargets;
	public List<List<short>> Buttons;
	public List<short> JoltageReqs;

	public Machine(int nr, bool[] lightTargets, List<List<short>> buttons, List<short> joltageReqs)
	{
		Nr = nr;
		LightTargets = lightTargets;
		Buttons = buttons; //OrderByDescending(b => b.Count).ToList();
		JoltageReqs = joltageReqs;
	}
}