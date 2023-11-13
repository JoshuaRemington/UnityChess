using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Unity.Mathematics.math;

public class MoveGenerator
{
    public static ulong[] kingBitboards = new ulong[64];
    public static ulong[] knightBitboards = new ulong[64];
    public static ulong[,] pawnAttacks = new ulong[2,64];
    private static int currIndex;
    private static int player, enemyPlayer;
    
    //if generateForWhite, white is next to move
    private static bool generateForWhite = true; 
    private static bool inDoubleCheck = false;
    private static bool inCheck = false;
    private static ulong emptySquaresBitboard, friendlyBitboard, opponentBitboard, diagonalPinnedPieces, orthogonalPinnedPieces, opponentAttackMap, checkingSquares, pinRays;
    private static ulong enemyOrthogonalSliders, enemyDiagonalSliders;
    //2-7 = white piece bitboard, 8-13 = black piece bitboards
    public static Move lastMove = new Move(-1,-1);
    public static int friendlyKingSquare;
    public static int GenerateMoves(ref Move[] moveList, Bitboards bitboardObject)
    {
        inCheck = false;
        inDoubleCheck = false;
        moveList = new Move[256];
        currIndex = 0;
        pinRays = 0UL;
        orthogonalPinnedPieces = 0UL;
        diagonalPinnedPieces = 0UL;
        opponentAttackMap = 0UL;
        checkingSquares = 0UL;
        generateForWhite = bitboardObject.whiteTurn;
        player = generateForWhite ? 0:1;
        enemyPlayer = generateForWhite ? 1:0;
        emptySquaresBitboard = ~bitboardObject.occupiedSquares;
        if(generateForWhite)
        {
            friendlyBitboard = bitboardObject.pieces[0];
            opponentBitboard = bitboardObject.pieces[1];
            enemyOrthogonalSliders = bitboardObject.rooks[1] | bitboardObject.queens[1];
            enemyDiagonalSliders = bitboardObject.bishops[1] | bitboardObject.queens[1];
            friendlyKingSquare = bitboardObject.kings[0];
        }
        else{
            friendlyBitboard = bitboardObject.pieces[1];
            opponentBitboard = bitboardObject.pieces[0];
            enemyOrthogonalSliders = bitboardObject.rooks[0] | bitboardObject.queens[0];
            enemyDiagonalSliders = bitboardObject.bishops[0] | bitboardObject.queens[0];
            friendlyKingSquare = bitboardObject.kings[1];
        }
        DetermineCheckAndPins(bitboardObject);
        GenerateKingMoves(ref moveList, bitboardObject);
        if(!inDoubleCheck)
        {
            GeneratePawnMoves(ref moveList, bitboardObject);
            GenerateKnightMoves(ref moveList, bitboardObject);
            GenerateSlidingMoves(ref moveList, bitboardObject);
        }
        if(orthogonalPinnedPieces != 0 || diagonalPinnedPieces != 0)
            {
                Bitboards.printBitBoard(orthogonalPinnedPieces);
                Bitboards.printBitBoard(diagonalPinnedPieces);
            }
        return currIndex;
    }

    private static void DetermineCheckAndPins(Bitboards bitboardObject)
    {
        ulong kingRookRay = Magic.GetRookAttacks(bitboardObject.kings[player], bitboardObject.occupiedSquares);
        ulong kingBishopRay = Magic.GetBishopAttacks(bitboardObject.kings[player], bitboardObject.occupiedSquares);
        ulong friendlyNoKing = friendlyBitboard & ~bitboardObject.kingBitboards[player];
        while(enemyOrthogonalSliders != 0 && !inDoubleCheck)
        {
            int i = tzcnt(enemyOrthogonalSliders);
            enemyOrthogonalSliders &= enemyOrthogonalSliders-1;
            ulong testing = Magic.GetRookAttacks(i, bitboardObject.occupiedSquares);
            opponentAttackMap |= testing;
            if((testing & friendlyNoKing) != 0)
            {
                ulong intersection = kingRookRay & (testing & friendlyNoKing);
                if(intersection != 0)
                {
                    int j = tzcnt(intersection);
                    if((j % 8) == (bitboardObject.kings[player] % 8) && (j% 8) == (i % 8))
                        Bitboards.setSquare(ref orthogonalPinnedPieces, j);
                    else if((j / 8) == (bitboardObject.kings[player] / 8) && (j / 8) == (i / 8))
                        Bitboards.setSquare(ref orthogonalPinnedPieces, j);
                    
                    ulong tempWithoutPinned = bitboardObject.occupiedSquares & ~intersection;
                    pinRays |= Magic.GetRookAttacks(i, tempWithoutPinned) & Magic.GetRookAttacks(bitboardObject.kings[player], tempWithoutPinned);
                }
            }
            if((testing & bitboardObject.kingBitboards[player]) != 0)
            {
                inDoubleCheck = inCheck;
                inCheck = true;
                checkingSquares |= Magic.GetRookAttacks(i, bitboardObject.occupiedSquares) & Magic.GetRookAttacks(bitboardObject.kings[player], bitboardObject.occupiedSquares);
            }
        }

        while(enemyDiagonalSliders != 0 && !inDoubleCheck)
        {
            int i = tzcnt(enemyDiagonalSliders);
            enemyDiagonalSliders &= enemyDiagonalSliders-1;
            ulong testing = Magic.GetBishopAttacks(i, bitboardObject.occupiedSquares);
            opponentAttackMap |= testing;
            if((testing & friendlyNoKing) != 0)
            {
                ulong intersection = kingBishopRay & (testing & friendlyNoKing);
                if(intersection != 0)
                {
                    int j = tzcnt(intersection);
                    if((j % 9) == (bitboardObject.kings[player] % 9) && (j% 9) == (i % 9))
                        Bitboards.setSquare(ref diagonalPinnedPieces, j);
                    else if((j % 7) == (bitboardObject.kings[player] % 7) && (j % 7) == (i % 7))
                        Bitboards.setSquare(ref diagonalPinnedPieces, j);
                    
                    ulong tempWithoutPinned = bitboardObject.occupiedSquares & ~intersection;
                    pinRays |= Magic.GetBishopAttacks(i, tempWithoutPinned) & Magic.GetBishopAttacks(bitboardObject.kings[player], tempWithoutPinned);
                }
            }
            if((testing & bitboardObject.kingBitboards[player]) != 0)
            {
                inDoubleCheck = inCheck;
                inCheck = true;
                checkingSquares |= Magic.GetBishopAttacks(i, bitboardObject.occupiedSquares) & Magic.GetBishopAttacks(bitboardObject.kings[player], bitboardObject.occupiedSquares);
            }
        }

        ulong opponentPawns = bitboardObject.pawns[enemyPlayer];
        ulong opponentKnights = bitboardObject.knights[enemyPlayer];
        while(opponentPawns != 0)
        {
            int i = tzcnt(opponentPawns);
            opponentPawns &= opponentPawns-1;
            ulong checkForCheck = pawnAttacks[player,i];
            if((checkForCheck & bitboardObject.kingBitboards[player]) != 0) Bitboards.setSquare(ref checkingSquares, i);
            opponentAttackMap |= checkForCheck;
        }
        while(opponentKnights != 0)
        {
            int i = tzcnt(opponentKnights);
            opponentKnights &= opponentKnights-1;
            ulong checkForCheck = knightBitboards[i];
            if((checkForCheck & bitboardObject.kingBitboards[player]) != 0) Bitboards.setSquare(ref checkingSquares, i);
            opponentAttackMap |= checkForCheck;
        }
        opponentAttackMap |= kingBitboards[bitboardObject.kings[enemyPlayer]];
    }

    private static void GeneratePawnMoves(ref Move[] moveList, Bitboards bitboardObject)
    {
        ulong pawnBoard = bitboardObject.pawns[player];
        ulong push = new ulong();
        ulong doublePush = new ulong();
        int pushDirection;

        if(generateForWhite)
        {
            pushDirection = 8;
            push |= (pawnBoard & ~(diagonalPinnedPieces)) << 8;
            push &= emptySquaresBitboard;
            doublePush = push & Bitboards.Rank3;
            doublePush = doublePush << 8;
            doublePush &= emptySquaresBitboard;
        }
        else
        {
            pushDirection = -8;
            push |= (pawnBoard & ~(diagonalPinnedPieces)) >> 8;
            push &= emptySquaresBitboard;
            doublePush = push & Bitboards.Rank6;
            doublePush = doublePush >> 8;
            doublePush &= emptySquaresBitboard;
        }
        if(inCheck)
        {
            push &= checkingSquares;
            doublePush &= checkingSquares;
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
        pawnBoard &= ~(orthogonalPinnedPieces);
        if(lastMove.flag == Move.PawnTwoUpFlag)
        {
            ulong enPassantSquares = 1ul << lastMove.targetSquare+1;
            enPassantSquares |= 1ul << lastMove.targetSquare-1;
            enPassantSquares &= pawnBoard;
            if(inCheck) enPassantSquares &= checkingSquares;
            while(enPassantSquares != 0)
            {
                int i = tzcnt(enPassantSquares);
                enPassantSquares &= enPassantSquares-1;
                int landingSquare = lastMove.targetSquare + pushDirection;
                Move m = new Move(i, landingSquare, Move.EnPassantCaptureFlag, lastMove.targetSquare);
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
            if(inCheck) attacks &= checkingSquares;
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
        ulong knightBoard = bitboardObject.knights[player];
        knightBoard &= ~(diagonalPinnedPieces) | ~(orthogonalPinnedPieces);
        while(knightBoard != 0)
        {
            int i = tzcnt(knightBoard);
            knightBoard &= (knightBoard-1);
            ulong knightMove = knightBitboards[i];
            knightMove &= ~friendlyBitboard;
            if(inCheck) knightMove &= checkingSquares;
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
        ulong diagonalBoard = bitboardObject.bishops[player] | bitboardObject.queens[player];
        ulong straightBoard = bitboardObject.rooks[player] | bitboardObject.queens[player];

        while(diagonalBoard != 0) {
            ulong diagonalMove = new ulong();
            int i = tzcnt(diagonalBoard);
            diagonalBoard &= diagonalBoard-1;
            diagonalMove = Magic.GetBishopAttacks(i, bitboardObject.occupiedSquares);
            diagonalMove &= ~friendlyBitboard;
            if(inCheck) diagonalMove &= checkingSquares;
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
            straightMove = Magic.GetRookAttacks(i, bitboardObject.occupiedSquares);
            straightMove &= ~friendlyBitboard;
            if(inCheck) straightMove &= checkingSquares;
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
        ulong kingMoves = kingBitboards[friendlyKingSquare];
        kingMoves &= (~friendlyBitboard & ~opponentAttackMap);
        while(kingMoves != 0)
        {
            int j = tzcnt(kingMoves);
            kingMoves &= kingMoves - 1;
            Move m = new Move(friendlyKingSquare, j);
            moveList[currIndex++] = m;
        }

        if(inCheck) return;

        if(generateForWhite)
        {
            ulong test1 = bitboardObject.occupiedSquares & bitboardObject.whiteKingCastleMask;
            ulong test2 = bitboardObject.occupiedSquares & bitboardObject.whiteQueenCastleMask;
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
            ulong test1 = bitboardObject.occupiedSquares & bitboardObject.blackKingCastleMask;
            ulong test2 = bitboardObject.occupiedSquares & bitboardObject.blackQueenCastleMask;
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