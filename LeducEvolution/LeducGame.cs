using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace LeducEvolution
{
    public class LeducGame
    {
        

        public LeducGame()
        {
        }

        public bool CanDoAction(Actions a)
        {
            return false;
        }

        public void DoAction(Actions action)
        {
            Debug.Assert(CanDoAction(action));
        }
    }
}
