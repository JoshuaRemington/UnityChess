using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameState
{
    public bool whiteKingCastle, blackKingCastle, whiteQueenCastle, blackQueenCastle;
    public GameState(bool wKingCastle, bool bKingCastle, bool wQuenCastle, bool bQueenCastle)
    {
        this.whiteKingCastle = wKingCastle;
        this.blackKingCastle = bKingCastle;
        this.whiteQueenCastle = wQuenCastle;
        this.blackQueenCastle = bQueenCastle;
    }
}
