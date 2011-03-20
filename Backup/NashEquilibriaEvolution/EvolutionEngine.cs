using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/*
 * Simple evolution engine to do plus selection on a collection of doubles.
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
    public class EvolutionEngine
    {
        public int Generations { get; set; }
        public int Parents { get; set; }
        public int Children { get; set; }
        public IFitnessFunction FitnessFunction { get; set; }
        
        private Random random = new Random();

        public EvoResults Evolve(IEnumerable<double[]> initialPopulation)
        {
            EvoResults results = new EvoResults() { Generations = new GenerationResults[Generations] };
            Individual[] pop = new Individual[Parents + Children];
            for (int i = 0; i < initialPopulation.Count(); i++)
                pop[i] = new Individual() { Fitness = 0, Genome = initialPopulation.ElementAt(i) };
            for (int i = initialPopulation.Count(); i < pop.Length; i++)
                pop[i] = new Individual() { Fitness = 0, Genome = createChild(pop[i%initialPopulation.Count()].Genome)};

            for (int curGen = 0; curGen < Generations; curGen++)
            {
                Console.WriteLine("Generation #{0}", curGen);
                FitnessFunction.Evaluate(pop);

                Array.Sort(pop, delegate(Individual ind1, Individual ind2)
                {
                    int compare = ind2.Fitness.CompareTo(ind1.Fitness);
                    return compare;
                });
                if (pop[0].Fitness < pop[1].Fitness)
                    throw new Exception("Sorted the wrong way!");

                GenerationResults gen = new GenerationResults()
                {
                    Champion = new double[pop[0].Genome.Length],
                    Fitness = pop[0].Fitness
                };
                pop[0].Genome.CopyTo(gen.Champion, 0);
                results.Generations[curGen] = gen;

                for (int i = Parents; i < pop.Length; i++)
                    createChild(pop[i % Parents].Genome, pop[i].Genome);
            }

            return results;
        }

        private double[] createChild(double[] parent)
        {
            double[] child = new double[parent.Length];
            for (int i = 0; i < child.Length; i++)
                child[i] = gaussianMutation(parent[i], 0, 1, 1.0 / 6.0);
            return child;
        }

        private void createChild(double[] parent, double[] child)
        {
            for (int i = 0; i < child.Length; i++)
                child[i] = gaussianMutation(parent[i], 0, 1, 1.0 / 6.0);
        }

        public class Individual
        {
            public double[] Genome;
            public double Fitness;
        }

        private double clamp(double val, double low, double high)
        {
            if (val >= high)
                return high;
            if (val <= low)
                return low;
            return val;
        }

        private double gaussianMutation(double mean, double stddev)
        {
            double x1 = random.NextDouble();
            double x2 = random.NextDouble();
            double y1 = Math.Sqrt(-2.0 * Math.Log(x1)) * Math.Cos(2.0 * Math.PI * x2);

            return y1 * stddev + mean;
        }

        private double gaussianMutation(double mean, double min, double max, double stddev)
        {
            double x1 = random.NextDouble();
            double x2 = random.NextDouble();
            double y1 = Math.Sqrt(-2.0 * Math.Log(x1)) * Math.Cos(2.0 * Math.PI * x2);

            return clamp(y1 * stddev + mean, min, max);
        }
    }
}
