using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WingnutSnap : MonoBehaviour {

    public float stepNum;
    private bool filled;
    private MainCounter MC;
    private GameObject VRTK_SDK_Manager;
    //private bool destroy;

	// Use this for initialization
	void Start () {
        MC = GameObject.Find("MainScripts").GetComponent<MainCounter>();
        VRTK_SDK_Manager = GameObject.Find("[VRTK_SDKManager]");
        filled = false;
        //destroy = false;
	}
	
	// Update is called once per frame
	void Update () {
        if (MC.GetStep() > stepNum) //If other player increases step
        {
            //currently wont destroy the other gameobject so a piece may get dragged into place by User1 but User 2 sees the In Place piece and an extra piece
            filled = true;
            this.GetComponent<MeshRenderer>().enabled = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(VRTK_SDK_Manager != null)
        {
            if (other.gameObject.tag == "wingnut"
            && !filled
            && MC.GetStep() == stepNum)
            {
                //other.GetComponent<OVRGrabbable>().grabbedBy.GetComponent<OVRGrabber>().ForceRelease(other.GetComponent<OVRGrabbable>());
                //other.GetComponent<VRTK.VRTK_InteractableObject>().GetGrabbingObject().GetComponent<VRTK.VRTK_InteractGrab>().ForceRelease();
                other.GetComponent<MeshRenderer>().enabled = false;
                other.GetComponent<BoxCollider>().enabled = false;
                Destroy(other.gameObject, 10f);
                this.GetComponent<MeshRenderer>().enabled = true;
                filled = true;

                MC.NextStep();
            }
        }
        
    }

}
