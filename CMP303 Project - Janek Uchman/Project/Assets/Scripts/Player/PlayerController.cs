﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour, IDamageable {

	public enum PlayerState
	{
		GROUNDED,
		IN_AIR,
		DEAD
	}

	public PlayerState playerState;
	
    [Header ("Groundcheck")]
	[SerializeField] private float lengthToSearch = 0.2f; //distance to check for ground
    [Header ("Jump variables")]
    [SerializeField] private float jumpSpeed = 8; 
    [Header ("Damage variables")]
	[SerializeField] private float health = 500.0f;

	private float maxHealth;
	[SerializeField] private float invulnerabilityTimer = 0.3f;

	private int[] layerMask =  new int[3] { 8, 9, 11 };
	private Rigidbody2D rigidbody2D;
	private Animator playerAnimator = null;
	private BoxCollider2D playerBoxCollider = null;
	private bool canAirJump = true;

	[HideInInspector] public float horizontal = 0;
    [Header("Movement variables")]
	[SerializeField] private float maxSpeed = 10;
	[SerializeField] private float acceleration = 50;

	public event EventHandler<EventArgs> PlayerDeath;
	public event EventHandler<EventArgs> PlayerRespawn;

	void Start()
	{
		rigidbody2D = gameObject.GetComponent<Rigidbody2D>();
		playerAnimator = GetComponent<Animator>();
		playerBoxCollider = GetComponent<BoxCollider2D>();
		maxHealth = health;
	}

    void Update()
    {
	    horizontal = Input.GetAxis("Horizontal");
	    switch (playerState)
	    {
			    case PlayerState.GROUNDED:
				    canAirJump = true;
				    FlipTransform();
				    CheckJump();
					Movement();
				    CheckGrounded();
				    PlayerMovement();
				    break;
			    
			    case PlayerState.IN_AIR:
				    CheckAirJump();
				    FlipTransform();
				    CheckGrounded();
				    Movement();
				    break;
			    case PlayerState.DEAD:
				    break;
	    }
    }

	private void CheckJump()
	{
		if (Input.GetKeyDown("space"))
		{
			Jump();
		}
	}

	private void CheckAirJump()
	{
		if (canAirJump && Input.GetKeyDown("space"))
		{
			canAirJump = false;
			Jump();
		}
	}

	private void FlipTransform()
	{
		if (horizontal < -0.1f)
		{
			if (rigidbody2D.velocity.x < -0.1f)
			{
				transform.localScale = new Vector3(-1, transform.localScale.y, transform.localScale.z);
			}
		}
		if (horizontal > 0.1f)
		{
			if (rigidbody2D.velocity.x > 0.1f)
			{
				transform.localScale = new Vector3(1, transform.localScale.y, transform.localScale.z);
			}
		}
	}
	
    private void CheckGrounded()
    {
	    
        var colliderThreshold = 0.05f;
        var topLeftOfGroundCheck = new Vector2(playerBoxCollider.bounds.min.x, playerBoxCollider.bounds.min.y - colliderThreshold);
        var bottomRightOfGroundCheck = new Vector2(playerBoxCollider.bounds.max.x, playerBoxCollider.bounds.min.y - lengthToSearch);
	    
	    playerState = !CharacterFunctions.GroundCheck(topLeftOfGroundCheck, bottomRightOfGroundCheck) ? PlayerState.IN_AIR : PlayerState.GROUNDED;
    }

	private void PlayerMovement()
	{
		playerAnimator.SetBool("isJumping", false);
		playerAnimator.SetBool("isDead", false);
		playerAnimator.SetBool("isRunning", (Mathf.Abs(rigidbody2D.velocity.x) > 0.2));
		playerAnimator.SetBool("isIdle", (Mathf.Abs(rigidbody2D.velocity.x) < 0.2));
	}
	

    private void Movement()
    {
	    if (Mathf.Abs(rigidbody2D.velocity.x) > maxSpeed)
	    {
		    rigidbody2D.AddForce(new Vector2(acceleration * horizontal * Time.deltaTime, 0.0f));
	    }
	    else
	    {
		    rigidbody2D.velocity = new Vector2(maxSpeed * horizontal, rigidbody2D.velocity.y); //if the velocity is higher than the max speed set it to the max speed
	    }
    }

    private void Jump()
    {
        playerAnimator.SetBool("isJumping", true);
	    playerAnimator.SetBool("isRunning", false);
	    playerAnimator.SetBool("isIdle", false);
        rigidbody2D.velocity = new Vector2(rigidbody2D.velocity.x, jumpSpeed);

    }

	public void TakeDamage(float damage)
	{
		health -=damage;
		UpdateHealthColour();
		if (health <= 0) { StartCoroutine(KillPlayer()); }
	}

	private void UpdateHealthColour()
	{
		float healthPercentage = health/maxHealth;
		var rend = GetComponent<SpriteRenderer>();
		rend.color = new Color(rend.color.r, (healthPercentage),  (healthPercentage));
	}
	
	private void RespawnPlayer(Vector2 position)
	{
		health = maxHealth;
		playerAnimator.SetBool("isIdle", true);
		UpdateHealthColour();
		transform.position = position;
		playerState = PlayerState.GROUNDED;
		if (PlayerRespawn != null) PlayerRespawn(this, EventArgs.Empty);
	}

	private IEnumerator KillPlayer()
	{
		if (PlayerDeath != null) PlayerDeath(this, EventArgs.Empty);
		
		playerAnimator.SetBool("isDead", true);
		playerAnimator.SetBool("isIdle", false);
		playerState = PlayerState.DEAD;
		yield return new WaitForSeconds(3.0f);
		var spawn = SpawnPointManager.instance.GetFurthestSpawn(transform);
		RespawnPlayer(spawn.position);
	}


}