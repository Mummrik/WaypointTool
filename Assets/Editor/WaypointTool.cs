using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
using System.Collections.Generic;

public class WaypointTool : EditorWindow
{
    [MenuItem("Tools/Waypoint Creator")]
    public static void OpenWaypointTool() => GetWindow<WaypointTool>();
    [SerializeField] private bool loopWaypoints;
    [SerializeField] private int waypointIndex;

    private bool hasWaypointSystem;

    private List<Vector3> waypoints;

    SerializedObject so;
    SerializedProperty propLoopWaypoints;
    SerializedProperty propWaypointIndex;

    private bool drawPreviewWaypointMovement;
    private Vector3 previewWaypoint;
    private Vector3 previewTargetWaypoint;
    private int previewWaypointIndex;

    private void OnEnable()
    {
        so = new SerializedObject(this);
        loopWaypoints = false;
        hasWaypointSystem = false;
        waypoints = new List<Vector3>();
        propLoopWaypoints = so.FindProperty("loopWaypoints");
        propWaypointIndex = so.FindProperty("waypointIndex");

        Selection.selectionChanged += Repaint;
        SceneView.duringSceneGui += DuringSceneGUI;
    }
    private void OnDisable()
    {
        hasWaypointSystem = false;
        Selection.selectionChanged -= Repaint;
        SceneView.duringSceneGui -= DuringSceneGUI;
    }

    private void DuringSceneGUI(SceneView sceneView)
    {
        if (waypoints.Count > 1)
        {
            Handles.BeginGUI();
            if (GUI.Button(new Rect(8, 8, 125, 20), "Preview Movement"))
            {
                drawPreviewWaypointMovement = !drawPreviewWaypointMovement;
                ResetPreview();
            }
            if (loopWaypoints == false && drawPreviewWaypointMovement)
            {
                if (GUI.Button(new Rect(135, 8, 60, 20), "Restart"))
                {
                    ResetPreview();
                }
            }
            Handles.EndGUI();
        }

        if (waypoints.Count > 0)
        {
            for (int i = 0; i < waypoints.Count; i++)
            {
                Vector3 waypoint = waypoints[i];
                Handles.Label(waypoint + Vector3.up * 0.2f, $"Waypoint [{i}]");
                waypoints[i] = Handles.PositionHandle(waypoint, Quaternion.identity);
                if (waypoints[i] != waypoint)
                {
                    //Debug.Log($"{i} moved");
                    //Undo.RecordObject(waypoint, $"Move {i}");
                }
                Handles.color = i == 0 ? Color.yellow : i == waypoints.Count - 1 ? Color.green : Color.white;
                Handles.SphereHandleCap(i, waypoint, Quaternion.identity, 0.1f, EventType.Repaint);
                Vector3 waypointNext = loopWaypoints ? i < waypoints.Count - 1 ? waypoints[i + 1] : waypoints[0] : i < waypoints.Count - 1 ? waypoints[i + 1] : waypoints[i];

                Handles.color = Color.magenta;
                Handles.DrawLine(waypoint, waypointNext);
                Handles.color = Color.white;

                Handles.color = Color.magenta;
                Vector3 midPoint = (waypoint + waypointNext) * 0.5f;
                if (!loopWaypoints && i == waypoints.Count - 1)
                {
                    midPoint += Vector3.forward * 0.25f;
                    Handles.Label(midPoint + Vector3.up * 0.1f, $"Add New");
                }
                if (Handles.Button(midPoint, Quaternion.identity, 0.05f, 0.02f, Handles.SphereHandleCap))
                {
                    waypoints.Insert(i + 1, midPoint + (i < waypoints.Count - 1 ? Vector3.zero : (Vector3.forward * 0.25f)));
                }
            }

            if (drawPreviewWaypointMovement)
            {
                if (previewWaypoint == previewTargetWaypoint)
                {
                    previewWaypointIndex++;
                    if (loopWaypoints)
                    {
                        if (previewWaypointIndex > waypoints.Count)
                        {
                            previewWaypointIndex = 0;
                        }
                        previewTargetWaypoint = previewWaypointIndex < waypoints.Count ? waypoints[previewWaypointIndex] : waypoints[0];
                    }
                    else
                    {
                        if (previewWaypointIndex < waypoints.Count)
                        {
                            previewTargetWaypoint = waypoints[previewWaypointIndex];
                        }
                    }
                }

                previewWaypoint = Vector3.MoveTowards(previewWaypoint, previewTargetWaypoint, 0.01f);
                Handles.color = Color.blue;
                Handles.SphereHandleCap(-1, previewWaypoint, Quaternion.identity, 0.2f, EventType.Repaint);
            }
        }
        sceneView.Repaint();
    }

    private void OnGUI()
    {
        GUILayout.Space(20f);
        so.Update();
        if (hasWaypointSystem)
        {
            propLoopWaypoints.boolValue = propLoopWaypoints.boolValue;
            EditorGUILayout.PropertyField(propLoopWaypoints);
        }


        if (!hasWaypointSystem)
        {
            if (GUILayout.Button("Create new"))
            {
                hasWaypointSystem = true;
                drawPreviewWaypointMovement = false;
                waypoints = new List<Vector3>();

                //TODO: Get the sceneView camera center as starting point
                Vector3 point = Vector3.zero;
                waypoints.Add(point);
            }
            using (new EditorGUI.DisabledScope(Selection.gameObjects.Length != 1))
            {
                if (GUILayout.Button("Load from selected"))
                {
                    WaypointSystem waypointSystem = Selection.activeGameObject.GetComponent<WaypointSystem>();
                    if (waypointSystem)
                    {
                        hasWaypointSystem = true;
                        waypoints = waypointSystem.GetWaypoints();
                        ResetPreview();
                    }
                    else
                    {
                        Debug.LogWarning("Selection do not have a waypoint system!");
                    }
                }
            }
        }
        else
        {
            if (GUILayout.Button("Add waypoint"))
            {
                //TODO: Get the sceneView camera center as starting point
                Vector3 point = Vector3.zero;
                if (waypoints.Count > 0)
                    point = waypoints[waypoints.Count - 1] + (Vector3.forward * 0.5f);

                waypoints.Add(point);
                ResetPreview();
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(propWaypointIndex);
            if (GUILayout.Button("Add"))
            {
                //TODO: Get the sceneView camera center as starting point
                Vector3 point = Vector3.zero;
                if (waypoints.Count > 0 && propWaypointIndex.intValue < waypoints.Count - 1)
                {
                    if (propWaypointIndex.intValue < 1)
                    {
                        point = (waypoints[propWaypointIndex.intValue] + Vector3.back) * 0.5f;
                    }
                    else
                    {
                        point = (waypoints[propWaypointIndex.intValue - 1] + waypoints[propWaypointIndex.intValue]) * 0.5f;
                    }
                }
                else
                    point = waypoints[waypoints.Count - 1] + (Vector3.forward * 0.5f);

                waypoints.Insert(propWaypointIndex.intValue, point);
                ResetPreview();
            }

            if (GUILayout.Button("Remove"))
            {
                if (propWaypointIndex.intValue < 0 || propWaypointIndex.intValue > waypoints.Count - 1) { return; }
                waypoints.RemoveAt(propWaypointIndex.intValue);
            }
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(100f);
            using (new EditorGUI.DisabledScope(Selection.gameObjects.Length != 1))
            {
                if (GUILayout.Button("Apply to selected object"))
                {
                    GameObject go = Selection.activeObject as GameObject;
                    WaypointSystem waypointSystem = go.GetComponent<WaypointSystem>();
                    if (!waypointSystem)
                    {
                        waypointSystem = go.AddComponent<WaypointSystem>();
                    }

                    waypointSystem.SetupWaypointSystem(waypoints, propLoopWaypoints.boolValue);
                }
            }

            if (GUILayout.Button("Discard"))
            {
                ResetPreview();
                waypoints = new List<Vector3>();
                hasWaypointSystem = false;
            }
            if (Selection.activeGameObject)
            {
                if (GUILayout.Button("Load from selected"))
                {
                    WaypointSystem waypointSystem = Selection.activeGameObject.GetComponent<WaypointSystem>();
                    if (waypointSystem)
                    {
                        hasWaypointSystem = true;
                        waypoints = waypointSystem.GetWaypoints();
                        loopWaypoints = waypointSystem.LoopWaypoints;
                        ResetPreview();
                    }
                    else
                    {
                        Debug.LogWarning("Selection do not have a waypoint system!");
                    }
                }
            }

        }

        if (so.ApplyModifiedProperties())
        {
            SceneView.RepaintAll();
            Repaint();
        }
    }

    private void ResetPreview()
    {
        previewWaypointIndex = 0;
        previewWaypoint = waypoints[previewWaypointIndex];
        previewTargetWaypoint = waypoints[previewWaypointIndex];
    }
}
