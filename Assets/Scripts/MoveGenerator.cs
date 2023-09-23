using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Unity.Mathematics.math;

public class MoveGenerator
{
    /*  0-13 for bitboard index correlation in array: 0 = white, BlackKing = 13
        White,
        Black,
        WhitePawn,
        WhiteKnight,
        WhiteBishop,
        WhiteRook,
        WhiteQueen,
        WhiteKing,
        BlackPawn,
        BlackKnight,
        BlackBishop,
        BlackRook,
        BlackQueen,
        BlackKing
    */
    public const int MaxNumberMoves = 218;
    private static Move[] moveList;
    
    //if generateForWhite, white is next to move
    private static bool generateForWhite = true; 
    private static ulong friendlyBitboard, opponentBitboard, occupiedSquaresBitboard, emptySquaresBitboard;

    //2-7 = white piece bitboard, 8-13 = black piece bitboards
    private static int startPieceIndex;
    public static Move[] GenerateMoves(ulong[] board, bool whiteToPlay)
    {
        moveList = new Move[MaxNumberMoves];
        generateForWhite = whiteToPlay;
        occupiedSquaresBitboard = board[0] & board[1];
        emptySquaresBitboard = ~occupiedSquaresBitboard;
        if(generateForWhite)
        {
            friendlyBitboard = board[0];
            opponentBitboard = board[1];
            startPieceIndex = 2;

        }
        else{
            friendlyBitboard = board[1];
            opponentBitboard = board[0];
            startPieceIndex = 8;
        }
        GeneratePawnMoves(board);

        
        return moveList;
    }

    private static void GeneratePawnMoves(ulong[] board)
    {
        //for direction, if white, we increase numbers to push pawns, if black, we decrease to push pawns
        ulong pawnPush = board[startPieceIndex];
        Bitboards.printBitBoard(board[startPieceIndex]);
        Bitboards.printBitBoard(pawnPush);

        if(generateForWhite)
        {
            pawnPush |= pawnPush << 8;
            pawnPush &= emptySquaresBitboard;
            pawnPush |= pawnPush << 8;
            pawnPush &= emptySquaresBitboard;
        }
        else
        {
            pawnPush |= pawnPush >> 8;
            pawnPush &= emptySquaresBitboard;
            pawnPush |= pawnPush >> 8;
            pawnPush &= emptySquaresBitboard;
        }
        while(pawnPush != 0)
        {
            int i = tzcnt(pawnPush);
            pawnPush &= (pawnPush-1);
        }
    }
}