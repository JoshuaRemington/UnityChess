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
    public static ulong[,] pawnAttacks = new ulong[2,64];
    public static ulong[] attackedSquares = new ulong[2];
    private static int currIndex;
    private static int player;
    
    //if generateForWhite, white is next to move
    private static bool generateForWhite = true; 
    private static ulong friendlyBitboard, opponentBitboard, occupiedSquaresBitboard, emptySquaresBitboard;

    //2-7 = white piece bitboard, 8-13 = black piece bitboards
    private static int startPieceIndex;
    public static Move lastMove = new Move(-1,-1);
    public static int GenerateMoves(ref Move[] moveList, Bitboards bitboardObject)
    {
        moveList = new Move[256];
        currIndex = 0;
        generateForWhite = bitboardObject.whiteTurn;
        player = generateForWhite ? 0:1;
        occupiedSquaresBitboard = bitboardObject.bitboards[0] | bitboardObject.bitboards[1];
        emptySquaresBitboard = ~occupiedSquaresBitboard;
        if(generateForWhite)
        {
            friendlyBitboard = bitboardObject.bitboards[0];
            opponentBitboard = bitboardObject.bitboards[1];
            startPieceIndex = 2;

        }
        else{
            friendlyBitboard = bitboardObject.bitboards[1];
            opponentBitboard = bitboardObject.bitboards[0];
            startPieceIndex = 8;
        }
        GeneratePawnMoves(ref moveList, bitboardObject);
        GenerateKnightMoves(ref moveList, bitboardObject);
        GenerateSlidingMoves(ref moveList, bitboardObject);
        GenerateKingMoves(ref moveList, bitboardObject);
        return currIndex;
    }

    private static void GeneratePawnMoves(ref Move[] moveList, Bitboards bitboardObject)
    {
        ulong pawnBoard = bitboardObject.bitboards[startPieceIndex++];
        ulong push = new ulong();
        ulong doublePush = new ulong();
        int pushDirection;

        if(generateForWhite)
        {
            pushDirection = 8;
            push |= pawnBoard << 8;
            push &= emptySquaresBitboard;
            doublePush = push & Bitboards.Rank3;
            doublePush = doublePush << 8;
            doublePush &= emptySquaresBitboard;
        }
        else
        {
            pushDirection = -8;
            push |= pawnBoard >> 8;
            push &= emptySquaresBitboard;
            doublePush = push & Bitboards.Rank6;
            doublePush = doublePush >> 8;
            doublePush &= emptySquaresBitboard;
        }
        while(push != 0)
        {
            int j = tzcnt(push);
            push &= push-1;
            Move m = new Move(j-(pushDirection), j);
            moveList[currIndex++] = m;
        }
        while(doublePush != 0)
        {
                int j = tzcnt(doublePush);
                doublePush &= doublePush - 1;
                Move m = new Move(j-(pushDirection * 2), j, Move.PawnTwoUpFlag);
                moveList[currIndex++] = m;
        }

        if(lastMove.flag == Move.PawnTwoUpFlag)
        {
            ulong enPassantSquares = 1ul << lastMove.targetSquare+1;
            enPassantSquares |= 1ul << lastMove.targetSquare-1;
            enPassantSquares &= pawnBoard;
            while(enPassantSquares != 0)
            {
                int i = tzcnt(enPassantSquares);
                enPassantSquares &= enPassantSquares-1;
                int landingSquare = lastMove.targetSquare + pushDirection;
                Move m = new Move(i, landingSquare, Move.EnPassantCaptureFlag);
                moveList[currIndex++] = m;
            }
        }

        while(pawnBoard != 0)
        {
            int i = tzcnt(pawnBoard);
            pawnBoard &= pawnBoard - 1;
            ulong attacks;
            attacks = pawnAttacks[player, i];
            attacks &= opponentBitboard;
            attackedSquares[player] |= attacks;

            while(attacks != 0)
            {
                int j = tzcnt(attacks);
                attacks &= attacks - 1;
                Move m = new Move(i, j);
                moveList[currIndex++] = m;
            }
        }
    }

    private static void GenerateKnightMoves(ref Move[] moveList, Bitboards bitboardObject)
    {
        // increments, 6, 10, 15, 17
        ulong knightBoard = bitboardObject.bitboards[startPieceIndex++];

        while(knightBoard != 0)
        {
            int i = tzcnt(knightBoard);
            knightBoard &= (knightBoard-1);
            ulong knightMove = knightBitboards[i];
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
    private static void GenerateSlidingMoves(ref Move[] moveList, Bitboards bitboardObject) {
        ulong diagonalBoard = bitboardObject.bitboards[startPieceIndex++];
        ulong straightBoard = bitboardObject.bitboards[startPieceIndex++];
        diagonalBoard |= bitboardObject.bitboards[startPieceIndex];
        straightBoard |= bitboardObject.bitboards[startPieceIndex++];

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
    private static void GenerateKingMoves(ref Move[] moveList, Bitboards bitboardObject)
    {
        ulong kingBoard = bitboardObject.bitboards[startPieceIndex];
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

        if(generateForWhite)
        {
            ulong test1 = occupiedSquaresBitboard & bitboardObject.whiteKingCastleMask;
            ulong test2 = occupiedSquaresBitboard & bitboardObject.whiteQueenCastleMask;
            if(bitboardObject.whiteKingCastle && test1 == 0)
            {
                Move m = new Move(4, 6, Move.CastleFlag);
                moveList[currIndex++] = m;
            }
            if(bitboardObject.whiteQueenCastle && test2 == 0)
            {
                Move m = new Move(4, 2, Move.CastleFlag);
                moveList[currIndex++] = m;
            }
        }
        else
        {
            ulong test1 = occupiedSquaresBitboard & bitboardObject.blackKingCastleMask;
            ulong test2 = occupiedSquaresBitboard & bitboardObject.blackQueenCastleMask;
            if(bitboardObject.blackKingCastle && test1 == 0)
            {
                Move m = new Move(60, 62, Move.CastleFlag);
                moveList[currIndex++] = m;
            }
            if(bitboardObject.blackQueenCastle && test2 == 0)
            {
                Move m = new Move(60, 58, Move.CastleFlag);
                moveList[currIndex++] = m;
            }
        }
    }

    public void StoreMoves()
    {
        for(int i = 0; i < 64; i++)
        {
            ulong tempBitboard = 1ul << i;

            ulong kingMoves = new ulong();
            kingMoves |= tempBitboard << 1 & Bitboards.notAFile;
            kingMoves |= tempBitboard << 7 & Bitboards.notHFile;
            kingMoves |= tempBitboard << 8;
            kingMoves |= tempBitboard << 9 & Bitboards.notAFile;
            kingMoves |= tempBitboard >> 1 & Bitboards.notHFile;
            kingMoves |= tempBitboard >> 7 & Bitboards.notAFile;
            kingMoves |= tempBitboard >> 8;
            kingMoves |= tempBitboard >> 9 & Bitboards.notHFile;
            kingBitboards[i] = kingMoves;

            ulong knightMoves = new ulong();
            knightMoves |= (tempBitboard >> 6) & Bitboards.notABFiles;
            knightMoves |= (tempBitboard >> 10) & Bitboards.notGHFiles;
            knightMoves |= (tempBitboard >> 15) & Bitboards.notAFile;
            knightMoves |= (tempBitboard >> 17) & Bitboards.notHFile;
            knightMoves |= (tempBitboard << 6) & Bitboards.notGHFiles;
            knightMoves |= (tempBitboard << 10) & Bitboards.notABFiles;
            knightMoves |= (tempBitboard << 15) & Bitboards.notHFile;
            knightMoves |= (tempBitboard << 17) & Bitboards.notAFile;
            knightBitboards[i] = knightMoves;

            ulong whitePawnAttackMoves = new ulong();
            whitePawnAttackMoves |= tempBitboard << 7 & Bitboards.notHFile;
            whitePawnAttackMoves |= tempBitboard << 9 & Bitboards.notAFile;
            pawnAttacks[0,i] = whitePawnAttackMoves;

            ulong blackPawnAttackMoves = new ulong();
            blackPawnAttackMoves |= tempBitboard >> 7 & Bitboards.notAFile;
            blackPawnAttackMoves |= tempBitboard >> 9 & Bitboards.notHFile;
            pawnAttacks[1,i] = blackPawnAttackMoves;
        }
    }
}