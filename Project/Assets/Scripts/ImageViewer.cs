using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ImageViewer : MonoBehaviour {

    //This will give us the colour and depth data
    public MultiSourceManager mMultiSource;

    //What we will be using to output the raw image we are getting from the connect
    public RawImage mRawImage; 
	
	// Update is called once per frame
	void Update () {

        //
        mRawImage.texture = mMultiSource.GetColorTexture();
	}
}
