using CAVS.Recording;
using UnityEngine;
using System.Collections;

public class MoveForwardTitan : CAVS.Recording.PlaybackActorBehavior
{
    public float speed = 13.4f; //m/s or 30 mph

    public bool moving = false;
	
	// Update is called once per frame
	void FixedUpdate () {
        if (moving)
        {
            gameObject.transform.Translate(Vector3.right * speed * Time.deltaTime);
        }        
	}
}
