using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
Game state is meant to be a helper class to keep the current state of the board, 
or allow us to create an object that will hold a game state at a current position for later use
*/
public class GameState
{
    public bool whiteKingCastle, blackKingCastle, whiteQueenCastle, blackQueenCastle;  //all the castling rights for each side
    //constructor, if the game is set up in a different state than base chess, it may have different castling rights
    public GameState(bool wKingCastle, bool bKingCastle, bool wQuenCastle, bool bQueenCastle)
    {
        //assign all the castling rights from parameters
        this.whiteKingCastle = wKingCastle;  
        this.blackKingCastle = bKingCastle;
        this.whiteQueenCastle = wQuenCastle;
        this.blackQueenCastle = bQueenCastle;
    }
}
