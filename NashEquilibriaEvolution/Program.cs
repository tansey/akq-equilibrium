using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

/*
 * This program tests different fitness functions for the AKQ game.
 *
 * Copyright (C) 2010 Wesley Tansey.
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 */
namespace NashEquilibriaEvolution
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 1 && args[0] == "-gen")
            {
                GenerateCondorFile();
                return;
            }

            int handsPerEval = args[0].ToInt();
            int faceoffs = args[1].ToInt();
            int parents = args[2].ToInt();

            ThreeCardPokerFitness fitness = new ThreeCardPokerFitness()
            {
                HandsPerEvaluation = handsPerEval,
                RandomSamplingFaceoffs = faceoffs,
                Type = ThreeCardPokerFitness.FitnessTypes.SquaredLosses,
                EvalStrategy = ThreeCardPokerFitness.EvaluationStrategy.UCB1,
                EvaluationType = ThreeCardPokerFitness.EvaluationTypes.RandomSampling
            };
            Console.WriteLine("Hands: {0}", fitness.HandsPerEvaluation);
            Console.WriteLine("Faceoffs: {0}", fitness.RandomSamplingFaceoffs);
            Console.WriteLine("Type: {0}", fitness.Type);
            Console.WriteLine("EvalStrategy: {0}", fitness.EvalStrategy);
            Console.WriteLine("EvalType: {0}", fitness.EvaluationType);
            EvolutionEngine engine = new EvolutionEngine()
                {
                    Parents = parents,
                    Children = parents,
                    FitnessFunction = fitness,
                    Generations = 500
                };
            var initial = new double[][] { new double[] { 1, 1, 1, 1, 1, 1 } };//{ new double[] { 1.0 / 3.0, 0, 1, 0, 1.0 / 3.0, 1.0 } };

            var results = engine.Evolve(initial);

            using (TextWriter writer = new StreamWriter(string.Format("sqloss_ucb_results_{0}eval_{1}faceoffs_parents{2}.csv",
                                                                        fitness.HandsPerEvaluation,
                                                                        fitness.RandomSamplingFaceoffs, 
                                                                        engine.Parents)))
            {
                writer.WriteLine("Generation,Fitness,P1_Queen,P1_King,P1_Ace,P2_Queen,P2_King,P2_Ace");
                for (int i = 0; i < results.Generations.Length; i++)
                {
                    writer.Write(i + "," + results.Generations[i].Fitness);
                    for (int j = 0; j < 6; j++)
                        writer.Write("," + results.Generations[i].Champion[j]);
                    writer.WriteLine();
                }
            }
        }

        static void GenerateCondorFile()
        {
            int[] handsPerEval = new int[] { 1, 10, 100, 1000, 10000, 50000, 100000 };
            int[] faceoffs = new int[] { 1, 2, 5, 10 };
            int[] parents = new int[] { 1, 10, 50, 100 };
            using (TextWriter writer = new StreamWriter("condor.jobs"))
            {
                writer.WriteLine("universe = vanilla");
                writer.WriteLine("Initialdir =/u/tansey/akq_nash/");
                writer.WriteLine();
                writer.WriteLine("Executable = /lusr/opt/mono-2.10.8/bin/mono");
                writer.WriteLine("+Group   = \"GRAD\"");
                writer.WriteLine("+Project = \"AI_ROBOTICS\"");
                writer.WriteLine("+ProjectDescription = \"Nash equilibria evolution parameter sweep\"");

                for(int i = 0; i < handsPerEval.Length; i++)
                    for(int j = 0; j < faceoffs.Length; j++)
                        for (int k = 0; k < parents.Length; k++)
                        {
                            if (handsPerEval[i] > 10000 && faceoffs[j] > 2 && parents[k] > 10)
                                continue;
                            int popsize = parents[k] * 2;
                            int faces = faceoffs[j] * popsize * popsize;
                            writer.WriteLine();
                            writer.WriteLine("Log = /u/tansey/akq_nash/sqloss_ucb_results_{0}eval_{1}faceoffs_parents{2}.log", handsPerEval[i], faces, parents[k]);
                            writer.WriteLine("Arguments = /u/tansey/akq_nash/NashEquilibriaEvolution.exe {0} {1} {2}", handsPerEval[i], faces, parents[k]);
                            writer.WriteLine("Output=/u/tansey/akq_nash/sqloss_ucb_results_{0}eval_{1}faceoffs_parents{2}.out", handsPerEval[i], faces, parents[k]);
                            writer.WriteLine("Error=/u/tansey/akq_nash/sqloss_ucb_results_{0}eval_{1}faceoffs_parents{2}.err", handsPerEval[i], faces, parents[k]);
                            writer.WriteLine("Queue 1");
                        }
            }
        }
    }
}
