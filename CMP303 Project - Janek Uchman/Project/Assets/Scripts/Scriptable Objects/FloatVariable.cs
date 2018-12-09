using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatVariable : ScriptableObject
{

	[SerializeField]
	private float InitialValue;

	[NonSerialized]
	public float RunTimeValue;

	public void OnAfterDeserialize()
	{
		RunTimeValue = InitialValue;
	}
}
