using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Unity.Mathematics.math;

public class AI
{
    public static Move rootNegaMax (int depth, ref Bitboards bitboard)
    {
        Move[] moves = new Move[256];
        Move bestMove = null;
        ulong num_moves = new ulong();
        int max = -int.MaxValue;
        num_moves = MoveGenerator.GenerateMoves(ref moves, bitboard);
        for(ulong i = 0; i < num_moves; i++)
        {
           Move test = moves[i];
           int captureIndex = bitboard.playMove(test);
           int score = -negaMax(depth-1, ref bitboard);
           bitboard.undoMove(test, captureIndex);
           if(score > max) 
           {
                max = score;
                bestMove = moves[i];
           }
        }
        return bestMove;
    }

    public static int negaMax(int depth, ref Bitboards bitboard)
    {
        if(depth == 0) return evaluate(bitboard);
        Move[] moves = new Move[256];
        ulong num_moves = new ulong();
        int max = -int.MaxValue;
        num_moves = MoveGenerator.GenerateMoves(ref moves, bitboard);
        for(ulong i = 0; i < num_moves; i++)
        {
           Move test = moves[i];
           int captureIndex = bitboard.playMove(test);
           int score = -negaMax(depth-1, ref bitboard);
           bitboard.undoMove(test, captureIndex);
           if(score > max) max = score;
        }
        return max;
    }
    
    public static int evaluate(Bitboards bitboard)
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
        }
        while(knightValue != 0)
        {
            int i = tzcnt(knightValue);
            knightValue &= knightValue - 1;
            value += 290;
        }
        while(bishopValue != 0)
        {
            int i = tzcnt(bishopValue);
            bishopValue &= bishopValue - 1;
            value += 310;
        }
        while(rookValue != 0)
        {
            int i = tzcnt(rookValue);
            rookValue &= rookValue - 1;
            value += 500;
        }
        while(queenValue != 0)
        {
            int i = tzcnt(queenValue);
            queenValue &= queenValue - 1;
            value += 900;
        }
        return value;
    }
}