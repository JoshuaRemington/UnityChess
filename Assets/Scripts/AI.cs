using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Unity.Mathematics.math;

public class AI
{
    private static ulong numberOfNodes;
    public static Move rootNegaMax (int depth, ref Bitboards bitboard)
    {
        numberOfNodes = 0;
        Move[] moves = new Move[256];
        Move bestMove = Move.nullMove;
        ulong num_moves = new ulong();
        int alpha = int.MinValue;
        int beta = int.MaxValue;
        num_moves = MoveGenerator.GenerateMoves(ref moves, bitboard);
        if(num_moves == 0) return Move.nullMove;
        for(ulong i = 0; i < num_moves; i++)
        {
            Move test = moves[i];
            int captureIndex = bitboard.playMove(test);
            int score = -negaMax(depth-1, -beta, -alpha,ref bitboard, test);
            bitboard.undoMove(test, captureIndex);
            if(score > alpha) 
            {
                alpha = score;
                bestMove = moves[i];
            }
        }
        Debug.Log("AI leaf nodes reached: " + numberOfNodes);
        return bestMove;
    }

    public static int negaMax(int depth, int alpha, int beta, ref Bitboards bitboard, Move parentMove)
    {
        if(depth == 0){numberOfNodes++; return evaluate(ref bitboard);}
        Move[] moves = new Move[256];
        ulong num_moves = new ulong();
        int score = int.MinValue;
        MoveGenerator.lastMove = parentMove;
        num_moves = MoveGenerator.GenerateMoves(ref moves, bitboard);
        for(ulong i = 0; i < num_moves; i++)
        {
            Move test = moves[i];
            int captureIndex = bitboard.playMove(test);
            score = System.Math.Max(score,-negaMax(depth-1, -beta, -alpha, ref bitboard, test));
            bitboard.undoMove(test, captureIndex);
            alpha = System.Math.Max(alpha, score);
            if(alpha >= beta) break;
        }
        return score;
    }
    
    public static int evaluate(ref Bitboards bitboard)
    {
        int player = bitboard.whiteTurn ? 0 : 1;
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
            value += 100;
            value += pawnStartTable[i];
        }
        while(knightValue != 0)
        {
            int i = tzcnt(knightValue);
            knightValue &= knightValue - 1;
            value += 290;
            value += knightTable[i];
        }
        while(bishopValue != 0)
        {
            int i = tzcnt(bishopValue);
            bishopValue &= bishopValue - 1;
            value += 310;
            value += bishopTable[i];
        }
        while(rookValue != 0)
        {
            int i = tzcnt(rookValue);
            rookValue &= rookValue - 1;
            value += 500;
            value += rookTable[i];
        }
        while(queenValue != 0)
        {
            int i = tzcnt(queenValue);
            queenValue &= queenValue - 1;
            value += 900;
            value += queenTable[i];
        }
        value += kingStartTable[kingValue];

        return value;
    }

    public static void orderMoves(ref Move[] moves, ulong num_moves)
    {
        Move[] reorderedMoveList = new Move[num_moves];
        return;
    }

    public static readonly int[] pawnStartTable = {
			 0,   0,   0,   0,   0,   0,   0,   0,
			50,  50,  50,  50,  50,  50,  50,  50,
			10,  10,  20,  30,  30,  20,  10,  10,
			 5,   5,  10,  25,  25,  10,   5,   5,
			 0,   0,   0,  20,  20,   0,   0,   0,
			 5,  -5, -10,   0,   0, -10,  -5,   5,
			 5,  10,  10, -20, -20,  10,  10,   5,
			 0,   0,   0,   0,   0,   0,   0,   0
		};

		public static readonly int[] pawnEndTable = {
			 0,   0,   0,   0,   0,   0,   0,   0,
			80,  80,  80,  80,  80,  80,  80,  80,
			50,  50,  50,  50,  50,  50,  50,  50,
			30,  30,  30,  30,  30,  30,  30,  30,
			20,  20,  20,  20,  20,  20,  20,  20,
			10,  10,  10,  10,  10,  10,  10,  10,
			10,  10,  10,  10,  10,  10,  10,  10,
			 0,   0,   0,   0,   0,   0,   0,   0
		};

		public static readonly int[] rookTable =  {
			0,  0,  0,  0,  0,  0,  0,  0,
			5, 10, 10, 10, 10, 10, 10,  5,
			-5,  0,  0,  0,  0,  0,  0, -5,
			-5,  0,  0,  0,  0,  0,  0, -5,
			-5,  0,  0,  0,  0,  0,  0, -5,
			-5,  0,  0,  0,  0,  0,  0, -5,
			-5,  0,  0,  0,  0,  0,  0, -5,
			0,  0,  0,  5,  5,  0,  0,  0
		};
		public static readonly int[] knightTable = {
			-50,-40,-30,-30,-30,-30,-40,-50,
			-40,-20,  0,  0,  0,  0,-20,-40,
			-30,  0, 10, 15, 15, 10,  0,-30,
			-30,  5, 15, 20, 20, 15,  5,-30,
			-30,  0, 15, 20, 20, 15,  0,-30,
			-30,  5, 10, 15, 15, 10,  5,-30,
			-40,-20,  0,  5,  5,  0,-20,-40,
			-50,-40,-30,-30,-30,-30,-40,-50,
		};
		public static readonly int[] bishopTable =  {
			-20,-10,-10,-10,-10,-10,-10,-20,
			-10,  0,  0,  0,  0,  0,  0,-10,
			-10,  0,  5, 10, 10,  5,  0,-10,
			-10,  5,  5, 10, 10,  5,  5,-10,
			-10,  0, 10, 10, 10, 10,  0,-10,
			-10, 10, 10, 10, 10, 10, 10,-10,
			-10,  5,  0,  0,  0,  0,  5,-10,
			-20,-10,-10,-10,-10,-10,-10,-20,
		};
		public static readonly int[] queenTable =  {
			-20,-10,-10, -5, -5,-10,-10,-20,
			-10,  0,  0,  0,  0,  0,  0,-10,
			-10,  0,  5,  5,  5,  5,  0,-10,
			-5,   0,  5,  5,  5,  5,  0, -5,
			0,    0,  5,  5,  5,  5,  0, -5,
			-10,  5,  5,  5,  5,  5,  0,-10,
			-10,  0,  5,  0,  0,  0,  0,-10,
			-20,-10,-10, -5, -5,-10,-10,-20
		};
		public static readonly int[] kingStartTable = 
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

		public static readonly int[] kingEndTable = 
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
}