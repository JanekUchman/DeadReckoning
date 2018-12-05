
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
	public delegate void UpdateClientPosition(Vector3 position, int clientId );
	public static event UpdateClientPosition UpdateClientPositionHandler;
	public static void RaiseUpdateClientPosition( SerializableVector pos, int id)
	{
		if (UpdateClientPositionHandler != null) UpdateClientPositionHandler.Invoke(pos, id);
	}

	public delegate void ClientFiredGun(float _angle, int _seed, Vector3 _gunPosition, int clientId );
	public static event ClientFiredGun FiredGunHandler;
	public static void RaiseClientFiredGun(float _angle, int _seed, Vector3 _gunPosition, int clientId)
	{
		if (FiredGunHandler!= null) FiredGunHandler.Invoke( _angle, _seed, _gunPosition, clientId);
	}

	public delegate void ClientHit(float _damage, int clientId);
	public static event ClientHit ClientHitHandler;
	public static void RaiseClientHit(float damage, int clientId)
	{
		if (ClientHitHandler!= null) ClientHitHandler.Invoke(damage, clientId);
	}
	//Messages sent from the client to the server
	//Hit detection is client side
	//Health packet - needs send immediately to other clients, check every x mseconds for a health change (shotgun so everytime damage is dealt would be ineffecient)
	//Movement packet - doesn't need send immediately, sent every x mseconds, updates the position
	//Shot packet - needs send immediately to other clients, on click sends the packet
	[Serializable]
	public class FromClient
	{

		public SerializableVector positionVector;
		public int playerId;
		public ServerMessages packetType;

		public float angle;
		public int seed;
		public SerializableVector gunPosition;

		public float damage;
		public int damageId;

		public FromClient(){}
		
		public DataPacket.FromClient CreatePositionPacket(SerializableVector _positionVector, int _playerId)
		{
			packetType = ServerMessages.POSITION;
			positionVector = _positionVector;
			playerId = _playerId;
			return this;
		}

		public DataPacket.FromClient CreateHealthPacket(float _damage, int _playerId)
		{
			packetType = ServerMessages.HEALTH;
			damage = _damage;
			damageId = _playerId;
			return this;
		}

		public DataPacket.FromClient CreateFireGunPacket(float _angle, int _seed, SerializableVector _gunPosition, int _playerId)
		{
			packetType = ServerMessages.FIREGUN;
			gunPosition = _gunPosition;
			angle = _angle;
			seed = _seed;
			playerId = _playerId;
			return this;
		}


	}
	//Messages sent from the server to the client
	//position update - updates all clients on the current position of other clients and server
	//health update - updates all clients on health status of clients and server
	//shot packet - updates all clients (apart from the one that sent the packet) where the player shot
	[Serializable]
	public class FromServer
	{
		
		
		public SerializableVector[] positionVectors;
		public int playerId;
		public int numberOfClients;
		public ServerMessages packetType;

		public float angle;
		public int seed;
		public SerializableVector gunPosition;
		public int shotId;

		public float damage;
		public int damageId;

		//XML needs a paramaterless constructor
		public FromServer(){}

		public DataPacket.FromServer CreatePositionPacket(SerializableVector[] _positionVectors, int _playerId)
		{
			packetType = ServerMessages.POSITION;
			positionVectors = new SerializableVector[numberOfClients];
			positionVectors = _positionVectors;
			playerId = _playerId;
			return this;
		}

		public DataPacket.FromServer CreateFireGunPacket(float _angle, int _seed, SerializableVector _gunPosition, int _playerId)
		{
			packetType = ServerMessages.FIREGUN;
			gunPosition = _gunPosition;
			angle = _angle;
			seed = _seed;
			shotId = _playerId;
			return this;
		}

		public DataPacket.FromServer CreateHealthPacket(float _damage, int _playerId)
		{
			packetType = ServerMessages.HEALTH;
			damage = _damage;
			damageId = _playerId;
			return this;
		}

		public DataPacket.FromServer CreateStartGamePacket(int _playerId, int _numberOfClients)
		{
			packetType = ServerMessages.STARTGAME;
			playerId = _playerId;
			numberOfClients = _numberOfClients;
			return this;
		}

	}
	
	

}
