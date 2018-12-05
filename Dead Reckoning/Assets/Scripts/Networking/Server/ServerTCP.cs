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
using Vector3 = UnityEngine.Vector3;

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
	private DataPacket.FromServer serverPacket;
	
	public static ServerTCP instance;
	public GameObject hostPlayer;
	
	private void Start()
	{
		if (instance != null) DestroyImmediate(gameObject);
		else instance = this;
		
		serverPacket = new DataPacket.FromServer();

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
			ServerSettings.instance.numberOfClients = clients.Count+1;
			ServerSettings.instance.playerId = 0;
			SceneManager.LoadScene(1);
			SceneManager.sceneLoaded += OnSceneLoaded;
			
			startGame = false;
		}
	}

	private IEnumerator UpdateClients(float timeBetweenPackets)
	{
		while (true)
		{
			yield return new WaitForSeconds(timeBetweenPackets);
			SerializableVector[] pos = new SerializableVector[clients.Count+1];
			pos[0] = hostPlayer.transform.position;
			foreach (var serverClient in clients)
			{
				pos[serverClient.clientId] = serverClient.position;
			}

			serverPacket = serverPacket.CreatePositionPacket(pos, 0);
			Broadcast(serverPacket);
		}
	}

	void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		enemyPlayers = new EnemyPlayer[maxClients];
		enemyPlayers = FindObjectsOfType<EnemyPlayer>();
		StartCoroutine(UpdateClients(ServerSettings.instance.TimeBetweenUpdatesServer));
	}

	private void OnIncomingData(ServerClient client, byte[] data)
	{
		DataPacket.FromClient packet = (DataPacket.FromClient)Serializer.BinaryDeserialize(data);
		if (packet == null) return;
		Debug.Log("Client: "+packet.packetType);
		switch (packet.packetType)
		{
			case ServerMessages.POSITION:
				DataPacket.RaiseUpdateClientPosition( packet.positionVector, packet.playerId);
				foreach (var serverClient in clients)
				{
					if (serverClient.clientId == packet.playerId)
					{
						serverClient.position = packet.positionVector;
					}
				}
				break;
			case ServerMessages.FIREGUN:
				DataPacket.RaiseClientFiredGun(packet.angle, packet.seed, packet.gunPosition, packet.playerId);
				serverPacket =
					serverPacket.CreateFireGunPacket(packet.angle, packet.seed, packet.gunPosition, packet.playerId);
				Broadcast(serverPacket);
				break;
			case ServerMessages.HEALTH:
				DataPacket.RaiseClientHit(packet.damage, packet.damageId);
				serverPacket = serverPacket.CreateHealthPacket(packet.damage, packet.damageId);
				Broadcast(serverPacket);
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
		//Add one since the client hasn't been added yet
		clients.Add(new ServerClient(listener.EndAcceptTcpClient(asyncResult), clients.Count+1));
		Debug.LogFormat("Client: {0} connected.", clients.Count);
		
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
		//Add one since the houst counts as a client
		serverPacket = serverPacket.CreateStartGamePacket(0, clients.Count+1);
		Broadcast(serverPacket);
		startGame = true;
	}

	private void DeclineTcpClient(IAsyncResult asyncResult)
	{
		Debug.Log("Client tried to connect when max connected.");
	}

	public void Broadcast(DataPacket.FromServer packet)
	{
		
		foreach (var client in clients)
		{
			packet.playerId = client.clientId;
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

	//A definition of who's connected to the server
	//Only accessible by the server
	private class ServerClient
	{
		public TcpClient tcp;
		public int clientId;
		public Vector3 position;

		public ServerClient(TcpClient clientSocket, int _clientId)
		{
			clientId = _clientId;
			tcp = clientSocket;
			position = Vector3.zero;
		}

	}

}

