# AKQ Nash Equilibrium Evolution

This is an implementation of the Ace, King, Queen (AKQ) variant of poker along with a set of experiments for evolving optimal (Nash equilibrium) AKQ strategies. The game's rules are as follows:

* Three cards in the deck (A, K, and Q). Each player dealt 1 card face down.
* Each player antes $1 into the pot.
* Player 1 acts first and may check or bet.
* Player 2 acts second and may fold or call if player 1 bets. Player 2 cannot bet if Player 1 checks. Thus, all you have to worry about is how often Player 2 should call Player 1's bet.

You can work out the game tree and solve the linear equation yourself pretty easily, but basically the Nash-equilibrium (NE) optimal strategy is Player1={1/3, 0, 1} and Player2={0,1/3,1} for { Q, K, A }, respectively. Note that all the parameters except Player1 bluffing with a Queen and Player2 calling with a King are strictly dominated if you deviate from the optimal, so really the only thing that someone should have doubts about are those two probabilities.

More information is available on [nashcoding](http://www.nashcoding.com/2010/03/06/a-comment-on-evolving-nash-optimal-poker-strategies-using-evolutionary-computation/).

