using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/*
Main purpose of this class is to allow us to create different colors on tiles to indicate things
This includes when a player picks up a piece, all the possible moves for that piece
The last move to be played is displayed
*/
public class MoveTile : MonoBehaviour
{
    //Relative straight forward function to place the different colored tile on the correct position on board/canvas
    public void Place(int pos)
    {
        //4.48
        float x = -4.68f + (1.28f * (pos % 8f));  //calculation for the x corrdinates
        float temp = pos / 8;
        float y = 4.48f - (1.28f * temp);  //calculation for the y coordinates

        this.transform.position = new Vector3(x, y,-1.0f);  //putting the actual object on canvas with change of position
        return;
    }
}

