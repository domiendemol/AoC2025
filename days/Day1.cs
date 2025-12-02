using System.Text.RegularExpressions;

namespace AoC2025;

public class Day1
{
    public (string, string) Run(List<string> lines)
    {
        return (CountZeros(lines, false).ToString(), CountZeros(lines, true).ToString());
    }

    int CountZeros(List<string> lines, bool all)
    {
        int pointer = 50;
        int zeros = 0;
        foreach (string line in lines)
        {
            Match m = Regex.Match(line, @"([L-R])([0-9]+)");
            int nr = Convert.ToInt32(m.Groups[2].Value);
            bool left  = m.Groups[1].Value == "L";
            if (!all && AddAndCountZeros(ref pointer, left, nr)) zeros++;
            else if (all) zeros += AddAndCountAllZeros(ref pointer, left, nr);
        }
        return zeros;
    }
    
    
    bool AddAndCountZeros(ref int pointer, bool left, int nr)
    {
        pointer += (left ? -1 : 1) * nr;
        while (pointer < 0) pointer += 100;
        if (pointer >= 100) pointer %= 100;
        return pointer == 0;
    }
    
    int AddAndCountAllZeros(ref int pointer, bool left, int nr)
    {
        int zeros = pointer == 0 && left ? -1 : 0;
        pointer += (left ? -1 : 1) * nr;
        
        while (pointer < 0) {
            pointer += 100;
            zeros++;
        }
        while (pointer >= 100) {
            pointer -= 100;
            if (pointer != 0) zeros++;
        }

        zeros = Math.Clamp(zeros + (pointer == 0 ? 1 : 0), 0, Int32.MaxValue);
        return zeros;
    }
}