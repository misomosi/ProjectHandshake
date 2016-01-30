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

	public OpponentControl opponent;

	public Vector2 initialMousePos;

	public enum phase
	{
		approach,
		shake
	}

	public phase currentPhase = phase.approach;

	private GripBar gripBar;
	private SpriteRenderer handSprite;

	// Use this for initialization
	void Start () {
		Debug.Log ("Hello");

		initialMousePos = Input.mousePosition;

		gripBar = Object.FindObjectOfType<GripBar> ();
		handSprite = GetComponentInChildren<SpriteRenderer> ();
		opponent = Object.FindObjectOfType<OpponentControl> ();
	}
	
	// Update is called once per frame
	void Update () {
		switch (currentPhase) {
		case phase.approach:
			DoMovementPhase();
			break;

		case phase.shake:
			DoShakePhase();
			break;
		}
	}

	void DoMovementPhase () {
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
			AttemptGrip();
		}
		
		transform.position = pos;
	}

	void DoShakePhase() {

	}

	void AttemptGrip() {
		// Find the center and range of both the grip points
		Transform myGripPointObj = transform.FindChild ("GripPoint");
		Transform otherGripPointObj = opponent.transform.FindChild ("GripPoint");
		Vector2 myGripPos = myGripPointObj.TransformPoint (myGripPointObj.GetComponent<CircleCollider2D> ().offset);
		Vector2 otherGripPos = otherGripPointObj.TransformPoint (otherGripPointObj.GetComponent<CircleCollider2D> ().offset);

		if (Vector2.Distance(myGripPos, otherGripPos) < myGripPointObj.GetComponent<CircleCollider2D> ().radius) {
			// We're in range!!
			speed = 0;
			handSprite.color = Color.green;
			// Move to next phase
			currentPhase = phase.shake;
		} else {
			// Failed to grip correctly
			handSprite.color = Color.red;
		}

	}
}
