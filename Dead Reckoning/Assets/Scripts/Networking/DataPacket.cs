
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Xml.Serialization;
using UnityEngine;

[Serializable]
public class DataPacket
{
	public delegate void UpdateClientPosition(Vector3 position, bool positionUpdated, int clientId );
	public static event UpdateClientPosition UpdateClientPositionHandler;
	public static void RaiseUpdateClientPosition( SerializableVector pos, bool posUpdated, int id)
	{
		if (UpdateClientPositionHandler != null) UpdateClientPositionHandler.Invoke(pos, posUpdated, id);
	}

	public delegate void ClientFiredGun(float _angle, int _seed, Vector3 _gunPosition, int clientId );
	public static event ClientFiredGun FiredGunHandler;
	public static void RaiseClientFiredGun(float _angle, int _seed, Vector3 _gunPosition, int clientId)
	{
		if (FiredGunHandler!= null) FiredGunHandler.Invoke( _angle, _seed, _gunPosition, clientId);
	}

	public delegate void ClientHit(float _damage, int clientId,  int reportedPlayerId);
	public static event ClientHit ClientHitHandler;
	public static void RaiseClientHit(float damage, int clientId, int reportedPlayerId)
	{
		if (ClientHitHandler!= null) ClientHitHandler.Invoke(damage, clientId, reportedPlayerId);
	}

	public static int byteSize = 65536;
	//Messages sent from the client to the server
	//Hit detection is client side
	//Health packet - needs send immediately to other clients, check every x mseconds for a health change (shotgun so everytime damage is dealt would be ineffecient)
	//Movement packet - doesn't need send immediately, sent every x mseconds, updates the position
	//Shot packet - needs send immediately to other clients, on click sends the packet
	[Serializable]
	public struct FromClient
	{

		public SerializableVector positionVector;
		public int playerId;
		public ServerMessages packetType;

		public float angle;
		public int seed;
		public SerializableVector gunPosition;

		public float damage;
		public int damageId;
		public int shooterId;

	}
	//Messages sent from the server to the client
	//position update - updates all clients on the current position of other clients and server
	//health update - updates all clients on health status of clients and server
	//shot packet - updates all clients (apart from the one that sent the packet) where the player shot
	[Serializable]
	public struct FromServer
	{
		public SerializableVector[] positionVectors;
		public bool[] positionUpdates;
		public int playerId;
		public int numberOfClients;
		public ServerMessages packetType;

		public float angle;
		public int seed;
		public SerializableVector gunPosition;
		public int shotId;

		public float damage;
		public int damageId;
		public int shooterId;

	}

	public static DataPacket.FromClient GetFromClientPositionPacket(SerializableVector _positionVector, int _playerId)
	{
		var packet = new FromClient();

		packet.packetType = ServerMessages.POSITION;
		packet.positionVector = _positionVector;
		packet.playerId = _playerId;
		return packet;
	}

	public static DataPacket.FromClient GetFromClientHealthPacket(float _damage, int _playerId, int _reportedPlayerId)
	{
		var packet = new FromClient();

		packet.packetType = ServerMessages.HEALTH;
		packet.damage = _damage;
		packet.damageId = _playerId;
		packet.shooterId = _reportedPlayerId;
		return packet;
	}

	public static DataPacket.FromClient GetFromClientHealthPacket(float _angle, int _seed, SerializableVector _gunPosition, int _playerId)
	{
		var packet = new FromClient();
		packet.packetType = ServerMessages.FIREGUN;
		packet.gunPosition = _gunPosition;
		packet.angle = _angle;
		packet.seed = _seed;
		packet.playerId = _playerId;
		return packet;
	}



	public static DataPacket.FromServer GetFromServerPositionPacket(SerializableVector[] _positionVectors, bool[] _positionUpdates, int _playerId)
	{
		var packet = new FromServer();
		packet.packetType = ServerMessages.POSITION;
		packet.positionVectors = new SerializableVector[ServerSettings.instance.numberOfClients];
		packet.positionVectors = _positionVectors;
		packet.playerId = _playerId;
		packet.positionUpdates = new bool[ServerSettings.instance.numberOfClients];
		packet.positionUpdates = _positionUpdates;
		return packet;
	}

	public static DataPacket.FromServer GetFromServerPositionPacket(float _angle, int _seed, SerializableVector _gunPosition, int _playerId)
	{
		var packet = new FromServer();

		packet.packetType = ServerMessages.FIREGUN;
		packet.gunPosition = _gunPosition;
		packet.angle = _angle;
		packet.seed = _seed;
		packet.shotId = _playerId;
		packet.playerId = 0;
		return packet;
	}

	public static DataPacket.FromServer GetFromServerHealthPacket(float _damage, int _playerId, int _reportedPlayerId)
	{
		var packet = new FromServer();

		packet.packetType = ServerMessages.HEALTH;
		packet.damage = _damage;
		packet.damageId = _playerId;
		packet.shooterId = _reportedPlayerId;
		return packet;
	}

	public static DataPacket.FromServer GetFromServerStartGamePacket(int _playerId, int _numberOfClients)
	{
		var packet = new FromServer();

		packet.packetType = ServerMessages.STARTGAME;
		packet.playerId = _playerId;
		packet.numberOfClients = _numberOfClients;
		return packet;
	}



}
