using UnityEngine;
using UnityEditor;
using System;

[CustomEditor(typeof(WaypointSystem))]
public class WaypointEditor : Editor
{
    WaypointSystem waypointSystem;
    int index;

    private void OnEnable()
    {
        waypointSystem = (WaypointSystem)target;
    }
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GUILayout.Space(20f);
        GUILayout.BeginHorizontal();
        GUILayout.Label("Waypoint Index");

        string input = GUILayout.TextField($"{index}");
        index = Convert.ToInt32(input);

        EditorGUI.BeginChangeCheck();

        GUILayout.Space(10f);
        if (GUILayout.Button("Add"))
        {
            EditorGUI.EndChangeCheck();
            Undo.RecordObject(target, $"Waypoint added");

            if (waypointSystem.waypoints.Count == 0)
            {
                waypointSystem.waypoints.Add(Vector3.zero);
            }
            else if (index >= waypointSystem.waypoints.Count - 1)
            {
                waypointSystem.waypoints.Add(waypointSystem.waypoints[waypointSystem.waypoints.Count - 1] + Vector3.forward);
                index = waypointSystem.waypoints.Count;
            }
            else
            {
                waypointSystem.waypoints.Insert(index, waypointSystem.waypoints[index] + Vector3.forward);
                index = waypointSystem.waypoints.Count - 1;
            }
            waypointSystem.ResetPreview();
            SceneView.RepaintAll();
        }

        GUILayout.Space(5f);
        if (GUILayout.Button("Remove") && waypointSystem.waypoints.Count > 0)
        {
            EditorGUI.EndChangeCheck();
            Undo.RecordObject(target, $"Waypoint removed");

            if (index >= waypointSystem.waypoints.Count)
            {
                index = waypointSystem.waypoints.Count;
                waypointSystem.waypoints.RemoveAt(index - 1);
            }
            else
            {
                if (index < 0)
                {
                    index = 0;
                }
                waypointSystem.waypoints.RemoveAt(index);
            }
            if (index > 0)
            {
                index = waypointSystem.waypoints.Count - 1;
            }
            waypointSystem.ResetPreview();
            SceneView.RepaintAll();
        }
        GUILayout.EndHorizontal();
    }

    public void OnSceneGUI()
    {
        Handles.BeginGUI();
        if (GUI.Button(new Rect(8, 8, 125, 20), "Preview Movement"))
        {
            waypointSystem.movePreview = !waypointSystem.movePreview;
            waypointSystem.ResetPreview();
        }
        if (waypointSystem.LoopWaypoints == false && waypointSystem.movePreview)
        {
            if (GUI.Button(new Rect(135, 8, 60, 20), "Restart"))
            {
                waypointSystem.ResetPreview();
            }
        }
        if (waypointSystem.movePreview)
        {
            GUI.Label(new Rect(10, 30, 310, 20), "Hold right mouse button down to preview in realtime.");
        }
        Handles.EndGUI();

        for (int i = 0; i < waypointSystem.waypoints.Count; i++)
        {
            EditorGUI.BeginChangeCheck();
            Vector3 move = Handles.PositionHandle(waypointSystem.waypoints[i], Quaternion.identity);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, $"Waypoint '{i}' move");
                waypointSystem.waypoints[i] = move;
                waypointSystem.ResetPreview();
            }
        }

        for (int i = 0; i < waypointSystem.waypoints.Count; i++)
        {
            Vector3 waypointNext = waypointSystem.LoopWaypoints ? i < waypointSystem.waypoints.Count - 1 ?
                waypointSystem.waypoints[i + 1] : waypointSystem.waypoints[0] : i < waypointSystem.waypoints.Count - 1 ?
                waypointSystem.waypoints[i + 1] : waypointSystem.waypoints[i];

            EditorGUI.BeginChangeCheck();
            Vector3 midPoint = (waypointSystem.waypoints[i] + waypointNext) * 0.5f;

            if (Handles.Button(midPoint, Quaternion.identity, 0.05f, 0.02f, Handles.SphereHandleCap))
            {
                EditorGUI.EndChangeCheck();
                Undo.RecordObject(target, $"Insert waypoint between");
                waypointSystem.waypoints.Insert(i + 1, midPoint + (i < waypointSystem.waypoints.Count - 1 ? Vector3.zero : (Vector3.forward * 0.25f)));
                waypointSystem.ResetPreview();

            }
        }
    }
}
