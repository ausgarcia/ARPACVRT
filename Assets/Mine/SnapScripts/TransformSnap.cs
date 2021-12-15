using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Should combine all the snap scripts for ease of use with some enums
public class TransformSnap : MonoBehaviour {

    public GameObject IPpiece;
    //public GameObject currentAnim;
    public float stepNum;
    public float rotateTolerance; // normal: 1f
    public float positionTolerance; // normal .1f
    private bool destroy;
    private MainCounter MC;
    private GameObject VRTK_SDK_Manager;
    private void Start()
    {
        MC = GameObject.Find("MainScripts").GetComponent<MainCounter>();
        VRTK_SDK_Manager = GameObject.Find("[VRTK_SDKManager]");
        destroy = false;
    }


    void Update() {

        if (destroy == true)
        {

            Destroy(gameObject);
        }

        if (VRTK_SDK_Manager != null &&  Mathf.Abs(transform.rotation.eulerAngles.magnitude - IPpiece.transform.rotation.eulerAngles.magnitude) < rotateTolerance &&   //1f
            Vector3.Distance(transform.position, IPpiece.transform.position) < positionTolerance   //.1f
            && MC.GetStep() == stepNum
            )
        {
            IPpiece.SetActive(true);
            //GetComponent<OVRGrabbable>().GrabEnd(;
            destroy = true;

            MC.NextStep();
            GameObject.Find("MainScripts").GetComponent<tvCount>().incrementDisplay();
        }
        if (MC.GetStep() > stepNum)//If other player increases step
        {
            destroy = true;
        }
	    


    }
}
