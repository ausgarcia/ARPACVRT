using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.Events;
using Photon.Pun;

//This script is not going to do the calculations, just sends the signal to PUN that the button was pressed and pass the data along to the bodyalignment AR script

public class Calibration : MonoBehaviour
{
    private bool isVRScene;
    private Vector3 VRHandPosition;
    private float VRDistanceFromButton;
    private string whichHand;
    private XRGrabInteractable thisInteractable;
    private PhotonView PV;
    public UnityEvent CalibratePressed;
    // Start is called before the first frame update
    void Start()
    {
        PV = GetComponent<PhotonView>();
        if (GameObject.Find("XR Rig") == null)
        {
            isVRScene = false;
        }
        else
        {
            isVRScene = true;
            thisInteractable = this.gameObject.GetComponent<XRGrabInteractable>();
            //Event thisIsSelected = new Event(ThisWasSelected);
            //thisInteractable.selectEntered
            thisInteractable.selectEntered.AddListener(ThisWasSelected);
        }
        if (CalibratePressed == null)
        {
            CalibratePressed = new UnityEvent();
        }
    }

    private void ThisWasSelected(SelectEnterEventArgs arg)
    {
        Transform handPosition = null;
        string leftOrRightHand = null;
        //print("CALIBRATION BUTTON was selected");
        if (thisInteractable.selectingInteractor.gameObject.name == "LeftHand Controller")//if left hand grabbing
        {
            handPosition = GameObject.Find("NetworkPlayer(Clone)").transform.GetChild(1).gameObject.transform;
            leftOrRightHand = "Left";
        }
        else //right hand is grabbing
        {
            handPosition = GameObject.Find("NetworkPlayer(Clone)").transform.GetChild(2).transform;
            leftOrRightHand = "Right";
        }

        float distanceFromButton = Vector3.Distance(handPosition.position, this.gameObject.transform.position);
        PV.RPC("CalibrationButtonPressed", RpcTarget.AllBuffered, handPosition.position, distanceFromButton, leftOrRightHand);//may beed to send over local position not global?
    }

    [PunRPC]
    public void CalibrationButtonPressed(Vector3 handPosition, float distanceFromButton, string leftOrRightHand)
    {
        print("CalibrationButtonPressed: " + leftOrRightHand + " - " + distanceFromButton);
        VRHandPosition = handPosition;
        VRDistanceFromButton = distanceFromButton;
        whichHand = leftOrRightHand;
        if (!isVRScene)
        {
            CalibratePressed.Invoke();
        }
    }
    public Vector3 getVRHandPosition()
    {
        return VRHandPosition;
    }
    public float getDistanceFromButton()
    {
        return VRDistanceFromButton;
    }
    public string getLeftOrRightHand()
    {
        return whichHand;
    }
}
