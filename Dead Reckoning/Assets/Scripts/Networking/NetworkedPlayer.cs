using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkedPlayer : MonoBehaviour {


	[HideInInspector]
	public int Id;

	// Use this for initialization
	void Start ()
	{
		if (Id != 0)
		{
			ClientSideUpdates();
		}
		
			
	}

	private void ClientSideUpdates()
	{
		StartCoroutine(ReportPosition(ServerSettings.instance.timeBetweenUpdates));
	}

	// Update is called once per frame
	void Update () {
		
	}
	
	private IEnumerator ReportPosition(float timeBetweenUpdates)
	{
		while (true)
		{
			yield return new WaitForSeconds(timeBetweenUpdates);
			DataPacket.FromClient packet = new DataPacket.FromClient();
			SerializableVector pos = transform.position;
			packet = packet.CreatePositionPacket(pos, Id);
			ClientTCP.instance.SendData(packet);
		}
	}
}
