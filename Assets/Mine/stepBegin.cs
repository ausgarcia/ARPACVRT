using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class stepBegin : MonoBehaviour
{

    public GameObject thisIP;
    public GameObject nextAnim;
    private int x;

    // Use this for initialization
    void Start()
    {
        int x = 0;
    }

    // Update is called once per frame
    void Update()
    {

        if (thisIP.activeInHierarchy == true && x==0)// is the previous piece in place
        {

            nextAnim.SetActive(true);
            x++;
        }


    }
}