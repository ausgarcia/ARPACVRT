using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;

public class MainCounter : Photon.Pun.MonoBehaviourPun
{
    private float step = 1;
    public float maxSteps = 60;
    public string endScene = "";
    private PhotonView PV;

    private void Start()
    {
        PV = GetComponent<PhotonView>();

    }

    private void Update()
    {
        
        //GameObject.Find("TVCanvas").GetComponent<TextEditor>().text = "4";
    }

    public float GetStep()
    {
        return step;
    }
    
    public void NextStep()
    {
        step++;
        PV.RPC("UpdateStep", RpcTarget.AllBuffered, step);
        
        if (step == maxSteps)
        {
            print("End of steps");
            SceneManager.LoadScene(endScene);
            //scene load not currently supported
        }
    }

    [PunRPC]
    public void UpdateStep(float stepVal)   //Used for photon step update
    {
        step = stepVal;
        //print("Server Step Updated: " + step);
    }
}
