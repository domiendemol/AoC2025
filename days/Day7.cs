using System.Numerics;

namespace AoC2025;

public class Day7
{
    Dictionary<Vector2Int, long> _timelineMap = new();
    
    public (string, string) Run(List<string> lines)
    {
        char[,] grid = Utils.ToCharArray(lines);

        // part 1
        List<Vector2Int> splits = new List<Vector2Int>();
        for (int row = 1; row < grid.GetLength(0); row++)
        {
            for (int col = 0; col < grid.GetLength(1); col++)
            {
                // split beam
                if (grid[row - 1, col] == '|' && grid[row, col] == '^') {
                    grid[row, col-1] = '|';
                    grid[row, col+1] = '|';
                    splits.Add(new Vector2Int(row, col));
                }
                // continue beam
                else if (grid[row-1, col] == 'S' || grid[row-1, col] == '|') 
                    grid[row, col] = '|';
            }
        }
        // Utils.PrintCharArray(grid);

        return (splits.Count.ToString(), (2 + GetTimelines(grid, splits[0])).ToString());
    }

    
    long GetTimelines(char[,] grid, Vector2Int startPos)
    {
        if (_timelineMap.ContainsKey(startPos)) return _timelineMap[startPos];
        
        long timelines = 0;
        // follow grid down until we meet a new splitter
        // for both beams
        
        // left
        Vector2Int newPos = new Vector2Int(startPos.x+1, startPos.y-1);
        while (newPos.x < grid.GetLength(0) && newPos.y < grid.GetLength(1))
        {
            if (grid[newPos.x++, newPos.y] == '^')
            {
                timelines += GetTimelines(grid, newPos);
                break;
            }
        }
        if (newPos.x < grid.GetLength(0)) timelines++; 
        
                
        // right
        newPos = new Vector2Int(startPos.x+1, startPos.y+1);
        while (newPos.x < grid.GetLength(0) && newPos.y < grid.GetLength(1))
        {
            if (grid[newPos.x++, newPos.y] == '^')
            {
                timelines += GetTimelines(grid, newPos);
                break;
            }
        }        
        if (newPos.x < grid.GetLength(0)) timelines++; 

        _timelineMap[startPos] = timelines;
        return timelines;
    }
}