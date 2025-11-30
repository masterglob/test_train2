using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SplineTerrainProjector))]
public class SplineTerrainProjectorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        SplineTerrainProjector proj = (SplineTerrainProjector)target;

        GUILayout.Space(10);

        if (GUILayout.Button("Project splines on terrain"))
        {
            proj.ProjectAllSplines();
        }
    }
}
