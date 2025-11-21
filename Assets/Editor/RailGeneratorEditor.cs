using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(RailGenerator))]
public class RailGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        RailGenerator generator = (RailGenerator)target;

        GUILayout.Space(10);

        if (GUILayout.Button("Generate Rails"))
        {
            generator.Generate();
        }
    }
}
