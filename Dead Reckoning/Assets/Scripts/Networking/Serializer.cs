using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Xml.Serialization;
using UnityEngine;

public class Serializer : MonoBehaviour {

	
	public static DataPacket DeSerialize(string xmlString)
	{
		XmlSerializer xmlSerializer;
		MemoryStream memStream = null;
		try
		{
			xmlSerializer = new XmlSerializer(typeof(DataPacket));
			byte[] bytes = new byte[xmlString.Length];
			Encoding.ASCII.GetBytes(xmlString, 0, xmlString.Length, bytes, 0);
			memStream = new MemoryStream(bytes);
			object objectFromXml = xmlSerializer.Deserialize(memStream);
			DataPacket a = (DataPacket)objectFromXml;
			return a;

		}
		catch (Exception Ex)
		{
			throw;
		}
		finally
		{
			if (memStream != null)
				memStream.Close();
		}

		return null;
	}
	
	public static string Serialize(DataPacket packet)
	{
		StreamWriter stWriter = null;
		XmlSerializer xmlSerializer;
		string buffer;
		try
		{
			xmlSerializer = new XmlSerializer(typeof(DataPacket));
			MemoryStream memStream = new MemoryStream();
			stWriter = new StreamWriter(memStream, Encoding.ASCII);
			System.Xml.Serialization.XmlSerializerNamespaces xs = new XmlSerializerNamespaces();
			
			xmlSerializer.Serialize(stWriter, packet, xs);
			buffer = Encoding.ASCII.GetString(memStream.GetBuffer());
		}
		catch (Exception e)
		{
			throw;
		}
		finally
		{
			if (stWriter != null)
				stWriter.Close();
		}
		return buffer;

	}

	public static byte[] BinarySerialize(object packet)
	{
		// To serialize the hashtable and its key/value pairs,  
		// you must first open a stream for writing. 
		// In this case, use a file stream.
		MemoryStream stream = new MemoryStream();

		// Construct a BinaryFormatter and use it to serialize the data to the stream.
		BinaryFormatter formatter = new BinaryFormatter();
		try 
		{
			formatter.Serialize(stream, packet);
		}
		catch (SerializationException e) 
		{
			Console.WriteLine("Failed to serialize. Reason: " + e.Message);
			throw;
		}
		finally 
		{
			stream.Close();
		}
		return stream.ToArray();
	}

	public static object BinaryDeserialize(byte[] data)
	{
		object packet = null;
		MemoryStream stream = new MemoryStream();
		stream.Write(data, 0, data.Length);
		stream.Seek(0, SeekOrigin.Begin);
		BinaryFormatter formatter = new BinaryFormatter();
		try
		{
			packet = formatter.Deserialize(stream);
		}
		catch (SerializationException e)
		{
			Debug.Log("Deserialization Failed : " + e.Message);
		}
		stream.Close();
		return packet;
	}
	

}
