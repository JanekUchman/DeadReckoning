using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;

[System.Serializable]
public class TileMap : MonoBehaviour
{
    public Dictionary<string, GameObject> tiles;
    public GameObject lastTilePrefabUsed;

    public bool Initialized
    {
        get
        {
            return tiles != null;
        }
    }

    public void Initialize()
    {
        tiles = new Dictionary<string, GameObject>();
    }

    public static string CoordToKey(float x, float y)
    {
        return x + "," + y;
    }

    public void PlaceTile(float x, float y, GameObject tile)
    {
        RemoveTile(x, y);
        var key = CoordToKey(x, y);
        var newTile = (GameObject)Instantiate(tile, new Vector3(x, y, 0), Quaternion.identity);
        newTile.transform.parent = transform;
        tiles[key]= newTile;
    }
    public void RemoveTile(float x, float y)
    {
        var key = CoordToKey(x, y);
        if (tiles.ContainsKey(key))
        {
            GameObject.DestroyImmediate(tiles[key]);
            tiles.Remove(key);
        }
    }
}
