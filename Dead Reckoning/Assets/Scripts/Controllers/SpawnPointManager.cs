using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPointManager : MonoBehaviour
{
	public static SpawnPointManager instance;

	private Transform[] spawnPoints;
	
	// Use this for initialization
	void Start ()
	{
		if (instance) DestroyImmediate(this);
		else instance = this;

		for (int i = 0; i < transform.childCount; i++)
		{
			spawnPoints[i] = transform.GetChild(i);
		}
	}

	public Transform GetFurthestSpawn(Transform objectTransform)
	{
		var furthestDistance = 0f;
		var furthestSpawn = spawnPoints[0];
		foreach (var spawnPoint in spawnPoints)
		{
			var dist = Vector2.Distance(spawnPoint.position, objectTransform.position);

			if (dist > furthestDistance)
			{
				furthestDistance = dist;
				furthestSpawn = spawnPoint;
			}
		}

		return furthestSpawn;
	}
}
