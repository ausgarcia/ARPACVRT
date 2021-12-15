using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class tvCount : MonoBehaviour//Photon.Pun.MonoBehaviourPun
{

    private int displayCount;
    public Text display;
    private PhotonView PV;

    // Use this for initialization
    void Start () {
        PV = GetComponent<PhotonView>();
        displayCount = 0;
        display.text = displayCount.ToString();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    
    public void incrementDisplay()
    {
        displayCount++;
        display.text = displayCount.ToString();
        PV.RPC("UpdateTvCount", RpcTarget.AllBuffered, displayCount);
        
    }

    [PunRPC]
    public void UpdateTvCount(int newVal)
    {
        displayCount = newVal;
        display.text = displayCount.ToString();
        //print("TV Count: " + displayCount);
    }
}
