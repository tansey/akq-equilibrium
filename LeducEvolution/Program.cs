using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace LeducEvolution
{
    class Program
    {
        static Random random = new Random();

        static void Main(string[] args)
        {
            Diff();
            return;
            GameTree gt = new GameTree();

            double[] s1 = LeducStrategy.LoadFromFile(gt, "strategies/fullgame_1.strat", "strategies/fullgame_2.strat");
            //double[] s2 = LeducStrategy.LoadFromFile(gt, "strategies/fullgame_1.strat", "strategies/fullgame_2.strat");
            double[] s2 = LeducStrategy.LoadFromFile(gt, "strategies/random.strat");
            //double[] s1 = LeducStrategy.LoadFromFile(gt, "strategies/always_fold.strat");
            //double[] s2 = LeducStrategy.LoadFromFile(gt, "strategies/random.strat");

            //double[] s1 = RandomStrategy(gt);
            //double[] s2 = RandomStrategy(gt);

            Console.WriteLine("Strategy 1 EV: {0}", gt.ExpectedValue(s1, s2));
            //gt.PrintTree();

            //LeducStrategy.Save(gt, s1, "strategies/temp.strat");
        }

        static void Diff()
        {
            string[] opencfr = ReadLines("strategies/opencfr.log");
            var target = new List<string[]>(ReadLines("strategies/target.log").Select(s => s.Split()));
            int found = 0;
            int skipped = 0;
            foreach (var line in opencfr)
            {
                string[] tokens = line.Split();
                int idx = tokens[4] == "Board:" ? 8 : 6;
                string prob = tokens[idx];
                string hole1 = tokens[1];
                string hole2 = tokens[3];
                string sequence = tokens[idx - 2];
                string payoff = tokens.Last();

                string[] targetLine = target.FirstOrDefault(s => s[1] == hole1 && 
                                                          s[3] == hole2 && 
                                                          s[6] == sequence && 
                                                          (idx == 6 || s[5] == tokens[5]) && //board 
                                                          s[8] == prob
                                                          );
                if (targetLine == null)
                {
                    Console.WriteLine(line);
                    skipped++;
                }
                else
                {
                    target.Remove(targetLine);
                    found++;
                }
            }

            Console.WriteLine("Found: {0} Skipped: {1}", found, skipped);

            //Console.WriteLine();
            //Console.WriteLine("Remaining lines");
            //foreach (var line in target)
            //    Console.WriteLine(line);

            Console.WriteLine();
            Console.WriteLine("Gathered probabilities");
            var gathered = target.Where(s => !s[0].StartsWith("#"))
                  .GroupBy(s => s[1] + s[3] + s[6])
                  .Where(g => g.Sum(s => double.Parse(s[8])) > double.Epsilon)
                  .Select(g => string.Format("P1: {0} P2: {1} {2} Prob: {3:N6} Payoff: {4}",
                                            g.First()[1],
                                            g.First()[3],
                                            g.First()[6],
                                            g.Sum(s => double.Parse(s[8])),
                                            g.First().Last())
                          );
            foreach (var gline in gathered)
                Console.WriteLine(gline);
        }

        static string[] ReadLines(string file)
        {
            List<string> lines = new List<string>();

            using (TextReader reader = new StreamReader(file))
            {
                string line = null;
                while ((line = reader.ReadLine()) != null)
                    if (line.Length > 0)
                        lines.Add(line);
            }

            return lines.ToArray();
        }

        static double[] RandomStrategy(GameTree gt)
        {
            double[] s = new double[gt.ActionStates];
            for (int i = 0; i < s.Length; i++)
                s[i] = random.NextDouble();
            gt.NormalizeStrategy(s);
            return s;
        }
    }
}
