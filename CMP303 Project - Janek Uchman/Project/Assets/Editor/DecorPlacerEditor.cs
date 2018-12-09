using System;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

[CustomEditor(typeof(DecorMap))]
public class DecorPlacerEditor : Editor
{
    private DecorMap decorMap;
    [SerializeField]
    private GameObject currentDecor;
    public int currentDecorNumber = 0;

    private GameObject decorPrefab;
    private List<GameObject> decorList = new List<GameObject>();

    private BrushInformation brushInformation = new BrushInformation();


    public void OnEnable()
    {
        decorMap = (DecorMap)target;
        if (!decorMap.Initialized)
            decorMap.Initialize();
    }

    public override void OnInspectorGUI()
    {
        SetValues();
        ClampValues();
        if (decorPrefab != decorMap.lastDecorPrefabUsed || currentDecor == null)
        {
            LoadFromPrefab();
        }
        if (decorMap.brushInformationList.Count >= currentDecorNumber && decorMap.brushInformationList.Count != 0 && brushInformation != null)
            decorMap.brushInformationList[currentDecorNumber] = brushInformation;

    }

    public void OnSceneGUI()
    {

        if (decorMap && currentDecor)
        {


            var e = Event.current;
            var controlID = GUIUtility.GetControlID(FocusType.Passive);
            var placedecor = false;
            var incrementdecor = false;
            var decrementdecor = false;
            var removedecor = false;

            HandleEvent(e, controlID, ref placedecor, ref removedecor, ref incrementdecor, ref decrementdecor);

            float x;
            float y;
            CalculatePosition(e, out x, out y);


            if (e.shift)
            {
                DrawDeleteBox(x, y);
            }
            else
            {
                DrawPlacementBox(x, y);
            }

            if (placedecor)
                decorMap.PlaceDecor(x + brushInformation.decorWidth / 2, y + brushInformation.decorHeight / 2, brushInformation.randomRotation, currentDecor);
            if (removedecor)
            {
                if (brushInformation.overrideDefaultDeleteBrush)
                {
                    CycleThroughDeleteBrush(x, y);
                }
                else
                {
                    decorMap.RemoveDecor(x + brushInformation.decorWidth / 2, y + brushInformation.decorHeight / 2);

                }
            }

            if (incrementdecor)
            {
                GoToNextDecor();
            }

            if (decrementdecor)
            {
                GoToPreviousDecor();
            }



        }
        else
        {
            decorMap = (DecorMap)target;

        }
        SceneView.RepaintAll();






    }

    private void CalculatePosition(Event e, out float x, out float y)
    {
        var worldPoint =
            Camera.current.ScreenToWorldPoint(new Vector3(e.mousePosition.x,
                -e.mousePosition.y + Camera.current.pixelHeight));
        var normalPoint =
            decorMap.transform.worldToLocalMatrix.MultiplyPoint(
                new Vector3(worldPoint.x / brushInformation.decorWidth, worldPoint.y / brushInformation.decorHeight, worldPoint.z));
        x = Mathf.FloorToInt(normalPoint.x);
        x = x * brushInformation.decorWidth;
        y = Mathf.FloorToInt(normalPoint.y);
        y = y * brushInformation.decorHeight;
    }

    private static void HandleEvent(Event e, int controlID, ref bool placedecor, ref bool removedecor,
        ref bool incrementdecor, ref bool decrementdecor)
    {
        switch (e.type)
        {
            case EventType.Layout:
                HandleUtility.AddDefaultControl(controlID);
                break;
            case EventType.MouseDrag:

                if (e.button == 0 && !e.shift)
                    placedecor = true;
                if (e.button == 0 && e.shift)
                    removedecor = true;


                break;
            case EventType.MouseDown:
                if (e.button == 0 && !e.shift)
                    placedecor = true;
                if (e.button == 0 && e.shift)
                    removedecor = true;
                break;
            case EventType.KeyDown:
                if (e.keyCode == KeyCode.Alpha3)
                    incrementdecor = true;
                if (e.keyCode == KeyCode.Alpha4)
                    decrementdecor = true;
                break;

            default:
                break;
        }
    }

    private void GoToPreviousDecor()
    {
        try
        {
            if (currentDecorNumber != 0)
                currentDecorNumber--;
            else
                currentDecorNumber = decorList.Count - 1;

            brushInformation = decorMap.brushInformationList[currentDecorNumber];
            currentDecor = decorList[currentDecorNumber];
            Repaint();
        }
        catch (Exception)
        {
            Debug.Log("Please assign a prefab with children decor objects.");
        }
    }

    private void GoToNextDecor()
    {
        try
        {
            if (currentDecorNumber != decorList.Count - 1)
                currentDecorNumber++;
            else
                currentDecorNumber = 0;
            brushInformation = decorMap.brushInformationList[currentDecorNumber];

            currentDecor = decorList[currentDecorNumber];
            Repaint();
        }
        catch (Exception exception)
        {
            Debug.Log("Please assign a prefab with children decor objects.");
            Debug.Log(exception);
        }
    }

    private void CycleThroughDeleteBrush(float x, float y)
    {
        for (var i = 0.0f; i <=2* brushInformation.deleteBrushWidth+brushInformation.deleteIterations; i += brushInformation.deleteIterations)
        {
            for (var j = 0.0f; j <=2* brushInformation.deleteBrushHeight+brushInformation.deleteIterations; j += brushInformation.deleteIterations)
            {
                var n = (float)Math.Round(i, 2);
                var m = (float)Math.Round(j, 2);   
                decorMap.RemoveDecor(x + n /2 , y + m/2 );
            }
        }
    }

    private void LoadFromPrefab()
    {
        var reloadBrushInformation = true;
        if (!decorPrefab && decorMap.lastDecorPrefabUsed)
        {
            decorPrefab = decorMap.lastDecorPrefabUsed;
            reloadBrushInformation = false;
        }
        if (decorPrefab)
        {
            decorList.Clear();
            decorMap.lastDecorPrefabUsed = decorPrefab;
            if (reloadBrushInformation && decorMap.brushInformationList.Count != 0)
                decorMap.brushInformationList.Clear();
            foreach (Transform childTransform in decorPrefab.transform)
            {
                decorList.Add(childTransform.gameObject);
                if (reloadBrushInformation)
                {
                    decorMap.brushInformationList.Add(new BrushInformation());
                }
            }
            if (decorMap.brushInformationList.Count != 0)
                brushInformation = decorMap.brushInformationList[0];
            currentDecor = decorList[0];
            Repaint();
        }
    }

    private void SetValues()
    {
        if (decorMap.brushInformationList.Count >= currentDecorNumber && decorMap.brushInformationList.Count != 0)
            brushInformation = decorMap.brushInformationList[currentDecorNumber];
        currentDecor = (GameObject)EditorGUILayout.ObjectField("Current decor", currentDecor, typeof(GameObject), false);
        decorPrefab = (GameObject)EditorGUILayout.ObjectField("Decor prefab", decorPrefab, typeof(GameObject), true);
        brushInformation.decorHeight = EditorGUILayout.FloatField("Decor Height", brushInformation.decorHeight);

        brushInformation.decorWidth = EditorGUILayout.FloatField("Decor Width", brushInformation.decorWidth);


        brushInformation.randomRotation = EditorGUILayout.Slider("Random Rotation", brushInformation.randomRotation, 0.0f, 180.0f);

        brushInformation.overrideDefaultDeleteBrush = EditorGUILayout.Toggle("Use custom delete brush", brushInformation.overrideDefaultDeleteBrush);
        if (brushInformation.overrideDefaultDeleteBrush)
        {
            brushInformation.deleteBrushHeight = EditorGUILayout.Slider("Delete Brush Height", brushInformation.deleteBrushHeight, 0.01f, 5.0f);
            brushInformation.deleteBrushWidth = EditorGUILayout.Slider("Delete Brush Width", brushInformation.deleteBrushWidth, 0.01f, 5.0f);
            brushInformation.deleteIterations = EditorGUILayout.FloatField("Iterations of delete brush", brushInformation.deleteIterations);
        }
        else
        {
            brushInformation.deleteBrushHeight = brushInformation.decorHeight;
            brushInformation.deleteBrushWidth = brushInformation.decorWidth;
        }
    }

    private void DrawPlacementBox(float x, float y)
    {
        Handles.DrawLine(new Vector3(x, y), new Vector3(x + brushInformation.decorWidth, y));
        Handles.DrawLine(new Vector3(x + brushInformation.decorWidth, y), new Vector3(x + brushInformation.decorWidth, y + brushInformation.decorHeight));
        Handles.DrawLine(new Vector3(x + brushInformation.decorWidth, y + brushInformation.decorHeight), new Vector3(x, y + brushInformation.decorHeight));
        Handles.DrawLine(new Vector3(x, y + brushInformation.decorHeight), new Vector3(x, y));
    }

    private void DrawDeleteBox(float x, float y)
    {
        Handles.DrawLine(new Vector3(x, y), new Vector3(x + brushInformation.deleteBrushWidth, y));
        Handles.DrawLine(new Vector3(x + brushInformation.deleteBrushWidth, y),
            new Vector3(x + brushInformation.deleteBrushWidth, y + brushInformation.deleteBrushHeight));
        Handles.DrawLine(new Vector3(x + brushInformation.deleteBrushWidth, y + brushInformation.deleteBrushHeight),
            new Vector3(x, y + brushInformation.deleteBrushHeight));
        Handles.DrawLine(new Vector3(x, y + brushInformation.deleteBrushHeight), new Vector3(x, y));
    }

    private void ClampValues()
    {
        if (brushInformation.decorHeight < 0.01f)
            brushInformation.decorHeight = 0.01f;
        if (brushInformation.decorWidth < 0.01f)
            brushInformation.decorWidth = 0.01f;
        
        if (brushInformation.deleteIterations < 0.01f)
            brushInformation.deleteIterations = 0.01f;
    }
}
