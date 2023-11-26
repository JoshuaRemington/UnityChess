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
    private static ulong emptySquaresBitboard, friendlyBitboard, opponentBitboard, diagonalPinnedPieces, orthogonalPinnedPieces, opponentAttackMap, checkingSquares, orthogonalPinRays, diagonalPinRays, raysOnKingInCheck;
    private static ulong enemyOrthogonalSliders, enemyDiagonalSliders;
    private static ulong whiteKingRookCheck, blackKingRookCheck, whiteQueenRookCheck, blackQueenRookCheck;
    //2-7 = white piece bitboard, 8-13 = black piece bitboards
    public static Move lastMove = new Move(-1,-1);
    public static int friendlyKingSquare;
    public static int GenerateMoves(ref Move[] moveList, Bitboards bitboardObject)
    {
        inCheck = false;
        inDoubleCheck = false;
        moveList = new Move[256];
        currIndex = 0;
        diagonalPinRays = 0UL;
        orthogonalPinRays = 0UL;
        orthogonalPinnedPieces = 0UL;
        diagonalPinnedPieces = 0UL;
        opponentAttackMap = 0UL;
        checkingSquares = 0UL;
        raysOnKingInCheck = 0UL;
        generateForWhite = bitboardObject.whiteTurn;
        player = generateForWhite ? 0:1;
        enemyPlayer = generateForWhite ? 1:0;
        emptySquaresBitboard = ~bitboardObject.occupiedSquares;
        if(bitboardObject.kingBitboards[player] == 0) return 0;
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
        return currIndex;
    }

    private static void DetermineCheckAndPins(Bitboards bitboardObject)
    {
        ulong kingRookRay = Magic.GetRookAttacks(bitboardObject.kings[player], bitboardObject.occupiedSquares);
        ulong kingBishopRay = Magic.GetBishopAttacks(bitboardObject.kings[player], bitboardObject.occupiedSquares);
        ulong friendlyNoKing = friendlyBitboard & ~bitboardObject.kingBitboards[player];
        ulong copyOfEnemyOrthogonalSliders = enemyOrthogonalSliders;
        while(copyOfEnemyOrthogonalSliders != 0 && !inDoubleCheck)
        {
            int i = tzcnt(copyOfEnemyOrthogonalSliders);
            copyOfEnemyOrthogonalSliders &= copyOfEnemyOrthogonalSliders-1;
            ulong testing = Magic.GetRookAttacks(i, bitboardObject.occupiedSquares);
            opponentAttackMap |= testing;
            if((testing & friendlyNoKing) != 0)
            {
                ulong intersection = kingRookRay & (testing & friendlyNoKing);
                if(intersection != 0)
                {
                    int j = tzcnt(intersection);
                    bool createPin = false;
                    if((j % 8) == (bitboardObject.kings[player] % 8) && (j% 8) == (i % 8))
                    {
                        Bitboards.setSquare(ref orthogonalPinnedPieces, j);
                        createPin = true;
                    }
                    else if((j / 8) == (bitboardObject.kings[player] / 8) && (j / 8) == (i / 8))
                    {
                        Bitboards.setSquare(ref orthogonalPinnedPieces, j);
                        createPin = true;
                    }
                    
                    if(createPin)
                    {
                        ulong tempWithoutPinned = bitboardObject.occupiedSquares & ~intersection;
                        orthogonalPinRays |= Magic.GetRookAttacks(i, tempWithoutPinned) & Magic.GetRookAttacks(bitboardObject.kings[player], tempWithoutPinned);
                        Bitboards.setSquare(ref orthogonalPinRays, i);
                    }
                }
            }
            if((testing & bitboardObject.kingBitboards[player]) != 0)
            {
                inDoubleCheck = inCheck;
                inCheck = true;
                checkingSquares |= Magic.GetRookAttacks(i, bitboardObject.occupiedSquares) & Magic.GetRookAttacks(bitboardObject.kings[player], bitboardObject.occupiedSquares);
                raysOnKingInCheck |= Magic.GetRookAttacks(i, friendlyNoKing) & Magic.GetRookAttacks(bitboardObject.kings[player], bitboardObject.occupiedSquares);
                Bitboards.setSquare(ref checkingSquares, i);
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
                    bool createPin = false;
                    if((j % 9) == (bitboardObject.kings[player] % 9) && (j% 9) == (i % 9))
                    {
                        Bitboards.setSquare(ref diagonalPinnedPieces, j);
                        createPin = true;
                    }
                    else if((j % 7) == (bitboardObject.kings[player] % 7) && (j % 7) == (i % 7))
                    {
                        Bitboards.setSquare(ref diagonalPinnedPieces, j);
                        createPin = true;
                    }
                    
                    if(createPin)
                    {
                        ulong tempWithoutPinned = bitboardObject.occupiedSquares & ~intersection;
                        diagonalPinRays |= Magic.GetBishopAttacks(i, tempWithoutPinned) & Magic.GetBishopAttacks(bitboardObject.kings[player], tempWithoutPinned);
                        Bitboards.setSquare(ref diagonalPinRays, i);
                    }  
                }
            }
            if((testing & bitboardObject.kingBitboards[player]) != 0)
            {
                inDoubleCheck = inCheck;
                inCheck = true;
                checkingSquares |= Magic.GetBishopAttacks(i, bitboardObject.occupiedSquares) & Magic.GetBishopAttacks(bitboardObject.kings[player], bitboardObject.occupiedSquares);
                raysOnKingInCheck |= Magic.GetBishopAttacks(i, friendlyNoKing) & Magic.GetBishopAttacks(bitboardObject.kings[player], bitboardObject.occupiedSquares);
                Bitboards.setSquare(ref checkingSquares, i);
            }
        }

        ulong opponentPawns = bitboardObject.pawns[enemyPlayer];
        ulong opponentKnights = bitboardObject.knights[enemyPlayer];
        while(opponentPawns != 0)
        {
            int i = tzcnt(opponentPawns);
            opponentPawns &= opponentPawns-1;
            ulong checkForCheck = pawnAttacks[enemyPlayer,i];
            if((checkForCheck & bitboardObject.kingBitboards[player]) != 0) {Bitboards.setSquare(ref checkingSquares, i); inDoubleCheck = inCheck;
                inCheck = true;}
            opponentAttackMap |= checkForCheck;
        }
        while(opponentKnights != 0)
        {
            int i = tzcnt(opponentKnights);
            opponentKnights &= opponentKnights-1;
            ulong checkForCheck = knightBitboards[i];
            if((checkForCheck & bitboardObject.kingBitboards[player]) != 0) {Bitboards.setSquare(ref checkingSquares, i); inDoubleCheck = inCheck;
                inCheck = true;}
            opponentAttackMap |= checkForCheck;
        }
        opponentAttackMap |= kingBitboards[bitboardObject.kings[enemyPlayer]];
    }

    private static void GeneratePawnMoves(ref Move[] moveList, Bitboards bitboardObject)
    {
        ulong pawnBoard = bitboardObject.pawns[player];
        ulong push = new ulong();
        ulong doublePush = new ulong();
        ulong promotions = new ulong();
        int pushDirection;
        ulong orthgonalPinnedPawns = pawnBoard & orthogonalPinnedPieces;
        ulong diagonalPinnedPawns = pawnBoard & diagonalPinnedPieces;

        pawnBoard &= ~(orthgonalPinnedPawns | diagonalPinnedPawns);

        if(generateForWhite)
        {
            pushDirection = 8;
            push |= (pawnBoard & ~(diagonalPinnedPieces)) << 8;
            push &= emptySquaresBitboard;
            doublePush = push & Bitboards.Rank3;
            doublePush = doublePush << 8;
            doublePush &= emptySquaresBitboard;
            push |= (orthgonalPinnedPawns << 8) & orthogonalPinRays;
            push |= ((orthgonalPinnedPawns & Bitboards.Rank3) << 16) & orthogonalPinRays;
        }
        else
        {
            pushDirection = -8;
            push |= (pawnBoard & ~(diagonalPinnedPieces)) >> 8;
            push &= emptySquaresBitboard;
            doublePush = push & Bitboards.Rank6;
            doublePush = doublePush >> 8;
            doublePush &= emptySquaresBitboard;
            push |= (orthgonalPinnedPawns >> 8) & orthogonalPinRays;
            push |= ((orthgonalPinnedPawns & Bitboards.Rank3) >> 16) & orthogonalPinRays;
        }
        if(inCheck)
        {
            push &= checkingSquares;
            doublePush &= checkingSquares;
        }

        promotions = push & Bitboards.promotionMask;
        push &= ~Bitboards.promotionMask;

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
            ulong testForCaptureOnCheck = 1ul << lastMove.targetSquare;
            if(inCheck && (testForCaptureOnCheck & checkingSquares) == 0) enPassantSquares = 0;
            while(enPassantSquares != 0)
            {
                int i = tzcnt(enPassantSquares);
                enPassantSquares &= enPassantSquares-1;
                int landingSquare = lastMove.targetSquare + pushDirection;
                if(!InCheckAfterEnPassant(i, landingSquare, lastMove.targetSquare, bitboardObject))
                {
                    Move m = new Move(i, landingSquare, Move.EnPassantCaptureFlag, lastMove.targetSquare);
                    moveList[currIndex++] = m;
                }
            }
        }

        while(promotions != 0)
        {
            int j = tzcnt(promotions);
            promotions &= promotions - 1;
            Move m = new Move(j-(pushDirection), j, Move.PromoteToQueenFlag);
            moveList[currIndex++] = m;
            m = new Move(j-(pushDirection), j, Move.PromoteToRookFlag);
            moveList[currIndex++] = m;
            m = new Move(j-(pushDirection), j, Move.PromoteToKnightFlag);
            moveList[currIndex++] = m;
            m = new Move(j-(pushDirection), j, Move.PromoteToBishopFlag);
            moveList[currIndex++] = m;
        }

        pawnBoard |= diagonalPinnedPawns;
        while(pawnBoard != 0)
        {
            int i = tzcnt(pawnBoard);
            pawnBoard &= pawnBoard - 1;
            ulong attacks;
            attacks = pawnAttacks[player, i];
            attacks &= opponentBitboard;
            if(inCheck) attacks &= checkingSquares;
            ulong checkPin = new ulong();
            Bitboards.setSquare(ref checkPin, i);
            if((checkPin & diagonalPinnedPawns) != 0) attacks &= diagonalPinRays;
            promotions = attacks & Bitboards.promotionMask;
            attacks &= ~Bitboards.promotionMask;
            while(attacks != 0)
            {
                int j = tzcnt(attacks);
                attacks &= attacks - 1;
                Move m = new Move(i, j);
                moveList[currIndex++] = m;
            }
            while(promotions != 0)
            {
                int j = tzcnt(promotions);
                promotions &= promotions - 1;
                Move m = new Move(i, j, Move.PromoteToQueenFlag);
                moveList[currIndex++] = m;
                m = new Move(i, j, Move.PromoteToRookFlag);
                moveList[currIndex++] = m;
                m = new Move(i, j, Move.PromoteToKnightFlag);
                moveList[currIndex++] = m;
                m = new Move(i, j, Move.PromoteToBishopFlag);
                moveList[currIndex++] = m;
            }
        }
    }

    private static void GenerateKnightMoves(ref Move[] moveList, Bitboards bitboardObject)
    {
        // increments, 6, 10, 15, 17
        ulong knightBoard = bitboardObject.knights[player];
        knightBoard &= ~((diagonalPinnedPieces) | (orthogonalPinnedPieces));
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

        diagonalBoard &= ~orthogonalPinnedPieces;
        straightBoard &= ~diagonalPinnedPieces;

        while(diagonalBoard != 0) {
            ulong diagonalMove = new ulong();
            int i = tzcnt(diagonalBoard);
            diagonalBoard &= diagonalBoard-1;
            diagonalMove = Magic.GetBishopAttacks(i, bitboardObject.occupiedSquares);
            diagonalMove &= ~friendlyBitboard;
            if(inCheck) diagonalMove &= checkingSquares;
            ulong checkPin = new ulong();
            Bitboards.setSquare(ref checkPin, i);
            if((diagonalPinnedPieces & checkPin) != 0) diagonalMove &= diagonalPinRays;
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
            ulong checkPin = new ulong();
            Bitboards.setSquare(ref checkPin, i);
            if((orthogonalPinnedPieces & checkPin) != 0) straightMove &= orthogonalPinRays;
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
        kingMoves &= ~raysOnKingInCheck;
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
            if(bitboardObject.whiteKingCastle && test1 == 0 && (bitboardObject.rooks[0] & whiteKingRookCheck) != 0)
            {
                Move m = new Move(3, 1, Move.CastleFlag);
                moveList[currIndex++] = m;
            }
            if(bitboardObject.whiteQueenCastle && test2 == 0 && (bitboardObject.rooks[0] & whiteQueenRookCheck) != 0)
            {
                Move m = new Move(3, 5, Move.CastleFlag);
                moveList[currIndex++] = m;
            }
        }
        else
        {
            ulong test1 = bitboardObject.occupiedSquares & bitboardObject.blackKingCastleMask;
            ulong test2 = bitboardObject.occupiedSquares & bitboardObject.blackQueenCastleMask;
            if(bitboardObject.blackKingCastle && test1 == 0 && (bitboardObject.rooks[1] & blackKingRookCheck) != 0)
            {
                Move m = new Move(59, 57, Move.CastleFlag);
                moveList[currIndex++] = m;
            }
            if(bitboardObject.blackQueenCastle && test2 == 0 && (bitboardObject.rooks[1] & blackQueenRookCheck) != 0)
            {
                Move m = new Move(59, 61, Move.CastleFlag);
                moveList[currIndex++] = m;
            }
        }
    }

    public void StoreMoves()
    {
        Bitboards.setSquare(ref whiteQueenRookCheck, 7);
        Bitboards.setSquare(ref whiteKingRookCheck, 0);
        Bitboards.setSquare(ref blackKingRookCheck, 56);
        Bitboards.setSquare(ref blackQueenRookCheck, 63);
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

    private static bool InCheckAfterEnPassant(int pawnStartSquare, int pawnEndSquare, int pawnCaptureSquare, Bitboards bitboardObject)
    {
        ulong enemyOrthogonal = enemyOrthogonalSliders;
        if(enemyOrthogonal != 0)
        {
            ulong maskedBlockers = ((bitboardObject.pieces[0] | bitboardObject.pieces[1]) ^ (1ul << pawnCaptureSquare | 1ul << pawnStartSquare | 1ul << pawnEndSquare));
            ulong rookAttacks = Magic.GetRookAttacks(friendlyKingSquare, maskedBlockers);
            return (rookAttacks & enemyOrthogonal) != 0;
        }

        return false;
    }
}