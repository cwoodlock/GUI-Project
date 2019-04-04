using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Windows.Kinect;

public class MeasureDepth : MonoBehaviour {

    public MultiSourceManager mMultiSource;
    public Texture2D mDeptTexture;

    //Cutoff the screen to only read a certain amount of space
    //Slider to adjust sensitivity
    [Range(0,1.0f)]
    public float mDepthSensitivity = 1;
    //See how far the kinect is away from the wall
    [Range(-10, 10f)]
    public float mWallDepth = -10;

    //Parts of wall to cut off
    [Header("Top and Bottom")]
    [Range(-1, 1f)]
    public float mTopCutOff = 1;
    [Range(-1, 1f)]
    public float mBottomCutOff = -1;
    [Header("Left and Right")]
    [Range(-1, 1f)]
    public float mLeftCutOff = -1;
    [Range(-1, 1f)]
    public float mRightCutOff = 1;

    //Arrays to be used to store the camera space points and colout space points
    private CameraSpacePoint[] mCameraSpacePoints = null;
    private ColorSpacePoint[] mColorSpacePoints = null;
    private ushort[] mDepthData;
    private List<ValidPoint> mValidPoints = null;


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
        if (Input.GetKeyDown(KeyCode.Space))
        {
            mValidPoints = DepthToColour();

            //Use depthtocolor to creat a texture
            mDeptTexture = CreateTexture(mValidPoints);
        }
    }

    private List<ValidPoint> DepthToColour()
    {
        //Get Depth Data
        mDepthData = mMultiSource.GetDepthData();

        //Map Depth Data
        mMapper.MapDepthFrameToCameraSpace(mDepthData, mCameraSpacePoints);
        mMapper.MapDepthFrameToColorSpace(mDepthData, mColorSpacePoints);

        //Points to return
        List<ValidPoint> validPoints = new List<ValidPoint>();

        // Filter
        for(int i = 0; i < mDepthResolution.x / 8; i++)
        {
            //Divide by 8 to avoid index out of bounds
            for (int j = 0; j < mDepthResolution.y / 8; j++)
            {
                //downsample all of the points to help it run better
                //go through one dimensional array of camera points
                int sampleIndex = (j * mDepthResolution.x) + i;
                //Skip over 8 values between each point
                sampleIndex *= 8;

                //Cut off tests
                if(mCameraSpacePoints[sampleIndex].X < mLeftCutOff)
                {
                    continue;
                }
                if (mCameraSpacePoints[sampleIndex].X > mRightCutOff)
                {
                    continue;
                }
                if (mCameraSpacePoints[sampleIndex].Y > mTopCutOff)
                {
                    continue;
                }
                if (mCameraSpacePoints[sampleIndex].Y < mBottomCutOff)
                {
                    continue;
                }

                //Create a new valid point
                ValidPoint newPoint = new ValidPoint(mColorSpacePoints[sampleIndex], mCameraSpacePoints[sampleIndex].Z);

                //Depth Test
                if(mCameraSpacePoints[sampleIndex].Z >= mWallDepth)
                {
                    newPoint.mWithinWallDepth = true;
                }

                //Add out new pont to valid points
                validPoints.Add(newPoint);
            }
        }

        return validPoints;
    }

    //Use colorSpacePoints to create the texture
    private Texture2D CreateTexture(List<ValidPoint> validPoints)
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

        foreach(ValidPoint point in validPoints)
        {
            newTexture.SetPixel((int)point.colorSpace.X, (int)point.colorSpace.Y, Color.black);
        }

        newTexture.Apply();

        return newTexture;
    }

}

//Use this to store a reference to a colour point and give it a depth value
public class ValidPoint
{
    public ColorSpacePoint colorSpace;
    public float z = 0.0f;

    //Check if within wall depth
    public bool mWithinWallDepth = false;

    //Constructor
    public ValidPoint(ColorSpacePoint newColorSpace, float newZ)
    {
        colorSpace = newColorSpace;
        z = newZ;
    }

}
