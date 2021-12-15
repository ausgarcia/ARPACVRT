using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnapToWing : MonoBehaviour {

    [SerializeField]
    protected Transform m_snapOffset;


    /// <summary>
    /// Notifies the object that it has been grabbed.
    /// </summary>
    /// 

    public Transform snapOffset
    {
        get { return m_snapOffset; }
    }

}
