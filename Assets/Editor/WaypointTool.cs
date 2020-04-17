using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
using System.Collections.Generic;

public class WaypointTool : EditorWindow
{
    [MenuItem("Tools/Waypoint Creator")]
    public static void OpenWaypointTool() => GetWindow<WaypointTool>();

    private bool hasWaypointSystem;
    private bool loopWaypoints;
    private bool drawBezier;

    private List<Vector3> waypoints;

    SerializedObject so;

    private void OnEnable()
    {
        loopWaypoints = false;
        hasWaypointSystem = false;
        waypoints = new List<Vector3>();
        so = new SerializedObject(this);
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
        if (waypoints.Count > 0)
        {
            for (int i = 0; i < waypoints.Count; i++)
            {
                Vector3 waypoint = waypoints[i];
                waypoints[i] = Handles.PositionHandle(waypoint, Quaternion.identity);
                if (i == 0)
                {
                    Handles.color = Color.yellow;
                }else if(i == waypoints.Count - 1)
                {
                    Handles.color = Color.green;
                }
                else
                {
                    Handles.color = Color.white;
                }
                Handles.SphereHandleCap(i, waypoint, Quaternion.identity, 0.1f, EventType.Repaint);
                Vector3 waypointNext = loopWaypoints ? i < waypoints.Count - 1 ? waypoints[i + 1] : waypoints[0] : i < waypoints.Count - 1 ? waypoints[i + 1] : waypoints[i];
                float halfHeight = (waypoint.z - waypointNext.z) * .5f;
                Vector3 offset = Vector3.forward * (halfHeight < 0 ? halfHeight : -halfHeight);

                if (drawBezier)
                {
                    Handles.DrawBezier(
                        waypoint,
                        waypointNext,
                        waypoint - offset,
                        waypointNext + offset,
                        Color.magenta,
                        EditorGUIUtility.whiteTexture,
                        1f
                    );
                }
                else
                {
                    Handles.color = Color.magenta;
                    Handles.DrawLine(waypoint, waypointNext);
                    Handles.color = Color.white;
                }
            }
        }
        sceneView.Repaint();
    }

    private void OnGUI()
    {
        so.Update();
        if (so.ApplyModifiedProperties())
        {

        }

        GUILayout.Space(20f);
        if (!hasWaypointSystem)
        {
            if (GUILayout.Button("Create new waypoint system"))
            {
                hasWaypointSystem = true;
                waypoints = new List<Vector3>();
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
            }

            using (new EditorGUI.DisabledScope(Selection.gameObjects.Length == 0 || Selection.gameObjects.Length > 1))
            {
                if (GUILayout.Button("Apply to selected object"))
                {
                    GameObject go = Selection.activeObject as GameObject;
                    if (go.GetComponent<WaypointSystem>())
                    {
                        //TODO: Replace the waypoint system data
                    }
                    else
                    {
                        go.AddComponent<WaypointSystem>();
                    }
                }
            }
        }
    }
}
