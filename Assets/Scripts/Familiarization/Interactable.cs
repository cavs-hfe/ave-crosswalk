using UnityEngine;
using System.Collections;
using VRStandardAssets.Utils;

public class Interactable : MonoBehaviour
{

    public VRInteractiveItem vrInteractiveItem;

    public SelectionRadial reticleSelector;

    [Tooltip("Determines if the object is interactable on start.")]
    public bool active_on_start = true;

    [Tooltip("Determines if the object is interactable.")]
    public bool interactable = true;

    [Tooltip("Determines how much time is required to complete its task.")]
    public float time_to_complete = 3;

    [Tooltip("Decrease when not incrementing.")]
    public bool decrement_time = true;

    [Tooltip("When complete, reset back to 0.")]
    public bool reset_on_complete = false;

    [Tooltip("Hide when completed (Deactivated if reset_on_complete).")]
    public bool hide_on_complete = false;

    [Tooltip("Activates an interactable object (buggy without hide_on_complete when target has hide_on_complete)")]
    public Interactable activate_interactable;

    [Tooltip("When the player looks down, this text will be displayed.")]
    public string gui_display;

    [Tooltip("Starting value of timer (Not typically used).")]
    public float value;

    private float time_temp;        // Used for decrement_time.

    [SerializeField]
    private bool activated;         // Determines if the object has been activated.
    private bool cursorOver = false;

    // Use this for initialization
    void Start()
    {
        if (!active_on_start) { interactable = false; }
        if (reset_on_complete) { hide_on_complete = false; }

        vrInteractiveItem.OnOver += interactive_OnOver;
        vrInteractiveItem.OnOut += interactive_OnOut;

        reticleSelector.OnSelectionComplete += reticleSelector_OnSelectionComplete;
    }

    void reticleSelector_OnSelectionComplete()
    {
        if (interactable)
        {
            activated = true;
            if (activate_interactable)
            {
                activate_interactable.interactable = true;
            }
            if (hide_on_complete)
            {
                interactable = false;
            }
        }
    }

    void interactive_OnOut()
    {
        if (interactable && cursorOver)
        {
            cursorOver = false;
        }
        reticleSelector.OnSelectionComplete -= reticleSelector_OnSelectionComplete;
        reticleSelector.StopFill();
        reticleSelector.Hide();
    }

    void interactive_OnOver()
    {
        if (interactable)
        {
            cursorOver = true;
            reticleSelector.OnSelectionComplete += reticleSelector_OnSelectionComplete;
            reticleSelector.Show();
            reticleSelector.StartFill();
        }
    }

    // Update is called once per frame
    void Update()
    {
        /*if (!activated && cursorOver && value < 100)
        {
            value += 1 / time_to_complete * 100 * Time.deltaTime;
            if (value >= 100)
            {
                value = 100;
                activated = true;
                reticleSelector.StopFill();
                reticleSelector.Hide(); 
                if (activate_interactable)
                {
                    activate_interactable.interactable = true;
                }
                if (reset_on_complete)
                {
                    value = 0;
                    if (decrement_time) { time_temp = 0; }
                    activated = !activated;
                }
                if (hide_on_complete)
                {
                    value = 0;
                    interactable = false;
                    if (decrement_time) { time_temp = 0; }
                    //gameObject.SetActive(false);
                }
            }
        } else if (!cursorOver && decrement_time && value < 100)
        {
            if (value > time_temp)
            {
                time_temp = value;
            }
            else if (value <= time_temp && value > 0)
            {
                value -= 1 / time_to_complete * 100 * Time.deltaTime;
                if (value < 0) { value = 0; }
                time_temp = value;
            }
        }*/
    }

    public bool isActivated()
    {
        return activated;
    }

    /*void OnGUI()
    {

        int width = 300;
        int height = 200;
        if (Camera.current.transform.eulerAngles.x >= 60 && Camera.current.transform.eulerAngles.x <= 90)
        {
            Debug.Log(Camera.current.transform.parent.name);
            GUI.skin.label.alignment = TextAnchor.MiddleCenter;
            GUI.skin.label.fontSize = 24;
            GUI.Label(new Rect(Screen.width / 2 - (width / 2), Screen.height / 2 - (height / 2), width, height), gui_display);
        }
    }*/
}
