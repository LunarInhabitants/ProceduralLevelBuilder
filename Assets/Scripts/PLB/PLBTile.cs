using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A tile with connectors that can connect to other tiles
/// </summary>
public class PLBTile : MonoBehaviour
{
    /// <summary>The connectors attached to this tile</summary>
    public List<PLBTileConnector> connectors = new List<PLBTileConnector>();

    void Start()
    {
        CleanupConnectors();
        foreach (PLBTileConnector connector in connectors)
            connector.parentTile = this;
    }

    public void CleanupConnectors()
    {
        connectors.RemoveAll(c => c == null);
    }

    public void SetConnectorAnimationData()
    {
        foreach (PLBTileConnector c in connectors)
        {
            c.animationGoalPosition = c.transform.position;
            c.animationGoalRotation = c.transform.rotation;
        }
    }
}
