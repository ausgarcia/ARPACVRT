﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using System.IO;    //for writing out files
using UnityNative.Sharing.Example;

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
    //private int maxTris = 0;
    //private int maxVerts = 0;
    private int fileNum = 1;

    public string csvname = "";
    private bool csvWritten = true;
    //dont need to store feet vals

    [System.Serializable]
    public class StoredData
    {
        public Vector3 ARhead;
        public Vector3 ARright;
        public Vector3 ARleft;
        public Vector3 VRhead;
        public Vector3 VRleft;
        public Vector3 VRright;
        //will need to get some sort of angle for calculation
        public Vector3 ARsceneRot;
        public Vector3 tracker;
        public Vector3 iPadPos;
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
                    Vector3 BodyToLeft = bodyLeftHand.transform.position - bodyHead.transform.position;
                    Vector3 BodyToRight = bodyRightHand.transform.position - bodyHead.transform.position;
                    Vector3 BodyDirection = BodyToLeft + BodyToRight;

                    Vector3 AvatarToLeft = avatarLeftHand.transform.position - avatarHead.transform.position;
                    Vector3 AvatarToRight = avatarRightHand.transform.position - avatarHead.transform.position;
                    Vector3 AvatarDirection = AvatarToLeft + AvatarToRight;
                    //BodyDirection *= Quaternion.Inverse(AvatarDirection);

                    Quaternion BodyDirectionQuat = Quaternion.LookRotation(BodyDirection, Vector3.up);
                    Quaternion AvatarDirectionQuat = Quaternion.LookRotation(AvatarDirection, Vector3.up);
                    //Quaternion SceneDirection = AvatarDirectionQuat * Quaternion.Inverse(BodyDirectionQuat);
                    //Debug.Log("Body Euler: " + BodyDirectionQuat.eulerAngles.ToString());
                    //Debug.Log("Avatar Euler: " + AvatarDirectionQuat.eulerAngles.ToString());

                    //Debug.Log("BodyDirection QUAT: " + BodyDirection.ToString());
                    //Debug.Log("AvatarDirection QUAT: " + AvatarDirection.ToString());
                    //Debug.Log("SceneDirection QUAT: " + SceneDirection.ToString());

                    float yDif = BodyDirectionQuat.eulerAngles.y - AvatarDirectionQuat.eulerAngles.y;
                    //float AvatarLocalY = avatarHead.transform.rotation.y*(180/Mathf.PI);//Assuming that NetworkPlayerClone rotation = 0
                    //Debug.Log("yDif: " + yDif);
                    //Debug.Log("Avatar Local Y: " + AvatarLocalY);

                    //SceneDirection.x = 0;
                    //SceneDirection.z = 0;
                    //WingSceneC.transform.rotation = new Quaternion(0f,yDif + AvatarLocalY , 0f, 0f);
                    //WingSceneC.transform.Rotate(new Vector3(0, yDif, 0));
                    //WingSceneC.transform.rotation = Quaternion.AngleAxis(yDif, WingSceneC.transform.up) * WingSceneC.transform.rotation;

                    //if (prevSceneVals.Count() > framesToStore)
                    //{
                    //    List<Quaternion> tmp = prevSceneVals.GetRange(1, prevSceneVals.Count());
                    //    prevSceneVals = tmp;
                    //    prevSceneVals.Add(WingSceneC.transform.rotation);
                    //    tmp.Clear();
                    //}
                    //else
                    //{
                    //    prevSceneVals.Add(Quaternion.AngleAxis(yDif, WingSceneC.transform.up) * WingSceneC.transform.rotation);
                    //}

                    //prevSceneVals.Add(Quaternion.AngleAxis(yDif, WingSceneC.transform.up) * WingSceneC.transform.rotation);

                    //adding work to find median rather than mean
                    Quaternion currentQuat = Quaternion.AngleAxis(yDif, WingSceneC.transform.up) * WingSceneC.transform.rotation;
                    if (increasingQuats.Count == 0)
                    {
                        increasingQuats.Add(currentQuat);
                        SceneRotAverage = increasingQuats[0];
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
                        int median = increasingQuats.Count() / 2;
                        SceneRotAverage = increasingQuats[median];
                    }
                    

                    //Finds mean
                    //prevSceneVals.Add(Quaternion.AngleAxis(yDif, WingSceneC.transform.up) * WingSceneC.transform.rotation);
                    //if (prevSceneVals.Count == 0)//should never be true though
                    //{
                    //    SceneRotAverage = Quaternion.identity;
                    //}
                    //else if (prevSceneVals.Count() == 1)
                    //{
                    //    SceneRotAverage = prevSceneVals[0];
                    //}
                    //else
                    //{
                    //    int count = prevSceneVals.Count();
                    //    float weight = 1.0f / (float)count;
                    //    SceneRotAverage = Quaternion.identity;

                    //    for (int i = 0; i < count; i++)//SLERP MAY NOT BE ACCURATE IF AVERAGING MORE THAN TWO QUATERNIONS, may need to redo this
                    //        SceneRotAverage *= Quaternion.Slerp(Quaternion.identity, prevSceneVals[i], weight); //gets average angles of last x frames
                    //}

                    WingSceneC.transform.rotation = SceneRotAverage;

                    //Debug.Log("WingScene New Y Rotation: " + WingSceneC.transform.rotation.y);
                    //Debug.Log("NEW Y: " + (yDif - AvatarLocalY));

                    //POSITIONING
                    WingSceneC.transform.position = bodyHead.transform.position - (avatarHead.transform.localPosition + networkPlayer.transform.localPosition);

                    //WingSceneC.transform.position = bodyHead.transform.position - (avatarHead.transform.localPosition + networkPlayer.transform.localPosition);
                    Vector3 targetPosition = bodyHead.transform.position - (avatarHead.transform.localPosition + networkPlayer.transform.localPosition);//position is moving with head? position should lock in place in scene
                    Vector3 velocity = Vector3.zero;
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
                    //Store data
                    StoredData sd = new StoredData();
                    sd.VRhead = avatarHead.transform.position;
                    sd.VRleft = avatarLeftHand.transform.position;
                    sd.VRright = avatarRightHand.transform.position;
                    sd.ARhead = bodyHead.transform.position;
                    sd.ARleft = bodyLeftHand.transform.position;
                    sd.ARright = bodyRightHand.transform.position;
                    sd.ARsceneRot = WingSceneC.transform.rotation.eulerAngles;
                    sd.tracker = arTracker.transform.position;
                    sd.iPadPos = ARCamera.transform.position;//Vector3.zero;//Replace this with actual ipad location or is ipad worldspace 0?
                    myStoredDataList.Add(sd);
                }
                else
                {
                    Debug.Log("DATA COLLECTED");
                }

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
            string header = "VRheadx, VRheady, VRheadz, " +
                "VRleftx, VRlefty, VRleftz," +
                "VRrightx, VRrighty, VRrightz," +
                "ARheadx, ARheady, ARheadz, " +
                "ARleftx, ARlefty, ARleftz," +
                "ARrightx, ARrighty, ARrightz," +
                "ARsceneRotx, ARsceneRoty, ARsceneRotz," +
                "trackerx, trackery, trackerz," +
                "ipadx, ipady, ipadz";
            sw.WriteLine(header);
            Debug.Log("Headers created successfully");
                                                        //currently getting the initialization data which is not what I want
            foreach (StoredData sd in myStoredDataList)//i dont need to export all to csv, just the last value being used? no I need a bunch of values after initialization
            {
                sw.WriteLine(
                    sd.VRhead.x + "," +
                    sd.VRhead.y + "," +
                    sd.VRhead.z + "," +

                    sd.VRleft.x + "," +
                    sd.VRleft.y + "," +
                    sd.VRleft.z + "," +

                    sd.VRright.x + "," +
                    sd.VRright.y + "," +
                    sd.VRright.z + "," +

                    sd.ARhead.x + "," +
                    sd.ARhead.y + "," +
                    sd.ARhead.z + "," +

                    sd.ARleft.x + "," +
                    sd.ARleft.y + "," +
                    sd.ARleft.z + "," +

                    sd.ARright.x + "," +
                    sd.ARright.y + "," +
                    sd.ARright.z + "," +

                    sd.ARsceneRot.x + "," +
                    sd.ARsceneRot.y + "," +
                    sd.ARsceneRot.z + "," +

                    sd.tracker.x + "," +
                    sd.tracker.y + "," +
                    sd.tracker.z + "," +

                    sd.iPadPos.x + "," +
                    sd.iPadPos.y + "," +
                    sd.iPadPos.z
                    );
            }
            sw.Close();
            fs.Close();
            Debug.Log("csv path: " + csvname);
            //string csvstring = System.IO.File.ReadAllText(csvname);
            //Debug.Log("CSV STRING!!!!!!!!!!!");
            //Debug.Log(csvstring);
            //UnityNativeSharingHelper.ShareText(csvstring);
            //Debug.Log("String Shared Successfully");
        }
    }
}

