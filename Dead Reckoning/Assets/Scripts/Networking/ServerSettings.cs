using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerSettings : MonoBehaviour
{

	public static ServerSettings instance;
	public int tickRate;

	public float timeBetweenUpdates
	{
		get { return 1 / tickRate; }
	}
	
	// Use this for initialization
	void Awake ()
	{
		if (instance != null) DestroyImmediate(this);
		else instance = this;
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
