using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class GlowScript : MonoBehaviour {
    //public GameObject stepAnim;
    public float stepNum;
    public GameObject glowAnim;
    int x;
    //private GameObject VRTK_SDK_Manager;
    private MainCounter MC;
    private GameObject XRrig;

	// Use this for initialization
	void Start () {
        x = 0;
        //VRTK_SDK_Manager = GameObject.Find("[VRTK_SDKManager]");
        MC = GameObject.Find("MainScripts").GetComponent<MainCounter>();
        XRrig = GameObject.Find("XR Rig");
    }
	
	// Update is called once per frame
	void Update () {
        if(XRrig != null)
        {
            if (
            MC.GetStep() == stepNum
            //stepAnim.activeInHierarchy 
            && !this.GetComponent<XRGrabInteractable>().isSelected && x > 80)    //Changed to VRTK
            {
                //Debug.Log("working");
                glowAnim.SetActive(true);

            }
            else if (
                MC.GetStep() == stepNum
                //stepAnim.activeInHierarchy 
                && !this.GetComponent<XRGrabInteractable>().isSelected) //Changed to VRTK
            {

            }
            else
            {
                //WaitForSecondsRealtime.Equals(1, 2);
                glowAnim.SetActive(false);
                x = 0;
            }
            x++;
        }
        else
        {
            if (
            MC.GetStep() == stepNum && x > 80)    //Removed to VRTK
            {
                //Debug.Log("working");
                glowAnim.SetActive(true);

            }
            else if (
                MC.GetStep() == stepNum) //Removed to VRTK
            {

            }
            else
            {
                //WaitForSecondsRealtime.Equals(1, 2);
                glowAnim.SetActive(false);
                x = 0;
            }
            x++;
        }
	}
}
