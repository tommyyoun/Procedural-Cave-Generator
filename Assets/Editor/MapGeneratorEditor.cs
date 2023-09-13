using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor (typeof (MapGenerator))]
public class MapGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        MapGenerator mapGen = (MapGenerator)target;

        if (DrawDefaultInspector())
        {
            if (mapGen.autoUpdate)
            {
                if (mapGen.mode.ToString() == "Basic")
                {
                    mapGen.GenerateBasic();
                }

                if (mapGen.mode.ToString() == "Advanced")
                {
                    mapGen.GenerateAdvanced();
                }
            }
        }

        if (GUILayout.Button("Generate"))
        {
            if (mapGen.mode.ToString() == "Basic")
            {
                mapGen.GenerateBasic();
            }

            if (mapGen.mode.ToString() == "Advanced")
            {
                mapGen.GenerateAdvanced();
            }
        }
    }
}
