﻿using UnityEngine;
using System.Collections;

public class Door_Interactable : MonoBehaviour {
    
    public float smooth = 2.0f;
    public float door_open_angle = 90.0f;

    public GameObject check_activated;

    bool open = false;
    Vector3 default_rotation;
    Vector3 open_rotation;
    AudioSource doorAudioSource;
    bool audioPlayed = false;

	// Use this for initialization
	void Start () {
        default_rotation = transform.eulerAngles;
        open_rotation = new Vector3(default_rotation.x, default_rotation.y + door_open_angle, default_rotation.z);
        doorAudioSource = GetComponent<AudioSource>();
    }
	
	// Update is called once per frame
	void Update () {
        if (open)
        {
            if (!audioPlayed)
            {
                doorAudioSource.Play();
                audioPlayed = true;
            }
            transform.eulerAngles = Vector3.Slerp(transform.eulerAngles, open_rotation, Time.deltaTime * smooth);
        }
        else
        {
            transform.eulerAngles = Vector3.Slerp(transform.eulerAngles, default_rotation, Time.deltaTime * smooth);
        }

        if (check_activated.GetComponent<Interactable>().isActivated())
        {
            open = true;
        }
        else
        {
            open = false;
		}

    }


}
