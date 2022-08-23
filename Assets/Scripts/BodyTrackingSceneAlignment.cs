using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using System.IO;    //for writing out files
using System;

public class BodyTrackingSceneAlignment : MonoBehaviour
{

    public GameObject bodyHead;
    public GameObject bodyRightHand;
    public GameObject bodyLeftHand;
    public GameObject bodyRightFoot;
    public GameObject bodyLeftFoot;


    public GameObject bodyParent;   //there may be none so this is probably going to be 1

    public GameObject ARSessionOriginBody;
    public GameObject ARCamera;

    public GameObject avatarHead;
    public GameObject avatarRightHand;
    public GameObject avatarLeftHand;
    public GameObject networkPlayer;
    //public GameObject arTracker;

    //not sure if network player will be scaled
    public GameObject WingSceneC;    //WingSceneContainer
    public Toggle ManualToggle;
    public Slider AngleSlider;
    public TMPro.TMP_Dropdown TrackingType;
    public Calibration CalibrationScript;
    //public float RefreshTime;
    private List<Quaternion> prevSceneVals = new List<Quaternion>();
    private int framesToInit = 120*5;    //at 120 fps lets max this at 10 sec for now, 5 for shorter initialization trial
    private int framesToCollect;
    private Quaternion SceneRotAverage = Quaternion.identity;
    private List<Quaternion> increasingQuats = new List<Quaternion>();
    //private List<Quaternion> increasingTrackerQuats = new List<Quaternion>();
    private List<Vector3> allPositionsWSO = new List<Vector3>();
    private List<Vector3> allPositionsWSC = new List<Vector3>();
    private Vector3 averagePos = Vector3.zero;
    private Vector3 VrScenePosition = new Vector3(-4.06f, 0, -9.6f);    //(-4.06f, 0, -9.6f)
    //private int maxTris = 0;
    //private int maxVerts = 0;
    private int fileNum = 1;
    private LineRenderer line1;
    private LineRenderer line2;
    private GameObject estimatedScene;
    private GameObject actualScene;

    public string csvname = "";
    private bool csvWritten = true;
    //dont need to store feet vals

    [System.Serializable]
    public class StoredData
    {
        public float ArDistanceMag;
        public float VrDistanceMag;
        public float DistanceOff;
        
        public float ArSceneRot;
        public float VrSceneRot;
        public float RotationOff;

        public Vector3 ARiPadPos;
        public Vector3 VRiPadPos;
        public float BodyPositionOff;
        public float ScenePositionOff;
    }

    private List<StoredData> myStoredDataList = new List<StoredData>();

    // Start is called before the first frame update
    void Start()
    {
        framesToCollect = framesToInit + 1000;
        CalibrationScript.CalibratePressed.AddListener(CalibratePressed);
        csvname = Path.Combine(Application.persistentDataPath, "output");
        estimatedScene = new GameObject("estimatedScenePosition");
        actualScene = new GameObject("actualScenePosition");

        //For Testing
        //line1 = new GameObject("Line1").AddComponent<LineRenderer>();
        //line1.startColor = Color.black;
        //line1.endColor = Color.black;
        //line1.startWidth = 0.03f;
        //line1.endWidth = 0.03f;
        //line1.positionCount = 2;
        //line1.useWorldSpace = true;

        //line2 = new GameObject("Line2").AddComponent<LineRenderer>();
        //line2.startColor = Color.black;
        //line2.endColor = Color.black;
        //line2.startWidth = 0.03f;
        //line2.endWidth = 0.03f;
        //line2.positionCount = 2;
        //line2.useWorldSpace = true;
    }

    private void FixedUpdate()
    {
        //show max verts and tris
        //if (UnityEditor.UnityStats.vertices > maxVerts)
        //{
        //    maxVerts = UnityEditor.UnityStats.vertices;
        //}
        //if(UnityEditor.UnityStats.triangles > maxTris)
        //{
        //    maxTris = UnityEditor.UnityStats.triangles;
        //}
        //Debug.Log("maxVerts: " + maxVerts);
        //Debug.Log("maxTris: " + maxTris);

        //Try storing last ten/three seconds of values and averaging? or stick with calibrate button?
        bodyParent = GameObject.Find("ControlledRobot(Clone)");
        networkPlayer = GameObject.Find("NetworkPlayer(Clone)");
        if (TrackingType.value == 1 && bodyParent != null && networkPlayer != null)
        {
            ARSessionOriginBody = GameObject.Find("AR Session Origin Body");
            ARCamera = ARSessionOriginBody.transform.Find("AR Camera").gameObject;
            //Find body Gameobjects
            bodyHead = GameObject.Find("Head").gameObject;    //Could grab full path
            //Debug.Log("Body Head Position: " + bodyHead.transform.position.ToString());
            bodyLeftHand = GameObject.Find("LeftHand").gameObject;    //Could grab full path
            //Debug.Log("Body Left Hand Position: " + bodyLeftHand.transform.position.ToString());
            bodyRightHand = GameObject.Find("RightHand").gameObject;    //Could grab full path
            //Debug.Log("Body Right Hand Position: " + bodyRightHand.transform.position.ToString());
            bodyRightFoot = GameObject.Find("RightFoot").gameObject;
            bodyLeftFoot = GameObject.Find("LeftFoot").gameObject;

            avatarHead = GameObject.Find("Avatar").gameObject;
            //Debug.Log("Avatar Head Position: " + avatarHead.transform.position.ToString());
            avatarLeftHand = GameObject.Find("AvatarLeftHand").gameObject;
            //Debug.Log("Avatar Right Hand Position: " + avatarLeftHand.transform.position.ToString());
            avatarRightHand = GameObject.Find("AvatarRightHand").gameObject;
            //arTracker = GameObject.Find("arTracker");

            avatarHead.GetComponent<MeshRenderer>().enabled = false;
            avatarHead.transform.Find("LabelCanvas").gameObject.SetActive(false);
            avatarLeftHand.GetComponent<MeshRenderer>().enabled = false;
            avatarRightHand.GetComponent<MeshRenderer>().enabled = false;

            GameObject WingSceneObjects = WingSceneC.transform.GetComponentsInChildren<Transform>(true)[1].gameObject;

            //ROTATION
            //How good is the body tracking head rotation? probably need to get angle from all three points
            if (ManualToggle.isOn)  //Manual Scene Alignment
            {
                //print("")
                prevSceneVals.Clear();  //reset stored frames if switched to manual
                if (csvWritten == false)
                {
                    Debug.Log("WRITING CSV");
                    WriteCSV();                 //write out stored data
                    Debug.Log("CSV WAS WRITTEN");
                    myStoredDataList.Clear();   //clear stored data
                    csvWritten = true;
                }

                //WingSceneC.transform.position = bodyHead.transform.position - (avatarHead.transform.localPosition + networkPlayer.transform.localPosition);
                Vector3 newArWingSceneCposition;
                Vector3 newArWingSceneObjectPosition;
                newArWingSceneCposition = bodyHead.transform.position;  //Using wing scene container parent as rotation point
                WingSceneC.transform.position = newArWingSceneCposition;
                //VR scene position and rotation hardcoded because it never moves in VR scene
                newArWingSceneObjectPosition = VrScenePosition - avatarHead.transform.position;     //Vector from VR HMD to WingSceneObject local position
                WingSceneObjects.transform.localPosition = newArWingSceneObjectPosition;

                //Debug.Log("Angle Slider: " + AngleSlider.value);
                Quaternion id = Quaternion.identity;
                Transform t = WingSceneC.transform;
                t.rotation = Quaternion.identity;
                t.RotateAround(bodyHead.transform.position, Vector3.up, AngleSlider.value);
                WingSceneC.transform.rotation = t.rotation;
                //Debug.Log("WingScene Y: " + WingSceneC.transform.rotation.y);
            }
            else   //Automatic rotation
            {
                csvWritten = false;
                if(prevSceneVals.Count() < framesToInit)   //Just store first ten seconds of rotation values
                {
                    //Determine body direction
                    Vector3 BodyToLeft = new Vector3(bodyLeftHand.transform.position.x, 0, bodyLeftHand.transform.position.z)
                        - new Vector3(bodyHead.transform.position.x, 0, bodyHead.transform.position.z);
                    Vector3 BodyToRight = new Vector3(bodyRightHand.transform.position.x, 0, bodyRightHand.transform.position.z)
                        - new Vector3(bodyHead.transform.position.x, 0, bodyHead.transform.position.z);
                    Vector3 BodyDirection = BodyToLeft + BodyToRight;
                    //determine avatar direction
                    Vector3 AvatarToLeft = new Vector3(avatarLeftHand.transform.position.x, 0, avatarLeftHand.transform.position.z)
                        - new Vector3(avatarHead.transform.position.x, 0, avatarHead.transform.position.z);
                    Vector3 AvatarToRight = new Vector3(avatarRightHand.transform.position.x, 0, avatarRightHand.transform.position.z)
                        - new Vector3(avatarHead.transform.position.x, 0, avatarHead.transform.position.z);
                    Vector3 AvatarDirection = AvatarToLeft + AvatarToRight;

                    Quaternion BodyDirectionQuat = Quaternion.LookRotation(BodyDirection, Vector3.up);
                    Quaternion AvatarDirectionQuat = Quaternion.LookRotation(AvatarDirection, Vector3.up);
                    float yDif = BodyDirectionQuat.eulerAngles.y - AvatarDirectionQuat.eulerAngles.y;

                    Vector3 newArWingSceneCposition;
                    Vector3 newArWingSceneObjectPosition;
                    newArWingSceneCposition = bodyHead.transform.position;  //Using wing scene container parent as rotation point
                    WingSceneC.transform.position = newArWingSceneCposition;
                    //VR scene position and rotation hardcoded because it never moves in VR scene
                    newArWingSceneObjectPosition = VrScenePosition - avatarHead.transform.position;     //Vector from VR HMD to WingSceneObject local position
                    WingSceneObjects.transform.localPosition = newArWingSceneObjectPosition;//NEED to change this to give average not alter actual

                    allPositionsWSC.Add(WingSceneC.transform.position);
                    allPositionsWSO.Add(WingSceneObjects.transform.position);//NEED to change this to give average not alter actual

                    //Line renderer
                    //line1.SetPosition(0, bodyHead.transform.position);
                    //line1.SetPosition(1, bodyHead.transform.position + BodyDirection);
                    //line2.SetPosition(0, bodyHead.transform.position);
                    //line2.SetPosition(1, bodyHead.transform.position + AvatarDirection);

                    //for evaluation
                    //Quaternion VrTrackerDirectionQuat = Quaternion.LookRotation(arTracker.transform.position - avatarHead.transform.position, Vector3.up);
                    Quaternion ArTrackerDirectionQuat = Quaternion.LookRotation(ARCamera.transform.position - bodyHead.transform.position, Vector3.up);
                    //float trackerYdif = ArTrackerDirectionQuat.eulerAngles.y - VrTrackerDirectionQuat.eulerAngles.y;
                    //Quaternion currentTrackerQuat = Quaternion.AngleAxis(trackerYdif, WingSceneC.transform.up);

                    //finds median rotation
                    Quaternion currentQuat = Quaternion.AngleAxis(yDif, WingSceneC.transform.up);

                    Debug.Log("WingSceneO Position: " + WingSceneObjects.transform.position.x + "," + WingSceneObjects.transform.position.y + "," + WingSceneObjects.transform.position.z);

                    if (increasingQuats.Count == 0)
                    {
                        increasingQuats.Add(currentQuat);
                        //increasingTrackerQuats.Add(currentTrackerQuat);
                        allPositionsWSC.Add(WingSceneC.transform.position);
                        allPositionsWSO.Add(WingSceneObjects.transform.position);
                    }
                    else
                    {
                        for (int x = 0; x < increasingQuats.Count(); x++)
                        {
                            if (increasingQuats[x].eulerAngles.y >= currentQuat.eulerAngles.y)
                            {
                                increasingQuats.Insert(x, currentQuat);
                                break;
                            }
                        }
                        //for evaluation
                        //for (int x = 0; x < increasingTrackerQuats.Count(); x++)
                        //{
                        //    if (increasingTrackerQuats[x].eulerAngles.y >= currentTrackerQuat.eulerAngles.y)
                        //    {
                        //        increasingTrackerQuats.Insert(x, currentTrackerQuat);
                        //        break;
                        //    }
                        //}

                        int median = increasingQuats.Count() / 2;
                        SceneRotAverage = increasingQuats[median];
                    }
                    //newArWingSceneCrotation = SceneRotAverage.eulerAngles;
                    //WingSceneC.transform.rotation = SceneRotAverage;
                    Debug.Log("YDIF = " + yDif);
                    WingSceneC.transform.rotation = Quaternion.AngleAxis(yDif, WingSceneC.transform.up);
                }
                else if (prevSceneVals.Count < framesToCollect)
                {
                    //Evaluation should be based on wingsceneobjects global position and rotation
                    //do not change rotation of scene
                    if (prevSceneVals.Count == framesToInit)
                    {
                        Debug.Log("Initialization Completed");
                        //Average both positions
                        Vector3 avePos = Vector3.zero;
                        foreach (Vector3 posWSC in allPositionsWSC)
                        {
                            avePos += posWSC;
                        }
                        WingSceneC.transform.position = avePos / allPositionsWSC.Count();
                        avePos = Vector3.zero;
                        foreach (Vector3 pos in allPositionsWSO)
                        {
                            avePos += pos;
                        }
                        WingSceneObjects.transform.position = avePos / allPositionsWSO.Count();

                        //Find rotation differences
                        int median = increasingQuats.Count() / 2;
                        //int trMedian = increasingTrackerQuats.Count() / 2;
                        Quaternion RotateByMedian = increasingQuats[median];
                        //Quaternion trRotateByMedian = increasingTrackerQuats[trMedian];
                        WingSceneC.transform.rotation = RotateByMedian;     //try replacing this with trrotatebymedian to ensure accuracy for evaluation
                        //Debug.Log("FINAL ROTATION DIFFs, Expected: " + RotateByMedian.eulerAngles.y + "| Actual: " + trRotateByMedian.eulerAngles.y);
                        //float RotationOffBy = Math.Abs(RotateByMedian.eulerAngles.y - trRotateByMedian.eulerAngles.y);
                        //Debug.Log("FINAL ROTATION DIFFERENCE: " + RotationOffBy);

                        StoredData sd = new StoredData();
                        //sd.RotationOff = RotationOffBy;
                        //sd.VrSceneRot = trRotateByMedian.eulerAngles.y;
                        sd.ArSceneRot = RotateByMedian.eulerAngles.y;
                        myStoredDataList.Add(sd);
                    }
                    //Find position differences
                    //Vector3 TrackerToAvatar = new Vector3(avatarHead.transform.position.x, 0, avatarHead.transform.position.z)
                    //    - new Vector3(arTracker.transform.position.x, 0, arTracker.transform.position.z);
                    //Vector3 CameraToBody = new Vector3(bodyHead.transform.position.x, 0, bodyHead.transform.position.z)
                    //    - new Vector3(ARCamera.transform.position.x, 0, ARCamera.transform.position.z);
                    //Vector3 TrackerToScene = VrScenePosition      //just use these for magnitude
                    //    - new Vector3(arTracker.transform.position.x, 0, arTracker.transform.position.z);
                    //Vector3 CameraToScene = new Vector3(WingSceneObjects.transform.position.x, 0, WingSceneObjects.transform.position.z)
                    //    - new Vector3(ARCamera.transform.position.x, 0, ARCamera.transform.position.z);
                    //Vector3 CameraForward = new Vector3(ARCamera.transform.forward.x, 0, ARCamera.transform.forward.z);
                    //Vector3 TrackerForward = new Vector3(arTracker.transform.forward.x, 0, arTracker.transform.forward.z);
                    //float ArToBodyMag = (TrackerToAvatar.magnitude * CameraToScene.magnitude) / TrackerToScene.magnitude;
                    //float VrAngleBetweenBody = Vector3.Angle(CameraForward, CameraToBody);
                    //float ArAngleBetweenBody = Vector3.Angle(TrackerForward, TrackerToAvatar);
                    //Debug.Log("AR ANGLE BODY: " + ArAngleBetweenBody + " VR ANGLE BODY: " + VrAngleBetweenBody);
                    //Vector3 VrVecInArSceneBody = CameraToBody;
                    //VrVecInArSceneBody = Quaternion.Euler(0, VrAngleBetweenBody-ArAngleBetweenBody, 0) * VrVecInArSceneBody;
                    //VrVecInArSceneBody = VrVecInArSceneBody.normalized * ArToBodyMag;
                    //float BodyPosOff = Vector3.Distance(VrVecInArSceneBody, CameraToBody);
                    //Debug.Log("BODY POSOFF: " + BodyPosOff);

                    //float ArToSceneMag = (TrackerToScene.magnitude * CameraToBody.magnitude) / TrackerToAvatar.magnitude;
                    //float VrAngleBetweenScene = Vector3.Angle(CameraForward, CameraToScene);
                    //float ArAngleBetweenScene = Vector3.Angle(TrackerForward, TrackerToScene);
                    //Debug.Log("AR ANGLE SCENE: " + ArAngleBetweenScene + " VR ANGLE SCENE: " + VrAngleBetweenScene);
                    //Vector3 VrVecInArSceneS = CameraToScene;
                    //VrVecInArSceneS = Quaternion.Euler(0, VrAngleBetweenScene - ArAngleBetweenScene, 0) * VrVecInArSceneS;
                    //VrVecInArSceneS = VrVecInArSceneS.normalized * ArToSceneMag;
                    //float ScenePosOff = Vector3.Distance(VrVecInArSceneS, CameraToScene);
                    //Debug.Log("SCENE POSOFF: " + ScenePosOff);


                    //float PositionOffBy = BodyPosOff+ScenePosOff;
                    //Vector3 AvatarToTracker = new Vector3(arTracker.transform.position.x, 0, arTracker.transform.position.z)
                    //                        - new Vector3(avatarHead.transform.position.x, 0, avatarHead.transform.position.z);
                    //Vector3 AvatarToScene = new Vector3(-4.06f, 0, -9.6f)
                    //                        - new Vector3(avatarHead.transform.position.x, 0, avatarHead.transform.position.z);
                    //Vector3 BodyToTracker = new Vector3(ARCamera.transform.position.x, 0, ARCamera.transform.position.z)
                    //                        - new Vector3(bodyHead.transform.position.x, 0, bodyHead.transform.position.z);
                    //Vector3 BodyToScene = new Vector3(WingSceneObjects.transform.position.x, 0, WingSceneObjects.transform.position.z)
                    //                        - new Vector3(bodyHead.transform.position.x, 0, bodyHead.transform.position.z);
                    //float VrTrackerToSceneAngle = Vector3.Angle(new Vector3(AvatarToTracker.x, 0, AvatarToTracker.z), 
                    //                                            new Vector3(AvatarToScene.x, 0, AvatarToScene.z));    //No y since we can just check heights
                    //float ARCameraToSceneAngle = Vector3.Angle(new Vector3(BodyToTracker.x, 0, BodyToTracker.z),
                    //                                            new Vector3(BodyToScene.x, 0, BodyToScene.z));//this is just to see how different these angles are
                    //float EstimatedBodyToSceneMagnitude = (BodyToTracker.magnitude * AvatarToScene.magnitude) / AvatarToTracker.magnitude;  //bodyToTracker/bodyToScene=avatarToTracker/avatarToScene
                    //Vector3 TrackerAngle = increasingTrackerQuats[increasingTrackerQuats.Count() / 2].eulerAngles;
                    ////Vector2 EstimatedArScenePosition = new Vector2(Mathf.Cos(TrackerAngle.y + VrTrackerToSceneAngle), Mathf.Sin(TrackerAngle.y + VrTrackerToSceneAngle)) * EstimatedBodyToSceneMagnitude;   //does not account for y distance off

                    //actualScene.transform.position = Vector3.zero;
                    //actualScene.transform.rotation = Quaternion.identity;
                    //actualScene.transform.position = WingSceneObjects.transform.position - new Vector3(-4.06f, 0, -9.6f);
                    //actualScene.transform.rotation = WingSceneObjects.transform.rotation;


                    //estimatedScene.transform.position = Vector3.zero;//WingSceneC.transform.position
                    //estimatedScene.transform.rotation = Quaternion.identity;
                    //estimatedScene.transform.position = new Vector3(ARCamera.transform.position.x, 0, ARCamera.transform.position.z) + new Vector3(-4.06f, 0, -9.6f);//+ new Vector3(-4.06f, 0, -9.6f) + WingSceneC.transform.position
                    //estimatedScene.transform.rotation = ARCamera.transform.rotation;
                    //Debug.Log("VR Angle: " + VrTrackerToSceneAngle + " AR Angle: " + ARCameraToSceneAngle);
                    //estimatedScene.transform.Rotate(WingSceneC.transform.up,TrackerAngle.y + VrTrackerToSceneAngle + WingSceneC.transform.rotation.y);
                    //Vector3 estimatedScenePosition = estimatedScene.transform.position;
                    //estimatedScenePosition = estimatedScenePosition.normalized * EstimatedBodyToSceneMagnitude;
                    //estimatedScene.transform.position = estimatedScenePosition + WingSceneC.transform.position;

                    //float PositionOffBy = Vector3.Distance(new Vector3(WingSceneObjects.transform.position.x, 0, WingSceneObjects.transform.position.z), 
                    //    new Vector3(estimatedScene.transform.position.x, 0, estimatedScene.transform.position.z)); // might only need to add y to estimated arSceneposition and then can keep full wingsceneobjects position
                    StoredData sdp = new StoredData();
                    //sdp.ScenePositionOff = ScenePosOff;     //Just x and z off, not y yet
                    //sdp.BodyPositionOff = BodyPosOff;
                    //Debug.Log("ESTIMATED MAGNITUDE: " + BodyToTracker.magnitude + " " + AvatarToScene.magnitude+ " " + AvatarToTracker.magnitude);
                    //Debug.Log("VRTrackerToScene: " + VrTrackerToSceneAngle + " TrackerAngle: " + TrackerAngle.y);
                    //Debug.Log("ESTIMATED MAGNITUDE: " + EstimatedBodyToSceneMagnitude);
                    //Debug.Log("FINAL POSITION DIFFERENCE: " + PositionOffBy);
                    myStoredDataList.Add(sdp);

                    //Line renderer
                    
                    //line1.SetPosition(0, new Vector3(bodyHead.transform.position.x, 0, bodyHead.transform.position.z));
                    //line1.SetPosition(1, new Vector3(estimatedScene.transform.position.x, 0, estimatedScene.transform.position.z));   // - new Vector3(-4.06f, 0, -9.6f)
                    //line2.SetPosition(0, new Vector3(avatarHead.transform.position.x, 0, avatarHead.transform.position.z));   // - new Vector3(-4.06f, 0, -9.6f)
                    //line2.SetPosition(1, new Vector3(-4.06f, 0, -9.6f));
                    //Debug.Log("BODYHEAD POSITION: " + bodyHead.transform.position); //see if distance between these positions is correct
                    //Debug.Log("ESTIMATED SCENE POSITION: " + estimatedScene.transform.position);

                    //line1.SetPosition(0, Vector3.zero);//new Vector3(ARCamera.transform.position.x, 0, ARCamera.transform.position.z)
                    //line1.SetPosition(1, VrVecInArSceneS);   // - new Vector3(-4.06f, 0, -9.6f)
                    //line2.SetPosition(0, Vector3.zero);   // - new Vector3(-4.06f, 0, -9.6f) new Vector3(ARCamera.transform.position.x, 0, ARCamera.transform.position.z)
                    //line2.SetPosition(1, CameraToScene);

                    //WingSceneObjects.transform.position = estimatedScene.transform.position;

                    ////Determine body direction
                    //Vector3 BodyToLeft = new Vector3(bodyLeftHand.transform.position.x, 0, bodyLeftHand.transform.position.z)
                    //    - new Vector3(bodyHead.transform.position.x, 0, bodyHead.transform.position.z);
                    //Vector3 BodyToRight = new Vector3(bodyRightHand.transform.position.x, 0, bodyRightHand.transform.position.z)
                    //    - new Vector3(bodyHead.transform.position.x, 0, bodyHead.transform.position.z);
                    //Vector3 BodyDirection = BodyToLeft + BodyToRight;
                    ////determine avatar direction
                    //Vector3 AvatarToLeft = new Vector3(avatarLeftHand.transform.position.x, 0, avatarLeftHand.transform.position.z)
                    //    - new Vector3(avatarHead.transform.position.x, 0, avatarHead.transform.position.z);
                    //Vector3 AvatarToRight = new Vector3(avatarRightHand.transform.position.x, 0, avatarRightHand.transform.position.z)
                    //    - new Vector3(avatarHead.transform.position.x, 0, avatarHead.transform.position.z);
                    //Vector3 AvatarDirection = AvatarToLeft + AvatarToRight;

                    //Quaternion BodyDirectionQuat = Quaternion.LookRotation(BodyDirection, Vector3.up);
                    //Quaternion AvatarDirectionQuat = Quaternion.LookRotation(AvatarDirection, Vector3.up);
                    //float yDif = BodyDirectionQuat.eulerAngles.y - AvatarDirectionQuat.eulerAngles.y;

                    //Vector3 newArWingSceneCposition;
                    //Vector3 newArWingSceneCrotation;
                    //Vector3 newArWingSceneObjectPosition;

                    //newArWingSceneCposition = bodyHead.transform.position;  //Using wing scene container parent as rotation point
                    ////VR scene position and rotation hardcoded because it never moves in VR scene
                    //newArWingSceneObjectPosition = new Vector3(-4.06f, 0, -9.6f) - avatarHead.transform.position;     //Vector from VR HMD to WingSceneObject local position

                    //////collect data
                    //////calculate position off
                    ////Vector3 VrHeadPosition = avatarHead.transform.position;//just get difference between these two rather than use any tracker data? WAIT no I think these are set to the same position so no worky
                    ////Vector3 ArHeadPosition = bodyHead.transform.position;//just get difference between these two rather than use any tracker data?since this one get locked in
                    ////Vector3 ExpectedTrackerPosition = arTracker.transform.position;
                    ////Vector3 ArTrackerPosition = ARCamera.transform.position;
                    ////Vector3 BetweenVrHeadAndTracker = new Vector3(ExpectedTrackerPosition.x, 0, ExpectedTrackerPosition.z)
                    ////    - new Vector3(VrHeadPosition.x, 0, VrHeadPosition.z);//This is assuming same scales
                    ////Vector3 BetweenArHeadAndTracker = new Vector3(ArTrackerPosition.x, 0, ArTrackerPosition.z)
                    ////    - new Vector3(ArHeadPosition.x, 0, ArHeadPosition.z);
                    //////Vector3 TranslatedArTrackerPosition;//rotate then translate, but does the fix position off by rotation?
                    //////Vector3 BetweenExpectedAndActual = ;
                    //////MAYBE UNDO SCENE ROTATION to get off rotation??
                    //////Easy way, if I do full rotation and translation then its just gonna be off by the same amount unless I get the rotation off by values first? maybe place vr values overtop of ar values?
                    ////float difInDistance = Math.Abs(BetweenArHeadAndTracker.magnitude - BetweenVrHeadAndTracker.magnitude);
                    //////float trackingOffBy = BetweenExpectedAndActual.magnitude;
                    ////float trackingOffBy = difInDistance;
                    ////Debug.Log("Distance OFF BY: " + trackingOffBy);
                    //////use the correct scene rotation? I know VRHeadPosition = ARHeadPosition, I know translation vector from VRHeadPosition to ViveTracker, so apply that
                    //////translation to ARHeadPosition and see how far off the position is. Shouldnt rely on scene rotation
                    //////Or just one vector minus the other?
                    //////Will probably need to apply correct scene rotation
                    ////Vector3 ExpectedPosition = new Vector3(VrHeadPosition.x, 0, VrHeadPosition.z)
                    ////    + new Vector3(BetweenArHeadAndTracker.x, 0, BetweenArHeadAndTracker.z);
                    ////Vector3 ActualPosition = new Vector3(VrHeadPosition.x, 0, VrHeadPosition.z)
                    ////    + new Vector3(BetweenVrHeadAndTracker.x, 0, BetweenVrHeadAndTracker.z);
                    ////float positionOffBy = Vector3.Distance(ExpectedPosition, ActualPosition);
                    ////Debug.Log("Position off by: "+ positionOffBy);



                    //////calculate rotation off
                    ////float angleBetweenTrackerVectors = Vector3.Angle(BetweenVrHeadAndTracker, BetweenArHeadAndTracker);
                    ////Debug.Log("Angle of difference between head and ipad vectors: " + angleBetweenTrackerVectors);
                    ////Quaternion median = increasingQuats[increasingQuats.Count() / 2];
                    ////Quaternion trackerMedian = increasingTrackerQuats[increasingTrackerQuats.Count() / 2];
                    ////float rotationOffBy = Math.Abs(median.eulerAngles.y - trackerMedian.eulerAngles.y);
                    ////Debug.Log("Scene rotation off by: " + rotationOffBy);//SHOULD I DO SOMETHING SIMILAR FOR POSITION?

                    ////Store data
                    //StoredData sd = new StoredData();
                    ////sd.ArDistanceMag = BetweenArHeadAndTracker.magnitude;
                    ////sd.VrDistanceMag = BetweenVrHeadAndTracker.magnitude;
                    ////sd.DistanceOff = trackingOffBy;
                    ////sd.ARiPadPos = ExpectedPosition;
                    ////sd.VRiPadPos = ActualPosition;
                    ////sd.PositionOff = positionOffBy;
                    ////sd.ArSceneRot = median.eulerAngles.y;
                    ////sd.VrSceneRot = trackerMedian.eulerAngles.y;
                    //sd.RotationOff = RotationOffBy;
                    //myStoredDataList.Add(sd);
                }
                else
                {
                    //Debug.Log("DATA COLLECTED");
                }

                prevSceneVals.Add(WingSceneC.transform.rotation);

                //need this for movement with joystick
                ////POSITIONING
                //WingSceneC.transform.position = bodyHead.transform.position - (avatarHead.transform.localPosition + networkPlayer.transform.localPosition);
                

                ////WingSceneC.transform.position = bodyHead.transform.position - (avatarHead.transform.localPosition + networkPlayer.transform.localPosition);
                //Vector3 targetPosition = bodyHead.transform.position - (avatarHead.transform.localPosition + networkPlayer.transform.localPosition);//position is moving with head? position should lock in place in scene
                //Vector3 velocity = Vector3.zero;
                ////WingSceneC.transform.position = Vector3.SmoothDamp(WingSceneC.transform.position, targetPosition, ref velocity, 0.3f);  //testing out the smoothing

                ////Store data
                //StoredData sd = new StoredData();
                //sd.VRhead = avatarHead.transform.position;
                //sd.VRleft = avatarLeftHand.transform.position;
                //sd.VRright = avatarRightHand.transform.position;
                //sd.ARhead = bodyHead.transform.position;
                //sd.ARleft = bodyLeftHand.transform.position;
                //sd.ARright = bodyRightHand.transform.position;
                //sd.ARsceneRot = WingSceneC.transform.rotation.eulerAngles;
                //sd.tracker = arTracker.transform.position;
                //sd.iPadPos = ARCamera.transform.position;//Vector3.zero;//Replace this with actual ipad location or is ipad worldspace 0?
                //myStoredDataList.Add(sd);
            }
            //END OF ROTATION

            //POSITIONING
            //avatarLocalPosition + WingSceneGlobalPosition = bodyGlobalPosition, solve for wingscene global
            ///WingSceneC.transform.position = bodyHead.transform.position - (avatarHead.transform.localPosition + networkPlayer.transform.localPosition);
            
            /*Debug.Log("bodyHead Position: " + bodyHead.transform.position.ToString());
            Debug.Log("Avatar Position: " + avatarHead.transform.position.ToString());
            Debug.Log("WingSceneC Position: " + WingSceneC.transform.position.ToString());
            */
            //END OF POSITIONING


        }
    }

public void CalibratePressed()
    {
        print("TESTING TO SEE IF THE CALIBRATE EVENT WORKS(DISABLED)");

        //Vector3 VRHandPosition = CalibrationScript.getVRHandPosition();
        //float VRDistanceFromButton = CalibrationScript.getDistanceFromButton();
        //string whichHand = CalibrationScript.getLeftOrRightHand();

        //Vector3 HeadToButton = avatarHead.transform.InverseTransformPoint(CalibrationScript.gameObject.transform.position);
        ////Vector3 HeadToButton = CalibrationScript.gameObject.transform.position - avatarHead.transform.position;
        //Vector3 HeadToHand;
        //if(whichHand == "Left")
        //{
        //    //HeadToHand = avatarLeftHand.transform.position - avatarHead.transform.position;
        //    HeadToHand = avatarHead.transform.InverseTransformPoint(bodyLeftHand.transform.position);//body or avatar hand?
        //}
        //else
        //{
        //    //HeadToHand = avatarRightHand.transform.position - avatarHead.transform.position;
        //    HeadToHand = avatarHead.transform.InverseTransformPoint(bodyRightHand.transform.position);
        //}
        //HeadToHand.y = 0f;  //dont care about the y value since we are only rotating around the y axis, do i need to normalize as well?
        //HeadToButton.y = 0f;
        ////Quaternion fromHandToButton = Quaternion.FromToRotation(HeadToHand, HeadToButton);
        ////float fromHandToButton = Vector3.Angle(HeadToHand, HeadToButton);   //first time seems to work but second and third calibrations 
        ////are incorrect, needs to work everytime so I can get error over time
        ////Debug.DrawLine(Vector3.zero, HeadToButton, Color.white, 100f);
        ////Debug.DrawLine(Vector3.zero, HeadToHand, Color.cyan, 100f);
        ////float fromHandToButton = AngleBetweenVector2(new Vector2(HeadToHand.x,HeadToHand.z), new Vector2(HeadToButton.x, HeadToButton.z));
        //float fromHandToButton = Vector3.SignedAngle(HeadToHand, HeadToButton, Vector3.up);

        //print("Angle between vectors: " + fromHandToButton);//may need to zero y's still
        //Transform t = WingSceneC.transform;
        //print("Pre-rotation: " + t.rotation.eulerAngles.y);
        ///*if (fromHandToButton < 5f)
        //{
        //    print("DIDNT ROTATE");
        //}
        //else
        //{*/
        //t.RotateAround(avatarHead.transform.position, Vector3.up, 360-fromHandToButton);
        //WingSceneC.transform.rotation = t.rotation;
        //print("Post-rotation: " + t.rotation.eulerAngles.y);

        //}
        

        //manual version

        /*
        WingSceneC.transform.position = bodyHead.transform.position - (avatarHead.transform.localPosition + networkPlayer.transform.localPosition);

        //Debug.Log("Angle Slider: " + AngleSlider.value);
        Quaternion id = Quaternion.identity;
        Transform t = WingSceneC.transform;
        t.rotation = Quaternion.identity;
        t.RotateAround(avatarHead.transform.position, Vector3.up, ValueDeterminedbyCalibration);
        WingSceneC.transform.rotation = t.rotation;
        */
    }

    /*float AngleBetweenVector2(Vector2 vec1, Vector2 vec2)
    {
        Vector2 vec1Rotated90 = new Vector2(-vec1.y, vec1.x);
        float sign = (Vector2.Dot(vec1Rotated90, vec2) < 0) ? -1.0f : 1.0f;
        return Vector2.Angle(vec1, vec2) * sign;
    }*/
    

    public void WriteCSV()
    {
        Debug.Log("List size: " + myStoredDataList.Count);
        if(myStoredDataList.Count > 0)
        {
            FileStream fs = File.Open(csvname + fileNum + ".csv", FileMode.OpenOrCreate, FileAccess.Write);
            fileNum++;
            StreamWriter sw = new StreamWriter(fs);
            string header = "ArDistanceMag, VrDistanceMag, DistanceOff, " +
                "ArPosX, ArPosY, ArPosZ," +
                "VrPosX, VrPosY, VrPosZ," +
                "BodyPositionOff, ScenePositionOff," +
                "ArSceneRot, VrSceneRot, RotationOff";
            sw.WriteLine(header);
            Debug.Log("Headers created successfully");
                                                        //currently getting the initialization data which is not what I want
            foreach (StoredData sd in myStoredDataList)//i dont need to export all to csv, just the last value being used? no I need a bunch of values after initialization
            {
                sw.WriteLine(
                    sd.ArDistanceMag + "," +
                    sd.VrDistanceMag + "," +
                    sd.DistanceOff + "," +

                    sd.ARiPadPos.x + "," +
                    sd.ARiPadPos.y + "," +
                    sd.ARiPadPos.z + "," +

                    sd.VRiPadPos.x + "," +
                    sd.VRiPadPos.y + "," +
                    sd.VRiPadPos.z + "," +

                    sd.BodyPositionOff + "," +
                    sd.ScenePositionOff + "," +

                    sd.ArSceneRot + "," +
                    sd.VrSceneRot + "," +
                    sd.RotationOff
                    );
            }
            sw.Close();
            fs.Close();
            Debug.Log("csv path: " + csvname);
            //string csvstring = System.IO.File.ReadAllText(csvname);
            //Debug.Log("CSV STRING!!!!!!!!!!!");
            //Debug.Log(csvstring);
            //Debug.Log("String Shared Successfully");
        }
    }
}

