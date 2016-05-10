using UnityEngine;
using System.Collections;
using VRStandardAssets.Utils;

public class StreetlightInteractiveItem : MonoBehaviour {

    [SerializeField]
    private VRInteractiveItem item;

    [SerializeField]
    private Material originalMaterial;

    [SerializeField]
    private Material highlightMaterial;

    private bool isHighlighted = false;
    private bool locked = false;

	// Use this for initialization
	void Start () {
        item.OnOver += item_OnOver;
	}

    void item_OnOver()
    {
        if (isHighlighted && !locked)
        {
            this.gameObject.GetComponent<Renderer>().material = originalMaterial;
            isHighlighted = false;
        }
    }

    public void highlightPole()
    {
        highlightPole(false);
    }

    public void highlightPole(bool lockState)
    {
        this.gameObject.GetComponent<Renderer>().material = highlightMaterial;
        isHighlighted = true;
        locked = lockState;
    }

    public void unlockState()
    {
        locked = false;
    }
}
