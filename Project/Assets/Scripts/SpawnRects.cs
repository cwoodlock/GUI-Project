using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpawnRects : MonoBehaviour {

    public GameObject[] rects;

    public void Start()
    {
        rects[0].transform.position = new Vector3(Screen.width/2, Screen.height - 160, 0);
        //Debug.Log("Y val: " + Screen.height);
        rects[1].transform.position = new Vector3((Screen.width / 2) + 200, Screen.height - 360, 0);
        rects[2].transform.position = new Vector3((Screen.width / 2) - 200, Screen.height - 360, 0);
        rects[3].transform.position = new Vector3((Screen.width / 2) + 400, Screen.height - 560, 0);
        rects[4].transform.position = new Vector3((Screen.width / 2) - 400, Screen.height - 560, 0);

    }
}
