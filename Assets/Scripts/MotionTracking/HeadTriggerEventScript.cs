using UnityEngine;
using System;
using System.Collections;

public class HeadTriggerEventScript : MonoBehaviour {

    public event Action<string> OnHeadTriggered;

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("collider triggered: " + other.gameObject.tag);

        if (OnHeadTriggered != null)
        {
            OnHeadTriggered(other.gameObject.tag);
        }
    }
}
