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
	private EnemyPlayer[] enemyPlayers;

	public static ServerTCP instance; 
	
	private void Start()
	{
		if (instance != null) DestroyImmediate(gameObject);
		else instance = this;
		
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
					byte[] data = new byte[1024];
					BinaryReader br = new BinaryReader(stream);
					//reader.Read();
					br.Read(data, 0, 1024);

					if (data != null) OnIncomingData(client, data);
				}
			}
		}
		//Some unity functions can only be called from the main thread, so call them here
		if (startGame)
		{
			SceneManager.LoadScene(1);
			SceneManager.sceneLoaded += OnSceneLoaded;
			
			startGame = false;
		}
	}

	void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		enemyPlayers = new EnemyPlayer[maxClients];
		enemyPlayers = FindObjectsOfType<EnemyPlayer>();
		foreach (var enemyPlayer in enemyPlayers)
		{
			enemyPlayer.ProcessClientId(0, maxClients);
		}
	}

	private void OnIncomingData(ServerClient client, byte[] data)
	{
		DataPacket.FromClient packet = (DataPacket.FromClient)Serializer.BinaryDeserialize(data);
		Debug.Log("Client: "+packet.packetType);
		switch (packet.packetType)
		{
			case ServerMessages.POSITION:
				DataPacket.RaiseUpdateClientPosition(packet.playerId, packet.positionVector);
				break;
			default:
				break;
		}
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
		clients.Add(new ServerClient(listener.EndAcceptTcpClient(asyncResult), clients.Count+1));

		if (clients.Count == maxClients)
		{
			StartGame();
		}
		else
		{
			StartListening();
		}
	}

	private void StartGame()
	{
		
		DataPacket.FromServer packet = new DataPacket.FromServer();
		foreach (var client in clients)
		{
			//Let the clients know what their ID is and how many other clients to care about
			packet = packet.CreateStartGamePacket(client.clientId, clients.Count+1);
			SendPacket(packet, client);
		}
		startGame = true;
	}

	private void DeclineTcpClient(IAsyncResult asyncResult)
	{
		Debug.Log("Client tried to connect when max connected.");
	}

	private void Broadcast(DataPacket.FromServer packet, List<ServerClient> _clients)
	{
		
		foreach (var client in _clients)
		{
			StreamWriter writer = null;
			try
			{

				NetworkStream tcpStream = client.tcp.GetStream();
				if (tcpStream.CanWrite)
				{
					Byte[] msg = Serializer.BinarySerialize(packet);
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

	private void SendPacket(DataPacket.FromServer packet, ServerClient client)
	{
		StreamWriter writer = null;
		try
		{

			NetworkStream tcpStream = client.tcp.GetStream();
			if (tcpStream.CanWrite)
			{
				//Let the clients know what their ID is and how many other clients to care about
				Byte[] msg = Serializer.BinarySerialize(packet);
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

//A definition of who's connected to the server
//Only accessible by the server
public class ServerClient
{
	public TcpClient tcp;
	public int clientId;

	public ServerClient(TcpClient clientSocket, int _clientId)
	{
		clientId = _clientId;
		tcp = clientSocket;
	}

}
