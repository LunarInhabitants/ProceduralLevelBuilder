using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SRandom = System.Random;

/// <summary>
/// A sliding animation that eases out at the end
/// </summary>
[CreateAssetMenu(fileName = "PLB_TCC_Timed", menuName = "Procedural Level Builder/Tile Creation Controllers/Timed", order = 1100)]
public class PLBTileCreationController_Timed : PLBTileCreationController
{
    public float timeBeforeNextSpawn = 1.0f;
    private float timeUntilNextSpawn = 0.0f;

    public override List<PLBTileConnector> CanAddTiles(List<PLBTile> freeTiles, SRandom tileCheckRandom)
    {
        timeUntilNextSpawn -= Time.deltaTime;
        if (timeUntilNextSpawn > 0.0f)
            return null;

        timeUntilNextSpawn = timeBeforeNextSpawn;

        int tileAttemptCount = 0, connectorAttemptCount = 0;
        PLBTile freeTile;
        PLBTileConnector freeTileConn;
        do
        {
            freeTile = freeTiles[tileCheckRandom.Next(freeTiles.Count)];

            connectorAttemptCount = 0;
            do
            {
                freeTileConn = freeTile.connectors[tileCheckRandom.Next(freeTile.connectors.Count)];

                if (freeTileConn.connectedTo == null) //This connector is free. Now it's ours!
                    return new List<PLBTileConnector>() { freeTileConn };
            }
            while (++connectorAttemptCount < 8);
        }
        while (++tileAttemptCount < 32);

        return null;
    }
}
