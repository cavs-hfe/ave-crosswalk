using UnityEngine;
using System.Collections;
using System;

public class FOVScript : MonoBehaviour
{

    private GameObject fovRight;
    private GameObject fovLeft;
    private GameObject fovBack;
    private GameObject fovFront;

    private GameObject cube;

    public Material fovMaterial;

    public float width;
    public float depth;

    public bool useCube;

    public GameObject target;

    [SerializeField]
    private Color gridColor;

    private bool setupRectFov = false;
    private bool setupKinectFov = false;

    // Use this for initialization
    void Start()
    {
        fovRight = GameObject.CreatePrimitive(PrimitiveType.Cube);
        fovLeft = GameObject.CreatePrimitive(PrimitiveType.Cube);
        fovBack = GameObject.CreatePrimitive(PrimitiveType.Cube);
        fovFront = GameObject.CreatePrimitive(PrimitiveType.Cube);

        fovRight.name = "FovRight";
        fovLeft.name = "FovLeft";
        fovBack.name = "FovBack";
        fovFront.name = "FovFront";

        fovRight.transform.parent = this.transform;
        fovLeft.transform.parent = this.transform;
        fovBack.transform.parent = this.transform;
        fovFront.transform.parent = this.transform;

        Renderer rRend = fovRight.GetComponent<Renderer>();
        rRend.material = fovMaterial;
        Renderer lRend = fovLeft.GetComponent<Renderer>();
        lRend.material = fovMaterial;
        Renderer bRend = fovBack.GetComponent<Renderer>();
        bRend.material = fovMaterial;
        Renderer fRend = fovFront.GetComponent<Renderer>();
        fRend.material = fovMaterial;

        BoxCollider rColl = fovRight.GetComponent<BoxCollider>();
        rColl.enabled = false;
        BoxCollider lColl = fovLeft.GetComponent<BoxCollider>();
        lColl.enabled = false;
        BoxCollider bColl = fovBack.GetComponent<BoxCollider>();
        bColl.enabled = false;
        BoxCollider fColl = fovFront.GetComponent<BoxCollider>();
        fColl.enabled = false;

        cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.transform.parent = this.transform;
        Renderer cRend = cube.GetComponent<Renderer>();
        cRend.material = fovMaterial;
        BoxCollider cColl = cube.GetComponent<BoxCollider>();
        cColl.enabled = false;
        cube.tag = "BoxBoundary";
        cube.name = "BoxBoundary";

        gridColor = fovMaterial.GetColor("_GridColour");

        disableFOV();
    }

    // Update is called once per frame
    void Update()
    {
        if (setupKinectFov)
        {
            useKinectFov();
            setupKinectFov = false;
        }
        else if (setupRectFov)
        {
            useRectFov();
            setupRectFov = false;
        }

        if (cube.activeSelf)
        {
            cube.GetComponent<MeshRenderer>().material.SetColor("_GridColour", new Color(gridColor.r, gridColor.g, gridColor.b, getDistanceAlpha()));
        }

        if (fovRight.activeSelf)
        {
            fovRight.GetComponent<MeshRenderer>().material.SetColor("_GridColour", new Color(gridColor.r, gridColor.g, gridColor.b, getDistanceAlpha(fovRight, false)));
            fovLeft.GetComponent<MeshRenderer>().material.SetColor("_GridColour", new Color(gridColor.r, gridColor.g, gridColor.b, getDistanceAlpha(fovLeft, false)));
            fovBack.GetComponent<MeshRenderer>().material.SetColor("_GridColour", new Color(gridColor.r, gridColor.g, gridColor.b, getDistanceAlpha(fovBack, true)));
            fovFront.GetComponent<MeshRenderer>().material.SetColor("_GridColour", new Color(gridColor.r, gridColor.g, gridColor.b, getDistanceAlpha(fovFront, true)));
        }

    }

    public void enableKinectFov()
    {
        setupKinectFov = true;
    }

    private void useKinectFov()
    {
        fovRight.transform.localPosition = new Vector3(1.58f, -1.3f, 2.75f);
        Quaternion rRot = Quaternion.identity;
        rRot.eulerAngles = new Vector3(0f, 35f, 0f);
        fovRight.transform.localRotation = rRot;
        fovRight.transform.localScale = new Vector3(0.01f, 1f, 4.27f);

        fovLeft.transform.localPosition = new Vector3(-1.58f, -1.3f, 2.75f);
        Quaternion lRot = Quaternion.identity;
        lRot.eulerAngles = new Vector3(0f, 325f, 0f);
        fovLeft.transform.localRotation = lRot;
        fovLeft.transform.localScale = new Vector3(0.01f, 1f, 4.27f);

        fovBack.transform.localPosition = new Vector3(0, -1.3f, 4.5f);
        Quaternion bRot = Quaternion.identity;
        bRot.eulerAngles = new Vector3(0f, 90f, 0f);
        fovBack.transform.localRotation = bRot;
        fovBack.transform.localScale = new Vector3(0.01f, 1f, 5.6f);

        fovFront.transform.localPosition = new Vector3(0f, -1.3f, 1f);
        Quaternion fRot = Quaternion.identity;
        fRot.eulerAngles = new Vector3(0f, 90f, 0f);
        fovFront.transform.localRotation = fRot;
        fovFront.transform.localScale = new Vector3(0.01f, 1f, 0.7f);

        enableFOV();
    }

    public void enableRectFov()
    {
        setupRectFov = true;
    }

    private void useRectFov()
    {
        useRectFov(width, depth);
    }

    private void useRectFov(float width, float depth)
    {
        if (useCube)
        {
            cube.transform.localPosition = Vector3.zero;
            cube.transform.localScale = new Vector3(width, 2.8f, depth);
            cube.transform.localRotation = Quaternion.identity;
        }
        else
        {
            fovRight.transform.localPosition = new Vector3(width / 2, -1.3f, 0f);
            Quaternion rRot = Quaternion.identity;
            rRot.eulerAngles = new Vector3(0f, 180f, 0f);
            fovRight.transform.localRotation = rRot;
            fovRight.transform.localScale = new Vector3(0.01f, 5f, depth);

            fovLeft.transform.localPosition = new Vector3(-width / 2, -1.3f, 0f);
            Quaternion lRot = Quaternion.identity;
            lRot.eulerAngles = new Vector3(0f, 0f, 0f);
            fovLeft.transform.localRotation = lRot;
            fovLeft.transform.localScale = new Vector3(0.01f, 5f, depth);

            fovBack.transform.localPosition = new Vector3(0, -1.3f, -depth / 2);
            Quaternion bRot = Quaternion.identity;
            bRot.eulerAngles = new Vector3(0f, 270f, 0f);
            fovBack.transform.localRotation = bRot;
            fovBack.transform.localScale = new Vector3(0.01f, 5f, width);

            fovFront.transform.localPosition = new Vector3(0f, -1.3f, depth / 2);
            Quaternion fRot = Quaternion.identity;
            fRot.eulerAngles = new Vector3(0f, 90f, 0f);
            fovFront.transform.localRotation = fRot;
            fovFront.transform.localScale = new Vector3(0.01f, 5f, width);
        }

        enableFOV();
    }

    private void disableFOV()
    {
        fovRight.SetActive(false);
        fovLeft.SetActive(false);
        fovBack.SetActive(false);
        fovFront.SetActive(false);

        cube.SetActive(false);
    }

    private void enableFOV()
    {
        if (useCube && !setupKinectFov)
        {
            cube.SetActive(true);
        }
        else
        {
            fovRight.SetActive(true);
            fovLeft.SetActive(true);
            fovBack.SetActive(true);
            fovFront.SetActive(true);
        }

    }

    private float getDistanceAlpha()
    {
        float alpha = 1.0f;
        if (target != null)
        {
            float xDist = Mathf.Abs(target.transform.position.x - cube.transform.position.x);
            float zDist = Mathf.Abs(target.transform.position.z - cube.transform.position.z);

            alpha = Mathf.Max(xDist / (width / 2), zDist / (depth / 2));

        }
        return alpha;
    }

    private float getDistanceAlpha(GameObject wall, bool frontBack)
    {
        float alpha = 1.0f;
        if (target != null)
        {
            //float distance = Vector3.Distance(target.transform.position, wall.transform.position);

            Plane p = new Plane(wall.transform.right, wall.transform.position);
            float distance = p.GetDistanceToPoint(target.transform.position);

            //((log(x)/log(0.2))+2)/8
            alpha = Mathf.Clamp01(((Mathf.Log(distance) / Mathf.Log(0.2f)) + 1) / 8);

            /*if (distance >= 1.0f)
            {
                alpha = 0.01f;
            }
            else
            {
                alpha = 1 / distance;
            }*/

            //(max_distance – distance) / (max_distance – min_distance)
            /*if (frontBack)
            {
                alpha = (depth - distance) / depth;
            }
            else
            {
                alpha = (width - distance) / width;
            }  */
        }
        return alpha;
    }
}
