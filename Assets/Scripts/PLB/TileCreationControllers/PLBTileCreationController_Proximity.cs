using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SRandom = System.Random;

/// <summary>
/// A sliding animation that eases out at the end
/// </summary>
[CreateAssetMenu(fileName = "PLB_TCC_Proximity", menuName = "Procedural Level Builder/Tile Creation Controllers/Proximity", order = 1101)]
public class PLBTileCreationController_Proximity : PLBTileCreationController
{
    [NonSerialized] public List<GameObject> proximityObjects = new List<GameObject>();
    public float proximityDistance = 4.0f;

    public override List<PLBTileConnector> CanAddTiles(List<PLBTile> freeTiles, SRandom tileCheckRandom)
    {
        float proxDistSq = proximityDistance * proximityDistance;
        List<PLBTileConnector> triggeredConnectors = new List<PLBTileConnector>();

        foreach (PLBTile t in freeTiles)
        {
            foreach (PLBTileConnector c in t.connectors)
            {
                if (c.connectedTo == null)
                {
                    foreach (GameObject o in proximityObjects)
                    {
                        if ((o.transform.position - c.transform.position).sqrMagnitude <= proxDistSq)
                        {
                            triggeredConnectors.Add(c);
                            break;
                        }
                    }
                }
            }
        }

        return triggeredConnectors;
    }
}
