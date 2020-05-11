using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaypointSystem : MonoBehaviour
{
    //TODO: Add the waypoint data and functions
    [SerializeField] private List<Vector3> m_Waypoints;
    [SerializeField] private bool m_LoopWaypoints;

    private Vector3 m_TargetPosition;
    private int m_WaypointIndex;

    public bool LoopWaypoints { get => m_LoopWaypoints; set => m_LoopWaypoints = value; }

    public List<Vector3> GetWaypoints() => m_Waypoints;

    public void SetupWaypointSystem( in List<Vector3> waypoints, in bool loopWaypoints)
    {
        m_Waypoints = waypoints;
        LoopWaypoints = loopWaypoints;
        m_Waypoints[0] = gameObject.transform.position;
        for (int i = 1; i < m_Waypoints.Count; i++)
        {
            Vector3 waypoint = m_Waypoints[i];
            m_Waypoints[i] = waypoint + m_Waypoints[0];
        }
        m_TargetPosition = m_Waypoints[m_WaypointIndex];
    }


    private void Update()
    {
        if (m_Waypoints != null)
        {
            if (transform.position == m_TargetPosition)
            {
                m_WaypointIndex++;
                if (LoopWaypoints)
                {
                    if (m_WaypointIndex > m_Waypoints.Count)
                    {
                        m_WaypointIndex = 0;
                    }
                    m_TargetPosition = m_WaypointIndex < m_Waypoints.Count ? m_Waypoints[m_WaypointIndex] : m_Waypoints[0];
                }
                else
                {
                    if (m_WaypointIndex < m_Waypoints.Count)
                    {
                        m_TargetPosition = m_Waypoints[m_WaypointIndex];
                    }
                }
            }
            transform.position = Vector3.MoveTowards(transform.position, m_TargetPosition, 10f * Time.deltaTime);
        }
    }

}