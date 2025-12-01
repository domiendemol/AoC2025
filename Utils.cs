using System.Collections.Concurrent;
using System.Runtime.Intrinsics.X86;
using System.Text;

namespace AoC2024;

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

}