using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR.WSA;

public class ReloadController : MonoBehaviour
{
	
	[SerializeField] private Transform slideBar;
	[SerializeField] private float barMoveSpeed;
	
	private float startX;
	private float barLength;
	private Vector2 position;
	
	
	private void OnEnable()
	{
		var rend = GetComponent<SpriteRenderer>().sprite.bounds;
		startX = rend.min.x;
		barLength = rend.max.x*2;
		slideBar.localPosition = new Vector2(rend.min.x, slideBar.localPosition.y);
	}

	private void Update()
	{
		MoveSlideBar();
	}

	public void StartReload()
	{
		gameObject.SetActive(true);
	}

	private void MoveSlideBar()
	{
		if (slideBar.localPosition.x < startX + barLength)
		{
			slideBar.localPosition = new Vector2(slideBar.localPosition.x + (barMoveSpeed * Time.deltaTime), slideBar.localPosition.y);
		}
		else
		{
			GunController.instance.ammo++;
			DisableSlideBar();	
		}
	}

	private void DisableSlideBar()
	{
		gameObject.SetActive(false);
	}
	
}
