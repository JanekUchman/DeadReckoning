using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour {
	
    [Header ("Groundcheck")]
	[SerializeField] private float lengthToSearch = 0.2f; //distance to check for ground
    [Header ("Jump variables")]
    [SerializeField] private float jumpSpeed = 8; 
	private bool wallHitDoubleJumpOverride = true;
    [Header ("Damage variables")]
	[SerializeField] private float health = 500.0f;
	[SerializeField] private float invulnerabilityTimer = 0.3f;
    public bool canTakeDamage = true;


	private int[] layerMask =  new int[3] { 8, 9, 11 };
	//private int layerNoWallJump = 9;
	private Renderer rend = null;
	private Rigidbody2D rigidbody2D_;
	private Shotgun shotgun = null;
	private Animator playerAnimator = null;
	private BoxCollider2D playerBoxCollider = null;
	private SpriteRenderer spriteRenderer = null;


	[HideInInspector] public int movementDirection = 1;
	[HideInInspector] public bool isPlayerMoving = false;
	[HideInInspector] public bool isAlive = true;

    [Header("Movement variables")]
	[SerializeField] private float maxSpeed = 10;
	[SerializeField] private float acceleration = 50;
	[HideInInspector] public bool wallHit = false;
	[HideInInspector] public bool leftWallHit = false;
	[HideInInspector] public bool rightWallHit = false;
	[HideInInspector] public bool grounded;


	void Start()
	{
		spriteRenderer = GetComponent<SpriteRenderer>();
		rend = gameObject.GetComponent<Renderer>();
		rigidbody2D_ = gameObject.GetComponent<Rigidbody2D>();
		shotgun = GameObject.FindGameObjectWithTag("GunController").GetComponent<Shotgun>();
		playerAnimator = GetComponent<Animator>();
		playerBoxCollider = GetComponent<BoxCollider2D>();
	}

    void Update()
    {
        if (isAlive)
        {
            var horizontal = Input.GetAxisRaw("Horizontal");
            grounded = CheckGrounded();
            playerAnimator.SetBool("playRunning", (Mathf.Abs(rigidbody2D_.velocity.x) > 0.2 && grounded));


            if (Input.GetKeyDown("space"))
            {
				if (grounded)
				{
					
					Jump();
				}
             
            }
           
	       
	        if(horizontal < -0.1f)
	        {
		        if (rigidbody2D_.velocity.x < -0.1f)
			        transform.localScale = new Vector3(1, transform.localScale.y, transform.localScale.z);


		        isPlayerMoving = true;
		        movementDirection = -1;
		        HorizontalMovement();
	        }
	        else if (horizontal > 0.1f)
	        {
		        if (rigidbody2D_.velocity.x > 0.1f)
			        transform.localScale = new Vector3(-1, transform.localScale.y, transform.localScale.z);
		        isPlayerMoving = true;
		        movementDirection = 1;
		        HorizontalMovement();
	        }
	        else
		        isPlayerMoving = false;
        }
    }

    

    private bool CheckGrounded()
    {
        var colliderThreshold = 0.05f;
        var topLeftOfGroundCheck = new Vector2(playerBoxCollider.bounds.min.x,
            playerBoxCollider.bounds.min.y - colliderThreshold);
        var bottomRightOfGroundCheck = new Vector2(playerBoxCollider.bounds.max.x,
            playerBoxCollider.bounds.min.y - lengthToSearch);
        return CharacterFunctions.GroundCheck(topLeftOfGroundCheck, bottomRightOfGroundCheck);
    }


    private void HorizontalMovement()
    {
        if (grounded)
        {
            if (Mathf.Abs(rigidbody2D_.velocity.x) > maxSpeed)
                rigidbody2D_.AddForce(new Vector2(acceleration * movementDirection *Time.deltaTime, 0.0f));
            else
                rigidbody2D_.velocity =
                    new Vector2(maxSpeed * movementDirection,
                        rigidbody2D_.velocity.y); //if the velocity is higher than the max speed set it to the max speed
        }
        else if (Mathf.Abs(rigidbody2D_.velocity.x) > maxSpeed)
            rigidbody2D_.AddForce(new Vector2(movementDirection * acceleration * Time.deltaTime, 0.0f));
        else
            rigidbody2D_.velocity =
                new Vector2(movementDirection * maxSpeed,
                    rigidbody2D_.velocity.y); //if the velocity is higher than the max speed set it to the max speed
    }

    private void LimitSpeed()
    {
        if (Mathf.Abs(rigidbody2D_.velocity.x) > maxSpeed)
            rigidbody2D_.velocity = new Vector2(movementDirection*maxSpeed, rigidbody2D_.velocity.y); //if the velocity is higher than the max speed set it to the max speed

        
    }

    
    private void Jump()
    {
        playerAnimator.SetBool("playJump", true);

        rigidbody2D_.velocity = new Vector2(rigidbody2D_.velocity.x, jumpSpeed);

    }

    private int GetWallHitDirection(int wallHitDirection)
    {
        if (rightWallHit)
        {
            wallHit = true;
            wallHitDirection = -1;
            rightWallHit = false;
        }
        else if (leftWallHit)
        {
            wallHit = true;
            wallHitDirection = 1;
            leftWallHit = false;
        }
        return wallHitDirection;
    }

   

    

    private bool IsOnWallLeft()
	{
		
		Vector2 lineStart = new Vector2(transform.position.x - playerBoxCollider.bounds.extents.x , transform.position.y );
		Vector2 vectorToSearch = new Vector2 (lineStart.x - lengthToSearch, transform.position.y);
		RaycastHit2D hit = Physics2D.Linecast(lineStart, vectorToSearch);
		Debug.DrawLine(lineStart, vectorToSearch, Color.red);

		if (hit)
		{
			Debug.Log("Left wall hit");

			if (hit.transform.gameObject.layer == layerMask[0])
			{
			Debug.Log("Left wall hit");

				return true;
			}
			
		}
		return false;
	}

	private bool IsOnWallRight()
	{
		Vector2 lineStart = new Vector2(transform.position.x + playerBoxCollider.bounds.extents.x, transform.position.y );
		Vector2 vectorToSearch = new Vector2 (lineStart.x + lengthToSearch, transform.position.y);
		RaycastHit2D hit = Physics2D.Linecast(lineStart, vectorToSearch);
		Debug.DrawLine(lineStart, vectorToSearch, Color.red);

		if (hit)
		{
			
			return hit.transform.gameObject.layer == layerMask[0];

		}
		return false;
	}

	public IEnumerator DamagePlayer(float damage)
	{
		if (canTakeDamage)
		{
			health -=damage;
			playerAnimator.SetBool("isDamaged", true);
			if (health <= 0)
				StartCoroutine(KillPlayer());
			canTakeDamage = false;
			yield return new WaitForSeconds(invulnerabilityTimer);
			canTakeDamage = true;
			playerAnimator.SetBool("isDamaged", false);

		}


	}
	private IEnumerator KillPlayer()
	{
		playerAnimator.SetBool("isDead", true);
		isAlive = false;
		canTakeDamage = false;
		yield return new WaitForSeconds(3.0f);
		SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
	}
}
