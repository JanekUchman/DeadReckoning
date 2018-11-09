using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.Rendering;

public class GunController : MonoBehaviour
{

	public int ammo;
	public static GunController instance;

	private void Awake()
	{
		if (instance == null)
		{
			instance = this;
		}
		else
		{
			DestroyImmediate(this);
		}
	}
}
