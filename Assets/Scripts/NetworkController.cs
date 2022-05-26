using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class NetworkController : MonoBehaviourPunCallbacks
{
    string _room = "VrMultiplayerTest";
    private GameObject VRTK_SDK_Manager;
    private GameObject SceneContainer;

    // Start is called before the first frame update
    void Start()
    {
        VRTK_SDK_Manager = GameObject.Find("[VRTK_SDKManager]");
        PhotonNetwork.ConnectUsingSettings();//this is different from tutorial, parameter was ("0.1")
        SceneContainer = GameObject.Find("WingSceneContainer");
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("CONNECTED TO SERVER: " + PhotonNetwork.CloudRegion);
        //PhotonNetwork.JoinLobby();
        PhotonNetwork.AutomaticallySyncScene = true;
        RoomOptions roomOptions = new RoomOptions() { };
        PhotonNetwork.JoinOrCreateRoom(_room, roomOptions, TypedLobby.Default);

    }
    public override void OnJoinedRoom()
    {
        print("IN A ROOM");
        //Need an android version
        if (VRTK_SDK_Manager != null)
        {
            GameObject NetPlayer = PhotonNetwork.Instantiate("NetworkPlayer", Vector3.zero, Quaternion.identity, 0);
            if (SceneContainer != null)///scene container will always be null here since this is vr only portion of script
            {
                //NetPlayer.transform.parent = SceneContainer.transform;
                NetPlayer.transform.position = new Vector3(0, 0, 0);
                Debug.Log("New Player Joined NC");
            }
        }
        else
        {
            //spawn ar prefab
        }

    }
    public override void OnCreatedRoom()
    {
        Debug.Log("Created a room");
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.Log("FAILED TO JOIN A ROOM");
    }


    /*void OnJoinedLobby()
    {
        Debug.Log("JOINED LOBBY");

        RoomOptions roomOptions = new RoomOptions() { };
        PhotonNetwork.JoinOrCreateRoom(_room, roomOptions, TypedLobby.Default);
    }

    void OnJoinedRoom()
    {
        //PhotonNetwork.Instantiate("PlayerController", Vector3.zero, Quaternion.identity, 0);
    }*/
}