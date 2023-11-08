using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Unity.Mathematics.math;

public class Bitboards
{
    /*  0-13 for bitboard index correlation in array: 0 = white, BlackKing = 13
        0  White,
        1  Black,
        2  WhitePawn,
        3  WhiteKnight,
        4  WhiteBishop,
        5  WhiteRook,
        6  WhiteQueen,
        7  WhiteKing,
        8  BlackPawn,
        9 BlackKnight,
        10 BlackBishop,
        11 BlackRook,
        12 BlackQueen,
        13 BlackKing
    */
    public ulong[] bitboards = new ulong[14];
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
        bitboards[0] = 0B0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_1111_1111_1111_1111; //white pieces
        bitboards[1] = 0B1111_1111_1111_1111_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000; //black pieces
        bitboards[2] = 0B0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_1111_1111_0000_0000; //white pawns
        bitboards[3] = 0B0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0100_0010; //white knights
        bitboards[4] = 0B0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0010_0100; //white bishops
        bitboards[5] = 0B0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_1000_0001; //white rooks
        bitboards[6] = 0B0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_1000;  //white queen
        bitboards[7] = 0B0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0001_0000;  //white king
        bitboards[8] = 0B0000_0000_1111_1111_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000; //black pawns
        bitboards[9] = 0B0100_0010_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000; //black knights
        bitboards[10] = 0B0010_0100_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000; //black bishops
        bitboards[11] = 0B1000_0001_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000; //black rooks
        bitboards[12] = 0B0000_1000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000;  //black queen
        bitboards[13] = 0B0001_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000;  //black king
        createCastlingMasks();
    }

    public static void printBitBoard(ulong val)
    {
        Debug.Log(Convert.ToString((long)val, 2));
    }

    public static void setSquare(ref ulong givenbitboard, int index)
    {
        givenbitboard |= 1ul << index;
    }

    public void clearSquare(ref ulong givenbitboard, int index)
    {
        givenbitboard &= ~(1ul << index);
    }

    public int playMove(Move m)
    {
        int arrayIndex = findIndex[m.startSquare];  
        int captureIndex = findIndex[m.targetSquare];
        int friendlyIndex = 1;
        int enemyIndex = 0;
        findIndex[m.targetSquare] = findIndex[m.startSquare];
        findIndex[m.startSquare] = 0;
        if(this.whiteTurn)  {friendlyIndex = 0; enemyIndex = 1;}
        this.whiteTurn = !this.whiteTurn;
        clearSquare(ref this.bitboards[arrayIndex], m.startSquare);
        setSquare(ref this.bitboards[arrayIndex], m.targetSquare);
        clearSquare(ref this.bitboards[friendlyIndex], m.startSquare);
        setSquare(ref this.bitboards[friendlyIndex], m.targetSquare);
        if(captureIndex != 0)
        {
            clearSquare(ref this.bitboards[enemyIndex], m.targetSquare);
            clearSquare(ref this.bitboards[captureIndex], m.targetSquare);
        }
        else if(m.flag == Move.EnPassantCaptureFlag)
        {
            captureIndex = findIndex[m.enPassantSquare];
            clearSquare(ref this.bitboards[enemyIndex], m.enPassantSquare);
            clearSquare(ref this.bitboards[captureIndex], m.enPassantSquare);
            findIndex[m.enPassantSquare] = 0;
        }
        else if(m.flag == Move.CastleFlag)
        {
            switch(m.targetSquare)
            {
                case 2: 
                    findIndex[3] = findIndex[0]; 
                    findIndex[0] = 0;
                    clearSquare(ref this.bitboards[5], 0);
                    setSquare(ref this.bitboards[5], 3);
                    clearSquare(ref this.bitboards[friendlyIndex], 0);
                    setSquare(ref this.bitboards[friendlyIndex], 3);
                    return 0;
                case 6:
                    findIndex[5] = findIndex[7]; 
                    findIndex[7] = 0;
                    clearSquare(ref this.bitboards[5], 7);
                    setSquare(ref this.bitboards[5], 5);
                    clearSquare(ref this.bitboards[friendlyIndex], 7);
                    setSquare(ref this.bitboards[friendlyIndex], 5);
                    return 7;
                case 58:
                    findIndex[59] = findIndex[56]; 
                    findIndex[56] = 0;
                    clearSquare(ref this.bitboards[11], 56);
                    setSquare(ref this.bitboards[11], 59);
                    clearSquare(ref this.bitboards[friendlyIndex], 56);
                    setSquare(ref this.bitboards[friendlyIndex], 59);
                    return 56;
                case 62:
                    findIndex[61] = findIndex[63]; 
                    findIndex[63] = 0;
                    clearSquare(ref this.bitboards[11], 63);
                    setSquare(ref this.bitboards[11], 61);
                    clearSquare(ref this.bitboards[friendlyIndex], 63);
                    setSquare(ref this.bitboards[friendlyIndex], 61);
                    return 63;
            }
        }
        return captureIndex;
    }

    public void undoMove(Move m, int captureIndex)
    {
        this.whiteTurn = !this.whiteTurn;
        int arrayIndex = findIndex[m.targetSquare];
        int friendlyIndex = 1;
        int enemyIndex = 0;
        findIndex[m.startSquare] = findIndex[m.targetSquare];
        findIndex[m.targetSquare] = captureIndex;
        if(this.whiteTurn)  {friendlyIndex = 0; enemyIndex = 1;}
        clearSquare(ref this.bitboards[arrayIndex], m.targetSquare);
        setSquare(ref this.bitboards[arrayIndex], m.startSquare);
    
        clearSquare(ref this.bitboards[friendlyIndex], m.targetSquare);
        setSquare(ref this.bitboards[friendlyIndex], m.startSquare);
        if(m.flag == Move.EnPassantCaptureFlag)
        {
            setSquare(ref this.bitboards[enemyIndex], m.enPassantSquare);
            setSquare(ref this.bitboards[captureIndex], m.enPassantSquare);
            findIndex[m.enPassantSquare] = captureIndex;
        }  
        else if (captureIndex != 0)
        {
            setSquare(ref this.bitboards[enemyIndex], m.targetSquare);
            setSquare(ref this.bitboards[captureIndex], m.targetSquare);
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