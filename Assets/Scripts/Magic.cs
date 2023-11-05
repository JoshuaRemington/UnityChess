using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Unity.Mathematics.math;

public class Magic
{
    // Define bitboard type

    // Bits manipulations
    private static ulong GetBit(ulong bitboard, int square) => (bitboard & (1UL << square));
    private ulong SetBit(ulong bitboard, int square) => (bitboard | (1UL << square));
    private ulong PopBit(ulong bitboard, int square) => (GetBit(bitboard, square) != 0 ? (bitboard ^ (1UL << square)) : 0);

    // rook rellevant occupancy bits
    static int[] rook_rellevant_bits = new int[64] {
        12, 11, 11, 11, 11, 11, 11, 12,
        11, 10, 10, 10, 10, 10, 10, 11,
        11, 10, 10, 10, 10, 10, 10, 11,
        11, 10, 10, 10, 10, 10, 10, 11,
        11, 10, 10, 10, 10, 10, 10, 11,
        11, 10, 10, 10, 10, 10, 10, 11,
        11, 10, 10, 10, 10, 10, 10, 11,
        12, 11, 11, 11, 11, 11, 11, 12
    };

    // bishop rellevant occupancy bits
    static int[] bishop_rellevant_bits = new int[64] {
        6, 5, 5, 5, 5, 5, 5, 6,
        5, 5, 5, 5, 5, 5, 5, 5,
        5, 5, 7, 7, 7, 7, 5, 5,
        5, 5, 7, 9, 9, 7, 5, 5,
        5, 5, 7, 9, 9, 7, 5, 5,
        5, 5, 7, 7, 7, 7, 5, 5,
        5, 5, 5, 5, 5, 5, 5, 5,
        6, 5, 5, 5, 5, 5, 5, 6
    };

    public static void PrintBitboard(ulong bitboard)
    {
        Debug.Log("");

        // Loop over board ranks
        for (int rank = 0; rank < 8; rank++)
        {
            // Loop over board files
            for (int file = 0; file < 8; file++)
            {
                // Initialize board square
                int square = rank * 8 + file;

                // Print ranks
                if (file == 0)
                    Debug.Log("  " + (8 - rank));

                // Print bit indexed by board square
                Debug.Log(" " + (GetBit(bitboard, square) != 0 ? 1 : 0));
            }

            Debug.Log("");
        }

        // Print files
        Debug.Log("\n     a b c d e f g h\n");

        // Print bitboard as decimal
        Debug.Log("     bitboard: " + bitboard + "\n");
    }

    private uint state = 1804289383;

    private uint GenerateRandomNumber()
    {
        // XOR shift algorithm
        uint x = state;
        x ^= x << 13;
        x ^= x >> 17;
        x ^= x << 5;
        state = x;
        return x;
    }

    private ulong Randomulong()
    {
        // Initialize numbers to randomize
        ulong u1, u2, u3, u4;

        // Randomize numbers
        u1 = (ulong)(GenerateRandomNumber()) & 0xFFFF;
        u2 = (ulong)(GenerateRandomNumber()) & 0xFFFF;
        u3 = (ulong)(GenerateRandomNumber()) & 0xFFFF;
        u4 = (ulong)(GenerateRandomNumber()) & 0xFFFF;

        // Shuffle bits and return
        return u1 | (u2 << 16) | (u3 << 32) | (u4 << 48);
    }

    private ulong RandomFewBits()
    {
        return Randomulong() & Randomulong() & Randomulong();
    }

    private int CountBits(ulong bitboard)
    {
        // Bit count
        int count = 0;

        // Pop bits until bitboard is empty
        while (bitboard != 0)
        {
            // Increment count
            count++;

            // Consecutively reset least significant 1st bit
            bitboard &= (bitboard - 1);
        }

        // Return bit count
        return count;
    }

    private int GetLS1BIndex(ulong bitboard)
    {
        // Make sure bitboard is not empty
        if (bitboard != 0)
        {
            // Convert trailing zeros before LS1B to ones and count them
            return CountBits((bitboard & (ulong)(-(long)bitboard)) - 1);
        }

        // Otherwise, return an illegal index
        return -1;
    }

    private ulong SetOccupancy(int index, int bitsInMask, ulong attackMask)
    {
        // Occupancy map
        ulong occupancy = 0UL;

        // Loop over the range of bits within the attack mask
        for (int count = 0; count < bitsInMask; count++)
        {
            // Get LS1B index of attack mask
            int square = GetLS1BIndex(attackMask);

            // Pop LS1B in attack map
            attackMask = PopBit(attackMask, square);

            // Make sure occupancy is on the board
            if ((index & (1 << count)) != 0)
            {
                // Populate occupancy map
                occupancy |= (1UL << square);
            }
        }

        // Return occupancy map
        return occupancy;
    }

    private ulong MaskBishopAttacks(int square)
    {
        // Attack bitboard
        ulong attacks = 0UL;

        // Initialize files & ranks
        int f, r;

        // Initialize target files & ranks
        int tr = square / 8;
        int tf = square % 8;

        // Generate attacks
        for (r = tr + 1, f = tf + 1; r <= 6 && f <= 6; r++, f++) attacks |= (1UL << (r * 8 + f));
        for (r = tr + 1, f = tf - 1; r <= 6 && f >= 1; r++, f--) attacks |= (1UL << (r * 8 + f));
        for (r = tr - 1, f = tf + 1; r >= 1 && f <= 6; r--, f++) attacks |= (1UL << (r * 8 + f));
        for (r = tr - 1, f = tf - 1; r >= 1 && f >= 1; r--, f--) attacks |= (1UL << (r * 8 + f));

        // Return attack map for bishop on a given square
        return attacks;
    }

    private ulong MaskRookAttacks(int square)
    {
        // Attacks bitboard
        ulong attacks = 0UL;

        // Initialize files & ranks
        int f, r;

        // Initialize target files & ranks
        int tr = square / 8;
        int tf = square % 8;

        // Generate attacks
        for (r = tr + 1; r <= 6; r++) attacks |= (1UL << (r * 8 + tf));
        for (r = tr - 1; r >= 1; r--) attacks |= (1UL << (r * 8 + tf));
        for (f = tf + 1; f <= 6; f++) attacks |= (1UL << (tr * 8 + f));
        for (f = tf - 1; f >= 1; f--) attacks |= (1UL << (tr * 8 + f));

        // Return attack map for rook on a given square
        return attacks;
    }

    private ulong BishopAttacksOnTheFly(int square, ulong block)
    {
        // Attack bitboard
        ulong attacks = 0UL;

        // Initialize files & ranks
        int f, r;

        // Initialize target files & ranks
        int tr = square / 8;
        int tf = square % 8;

        // Generate attacks
        for (r = tr + 1, f = tf + 1; r <= 7 && f <= 7; r++, f++)
        {
            attacks |= (1UL << (r * 8 + f));
            if ((1UL << (r * 8 + f) & block) != 0) break;
        }
        for (r = tr + 1, f = tf - 1; r <= 7 && f >= 0; r++, f--)
        {
            attacks |= (1UL << (r * 8 + f));
            if ((1UL << (r * 8 + f) & block) != 0) break;
        }
        for (r = tr - 1, f = tf + 1; r >= 0 && f <= 7; r--, f++)
        {
            attacks |= (1UL << (r * 8 + f));
            if ((1UL << (r * 8 + f) & block) != 0) break;
        }
        for (r = tr - 1, f = tf - 1; r >= 0 && f >= 0; r--, f--)
        {
            attacks |= (1UL << (r * 8 + f));
            if ((1UL << (r * 8 + f) & block) != 0) break;
        }

        // Return attack map for bishop on a given square
        return attacks;
    }

    private ulong RookAttacksOnTheFly(int square, ulong block)
    {
        // Attacks bitboard
        ulong attacks = 0UL;

        // Initialize files & ranks
        int f, r;

        // Initialize target files & ranks
        int tr = square / 8;
        int tf = square % 8;

        // Generate attacks
        for (r = tr + 1; r <= 7; r++)
        {
            attacks |= (1UL << (r * 8 + tf));
            if ((1UL << (r * 8 + tf) & block) != 0) break;
        }
        for (r = tr - 1; r >= 0; r--)
        {
            attacks |= (1UL << (r * 8 + tf));
            if ((1UL << (r * 8 + tf) & block) != 0) break;
        }
        for (f = tf + 1; f <= 7; f++)
        {
            attacks |= (1UL << (tr * 8 + f));
            if ((1UL << (tr * 8 + f) & block) != 0) break;
        }
        for (f = tf - 1; f >= 0; f--)
        {
            attacks |= (1UL << (tr * 8 + f));
            if ((1UL << (tr * 8 + f) & block) != 0) break;
        }

        // Return attack map for rook on a given square
        return attacks;
    }

    private void GenerateRookMagics()
    {
        // Initialize attack tables for all squares
        ulong[][] rookTable = new ulong[64][];
        ulong occupancyVariations;
        int occupancyIndices;

        // Loop through all squares
        for (int square = 0; square < 64; square++)
        {
            // Calculate occupancy variations for the given square
            occupancyVariations = RandomFewBits();
            occupancyIndices = 1 << CountBits(occupancyVariations);

            // Initialize attack table
            rookTable[square] = new ulong[occupancyIndices];

            // Loop through occupancy variations
            for (int i = 0; i < occupancyIndices; i++)
            {
                // Generate occupancy map
                ulong occupancyMap = SetOccupancy(i, CountBits(occupancyVariations), occupancyVariations);

                // Calculate rook attack for the square with this occupancy
                rookTable[square][i] = RookAttacksOnTheFly(square, occupancyMap);
            }
        }
    }

    private void GenerateBishopMagics()
    {
        // Initialize attack tables for all squares
        ulong[][] bishopTable = new ulong[64][];
        ulong occupancyVariations;
        int occupancyIndices;

        // Loop through all squares
        for (int square = 0; square < 64; square++)
        {
            // Calculate occupancy variations for the given square
            occupancyVariations = RandomFewBits();
            occupancyIndices = 1 << CountBits(occupancyVariations);

            // Initialize attack table
            bishopTable[square] = new ulong[occupancyIndices];

            // Loop through occupancy variations
            for (int i = 0; i < occupancyIndices; i++)
            {
                // Generate occupancy map
                ulong occupancyMap = SetOccupancy(i, CountBits(occupancyVariations), occupancyVariations);

                // Calculate bishop attack for the square with this occupancy
                bishopTable[square][i] = BishopAttacksOnTheFly(square, occupancyMap);
            }
        }
    }

    // masks
static ulong[] bishop_masks = new ulong[64];
static ulong[] rook_masks = new ulong[64];

// attacks
static ulong[,] bishop_attacks = new ulong[64, 512];
static ulong[,] rook_attacks = new ulong[64, 4096];

// rook magic numbers
static ulong[] rook_magics = new ulong[64] {
    0xa8002c000108020UL,
    0x6c00049b0002001UL,
    0x100200010090040UL,
    0x2480041000800801UL,
    0x280028004000800UL,
    0x900410008040022UL,
    0x280020001001080UL,
    0x2880002041000080UL,
    0xa000800080400034UL,
    0x4808020004000UL,
    0x2290802004801000UL,
    0x411000d00100020UL,
    0x402800800040080UL,
    0xb000401004208UL,
    0x2409000100040200UL,
    0x1002100004082UL,
    0x22878001e24000UL,
    0x1090810021004010UL,
    0x801030040200012UL,
    0x500808008001000UL,
    0xa08018014000880UL,
    0x8000808004000200UL,
    0x201008080010200UL,
    0x801020000441091UL,
    0x800080204005UL,
    0x1040200040100048UL,
    0x120200402082UL,
    0xd14880480100080UL,
    0x12040280080080UL,
    0x100040080020080UL,
    0x9020010080800200UL,
    0x813241200148449UL,
    0x491604001800080UL,
    0x100401000402001UL,
    0x4820010021001040UL,
    0x400402202000812UL,
    0x209009005000802UL,
    0x810800601800400UL,
    0x4301083214000150UL,
    0x204026458e001401UL,
    0x40204000808000UL,
    0x8001008040010020UL,
    0x8410820820420010UL,
    0x1003001000090020UL,
    0x804040008008080UL,
    0x12000810020004UL,
    0x1000100200040208UL,
    0x430000a044020001UL,
    0x280009023410300UL,
    0xe0100040002240UL,
    0x200100401700UL,
    0x2244100408008080UL,
    0x8000400801980UL,
    0x2000810040200UL,
    0x8010100228810400UL,
    0x2000009044210200UL,
    0x4080008040102101UL,
    0x40002080411d01UL,
    0x2005524060000901UL,
    0x502001008400422UL,
    0x489a000810200402UL,
    0x1004400080a13UL,
    0x4000011008020084UL,
    0x26002114058042UL,
};

// bishop magic number
static ulong[] bishop_magics = new ulong[64] {
    0x89a1121896040240UL,
    0x2004844802002010UL,
    0x2068080051921000UL,
    0x62880a0220200808UL,
    0x4042004000000UL,
    0x100822020200011UL,
    0xc00444222012000aUL,
    0x28808801216001UL,
    0x400492088408100UL,
    0x201c401040c0084UL,
    0x840800910a0010UL,
    0x82080240060UL,
    0x2000840504006000UL,
    0x30010c4108405004UL,
    0x1008005410080802UL,
    0x8144042209100900UL,
    0x208081020014400UL,
    0x4800201208ca00UL,
    0xf18140408012008UL,
    0x1004002802102001UL,
    0x841000820080811UL,
    0x40200200a42008UL,
    0x800054042000UL,
    0x88010400410c9000UL,
    0x520040470104290UL,
    0x1004040051500081UL,
    0x2002081833080021UL,
    0x400c00c010142UL,
    0x941408200c002000UL,
    0x658810000806011UL,
    0x188071040440a00UL,
    0x4800404002011c00UL,
    0x104442040404200UL,
    0x511080202091021UL,
    0x4022401120400UL,
    0x80c0040400080120UL,
    0x8040010040820802UL,
    0x480810700020090UL,
    0x102008e00040242UL,
    0x809005202050100UL,
    0x8002024220104080UL,
    0x431008804142000UL,
    0x19001802081400UL,
    0x200014208040080UL,
    0x3308082008200100UL,
    0x41010500040c020UL,
    0x4012020c04210308UL,
    0x208220a202004080UL,
    0x111040120082000UL,
    0x6803040141280a00UL,
    0x2101004202410000UL,
    0x8200000041108022UL,
    0x21082088000UL,
    0x2410204010040UL,
    0x40100400809000UL,
    0x822088220820214UL,
    0x40808090012004UL,
    0x910224040218c9UL,
    0x402814422015008UL,
    0x90014004842410UL,
    0x1000042304105UL,
    0x10008830412a00UL,
    0x2520081090008908UL,
    0x40102000a0a60140UL,
};

 public void InitSlidersAttacks(bool isBishop)
    {
        // Loop over 64 board squares
        for (int square = 0; square < 64; square++)
        {
            // Initialize bishop & rook masks
            bishop_masks[square] = MaskBishopAttacks(square);
            rook_masks[square] = MaskRookAttacks(square);

            // Initialize current mask
            ulong mask = isBishop ? MaskBishopAttacks(square) : MaskRookAttacks(square);

            // Count attack mask bits
            int bitCount = CountBits(mask);

            // Occupancy variations count
            int occupancyVariations = 1 << bitCount;

            // Loop over occupancy variations
            for (int count = 0; count < occupancyVariations; count++)
            {
                // Bishop
                if (isBishop)
                {
                    // Initialize occupancies, magic index & attacks
                    ulong occupancy = SetOccupancy(count, bitCount, mask);
                    ulong magicIndex = occupancy * bishop_magics[square] >> (64 - bishop_rellevant_bits[square]);
                    bishop_attacks[square,magicIndex] = BishopAttacksOnTheFly(square, occupancy);
                }
                // Rook
                else
                {
                    // Initialize occupancies, magic index & attacks
                    ulong occupancy = SetOccupancy(count, bitCount, mask);
                    ulong magicIndex = occupancy * rook_magics[square] >> (64 - rook_rellevant_bits[square]);
                    rook_attacks[square,magicIndex] = RookAttacksOnTheFly(square, occupancy);
                }
            }
        }
    }


    // Lookup bishop attacks
    public static ulong GetBishopAttacks(int square, ulong occupancy)
    {
        // Calculate magic index
        occupancy &= bishop_masks[square];
        occupancy *= bishop_magics[square];
        occupancy >>= (64 - bishop_rellevant_bits[square]);

        // Return relevant attacks
        //Debug.Log(square);
        //Debug.Log(occupancy);
        return bishop_attacks[square,occupancy];
    }

    // Lookup rook attacks
    public static ulong GetRookAttacks(int square, ulong occupancy)
    {
        // Calculate magic index
        occupancy &= rook_masks[square];
        occupancy *= rook_magics[square];
        occupancy >>= (64 - rook_rellevant_bits[square]);

        // Return relevant attacks
        return rook_attacks[square,occupancy];
    }

    public void Create()
    {
        // Initialize random seed
        state = 1804289383;

        InitSlidersAttacks(true);
        InitSlidersAttacks(false);
    }
}