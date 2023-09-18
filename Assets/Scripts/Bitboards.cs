using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bitboards
{
    ulong board;
    ulong whiteKnight;
    ulong whiteBishop;
    ulong whiteKing;
    ulong whiteRook;
    ulong whiteQueen;
    ulong whitePawn;
    ulong blackKnight;
    ulong blackBishop;
    ulong blackKing;
    ulong blackRook;
    ulong blackQueen;
    ulong blackPawn;

    void Start()
    {
        initiateBitboardStartPosition();
    }
    private void initiateBitboardStartPosition()
    {
        
    }
}
