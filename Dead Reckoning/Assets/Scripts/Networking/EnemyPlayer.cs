using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class EnemyPlayer : MonoBehaviour
{
	private Animator playerAnimator;

	private Rigidbody2D rigidbody2D;
	
	[SerializeField]
	private int characterNumber;

	[SerializeField]
	private Shotgun shotgun;

	// Use this for initialization
	void Start ()
	{
		playerAnimator = GetComponent<Animator>();
		rigidbody2D = GetComponent<Rigidbody2D>();
	}

	private void OnEnable()
	{
		DataPacket.UpdateClientPositionHandler += UpdatePosition;
	}

	private void OnDisable()
	{
		DataPacket.UpdateClientPositionHandler -= UpdatePosition;
	}

	// Update is called once per frame
	void Update () {
		PlayerMovement();
	}

	public void ProcessClientId(int id, int numberOfClients)
	{
		if (id > numberOfClients) DestroyImmediate(gameObject.transform.parent);
		//This isn't our player
		if (id != characterNumber)
		{
			shotgun.playerGun = false;
			GetComponent<PlayerController>().enabled = false;
			GetComponent<NetworkedPlayer>().enabled = false;
			return;
		}
		
		//Else this is our player
		GetComponent<NetworkedPlayer>().Id = characterNumber;
		Camera.main.transform.parent.GetComponent<CameraController>().UpdateCameraValues(gameObject);
		GetComponent<EnemyPlayer>().enabled = false;
		GetComponent<NetworkedPlayer>().Id = id;
		enabled = false;
	}
	
	private void PlayerMovement()
	{
		playerAnimator.SetBool("isJumping", false);
		playerAnimator.SetBool("isDead", false);
		playerAnimator.SetBool("isRunning", (Mathf.Abs(rigidbody2D.velocity.x) > 0.2));
		playerAnimator.SetBool("isIdle", (Mathf.Abs(rigidbody2D.velocity.x) < 0.2));
	}

	private void UpdatePosition(int id, Vector3 position)
	{
		if (id != characterNumber) return;
		gameObject.transform.position = position;
	}

}
