using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpawnRects : MonoBehaviour {

    public GameObject rectTrigger;
    public RawImage[] rects;

    public void CreateRects()
    {
        int arrayIdx = Random.Range(0, rects.Length);
        RawImage rectImage = rects[arrayIdx];
        string rectName = rectImage.name;

        GameObject newRect = Instantiate(rectTrigger);

        newRect.name = rectName;
        //newRect.GetComponent<RectTrigger>().
    }
}
