using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class EnemyPlayer : MonoBehaviour, IDamageable
{
	private Animator playerAnimator;

	private Rigidbody2D rigidbody2D;
	
	public int characterNumber;

	[SerializeField]
	private Shotgun shotgun;

	private float maxHealth;
	private float health = 500;
	private float damageOverTime = 0;

	private bool damaged;

	private bool canTakeDamage = true;
	// Use this for initialization
	void Start ()
	{
		playerAnimator = GetComponent<Animator>();
		rigidbody2D = GetComponent<Rigidbody2D>();
	}

	private void OnEnable()
	{
		maxHealth = health;
		DataPacket.UpdateClientPositionHandler += UpdatePosition;
		DataPacket.FiredGunHandler += FireGun;
		DataPacket.ClientHitHandler += RecievePacketDamage;

		if (characterNumber >= ServerSettings.instance.numberOfClients)
		{
			DestroyImmediate(gameObject.transform.parent.gameObject);
			DestroyImmediate(this);
			return;
		}
		//This isn't our player
		else if (ServerSettings.instance.playerId != characterNumber)
		{
			shotgun.playerGun = false;
			DestroyImmediate(GetComponent<PlayerController>());
			DestroyImmediate(GetComponent<NetworkedPlayer>());
			return;
		}
		else
		{
			//Else this is our player
			Camera.main.transform.parent.GetComponent<CameraController>().UpdateCameraValues(gameObject);
			DestroyImmediate(this);
		}
		
	}

	private void OnDisable()
	{
		DataPacket.UpdateClientPositionHandler -= UpdatePosition;
		DataPacket.FiredGunHandler -= FireGun;
		DataPacket.ClientHitHandler -= RecievePacketDamage;
	}

	// Update is called once per frame
	void Update () {
		ControlAnimation();
	}

	
	private void ControlAnimation()
	{
		if (canTakeDamage)
		{
			playerAnimator.SetBool("isJumping", false);
			playerAnimator.SetBool("isDead", false);
			playerAnimator.SetBool("isRunning", (Mathf.Abs(rigidbody2D.velocity.x) > 0.2));
			playerAnimator.SetBool("isIdle", (Mathf.Abs(rigidbody2D.velocity.x) < 0.2));
		}
	}

	private void UpdatePosition( Vector3 position, int clientId)
	{
		if (clientId != characterNumber) return;
		gameObject.transform.position = position;
	}

	private void FireGun(float angle, int seed, Vector3 gunPosition, int clientId)
	{
		if (clientId != characterNumber) return;
		shotgun.SpawnAndFireBullets(angle, seed, gunPosition, clientId);
	}

	private void RecievePacketDamage(float damage, int clientId)
	{
		if (clientId != characterNumber || !canTakeDamage) return;
		health -= damage;
		UpdateHealthColour();
		if (health <= 0)
		{
			StartCoroutine(KillPlayer());
		}
	}
	private IEnumerator KillPlayer()
	{
		playerAnimator.SetBool("isDead", true);
		playerAnimator.SetBool("isIdle", false);
		canTakeDamage = false;
		yield return new WaitForSeconds(3.0f);
		canTakeDamage = true;
		health = maxHealth;
		playerAnimator.SetBool("isIdle", true);
		UpdateHealthColour();
	}

	private void UpdateHealthColour()
	{
		float healthPercentage = health / maxHealth;
		var rend = GetComponent<SpriteRenderer>();
		rend.color = new Color(rend.color.r, (healthPercentage), (healthPercentage));
	}

	public void TakeDamage(float damage)
	{
		//Problems with the client reporting damage taken, then the host repeating that
		//A temporary fix is to just check on the host
		if (ServerSettings.instance.playerId != 0) return;
		RecievePacketDamage(damage, characterNumber);
		damageOverTime += damage;
		if (!damaged)
		{
			StartCoroutine(WaitForDamagePacket());
			damaged = true;
		}
	}

	//If we get hit by a shotgun blast, wait a bit and see how many bullets hit us before reporting it
	private IEnumerator WaitForDamagePacket()
	{
		yield return new WaitForSeconds(ServerSettings.instance.TimeBetweenUpdatesClient);
		if (ServerSettings.instance.playerId != 0)
		{
			var packet = new DataPacket.FromClient();
			packet = packet.CreateHealthPacket(damageOverTime, characterNumber);
			ClientTCP.instance.SendData(packet);
		}
		else
		{
			var packet = new DataPacket.FromServer();
			packet = packet.CreateHealthPacket(damageOverTime, characterNumber);
			ServerTCP.instance.Broadcast(packet);
		}

		damaged = false;
		damageOverTime = 0;
	}
}
