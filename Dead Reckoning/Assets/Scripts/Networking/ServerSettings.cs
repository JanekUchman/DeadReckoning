﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerSettings : MonoBehaviour
{

	public static ServerSettings instance;
	[SerializeField]
	private int clientTickRate;
	[SerializeField]
	private int serverTickRate;
	public int numberOfClients;
	public int playerId;

	public float TimeBetweenUpdatesClient
	{
		get { return 1 / clientTickRate; }
	}

	public float TimeBetweenUpdatesServer
	{
		get { return 1 / serverTickRate; }
	}


	// Use this for initialization
	void Awake ()
	{
		if (instance != null) DestroyImmediate(this);
		else instance = this;
		DontDestroyOnLoad(gameObject);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
