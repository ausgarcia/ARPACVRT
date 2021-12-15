using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BodyTrackingSceneAlignment : MonoBehaviour
{

    public GameObject bodyHead;
    public GameObject bodyRightHand;
    public GameObject bodyLeftHand;
    public GameObject bodyRightFoot;
    public GameObject bodyLeftFoot;

    public GameObject bodyParent;   //there may be none so this is probably going to be 1

    public GameObject avatarHead;
    public GameObject avatarRightHand;
    public GameObject avatarLeftHand;
    public GameObject networkPlayer;
    //not sure if network player will be scaled
    public GameObject WingSceneC;    //WingSceneContainer
    public Toggle ManualToggle;
    public Slider AngleSlider;
    public Calibration CalibrationScript;
    //public float RefreshTime;

    // Start is called before the first frame update
    void Start()
    {
        CalibrationScript.CalibratePressed.AddListener(CalibratePressed);
    }

    private void FixedUpdate()
    {
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

            avatarHead.GetComponent<MeshRenderer>().enabled = false;
            avatarHead.transform.Find("LabelCanvas").gameObject.SetActive(false);
            avatarLeftHand.GetComponent<MeshRenderer>().enabled = false;
            avatarRightHand.GetComponent<MeshRenderer>().enabled = false;

            //ROTATION
            //How good is the body tracking head rotation? probably need to get angle from all three points
            if (ManualToggle.isOn)  //Manual Scene Alignment
            {
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
                //ME580 way
                /*
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
                WingSceneC.transform.rotation = Quaternion.AngleAxis(yDif, WingSceneC.transform.up) * WingSceneC.transform.rotation;
                //Debug.Log("WingScene New Y Rotation: " + WingSceneC.transform.rotation.y);
                //Debug.Log("NEW Y: " + (yDif - AvatarLocalY));

                //POSITIONING
                WingSceneC.transform.position = bodyHead.transform.position - (avatarHead.transform.localPosition + networkPlayer.transform.localPosition);
                */
                WingSceneC.transform.position = bodyHead.transform.position - (avatarHead.transform.localPosition + networkPlayer.transform.localPosition);
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
        print("TESTING TO SEE IF THE CALIBRATE EVENT WORKS");

        Vector3 VRHandPosition = CalibrationScript.getVRHandPosition();
        float VRDistanceFromButton = CalibrationScript.getDistanceFromButton();
        string whichHand = CalibrationScript.getLeftOrRightHand();

        Vector3 HeadToButton = avatarHead.transform.InverseTransformPoint(CalibrationScript.gameObject.transform.position);
        //Vector3 HeadToButton = CalibrationScript.gameObject.transform.position - avatarHead.transform.position;
        Vector3 HeadToHand;
        if(whichHand == "Left")
        {
            //HeadToHand = avatarLeftHand.transform.position - avatarHead.transform.position;
            HeadToHand = avatarHead.transform.InverseTransformPoint(bodyLeftHand.transform.position);//body or avatar hand?
        }
        else
        {
            //HeadToHand = avatarRightHand.transform.position - avatarHead.transform.position;
            HeadToHand = avatarHead.transform.InverseTransformPoint(bodyRightHand.transform.position);
        }
        HeadToHand.y = 0f;  //dont care about the y value since we are only rotating around the y axis, do i need to normalize as well?
        HeadToButton.y = 0f;
        //Quaternion fromHandToButton = Quaternion.FromToRotation(HeadToHand, HeadToButton);
        //float fromHandToButton = Vector3.Angle(HeadToHand, HeadToButton);   //first time seems to work but second and third calibrations 
        //are incorrect, needs to work everytime so I can get error over time
        //Debug.DrawLine(Vector3.zero, HeadToButton, Color.white, 100f);
        //Debug.DrawLine(Vector3.zero, HeadToHand, Color.cyan, 100f);
        //float fromHandToButton = AngleBetweenVector2(new Vector2(HeadToHand.x,HeadToHand.z), new Vector2(HeadToButton.x, HeadToButton.z));
        float fromHandToButton = Vector3.SignedAngle(HeadToHand, HeadToButton, Vector3.up);

        print("Angle between vectors: " + fromHandToButton);//may need to zero y's still
        Transform t = WingSceneC.transform;
        print("Pre-rotation: " + t.rotation.eulerAngles.y);
        /*if (fromHandToButton < 5f)
        {
            print("DIDNT ROTATE");
        }
        else
        {*/
        t.RotateAround(avatarHead.transform.position, Vector3.up, 360-fromHandToButton);
        WingSceneC.transform.rotation = t.rotation;
        print("Post-rotation: " + t.rotation.eulerAngles.y);
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
}
