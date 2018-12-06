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
	private TcpClient client;
	private NetworkStream stream;
	private StreamReader reader;
	private StreamWriter writer;
	public GameObject clientPlayer;
	private EnemyPlayer[] enemyPlayers;
	private DataPacket.FromServer packet;

	public static ClientTCP instance;
	
	public delegate void ClientGotId(int id);

	public static event ClientGotId ClientIdHandler;

	private void Start()
	{
		if (instance != null) DestroyImmediate(gameObject);
		else instance = this;
		DontDestroyOnLoad(gameObject);
		packet = new DataPacket.FromServer();
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
			client = new TcpClient(host, port);
			stream = client.GetStream();
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
	
	private IEnumerator WaitAFrame(Action callback)
	{
		yield return new WaitForEndOfFrame();
		callback();
	}

	private void Update()
	{
		if (socketReady)
		{
			if (stream == null) stream = client.GetStream();
			if (stream.DataAvailable)
			{
				byte[] data = new byte[DataPacket.byteSize];
				BinaryReader br = new BinaryReader(stream);
				//reader.Read();
				br.Read(data, 0, DataPacket.byteSize);

				if (data != null) StartCoroutine(OnIncomingData(data));
			}
		}
	}

	public void SendDataWithLoss(DataPacket.FromClient packet)
	{

			ArrayList listenList = new ArrayList();
			listenList.Add(client.Client);
			int waitTime = (int)ServerSettings.instance.TimeBetweenUpdatesClient * 1000000;
			Socket.Select(null, listenList, null, waitTime);

		if (!listenList.Contains(client.Client)) return;
	

		StreamWriter writer = null;
		try
		{

			NetworkStream tcpStream = client.GetStream();
			if (tcpStream.CanWrite)
			{
				Byte[] msg = Serializer.BinarySerialize(packet);
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

	public IEnumerator SendData(DataPacket.FromClient packet)
	{
		bool ready = false;
		while (!ready)
		{
			ArrayList listenList = new ArrayList();
			listenList.Add(client.Client);
			int waitTime = (int) ServerSettings.instance.TimeBetweenUpdatesClient * 1000000;
			Socket.Select(null, listenList, null, waitTime);

			if (!listenList.Contains(client.Client))
			{
				yield return new WaitForEndOfFrame();
			}
			else
			{
				ready = true;
			}
		}

		StreamWriter writer = null;
		try
		{

			NetworkStream tcpStream = client.GetStream();
			if (tcpStream.CanWrite)
			{
				Byte[] msg = Serializer.BinarySerialize(packet);
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

	private IEnumerator OnIncomingData(byte[] data)
	{
		bool ready = false;
		while (!ready)
		{
			ArrayList listenList = new ArrayList();
			listenList.Add(client.Client);
			int waitTime = (int)ServerSettings.instance.TimeBetweenUpdatesClient * 1000000;
			Socket.Select(listenList, null, null, waitTime);

			if (!listenList.Contains(client.Client))
			{
				yield return new WaitForEndOfFrame();
			}
			else
			{
				ready = true;
			}
		}

		
		try
		{
			packet = (DataPacket.FromServer) Serializer.BinaryDeserialize(data);
			switch (packet.packetType)
			{
				case ServerMessages.STARTGAME:
					StartGamePacket(packet);
					break;
				case ServerMessages.POSITION:
					for (int i = 0; i < ServerSettings.instance.numberOfClients; i++)
					{
						DataPacket.RaiseUpdateClientPosition(packet.positionVectors[i], packet.positionUpdates[i],
							i);
					}

					break;
				case ServerMessages.FIREGUN:
					DataPacket.RaiseClientFiredGun(packet.angle, packet.seed, packet.gunPosition, packet.shotId);
					break;
				case ServerMessages.HEALTH:
					DataPacket.RaiseClientHit(packet.damage, packet.damageId, packet.shooterId);
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

	private void StartGamePacket(DataPacket.FromServer packet)
	{
		SceneManager.LoadScene(1);
		ServerSettings.instance.numberOfClients = packet.numberOfClients;
		ServerSettings.instance.playerId = packet.playerId;
		SceneManager.sceneLoaded += OnSceneLoaded;
		Debug.LogFormat("Client ID: {0}", ServerSettings.instance.playerId);
	}

	private void OnSceneLoaded(Scene arg0, LoadSceneMode loadSceneMode)
	{
		enemyPlayers = new EnemyPlayer[ServerSettings.instance.numberOfClients];

		enemyPlayers = FindObjectsOfType<EnemyPlayer>();
		
	}
}
