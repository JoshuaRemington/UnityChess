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

	public const ulong notAFile = ~FileA;
    public const ulong notBFile = ~(FileA << 1);
    public const ulong notABFiles = notAFile & notBFile;
	public const ulong notHFile = ~(FileA << 7);
    public const ulong notGFile = ~(FileA << 6);
    public const ulong notGHFiles = notGFile & notHFile;

    public bool whiteQueenCastle = true;
    public ulong whiteQueenCastleMask;
    public bool whiteKingCastle = true;
    public ulong whiteKingCastleMask;
    public bool blackQueenCastle = true;
    public ulong blackQueenCastleMask;
    public bool blackKingCastle = true;
    public ulong blackKingCastleMask;
    
    //standard chess start position with white on top of screen(at index 0)
    //Here is 64 0's: &B0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000
    //square index 0 is least significant bit, square index 63 is most significant bit
    public void initiateBitboardStartPosition()
    {
        pieces[0] = 0B0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_1111_1111_1111_1111; //white pieces
        pieces[1] = 0B1111_1111_1111_1111_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000; //black pieces
        pawns[0] = 0B0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_1111_1111_0000_0000; //white pawns
        knights[0] = 0B0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0100_0010; //white knights
        bishops[0] = 0B0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0010_0100; //white bishops
        rooks[0] = 0B0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_1000_0001; //white rooks
        queens[0] = 0B0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_1000;  //white queen
        kings[0] = 4;
        kingBitboards[0] = 0B0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0001_0000; 
        pawns[1] = 0B0000_0000_1111_1111_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000; //black pawns
        knights[1] = 0B0100_0010_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000; //black knights
        bishops[1] = 0B0010_0100_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000; //black bishops
        rooks[1] = 0B1000_0001_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000; //black rooks
        queens[1] = 0B0000_1000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000;  //black queen
        kings[1] = 60;
        kingBitboards[1] = 0B0001_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000; 
        createCastlingMasks();
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
                case 2: 
                    findIndex[3] = findIndex[0]; 
                    findIndex[0] = 0;
                    clearSquare(ref rooks[0], 0);
                    setSquare(ref rooks[0], 3);
                    clearSquare(ref pieces[0], 0);
                    setSquare(ref pieces[0], 3);
                    return 0;
                case 6:
                    findIndex[5] = findIndex[7]; 
                    findIndex[7] = 0;
                    clearSquare(ref rooks[0], 7);
                    setSquare(ref rooks[0], 5);
                    clearSquare(ref pieces[0], 7);
                    setSquare(ref pieces[0], 5);
                    return 7;
                case 58:
                    findIndex[59] = findIndex[56]; 
                    findIndex[56] = 0;
                    clearSquare(ref rooks[1], 56);
                    setSquare(ref rooks[1], 59);
                    clearSquare(ref pieces[1], 56);
                    setSquare(ref pieces[1], 59);
                    return 56;
                case 62:
                    findIndex[61] = findIndex[63]; 
                    findIndex[63] = 0;
                    clearSquare(ref rooks[1], 6);
                    setSquare(ref rooks[1], 61);
                    clearSquare(ref pieces[1], 63);
                    setSquare(ref pieces[1], 61);
                    return 63;
            }
        }
        return captureIndex;
    }

    public void undoMove(Move m, int captureIndex)
    {
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
                case 2: 
                    findIndex[0] = findIndex[3]; 
                    findIndex[3] = 0;
                    clearSquare(ref rooks[0], 3);
                    setSquare(ref rooks[0], 0);
                    clearSquare(ref pieces[0], 3);
                    setSquare(ref pieces[0], 0);
                    break;
                case 6:
                    findIndex[7] = findIndex[5]; 
                    findIndex[5] = 0;
                    clearSquare(ref rooks[0], 5);
                    setSquare(ref rooks[0], 7);
                    clearSquare(ref pieces[0], 5);
                    setSquare(ref pieces[0], 7);
                    break;
                case 58:
                    findIndex[56] = findIndex[59]; 
                    findIndex[59] = 0;
                    clearSquare(ref rooks[1], 59);
                    setSquare(ref rooks[1], 56);
                    clearSquare(ref pieces[1], 59);
                    setSquare(ref pieces[1], 56);
                    break;
                case 62:
                    findIndex[63] = findIndex[61]; 
                    findIndex[61] = 0;
                    clearSquare(ref rooks[1], 61);
                    setSquare(ref rooks[1], 63);
                    clearSquare(ref pieces[1], 61);
                    setSquare(ref pieces[1], 63);
                    break;
            }
        }
    }

    public void createCastlingMasks()
    {
        whiteKingCastleMask = 1UL << 5;
        whiteKingCastleMask |= 1UL << 6;
        
        whiteQueenCastleMask = 1UL << 1;
        whiteQueenCastleMask |= 1UL << 2;
        whiteQueenCastleMask |= 1UL << 3;

        blackKingCastleMask = 1UL << 61;
        blackKingCastleMask |= 1UL << 62;
        
        blackQueenCastleMask = 1UL << 57;
        blackQueenCastleMask |= 1UL << 58;
        blackQueenCastleMask |= 1UL << 59;
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