using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Windows.Kinect;

public class MeasureDepth : MonoBehaviour {

    public MultiSourceManager mMultiSource;

    //Arrays to be used to store the camera space points and colout space points
    private CameraSpacePoint[] mCameraSpacePoints = null;
    private ColorSpacePoint[] mColorSpacePoints = null;


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

        mCameraSpacePoints = new CameraSpacePoint[arraySize];
        mColorSpacePoints = new ColorSpacePoint[arraySize];
    }
}
