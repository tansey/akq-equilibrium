using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NashEquilibriaEvolution;

namespace LeducEvolution
{
    public class LeducExactFitness : IFitnessFunction
    {

        public void Evaluate(EvolutionEngine.Individual[] pop)
        {
            
        }

        /// <summary>
        /// Compete the two strategies against each other
        /// </summary>
        /// <param name="gt1"></param>
        /// <returns>The net amount s1 exploits s2. If s1 is better than s2, it is positive. If s1 is worse than s2, it is negative.</returns>
        private double compete(LeducStrategy s1, LeducStrategy s2)
        {

            return 0;
        }
    }
}
