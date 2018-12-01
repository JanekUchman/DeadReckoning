
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Xml.Serialization;
using UnityEngine;

[Serializable]
public class DataPacket  {


	public DataPacket(NetworkMessages _packetType)
	{
		packetType = _packetType;
	}

	//XML needs a paramaterless constructor
	// https://stackoverflow.com/questions/267724/why-xml-serializable-class-need-a-parameterless-constructor
	private DataPacket()
	{
		
	}
	
	public NetworkMessages packetType;
	

}
