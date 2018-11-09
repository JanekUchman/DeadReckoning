using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EZCameraShake;

public class BulletController : MonoBehaviour {
	[SerializeField] protected CameraShakeInstance shakeInstance;
    [SerializeField] protected int sleepOnHitTimer = 20;

	[Header("Camera shake controls for on hit")]
	[SerializeField] protected float screenShake = 0.0f;
	[SerializeField] protected float cameraShakeMagnitude = 0.3f;
	[SerializeField] protected float cameraShakeRoughness = 5f;
	[SerializeField] protected float cameraShakeFadeOutTime = 3f;

    [Header("Bullet variables")]
    [SerializeField] protected float bulletSpeed = 500.0f;
	[SerializeField] protected float speedVariation = 50.0f;
    [SerializeField] protected float bulletDamage = 50.0f;
    [Tooltip("Don't set speed decrease multiplied by the time from fire to greater than initial bullet speed")]
    [SerializeField] protected float speedDecreasePerSecond = 500.0f; 
    [Tooltip("Don't set speed decrease multiplied by the time from fire to greater than initial bullet speed")]
    [SerializeField] protected float timeFromFireTillDelete = 1.0f;

    protected float speedDecreasePerTick = 0.0f;
    protected bool bulletFired = false;
    protected Rigidbody2D rigidBody = null;
    protected Animator anim = null;

    protected virtual void FixedUpdate()
    {

        if (bulletFired)
        {
            rigidBody.AddRelativeForce(new Vector2(-speedDecreasePerTick, 0));
        }
    }

    public void FireBullet(float angle, CameraShakeInstance gunShakeInstance)
    {
        speedDecreasePerTick = speedDecreasePerSecond / 60;
        anim = GetComponent<Animator>();
        rigidBody = gameObject.GetComponent<Rigidbody2D>();
        shakeInstance = gunShakeInstance;
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));

        rigidBody.AddRelativeForce(new Vector2(bulletSpeed + Random.Range(-speedVariation, speedVariation), 0.0f));
        bulletFired = true;
        StartCoroutine(StartDelete());

    }
    
    private IEnumerator StartDelete()
    {
        yield return new WaitForSeconds(timeFromFireTillDelete);
        BulletDeath();

    }

    private void OnTriggerEnter2D(Collider2D coll)
	{
		if (coll.gameObject.layer != Layers.bulletLayer)
		{
			Debug.Log(coll.tag);
			BulletDeath();
		}
	}

    protected virtual void BulletDeath()
    {
		Destroy(gameObject);
    }
}
