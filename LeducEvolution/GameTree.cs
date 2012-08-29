using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;

namespace LeducEvolution
{
    public class GameTree
    {
        int[] _maxBetLevel = new int[] { 2, 2 };
        int[] _betSize = new int[] { 2, 4 };
        int _ante = 1;
        Node _root;
        Dictionary<string, int> _indexes;
        HashSet<string> _playerStates;
        public int ActionStates { get { return _indexes.Count; } }
        public IEnumerable<string> PlayerStates { get { return _playerStates; } }

        public class Node
        {
            public int index;
            public double payoff;
            public Node[] children;
            public Actions action;
            public bool terminal;
            public bool isAction;
            public Cards hole1;
            public Cards hole2;
            public Cards board;
            public Rounds round;
            public string sequence;
            public int player;

            private string _playerSeq;
            public string playerView()
            {
                if (_playerSeq == null)
                {
                    Debug.Assert(player == 1 || player == 2);
                    _playerSeq = (player == 1 ? cardToChar(hole1) : cardToChar(hole2))
                               + (round == Rounds.Flop ? cardToChar(board).ToString() : "")
                               + ":" + sequence;
                }
                return _playerSeq;
            }

            public Cards OpponentCard()
            {
                return player == 1 ? hole2 : hole1;
            }
        }

        public GameTree()
        {
            buildTree();
        }

        public GameTree(int[] maxBetLevel, int[] betSizes, int ante)
        {
            _maxBetLevel = maxBetLevel;
            _betSize = betSizes;
            _ante = ante;
        }

        //public double Compare(LeducStrategy s1, LeducStrategy s2)
        //{
        //    return ExpectedValue(s1, s2) - ExpectedValue(s2, s1);
        //}

        //private double ExpectedValue(LeducStrategy s1, LeducStrategy s2)
        //{
        //    double result = 0;
        //    double prob = 1.0 / (double)_root.children.Length;
        //    foreach (var child in _root.children)
        //        result += ExpectedValue(s1, s2, child, 1, prob);
        //    return result;
        //}

        //private double ExpectedValue(LeducStrategy s1, LeducStrategy s2, Node cur, int player, double prob)
        //{
        //    if (prob < double.Epsilon)
        //        return 0;

        //    if (cur.terminal)
        //        return cur.payoff * prob;

        //    LeducStrategy s = player == 1 ? s1 : s2;
        //    string state = "" + (player == 1 ? cardToChar(cur.hole1) : cardToChar(cur.hole2));
        //    if(cur.round == Rounds.Flop)
        //        state += cardToChar(cur.board);
        //    state += ":" + cur.sequence + ":";
        //    Console.WriteLine("{0}: {1}", player, state);
        //    double[] strat = s.Strategy(state);

        //    double ev = 0;
        //    int nextPlayer = player == 1 ? 2 : 1;
        //    for (int i = 0; i < strat.Length; i++)
        //        if (strat[i] > double.Epsilon)
        //        {
        //            var child = cur.children.First(n => n.action == (Actions)i);
        //            if(child.round == Rounds.Flop && cur.round == Rounds.Preflop)
        //                nextPlayer = 1;
        //            ev += ExpectedValue(s1, s2, child, nextPlayer, prob * strat[i]);
        //        }
        //    return ev;
        //}

        const string LOG_FILENAME = "strategies/target.log";
        public double Compare(double[] s1, double[] s2)
        {
            using (TextWriter writer = new StreamWriter(LOG_FILENAME))
                writer.WriteLine("# P1 = CFR, P2 = Target");
            double ev1 = ExpectedValue(s1, s2);
            using (TextWriter writer = new StreamWriter(LOG_FILENAME, true))
                writer.WriteLine("# P1 = Target, P2 = CFR");

            double ev2 = ExpectedValue(s2, s1);
            double ev = (ev1 - ev2) / 2.0;

            using (TextWriter writer = new StreamWriter(LOG_FILENAME, true))
            {
                writer.WriteLine("# EV(CFR, Target) = {0}", ev1);
                writer.WriteLine("# EV(Target, CFR) = {0}", ev2);
                writer.WriteLine("# Total EV = {0}", ev);
            }

            return ev;
        }

        public double ExpectedValue(double[] s1, double[] s2)
        {
            using (TextWriter writer = new StreamWriter(LOG_FILENAME))
                writer.WriteLine("# P1 = CFR, P2 = Target");
            Debug.Assert(s1.Length == s2.Length && s1.Length == ActionStates);
            double result = 0;

            foreach (var child in _root.children)
            {
                List<double> allProbs = new List<double>() { 1.0 / 3.0 };
                int[] deck = new int[] { 2, 2, 2 };
                deck[(int)child.hole1]--;
                allProbs.Add(deck[(int)child.hole2] / 5.0);
                deck[(int)child.hole2]--;
                allProbs.Add(deck[(int)child.board] / 4.0);
                result += ExpectedValue(s1, s2, child, 1, allProbs.Product(), allProbs);
            }

            return result;
        }

        private double ExpectedValue(double[] s1, double[] s2, Node cur, int player, double prob, List<double> allProbs)
        {
            if (prob < double.Epsilon)
                return 0;

            if (cur.terminal)
                return cur.payoff * prob;

            double[] s = player == 1 ? s1 : s2;

            //Debugging traces through the game tree
            for (int i = 0; i < cur.children.Length; i++)
            {
                if (!cur.children[i].terminal)
                    continue;

                allProbs.Add(s[cur.children[i].index]);
                Console.WriteLine("{0} -> {1} Opp: {2} Prob={3} Payoff={4} [{5}]", cur.playerView(), cur.children[i].action, cardToChar(cur.OpponentCard()), allProbs.Product(), cur.children[i].payoff, allProbs.Flatten(", "));
                using (TextWriter writer = new StreamWriter(LOG_FILENAME, true))
                    writer.WriteLine("P1: {0} P2: {1} Board: {2} {3}{4} Prob: {5:N6} Payoff: {6}",
                                        cardToChar(cur.hole1),
                                        cardToChar(cur.hole2),
                                        cardToChar(cur.board),
                                        cur.sequence,
                                        actionToChar(cur.children[i].action),
                                        allProbs.Product(),
                                        cur.children[i].payoff,
                                        allProbs.Select(d => d.ToString("N9")).Flatten(", "));
                allProbs.RemoveAt(allProbs.Count - 1);
            }

            Debug.Assert(cur.children.Sum(n => s[n.index]) >= 0.9999, string.Format("Failed {0}: {1}", cur.index, cur.children.Sum(n => s[n.index])));
            Debug.Assert(cur.children.Sum(n => s[n.index]) <= 1.0001, string.Format("Failed {0}: {1}", cur.index, cur.children.Sum(n => s[n.index])));

            double ev = 0;
            int nextPlayer = player == 1 ? 2 : 1;
            foreach (var child in cur.children)
            {
                int lookup = child.index;
                if (s[lookup] > double.Epsilon)
                {
                    allProbs.Add(s[lookup]);
                    if (child.round == Rounds.Flop && cur.round == Rounds.Preflop)
                        ev += ExpectedValue(s1, s2, child, 1, prob * s[lookup], allProbs);
                    else
                        ev += ExpectedValue(s1, s2, child, nextPlayer, prob * s[lookup], allProbs);
                    allProbs.RemoveAt(allProbs.Count - 1);
                }
            }
            return ev;
        }

        public void NormalizeStrategy(double[] s)
        {
            foreach (var child in _root.children)
                NormalizeStrategy(s, child);
        }

        private void NormalizeStrategy(double[] s, Node n)
        {
            if (n.terminal)
                return;

            int[] idx = n.children.Select(x => x.index).ToArray();

            double sum = 0;
            for (int i = 0; i < idx.Length; i++)
                sum += s[idx[i]];
            for (int i = 0; i < idx.Length; i++)
                s[idx[i]] /= sum;

            Debug.Assert(n.children.Sum(x => s[x.index]) <= 1.0001, n.children.Sum(x => s[x.index]).ToString());
            Debug.Assert(n.children.Sum(x => s[x.index]) >= 0.9999, n.children.Sum(x => s[x.index]).ToString());

            foreach (var child in n.children)
                NormalizeStrategy(s, child);
        }

        private void buildTree()
        {
            _root = new Node();
            _indexes = new Dictionary<string, int>();
            _playerStates = new HashSet<string>();
            List<Node> children = new List<Node>();
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    for (int k = 0; k < 3; k++)
                        if (i != j || j != k)//make sure we don't deal 3 of the same card
                        {
                            var n = new Node()
                            {
                                hole1 = (Cards)i,
                                hole2 = (Cards)j,
                                board = (Cards)k,
                                round = Rounds.Preflop,
                                terminal = false,
                                isAction = false,
                                player = 1,
                                sequence = "/",
                                index = -1
                            };
                            recursiveBuildTree(n, 1, Rounds.Preflop, _ante * 2, new int[] { 0, 0 }, _indexes);
                            children.Add(n);
                        }
            _root.children = children.ToArray();
            Console.WriteLine("Player States: {0}", _indexes.Count);
        }

        private void recursiveBuildTree(Node root, int player, Rounds round,
                                        double pot, int[] bets, Dictionary<string, int> indexes)
        {
            if (root.terminal)
                return;

            _playerStates.Add(root.playerView());

            List<Node> children = new List<Node>();
            int pIdx = player - 1;
            int nextPlayer = player == 1 ? 2 : 1;

            // Can we raise?
            if (bets.Max() < _maxBetLevel[(int)round])
            {
                var betNode = new Node()
                {
                    action = Actions.Raise,
                    board = root.board,
                    hole1 = root.hole1,
                    hole2 = root.hole2,
                    round = round,
                    terminal = false,
                    isAction = true,
                    player = nextPlayer,
                    sequence = root.sequence + 'r'
                };
                SetNodeIndex(betNode, root, indexes);
                int prevBets = bets[pIdx];
                int betsToAdd = bets.Max() - prevBets + 1;//may need to add either 1 or 2 bets
                bets[pIdx] = bets.Max() + 1;
                recursiveBuildTree(betNode, player == 1 ? 2 : 1, round, pot + _betSize[(int)round] * betsToAdd, bets, indexes);
                bets[pIdx] = prevBets;

                children.Add(betNode);
            }

            // Can we fold?
            if(bets.Max() > 0)
            {
                var foldNode = new Node()
                {
                    action = Actions.Fold,
                    board = root.board,
                    hole1 = root.hole1,
                    hole2 = root.hole2,
                    round = round,
                    // take back the uncalled bet and return the payoff as half the pot
                    payoff = (pot - _betSize[(int)round]) / (player == 1 ? -2.0 : 2.0),
                    terminal = true,
                    isAction = true,
                    player = nextPlayer,
                    sequence = root.sequence + 'f'
                };
                SetNodeIndex(foldNode, root, indexes);
                children.Add(foldNode);
            }

            // If we call, is the round over?
            var callNode = new Node()
            {
                action = Actions.Call,
                board = root.board,
                hole1 = root.hole1,
                hole2 = root.hole2,
                round = round,
                isAction = true,
                player = nextPlayer,
                sequence = root.sequence + 'c'
            };
            if (bets.Max() == 0 && player == 1)// first to act in a round can check
            {
                callNode.terminal = false;
                recursiveBuildTree(callNode, 2, round, pot, bets, indexes);
            }
            else if (round == Rounds.Preflop) // calling a bet preflop
            {
                callNode.sequence += '/';
                callNode.round = Rounds.Flop;
                callNode.player = 1;
                int callAmt = root.action == Actions.Raise ? _betSize[0] : 0;
                recursiveBuildTree(callNode, 1, Rounds.Flop, pot + callAmt, new int[] { 0, 0 }, indexes);
            }
            else // calling a flop bet and seeing a showdown
            {
                callNode.terminal = true;
                if (callNode.hole1 == callNode.hole2) // draw
                    callNode.payoff = 0;
                else if (callNode.hole1 == callNode.board) // p1 pair
                    callNode.payoff = (pot + _betSize[1]) / 2.0;
                else if (callNode.hole2 == callNode.board) // p2 pair
                    callNode.payoff = (pot + _betSize[1]) / -2.0;
                else if(callNode.hole1 > callNode.hole2) // p1 high card with no pairs on board
                    callNode.payoff = (pot + _betSize[1]) / 2.0;
                else // p2 high card with no pairs on board
                    callNode.payoff = (pot + _betSize[1]) / -2.0;
            }
            SetNodeIndex(callNode, root, indexes);
            children.Add(callNode);

            root.children = children.ToArray();
        }

        private void SetNodeIndex(Node child, Node parent, Dictionary<string, int> indexes)
        {
            string key = parent.playerView() + '-' + actionToChar(child.action);
            int value;
            if (!indexes.TryGetValue(key, out value))
            {
                value = indexes.Count;
                indexes.Add(key, value);
            }
            child.index = value;
        }

        public int GetIndex(string playerView, Actions action)
        {
            int result;
            if(!_indexes.TryGetValue(playerView + '-' + actionToChar(action), out result))
                return -1;
            return result;
        }

        int count = 0;
        public void PrintTree()
        {
            count = 0;
            foreach(var hand in _root.children)
                foreach(var action in hand.children)
                    printTree(action, "/");
            Console.WriteLine("Count: {0}", count);
        }

        private void printTree(Node node, string prefix)
        {
            if (node.terminal)
            {
                Console.WriteLine("{0}{1}{2} Opp: {3} Payoff: {4} Index: {5} PlayerView: {6}",
                                  cardToChar(node.hole1),
                                  cardToChar(node.board),
                                  prefix + actionToChar(node.action),
                                  cardToChar(node.hole2),
                                  node.payoff,
                                  node.index,
                                  node.playerView());
                count++;
            }
            else
            {
                string newfix = prefix;
                newfix += actionToChar(node.action);
                if (node.round == Rounds.Flop && newfix.Count(c => c == '/') == 1)
                    newfix += '/';

                foreach (var child in node.children)
                    printTree(child, newfix);
            }
        }
        private static char actionToChar(Actions a)
        {
            switch (a)
            {
                case Actions.Raise: return 'r';
                case Actions.Call: return 'c';
                case Actions.Fold: return 'f';
            }
            throw new Exception();
        }
        private static char cardToChar(Cards c)
        {
            switch (c)
            {
                case Cards.Jack: return 'J';
                case Cards.Queen: return 'Q';
                case Cards.King: return 'K';
            }
            throw new Exception();
        }
    }
}
