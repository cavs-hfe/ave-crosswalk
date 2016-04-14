using CAVS.Recording;
using UnityEngine;
using System.Collections;

public class MoveForwardTitan : CAVS.Recording.PlaybackActorBehavior
{
    public float speed = 13.4f; //m/s or 30 mph

    public float decelerationTime = 3.94f; //seconds
    private float currentDecelTime = 0;

    public bool moving = false;
    private bool decelerating = false;

    // Update is called once per frame
    void FixedUpdate()
    {
        if (decelerating)
        {
            float interpolatingFactor = currentDecelTime / decelerationTime;
            speed = Vector3.Slerp(new Vector3(speed, 0, 0), Vector3.zero, interpolatingFactor).x;
            currentDecelTime += Time.deltaTime;
        }
        if (moving)
        {
            gameObject.transform.Translate(Vector3.right * speed * Time.deltaTime);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag.Equals("Decelerate") && this.gameObject.tag.Equals("TrialVehicle"))
        {
            currentDecelTime = 0;
            decelerating = true;
        }
    }
}
