using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveTile : MonoBehaviour
{
    public void Place(int pos)
    {
        //4.48
        float x = -4.48f + (1.28f * (pos % 8f));
        float temp = pos / 8;
        float y = 4.48f - (1.28f * temp);

        this.transform.position = new Vector3(x, y,-1.0f);
        return;
    }
}

