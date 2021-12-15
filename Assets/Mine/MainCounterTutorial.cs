using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainCounterTutorial : MonoBehaviour {

    public float Step = 1;

    private void Update()
    {
        if (Step == 9)
        {
            Debug.Log("is step 9");
            SceneManager.LoadScene(3);
        }
    }
}
