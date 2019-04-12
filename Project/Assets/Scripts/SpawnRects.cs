using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpawnRects : MonoBehaviour {

    public GameObject[] rects;
    Color[] colours = { Color.black, Color.green, Color.red };
    public int randnum;
    public int seconds = 2;
    public bool stop;

    private int randRect;

    public void Start()
    {
        //Set the location of the rects
        rects[0].transform.position = new Vector3(Screen.width/2, Screen.height - 160, 0);
        //Debug.Log("Y val: " + Screen.height);
        rects[1].transform.position = new Vector3((Screen.width / 2) + 200, Screen.height - 260, 0);
        rects[2].transform.position = new Vector3((Screen.width / 2) - 200, Screen.height - 260, 0);
        rects[3].transform.position = new Vector3((Screen.width / 2) + 400, Screen.height - 360, 0);
        rects[4].transform.position = new Vector3((Screen.width / 2) - 400, Screen.height - 360, 0);

        StartCoroutine(NumberGen());

        

    }

    //Generate a new number every 2 seconds
    IEnumerator NumberGen()
    {
        yield return new WaitForSeconds(seconds);

        while (!stop)
        {
            randRect = Random.Range(0, rects.Length);
            rects[randRect].GetComponent<Image>().color = colours[1];

            yield return new WaitForSeconds(seconds);

            rects[randRect].GetComponent<Image>().color = colours[0];
        }
    }
}
