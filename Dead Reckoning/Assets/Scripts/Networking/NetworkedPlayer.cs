using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkedPlayer : MonoBehaviour {




	// Use this for initialization
	void Start ()
	{
		if (ServerSettings.instance.playerId != 0)
		{
			ClientSideUpdates();
			ClientTCP.instance.clientPlayer = gameObject;
		}
		else
		{
			ServerTCP.instance.hostPlayer = gameObject;
		}
		
			
	}

	private void OnEnable()
	{
		DataPacket.ClientHitHandler += RecievePacketDamage;
	}

	private void OnDisable()
	{
		DataPacket.ClientHitHandler -= RecievePacketDamage;
	}

	private void ClientSideUpdates()
	{
		StartCoroutine(ReportPosition(ServerSettings.instance.TimeBetweenUpdatesClient));
	}

	// Update is called once per frame
	void Update () {
		
	}
	private void RecievePacketDamage(float damage, int clientId, int reportedPlayerId)
	{
		if (clientId != ServerSettings.instance.playerId) return;
		GetComponent<PlayerController>().TakeDamage(damage);
	}
	private IEnumerator ReportPosition(float timeBetweenUpdates)
	{
		while (true)
		{
			yield return new WaitForSeconds(timeBetweenUpdates);
			DataPacket.FromClient packet = new DataPacket.FromClient();
			SerializableVector pos = transform.position;
			packet = DataPacket.GetFromClientPositionPacket(pos, ServerSettings.instance.playerId);
			ClientTCP.instance.SendDataWithLoss(packet);
		}
	}
}
