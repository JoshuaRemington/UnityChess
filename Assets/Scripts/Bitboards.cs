using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bitboards
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
    public ulong[] bitboards = new ulong[14];
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
	public const ulong notHFile = ~(FileA << 7);
    
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
    }

    public static void printBitBoard(ulong val)
    {
        Debug.Log(Convert.ToString((long)val, 2));
    }

    public void setSquare(ref ulong givenbitboard, int index)
    {
        givenbitboard |= 1ul << index;
    }

    public void clearSquare(ref ulong givenbitboard, int index)
    {
        givenbitboard &= ~(1ul << index);
    }

    public static int getLSB(ref ulong givenbitboard)
    {
        ulong input = givenbitboard;
        const int bits = 64;
        int n = 1;
        if ((input >> (bits - 32)) == 0) { n += 32; input <<= 32; }
        if ((input >> (bits - 16)) == 0) { n += 16; input <<= 16; }
        if ((input >> (bits - 8)) == 0) { n += 8; input <<= 8; }
        if ((input >> (bits - 4)) == 0) { n += 4; input <<= 4; }
        if ((input >> (bits - 2)) == 0) { n += 2; input <<= 2; }
        int i = n - (int)(input >> (bits - 1));
        return i;
    }

    public void playMove(Move m, int arrayIndex)
    {
        clearSquare(ref this.bitboards[arrayIndex], m.startSquare);
        setSquare(ref this.bitboards[arrayIndex], m.targetSquare);
        if(this.whiteTurn)
        {
            clearSquare(ref this.bitboards[0], m.startSquare);
            setSquare(ref this.bitboards[0], m.targetSquare);
        }
        else
        {
            clearSquare(ref this.bitboards[1], m.startSquare);
            setSquare(ref this.bitboards[1], m.targetSquare);
        }
        this.whiteTurn = !this.whiteTurn;
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
