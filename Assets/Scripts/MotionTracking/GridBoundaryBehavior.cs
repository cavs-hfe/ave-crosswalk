using UnityEngine;
using System.Collections;

public class GridBoundaryBehavior : MonoBehaviour
{
    [SerializeField]
    private GameObject target = null;

    [SerializeField]
    private GameObject cube;

    [SerializeField]
    private Color gridColor;

    // Use this for initialization
    void Start()
    {
        cube = GameObject.FindGameObjectWithTag("BoxBoundary");

        if (cube != null)
        {
            gridColor = cube.GetComponent<MeshRenderer>().material.GetColor("_GridColour");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (target == null || cube == null)
        {
            return;
        }

        float distanceFromCenterToEdge = transform.lossyScale.magnitude / 3f;
        float str = (Vector3.Distance(transform.position, target.transform.position) / distanceFromCenterToEdge);
        cube.GetComponent<MeshRenderer>().material.SetColor("_GridColour", new Color(gridColor.r, gridColor.g, gridColor.b, str));
    }
}