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

    public GameObject ARSessionOrigin;
    public GameObject ARCamera;

    public GameObject avatarHead;
    public GameObject avatarRightHand;
    public GameObject avatarLeftHand;
    public GameObject networkPlayer;
    public GameObject arTracker;

    //not sure if network player will be scaled
    public GameObject WingSceneC;    //WingSceneContainer
    public Toggle ManualToggle;
    public Slider AngleSlider;
    public Calibration CalibrationScript;
    //public float RefreshTime;
    private List<Quaternion> prevSceneVals = new List<Quaternion>();
    private int framesToInit = 120*10;    //at 120 fps lets max this at 10 sec for now
    private int framesToCollect;
    private Quaternion SceneRotAverage = Quaternion.identity;
    private List<Quaternion> increasingQuats = new List<Quaternion>();
    private List<Quaternion> increasingTrackerQuats = new List<Quaternion>();
    private List<Vector3> allPositions = new List<Vector3>();
    private Vector3 averagePos = Vector3.zero;
    //private int maxTris = 0;
    //private int maxVerts = 0;
    private int fileNum = 1;

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
        public float PositionOff;
    }

    private List<StoredData> myStoredDataList = new List<StoredData>();

    // Start is called before the first frame update
    void Start()
    {
        framesToCollect = framesToInit + 1000;
        ARSessionOrigin = GameObject.Find("AR Session Origin");
        ARCamera = ARSessionOrigin.transform.Find("AR Camera").gameObject;
        CalibrationScript.CalibratePressed.AddListener(CalibratePressed);
        csvname = Path.Combine(Application.persistentDataPath, "output");
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
        if (bodyParent != null && networkPlayer != null)
        {
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
            arTracker = GameObject.Find("arTracker");

            avatarHead.GetComponent<MeshRenderer>().enabled = false;
            avatarHead.transform.Find("LabelCanvas").gameObject.SetActive(false);
            avatarLeftHand.GetComponent<MeshRenderer>().enabled = false;
            avatarRightHand.GetComponent<MeshRenderer>().enabled = false;

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
                
                WingSceneC.transform.position = bodyHead.transform.position - (avatarHead.transform.localPosition + networkPlayer.transform.localPosition);
                
                //Debug.Log("Angle Slider: " + AngleSlider.value);
                Quaternion id = Quaternion.identity;
                Transform t = WingSceneC.transform;
                t.rotation = Quaternion.identity;
                t.RotateAround(avatarHead.transform.position, Vector3.up, AngleSlider.value);
                WingSceneC.transform.rotation = t.rotation;
                //Debug.Log("WingScene Y: " + WingSceneC.transform.rotation.y);
            }
            else   //Automatic rotation
            {
                csvWritten = false;
                //ME580 way
                if(prevSceneVals.Count() < framesToInit)   //Just store first ten seconds of rotation values
                {
                    Vector3 BodyToLeft = new Vector3(bodyLeftHand.transform.position.x, 0, bodyLeftHand.transform.position.z)
                        - new Vector3(bodyHead.transform.position.x, 0, bodyHead.transform.position.z);
                    Vector3 BodyToRight = new Vector3(bodyRightHand.transform.position.x, 0, bodyRightHand.transform.position.z)
                        - new Vector3(bodyHead.transform.position.x, 0, bodyHead.transform.position.z);
                    Vector3 BodyDirection = BodyToLeft + BodyToRight;

                    Vector3 AvatarToLeft = new Vector3(avatarLeftHand.transform.position.x, 0, avatarLeftHand.transform.position.z)
                        - new Vector3(avatarHead.transform.position.x, 0, avatarHead.transform.position.z);
                    Vector3 AvatarToRight = new Vector3(avatarRightHand.transform.position.x, 0, avatarRightHand.transform.position.z)
                        - new Vector3(avatarHead.transform.position.x, 0, avatarHead.transform.position.z);
                    Vector3 AvatarDirection = AvatarToLeft + AvatarToRight;
                    //BodyDirection *= Quaternion.Inverse(AvatarDirection);

                    Quaternion BodyDirectionQuat = Quaternion.LookRotation(BodyDirection, Vector3.up);
                    Quaternion AvatarDirectionQuat = Quaternion.LookRotation(AvatarDirection, Vector3.up);

                    float yDif = BodyDirectionQuat.eulerAngles.y - AvatarDirectionQuat.eulerAngles.y;

                    //for evaluation
                    Quaternion VrTrackerDirectionQuat = Quaternion.LookRotation(arTracker.transform.position - avatarHead.transform.position, Vector3.up);
                    Quaternion ArTrackerDirectionQuat = Quaternion.LookRotation(ARCamera.transform.position - bodyHead.transform.position, Vector3.up);
                    float trackerYdif = ArTrackerDirectionQuat.eulerAngles.y - VrTrackerDirectionQuat.eulerAngles.y;
                    Quaternion currentTrackerQuat = Quaternion.AngleAxis(trackerYdif, WingSceneC.transform.up) * WingSceneC.transform.rotation;

                    //finds median rotation
                    Quaternion currentQuat = Quaternion.AngleAxis(yDif, WingSceneC.transform.up) * WingSceneC.transform.rotation;
                    if (increasingQuats.Count == 0)
                    {
                        increasingQuats.Add(currentQuat);
                        increasingTrackerQuats.Add(currentTrackerQuat);
                        allPositions.Add(WingSceneC.transform.position);
                    }
                    else
                    {
                        for(int x = 0; x < increasingQuats.Count(); x++)
                        {
                            if (increasingQuats[x].eulerAngles.y >= currentQuat.eulerAngles.y)
                            {
                                increasingQuats.Insert(x, currentQuat);
                                break;
                            }
                        }
                        //for evaluation
                        for (int x = 0; x < increasingTrackerQuats.Count(); x++)
                        {
                            if (increasingTrackerQuats[x].eulerAngles.y >= currentTrackerQuat.eulerAngles.y)
                            {
                                increasingTrackerQuats.Insert(x, currentTrackerQuat);
                                break;
                            }
                        }

                        int median = increasingQuats.Count() / 2;
                        SceneRotAverage = increasingQuats[median];
                    }

                    WingSceneC.transform.rotation = SceneRotAverage;

                    //Debug.Log("WingScene New Y Rotation: " + WingSceneC.transform.rotation.y);
                    //Debug.Log("NEW Y: " + (yDif - AvatarLocalY));

                    //POSITIONING
                    //Average position
                    averagePos = Vector3.zero;
                    Vector3 currentPos = Vector3.zero;
                    foreach(Vector3 pos in allPositions)
                    {
                        currentPos += pos;
                    }
                    averagePos = currentPos/allPositions.Count();
                    //Sets position
                    WingSceneC.transform.position = averagePos;

                    //WingSceneC.transform.position = bodyHead.transform.position - (avatarHead.transform.localPosition + networkPlayer.transform.localPosition);
                    //Vector3 targetPosition = bodyHead.transform.position - (avatarHead.transform.localPosition + networkPlayer.transform.localPosition);//position is moving with head? position should lock in place in scene
                    //Vector3 velocity = Vector3.zero;
                    //WingSceneC.transform.position = Vector3.SmoothDamp(WingSceneC.transform.position, targetPosition, ref velocity, 0.3f);  //testing out the smoothing

                }
                else if (prevSceneVals.Count < framesToCollect)
                {
                    //do not change rotation of scene
                    if (prevSceneVals.Count == framesToInit)
                    {
                        Debug.Log("Initialization Completed");
                    }
                    //collect data
                    //calculate position off
                    Vector3 VrHeadPosition = avatarHead.transform.position;//just get difference between these two rather than use any tracker data? WAIT no I think these are set to the same position so no worky
                    Vector3 ArHeadPosition = bodyHead.transform.position;//just get difference between these two rather than use any tracker data?since this one get locked in
                    Vector3 ExpectedTrackerPosition = arTracker.transform.position;
                    Vector3 ArTrackerPosition = ARCamera.transform.position;
                    Vector3 BetweenVrHeadAndTracker = new Vector3(ExpectedTrackerPosition.x, 0, ExpectedTrackerPosition.z)
                        - new Vector3(VrHeadPosition.x, 0, VrHeadPosition.z);//This is assuming same scales
                    Vector3 BetweenArHeadAndTracker = new Vector3(ArTrackerPosition.x, 0, ArTrackerPosition.z)
                        - new Vector3(ArHeadPosition.x, 0, ArHeadPosition.z);
                    //Vector3 TranslatedArTrackerPosition;//rotate then translate, but does the fix position off by rotation?
                    //Vector3 BetweenExpectedAndActual = ;
                    //MAYBE UNDO SCENE ROTATION to get off rotation??
                    //Easy way, if I do full rotation and translation then its just gonna be off by the same amount unless I get the rotation off by values first? maybe place vr values overtop of ar values?
                    float difInDistance = Math.Abs(BetweenArHeadAndTracker.magnitude - BetweenVrHeadAndTracker.magnitude);
                    //float trackingOffBy = BetweenExpectedAndActual.magnitude;
                    float trackingOffBy = difInDistance;
                    Debug.Log("Distance OFF BY: " + trackingOffBy);
                    //use the correct scene rotation? I know VRHeadPosition = ARHeadPosition, I know translation vector from VRHeadPosition to ViveTracker, so apply that
                    //translation to ARHeadPosition and see how far off the position is. Shouldnt rely on scene rotation
                    //Or just one vector minus the other?
                    //Will probably need to apply correct scene rotation
                    Vector3 ExpectedPosition = new Vector3(VrHeadPosition.x, 0, VrHeadPosition.z)
                        + new Vector3(BetweenArHeadAndTracker.x, 0, BetweenArHeadAndTracker.z);
                    Vector3 ActualPosition = new Vector3(VrHeadPosition.x, 0, VrHeadPosition.z)
                        + new Vector3(BetweenVrHeadAndTracker.x, 0, BetweenVrHeadAndTracker.z);
                    float positionOffBy = Vector3.Distance(ExpectedPosition, ActualPosition);
                    Debug.Log("Position off by: "+ positionOffBy);



                    //calculate rotation off
                    float angleBetweenTrackerVectors = Vector3.Angle(BetweenVrHeadAndTracker, BetweenArHeadAndTracker);
                    Debug.Log("Angle of difference between head and ipad vectors: " + angleBetweenTrackerVectors);
                    Quaternion median = increasingQuats[increasingQuats.Count() / 2];
                    Quaternion trackerMedian = increasingTrackerQuats[increasingTrackerQuats.Count() / 2];
                    float rotationOffBy = Math.Abs(median.eulerAngles.y - trackerMedian.eulerAngles.y);
                    Debug.Log("Scene rotation off by: " + rotationOffBy);//SHOULD I DO SOMETHING SIMILAR FOR POSITION?

                    //Store data
                    StoredData sd = new StoredData();
                    sd.ArDistanceMag = BetweenArHeadAndTracker.magnitude;
                    sd.VrDistanceMag = BetweenVrHeadAndTracker.magnitude;
                    sd.DistanceOff = trackingOffBy;
                    sd.ARiPadPos = ExpectedPosition;
                    sd.VRiPadPos = ActualPosition;
                    sd.PositionOff = positionOffBy;
                    sd.ArSceneRot = median.eulerAngles.y;
                    sd.VrSceneRot = trackerMedian.eulerAngles.y;
                    sd.RotationOff = rotationOffBy;
                    myStoredDataList.Add(sd);
                }
                else
                {
                    Debug.Log("DATA COLLECTED");
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

    private float DistanceBetweenPoints(Vector3 p0, Vector3 p1)
    {
        float deltaX = p1.x - p0.x;
        float deltaY = p1.y - p0.y;
        float deltaZ = p1.z - p0.z;

        float distance = (float)System.Math.Sqrt(deltaX * deltaX + deltaY * deltaY + deltaZ * deltaZ);
        return distance;
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
                "PositionOff, " +
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

                    sd.PositionOff + "," +

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

