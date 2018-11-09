using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
public class Helpers
{
	static private HashSet<UnityEngine.Object> objectsToUnload = new HashSet<UnityEngine.Object>();
	static private void UnloadResources()
	{
		if(Application.isPlaying)
			return;
 
		foreach(UnityEngine.Object obj in objectsToUnload)
			Resources.UnloadAsset(obj);
		objectsToUnload.Clear();
	}
 
	static public T LoadResourceWithAutoUnload<T>(string name) where T: UnityEngine.Object
	{
		T result = Resources.Load<T>(name);
 
#if UNITY_EDITOR
		if(result is ScriptableObject)
		{
			objectsToUnload.Add(result);
			EditorApplication.playmodeStateChanged += UnloadResources;
		}
#endif
 
		return result;
	}
}