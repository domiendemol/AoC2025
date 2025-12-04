using System.Diagnostics;
using System.Reflection;
using Spectre.Console;

namespace AoC2025
{
    static class Program
    {
        private const string BENCHMARK = "BENCHMARK";
        private const int DAY = 4;
        
        public static void Main(string[] args)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            string arg = args.Length > 0 ? args[0] : "";
            if (arg == BENCHMARK) {
                Benchmark();
            }
            else {
                AnsiConsole.Write(new Rule($"[bold]Day {DAY}[/]").LeftJustified());
                (string, string) result = RunDay(DAY, arg == "TEST");
                AnsiConsole.MarkupLine($"- Part 1: [blue]{result.Item1}[/]");
                AnsiConsole.MarkupLine($"- Part 2: [blue]{result.Item2}[/]");
            }

            stopwatch.Stop();
            TimeSpan stopwatchElapsed = stopwatch.Elapsed;
            AnsiConsole.MarkupLine($"Completed in: [bold]{Convert.ToInt32(stopwatchElapsed.TotalMilliseconds)}[/] ms");
        }

        static (string, string) RunDay(int day, bool test)
        {
            // REFLECTIOOON
            Type type = Type.GetType($"AoC2025.Day{day}")!;
            MethodInfo? method = type.GetMethod("Run");
            object? obj = Activator.CreateInstance(type);
            string testSuffix = test ? "_test" : "";
            if (!File.Exists($"input/day{day}{testSuffix}.txt")) {
                Console.WriteLine($"Error: input file [input/day{day}{testSuffix}.txt] does not exist");
                return (null, null);
            }
            return ((string, string)) method.Invoke(obj, new object[]{File.ReadAllText($"input/day{day}{testSuffix}.txt").Trim().Split('\n').ToList()});
        }
        
        static void Benchmark()
        {
            Console.WriteLine("Running full benchmark: ");
            
            var table = new Table();
            table.Border(TableBorder.Rounded);
            AnsiConsole.Live(table)
                .Start(ctx => 
                {
                    table.AddColumn("[bold yellow]Day[/]");
                    table.AddColumn("[bold yellow]Part 1[/]");
                    table.AddColumn("[bold yellow]Part 2[/]");
                    table.AddColumn("[bold yellow]Time[/]");
                    ctx.Refresh();

                    for (int i = 1; i <= DAY; i++)
                    {
                        Stopwatch stopwatch = new Stopwatch();
                        stopwatch.Start();

                        table.AddRow($"[blue]Day {i}[/]", "-", "-", "-");
                        ctx.Refresh();

                        (string, string) result = RunDay(i, false);
                
                        stopwatch.Stop();
                        TimeSpan stopwatchElapsed = stopwatch.Elapsed;
                        table.RemoveRow(table.Rows.Count - 1);
                        string color = Convert.ToInt32(stopwatchElapsed.TotalMilliseconds) >= 1000 ? "red" : "green";
                        table.AddRow($"[blue]Day {i}[/]", result.Item1, result.Item2, $"[{color}]{Convert.ToInt32(stopwatchElapsed.TotalMilliseconds)}[/] ms");
                        table.Columns[3].RightAligned();
                        ctx.Refresh();
                    }
                });
        }
    }
}