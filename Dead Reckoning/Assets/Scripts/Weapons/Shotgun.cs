using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EZCameraShake;

public class Shotgun : MonoBehaviour {

	private SpriteRenderer playerRenderer = null;

	[SerializeField] private string fireButton = "Fire1";
	[SerializeField] private string reloadButton = "Fire2";
	[SerializeField] private float gunToCharacterLerpSpeed = 100.0f;
	[SerializeField] private GameObject bullet = null;
	

    [Header("Camera Shake Settings")]
	[SerializeField] private float cameraShakeMagnitude = 0.3f; 
	[SerializeField] private float cameraShakeRoughness = 5f;
	[SerializeField] private float cameraShakeFadeInTime = 1f;
	[SerializeField] private float cameraShakeFadeOutTime = 3f; 
	[SerializeField] private bool screenShakeOnHoldFire = true; 
	[SerializeField] private bool screenShakeOnFire = false;
    [SerializeField] private Vector3 screenShakePositionVariance = new Vector3(0, 0, 0);
    [SerializeField] private Vector3 screenShakeRotationVariance = new Vector3(2, 2, 2);

	[Header("Weapon Settings")] 
	[SerializeField] private float spreadAngle;
	[SerializeField] private int maxAmmo = 4;
	[SerializeField] private float fireDelay = 0.5f;
	[SerializeField] private int numberOfPellets = 8;
	[SerializeField] private float knockback = 100.0f;

	[Header("Reload Settings")]
	[SerializeField] private ReloadController reloadController;
	

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
	[HideInInspector] public bool swapControlTypes = true;
	[HideInInspector] public bool isShooting  = false;
	[HideInInspector] public bool shouldSwapDirections = false;

    void Start()
    {
        SetUpShakeInstance();
    }

	private void OnEnable()
	{
//		GunController.instance.ammo = maxAmmo;
		playerRenderer = GameObject.FindGameObjectWithTag("Player").GetComponent<SpriteRenderer>();
		gunSprite = GetComponentInChildren<SpriteRenderer>();
		gunTransform = transform.GetChild(0);
		bulletSpawnPoint = GameObject.FindGameObjectWithTag("BulletSpawnPoint").GetComponent<Transform>();
		playerRigidBody = GameObject.FindGameObjectWithTag("Player").GetComponent<Rigidbody2D>();
		playerController = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
		cameraController = Camera.main.GetComponentInParent<CameraController>();
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
	    gameObject.transform.position = Vector3.Lerp(new Vector3(transform.position.x, transform.position.y, 0.0f),
	        new Vector3(playerRenderer.bounds.center.x, playerRenderer.bounds.center.y, 0.0f),
	        Time.deltaTime * gunToCharacterLerpSpeed);
        var angleToMouse = GetAngleFromMouse();
	    AdjustGunFromAngle(angleToMouse);
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

		if (Input.GetButton(reloadButton))
		{
			reloadController.StartReload();
		}
		
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

    private void ControlCamera(float angleToMouse)
    {
        if ((angleToMouse < 180 && angleToMouse > 90) || (angleToMouse > -180 && angleToMouse < -90))
        {
            cameraController.FlipCamera(-1);
            playerController.movementDirection = -1;
        }
        else
        {
            cameraController.FlipCamera(1);
            playerController.movementDirection = 1;
        }
    }

    private void AdjustGunFromAngle(float angleToMouse)
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
	    ReduceAmmo();
	    KnockBack();
	    SpawnAndFireBullet(angleToMouse);
        ControlCamera(angleToMouse);
        nextFire = fireDelay;
        storeTime = 0.0f;
    }

	private void ReduceAmmo()
	{
		GunController.instance.ammo--;
		if (GunController.instance.ammo == 0) canFire = false;
	}

	

	private void KnockBack()
    {
	    
		var newKnockback = new Vector2(gunTransform.right.x * -knockback, (gunTransform.right.y * -knockback));
		playerRigidBody.AddForce(newKnockback);

	   
    }
    //TODO add object pooling
    private void SpawnAndFireBullet(float angleToMouse)
    {

	    for (int i = 0; i < numberOfPellets; i++)
	    {
		    var inaccuracyModifier = Random.Range(-spreadAngle, spreadAngle);

		    var newProjectile = Instantiate(bullet,
			    new Vector3(bulletSpawnPoint.position.x, bulletSpawnPoint.position.y, bulletSpawnPoint.position.z),
			    Quaternion.Euler(new Vector3(0, 0, transform.eulerAngles.z + inaccuracyModifier))) as GameObject;

		    var bulletController = newProjectile.GetComponent<BulletController>();
		    bulletController.FireBullet((angleToMouse + inaccuracyModifier), shakeInstance);
	    }
    }

    private void ScreenShake(bool isFiring)
    {
        if (isFiring)
        { 
            if (screenShakeOnHoldFire)
                 shakeInstance.StartFadeIn(cameraShakeFadeInTime);
            else if (screenShakeOnFire)
                CameraShaker.Instance.ShakeOnce(cameraShakeMagnitude, cameraShakeRoughness, fadeInTime: 0, fadeOutTime: cameraShakeFadeOutTime);
        }
        else 
            shakeInstance.StartFadeOut(cameraShakeFadeOutTime);
    }

    private void OnDestroy()
    {
        if (Camera.main)
            CameraShaker.Instance.FadeOutAndRemoveShakeInstance(shakeInstance, cameraShakeFadeOutTime);
    }
}
