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
    private static int currIndex;
    public static Move[] moveList = new Move[MaxNumberMoves];
    
    //if generateForWhite, white is next to move
    private static bool generateForWhite = true; 
    private static ulong friendlyBitboard, opponentBitboard, occupiedSquaresBitboard, emptySquaresBitboard;

    //2-7 = white piece bitboard, 8-13 = black piece bitboards
    private static int startPieceIndex;
    public static Move[] GenerateMoves(ulong[] board, bool whiteToPlay)
    {
        moveList = new Move[MaxNumberMoves];
        currIndex = 0;
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
        ulong pawnMoveStraight = new ulong(); //This is for when pawns move directly forward 1 square
        ulong pawnDoublePush = new ulong();  //This is for when pawns are able to move forward 2 squares
        ulong pawnCaptureRight = new ulong(); //for when a pawn can capture an opponent piece to the right diagonal of pawn facing direction
        ulong pawnCaptureLeft = new ulong(); //for when a pawn can capture an opponent piece to the left diagonal of pawn facing direction
        int pushDirection = generateForWhite ? 1: -1;
        int pushValue = pushDirection * 8;
        if(generateForWhite)
        {
            pawnMoveStraight |= pawnPush << 8;
            pawnMoveStraight &= emptySquaresBitboard;
            pawnDoublePush = pawnMoveStraight & Bitboards.Rank3;
            pawnDoublePush |= pawnDoublePush << 8;
            pawnDoublePush &= emptySquaresBitboard;
            pawnCaptureRight |= pawnPush << 7;
            pawnCaptureRight &= opponentBitboard;
            pawnCaptureRight &= Bitboards.notHFile;
            pawnCaptureLeft |= pawnPush << 9;
            pawnCaptureLeft &= opponentBitboard;
            pawnCaptureLeft &= Bitboards.notAFile;
        }
        else
        {
            pawnMoveStraight |= pawnPush >> 8;
            pawnMoveStraight &= emptySquaresBitboard;
            pawnDoublePush = pawnMoveStraight & Bitboards.Rank6;
            pawnDoublePush |= pawnDoublePush >> 8;
            pawnDoublePush &= emptySquaresBitboard;
            pawnCaptureRight |= pawnPush >> 7;
            pawnCaptureRight &= opponentBitboard;
            pawnCaptureRight &= Bitboards.notHFile;
            pawnCaptureLeft |= pawnPush >> 9;
            pawnCaptureLeft &= opponentBitboard;
            pawnCaptureLeft &= Bitboards.notAFile;
        }
        while(pawnMoveStraight != 0)
        {
            int i = tzcnt(pawnMoveStraight);
            pawnMoveStraight &= (pawnMoveStraight-1);
            Move m = new Move(i-pushValue, i);
            moveList[currIndex++] = m;
        }
        while(pawnDoublePush != 0)
        {
            int i = tzcnt(pawnDoublePush);
            pawnDoublePush &= (pawnDoublePush-1);
            Move m = new Move(i-(pushValue * 2), i);
            moveList[currIndex++] = m;
        }
        while(pawnCaptureRight != 0)
        {
            int i = tzcnt(pawnCaptureRight);
            pawnCaptureRight &= (pawnCaptureRight-1);
            Move m = new Move(i-(pushDirection * 7), i);
            moveList[currIndex++] = m;  
        }
        while(pawnCaptureLeft != 0)
        {
            int i = tzcnt(pawnCaptureLeft);
            pawnCaptureLeft &= (pawnCaptureLeft-1);
            Move m = new Move(i-(pushDirection * 9), i);
            moveList[currIndex++] = m;
        }
    }
}