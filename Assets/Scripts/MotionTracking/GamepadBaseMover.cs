using UnityEngine;
using System.Collections;

public class GamepadBaseMover : MonoBehaviour {

	public float scaling = 0.1f;
	public Camera referenceCamera;
	public bool useMouse = true;

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
		float h = Input.GetAxis ("Horizontal");
		float v = Input.GetAxis ("Vertical");

		Vector3 projectedForward = Vector3.ProjectOnPlane (referenceCamera.transform.forward, Vector3.up);
		Vector3 projectedRight = Vector3.ProjectOnPlane (referenceCamera.transform.right, Vector3.up);

		gameObject.transform.Translate (projectedForward * (v * scaling));
		gameObject.transform.Translate (projectedRight * (h * scaling));

		//if we are using the mouse and the left mouse button is held down
		if (useMouse && Input.GetKey(KeyCode.Mouse0)) {
			float x = Input.GetAxis("Mouse X");
			float y = Input.GetAxis("Mouse Y");

			gameObject.transform.Translate (projectedForward * (y * scaling));
			gameObject.transform.Translate (projectedRight * (x * scaling));
		}

	}
}
