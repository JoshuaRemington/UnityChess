using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Unity.Mathematics.math;

public class Bitboards
{
    public ulong[] pawns = new ulong[2];
    public ulong[] knights = new ulong[2];
    public ulong[] bishops = new ulong[2];
    public ulong[] rooks = new ulong[2];
    public ulong[] queens = new ulong[2];
    public ulong[] kingBitboards = new ulong[2];
    public int[] kings = new int[2];
    public ulong occupiedSquares => pieces[0] | pieces[1];
    public ulong[] pieces = new ulong[2];
    public int[] findIndex =   {5,3,4,6,7,4,3,5,
                                2,2,2,2,2,2,2,2,
                                0,0,0,0,0,0,0,0,
                                0,0,0,0,0,0,0,0,
                                0,0,0,0,0,0,0,0,
                                0,0,0,0,0,0,0,0,
                                8,8,8,8,8,8,8,8,
                                11,9,10,12,13,10,9,11};
    public bool whiteTurn = true;

    public const ulong FileA = 0x101010101010101;

	public const ulong Rank1 = 0b11111111;
	public const ulong Rank2 = Rank1 << 8;
	public const ulong Rank3 = Rank2 << 8;
	public const ulong Rank4 = Rank3 << 8;
	public const ulong Rank5 = Rank4 << 8;
	public const ulong Rank6 = Rank5 << 8;
	public const ulong Rank7 = Rank6 << 8;
	public const ulong Rank8 = Rank7 << 8;

    public const ulong promotionMask = Rank1 | Rank8;

	public const ulong notAFile = ~FileA;
    public const ulong notBFile = ~(FileA << 1);
    public const ulong notABFiles = notAFile & notBFile;
	public const ulong notHFile = ~(FileA << 7);
    public const ulong notGFile = ~(FileA << 6);
    public const ulong notGHFiles = notGFile & notHFile;

    //public bool whiteQueenCastle = true;
    public ulong whiteQueenCastleMask;
    //public bool whiteKingCastle = true;
    public ulong whiteKingCastleMask;
    //public bool blackQueenCastle = true;
    public ulong blackQueenCastleMask;
    //public bool blackKingCastle = true;
    public ulong blackKingCastleMask;
    public Stack<GameState> stackForGameStates = new Stack<GameState>();
    public GameState currentGameState;
    
    //standard chess start position with white on top of screen(at index 0)
    //Here is 64 0's: &B0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000
    //square index 0 is least significant bit, square index 63 is most significant bit
    public void initiateBitboardStartPosition(string fen)
    {
        parseFenForBitboard(fen);
        createCastlingMasks();
    }

    private void parseFenForBitboard(string fen)
    {
        bool loopForPieces = true;
        int i = 0;
        int j = 0;
        for(int r = 0; r < 64; r++) findIndex[r] = 0;
        while(loopForPieces && j < fen.Length && i < 64)
        {
            if(fen[j] == ' ') {loopForPieces = false; continue;}
            switch(fen[j])
            {
                case 'p': setSquare(ref pieces[1], 63-i); setSquare(ref pawns[1], 63-i); findIndex[63-i] = 8; break;
                case 'n': setSquare(ref pieces[1], 63-i); setSquare(ref knights[1], 63-i); findIndex[63-i] = 9; break;
                case 'b': setSquare(ref pieces[1], 63-i); setSquare(ref bishops[1], 63-i); findIndex[63-i] = 10; break;
                case 'r': setSquare(ref pieces[1], 63-i); setSquare(ref rooks[1], 63-i); findIndex[63-i] = 11; break;
                case 'q': setSquare(ref pieces[1], 63-i); setSquare(ref queens[1], 63-i); findIndex[63-i] = 12; break;
                case 'k': setSquare(ref pieces[1], 63-i); setSquare(ref kingBitboards[1], 63-i); kings[1] = 63-i; findIndex[63-i] = 13; break;
                case 'P': setSquare(ref pieces[0], 63-i); setSquare(ref pawns[0], 63-i); findIndex[63-i] = 2; break;
                case 'N': setSquare(ref pieces[0], 63-i); setSquare(ref knights[0], 63-i); findIndex[63-i] = 3; break;
                case 'B': setSquare(ref pieces[0], 63-i); setSquare(ref bishops[0], 63-i); findIndex[63-i] = 4; break;
                case 'R': setSquare(ref pieces[0], 63-i); setSquare(ref rooks[0], 63-i); findIndex[63-i] = 5; break;
                case 'Q': setSquare(ref pieces[0], 63-i); setSquare(ref queens[0], 63-i); findIndex[63-i] = 6; break;
                case 'K': setSquare(ref pieces[0], 63-i); setSquare(ref kingBitboards[0], 63-i); kings[0] = 63-i; findIndex[63-i] = 7; break;
                case '/': j++; continue;
                default: double storeValue = Char.GetNumericValue(fen[j]); i+= (int)storeValue; j++; continue;
            }
            i++;
            j++;
        }
        j++;
        bool whiteKingCastle = false;
        bool whiteQueenCastle = false;
        bool blackKingCastle = false;
        bool blackQueenCastle = false;
        if(j >= fen.Length) return;
        if(fen[j] == 'w') whiteTurn = true; 
        else whiteTurn = false;
        j += 2;
        while(fen[j] != ' ')
        {
            switch(fen[j++])
            {
                case 'K': whiteKingCastle = true; break;
                case 'k': blackKingCastle = true; break;
                case 'Q': whiteQueenCastle = true; break;
                case 'q': blackQueenCastle = true; break;
                default: break;
            }
        }
        currentGameState = new GameState(whiteKingCastle, blackKingCastle, whiteQueenCastle, blackQueenCastle);
    }

    public static void printBitBoard(ulong val)
    {
        Debug.Log(Convert.ToString((long)val, 2));
    }

    public static void setSquare(ref ulong givenbitboard, int index)
    {
        if(index == -1) return;
        givenbitboard |= 1ul << index;
    }

    public void clearSquare(ref ulong givenbitboard, int index)
    {
        if(index == -1) return;
        givenbitboard &= ~(1ul << index);
    }

    public void makeMove(int index, int startSquare, int endSquare)
    {
        switch(index)
        {
            case 2: clearSquare(ref pawns[0], startSquare); setSquare(ref pawns[0], endSquare); break;
            case 3: clearSquare(ref knights[0], startSquare); setSquare(ref knights[0], endSquare); break;
            case 4: clearSquare(ref bishops[0], startSquare); setSquare(ref bishops[0], endSquare); break;
            case 5: clearSquare(ref rooks[0], startSquare); setSquare(ref rooks[0], endSquare); break;
            case 6: clearSquare(ref queens[0], startSquare); setSquare(ref queens[0], endSquare); break;
            case 7: clearSquare(ref kingBitboards[0] ,startSquare); setSquare(ref kingBitboards[0], endSquare); kings[0]=endSquare; break;
            case 8: clearSquare(ref pawns[1], startSquare); setSquare(ref pawns[1], endSquare); break;
            case 9: clearSquare(ref knights[1], startSquare); setSquare(ref knights[1], endSquare); break;
            case 10: clearSquare(ref bishops[1], startSquare); setSquare(ref bishops[1], endSquare); break;
            case 11: clearSquare(ref rooks[1], startSquare); setSquare(ref rooks[1], endSquare); break;
            case 12: clearSquare(ref queens[1], startSquare); setSquare(ref queens[1], endSquare); break;
            case 13: clearSquare(ref kingBitboards[1] ,startSquare); setSquare(ref kingBitboards[1], endSquare); kings[1]=endSquare; break;
            default: 
                return;
        }
    }

    public int playMove(Move m)
    {
        stackForGameStates.Push(currentGameState);
        int arrayIndex = findIndex[m.startSquare];  
        int captureIndex = findIndex[m.targetSquare];
        findIndex[m.targetSquare] = findIndex[m.startSquare];
        findIndex[m.startSquare] = 0;
        int enemyIndex, friendlyIndex;
        if(this.whiteTurn)
        {
            enemyIndex = 1;
            friendlyIndex = 0;   
        }
        else
        {
            enemyIndex = 0;
            friendlyIndex = 1;
        }
        if(currentGameState.whiteKingCastle && arrayIndex == 5 && m.startSquare == 0)
            currentGameState.whiteKingCastle = false;
        else if(currentGameState.whiteQueenCastle && arrayIndex == 5 && m.startSquare == 7)
            currentGameState.whiteQueenCastle = false;
        else if(currentGameState.blackKingCastle && arrayIndex == 11 && m.startSquare == 56)
            currentGameState.blackKingCastle = false;
        else if(currentGameState.blackQueenCastle && arrayIndex == 11 && m.startSquare == 63)
            currentGameState.blackQueenCastle = false;    
        else if((currentGameState.whiteKingCastle || currentGameState.whiteQueenCastle) && arrayIndex == 7)
        {
            currentGameState.whiteKingCastle = false;
            currentGameState.whiteQueenCastle = false;
        }
        else if((currentGameState.blackKingCastle || currentGameState.blackQueenCastle) && arrayIndex == 13)
        {
            currentGameState.blackKingCastle = false;
            currentGameState.blackQueenCastle = false;
        }
        this.whiteTurn = !this.whiteTurn;
        clearSquare(ref pieces[friendlyIndex], m.startSquare);
        setSquare(ref pieces[friendlyIndex], m.targetSquare);
        makeMove(arrayIndex, m.startSquare, m.targetSquare);
        if(captureIndex != 0)
        {
            makeMove(captureIndex, m.targetSquare, -1);
            clearSquare(ref pieces[enemyIndex], m.targetSquare);
        }
        else if(m.flag == Move.EnPassantCaptureFlag)
        {
            captureIndex = findIndex[m.enPassantSquare];
            makeMove(captureIndex, m.enPassantSquare, -1);
            clearSquare(ref pieces[enemyIndex], m.enPassantSquare);
            findIndex[m.enPassantSquare] = 0;
        }
        else if(m.flag == Move.CastleFlag)
        {
            switch(m.targetSquare)
            {
                case 1: 
                    findIndex[2] = findIndex[0]; 
                    findIndex[0] = 0;
                    clearSquare(ref rooks[0], 0);
                    setSquare(ref rooks[0], 2);
                    clearSquare(ref pieces[0], 0);
                    setSquare(ref pieces[0], 2);
                    return 0;
                case 5:
                    findIndex[4] = findIndex[7]; 
                    findIndex[7] = 0;
                    clearSquare(ref rooks[0], 7);
                    setSquare(ref rooks[0], 4);
                    clearSquare(ref pieces[0], 7);
                    setSquare(ref pieces[0], 4);
                    return 7;
                case 57:
                    findIndex[58] = findIndex[56]; 
                    findIndex[56] = 0;
                    clearSquare(ref rooks[1], 56);
                    setSquare(ref rooks[1], 58);
                    clearSquare(ref pieces[1], 56);
                    setSquare(ref pieces[1], 58);
                    return 56;
                case 61:
                    findIndex[60] = findIndex[63]; 
                    findIndex[63] = 0;
                    clearSquare(ref rooks[1], 63);
                    setSquare(ref rooks[1], 60);
                    clearSquare(ref pieces[1], 63);
                    setSquare(ref pieces[1], 60);
                    return 63;
            }
        }

        if(m.flag == Move.PromoteToQueenFlag) 
        {
            findIndex[m.targetSquare] = this.whiteTurn ? 12 : 6;
            makeMove(arrayIndex, m.startSquare, -1);
            setSquare(ref queens[this.whiteTurn ? 1 : 0], m.targetSquare);
        }
        else if(m.flag == Move.PromoteToRookFlag)
        {
            findIndex[m.targetSquare] = this.whiteTurn ? 11 : 5;
            makeMove(arrayIndex, m.startSquare, -1);
            setSquare(ref rooks[this.whiteTurn ? 1 : 0], m.targetSquare);
        }
        else if(m.flag == Move.PromoteToBishopFlag)
        {
            findIndex[m.targetSquare] = this.whiteTurn ? 10 : 4;
            makeMove(arrayIndex, m.startSquare, -1);
            setSquare(ref bishops[this.whiteTurn ? 1 : 0], m.targetSquare);
        }
        else if(m.flag == Move.PromoteToKnightFlag)
        {
            findIndex[m.targetSquare] = this.whiteTurn ? 9 : 3;
            makeMove(arrayIndex, m.startSquare, -1);
            setSquare(ref knights[this.whiteTurn ? 1 : 0], m.targetSquare);
        }
        return captureIndex;
    }

    public void undoMove(Move m, int captureIndex)
    {
        currentGameState = stackForGameStates.Pop();
        this.whiteTurn = !this.whiteTurn;
        int arrayIndex = findIndex[m.targetSquare];  
        findIndex[m.startSquare] = findIndex[m.targetSquare];
        findIndex[m.targetSquare] = captureIndex;
        int enemyIndex, friendlyIndex;
        if(this.whiteTurn)
        {
            enemyIndex = 1;
            friendlyIndex = 0;   
        }
        else
        {
            enemyIndex = 0;
            friendlyIndex = 1;
        }
        makeMove(arrayIndex, m.targetSquare, m.startSquare);
    
        clearSquare(ref pieces[friendlyIndex], m.targetSquare);
        setSquare(ref pieces[friendlyIndex], m.startSquare);
        if(m.flag == Move.EnPassantCaptureFlag)
        {
            setSquare(ref pieces[enemyIndex], m.enPassantSquare);
            makeMove(captureIndex, -1, m.enPassantSquare);
            findIndex[m.enPassantSquare] = captureIndex;
        }  
        else if (captureIndex != 0)
        {
            setSquare(ref pieces[enemyIndex], m.targetSquare);
            makeMove(captureIndex, -1, m.targetSquare);
        }
        else if(m.flag == Move.CastleFlag)
        {
            switch(m.targetSquare)
            {
                case 1: 
                    findIndex[0] = findIndex[2]; 
                    findIndex[2] = 0;
                    clearSquare(ref rooks[0], 2);
                    setSquare(ref rooks[0], 0);
                    clearSquare(ref pieces[0], 2);
                    setSquare(ref pieces[0], 0);
                    break;
                case 5:
                    findIndex[7] = findIndex[4]; 
                    findIndex[4] = 0;
                    clearSquare(ref rooks[0], 4);
                    setSquare(ref rooks[0], 7);
                    clearSquare(ref pieces[0], 4);
                    setSquare(ref pieces[0], 7);
                    break;
                case 57:
                    findIndex[56] = findIndex[58]; 
                    findIndex[58] = 0;
                    clearSquare(ref rooks[1], 58);
                    setSquare(ref rooks[1], 56);
                    clearSquare(ref pieces[1], 58);
                    setSquare(ref pieces[1], 56);
                    break;
                case 61:
                    findIndex[63] = findIndex[60]; 
                    findIndex[60] = 0;
                    clearSquare(ref rooks[1], 60);
                    setSquare(ref rooks[1], 63);
                    clearSquare(ref pieces[1], 60);
                    setSquare(ref pieces[1], 63);
                    break;
            }
        }

        if(m.flag > 3) 
        {
            findIndex[m.startSquare] = this.whiteTurn ? 2 : 8;
            makeMove(arrayIndex, m.startSquare, -1);
            setSquare(ref pawns[this.whiteTurn ? 0 : 1], m.startSquare);
        }
    }

    public void createCastlingMasks()
    {
        whiteKingCastleMask = 1UL << 1;
        whiteKingCastleMask |= 1UL << 2;
        
        whiteQueenCastleMask = 1UL << 4;
        whiteQueenCastleMask |= 1UL << 5;
        whiteQueenCastleMask |= 1UL << 6;

        blackKingCastleMask = 1UL << 57;
        blackKingCastleMask |= 1UL << 58;
        
        blackQueenCastleMask = 1UL << 60;
        blackQueenCastleMask |= 1UL << 61;
        blackQueenCastleMask |= 1UL << 62;
    }
}

/*[System.Runtime.CompilerServices.MethodImpl(MethodImplOptions.AggressiveInlining)]
public static int CountLeadingZeros(this ulong input)
{
    const int bits = 64;
    // if (input == 0L) return bits; // Not needed. Use only if 0 is very common.
    int n = 1;
    if ((input >> (bits - 32)) == 0) { n += 32; input <<= 32; }
    if ((input >> (bits - 16)) == 0) { n += 16; input <<= 16; }
    if ((input >> (bits - 8)) == 0) { n += 8; input <<= 8; }
    if ((input >> (bits - 4)) == 0) { n += 4; input <<= 4; }
    if ((input >> (bits - 2)) == 0) { n += 2; input <<= 2; }
    return n - (int)(input >> (bits - 1));
}*/