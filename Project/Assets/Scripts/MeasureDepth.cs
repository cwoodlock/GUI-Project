using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Windows.Kinect;

public class MeasureDepth : MonoBehaviour {

    public MultiSourceManager mMultiSource;
    public Texture2D mDeptTexture;

    //Arrays to be used to store the camera space points and colout space points
    private CameraSpacePoint[] mCameraSpacePoints = null;
    private ColorSpacePoint[] mColorSpacePoints = null;
    private ushort[] mDepthData;


    //Has reference to our kinect sensor
    private KinectSensor mSensor = null;

    //Way to map depth data to colour points
    private CoordinateMapper mMapper = null;

    //Depth sensor creates image that is 512x424
    private readonly Vector2Int mDepthResolution = new Vector2Int(512, 424);


    private void Awake()
    {
        mSensor = KinectSensor.GetDefault();
        mMapper = mSensor.CoordinateMapper;

        //Initialise array size
        int arraySize = mDepthResolution.x * mDepthResolution.y;

        //Create arrays
        mCameraSpacePoints = new CameraSpacePoint[arraySize];
        mColorSpacePoints = new ColorSpacePoint[arraySize];
    }

    private void Update()
    {
        //if (Input.GetKeyDown(KeyCode.Space))
        {
            DepthToColour();

            //Use depthtocolor to creat a texture
            mDeptTexture = CreateTexture();
        }
    }

    private void DepthToColour()
    {
        //Get Depth Data
        mDepthData = mMultiSource.GetDepthData();

        //Map Depth Data
        mMapper.MapDepthFrameToCameraSpace(mDepthData, mCameraSpacePoints);
        mMapper.MapDepthFrameToColorSpace(mDepthData, mColorSpacePoints);

        // Filter
    }

    //Use colorSpacePoints to create the texture
    private Texture2D CreateTexture()
    {
        //Create new texture and make sure it is transparet by using Alpha8 and ensure it is lined up correctly
        Texture2D newTexture = new Texture2D(1920, 1080, TextureFormat.Alpha8, false);

        for(int x = 0; x < 1920; x++)
        {
            for (int y = 0; y < 1080; y++)
            {
                //Set all of the pizes to be clear
                newTexture.SetPixel(x, y, Color.clear);
            }
        }

        foreach(ColorSpacePoint point in mColorSpacePoints)
        {
            newTexture.SetPixel((int)point.X, (int)point.Y, Color.black);
        }

        newTexture.Apply();

        return newTexture;
    }
}
