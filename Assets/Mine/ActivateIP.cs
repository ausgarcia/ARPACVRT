using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivateIP : MonoBehaviour {

    public float stepNum;
    private MainCounter MC;

    // Use this for initialization
    void Start () {
		MC = GameObject.Find("MainScripts").GetComponent<MainCounter>();
    }
	
	// Update is called once per frame
	void Update () {

        if (MC.GetStep() > stepNum)      //should change this to trigger on an event
        {
            this.GetComponent<MeshRenderer>().enabled = true;
            if (this.GetComponent<MeshCollider>() == null)
            {
                this.GetComponent<BoxCollider>().enabled = true;
            }
            else
                this.GetComponent<MeshCollider>().enabled = true;
        }
        
	}
}
