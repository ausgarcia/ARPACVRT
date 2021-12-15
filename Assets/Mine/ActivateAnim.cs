using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivateAnim : MonoBehaviour {

    public float stepNum;
    private MainCounter MC;

	// Use this for initialization
	void Start () {
        MC = GameObject.Find("MainScripts").GetComponent<MainCounter>();
    }
	
	// Update is called once per frame
	void Update () {
        if (MC.GetStep() == stepNum)
        {
            this.GetComponent<MeshRenderer>().enabled = true;
        }
        else if(MC.GetStep() > stepNum)
        {
            Destroy(this.gameObject);
        }
    }
}
