using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisappearOnStep : MonoBehaviour {

    public float stepNum;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

        if (stepNum == GameObject.Find("MainScripts").GetComponent<MainCounter>().GetStep())
        {
            this.gameObject.SetActive(false);
        }


	}
}
