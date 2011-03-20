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
            ThreeCardPokerFitness fitness = new ThreeCardPokerFitness()
            {
                HandsPerEvaluation = 100,
                Type = ThreeCardPokerFitness.FitnessTypes.SquaredLosses
            };
            EvolutionEngine engine = new EvolutionEngine()
                {
                    Parents = 200,
                    Children = 200,
                    FitnessFunction = fitness,
                    Generations = 200
                };
            var initial = new double[][] { new double[] { 1, 1, 1, 1, 1, 1 } };//{ new double[] { 1.0 / 3.0, 0, 1, 0, 1.0 / 3.0, 1.0 } };

            var results = engine.Evolve(initial);

            using (TextWriter writer = new StreamWriter("results.csv"))
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
    }
}
