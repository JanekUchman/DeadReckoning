using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[ExecuteInEditMode]

public class SpriteChanger : MonoBehaviour 
{
	[SerializeField] private Sprite TOpen = null;
	[SerializeField] private Sprite TLOpen = null;
	[SerializeField] private Sprite TROpen = null;
	[SerializeField] private Sprite TLROpen = null;
	[SerializeField] private Sprite BOpen = null;
	[SerializeField] private Sprite NOpen = null;


	private SpriteRenderer rend = null;
	private bool topOpen = false, rightOpen = false, leftOpen = false, bottomOpen = false;
	private float colliderOffset = 0.01f;
	private float lengthToSearch = 0.1f;

    [HideInInspector] public bool updateNeighbours = true;

	// Use this for initialization
	void Awake ()
	{
	    updateNeighbours = true;
	    AssignSprite();
	    updateNeighbours = false;
	}

    public void AssignSprite()
    {
        rend = GetComponent<SpriteRenderer>();
        topOpen = HasTopOpen();
        rightOpen = HasRightOpen();
        leftOpen = HasLeftOpen();
        bottomOpen = HasBottomOpen();

        if (topOpen && leftOpen && rightOpen)
            rend.sprite = TLROpen;
        else if (topOpen && leftOpen && !rightOpen)
            rend.sprite = TLOpen;
        else if (topOpen && !leftOpen && rightOpen)
            rend.sprite = TROpen;
        else if (topOpen)
            rend.sprite = TOpen;
        else if (bottomOpen)
            rend.sprite = BOpen;
        else
            rend.sprite = NOpen;
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
