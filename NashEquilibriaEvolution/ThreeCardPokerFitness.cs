using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Individual = NashEquilibriaEvolution.EvolutionEngine.Individual;

/*
 * The AKQ game fitness function. It uses the EV calculation (deterministic)
 * fitness evaluator. If you want to use the random sampling fitness, just
 * change the code in Evaluate to call the commented out function instead.
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
    public class ThreeCardPokerFitness : IFitnessFunction
    {
        public int HandsPerEvaluation { get; set; }
        public FitnessTypes Type { get; set; }
        private Random random = new Random();
        public enum FitnessTypes
        {
            Winnings,
            SquaredLosses,
            BestOpponent
        }
        public void Evaluate(Individual[] pop)
        {
            CacheCards();
            for (int i = 0; i < pop.Length; i++)
                pop[i].Fitness = 0;
            for (int i = 0; i < pop.Length; i++)
                for (int j = 0; j < pop.Length; j++)
                {
                    double s1, s2;
                    
                    //Randomly sample N hands
                    //Evaluate(pop[i], pop[j], out s1, out s2);

                    //Calculate EV of the two strategies against each other
                    var g1 = pop[i].Genome;
                    var g2 = pop[j].Genome;
                    s1 = CalculateExpectedValue(g1, g2);
                    s2 = CalculateExpectedValue(g2, g1);

                    switch (Type)
                    {
                        case FitnessTypes.Winnings:
                            pop[i].Fitness += s1;
                            pop[j].Fitness += s2;
                            break;
                        case FitnessTypes.SquaredLosses:
                            if (s1 - s2 < 0)
                                pop[i].Fitness -= (s1 - s2) * (s1 - s2);
                            if (s2 - s1 < 0)
                                pop[i].Fitness -= (s2 - s1) * (s2 - s1);
                            break;
                        case FitnessTypes.BestOpponent:
                            if (s1 - s2 < pop[i].Fitness)
                                pop[i].Fitness = s1 - s2;
                            if (s2 - s1 < pop[j].Fitness)
                                pop[j].Fitness = s2 - s1;
                            break;
                        default:
                            break;
                    }

                }
        }

        private static double CalculateExpectedValue(double[] g1, double[] g2)
        {
            double s1;

            s1 =
                //queen vs. king
                1.0 / 6.0 * (g1[0] * (g2[4] * -2 + (1 - g2[4]) * 1) + (1 - g1[0]) * -1)
                //queen vs. ace
                + 1.0 / 6.0 * (g1[0] * (g2[5] * -2 + (1 - g2[5]) * 1) + (1 - g1[0]) * -1)
                //king vs. queen
                + 1.0 / 6.0 * (g1[1] * (g2[3] * 2 + (1 - g2[3]) * 1) + (1 - g1[1]) * 1)
                //king vs. ace
                + 1.0 / 6.0 * (g1[1] * (g2[5] * -2 + (1 - g2[5]) * 1) + (1 - g1[1]) * -1)
                //ace vs. queen
                + 1.0 / 6.0 * (g1[2] * (g2[3] * 2 + (1 - g2[3]) * 1) + (1 - g1[2]) * 1)
                //ace vs. king
                + 1.0 / 6.0 * (g1[2] * (g2[4] * 2 + (1 - g2[4]) * 1) + (1 - g1[2]) * 1);
            return s1;
        }

        private int[] cardFirst, cardSecond;
        private bool[] betFirst, betSecond;
        private void CacheCards()
        {
            cardFirst = new int[HandsPerEvaluation];
            cardSecond = new int[HandsPerEvaluation];
            betFirst = new bool[HandsPerEvaluation];
            betSecond = new bool[HandsPerEvaluation];
            for (int i = 0; i < HandsPerEvaluation; i++)
            {
                cardFirst[i] = random.Next(3);
                cardSecond[i] = random.Next(3);
                while (cardFirst[i] == cardSecond[i])
                    cardSecond[i] = random.Next(3);
                betFirst[i] = random.Next(0, 2) > 0;
                betSecond[i] = random.Next(0, 2) > 0;
            }
        }

        private void Evaluate(Individual first, Individual second, out double score1, out double score2)
        {
            double total1 = 0, total2 = 0;
            for (int i = 0; i < HandsPerEvaluation; i++)
            {
                double s1;
                double s2;

                double[] probs1 = first.Genome;
                double[] probs2 = second.Genome;
                
                Evaluate(i, probs1, probs2, out s1, out s2);

                //IncrementScore(first, score1);
                //IncrementScore(second, score2);

                total1 += s1 < 0 ? s1 : 0;
                total2 += s2 < 0 ? s2 : 0;
            }
            score1 = total1;
            score2 = total2;
        }

        public void Evaluate(int i, double[] probs1, double[] probs2, out double score1, out double score2)
        {
            int c1 = cardFirst[i];
            int c2 = cardSecond[i];

            double prob1 = probs1[c1];
            double prob2 = probs2[c2 + 3];

            bool bet1 = betFirst[i];
            bool bet2 = betSecond[i];

            //bool bet1 = prob1 >= random.NextDouble();
            //bool bet2 = prob2 >= random.NextDouble();

            double likelihood = (bet1 ? prob1 : (1 - prob1)) * (bet1 ? bet2 ? prob2 : (1 - prob2) : 1);

            double s1, s2;
            Score(c1, c2, bet1, bet2, out s1, out s2);
            score1 = s1; score2 = s2;
        }

        public void Score(int c1, int c2, bool bet1, bool bet2, out double score1, out double score2)
        {
            if (bet1 && !bet2)
            {
                score1 = 1;
                score2 = -1;
                return;
            }

            if (bet1 && bet2)
            {
                if (c1 > c2)
                {
                    score1 = 2;
                    score2 = -2;
                }
                else
                {
                    score1 = -2;
                    score2 = 2;
                }
            }
            else
            {
                if (c1 > c2)
                {
                    score1 = 1;
                    score2 = -1;
                }
                else
                {
                    score1 = -1;
                    score2 = 1;
                }
            }
        }

        public void ScoreProb(int c1, int c2, double prob1, double prob2, bool bet1, bool bet2, out double score1, out double score2)
        {
            if (bet1 && !bet2)
            {
                score1 = 1 * prob1 * (1 - prob2);
                score2 = -1 * prob1 * (1 - prob2);
                return;
            }

            if (bet1 && bet2)
            {
                if (c1 > c2)
                {
                    score1 = 2 * prob1 * prob2;
                    score2 = -2 * prob1 * prob2;
                }
                else
                {
                    score1 = -2 * prob1 * prob2;
                    score2 = 2 * prob1 * prob2;
                }
            }
            else
            {
                if (c1 > c2)
                {
                    score1 = 1 * (1 - prob1);
                    score2 = -1 * (1 - prob1);
                }
                else
                {
                    score1 = -1 * (1 - prob1);
                    score2 = 1 * (1 - prob1);
                }
            }
        }

    }
}
