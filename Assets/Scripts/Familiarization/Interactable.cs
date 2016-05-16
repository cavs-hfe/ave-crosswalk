using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using VRStandardAssets.Utils;

public class Interactable : MonoBehaviour
{

    public VRInteractiveItem vrInteractiveItem;

    public SelectionRadial reticleSelector;

    public Text text;

    [Tooltip("Determines if the object is interactable on start.")]
    public bool active_on_start = true;

    [SerializeField]
    [Tooltip("Determines if the object is interactable.")]
    private bool interactable = true;

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

        if (vrInteractiveItem)
        {
            vrInteractiveItem.OnOver += interactive_OnOver;
            vrInteractiveItem.OnOut += interactive_OnOut;
        }
        
    }

    void reticleSelector_OnSelectionComplete()
    {
        if (interactable)
        {
            activated = true;
            if (activate_interactable)
            {
                activate_interactable.setInteractable();
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

    public void setInteractable()
    {
        interactable = true;
        text.text = gui_display;
    }

    public bool isInteractable()
    {
        return interactable;
    }

    public bool isActivated()
    {
        return activated;
    }

}
