using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Unity.Mathematics.math;
using System.Diagnostics;

public class AI
{
    //private static ulong numberOfNodes;
    private static Stopwatch stopwatch;

    private static int timerForAIMove = 1000;
    public static Move startSearch(int depth, ref Bitboards bitboard)
    {
        Move bestMove = Move.nullMove;
        stopwatch = new Stopwatch();
        stopwatch.Start();
        for(int i = 1;stopwatch.ElapsedMilliseconds < timerForAIMove && i < 64; i++)
        {
            Move tempMove = rootNegaMax(i, ref bitboard);
            //UnityEngine.Debug.Log(tempMove.startSquare);
            if(!Move.SameMove(tempMove, Move.nullMove))
                bestMove = tempMove;
            
            if(stopwatch.ElapsedMilliseconds > timerForAIMove) UnityEngine.Debug.Log("Depth that was fully searched: " + (i-1));
        }
        stopwatch.Stop();
        //UnityEngine.Debug.Log(bestMove.startSquare);
        return bestMove;
    }
    public static Move rootNegaMax (int depth, ref Bitboards bitboard)
    {
        //numberOfNodes = 0;
        Move[] moves = new Move[256];
        Move bestMove = Move.nullMove;
        ulong num_moves = new ulong();
        int alpha = int.MinValue;
        int beta = int.MaxValue;
        int bestValue = int.MinValue;
        num_moves = MoveGenerator.GenerateMoves(ref moves, bitboard);
        if(num_moves == 0) return Move.nullMove;
        for(ulong i = 0; i < num_moves; i++)
        {
            Move test = moves[i];
            int captureIndex = bitboard.playMove(test);
            int score = -negaMax(depth-1, -beta, -alpha, ref bitboard, test);
            bitboard.undoMove(test, captureIndex);
            if(score > bestValue) 
            {
                bestValue = score;
                bestMove = test;
            }
            alpha = System.Math.Max(bestValue, alpha);
            if(alpha >= beta) break;
        }
        if(stopwatch.ElapsedMilliseconds > timerForAIMove)
            return Move.nullMove;
        //Debug.Log("AI leaf nodes reached: " + numberOfNodes);
        return bestMove;
    }

    public static int negaMax(int depth, int alpha, int beta, ref Bitboards bitboard, Move parentMove)
    {
        if(depth == 0) return captureOnlyNegaMax(alpha, beta, ref bitboard, parentMove);
        if(stopwatch.ElapsedMilliseconds > timerForAIMove) return alpha;
        Move[] moves = new Move[256];
        ulong num_moves = new ulong();
        int score = int.MinValue;
        MoveGenerator.lastMove = parentMove;
        num_moves = MoveGenerator.GenerateMoves(ref moves, bitboard);
        for(ulong i = 0; i < num_moves; i++)
        {
            Move test = moves[i];
            int captureIndex = bitboard.playMove(test);
            int val = -negaMax(depth-1, -beta, -alpha, ref bitboard, test);
            bitboard.undoMove(test, captureIndex);
            score = System.Math.Max(val, score);
            alpha = System.Math.Max(alpha, score);
            if(alpha >= beta) break;
        }
        return score;
    }

    public static int captureOnlyNegaMax(int alpha, int beta, ref Bitboards bitboard, Move parentMove)
    {
        int eval = evaluate(ref bitboard);
        if(eval >= beta) return beta;
        alpha = System.Math.Max(alpha, eval);
        Move[] moves = new Move[256];
        ulong num_moves = new ulong();
        MoveGenerator.lastMove = parentMove;
        num_moves = MoveGenerator.GenerateMoves(ref moves, bitboard, true);
        for(ulong i = 0; i < num_moves; i++)
        {
            Move test = moves[i];
            int captureIndex = bitboard.playMove(test);
            int val = -captureOnlyNegaMax(-beta, -alpha, ref bitboard, test);
            bitboard.undoMove(test, captureIndex);
            eval = System.Math.Max(eval, val);
            if(eval >= beta) return beta;
            alpha = System.Math.Max(alpha, eval);
        }
        return eval;
    }
    
    public static int evaluate(ref Bitboards bitboard)
    {
        int player = bitboard.whiteTurn ? 0 : 1;
        bool white = bitboard.whiteTurn ? true : false;
        int value = 0;
        
        ulong pawnValue = bitboard.pawns[player];
        ulong knightValue = bitboard.knights[player];
        ulong bishopValue = bitboard.bishops[player];
        ulong rookValue = bitboard.rooks[player];
        ulong queenValue = bitboard.queens[player];
        int kingValue = bitboard.kings[player];

        while(pawnValue != 0)
        {
            int i = tzcnt(pawnValue);
            pawnValue &= pawnValue - 1;
            value += 10;
            if(white) value += pawnStartTableWhite[i];
            else value += pawnStartTableBlack[i];
        }
        while(knightValue != 0)
        {
            int i = tzcnt(knightValue);
            knightValue &= knightValue - 1;
            value += 29;
            if(white) value += knightTableWhite[i];
            else value += knightTableBlack[i];
        }
        while(bishopValue != 0)
        {
            int i = tzcnt(bishopValue);
            bishopValue &= bishopValue - 1;
            value += 31;
            if(white) value += bishopTableWhite[i];
            else value += bishopTableBlack[i];
        }
        while(rookValue != 0)
        {
            int i = tzcnt(rookValue);
            rookValue &= rookValue - 1;
            value += 50;
            if(white) value += rookTableWhite[i];
            else value += rookTableBlack[i];
        }
        while(queenValue != 0)
        {
            int i = tzcnt(queenValue);
            queenValue &= queenValue - 1;
            value += 90;
            if(white) value += queenTableWhite[i];
            else value += queenTableBlack[i];
        }
        if(white) value += kingStartTableWhite[bitboard.kings[player]];
        else value += kingStartTableBlack[bitboard.kings[player]];
        
        return value;
    }

    public static void orderMoves(ref Move[] moves, ulong num_moves)
    {
        Move[] reorderedMoveList = new Move[num_moves];
        return;
    }

    public static readonly int[] pawnStartTableBlack = {
			 0,   0,   0,   0,   0,   0,   0,   0,
			50,  50,  50,  50,  50,  50,  50,  50,
			10,  10,  20,  30,  30,  20,  10,  10,
			 5,   5,  10,  25,  25,  10,   5,   5,
			 0,   0,   0,  20,  20,   0,   0,   0,
			 5,  -5, -10,   0,   0, -10,  -5,   5,
			 5,  10,  10, -20, -20,  10,  10,   5,
			 0,   0,   0,   0,   0,   0,   0,   0
		};

		public static readonly int[] pawnEndTableBlack = {
			 0,   0,   0,   0,   0,   0,   0,   0,
			80,  80,  80,  80,  80,  80,  80,  80,
			50,  50,  50,  50,  50,  50,  50,  50,
			30,  30,  30,  30,  30,  30,  30,  30,
			20,  20,  20,  20,  20,  20,  20,  20,
			10,  10,  10,  10,  10,  10,  10,  10,
			10,  10,  10,  10,  10,  10,  10,  10,
			 0,   0,   0,   0,   0,   0,   0,   0
		};

		public static readonly int[] rookTableBlack =  {
			0,  0,  0,  0,  0,  0,  0,  0,
			5, 10, 10, 10, 10, 10, 10,  5,
			-5,  0,  0,  0,  0,  0,  0, -5,
			-5,  0,  0,  0,  0,  0,  0, -5,
			-5,  0,  0,  0,  0,  0,  0, -5,
			-5,  0,  0,  0,  0,  0,  0, -5,
			-5,  0,  0,  0,  0,  0,  0, -5,
			0,  0,  0,  5,  5,  0,  0,  0
		};
		public static readonly int[] knightTableBlack = {
			-50,-40,-30,-30,-30,-30,-40,-50,
			-40,-20,  0,  0,  0,  0,-20,-40,
			-30,  0, 10, 15, 15, 10,  0,-30,
			-30,  5, 15, 20, 20, 15,  5,-30,
			-30,  0, 15, 20, 20, 15,  0,-30,
			-30,  5, 10, 15, 15, 10,  5,-30,
			-40,-20,  0,  5,  5,  0,-20,-40,
			-50,-40,-30,-30,-30,-30,-40,-50,
		};
		public static readonly int[] bishopTableBlack =  {
			-20,-10,-10,-10,-10,-10,-10,-20,
			-10,  0,  0,  0,  0,  0,  0,-10,
			-10,  0,  5, 10, 10,  5,  0,-10,
			-10,  5,  5, 10, 10,  5,  5,-10,
			-10,  0, 10, 10, 10, 10,  0,-10,
			-10, 10, 10, 10, 10, 10, 10,-10,
			-10,  5,  0,  0,  0,  0,  5,-10,
			-20,-10,-10,-10,-10,-10,-10,-20,
		};
		public static readonly int[] queenTableBlack =  {
			-20,-10,-10, -5, -5,-10,-10,-20,
			-10,  0,  0,  0,  0,  0,  0,-10,
			-10,  0,  5,  5,  5,  5,  0,-10,
			-5,   0,  5,  5,  5,  5,  0, -5,
			 0,   0,  5,  5,  5,  5,  0, -5,
			-10,  5,  5,  5,  5,  5,  0,-10,
			-10,  0,  5,  0,  0,  0,  0,-10,
			-20,-10,-10, -5, -5,-10,-10,-20
		};
		public static readonly int[] kingStartTableBlack = 
		{
			-80, -70, -70, -70, -70, -70, -70, -80, 
			-60, -60, -60, -60, -60, -60, -60, -60, 
			-40, -50, -50, -60, -60, -50, -50, -40, 
			-30, -40, -40, -50, -50, -40, -40, -30, 
			-20, -30, -30, -40, -40, -30, -30, -20, 
			-10, -20, -20, -20, -20, -20, -20, -10, 
			20,  20,  -5,  -5,  -5,  -5,  20,  20, 
			20,  30,  10,   0,   0,  10,  30,  20
		};

		public static readonly int[] kingEndTableBlack = 
		{
			-20, -10, -10, -10, -10, -10, -10, -20,
			-5,   0,   5,   5,   5,   5,   0,  -5,
			-10, -5,   20,  30,  30,  20,  -5, -10,
			-15, -10,  35,  45,  45,  35, -10, -15,
			-20, -15,  30,  40,  40,  30, -15, -20,
			-25, -20,  20,  25,  25,  20, -20, -25,
			-30, -25,   0,   0,   0,   0, -25, -30,
			-50, -30, -30, -30, -30, -30, -30, -50
		};
    
        public static readonly int[] pawnStartTableWhite = {
			0,  0,  0,  0,  0,  0,  0,  0,
            5, 10, 10, -20, -20, 10, 10,  5,
            5, -5, -10,  0,  0, -10, -5,  5,
            0,  0,  0, 20, 20,  0,  0,  0,
            5,  5, 10, 25, 25, 10,  5,  5,
            10, 10, 20, 30, 30, 20, 10, 10,
            50, 50, 50, 50, 50, 50, 50, 50,
            0, 0, 0, 0, 0, 0, 0, 0
		};

        public static readonly int[] pawnEndTableWhite = {
			 0,   0,   0,   0,   0,   0,   0,   0,
			10,  10,  10,  10,  10,  10,  10,  10,
			10,  10,  10,  10,  10,  10,  10,  10,
			20,  20,  20,  20,  20,  20,  20,  20,
			30,  30,  30,  30,  30,  30,  30,  30,
			50,  50,  50,  50,  50,  50,  50,  50,
			80,  80,  80,  80,  80,  80,  80,  80,
			 0,   0,   0,   0,   0,   0,   0,   0
		};

		public static readonly int[] rookTableWhite =  {
			0, 0, 0, 5, 5, 0, 0, 0,
            -5, 0, 0, 0, 0, 0, 0, -5,
            -5, 0, 0, 0, 0, 0, 0, -5,
            -5, 0, 0, 0, 0, 0, 0, -5,
            -5, 0, 0, 0, 0, 0, 0, -5,
            -5, 0, 0, 0, 0, 0, 0, -5,
            5, 10, 10, 10, 10, 10, 10, 5,
            0, 0, 0, 0, 0, 0, 0, 0
		};
		public static readonly int[] knightTableWhite = {
			-50,-40,-30,-30,-30,-30,-40,-50,
			-40,-20,  0,  5,  5,  0,-20,-40,
			-30,  0, 10, 15, 15, 10,  0,-30,
			-30,  5, 15, 20, 20, 15,  5,-30,
			-30,  0, 15, 20, 20, 15,  0,-30,
			-30,  5, 10, 15, 15, 10,  5,-30,
			-40,-20,  0,  0,  0,  0,-20,-40,
			-50,-40,-30,-30,-30,-30,-40,-50,
		};
		public static readonly int[] bishopTableWhite =  {
			-20, -10, -10, -10, -10, -10, -10, -20,
            -10, 5, 0, 0, 0, 0, 5, -10,
            -10, 10, 10, 10, 10, 10, 10, -10,
            -10, 0, 10, 10, 10, 10, 0, -10,
            -10, 5, 5, 10, 10, 5, 5, -10,
            -10, 0, 5, 10, 10, 5, 0, -10,
            -10, 0, 0, 0, 0, 0, 0, -10,
            -20, -10, -10, -10, -10, -10, -10, -20
		};
		public static readonly int[] queenTableWhite =  {
			-20, -10, -10, -5, -5, -10, -10, -20,
            -10, 0, 0, 0, 0, 0, 0, -10,
            -10, 0, 5, 5, 5, 5, 0, -10,
            -5, 0, 5, 5, 5, 5, 0, -5,
            0, 0, 5, 5, 5, 5, 0, -5,
            -10, 5, 5, 5, 5, 5, 0, -10,
            -10, 0, 5, 0, 0, 0, 0, -10,
            -20, -10, -10, -5, -5, -10, -10, -20
		};
		public static readonly int[] kingStartTableWhite = 
		{
			20, 30, 10, 0, 0, 10, 30, 20,
            20, 20, 0, 0, 0, 0, 20, 20,
            -10, -20, -20, -20, -20, -20, -20, -10,
            20, -30, -30, -40, -40, -30, -30, -20,
            -30, -40, -40, -50, -50, -40, -40, -30,
            -30, -40, -40, -50, -50, -40, -40, -30,
            -30, -40, -40, -50, -50, -40, -40, -30,
            -30, -40, -40, -50, -50, -40, -40, -30
		};

		public static readonly int[] kingEndTableWhite = 
		{
			50, -30, -30, -30, -30, -30, -30, -50,
            -30, -30,  0,  0,  0,  0, -30, -30,
            -30, -10, 20, 30, 30, 20, -10, -30,
            -30, -10, 30, 40, 40, 30, -10, -30,
            -30, -10, 30, 40, 40, 30, -10, -30,
            -30, -10, 20, 30, 30, 20, -10, -30,
            -30, -20, -10,  0,  0, -10, -20, -30,
            -50, -40, -30, -20, -20, -30, -40, -50
		};
}