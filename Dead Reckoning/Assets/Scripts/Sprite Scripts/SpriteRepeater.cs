using UnityEngine;
using System.Collections;

// @NOTE the attached sprite's position should be "Top Right" or the children will not align properly
// Strech out the image as you need in the sprite render, the following script will auto-correct it when rendered in the game
[RequireComponent (typeof (SpriteRenderer))]

// Generates a nice set of repeated sprites inside a streched sprite renderer
// @NOTE Vertical only, you can easily expand this to horizontal with a little tweaking
public class SpriteRepeater : MonoBehaviour {
	public float gridX = 0.0f;
	public float gridY = 0.0f;

	SpriteRenderer sprite;

	void Awake () {

		sprite = GetComponent<SpriteRenderer>();
		if(!GetSpriteAlignment(gameObject).Equals(SpriteAlignment.TopRight)){
			Debug.LogError("You forgot change the sprite pivot to Top Right.");
		}
		Vector2 spriteSize_wu = new Vector2(sprite.bounds.size.x / transform.localScale.x, sprite.bounds.size.y / transform.localScale.y);
		Vector3 scale = new Vector3(1.0f, 1.0f, 1.0f);



		if (0.0f != gridX) {
			float width_wu = sprite.bounds.size.x / gridX;
			scale.x = width_wu / spriteSize_wu.x;
			spriteSize_wu.x = width_wu;
		}

		if (0.0f != gridY) {
			float height_wu = sprite.bounds.size.y / gridY;
			scale.y = height_wu / spriteSize_wu.y;
			spriteSize_wu.y = height_wu;
		}

		GameObject childPrefab = new GameObject();

		SpriteRenderer childSprite = childPrefab.AddComponent<SpriteRenderer>();
		childPrefab.transform.position = transform.position;
		childSprite.sprite = sprite.sprite;

		GameObject child;
		for (int i = 0, h = (int)Mathf.Round(sprite.bounds.size.y); i*spriteSize_wu.y < h; i++) {
			for (int j = 0, w = (int)Mathf.Round(sprite.bounds.size.x); j*spriteSize_wu.x < w; j++) {
				child = Instantiate(childPrefab) as GameObject;
				child.transform.position = transform.position - (new Vector3(spriteSize_wu.x*j, spriteSize_wu.y*i, 0));
				child.transform.localScale = scale;
				child.transform.parent = transform;
			}
		}
		//FitColliderToChildren(gameObject);
		Destroy(childPrefab);
		sprite.enabled = false; // Disable this SpriteRenderer and let the prefab children render themselves

	}

	public static SpriteAlignment GetSpriteAlignment(GameObject SpriteObject)
	{
		BoxCollider2D MyBoxCollider= SpriteObject.GetComponent<BoxCollider2D>();
		float colX = MyBoxCollider.offset.x;
		float colY = MyBoxCollider.offset.y;
		if (colX > 0f && colY < 0f)
			return (SpriteAlignment.TopLeft);
		else if (colX < 0 && colY < 0)
			return (SpriteAlignment.TopRight);
		else if (colX == 0 && colY < 0)
			return (SpriteAlignment.TopCenter);
		else if (colX > 0 && colY == 0)
			return (SpriteAlignment.LeftCenter);
		else if (colX < 0 && colY == 0)
			return (SpriteAlignment.RightCenter);
		else if (colX > 0 && colY > 0)
			return (SpriteAlignment.BottomLeft);
		else if (colX < 0 && colY > 0)
			return (SpriteAlignment.BottomRight);
		else if (colX == 0 && colY > 0)
			return (SpriteAlignment.BottomCenter);
		else if (colX == 0 && colY == 0)
			return (SpriteAlignment.Center);
		else
			return (SpriteAlignment.Custom);
	}

	private void FitColliderToChildren (GameObject parentObject)
	{
		BoxCollider2D bc = parentObject.GetComponent<BoxCollider2D>();
		if(bc==null)
		{
			bc = parentObject.GetComponent<BoxCollider2D>();
		}
		Bounds bounds = new Bounds (Vector3.zero, Vector3.zero);
		bool hasBounds = false;
		Renderer[] renderers =  parentObject.GetComponentsInChildren<Renderer>();
		foreach (Renderer render in renderers) {
			if (hasBounds) {
				bounds.Encapsulate(render.bounds);
			} else {
				bounds = render.bounds;
				hasBounds = true;
			}
		}
		if (hasBounds) {
			bc.offset = bounds.center - parentObject.transform.position;
			bc.size = bounds.size;
		} else {
			bc.size = bc.offset = Vector3.zero;
			bc.size = Vector3.zero;
		}
	}
}
