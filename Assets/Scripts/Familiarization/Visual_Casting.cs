using UnityEngine;
using ProgressBar;
using System.Collections;

public class Visual_Casting : MonoBehaviour {
	Ray ray;
	RaycastHit hit;
	public Camera cam;
	public GameObject canvas;
	public float ray_distance = 2f; // NOTE TO JEFF: Added to allow the user to change the ray casting hit distance without altering the code. 




	// Use this for initialization
	void Start ()
	{
		// NOTE TO JEFF: This was added to prevent radial bar from displaying immediately on start.
		// For some reason, if the Ray hit distance was less than 50, the radial GUI was stuck on screen
		canvas.gameObject.SetActive(false);
	}

	// Update is called once per frame
	void Update ()
	{
		// **** NOTE FROM JOSH TO JEFF: This was added because if you were staring at 
		// the objective while backing away, even if you stepped outside the ray cast
		// hit area, the radial GUI wouldn't vanish. 
		if (!Physics.Raycast (ray, out hit, ray_distance))
		{
			canvas.gameObject.SetActive (false);
		}

	}

	void FixedUpdate ()
	{
		//ray = new Ray(cam.transform.position, cam.transform.forward);


        ray = new Ray(cam.transform.position, cam.transform.forward);
		//ray = (cam.ScreenPointToRay(Input.mousePosition));

		Debug.DrawRay(cam.transform.position, cam.transform.forward, Color.black);

		if (Physics.Raycast(ray, out hit, ray_distance)) {

			// Set the canvas and rotation
			canvas.transform.parent.position = ray.origin + (hit.distance * ray.direction.normalized * 0.5f);
			canvas.transform.rotation = Quaternion.LookRotation(ray.direction);


			// Detect if the game object has the "Interactable" Script
			if (hit.collider.gameObject.GetComponent<Interactable>() && hit.collider.gameObject.GetComponent<Interactable>().isInteractable())
			{
				// Increase its Timer
				hit.collider.gameObject.GetComponent<Interactable>().value += 1 / hit.collider.gameObject.GetComponent<Interactable>().time_to_complete  * 100 * Time.deltaTime;

				// Display Radial Progress Bar
				canvas.gameObject.SetActive(true);

				// Set Radial Progress Bar properties
				canvas.GetComponent<ProgressRadialBehaviour>().Value = hit.collider.gameObject.GetComponent<Interactable>().value;

			}
			else
			{
				canvas.gameObject.SetActive(false);
			}

		}
	}

}
