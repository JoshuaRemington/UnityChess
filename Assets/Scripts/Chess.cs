using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chess : MonoBehaviour
{
    public GameObject controller;
    public GameObject movePLate;
    public bool isWhite;

    public Sprite black_queen, black_knight, black_bishop, black_king, black_rook, black_pawn;
    public Sprite white_queen, white_knight, white_bishop, white_king, white_rook, white_pawn;

    public void Activate(int pos)
    {
        controller = GameObject.FindGameObjectWithTag("GameController");

        SetCoords(pos);

        switch (this.name)
        {
            case "black_queen": this.GetComponent<SpriteRenderer>().sprite = black_queen; this.isWhite = false; break;
            case "black_knight": this.GetComponent<SpriteRenderer>().sprite = black_knight; this.isWhite = false; break;
            case "black_bishop": this.GetComponent<SpriteRenderer>().sprite = black_bishop; this.isWhite = false; break;
            case "black_king": this.GetComponent<SpriteRenderer>().sprite = black_king; this.isWhite = false; break;
            case "black_rook": this.GetComponent<SpriteRenderer>().sprite = black_rook; this.isWhite = false; break;
            case "black_pawn": this.GetComponent<SpriteRenderer>().sprite = black_pawn; this.isWhite = false; break;
            
            case "white_queen": this.GetComponent<SpriteRenderer>().sprite = white_queen; this.isWhite = true; break;
            case "white_knight": this.GetComponent<SpriteRenderer>().sprite = white_knight; this.isWhite = true;break;
            case "white_bishop": this.GetComponent<SpriteRenderer>().sprite = white_bishop; this.isWhite = true; break;
            case "white_king": this.GetComponent<SpriteRenderer>().sprite = white_king; this.isWhite = true;break;
            case "white_rook": this.GetComponent<SpriteRenderer>().sprite = white_rook; this.isWhite = true; break;
            case "white_pawn": this.GetComponent<SpriteRenderer>().sprite = white_pawn; this.isWhite = true;break;
        }
        
    }

    public void SetCoords(int pos) 
    {
        float x = -4.5f + (1.28f * (pos % 8f));
        float temp = pos / 8;
        float y = 4.5f - (1.28f * temp);

        this.transform.position = new Vector3(x, y,-1.0f);
    }   
}

