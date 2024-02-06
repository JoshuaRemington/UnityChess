using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Menu : MonoBehaviour
{
    public GameObject menu;
    private Game gameReference;

    void Start()
    {
        gameReference = GetComponent<Game>();
    }
    public void playerVsPlayer() 
    {
        menu.SetActive(false);
        gameReference.GameStart("");
    }

    public void botPlaysWhite() 
    {
        menu.SetActive(false);
        gameReference.GameStart("bw");
    }

    public void botPlaysBlack()
    {
        menu.SetActive(false);
        gameReference.GameStart("bb");
    }

    public void botVsBot() 
    {
        menu.SetActive(false);
        gameReference.GameStart("bvb");
    }
}
