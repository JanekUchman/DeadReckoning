using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EZCameraShake;
using Random = UnityEngine.Random;

public class Shotgun : MonoBehaviour {

	private SpriteRenderer playerRenderer = null;
	
	[SerializeField] private Camera camera;
	[SerializeField] private GameObject gunOwner;
	[SerializeField] private string fireButton = "Fire1";
	[SerializeField] private float gunToCharacterLerpSpeed = 100.0f;
	[SerializeField] private GameObject bullet = null;
	

    [Header("Camera Shake Settings")]
	[SerializeField] private float cameraShakeMagnitude = 0.3f; 
	[SerializeField] private float cameraShakeRoughness = 5f;
	[SerializeField] private float cameraShakeFadeInTime = 1f;
	[SerializeField] private float cameraShakeFadeOutTime = 3f; 

    [SerializeField] private Vector3 screenShakePositionVariance = new Vector3(0, 0, 0);
    [SerializeField] private Vector3 screenShakeRotationVariance = new Vector3(2, 2, 2);

	[Header("Weapon Settings")] 
	[SerializeField] private float spreadAngle;
	[SerializeField] private float fireDelay = 0.5f;
	[SerializeField] private int numberOfPellets = 8;
	[SerializeField] private float knockback = 100.0f;
	

	private SpriteRenderer gunSprite = null;
	private Transform gunTransform = null;
	private Rigidbody2D playerRigidBody = null;
    private PlayerController playerController = null;
	private CameraController cameraController = null;
	private bool flippedLeft = false;
	private float storeTime = 0.0f;
	private float nextFire = 0.0f;
	private CameraShakeInstance shakeInstance;
	private bool canFire = true;

	private Transform bulletSpawnPoint = null;
	[HideInInspector] public bool isShooting  = false;

    void Start()
    {
        SetUpShakeInstance();
    }

	private void OnEnable()
	{
//		GunController.instance.ammo = maxAmmo;
		gunSprite = GetComponentInChildren<SpriteRenderer>();
		gunTransform = transform.GetChild(0);
		bulletSpawnPoint = transform.GetChild(0);
		
		playerRenderer = gunOwner.GetComponent<SpriteRenderer>();
		playerRigidBody = gunOwner.GetComponent<Rigidbody2D>();
		playerController = gunOwner.GetComponent<PlayerController>();
		
		cameraController = camera.GetComponentInParent<CameraController>();
		playerController.PlayerDeath += OnPlayerDeath;
	}

	private void OnPlayerDeath(object sender, EventArgs eventArgs)
	{
		canFire = false;
	}

	private void SetUpShakeInstance()
    {
        shakeInstance = CameraShaker.Instance.StartShake(cameraShakeMagnitude, cameraShakeRoughness,
            cameraShakeFadeInTime);
        shakeInstance.DeleteOnInactive = false;
        shakeInstance.StartFadeOut(0);
        shakeInstance.PositionInfluence = screenShakePositionVariance;
        shakeInstance.RotationInfluence = screenShakeRotationVariance;
    }


	void Update () {
	    LerpGunToPlayer();
		
        var angleToMouse = GetAngleFromMouse();
	    RotateSpriteToCursor(angleToMouse);
	    storeTime += Time.deltaTime;
		if (Input.GetButton(fireButton) && storeTime > nextFire && canFire)
		{
            ScreenShake(true);
		    FireGun(angleToMouse);
		}
		if (!Input.GetButton(fireButton))
		{
            ScreenShake(false);
			isShooting = false;
		}
	}

	private void LerpGunToPlayer()
	{
		gameObject.transform.position = Vector3.Lerp(new Vector3(transform.position.x, transform.position.y, 0.0f),
			new Vector3(playerRenderer.bounds.center.x, playerRenderer.bounds.center.y, 0.0f),
			Time.deltaTime * gunToCharacterLerpSpeed);
	}

	private float GetAngleFromMouse()
    {
        var mousePos = Input.mousePosition;
        mousePos.z = 5.0f;

        Vector3 objectPos = Camera.main.WorldToScreenPoint(transform.position);
        mousePos.x = mousePos.x - objectPos.x;
        mousePos.y = mousePos.y - objectPos.y;
        var exactAngle = (Mathf.Atan2(mousePos.y, mousePos.x) * Mathf.Rad2Deg);
        return exactAngle;
    }


    private void RotateSpriteToCursor(float angleToMouse)
    {
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, angleToMouse));
        if (angleToMouse < 90 && angleToMouse > -90 && flippedLeft)
        {
            gunSprite.flipY = false;
            flippedLeft = false;
        }
        else if ((angleToMouse < 180 && angleToMouse > 90) || (angleToMouse > -180 && angleToMouse < -90) && !flippedLeft)
        {
            gunSprite.flipY = true;
            flippedLeft = true;
        }
    }

    private void FireGun(float angleToMouse)
    {
        isShooting = true;
	    KnockBack();
	    SpawnAndFireBullets(angleToMouse, 0, bulletSpawnPoint.position);
        cameraController.MoveBasedOnAngle(angleToMouse);
        nextFire = fireDelay;
        storeTime = 0.0f;
    }

	private void KnockBack()
    {
	    
		var newKnockback = new Vector2(gunTransform.right.x * -knockback, (gunTransform.right.y * -knockback));
		playerRigidBody.AddForce(newKnockback);

	   
    }

    private void SpawnAndFireBullets(float angle, int seed, Vector3 position)
    {
	    //Random seed based on timestamp for getting shotgun spread
		Random.InitState(seed);
	    for (int i = 0; i < numberOfPellets; i++)
	    {
		    var inaccuracyModifier = Random.Range(-spreadAngle, spreadAngle);

		    var newProjectile = Instantiate(bullet,
			    position,
			    Quaternion.Euler(new Vector3(0, 0, transform.eulerAngles.z + inaccuracyModifier))) as GameObject;

		    var bulletController = newProjectile.GetComponent<BulletController>();
		    bulletController.FireBullet((angle + inaccuracyModifier), gameObject.tag, shakeInstance);
	    }
    }

    private void ScreenShake(bool isFiring)
    {
        if (isFiring)
        { 
            CameraShaker.Instance.ShakeOnce(cameraShakeMagnitude, cameraShakeRoughness, fadeInTime: 0, fadeOutTime: cameraShakeFadeOutTime);
        }
	    else
	    {
		    shakeInstance.StartFadeOut(cameraShakeFadeOutTime);
	    }
    }

    private void OnDestroy()
    {
        if (Camera.main) CameraShaker.Instance.FadeOutAndRemoveShakeInstance(shakeInstance, cameraShakeFadeOutTime);
    }
}
