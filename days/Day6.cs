using System.Text.RegularExpressions;

namespace AoC2025;

public class Day6
{
    public (string, string) Run(List<string> lines)
    {
        List<string> ops = lines.TakeLast(1).Select(line => Regex.Matches(line, @"([+*])").Select(m => m.Value).ToList()).First();
        List<List<string>> numbers = lines.Take(lines.Count - 1).
            Select(line => Regex.Matches(line, @"([0-9]+)").Select(m => m.Groups[1].Value).ToList()).
            ToList();

        return (Part1(numbers, ops).ToString(), Part2(ops, lines).ToString());
    }
    
    long Part1(List<List<string>> numbers, List<string> ops)
    {
        long total = 0;
        for (int col = 0; col < ops.Count; col++)
        {
            long calcResult = ops[col][0] == '*' ? 1 : 0;
            foreach (var nrs in numbers)
            {
                if (ops[col][0] == '*') calcResult *= Int32.Parse(nrs[col]);
                else if (ops[col][0] == '+') calcResult += Int32.Parse(nrs[col]);
            }
            total += calcResult;
        }

        return total;
    }
    
    long Part2(List<string> ops, List<string> lines)
    {
        List<List<string>> numbers = new List<List<string>>();
        // manual parsing, split on positions of ops. Because we need all the spaces
        foreach (string line in lines.Take(lines.Count-1))
        {
            string t = "";
            List<string> nrs  = new List<string>();
            for (int col = 0; col < line.Length; col++)
            {
                if (col < lines[^1].Length && lines[^1][col] != ' ')
                {
                    if (t.Length != 0) {
                        nrs.Add(t.Substring(0, t.Length-1));
                    }
                    t = "";
                }
                t += line[col];
            }
            nrs.Add(t);
            numbers.Add(nrs);
        }
        
        long total = 0;

        for (int col = 0; col < ops.Count; col++)
        {
            long calcResult = ops[col][0] == '*' ? 1 : 0;
            // now loop for every char
            int calcs = numbers.Select(nrs => nrs[col]).Max(n => n.Length);
            for (int i = 0; i < calcs; i++)
            {
                string compNr = "";
                foreach (var nrs in numbers) {
                    compNr += nrs[col][i];
                }
                if (ops[col][0] == '*') calcResult *= int.Parse(compNr);
                else if (ops[col][0] == '+') calcResult += int.Parse(compNr);
            }
            total += calcResult;
        }

        return total;
    }
}