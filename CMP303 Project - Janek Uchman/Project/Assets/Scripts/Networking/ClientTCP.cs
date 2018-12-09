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
	public GameObject clientPlayer;

	public static ClientTCP instance;


	private void Start()
	{
		if (instance != null) DestroyImmediate(gameObject);
		else instance = this;
		DontDestroyOnLoad(gameObject);
	}
	
	//Called from button in scene
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
			var stream = client.GetStream();
			if (stream.DataAvailable )
			{
				StartCoroutine(OnIncomingData());
			}
		}
	}

	//Used for movement, we don't particularly care if a movememt packet is lost
	//Doesn't wait for the socket to be writeable, just checks once
	public void SendDataWithLoss(DataPacket.FromClient packet)
	{

		ArrayList listenList = new ArrayList();
		listenList.Add(client.Client);
		//Second conversion to microseconds
		int waitTime = (int)ServerSettings.instance.TimeBetweenUpdatesClient * 1000000;
		Socket.Select(null, listenList, null, waitTime);

		//If that socket isn't writeable, return
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
		//Check if the socket is ready, if not wait a frame then check again
		//Consider reducing socket wait time to help frame rate on higher ping
		while (!ready)
		{
			ArrayList listenList = new ArrayList();
			listenList.Add(client.Client);
			//Second conversion to microseconds
			int waitTime = (int)ServerSettings.instance.TimeBetweenUpdatesClient * 1000000;
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
		NetworkStream tcpStream = client.GetStream();
		
		while (!tcpStream.CanWrite) yield return new WaitForEndOfFrame();
		try
		{

			if (tcpStream.CanWrite)
			{
				//Convert the object to binary data
				Byte[] msg = Serializer.BinarySerialize(packet);
				//Write the binary to the network stream, then dispose of unnecessary data
				tcpStream.Write(msg, 0,msg.Length);
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

	private IEnumerator OnIncomingData()
	{
		bool ready = false;
		//Check if the socket is ready, if not wait a frame then check again
		//Consider reducing socket wait time to help frame rate on higher ping
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
		var stream = client.GetStream();

		while (!stream.CanRead) yield return new WaitForEndOfFrame();
		byte[] data = new byte[DataPacket.byteSize];
		//Get the binary data from the network stream
		BinaryReader br = new BinaryReader(stream);
		//Read it into data
		br.Read(data, 0, DataPacket.byteSize);
		
		try
		{
			var packet = new DataPacket.FromServer();
			//Deserialize the binary back into a DataPacket type
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
		Debug.LogFormat("Client ID: {0}", ServerSettings.instance.playerId);
	}
}
