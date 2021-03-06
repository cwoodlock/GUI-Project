﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Windows.Kinect;

public class MeasureDepth : MonoBehaviour {
    #region Declarations
    public delegate void NewTriggerPoints(List<Vector2> triggerPoints);
    public static event NewTriggerPoints OnTriggerPoints = null;

    public MultiSourceManager mMultiSource;
    public Texture2D mDeptTexture;

    //Cutoff the screen to only read a certain amount of space
    //Slider to adjust sensitivity
    [Range(0, 1.0f)]
    public float mDepthSensitivity = 1f;
    //See how far the kinect is away from the wall
    [Range(-10, 10f)]
    public float mWallDepth = -10f;

    //Parts of wall to cut off
    [Header("Top and Bottom")]
    [Range(-1, 1f)]
    public float mTopCutOff = 1f;
    [Range(-1, 1f)]
    public float mBottomCutOff = -1f;
    [Header("Left and Right")]
    [Range(-1, 1f)]
    public float mLeftCutOff = -1f;
    [Range(-1, 1f)]
    public float mRightCutOff = 1f;

    //Arrays to be used to store the camera space points and colout space points
    private CameraSpacePoint[] mCameraSpacePoints = null;
    private ColorSpacePoint[] mColorSpacePoints = null;
    private ushort[] mDepthData;
    private List<ValidPoint> mValidPoints = null;
    private List<Vector2> mTriggerPoints = null;


    //Has reference to our kinect sensor and camera
    private KinectSensor mSensor = null;
    private Camera mCamera = null;

    //Way to map depth data to colour points
    private CoordinateMapper mMapper = null;


    //Depth sensor creates image that is 512x424
    private readonly Vector2Int mDepthResolution = new Vector2Int(512, 424);
    private Rect mRect;

    #endregion

    private void Awake()
    {
        mSensor = KinectSensor.GetDefault();
        mMapper = mSensor.CoordinateMapper;
        mCamera = Camera.main;

        //Initialise array size
        int arraySize = mDepthResolution.x * mDepthResolution.y;

        //Create arrays
        mCameraSpacePoints = new CameraSpacePoint[arraySize];
        mColorSpacePoints = new ColorSpacePoint[arraySize];
    }

    private void Update()
    {
        mValidPoints = DepthToColour();

        mTriggerPoints = FilterToTrigger(mValidPoints);

        if(OnTriggerPoints != null && mTriggerPoints.Count != 0)
        {
            OnTriggerPoints(mTriggerPoints);
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {

            mRect = CreateRect(mValidPoints);

            //Use depthtocolor to creat a texture
            mDeptTexture = CreateTexture(mValidPoints);
        }
    }

    public void setDepth(float depth)
    {
        Debug.Log(depth);
        mDepthSensitivity = depth;
    }

    public void setWallDepth(float depth)
    {
        Debug.Log(depth);
        mWallDepth = depth;
    }

    public void setTop(float val)
    {
        Debug.Log(val);
        mTopCutOff = val;
    }

    public void setBottom(float val)
    {
        Debug.Log(val);
        mBottomCutOff = val;
    }

    public void setLeft(float val)
    {
        Debug.Log(val);
        mLeftCutOff = val;
    }

    public void setRight(float val)
    {
        Debug.Log(val);
        mRightCutOff = val;
    }

    //Test function to draw rectangle
    private void OnGUI()
    {
        GUI.Box(mRect, "");

        if(mTriggerPoints == null)
        {
            return;
        }
        foreach(Vector2 point in mTriggerPoints)
        {
            Rect rect = new Rect(point, new Vector2(10, 10));
            GUI.Box(rect, "");
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
        for (int i = 0; i < mDepthResolution.x / 8; i++)
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
                if (mCameraSpacePoints[sampleIndex].X < mLeftCutOff)
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
                if (mCameraSpacePoints[sampleIndex].Z >= mWallDepth)
                {
                    newPoint.mWithinWallDepth = true;
                }

                //Add out new pont to valid points
                validPoints.Add(newPoint);
            }
        }

        return validPoints;
    }

    //Filtering valid points to a list of trigger points
    private List<Vector2> FilterToTrigger(List<ValidPoint> validPoints)
    {
        List<Vector2> triggerPoints = new List<Vector2>();

        //Go through each of the points and check if they are closer to teh kinect than where the wall is
        foreach(ValidPoint point in validPoints)
        {
            //Not beyond the wall
            if (!point.mWithinWallDepth)
            {
                
                //test the screen point
                if(point.z < mWallDepth * mDepthSensitivity)
                {
                    //Create the screen point
                    Vector2 screenPoint = ScreenToCamera(new Vector2(point.colorSpace.X, point.colorSpace.Y));

                    triggerPoints.Add(screenPoint);
                }
            }
        }

        return triggerPoints;
    }

    //Use colorSpacePoints to create the texture
    private Texture2D CreateTexture(List<ValidPoint> validPoints)
    {
        //Create new texture and make sure it is transparet by using Alpha8 and ensure it is lined up correctly
        Texture2D newTexture = new Texture2D(1920, 1080, TextureFormat.Alpha8, false);

        for (int x = 0; x < 1920; x++)
        {
            for (int y = 0; y < 1080; y++)
            {
                //Set all of the pizes to be clear
                newTexture.SetPixel(x, y, Color.clear);
            }
        }

        foreach (ValidPoint point in validPoints)
        {
            newTexture.SetPixel((int)point.colorSpace.X, (int)point.colorSpace.Y, Color.black);
        }

        newTexture.Apply();

        return newTexture;
    }

    #region Rect Creation
    private Rect CreateRect(List<ValidPoint> points)
    {
        //If the points we pass in are 0 then we create an empty rect
        if(points.Count == 0)
        {
            return new Rect();
        }

        // Get corners for the rect
        Vector2 topLeft = GetTopLeft(points);
        Vector2 bottomRight = GetBottomRight(points);

        //Translate to viewport
        Vector2 screenTopLeft = ScreenToCamera(topLeft);
        Vector2 screenBottomRight = ScreenToCamera(bottomRight);


        //Get dimensions of the rect (drawn from the top left)
        int width = (int)(screenBottomRight.x - screenTopLeft.x);
        int height = (int)(screenBottomRight.y - screenTopLeft.y);

        //Create the rect
        Vector2 size = new Vector2(width, height);
        Rect rect = new Rect(screenTopLeft, size);


        return rect;
    }

    //This will get the top left of the mapped area
    private Vector2 GetTopLeft(List<ValidPoint> points)
    {
        //What is going to be the highest number
        Vector2 topLeft = new Vector2(int.MaxValue, int.MaxValue);

        foreach(ValidPoint point in points)
        {
            //LEft most x value
            if(point.colorSpace.X < topLeft.x)
            {
                topLeft.x = point.colorSpace.X;
            }
            //Top most y value
            if (point.colorSpace.Y < topLeft.y)
            {
                topLeft.y = point.colorSpace.Y;
            }
        }

        return topLeft;
    }

    //This will get the bottom right of the game area
    private Vector2 GetBottomRight(List<ValidPoint> points)
    {
        //What is going to be the highest number
        Vector2 bottomRight = new Vector2(int.MinValue, int.MinValue);

        foreach (ValidPoint point in points)
        {
            //Right most x value
            if (point.colorSpace.X > bottomRight.x)
            {
                bottomRight.x = point.colorSpace.X;
            }
            //bottom most y value
            if (point.colorSpace.Y > bottomRight.y)
            {
                bottomRight.y = point.colorSpace.Y;
            }
        }

        return bottomRight;
    }

    //Converting two points topLeft + bottomRight and give proper location on sreen
    private Vector2 ScreenToCamera(Vector2 screenPosition)
    {
        //Get the position of the screen
        Vector2 normalisedScreen = new Vector2(Mathf.InverseLerp(0, 1920, screenPosition.x),
            Mathf.InverseLerp(0, 1080, screenPosition.y));

        //Convert normalisedScreen back to pixel position
        Vector2 screenPoint = new Vector2(normalisedScreen.x * mCamera.pixelWidth, normalisedScreen.y * mCamera.scaledPixelHeight);

        //return the screen positon
        return screenPoint;
    }

    #endregion

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
