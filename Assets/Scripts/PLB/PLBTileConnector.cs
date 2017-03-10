using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>A point which other tiles can connect to using their own tile connectors</summary>
public class PLBTileConnector : MonoBehaviour
{
    /// <summary>Which connector group this connector belongs to</summary>
    public string connectorGroup = "*";

    /// <summary>
    /// Which connector groups this connector can connect to.
    /// If empty, this connector can connect to all groups, excluding any in the exclusions list.
    /// </summary>
    public List<string> includeConnectorGroups = new List<string>();

    /// <summary>
    /// Which connector groups this connector cannot connect to.
    /// If empty, this connector can connect to either the connectors in the inclusion list, or all connectors.
    /// </summary>
    public List<string> excludeConnectorGroups = new List<string>();

    [NonSerialized] public PLBTile parentTile = null;
    [NonSerialized] public PLBTileConnector connectedTo = null;

    //Used so animations know their goal points correctly
    //Without these, tiles may spawn on an already animating tile, and the snap position and rotation will be wrong
    [NonSerialized] public Vector3 animationGoalPosition;
    [NonSerialized] public Quaternion animationGoalRotation;

    /// <summary>Draws a fancy gizmo so you can see the rotation of the connector.</summary>
    void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
        Gizmos.DrawCube(new Vector3(0.25f, 0.0f, 0.0f), new Vector3(0.5f, 0.25f, 0.25f));
        Gizmos.color = connectedTo == null ? Color.red : Color.green;
        Gizmos.DrawCube(new Vector3(0.45f, 0.0f, 0.0f), new Vector3(0.4f, 0.4f, 0.4f));
    }
}
