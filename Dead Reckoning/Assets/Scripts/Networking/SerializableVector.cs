using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Taken from: https://answers.unity.com/questions/956047/serialize-quaternion-or-vector3.html
[System.Serializable]
public struct SerializableVector
{
	/// <summary>
	/// x component
	/// </summary>
	public float x;
     
	/// <summary>
	/// y component
	/// </summary>
	public float y;
     
	/// <summary>
	/// z component
	/// </summary>
	public float z;
     
	/// <summary>
	/// Constructor
	/// </summary>
	/// <param name="rX"></param>
	/// <param name="rY"></param>
	/// <param name="rZ"></param>
	public SerializableVector(float rX, float rY, float rZ)
	{
		x = rX;
		y = rY;
		z = rZ;
	}
     
	/// <summary>
	/// Returns a string representation of the object
	/// </summary>
	/// <returns></returns>
	public override string ToString()
	{
		return String.Format("[{0}, {1}, {2}]", x, y, z);
	}
     
	/// <summary>
	/// Automatic conversion from SerializableVector3 to Vector3
	/// </summary>
	/// <param name="rValue"></param>
	/// <returns></returns>
	public static implicit operator Vector3(SerializableVector rValue)
	{
		return new Vector3(rValue.x, rValue.y, rValue.z);
	}
     
	/// <summary>
	/// Automatic conversion from Vector3 to SerializableVector3
	/// </summary>
	/// <param name="rValue"></param>
	/// <returns></returns>
	public static implicit operator SerializableVector(Vector3 rValue)
	{
		return new SerializableVector(rValue.x, rValue.y, rValue.z);
	}
}
