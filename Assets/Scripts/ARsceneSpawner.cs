using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ARsceneSpawner : MonoBehaviour
{
    // Start is called before the first frame update
    public bool bodyTracker;
    public GameObject WingScene;
    public GameObject WingSceneContainer;
    UImanager uim;
    void Start()
    {
        Debug.Log("IMAGE TRACKING SCENE SPAWNER STARTED");
        WingSceneContainer = GameObject.Find("WingSceneContainer");
        UImanager uim = GameObject.Find("UI").GetComponent<UImanager>();
        //WingScene = Resources.FindObjectsOfTypeAll<GameObject>().
        if (!bodyTracker)       //used for image target
        {
            //Debug.Log("IMAGE TRACKING SCENE SPAWNER");
            
            if (WingSceneContainer != null)
            {
                WingScene = WingSceneContainer.transform.GetComponentsInChildren<Transform>(true)[1].gameObject;
                /*if (WingScene != null)
                {
                    Debug.Log("WingScene found");
                }
                else
                {
                    Debug.Log("WingScene not found");
                }*/
                WingSceneContainer.transform.parent = this.transform;
                WingScene.transform.localPosition = new Vector3(-4.03f, 0, -9.63f);
                WingSceneContainer.transform.localPosition = new Vector3(0, 0, 0);
                WingSceneContainer.transform.rotation = Quaternion.identity;
                WingScene.SetActive(true);
                /*Debug.Log("WING SCENE SET ACTIVE");
                Debug.Log("ARSCENE Position: " + this.gameObject.transform.position.ToString());
                Debug.Log("ARSCENE Rotation: " + this.gameObject.transform.rotation.ToString());
                Debug.Log("WingSceneC Position: " + WingSceneContainer.transform.position.ToString());
                Debug.Log("WingSceneC Rotation: " + WingSceneContainer.transform.rotation.ToString());
                Debug.Log("WingScene Active: " + WingScene.activeInHierarchy.ToString());
                */
            }
            else
            {
                //Debug.Log("WingSceneContainer not found");
            }
        }

    }
    private void FixedUpdate()
    {
        UImanager uim = GameObject.Find("UI").GetComponent<UImanager>();
        if (bodyTracker && uim.CurrentMode() == 1)    //Used for bodytracking
        {

            WingSceneContainer = GameObject.Find("WingSceneContainer");//this not found apparently
            if (WingSceneContainer != null)//this must return null once the image tracker is set
            {

                /*Debug.Log("Looking for Wing Scene");    //this not found after switch to image tracking
                //WingScene = WingSceneContainer.transform.GetComponentsInChildren<Transform>(true)[1].gameObject;//this should work but dont
                WingScene = WingSceneContainer.transform.Find("WingSceneObjects").gameObject;
                Debug.Log("Wing Scene found: " + WingScene.name);
                if(GameObject.Find("ControlledRobot(Clone)") == null)
                {
                    Debug.Log("CANT FIND ROBOT");
                }
                if(GameObject.Find("NetworkPlayer(Clone)") == null)
                {
                    Debug.Log("CANT FIND NETWORK PLAYER");
                }
                
                /*if (WingScene != null)
                {
                    Debug.Log("WingScene found for bodytracking");
                }
                else
                {
                    Debug.Log("WingScene not found for bodytracking");
                }*/

                if (GameObject.Find("ControlledRobot(Clone)") != null && GameObject.Find("NetworkPlayer(Clone)") != null)  //if a body is being tracked and there is a network user, turn the scene on
                {
                    //Debug.Log("Wing Scene Set Active!!!!");
                    //WingScene.SetActive(true);
                    WingScene.transform.position = new Vector3(-4.03f, 0, -9.63f);
                }
                else
                {
                    //WingScene.SetActive(false);
                    WingScene.transform.position = new Vector3(0, 1000, 0);
                    //Debug.Log("WING SCENE SET TO FALSE");
                }

            }
            else
            {
                //Debug.Log("WingSceneContainer not found for bodytracking");
            }

            /*Debug.Log("///////////////////");
            foreach (GameObject obj in Object.FindObjectsOfType(typeof(GameObject)))
            {
                Debug.Log(obj.name);
            }
            Debug.Log("///////////////////");*/
        }
        else
        {
            
            if (uim.CurrentMode() == 0)
            {
                //Debug.Log("IMAGE TRACKING SCENE SPAWNER");
                WingSceneContainer = GameObject.Find("WingSceneContainer");
                if (WingSceneContainer != null)
                {
                    //WingScene = WingSceneContainer.transform.GetComponentsInChildren<Transform>(true)[1].gameObject;
                    /*if (WingScene != null)
                    {
                        Debug.Log("WingScene found");
                    }
                    else
                    {
                        Debug.Log("WingScene not found");
                    }*/
                    WingSceneContainer.transform.parent = this.transform;
                    /*WingScene.transform.localPosition = new Vector3(-4.03f, 0, -9.63f);
                    WingSceneContainer.transform.localPosition = new Vector3(0, 0, 0);
                    WingSceneContainer.transform.rotation = Quaternion.identity;

                    WingScene.gameObject.SetActive(true);*/
                    /*Debug.Log("ARSCENE Position: " + this.gameObject.transform.position.ToString());
                    Debug.Log("ARSCENE Rotation: " + this.gameObject.transform.rotation.ToString());
                    Debug.Log("WingSceneC Position: " + WingSceneContainer.transform.position.ToString());
                    Debug.Log("WingSceneC Rotation: " + WingSceneContainer.transform.rotation.ToString());
                    Debug.Log("WingScene Active: " + WingScene.activeInHierarchy.ToString());
                    */
                }
                /*else
                {
                    Debug.Log("WingSceneContainer not found");
                }*/
            }
        }
    }
}
