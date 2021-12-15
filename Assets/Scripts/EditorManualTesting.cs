using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EditorManualTesting : MonoBehaviour
{
    public GameObject avatarHead;
    public GameObject avatarRightHand;
    public GameObject avatarLeftHand;
    public GameObject networkPlayer;
    //not sure if network player will be scaled
    public GameObject WingSceneC;    //WingSceneContainer
    public Toggle ManualToggle;
    public Slider AngleSlider;

    // Start is called before the first frame update
    void Start()
    {

    }

    private void FixedUpdate()
    {
        //bodyParent = GameObject.Find("ControlledRobot(Clone)");
        networkPlayer = GameObject.Find("NetworkPlayer(Clone)");
        if (networkPlayer != null)
        {
            avatarHead = GameObject.Find("Avatar").gameObject;
            //Debug.Log("Avatar Head Position: " + avatarHead.transform.position.ToString());
            avatarLeftHand = GameObject.Find("AvatarLeftHand").gameObject;
            //Debug.Log("Avatar Right Hand Position: " + avatarLeftHand.transform.position.ToString());
            avatarRightHand = GameObject.Find("AvatarRightHand").gameObject;
            //Debug.Log("Avatar Left Hand Position: " + avatarRightHand.transform.position.ToString());

            //ROTATION
            //How good is the body tracking head rotation? probably need to get angle from all three points
            if (ManualToggle.isOn)  //Manual Scene Alignment
            {
                WingSceneC.transform.position = (avatarHead.transform.localPosition + networkPlayer.transform.localPosition);

                Debug.Log("Angle Slider: " + AngleSlider.value);
                Quaternion id = Quaternion.identity;
                //WingSceneC.transform.rotation = Quaternion.Euler(new Vector3(0,AngleSlider.value,0));
                Transform t = WingSceneC.transform;
                t.rotation = Quaternion.identity;
                t.RotateAround(avatarHead.transform.position, Vector3.up, AngleSlider.value);
                WingSceneC.transform.rotation = t.rotation;
                //WingSceneC.transform.position = t.position;
                


                //Quaternion q = Quaternion.AngleAxis(AngleSlider.value, avatarHead.transform.up) * WingSceneC.transform.rotation;
                //q = new Quaternion(0, q.y, 0, 0);
                //WingSceneC.transform.rotation = Quaternion.AngleAxis(AngleSlider.value, avatarHead.transform.up) * WingSceneC.transform.rotation;
                Debug.Log("WingScene Y: " + WingSceneC.transform.rotation.y);

                //POSITIONING
                
            }
            
            //END OF ROTATION

            //POSITIONING
            //avatarLocalPosition + WingSceneGlobalPosition = bodyGlobalPosition, solve for wingscene global
            ///WingSceneC.transform.position = bodyHead.transform.position - (avatarHead.transform.localPosition + networkPlayer.transform.localPosition);

            Debug.Log("Avatar Position: " + avatarHead.transform.position.ToString());
            Debug.Log("WingSceneC Position: " + WingSceneC.transform.position.ToString());

            //END OF POSITIONING


        }
    }
}