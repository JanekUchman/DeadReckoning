using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[ExecuteInEditMode]
//This script is awful and needs updating
public class SpriteChanger : MonoBehaviour 
{
	[SerializeField] private Sprite TOpen = null;
	[SerializeField] private Sprite TLOpen = null;
	[SerializeField] private Sprite TROpen = null;
	[SerializeField] private Sprite TLROpen = null;
	[SerializeField] private Sprite BOpen = null;
	[SerializeField] private Sprite BLOpen = null;
	[SerializeField] private Sprite BROpen = null;
	[SerializeField] private Sprite BLROpen = null;
	[SerializeField] private Sprite LOpen = null;
	[SerializeField] private Sprite ROpen = null;
	[SerializeField] private Sprite BRTOpen = null;
	[SerializeField] private Sprite BLTOpen = null;
	[SerializeField] private Sprite LROpen = null;
	[SerializeField] private Sprite TBOpen = null;
	[SerializeField] private Sprite AOpen = null;
	[SerializeField] private Sprite NOpen = null;


	private SpriteRenderer rend = null;
	private bool topOpen = false, rightOpen = false, leftOpen = false, bottomOpen = false;
	private float colliderOffset = 0.04f;
	private float lengthToSearch = 0.1f;

    [HideInInspector] public bool updateNeighbours = true;

	// Use this for initialization
	void Awake ()
	{
	    updateNeighbours = true;
	    AssignSprite();
	    updateNeighbours = false;
	}

	private void Update()
	{
		Vector2 lineStart = new Vector2(transform.position.x + rend.bounds.extents.x + colliderOffset, transform.position.y );
		Vector2 vectorToSearch = new Vector2 (lineStart.x + lengthToSearch, transform.position.y);
		Debug.DrawLine(lineStart, vectorToSearch);
	}

	public void AssignSprite()
    {
        rend = GetComponent<SpriteRenderer>();
        topOpen = HasTopOpen();
        rightOpen = HasRightOpen();
        leftOpen = HasLeftOpen();
        bottomOpen = HasBottomOpen();

	   
        if (topOpen && leftOpen && rightOpen && !bottomOpen) //top left right
            rend.sprite = TLROpen;
        else if (topOpen && leftOpen && !rightOpen && !bottomOpen) //top left
            rend.sprite = TLOpen;
        else if (topOpen && !leftOpen && rightOpen && !bottomOpen) //top right
            rend.sprite = TROpen;
        else if (topOpen && !leftOpen && !rightOpen && !bottomOpen) //top
            rend.sprite = TOpen;
        else if (!topOpen && !leftOpen && !rightOpen && bottomOpen) //bottom
            rend.sprite = BOpen;
        else if (topOpen && !leftOpen && rightOpen && !bottomOpen) //right
	        rend.sprite = ROpen;
        else if (!topOpen && leftOpen && !rightOpen && !bottomOpen) //left
	        rend.sprite = LOpen;
        else if (!topOpen && leftOpen && !rightOpen && bottomOpen) //bottom left
	        rend.sprite = BLOpen;
        else if (!topOpen && !leftOpen && rightOpen && bottomOpen) //bottom right
	        rend.sprite = BROpen;
        else if (!topOpen && leftOpen && rightOpen && bottomOpen) //bottom left right
	        rend.sprite = BLROpen;
        else if (topOpen && leftOpen && !rightOpen && bottomOpen) //bottom top left
	        rend.sprite = BLTOpen;
        else if (topOpen && !leftOpen && rightOpen && bottomOpen) //bottom top right
	        rend.sprite = BRTOpen;
        else if (!topOpen && leftOpen && rightOpen && !bottomOpen) //left right
	        rend.sprite = LROpen;
        else if (topOpen && !leftOpen && !rightOpen && bottomOpen) //top bottom
	        rend.sprite = TBOpen;
        else if (topOpen && leftOpen && rightOpen && bottomOpen) //top bottom
	        rend.sprite = AOpen;
        else
            rend.sprite = NOpen;
	    Debug.LogFormat("Top {0} Left {1} Right {2}  bottom {3}", topOpen, leftOpen, rightOpen, bottomOpen);
    }

	private bool HasLeftOpen()
	{
		Vector2 lineStart = new Vector2(transform.position.x - rend.bounds.extents.x - colliderOffset, transform.position.y );
		Vector2 vectorToSearch = new Vector2 (lineStart.x - lengthToSearch, transform.position.y);
		RaycastHit2D hit = Physics2D.Linecast(lineStart, vectorToSearch);

		if (hit)
		{
		    if (hit.transform.gameObject.layer == 8)
		    {
		        if (hit.transform.gameObject.GetComponent<SpriteChanger>() && updateNeighbours)
		        {
		            hit.transform.gameObject.GetComponent<SpriteChanger>().AssignSprite();
                }

		        return false;
		    }

		}
		return true;
	}

	private bool HasRightOpen()
	{
		Vector2 lineStart = new Vector2(transform.position.x + rend.bounds.extents.x + colliderOffset, transform.position.y );
		Vector2 vectorToSearch = new Vector2 (lineStart.x + lengthToSearch, transform.position.y);
		RaycastHit2D hit = Physics2D.Linecast(lineStart, vectorToSearch);

	    if (hit)
	    {
	        if (hit.transform.gameObject.layer == 8)
	        {
	            if (hit.transform.gameObject.GetComponent<SpriteChanger>() && updateNeighbours)
	            {
	                hit.transform.gameObject.GetComponent<SpriteChanger>().AssignSprite();
	            }
                return false;
	        }

	    }
        return true;
	}

	private bool HasTopOpen()
	{
		Vector2 lineStart = new Vector2(transform.position.x, transform.position.y + rend.bounds.extents.y + colliderOffset );
		Vector2 vectorToSearch = new Vector2 (transform.position.x, lineStart.y + lengthToSearch);
		RaycastHit2D hit = Physics2D.Linecast(lineStart, vectorToSearch);

	    if (hit)
	    {
	        if (hit.transform.gameObject.layer == 8)
	        {
	            if (hit.transform.gameObject.GetComponent<SpriteChanger>() && updateNeighbours)
	            {
	                hit.transform.gameObject.GetComponent<SpriteChanger>().AssignSprite();
	            }

                return false;
	        }

	    }
        return true;
	}

	private bool HasBottomOpen()
	{
		Vector2 lineStart = new Vector2(transform.position.x, transform.position.y - rend.bounds.extents.y - colliderOffset );
		Vector2 vectorToSearch = new Vector2 (transform.position.x, lineStart.y - lengthToSearch);
		RaycastHit2D hit = Physics2D.Linecast(lineStart, vectorToSearch);

	    if (hit)
	    {
	        if (hit.transform.gameObject.layer == 8)
	        {
	            if (hit.transform.gameObject.GetComponent<SpriteChanger>() && updateNeighbours)
	            {
	                hit.transform.gameObject.GetComponent<SpriteChanger>().AssignSprite();
	            }

                return false;
	        }

	    }
        return true;
	}

    void OnDestroy()
    {
       
        updateNeighbours = true;
         HasTopOpen();
         HasRightOpen();
            HasLeftOpen();
        HasBottomOpen();
    }
}
