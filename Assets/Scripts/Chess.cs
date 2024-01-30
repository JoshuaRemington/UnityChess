using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This is a class for controlling the chess pieces(the sprites for the pieces), allows us to correctly place on the canvas(board)
public class Chess : MonoBehaviour
{
    public GameObject controller;  //we need to grab the controller
    public bool isWhite;  //Determine if the piece is white or black
    public int pieceToBitboardValue;  //not needed, but we store the index of each sprite to correspond to an array index, essentially not used in this program

    public Sprite black_queen, black_knight, black_bishop, black_king, black_rook, black_pawn;  //grab all the sprites for the black pieces
    public Sprite white_queen, white_knight, white_bishop, white_king, white_rook, white_pawn;  //grab all the sprites for the white pieces

    //Activate is a function called at the start of the game to create the objects and put them into the correct locations
    public void Activate(int pos)  //pos is the square that the piece should be placed on
    {
        controller = GameObject.FindGameObjectWithTag("GameController");  //grab the controller, I don't really remember the purpose behind this

        SetCoords(pos);  //call SetCoords to place the sprite into the correct position on the board, pos is the square that we place the piece on

        //This switch statement allows us to quickly grab the sprite corresponding to the name that we have for the object
        switch (this.name)
        {
            /*
                For these cases, we grab the corresponding sprite based on name, these are stored correctly in unity, we then can break out of switch and return
            */
            case "black_queen": this.GetComponent<SpriteRenderer>().sprite = black_queen; this.isWhite = false; this.pieceToBitboardValue = 12;  break;
            case "black_knight": this.GetComponent<SpriteRenderer>().sprite = black_knight; this.isWhite = false; this.pieceToBitboardValue = 9; break;
            case "black_bishop": this.GetComponent<SpriteRenderer>().sprite = black_bishop; this.isWhite = false; this.pieceToBitboardValue = 10; break;
            case "black_king": this.GetComponent<SpriteRenderer>().sprite = black_king; this.isWhite = false; this.pieceToBitboardValue = 13; break;
            case "black_rook": this.GetComponent<SpriteRenderer>().sprite = black_rook; this.isWhite = false; this.pieceToBitboardValue = 11; break;
            case "black_pawn": this.GetComponent<SpriteRenderer>().sprite = black_pawn; this.isWhite = false; this.pieceToBitboardValue = 8; break;
            
            case "white_queen": this.GetComponent<SpriteRenderer>().sprite = white_queen; this.isWhite = true; this.pieceToBitboardValue = 6; break;
            case "white_knight": this.GetComponent<SpriteRenderer>().sprite = white_knight; this.isWhite = true; this.pieceToBitboardValue = 3; break;
            case "white_bishop": this.GetComponent<SpriteRenderer>().sprite = white_bishop; this.isWhite = true; this.pieceToBitboardValue = 4; break;
            case "white_king": this.GetComponent<SpriteRenderer>().sprite = white_king; this.isWhite = true; this.pieceToBitboardValue = 7; break;
            case "white_rook": this.GetComponent<SpriteRenderer>().sprite = white_rook; this.isWhite = true; this.pieceToBitboardValue = 5; break;
            case "white_pawn": this.GetComponent<SpriteRenderer>().sprite = white_pawn; this.isWhite = true; this.pieceToBitboardValue = 2; break;
        }
        
    }


    //SetCoords puts the sprite onto the correct location on the board
    public void SetCoords(int pos) 
    { 
        //easy algorithm based on the board size and square size to put onto the correct square
        float x = -4.5f + (1.28f * (pos % 8f));   //place correctly on x axis
        float temp = pos / 8;
        float y = 4.5f - (1.28f * temp);  //place correctly on y axis

        this.transform.position = new Vector3(x, y, -1.0f);  //assign position of chess object
    }   
}
