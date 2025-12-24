using System.Text.RegularExpressions;

namespace AoC2025;

public class Day12
{
    public (string, string) Run(List<string> input)
    {
        // parse gifts
        List<Gift> gifts = new();
        foreach (var line in input)
        {
            if (line.Contains('x'))
                break;
            else if (line.Contains(':'))
                gifts.Add(new Gift(Convert.ToInt32(line.Substring(0, line.Length - 1))));
            else if (line.Length > 0)
                gifts.Last().AddLine(line);
        }
        
        // parse regions    
        // 4x4: 0 0 0 0 2 0
        List<Region> regions = input.Where(l => l.Contains('x')).Select(l => new Region(l)).ToList();

        return ("", "");
    }

    public class Region
    {
        public int width;
        public int height;
        public int[] gifts;

        public Region(string line)
        {
            Match match = Regex.Match(line, "([0-9]+)x([0-9]+)");
            width = Convert.ToInt32(match.Groups[1].Value);
            height = Convert.ToInt32(match.Groups[2].Value);
                
            MatchCollection giftMatches = Regex.Matches(line, " ([0-9]+)");
            gifts = giftMatches.Select(m => int.Parse(m.Groups[1].Value)).ToArray();
        }
    }

    public class Gift
    {
        public int index;
        public bool[,] shape = new bool[3, 3];

        private int _lineIndex = 0;
        
        public Gift(int index)
        {
            this.index = index;
        }

        public void AddLine(string line)
        {
            shape[_lineIndex, 0] = line[0] == '#';
            shape[_lineIndex, 1] = line[1] == '#';
            shape[_lineIndex++, 2] = line[2] == '#';
        }
    }
}