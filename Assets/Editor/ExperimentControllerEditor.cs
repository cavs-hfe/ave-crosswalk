using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof (ExperimentController))]
public class ExperimentControllerEditor : Editor {

    public override void OnInspectorGUI()
    {
        //ExperimentController ec = (ExperimentController)target;

        DrawDefaultInspector();
    }
}
