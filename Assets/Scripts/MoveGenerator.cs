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
    public static ulong[] kingBitboards = new ulong[64];
    public static ulong[] knightBitboards = new ulong[64];
    public static ulong[] pawnBitboards = new ulong[64];
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
        occupiedSquaresBitboard = board[0] | board[1];
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
        GenerateKnightMoves(board);
        GenerateSlidingMoves(board);
        GenerateKingMoves(board);
        
        return moveList;
    }

    private static void GeneratePawnMoves(ulong[] board)
    {
        //for direction, if white, we increase numbers to push pawns, if black, we decrease to push pawns
        ulong pawnPush = board[startPieceIndex++]; 
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
            pawnDoublePush = pawnDoublePush << 8;
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
            pawnDoublePush = pawnDoublePush >> 8;
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
            //Debug.Log(i-pushValue);
            Move m = new Move(i-pushValue, i);
            moveList[currIndex++] = m;
        }
        while(pawnDoublePush != 0)
        {
            int i = tzcnt(pawnDoublePush);
            pawnDoublePush &= (pawnDoublePush-1);
            //Debug.Log(i-(pushValue*2));
            Move m = new Move(i-(pushValue * 2), i);
            moveList[currIndex++] = m;
        }
        while(pawnCaptureRight != 0)
        {
            int i = tzcnt(pawnCaptureRight);
            pawnCaptureRight &= (pawnCaptureRight-1);
           // Debug.Log(i-(pushDirection * 7));
            Move m = new Move(i-(pushDirection * 7), i);
            moveList[currIndex++] = m;  
        }
        while(pawnCaptureLeft != 0)
        {
            int i = tzcnt(pawnCaptureLeft);
            pawnCaptureLeft &= (pawnCaptureLeft-1);
            //Debug.Log(i-(pushDirection * 9));
            Move m = new Move(i-(pushDirection * 9), i);
            moveList[currIndex++] = m;
        }
    }

    private static void GenerateKnightMoves(ulong[] board)
    {
        // increments, 6, 10, 15, 17
        ulong knightBoard = board[startPieceIndex++];

        while(knightBoard != 0)
        {
            ulong knightMove = new ulong();
            ulong knightSquare = new ulong();
            int i = tzcnt(knightBoard);
            knightBoard &= (knightBoard-1);
            Bitboards.setSquare(ref knightSquare, i);
            knightMove |= (knightSquare >> 6) & Bitboards.notABFiles;
            knightMove |= (knightSquare >> 10) & Bitboards.notGHFiles;
            knightMove |= (knightSquare >> 15) & Bitboards.notAFile;
            knightMove |= (knightSquare >> 17) & Bitboards.notHFile;
            knightMove |= (knightSquare << 6) & Bitboards.notGHFiles;
            knightMove |= (knightSquare << 10) & Bitboards.notABFiles;
            knightMove |= (knightSquare << 15) & Bitboards.notHFile;
            knightMove |= (knightSquare << 17) & Bitboards.notAFile;
            knightMove &= ~friendlyBitboard;
            while(knightMove != 0)
            {
                int j = tzcnt(knightMove);
                knightMove &= (knightMove-1);
                Move m = new Move(i, j);
                moveList[currIndex++] = m;
            }
        }
    }
    private static void GenerateSlidingMoves(ulong[] board) {
        ulong diagonalBoard = board[startPieceIndex++];
        ulong straightBoard = board[startPieceIndex++];
        diagonalBoard |= board[startPieceIndex];
        straightBoard |= board[startPieceIndex++];
        occupiedSquaresBitboard = board[0] | board[1];

        while(diagonalBoard != 0) {
            ulong diagonalMove = new ulong();
            int i = tzcnt(diagonalBoard);
            diagonalBoard &= diagonalBoard-1;
            diagonalMove = Magic.GetBishopAttacks(i, occupiedSquaresBitboard);
            diagonalMove &= ~friendlyBitboard;
            while(diagonalMove != 0) {
                int j = tzcnt(diagonalMove);
                diagonalMove &= (diagonalMove-1);
                Move m = new Move(i, j);
                moveList[currIndex++] = m;
            }
        }
        while(straightBoard != 0) {
            ulong straightMove = new ulong();
            int i = tzcnt(straightBoard);
            straightBoard &= straightBoard-1;
            straightMove = Magic.GetRookAttacks(i, occupiedSquaresBitboard);
            straightMove &= ~friendlyBitboard;
            while(straightMove != 0) {
                int j = tzcnt(straightMove);
                straightMove &= (straightMove-1);
                Move m = new Move(i, j);
                moveList[currIndex++] = m;
            }
        }
    }
    private static void GenerateKingMoves(ulong[] board)
    {
        ulong kingBoard = board[startPieceIndex];
        int i = tzcnt(kingBoard);
        ulong kingMoves = kingBitboards[i];
        kingMoves &= ~friendlyBitboard;
        while(kingMoves != 0)
        {
            int j = tzcnt(kingMoves);
            kingMoves &= kingMoves - 1;
            Move m = new Move(i, j);
            moveList[currIndex++] = m;
        }
    }

    public void StoreMoves()
    {
        for(int i = 0; i < 64; i++)
        {
            ulong knightBitboard = 1ul << i;
            ulong kingBitboard = knightBitboard;
            ulong pawnBitboard = knightBitboard;

            ulong kingMoves = new ulong();
            kingMoves |= kingBitboard << 1 & Bitboards.notAFile;
            kingMoves |= kingBitboard << 7 & Bitboards.notHFile;
            kingMoves |= kingBitboard << 8;
            kingMoves |= kingBitboard << 9 & Bitboards.notAFile;
            kingMoves |= kingBitboard >> 1 & Bitboards.notHFile;
            kingMoves |= kingBitboard >> 7 & Bitboards.notAFile;
            kingMoves |= kingBitboard >> 8;
            kingMoves |= kingBitboard >> 9 & Bitboards.notHFile;
            kingBitboards[i] = kingMoves;
        }
    }
}