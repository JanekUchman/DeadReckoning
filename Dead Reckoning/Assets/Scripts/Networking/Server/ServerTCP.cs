using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.SceneManagement;

// https://www.youtube.com/watch?v=7_BCbzRMi2w 

public class ServerTCP : MonoBehaviour
{
	private List<ServerClient> clients;
	private List<ServerClient> disconnectList;

	[SerializeField]
	private int maxClients = 3;
	public int port = 6321;
	private TcpListener server;
	private bool serverStarted;
	private bool startGame;
	
	private void Start()
	{
		DontDestroyOnLoad(gameObject);
		clients = new List<ServerClient>();
		disconnectList = new List<ServerClient>();

		try
		{
			server = new TcpListener(IPAddress.Any, port);
			server.Start();

			StartListening();
			serverStarted = true;
			Debug.Log("Server has started on port: " + port);
		}
		catch (Exception e)
		{
			Debug.Log("Socket Error: "+ e.Message);
			throw;
		}
	}

	private void Update()
	{
		if (!serverStarted) return;

		foreach (var client in clients)
		{
			//is the client still connected
			//if so check for messages
			if (IsConnected(client.tcp))
			{
				NetworkStream stream = client.tcp.GetStream();
				//Is there something to be read?
				if (stream.DataAvailable)
				{
					//Read it from the client
					StreamReader reader = new StreamReader(stream, true);
					
					string data = reader.ReadLine();

					if (data != null) OnIncomingData(client, data);
				}
			}
		}

		if (startGame)
		{
			SceneManager.LoadScene(1);
			startGame = false;
		}
	}

	private void OnIncomingData(ServerClient client, string data)
	{
		Debug.Log(client.clientName + "has sent a message: " + data);
	}

	private bool IsConnected(TcpClient clientTcp)
	{
		//it's possible we can't reach the client
		try
		{
			if (clientTcp != null && clientTcp.Client != null && clientTcp.Client.Connected)
			{
				//Select the port so the program doesn't freeze waiting for comms
				if (clientTcp.Client.Poll(0, SelectMode.SelectRead))
				{
					//Check if there's a client error
					return clientTcp.Client.Receive(new byte[1], SocketFlags.Peek) != 0;
				}
			}
		}
		catch (Exception e)
		{
			Console.WriteLine(e);
			throw;
		}

		return false;
	}

	private void StartListening()
	{
		server.BeginAcceptTcpClient(AcceptTcpClient, server);
	}

	private void AcceptTcpClient(IAsyncResult asyncResult)
	{
		if (clients.Count == maxClients)
		{
			Debug.LogWarning("Too many clients attempted to connect.");
			return;
		}
		TcpListener listener = (TcpListener) asyncResult.AsyncState;
		clients.Add(new ServerClient(listener.EndAcceptTcpClient(asyncResult)));
		StartListening();
		
		if (clients.Count == maxClients) StartGame();
	}

	private void StartGame()
	{
		var msg = new DataPacket(NetworkMessages.STARTGAME);
		Broadcast(msg, clients);
		startGame = true;
	}

	private void Broadcast(DataPacket data, List<ServerClient> _clients)
	{
		
		foreach (var client in _clients)
		{
			StreamWriter writer = null;
			try
			{
//				Debug.Log("Message sent: " + data.message);
//				//https://social.msdn.microsoft.com/Forums/en-US/ae005637-65fc-482f-bfee-267e85f709d1/how-to-send-an-object-through-network-using-tcp-sockets?forum=netfxnetcom
//				XmlSerializer xmlSerializer = new XmlSerializer(typeof(DataPacket));
//				;
//				MemoryStream memStream = new MemoryStream();
//				string buffer;
//				writer = new StreamWriter(client.tcp.GetStream());
//				XmlSerializerNamespaces xs = new XmlSerializerNamespaces();
//				xs.Add("", "");
//				xmlSerializer.Serialize(writer, data, xs);
//
//				buffer = Encoding.ASCII.GetString(memStream.GetBuffer());
//				Char[] inputToBeSent = buffer.ToCharArray();
//				
//				
//				writer.Write(inputToBeSent, 0, inputToBeSent.Length);
//				writer.Flush();

				NetworkStream tcpStream = client.tcp.GetStream();
				if (tcpStream.CanWrite)
				{
					
					Byte[] msg = Serializer.BinarySerialize(data);
					Debug.Log(msg);
					//Byte[] inputToBeSent = Encoding.ASCII.GetBytes(msg.ToCharArray());
					tcpStream.Write(msg, 0, msg.Length);
					tcpStream.Flush();
				}

			}
			catch (Exception e)
			{
				Debug.LogError(e);
				throw;
			}
			finally
			{
				if (writer != null) writer.Close();
			}
		}
	}
}

//A definition of who's connected to the server
//Only accessible by the server
public class ServerClient
{
	public TcpClient tcp;
	public string clientName;

	public ServerClient(TcpClient clientSocket)
	{
		clientName = "guest";
		tcp = clientSocket;
	}

}
