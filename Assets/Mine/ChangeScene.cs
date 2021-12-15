using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeScene : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}

    private void Update()
    {
        /*if (this.GetComponent<OVRGrabbable>().isGrabbed)
        {
            if(this.gameObject.name == "toWing Button")
            {
                SceneManager.LoadScene(2);
            }
            if (this.gameObject.name == "introWing Button")
            {
                SceneManager.LoadScene(1);
            }
            if (this.gameObject.name == "toTutorial Button")
            {
                SceneManager.LoadScene(3);
            }
            if (this.gameObject.name == "introTutorial Button")
            {
                SceneManager.LoadScene(4);
            }
            if (this.gameObject.name == "toMenu Button")
            {
                SceneManager.LoadScene(0);
            }

        }*/

    }

    /*void OnTriggerEnter(Collider myCollision){
        //Debug.Log("ter");
		if (myCollision.gameObject.name == "DoorwayMountain") {
			SceneManager.LoadScene (2);
            //Debug.Log("triggering");
		}
        if (myCollision.gameObject.name == "DoorwayValley")
        {
            SceneManager.LoadScene(1);
        }
        if (myCollision.gameObject.name == "DoorwayOasis")
        {
            SceneManager.LoadScene(3);
        }
        if (myCollision.gameObject.name == "DoorwayMenu")
        {
            SceneManager.LoadScene(0);
        }
    }
    */
}