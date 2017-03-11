using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using SRandom = System.Random;

/// <summary>Override this to create your own animation system</summary>
public abstract class PLBTileCreationController : ScriptableObject
{
    public abstract List<PLBTileConnector> CanAddTiles(List<PLBTile> freeTiles, SRandom tileCheckRandom);
}

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
    public PLBTileCreationController tileCreationController = null;
    public int randomSeed = 1;

    public PLBTransitionAnimator pieceTransitionAnimation = null;
    public float transitionAnimationRate = 1.0f;

    SRandom tileSetRandom = null;
    SRandom freeTileCheckRandom = null;

    float tileSetMaxBias = 0.0f;

    List<PLBTile> spawnedTiles = new List<PLBTile>();
    List<PLBTile> freeTiles = new List<PLBTile>();

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

        tileSetRandom = new SRandom(randomSeed);
        freeTileCheckRandom = new SRandom(randomSeed);

        if (tileCreationController == null)
            tileCreationController = ScriptableObject.CreateInstance<PLBTileCreationController_Timed>();

        foreach (PLBTileSet.PLBTileSetEntry entry in tileSet.tiles)
        {
            tileSetMaxBias += entry.biasToSpawn;
        }

        PLBTile initialTile = null;
        if (tileSet.initialTiles.Count > 0)
        {
            initialTile = Instantiate(tileSet.initialTiles[tileSetRandom.Next(tileSet.initialTiles.Count)]);
            initialTile.CleanupConnectors();
        }
        else
        {
            initialTile = GetRandomTileFromTileset();
        }
        initialTile.SetConnectorAnimationData();
        spawnedTiles.Add(initialTile);
        freeTiles.Add(initialTile);
    }

    void Update()
    {
        if (freeTiles.Count == 0)
            return;

        List<PLBTileConnector> freeConnectors = tileCreationController.CanAddTiles(freeTiles, freeTileCheckRandom);

        if (freeConnectors != null && freeConnectors.Count > 0) //We can spawn
        {
            List<PLBTile> affectedTiles = new List<PLBTile>();
            PLBTile tile = null;
            PLBTileConnector tileConn = null;
            foreach (PLBTileConnector freeTileConn in freeConnectors)
            {
                affectedTiles.Add(freeTileConn.parentTile);

                tile = GetRandomTileFromTileset();
                tileConn = tile.connectors[freeTileCheckRandom.Next(tile.connectors.Count)];

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

                spawnedTiles.Add(tile);
                freeTiles.Add(tile); //Well, we probably have a free slot
            }

            bool tileIsStillFree = false;
            for (int i = 0; i < affectedTiles.Count; ++i)
            {
                tileIsStillFree = false;

                for (int j = 0; j < affectedTiles[i].connectors.Count; ++j)
                {
                    if (affectedTiles[i].connectors[j].connectedTo == null)
                    {
                        tileIsStillFree = true;
                        break;
                    }
                }

                if (!tileIsStillFree)
                    freeTiles.Remove(affectedTiles[i]);
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

        for (float i = 0; i < 1.0f; i += Time.deltaTime * transitionAnimationRate)
        {
            pieceTransitionAnimation.AnimateIn(tile.transform, goalPosition, goalRotation, i);
            yield return new WaitForEndOfFrame();
        }

        tile.transform.position = goalPosition;
        tile.transform.rotation = goalRotation;
    }
}