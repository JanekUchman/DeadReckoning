using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ClientTCP : MonoBehaviour
{

	private bool socketReady = false;
	private TcpClient socket;
	private NetworkStream stream;
	private StreamReader reader;
	private StreamWriter writer;

	private void Start()
	{
		DontDestroyOnLoad(gameObject);
	}
	
	//Called from button
	public void ConnectToServer()
	{
		if (socketReady) return;
		
		//default host and port values
		string host = "127.0.0.1";
		int port = 6321;

		//Overwrite defaults if there's something in input
		string h;
		int p;
		h = GameObject.Find("IP Input").GetComponent<InputField>().text;
		if (h != "") host = h;

		int.TryParse(GameObject.Find("Port Input").GetComponent<InputField>().text, out p);
		if (p != 0) port = p;
		
		//Create the socket
		try
		{
			socket = new TcpClient(host, port);
			stream = socket.GetStream();
			writer = new StreamWriter( stream);
			reader = new StreamReader(stream, Encoding.ASCII);
			socketReady = true;

		}
		catch (Exception e)
		{
			Debug.Log("Socket error: " +e.Message);
			throw;
		}
	}

	private void Update()
	{
		if (socketReady)
		{
			if (stream.DataAvailable)
			{
				byte[] data = new byte[1024];
				BinaryReader br = new BinaryReader(stream);
				//reader.Read();
				br.Read(data, 0, 1024);

				if (data != null) OnIncomingData(data);
			}
		}
	}

	private void OnIncomingData(byte[] data)
	{
		Debug.Log(data);
		DataPacket packet = Serializer.BinaryDeserialize(data);
		Debug.Log("Server: "+packet.packetType);

		switch (packet.packetType)
		{
			case NetworkMessages.STARTGAME:
				SceneManager.LoadScene(1);
				break;
			default:
				break;
		}
	}
}
