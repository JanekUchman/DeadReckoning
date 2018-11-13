using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {
	private Transform playerTransform = null;
	private Transform cameraTransform = null;
	private PlayerController playerController;
	private float characterLeadDirection = 0.0f;
	private Vector3 originalCameraPosition = Vector3.zero;
	private Shotgun shotgun = null;
	private float yTemp = 0.0f;

	[SerializeField] private float characterYOffset = 2.0f;
	[SerializeField] private float waitForYChange = 3.0f;
	[SerializeField] private float maxRoomX = 0, maxRoomY = 0, minRoomX = 0, minRoomY = 0;
	[SerializeField] private float zDistanceFromCharacter = -5.0f;
	[SerializeField] private float cameraLerpSpeed = 5.0f; 
	[SerializeField] private float characterLead = 5.0f; //how far in front of the player does the camera look 
	[SerializeField] private bool canShake = true;

	// Use this for initialization
	void Awake () 
	{
		playerTransform = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
		playerController = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
		shotgun = GameObject.FindGameObjectWithTag("GunController").GetComponent<Shotgun>();
		cameraTransform = transform;
	}
	
	// Update is called once per frame

    void FixedUpdate () 
    {

        //Override camera lead to face the way the player is shooting
        //Shooting gains priority over movement
        if (!shotgun.isShooting)
            characterLeadDirection = characterLead*playerController.movementDirection;


        UpdateCameraYPos();
				
        gameObject.transform.position = Vector3.Lerp(new Vector3(cameraTransform.position.x, cameraTransform.position.y, zDistanceFromCharacter), 
            new Vector3 (playerTransform.position.x + characterLeadDirection, yTemp, zDistanceFromCharacter), 
            cameraLerpSpeed * Time.deltaTime);
        LockCameraToRoom();


    }

    private void UpdateCameraYPos()
    {
        if (playerController.grounded || playerController.wallHit)
            yTemp = playerTransform.position.y + characterYOffset;
        else if ((yTemp - playerTransform.position.y - characterYOffset > waitForYChange) ||
                 (yTemp - playerTransform.position.y - characterYOffset < -waitForYChange))
            yTemp = playerTransform.position.y;
    }

    private void LockCameraToRoom()
    {
        if (gameObject.transform.position.x < minRoomX)
            gameObject.transform.position = new Vector3(minRoomX, gameObject.transform.position.y,
                gameObject.transform.position.z);
        if (gameObject.transform.position.x > maxRoomX)
            gameObject.transform.position = new Vector3(maxRoomX, gameObject.transform.position.y,
                gameObject.transform.position.z);
        if (gameObject.transform.position.y < minRoomY)
            gameObject.transform.position = new Vector3(gameObject.transform.position.x, minRoomY,
                gameObject.transform.position.z);
        if (gameObject.transform.position.y > maxRoomY)
            gameObject.transform.position = new Vector3(gameObject.transform.position.x, maxRoomY,
                gameObject.transform.position.z);
    }

    public void FlipCamera(int direction)
	{
		if (characterLeadDirection < 0 && direction > 0)
			characterLeadDirection = characterLead;
		else if (characterLeadDirection > 0 && direction < 0)
			characterLeadDirection = -characterLead;

	}



}
