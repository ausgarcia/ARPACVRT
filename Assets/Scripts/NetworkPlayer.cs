using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
//using VRTK;

public class NetworkPlayer : Photon.Pun.MonoBehaviourPun
{
    public GameObject avatar;
    public GameObject avatarLeftHand;
    public GameObject avatarRightHand;
    //public GameObject arTracker;
    public TMPro.TextMeshProUGUI Label;
    public Transform playerGlobal;
    public Transform playerLocal;
    public Transform leftHand;
    public Transform rightHand;
    //public Transform tracker;
    public PhotonView PV;
    //public GameObject VRTK_SDK_Manager;
    private string labelText = "type";
    private GameObject SceneContainer;

    private void Start()
    {
        print("Im instantiated");
        PV = GetComponent<PhotonView>();
        //VRTK_SDK_Manager = GameObject.Find("[VRTK_SDKManager]");
        SceneContainer = GameObject.Find("WingSceneContainer");

        if (PV.IsMine)
        {

            labelText = "UnityXR";
            playerGlobal = GameObject.Find("XR Rig").transform;
            playerLocal = playerGlobal.transform.Find("Camera Offset").transform.Find("Main Camera");
            leftHand = playerGlobal.transform.Find("Camera Offset").transform.Find("LeftHand Controller");
            rightHand = playerGlobal.transform.Find("Camera Offset").transform.Find("RightHand Controller");
            //tracker = GameObject.Find("PuckCameraTracker").transform;
            //print("HEADSET TYPE " + VRTK_SDK_Manager.GetComponent<VRTK_SDKManager>().loadedSetup);
            /*switch (VRTK_SDK_Manager.GetComponent<VRTK_SDKManager>().loadedSetup.ToString())
            {
                case "Oculus (VRTK.VRTK_SDKSetup)":
                    labelText = "Oculus";
                    playerGlobal = GameObject.Find("OVRCameraRig").transform;
                    playerLocal = playerGlobal.transform.Find("TrackingSpace/CenterEyeAnchor");
                    leftHand = playerGlobal.transform.Find("TrackingSpace/LeftHandAnchor");
                    rightHand = playerGlobal.transform.Find("TrackingSpace/RightHandAnchor");
                    break;

                case "SteamVR (VRTK.VRTK_SDKSetup)":
                    labelText = "SteamVR";
                    playerGlobal = GameObject.Find("[CameraRig]").transform;
                    playerLocal = playerGlobal.transform.Find("Camera (eye)");
                    leftHand = playerGlobal.transform.Find("Controller (left)");
                    rightHand = playerGlobal.transform.Find("Controller (right)");
                    break;

                case "UnityXR (VRTK.VRTK_SDKSetup)":
                    labelText = "UnityXR";
                    playerGlobal = GameObject.Find("[UnityBase_CameraRig]").transform;
                    playerLocal = playerGlobal.transform.Find("Head");
                    leftHand = playerGlobal.transform.Find("LeftHandAnchor");
                    rightHand = playerGlobal.transform.Find("RightHandAnchor");
                    break;

                default:
                    print("HEADSET NOT RECOGNIZED");
                    labelText = "Unknown";
                    break;
            }*/

            //this.transform.SetParent(playerLocal);
            //this.transform.localPosition = Vector3.zero;

            //avatar.SetActive(false);
            Label.SetText(labelText);
            PV.RPC("UpdateLabel", RpcTarget.AllBuffered, labelText);
            Label.gameObject.SetActive(false);
            avatarLeftHand.gameObject.GetComponent<MeshRenderer>().enabled = false;
            avatarRightHand.gameObject.GetComponent<MeshRenderer>().enabled = false;
        }
        else        //if not mine
        {
            Label.SetText(labelText);

            if (SceneContainer != null)// This is reached/ is mobile version
            {
                //this.gameObject.transform.parent = SceneContainer.transform;
                this.gameObject.transform.localPosition = new Vector3(0, 0, 0);
                Debug.Log("New Player Joined NP");
            }
        }

    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        //THESE NEVER REACHED
        Debug.Log("SERIALIZING");
        /*if (stream.IsWriting)
        {
            Debug.Log("STREAM IS WRITING");
            stream.SendNext(playerGlobal.position);
            stream.SendNext(playerGlobal.rotation);
            stream.SendNext(playerLocal.localPosition);
            stream.SendNext(playerLocal.localRotation);
        }
        else
        {
            Debug.Log("STREAM IS READING");
            this.transform.position = (Vector3)stream.ReceiveNext();
            this.transform.rotation = (Quaternion)stream.ReceiveNext();
            avatar.transform.localPosition = (Vector3)stream.ReceiveNext();
            avatar.transform.localRotation = (Quaternion)stream.ReceiveNext();
        }
        */
    }

    private void Update()
    {
        if (PV.IsMine)
        {
            this.transform.position = playerGlobal.transform.position;
            this.transform.rotation = playerGlobal.transform.rotation;
            avatar.transform.position = playerLocal.transform.position;//was local
            avatar.transform.localRotation = playerLocal.transform.localRotation;
            avatarLeftHand.transform.position = leftHand.transform.position;
            avatarLeftHand.transform.rotation = leftHand.transform.rotation;
            avatarRightHand.transform.position = rightHand.transform.position;
            avatarRightHand.transform.rotation = rightHand.transform.rotation;
            //arTracker.transform.position = tracker.transform.position;
            //arTracker.transform.rotation = tracker.transform.rotation;
        }
        else//will not always be in update    
        {
            //PV.RPC("UpdateLabel", RpcTarget.All, labelText);
        }
    }

    [PunRPC]
    public void UpdateLabel(string newLabel)
    {
        labelText = newLabel;
        Label.SetText(newLabel);
    }

    //Add check for if the user is grabbing something?
}