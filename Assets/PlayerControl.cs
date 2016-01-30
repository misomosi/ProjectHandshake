using UnityEngine;
using System.Collections;

public class PlayerControl : MonoBehaviour {

	public float verticalVelocityScale = 1.0f;
	public float speed = 0;
	public float forwardAcceleration = 1.0f;
	public float minSpeed = 0.1f;
	public float friction = 0.2f;
	public float gripStrength = 0.0f;
	public float gripChangeSpeed = 1.0f;

	public bool isGrab = false;

	public Vector2 initialMousePos;


	private GripBar gripBar;
	private SpriteRenderer handSprite;
	
	// Use this for initialization
	void Start () {
		Debug.Log ("Hello");

		initialMousePos = Input.mousePosition;

		gripBar = Object.FindObjectOfType<GripBar> ();
		handSprite = GetComponentInChildren<SpriteRenderer> ();
	}
	
	// Update is called once per frame
	void Update () {
		Vector2 mousePos = Camera.main.ScreenPointToRay (Input.mousePosition).origin;
		Vector2 pos = transform.position;
		pos.y = mousePos.y; // * verticalVelocityScale;

		// Add forward velocity when the player hold accelerator
		if (Input.GetButton ("Accelerate")) {
			speed += forwardAcceleration * Time.deltaTime;
		} else {
			if (Mathf.Abs (speed) < minSpeed)
				speed = 0;
		}
		speed -= speed * friction;
		pos.x += speed * Time.deltaTime;

		// Handle gripping
		if (Input.GetButtonDown ("Grip")) {
			isGrab = true;
			handSprite.color = Color.red;
		}

		transform.position = pos;
	}
}
