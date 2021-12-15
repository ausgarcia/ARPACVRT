using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class addToDisplay : MonoBehaviour {

    private bool doOnce;

	// Use this for initialization
	void Start () {
        doOnce = false;
	}
	
	// Update is called once per frame
	void Update () {
        if (this.GetComponent<MeshRenderer>().enabled == true && doOnce==false)
        {
            GameObject.Find("MainScripts").GetComponent<tvCount>().incrementDisplay();
            doOnce = true;
        }
	}
}
