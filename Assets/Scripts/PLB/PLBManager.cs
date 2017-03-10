using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using SRandom = System.Random;

/// <summary>Override this to create your own animation system</summary>
public abstract class PLBTransitionAnimator : ScriptableObject
{
    public abstract void AnimateIn(Transform levelPiece, Vector3 targetPosition, Quaternion targetRotation, float alpha);
    public abstract void AnimateOut(Transform levelPiece, Vector3 startPosition, Quaternion startRotation, float alpha);
}

/// <summary>Primary handler of level generation</summary>
public class PLBManager : MonoBehaviour
{
    public PLBTileSet tileSet = null;
    public PLBTransitionAnimator pieceTransitionAnimation = null;
    public int randomSeed = 1;
    public float tileSpawnRate = 0.1f;

    SRandom tileSetRandom = null;
    SRandom freeTileCheckRandom = null;

    float tileSetMaxBias = 0.0f;

    List<PLBTile> spawnedTiles = new List<PLBTile>();
    List<PLBTile> freeTiles = new List<PLBTile>();
    float spawnTimeout = 1.0f;

    void Start()
    {
        if (tileSet == null)
        {
            Debug.LogError("TileSet not set for PLBManager", gameObject);
            Destroy(gameObject);
            return;
        }

        if (tileSet.tiles.Count == 0)
        {
            Debug.LogError("TileSet for PLBManager has no tiles", gameObject);
            Destroy(gameObject);
            return;
        }

        spawnTimeout = tileSpawnRate;
        tileSetRandom = new SRandom(randomSeed);
        freeTileCheckRandom = new SRandom(randomSeed);

        foreach (PLBTileSet.PLBTileSetEntry entry in tileSet.tiles)
        {
            tileSetMaxBias += entry.biasToSpawn;
        }

        PLBTile initialTile = GetRandomTileFromTileset();
        initialTile.SetConnectorAnimationData();
        spawnedTiles.Add(initialTile);
        freeTiles.Add(initialTile);
    }

    void Update()
    {
        spawnTimeout -= Time.deltaTime;
        if (spawnTimeout <= 0) //It's tile spawning time!
        {
            spawnTimeout = tileSpawnRate;

            PLBTile tile = GetRandomTileFromTileset();

            bool madeConnection = false;
            int tileAttemptCount = 0, connectorAttemptCount = 0;
            PLBTile freeTile;
            PLBTileConnector freeTileConn;
            do
            {
                freeTile = freeTiles[freeTileCheckRandom.Next(freeTiles.Count)];

                connectorAttemptCount = 0;
                do
                {
                    freeTileConn = freeTile.connectors[freeTileCheckRandom.Next(freeTile.connectors.Count)];

                    if (freeTileConn.connectedTo == null) //This connector is free. Now it's ours!
                    {
                        PLBTileConnector tileConn = tile.connectors[freeTileCheckRandom.Next(tile.connectors.Count)];
                        tileConn.connectedTo = freeTileConn; //Forward connect
                        freeTileConn.connectedTo = tileConn; //Back connect

                        //Time for hell. It's easier to parent the tile to the connector, move and rotate the connector, then
                        //reparent it back.
                        tileConn.transform.SetParent(transform, true);
                        tile.transform.SetParent(tileConn.transform, true);

                        //Use animationGoalXYZ as that's the final position for this tile
                        //freeTile may be animating and not in the position it should be
                        tileConn.transform.position = freeTileConn.animationGoalPosition;
                        tileConn.transform.rotation = Quaternion.Euler(freeTileConn.animationGoalRotation.eulerAngles + new Vector3(0.0f, 180.0f, 0.0f));

                        tile.transform.SetParent(transform, true);
                        tileConn.transform.SetParent(tile.transform, true);

                        //Store the current location data for animation purposes
                        tile.SetConnectorAnimationData();
                        StartCoroutine(TransitionInAnimation(tile));

                        //Check to see if we can remove this tile from the free list
                        bool isFreeTileNoLongerFree = true;
                        foreach (PLBTileConnector c in freeTile.connectors)
                        {
                            if (c.connectedTo == null)
                            {
                                isFreeTileNoLongerFree = false;
                                break;
                            }
                        }

                        if (isFreeTileNoLongerFree)
                        {
                            freeTiles.Remove(freeTile);
                        }

                        madeConnection = true;
                        break;
                    }
                }
                while (++connectorAttemptCount < 8);
            }
            while (!madeConnection && ++tileAttemptCount < 32);

            if (madeConnection)
            {
                spawnedTiles.Add(tile);
                freeTiles.Add(tile); //Well, we probably have a free slot
            }
            else
            {
                Destroy(tile.gameObject); //Couldn't connect. Fuck it.
            }
        }
    }

    private PLBTile GetRandomTileFromTileset()
    {
        PLBTile tile = null;

        float currentMinimum = 0.0f;
        float randVal = (float)(tileSetRandom.NextDouble() * tileSetMaxBias);

        foreach (PLBTileSet.PLBTileSetEntry entry in tileSet.tiles)
        {
            if (currentMinimum < randVal && currentMinimum + entry.biasToSpawn > randVal) //We found our tile
            {
                tile = Instantiate(entry.tile);
                break;
            }
            currentMinimum += entry.biasToSpawn;
        }

        if (tile == null)
        {
            //If we reach here, something went wrong, although the FPU may be iffy. Assume last tile won
            tile = Instantiate(tileSet.tiles.Last().tile);
        }
        tile.CleanupConnectors();
        return tile;
    }

    private IEnumerator TransitionInAnimation(PLBTile tile)
    {
        if (pieceTransitionAnimation == null)
            yield break;

        Vector3 goalPosition = tile.transform.position;
        Quaternion goalRotation = tile.transform.rotation;

        for (float i = 0; i < 1.0f; i += Time.deltaTime)
        {
            pieceTransitionAnimation.AnimateIn(tile.transform, goalPosition, goalRotation, i);
            yield return new WaitForEndOfFrame();
        }

        tile.transform.position = goalPosition;
        tile.transform.rotation = goalRotation;
    }
}