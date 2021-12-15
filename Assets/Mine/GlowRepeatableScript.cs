using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlowRepeatableScript : MonoBehaviour {

    public float[] stepList;

    private float stepProcedure; 
    // Use this for initialization
    void Start () {
        stepProcedure = GameObject.Find("MainScripts").GetComponent<MainCounter>().GetStep(); 
	}
	
	// Update is called once per frame
	void Update () {
		/*foreach (float stepNum in stepList)
        {
            Debug.Log("**** Glow Repeatable Loop ****:");
            Debug.Log("Loop Step Number: " + stepNum);
            Debug.Log("Script number: " + GameObject.Find("MainScripts").GetComponent<MainCounter>().Step);
            if (stepNum == GameObject.Find("MainScripts").GetComponent<MainCounter>().Step)
            {
                this.GetComponent<MeshRenderer>().enabled = true;
            }
            else
            {
                this.GetComponent<MeshRenderer>().enabled = false;
            }
        }
        */
        //for (unsigned int i=0; i<stepList.size(); ++i)
        //{

        //}
        float currentStep = GameObject.Find("MainScripts").GetComponent<MainCounter>().GetStep();
        if (stepProcedure != currentStep)
        {
            this.GetComponent<MeshRenderer>().enabled = false;
            foreach (float stepNum in stepList)
            {
                //Debug.Log("**** Glow Repeatable Loop ****:");
                //Debug.Log("Loop Step Number: " + stepNum);
                //Debug.Log("Script number: " + GameObject.Find("MainScripts").GetComponent<MainCounter>().Step);
                if (stepNum == currentStep)
                {
                    this.GetComponent<MeshRenderer>().enabled = true;
                }
            }
                
            stepProcedure = GameObject.Find("MainScripts").GetComponent<MainCounter>().GetStep();
        }
	}
}
