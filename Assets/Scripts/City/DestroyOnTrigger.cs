using UnityEngine;
using System.Collections;

public class DestroyOnTrigger : MonoBehaviour {

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("collider triggered: " + other.gameObject.tag);

        if (other.gameObject.tag.Equals("Destructor"))
        {
            GameObject.Destroy(this.gameObject);
        }
    }
}
