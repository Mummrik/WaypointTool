using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class WaypointSystem : MonoBehaviour
{
    [SerializeField] private bool loopWaypoints;

    private Vector3 targetPosition;
    private int waypointIndex;
    private Vector3 previewWaypoint;
    private Vector3 previewTargetWaypoint;
    private int previewWaypointIndex;

    [HideInInspector] public bool movePreview;
    public List<Vector3> waypoints;
    public bool LoopWaypoints { get => loopWaypoints; set => loopWaypoints = value; }


    private void Update()
    {
        if (waypoints != null)
        {
            if (transform.position == targetPosition)
            {
                waypointIndex++;
                if (LoopWaypoints)
                {
                    if (waypointIndex > waypoints.Count)
                    {
                        waypointIndex = 0;
                    }
                    targetPosition = waypointIndex < waypoints.Count ? waypoints[waypointIndex] : waypoints[0];
                }
                else
                {
                    if (waypointIndex < waypoints.Count)
                    {
                        targetPosition = waypoints[waypointIndex];
                    }
                }
            }
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, 10f * Time.deltaTime);
        }
    }
    private void OnDrawGizmosSelected()
    {
        for (int i = 0; i < waypoints.Count; i++)
        {
            if (i == 0)
            {
                Gizmos.color = Color.yellow;
            }
            else if (i == waypoints.Count - 1)
            {
                Gizmos.color = Color.green;
            }
            else
            {
                Gizmos.color = Color.magenta;
            }
            Gizmos.DrawSphere(waypoints[i], .05f);
            Handles.Label(waypoints[i] + Vector3.up * 0.33f, $"Waypoint [{i}]");

            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(waypoints[i], i < waypoints.Count - 1 ? waypoints[i + 1] : LoopWaypoints ? waypoints[0] : waypoints[i]);
        }

        if (movePreview && waypoints.Count > 0)
        {
            if (previewWaypoint == previewTargetWaypoint)
            {
                previewWaypointIndex++;
                if (LoopWaypoints)
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
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(previewWaypoint, .07f);
        }

    }
    public void ResetPreview()
    {
        if (waypoints.Count == 0) { return; }
        previewWaypointIndex = 0;
        previewWaypoint = waypoints[previewWaypointIndex];
        previewTargetWaypoint = waypoints[previewWaypointIndex];
    }
}