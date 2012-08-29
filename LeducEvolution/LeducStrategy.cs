using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace LeducEvolution
{
    public class LeducStrategy
    {
        public static double[] LoadFromFile(GameTree gt, params string[] files)
        {
            double[] strat = new double[gt.ActionStates];
            string line;
            foreach(string s in files)
                using (TextReader reader = new StreamReader(s))
                    while ((line = reader.ReadLine()) != null)
                    {
                        line = line.Trim();
                        if (line.Length == 0 || line[0] == '#')
                            continue;

                        var tokens = line.Split();
                        Debug.Assert(tokens.Length == 4, "Improperly formatted line: " + line);
                        string playerView = tokens[0].Substring(0, tokens[0].Length - 1);
                        var triple = new double[] { double.Parse(tokens[1]), double.Parse(tokens[2]), double.Parse(tokens[3]) };

                        for (int i = 0; i < triple.Length; i++)
                        {
                            int idx = gt.GetIndex(playerView, (Actions)i);
                            if(idx > -1) // if this action is possible
                                strat[idx] = triple[i];
                        }
                        
                        Console.WriteLine(playerView);
                    }
            return strat;
        }

        public static void Save(GameTree gt, double[] strat, string filename)
        {
            using(TextWriter writer = new StreamWriter(filename))
                foreach (var state in gt.PlayerStates)
                {
                    double[] probs = new double[3];
                    for (int i = 0; i < 3; i++)
                    {
                        int idx = gt.GetIndex(state, (Actions)i);
                        if (idx > -1)
                            probs[i] = strat[idx];
                    }
                    writer.WriteLine("{0}: {1:N9} {2:N9} {3:N9}", state, probs[0], probs[1], probs[2]);
                }
        }

        public static double[] AlwaysRaise(GameTree gt)
        {
            double[] strat = new double[gt.ActionStates];
            foreach (var state in gt.PlayerStates)
            {
                int idx = gt.GetIndex(state, Actions.Raise);
                if (idx == -1)
                    idx = gt.GetIndex(state, Actions.Call);
                strat[idx] = 1;
            }
            return strat;
        }
    }
}
