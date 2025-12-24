using System.Collections.Concurrent;
using System.Runtime.Intrinsics.X86;
using System.Text;

namespace AoC2025;

public static class Utils
{
    public static string ReplaceAtIndex(this string text, int index, char c)
    {
        var stringBuilder = new StringBuilder(text);
        stringBuilder[index] = c;
        return stringBuilder.ToString();
    }
    
    public static char[,] RotateBy90(char[,] grid) 
    {
        char[,] rotated = new char[grid.GetLength(1), grid.GetLength(0)];

        // rotate values
        for(int j = 0; j < grid.GetLength(1); j++) {
            for(int i = 0; i < grid.GetLength(0); i++) {
                rotated[i,j] = grid[j,i];
            }
        }

        return rotated;
    }
    
    public static long Factorial(int n)
    {
        if (n < 0) throw new ArgumentException("Input should be a non-negative integer.");

        long result = 1;
        for (int i = 2; i <= n; i++) {
            result *= i;
        }

        return result;
    }
    
    	
    // is there a better way?
    public static char[,] ToCharArray(List<string> input)
    {
        char[,] tempShape = new char[input.Count, input[0].Length];
        for(int j = 0; j < input.Count; j++) {
            for(int i = 0; i < input[j].Length; i++) {
                tempShape[j,i] = input[j][i];
            }
        }
        return tempShape;
    }
    
    public static int[,] ToIntArray(List<string> input)
    {
        int[,] tempShape = new int[input[0].Length,input.Count];
        for(int j = 0; j < input.Count; j++) {
            for(int i = 0; i < input[j].Length; i++) {
                tempShape[i,j] = (int) char.GetNumericValue(input[i][j]);
            }
        }
        return tempShape;
    }
    
    public static T? TryGetValue<T>(this T[,] array, int x, int y, T defaultValue = default)
    {
        if (x < 0 || x >= array.GetLength(0) || y < 0 || y >= array.GetLength(1)) return defaultValue;
        return array[x,y];
    }
    
    public static T? TryGetValue<T>(this T[] array, int x, T defaultValue = default)
    {
        if (x < 0 || x >= array.GetLength(0)) return defaultValue;
        return array[x];
    }
    
    public static T? TryGetValue<T>(this List<T> list, int x, T defaultValue = default)
    {
        if (x < 0 || x >= list.Count) return defaultValue;
        return list[x];
    }

    public static void PrintCharArray(char[,] grid)
    {
        for (int i = 0; i < grid.GetLength(0); i++) {
            for (int j = 0; j < grid.GetLength(1); j++) {
                Console.Write(grid[i, j]);
            }
            Console.WriteLine();
        }
        Console.WriteLine();
    }
    
    public static void PrintArray<T>(T[,] grid)
    {
        for (int i = 0; i < grid.GetLength(0); i++) {
            for (int j = 0; j < grid.GetLength(1); j++) {
                Console.Write(grid[i, j]);
                Console.Write(" ");
            }
            Console.WriteLine();
        }
        // Console.WriteLine();
    }

    public static Vector2Int FindIndex<T>(T[,] grid, T obj)
    {
        for (int i = 0; i < grid.GetLength(0); i++)
        {
            for (int j = 0; j < grid.GetLength(1); j++)
            {
                if (obj.Equals(grid[i, j])) return new Vector2Int(i, j);
            }
        }
        return new Vector2Int(-1, -1);
    }

    // handles negative numbers
    public static int Mod(int x, int m)
    {
        if (x < 0) x += m;
       return (x%m + m)%m;
    }
    
    public static long GCF(long a, long b)
    {
        while (b != 0)
        {
            long temp = b;
            b = a % b;
            a = temp;
        }
        return a;
    }

    public static long LCM(long a, long b)
    {
        return (a / GCF(a, b)) * b;
    }
    
    public static void AddRange<T>(this ConcurrentBag<T> @this, IEnumerable<T> toAdd)
    {
        foreach (var element in toAdd)
        {
            @this.Add(element);
        }
    }
    
    /// <summary>
    /// Projects each element of a sequence into a new form by incorporating the element's index.
    /// </summary>
    /// <typeparam name="T">The type of the elements of the array.</typeparam>
    /// <param name="array">A sequence of values to invoke the action on.</param>
    /// <param name="action">An action to apply to each source element; the second parameter of the function represents the index of the source element.</param>
    // example: foo.ForEach<string>((value, coords) => Console.WriteLine("(" + String.Join(", ", coords) + $")={value}"));
    public static void ForEach<T>(this Array array, Action<T, int[]> action)
    {
        var dimensionSizes = Enumerable.Range(0, array.Rank).Select(i => array.GetLength(i)).ToArray();
        ArrayForEach(dimensionSizes, action, new int[] { }, array);
    }
    private static void ArrayForEach<T>(int[] dimensionSizes, Action<T, int[]> action, int[] externalCoordinates, Array masterArray)
    {
        if (dimensionSizes.Length == 1)
            for (int i = 0; i < dimensionSizes[0]; i++)
            {
                var globalCoordinates = externalCoordinates.Concat(new[] { i }).ToArray();
                var value = (T)masterArray.GetValue(globalCoordinates);
                action(value, globalCoordinates);
            }
        else
            for (int i = 0; i < dimensionSizes[0]; i++)
                ArrayForEach(dimensionSizes.Skip(1).ToArray(), action, externalCoordinates.Concat(new[] { i }).ToArray(), masterArray);
    }

    public static void PopulateArray<T>(this Array array, Func<int[], T> calculateElement)
    {
        array.ForEach<T>((element, indexArray) => array.SetValue(calculateElement(indexArray), indexArray));
    }
    
    // TODO option to check diagonally or not
    public static List<T> GetNeighbourValues<T>(T[,] inputGrid, int[] pos)
    {
        List<T> neighbours = new List<T>();
        for (int i = -1; i <= 1; i++) { 
            for (int j = -1; j <= 1; j++)
            {
                if ((i == 0 && j == 0)) continue; // (i != 0 && j != 0)
                T value = inputGrid.TryGetValue<T>(pos[0] + i, pos[1] + j, default);
                if (value == null || value.Equals(default)) continue;
                neighbours.Add(value);
            }
        }
        return neighbours;
    }
    
    // TODO option to check diagonally or not
    public static List<int[]> GetNeighbours<T>(T[,] inputGrid, int[] pos)
    {
        List<int[]> neighbours = [];
        for (int i = -1; i <= 1; i++) { 
            for (int j = -1; j <= 1; j++)
            {
                if ((i == 0 && j == 0)) continue; // (i != 0 && j != 0)
                T? value = inputGrid.TryGetValue<T>(pos[0] + i, pos[1] + j, default(T));
                if (value == null || value.Equals(default(T))) continue;
                neighbours.Add([pos[0] + i, pos[1] + j]);
            }
        }
        return neighbours;
    }
    
    public static void GenerateCombinationsHelper(int[] current, int index, int min, int max, int count, List<int[]> result)
    {
        if (index == count)
        {
            // Copy the current combination to avoid modifying the original array
            int[] combo = new int[count];
            Array.Copy(current, combo, count);
            result.Add(combo);
        }
        else
        {
            for (int i = min; i <= max; i++)
            {
                current[index] = i;
                GenerateCombinationsHelper(current, index + 1, min, max, count, result);
            }
        }
    }
    
    public static T[,] ResizeArray<T>(T[,] original, int rows, int cols)
    {
        var newArray = new T[rows,cols];
        int minRows = Math.Min(rows, original.GetLength(0));
        int minCols = Math.Min(cols, original.GetLength(1));
        for(int i = 0; i < minRows; i++)
        for(int j = 0; j < minCols; j++)
            newArray[i, j] = original[i, j];
        return newArray;
    }
    
    public static T[,] TrimArray<T>(int rowToRemove, T[,] originalArray)
    {
        T[,] result = new T[originalArray.GetLength(0) - 1, originalArray.GetLength(1)];

        for (int i = 0, j = 0; i < originalArray.GetLength(0); i++)
        {
            if (i == rowToRemove)
                continue;

            for (int k = 0, u = 0; k < originalArray.GetLength(1); k++)
            {
                result[j, u] = originalArray[i, k];
                u++;
            }
            j++;
        }

        return result;
    }
}