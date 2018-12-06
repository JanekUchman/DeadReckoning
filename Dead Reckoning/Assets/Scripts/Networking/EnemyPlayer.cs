﻿using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class EnemyPlayer : MonoBehaviour, IDamageable
{
	private enum AnimationState
	{
		MOVING,
		IDLE,
		JUMPING,
		DEAD
	};

	private AnimationState animationState;
	private Animator playerAnimator;
	private Renderer rend;

	private Rigidbody2D rigidbody2D;
	
	public int characterNumber;

	[SerializeField]
	private Shotgun shotgun;

	private float maxHealth;
	private float health = 500;
	private float damageOverTime = 0;
	private bool damaged;
	private bool flippedLeft;
	private bool positionUpdateReceived;
	private Vector3 targetPosition;
	private Vector3 oldestPacketPosition;
	private Vector3 latestReceivedPacketPosition;


	// Use this for initialization
	void Start ()
	{
		playerAnimator = GetComponent<Animator>();
		rigidbody2D = GetComponent<Rigidbody2D>();
		rend = GetComponent<Renderer>();
		oldestPacketPosition = transform.position;
		targetPosition = transform.position;
		latestReceivedPacketPosition = transform.position;
	}

	private void OnEnable()
	{
		maxHealth = health;
		DataPacket.UpdateClientPositionHandler += UpdatePosition;
		DataPacket.FiredGunHandler += FireGun;
		DataPacket.ClientHitHandler += ReceivePacketDamage;

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
			GetComponent<Rigidbody2D>().gravityScale = 0;
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
		DataPacket.ClientHitHandler -= ReceivePacketDamage;
	}

	// Update is called once per frame
	void Update () {
		GetAnimationState();
		ControlAnimation();
		var step = ServerSettings.instance.speed * Time.deltaTime;
		
		transform.position = Vector2.MoveTowards(transform.position, targetPosition, step);
		
	}

	private IEnumerator PacketTimer()
	{
		while (true)
		{
			yield return new WaitForSeconds(ServerSettings.instance.TimeBetweenUpdatesClient);
			MakePrediction();
		}
	}

	private void UpdatePosition( Vector3 position, bool posUpdated, int clientId)
	{
		if (clientId != characterNumber) return;
		positionUpdateReceived = posUpdated;
		if (!posUpdated)
		{
			MakePrediction();
		}
		else
		{
			StopCoroutine(PacketTimer());
			StartCoroutine(PacketTimer());
			oldestPacketPosition = latestReceivedPacketPosition;
			latestReceivedPacketPosition = position;
			targetPosition = position;
			//If there's too big a difference just move the player
			if (Vector2.Distance(position, transform.position) > 5)
			{
				transform.position = position;
				oldestPacketPosition = position;
			}
		}
		
	}

	private void MakePrediction()
	{
		var direction = (latestReceivedPacketPosition - oldestPacketPosition);
		//Normalize it just in case there's a large time between packets and the size gets too big
		//targetPosition = transform.position += direction.normalized;
		targetPosition = transform.position + direction;
	}

	private void FireGun(float angle, int seed, Vector3 gunPosition, int clientId)
	{
		if (clientId != characterNumber) return;
		shotgun.SpawnAndFireBullets(angle, seed, gunPosition, clientId, false);
	}

	private void ReceivePacketDamage(float damage, int clientId, int reportedPlayerId)
	{
		if (clientId != characterNumber || reportedPlayerId == ServerSettings.instance.playerId) return;
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
		yield return new WaitForSeconds(3.0f);
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
		//ReceivePacketDamage(damage, characterNumber);
		if (!damaged)
		{
			damageOverTime = damage;
			StartCoroutine(WaitForDamagePacket());
			damaged = true;
		}
		else
		{
			damageOverTime += damage;
		}
	}

	//If we get hit by a shotgun blast, wait a bit and see how many bullets hit us before reporting it
	private IEnumerator WaitForDamagePacket()
	{
		yield return new WaitForSeconds(ServerSettings.instance.TimeBetweenUpdatesClient);

		if (ServerSettings.instance.playerId != 0)
		{
			ReceivePacketDamage(damageOverTime, characterNumber, -1);
			var packet = new DataPacket.FromClient();
			packet = DataPacket.GetFromClientHealthPacket(damageOverTime, characterNumber, ServerSettings.instance.playerId);
			StartCoroutine(ClientTCP.instance.SendData(packet));
		}
		else
		{
			ReceivePacketDamage(damageOverTime, characterNumber, -1);
			var packet = new DataPacket.FromServer();
			packet = DataPacket.GetFromServerHealthPacket(damageOverTime, characterNumber, 0);
			StartCoroutine(ServerTCP.instance.Broadcast(packet));
		}
		damaged = false;

	}

	private void GetAnimationState()
	{
		if (targetPosition.x < transform.position.x && !flippedLeft)
		{
			flippedLeft = true;
			transform.localScale = new Vector3(-1, transform.localScale.y, transform.localScale.z);
		}
		else if (targetPosition.x > transform.position.x && flippedLeft)
		{
			flippedLeft = false;
			transform.localScale = new Vector3(1, transform.localScale.y, transform.localScale.z);
		}
		if (targetPosition != transform.position)
		{
			animationState = AnimationState.MOVING;
		}
		else
		{
			animationState = AnimationState.IDLE;
		}

		if (CharacterFunctions.GroundCheck(new Vector2(rend.bounds.min.x, rend.bounds.max.y),
			new Vector2(rend.bounds.max.x, rend.bounds.max.y + 0.1f)))
		{
			animationState = AnimationState.JUMPING;
		}

		if (health <= 0)
		{
			animationState = AnimationState.DEAD;
		}
	}


	private void ControlAnimation()
	{
		playerAnimator.SetBool("isJumping", false);
		playerAnimator.SetBool("isDead", false);
		playerAnimator.SetBool("isRunning", false);
		playerAnimator.SetBool("isIdle", false);
		switch (animationState)
		{
			case AnimationState.MOVING:
				playerAnimator.SetBool("isRunning", true);
				break;
			case AnimationState.IDLE:
				playerAnimator.SetBool("isIdle", true);
				break;
			case AnimationState.DEAD:
				playerAnimator.SetBool("isDead", true);
				break;
			case AnimationState.JUMPING:
				playerAnimator.SetBool("isJumping", true);
				break;
		}

	}

}
