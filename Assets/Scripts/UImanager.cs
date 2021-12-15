using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UImanager : MonoBehaviour
{
    public Button MenuButton;
    public GameObject MenuPanel;
    public Toggle ManualToggle;
    public GameObject AngleSlider;
    public TMPro.TMP_Dropdown TrackingTypeDD;
    public GameObject imageSessionOr;
    public GameObject bodySessionOr;
    public GameObject arSession;
    public GameObject bodyTrackingSceneAlignment;
    public GameObject TrackedBody;
    public GameObject WingSceneC;
    public GameObject WingScene;
    private GameObject skel;
    private GameObject skelParent;
    // Start is called before the first frame update
    void Start()
    {
        MenuButton.onClick.AddListener(ToggleMenu);
        ManualToggle.onValueChanged.AddListener(ToggleSlider);
        TrackingTypeDD.onValueChanged.AddListener(TrackingTypeSwitcher);
    }
    private void Update()
    {
        if(TrackingTypeDD.value == 0 && GameObject.Find("NetworkPlayer(Clone)") != null)
        {
            GameObject avatarHead = GameObject.Find("Avatar").gameObject;
            GameObject avatarLeftHand = GameObject.Find("AvatarLeftHand").gameObject;
            GameObject avatarRightHand = GameObject.Find("AvatarRightHand").gameObject;
            avatarHead.GetComponent<MeshRenderer>().enabled = true;
            avatarHead.transform.Find("LabelCanvas").gameObject.SetActive(true);
            avatarLeftHand.GetComponent<MeshRenderer>().enabled = true;
            avatarRightHand.GetComponent<MeshRenderer>().enabled = true;
        }
        foreach (GameObject obj in Object.FindObjectsOfType(typeof(GameObject)))//could add trackingtype == 1 here
        {
            if(obj.tag == "Skeleton")
            {
                if(obj.gameObject.transform.parent == null)
                {
                    //Debug.Log("SKELETON: " + obj.name + "  SKELETON PARENT: null");

                }
                else
                {
                    //Debug.Log("SKELETON: " + obj.name + "  SKELETON PARENT: " + obj.gameObject.transform.parent.name);
                    //GameObject parent = obj.gameObject.transform.parent.gameObject;
                    skel = obj;
                    skelParent = obj.gameObject.transform.parent.gameObject;
                    //GameObject.Destroy(obj);
                    //GameObject.Destroy(parent);
                }
                
            }
            //Debug.Log(obj.name);
        }
    }

    void TrackingTypeSwitcher(int DropdownVal)
    {
        Debug.Log("Tracking Type Switched");//should move all of this to set on startup as well so scene doesnt need to 
        if(DropdownVal == 0)    //imagetracking
        {
            //Debug.Log("SWITCHED TO IMAGE TRACKING");
            AngleSlider.gameObject.SetActive(false);
            ManualToggle.gameObject.SetActive(false);
            bodyTrackingSceneAlignment.SetActive(false);
            TrackedBody.SetActive(false);
            bodySessionOr.SetActive(false);
            imageSessionOr.SetActive(true);
            arSession.transform.localScale = new Vector3(2, 2, 2);

            WingScene.transform.localPosition = new Vector3(-4.03f, 0, -9.63f);
            WingSceneC.transform.localPosition = new Vector3(0, 0, 0);
            WingSceneC.transform.rotation = Quaternion.identity;

            WingScene.gameObject.SetActive(true);

            if(skel != null)
            {
                //Debug.Log("skel about to be destroyed");
                GameObject.Destroy(skel.gameObject);
            }
            else
            {
                //Debug.Log("skel is null");
            }
            if (skelParent != null)
            {
                //Debug.Log("skelParent about to be destroyed");
                GameObject.Destroy(skelParent.gameObject);
            }
            else
            {
                //Debug.Log("skelParent is null");
            }

            /*GameObject oldRobot = null;
            GameObject[] skeletons = GameObject.FindGameObjectsWithTag("Skeleton");
            if (skeletons.Length > 0)
            {
                Debug.Log("SKELETONS FOUND: "+skeletons.Length);
                oldRobot = skeletons[0];
            }
            else
            {
                Debug.Log("SKELETONS NOOOOT FOUND");
            }
            if (oldRobot != null)
            {
                Debug.Log("Delete AR HUMAN BODY");
                GameObject.Destroy(oldRobot.gameObject);
            }
            else
            {
                Debug.Log("COULDNT FIND OLD ROBOT");
            }*/

        }
        else if(DropdownVal == 1)   //bodytracking
        {
            WingSceneC.transform.parent = null;
            TrackedBody.SetActive(true);
            ManualToggle.gameObject.SetActive(true);
            if (ManualToggle.isOn)
            {
                AngleSlider.gameObject.SetActive(true);
            }
            imageSessionOr.SetActive(false);
            bodySessionOr.SetActive(true);
            bodyTrackingSceneAlignment.SetActive(true);

            arSession.transform.localScale = new Vector3(1, 1, 1);
        }
    }

    void ToggleMenu()
    {
        //Debug.Log("panel set to: " + !MenuPanel.activeInHierarchy);
        MenuPanel.SetActive(!MenuPanel.activeInHierarchy);
    }

    void ToggleSlider(bool b)
    {
        if (b)
        {
            AngleSlider.SetActive(true);
        }
        else
        {
            AngleSlider.SetActive(false);
        }
    }
    public int CurrentMode()//if 0 image tracking, if 1 bodytracking
    {
        return TrackingTypeDD.value;
    }
}
