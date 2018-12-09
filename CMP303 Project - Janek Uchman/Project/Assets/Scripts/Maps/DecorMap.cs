using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class DecorMap : MonoBehaviour
{
    public Dictionary<string, GameObject> decor;
    public GameObject lastDecorPrefabUsed;

    public List<BrushInformation> brushInformationList;
    //TODO store lists of settings
    public bool Initialized
    {
        get
        {
            return decor != null;
        }
    }

    public void Initialize()
    {
        brushInformationList = new List<BrushInformation>();

        decor = new Dictionary<string, GameObject>();
    }

    public static string CoordToKey(float x, float y)
    {
        return x + "," + y;
    }

    public void PlaceDecor(float x, float y, float randomRotation, GameObject decorToBePlaced)
    {
        RemoveDecor(x, y);
        var key = CoordToKey(x, y);
        var newDecor = (GameObject)Instantiate(decorToBePlaced, new Vector3(x, y, 0), Quaternion.identity);
        newDecor.transform.parent = transform;
        newDecor.transform.Rotate(new Vector3(0, 0, 1), Random.Range(-randomRotation, randomRotation));

        decor[key] = newDecor;
    }
    public void RemoveDecor(float x, float y)
    {
        var key = CoordToKey(x, y);
        if (decor.ContainsKey(key))
        {
            GameObject.DestroyImmediate(decor[key]);
            decor.Remove(key);
        }
    }
}
