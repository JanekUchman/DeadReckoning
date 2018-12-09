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

public class ServerTCP : MonoBehaviour
{
	private List<ServerClient> clients;

	//Set in inspector
	[SerializeField]
	private int maxClients = 3;
	public int port = 6321;
	private TcpListener server;
	private bool serverStarted;
	private bool startGame;
	
	public static ServerTCP instance;
	[HideInInspector]
	public GameObject hostPlayer;
	
	private void Start()
	{
		if (instance != null) DestroyImmediate(gameObject);
		else instance = this;
		DontDestroyOnLoad(gameObject);
		clients = new List<ServerClient>();

		try
		{
			//Try and start the server
			server = new TcpListener(IPAddress.Any, port);
			server.Start();

			//Prepare it to accept the clients
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
				if (stream.DataAvailable && stream.CanRead)
				{
					StartCoroutine(OnIncomingData(client));
				}
			}
		}
		//Some unity functions can only be called from the main thread, so we'll call them here
		if (startGame)
		{
			//Add one since the houst counts as a client
			var serverPacket = new DataPacket.FromServer();
			serverPacket = DataPacket.GetFromServerStartGamePacket(0, clients.Count + 1);
			StartCoroutine(Broadcast(serverPacket));
			ServerSettings.instance.numberOfClients = clients.Count+1;
			ServerSettings.instance.playerId = 0;
			SceneManager.LoadScene(1);
			SceneManager.sceneLoaded += OnSceneLoaded;
			
			startGame = false;
		}
	}

	//A constant loop that updates the clients on the position of other clients and the host
	private IEnumerator UpdateClients(float timeBetweenPackets)
	{
		while (true)
		{
			yield return new WaitForSeconds(timeBetweenPackets);
			SerializableVector[] pos = new SerializableVector[clients.Count+1];
			//
			bool[] posUpdates = new bool[clients.Count+1];
			//The first position is that of the host
			pos[0] = hostPlayer.transform.position;
			posUpdates[0] = true;

			//Get all the clients latest positions, load them into the packet along with whether that client sent in a position update
			foreach (var serverClient in clients)
			{
				pos[serverClient.clientId] = serverClient.position;
				posUpdates[serverClient.clientId] = serverClient.positionUpdated;
				serverClient.positionUpdated = false;
			}

			var serverPacket = new DataPacket.FromServer();
			serverPacket = DataPacket.GetFromServerPositionPacket(pos, posUpdates, 0);
			//We don't particularly mind if movement packets occasionally get lost
			//It'll help with network bandwidth to just send these frequently without care for them
			BroadcastWithLoss(serverPacket);
		}
	}

	void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		//Start updating the positions
		StartCoroutine(UpdateClients(ServerSettings.instance.TimeBetweenUpdatesServer));
	}

	public IEnumerator Broadcast(DataPacket.FromServer packet)
	{
		//Cycle through all clients and update them
		foreach (var client in clients)
		{
			bool ready = false;
			//Check if the socket is ready, if not wait a frame then check again
			//Consider reducing socket wait time to help frame rate on higher ping
			while (!ready)
			{
				ArrayList listenList = new ArrayList();
				listenList.Add(client.tcp.Client);
				//Second conversion to microseconds
				int waitTime = (int)ServerSettings.instance.TimeBetweenUpdatesClient * 1000000;
				Socket.Select(null, listenList, null, waitTime);
				if (!listenList.Contains(client.tcp.Client))
				{
					yield return new WaitForEndOfFrame();
				}
				else
				{
					ready = true;
				}
			}

			//Change the playerID in the packet to be the client we're sending the packet to
			packet.playerId = client.clientId;
			StreamWriter writer = null;
			NetworkStream tcpStream = client.tcp.GetStream();
			while (!tcpStream.CanWrite) yield return new WaitForEndOfFrame();
			try
			{

				if (tcpStream.CanWrite)
				{
					Byte[] msg = Serializer.BinarySerialize(packet);
					Debug.Log(msg);
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

	private IEnumerator OnIncomingData(ServerClient client)
	{
		bool ready = false;
		//Check if the socket's open for reading
		while (!ready)
		{
			ArrayList listenList = new ArrayList();
			listenList.Add(client.tcp.Client);
			//Second conversion to microseconds
			int waitTime = (int)ServerSettings.instance.TimeBetweenUpdatesClient * 1000000;
			Socket.Select(listenList, null, null, waitTime);
			if (!listenList.Contains(client.tcp.Client))
			{
				yield return new WaitForEndOfFrame();
			}
			else
			{
				ready = true;
			}
		}

		//Get the binary data from the stream
		var stream = client.tcp.GetStream();
		while (!stream.CanRead) yield return new WaitForEndOfFrame();
		byte[] data = new byte[DataPacket.byteSize];
		BinaryReader br = new BinaryReader(stream);
		br.Read(data, 0, DataPacket.byteSize);
		
		try
		{
			DataPacket.FromClient packet = (DataPacket.FromClient) Serializer.BinaryDeserialize(data);

			var serverPacket = new DataPacket.FromServer();
			switch (packet.packetType)
			{
				case ServerMessages.POSITION:
					//Don't rebroadcast the position instantly, it can wait since we might get other client updates inbetween
					DataPacket.RaiseUpdateClientPosition(packet.positionVector, true, packet.playerId);
					foreach (var serverClient in clients)
					{
						if (serverClient.clientId == packet.playerId)
						{
							serverClient.position = packet.positionVector;
							serverClient.positionUpdated = true;
						}
					}
					break;

				case ServerMessages.FIREGUN:
					DataPacket.RaiseClientFiredGun(packet.angle, packet.seed, packet.gunPosition, packet.playerId);
					//Instantly send this packet out as it's important
					serverPacket =
						DataPacket.GetFromServerPositionPacket(packet.angle, packet.seed, packet.gunPosition,
							packet.playerId);
					StartCoroutine(Broadcast(serverPacket));
					break;

				case ServerMessages.HEALTH:
					DataPacket.RaiseClientHit(packet.damage, packet.damageId, packet.shooterId);
					//Instantly send this packet out as it's important
					serverPacket = DataPacket.GetFromServerHealthPacket(packet.damage, packet.damageId, packet.shooterId);
					StartCoroutine(Broadcast(serverPacket));
					break;
				default:
					break;
			}

		}
		catch (Exception e)
		{
			Debug.LogWarning(e);
		}
		
	}

	//Used for broadcasting  positions, doesn't bother to loop if the socket isn't ready to be written to 
	//Just goes onto the next client
	//Good for keeping packet usage down
	public void BroadcastWithLoss(DataPacket.FromServer packet)
	{
		foreach (var client in clients)
		{
			
			ArrayList listenList = new ArrayList();
			listenList.Add(client.tcp.Client);
			//Second conversion to microseconds
			int waitTime = (int)ServerSettings.instance.TimeBetweenUpdatesClient * 1000000;
			Socket.Select(null, listenList, null, waitTime);
			if (!listenList.Contains(client.tcp.Client)) continue;

			packet.playerId = client.clientId;
			StreamWriter writer = null;
			try
			{

				NetworkStream tcpStream = client.tcp.GetStream();
				if (tcpStream.CanWrite)
				{
					Byte[] msg = Serializer.BinarySerialize(packet);
					Debug.Log(msg);
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

		//Add one to the player's ID since the client hasn't been added yet
		clients.Add(new ServerClient(listener.EndAcceptTcpClient(asyncResult), clients.Count+1));
		Debug.LogFormat("Client: {0} connected.", clients.Count);
		
		if (clients.Count == maxClients)
		{
			startGame = true;
		}
		else
		{
			StartListening();
		}
	}
	

	private void DeclineTcpClient(IAsyncResult asyncResult)
	{
		Debug.Log("Client tried to connect when max connected.");
	}


	//A definition of who's connected to the server
	//Only accessible by the server
	//Contains info we'd need to update other players on along with socket info
	private class ServerClient
	{
		public TcpClient tcp;
		public int clientId;
		public Vector3 position;
		public bool positionUpdated = false;

		public ServerClient(TcpClient clientSocket, int _clientId)
		{
			clientId = _clientId;
			tcp = clientSocket;
			position = Vector3.zero;
		}

	}

}

