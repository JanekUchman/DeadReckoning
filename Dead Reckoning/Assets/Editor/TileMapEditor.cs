using System;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;


[CustomEditor(typeof(TileMap))]
public class TileMapEditor : Editor
{

    private TileMap tileMap;
    [SerializeField]
    private GameObject currentTile;

    private GameObject tiles;
    private List<GameObject> tileList = new List<GameObject>();
    private float tileWidth = 1.0f;
    private float tileHeight = 1.0f;
    private int currentTileNumber = 0;

    public void OnEnable()
    {
        tileMap = (TileMap) target;
        EditorUtility.SetDirty(tileMap);
        if (!tileMap.Initialized)
            tileMap.Initialize();
    }

    public override void OnInspectorGUI()
    {
        currentTile = (GameObject) EditorGUILayout.ObjectField("Current tile", currentTile, typeof(GameObject), false);
        tiles = (GameObject)EditorGUILayout.ObjectField("Tiles prefab", tiles, typeof(GameObject), true);
        if (GUILayout.Button("Reload tiles") || currentTile == null)
        {
            LoadFromPrefab();
        }

        
        
    }

    public void OnSceneGUI()
    {

        if (tileMap && currentTile)
        {
            
            
            var e = Event.current;
            var controlID = GUIUtility.GetControlID(FocusType.Passive);
            var placeTile = false;
            var incrementTile = false;
            var decrementTile = false;
            var removeTile = false;
            HandleEvent(e, controlID, ref placeTile, ref removeTile, ref incrementTile, ref decrementTile);

            float x;
            float y;
            CalculateMousePos(e, out x, out y);


            DrawHandles(x, y);


            if (placeTile)
                tileMap.PlaceTile(x + tileWidth / 2, y + tileHeight / 2, currentTile);
            if (removeTile)
                tileMap.RemoveTile(x + tileWidth / 2, y + tileHeight / 2);

            if (incrementTile)
            {
                MoveToNextObject();
            }

            if (decrementTile)
            {
                MoveToPreviousObject();
            }
        }
        else
        {
            tileMap = (TileMap) target;
        }
        SceneView.RepaintAll();
    }

    private void LoadFromPrefab()
    {
        if (!tiles && tileMap.lastTilePrefabUsed)
        {
            tiles = tileMap.lastTilePrefabUsed;
        }
        if (tiles)
        {
            tileList.Clear();
            tileMap.lastTilePrefabUsed = tiles;
            foreach (Transform childTransform in tiles.transform)
            {
                tileList.Add(childTransform.gameObject);
            }
            currentTile = tileList[0];
            UpdateTileValues();
            Repaint();
        }
    }

    private void MoveToPreviousObject()
    {
        try
        {
            if (currentTileNumber != 0)
                currentTileNumber--;
            else
                currentTileNumber = tileList.Count - 1;

            currentTile = tileList[currentTileNumber];
            Repaint();
        }
        catch (Exception)
        {
            Debug.Log("Please assign a prefab with children tile objects.");
        }
        UpdateTileValues();
    }

    private void MoveToNextObject()
    {
        try
        {
            if (currentTileNumber != tileList.Count - 1)
                currentTileNumber++;
            else
                currentTileNumber = 0;

            currentTile = tileList[currentTileNumber];
            Repaint();
            UpdateTileValues();
        }
        catch (Exception exception)
        {
            Debug.Log("Please assign a prefab with children tile objects.");
            Debug.Log(exception);
        }
        UpdateTileValues();
    }

    private void DrawHandles(float x, float y)
    {
        Handles.DrawLine(new Vector3(x, y), new Vector3(x + tileWidth, y));
        Handles.DrawLine(new Vector3(x + tileWidth, y), new Vector3(x + tileWidth, y + tileHeight));
        Handles.DrawLine(new Vector3(x + tileWidth, y + tileHeight), new Vector3(x, y + tileHeight));
        Handles.DrawLine(new Vector3(x, y + tileHeight), new Vector3(x, y));
    }

    private void CalculateMousePos(Event e, out float x, out float y)
    {
        var worldPoint =
            Camera.current.ScreenToWorldPoint(new Vector3(e.mousePosition.x,
                -e.mousePosition.y + Camera.current.pixelHeight));
        var normalPoint =
            tileMap.transform.worldToLocalMatrix.MultiplyPoint(
                new Vector3(worldPoint.x / tileWidth, worldPoint.y / tileHeight, worldPoint.z));
        x = Mathf.FloorToInt(normalPoint.x);
        x = x * tileWidth;
        y = Mathf.FloorToInt(normalPoint.y);
        y = y * tileHeight;
    }

    private static void HandleEvent(Event e, int controlID, ref bool placeTile, ref bool removeTile, ref bool incrementTile,
        ref bool decrementTile)
    {
        switch (e.type)
        {
            case EventType.Layout:
                HandleUtility.AddDefaultControl(controlID);
                break;
            case EventType.MouseDrag:

                if (e.button == 0)
                    placeTile = true;
                if (e.button == 0 && e.shift)
                    removeTile = true;


                break;
            case EventType.MouseDown:
                if (e.button == 0)
                    placeTile = true;
                if (e.button == 1 || e.button == 0 && e.shift)
                    removeTile = true;
                break;
            case EventType.KeyDown:
                if (e.keyCode == KeyCode.Alpha3)
                    incrementTile = true;
                if (e.keyCode == KeyCode.Alpha4)
                    decrementTile = true;
                break;

            default:
                break;
        }
    }

    private void UpdateTileValues()
    {
        try
        {
            tileWidth = currentTile.GetComponent<SpriteRenderer>().size.x;
            tileHeight = currentTile.GetComponent<SpriteRenderer>().size.y;
        }
        catch (Exception)
        {
            Debug.Log("Please assign a gameobject with a sprite renderer");
            currentTile = null;
        }
    }
}