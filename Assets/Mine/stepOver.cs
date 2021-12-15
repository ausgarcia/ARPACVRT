using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class stepOver : MonoBehaviour {

    public GameObject ifActive;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

        if (ifActive.activeInHierarchy == true)
        {
            Destroy(this.gameObject);
        }

	}
}
