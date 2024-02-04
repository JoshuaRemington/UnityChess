using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Menu : MonoBehaviour
{
    //public GameObject menu;
    public void playerVsPlayer() 
    {
        //menu.SetActive(false);
        Debug.Log("pvp");
    }

    public void botPlaysWhite() 
    {
        //menu.SetActive(false);
        Debug.Log("bot white");
    }

    public void botPlaysBlack()
    {
        //menu.SetActive(false);
        Debug.Log("bot black");
    }

    public void botVsBot() 
    {
        //menu.SetActive(false);
        Debug.Log("bvb");
    }
}
