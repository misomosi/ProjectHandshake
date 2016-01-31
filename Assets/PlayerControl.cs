using UnityEngine;
using System.Collections;

public class PlayerControl : MonoBehaviour {

	[Header("Approach Stage")]
	public float verticalVelocityScale = 1.0f;
	private float speed = 0;
	public float forwardAcceleration = 1.0f;
	public float minSpeed = 0.1f;
	public float friction = 0.2f;
	public float gripStrength = 0.0f;
	public float gripChangeSpeed = 1.0f;
	public float verticalSmoothing = 1.0f;
	public float verticalMaxSpeed = 5.0f;
	private float verticalSpeed;

	public bool isGrab = false;

	public OpponentControl opponent;

	public Vector2 initialMousePos;

	[Header("Shake Stage")]
	public float shakeMouseSensitivity = 1.0f;

	[Header("General")]
	public Sprite openImage;
	public Sprite claspedImageSucces;
	public Sprite claspedImageFailure;

	private GripBar gripBar;
	private SpriteRenderer handSprite;

	// Use this for initialization
	void Start () {
		Debug.Log ("Hello");

		initialMousePos = Input.mousePosition;

		gripBar = Object.FindObjectOfType<GripBar> ();
		handSprite = GetComponentInChildren<SpriteRenderer> ();
		opponent = Object.FindObjectOfType<OpponentControl> ();
		handSprite.sprite = openImage;
	}
	
	// Update is called once per frame
	void Update () {
		switch (MasterGame.instance.currentHandStage) {
		case MasterGame.handStage.Joust:
			DoMovementPhase();
			break;

		case MasterGame.handStage.Shake:
			DoShakePhase();
			break;
		}

		if (Input.GetButtonDown ("Fire1")) {
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
		}
		if (Input.GetButtonDown ("Cancel")) {
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
		}

	}

	void DoMovementPhase () {
		Vector2 mousePos = Camera.main.ScreenPointToRay (Input.mousePosition).origin;
		Vector2 pos = transform.position;
		pos.y = Mathf.SmoothDamp (pos.y, mousePos.y, ref verticalSpeed, verticalSmoothing);
		
		// Add forward velocity when the player hold accelerator
		if (Input.GetButton ("Accelerate")) {
			speed += forwardAcceleration * Time.deltaTime;
		} else {
			if (Mathf.Abs (speed) < minSpeed)
				speed = minSpeed;
		}
		speed -= speed * friction;
		pos.x += speed * Time.deltaTime;
		
		// Handle gripping
		if (Input.GetButtonDown ("Grip")) {
			AttemptGrip();
		}
		
		transform.position = pos;
	}

	public void AutoCenter() {
		StartCoroutine (DoAutoCenter ());
	}

	// Coroutine that moves the hand towards the center of the screen
	IEnumerator DoAutoCenter() {
		float maxTime = MasterGame.instance.AutoCenterTime;
		float t = 0;
		Vector2 startPos = transform.position;
		AnimationCurve easeEvaluator = AnimationCurve.EaseInOut (0, 0, maxTime, 1);

		while (Vector2.Distance(transform.position, Vector2.zero) > Mathf.Epsilon) {
			// Ease towards the center
			transform.position = Vector2.Lerp(startPos, Vector2.zero, easeEvaluator.Evaluate(t));
			t += Time.deltaTime;
			yield return null;
		}
		transform.position = Vector2.zero;
		MasterGame.instance.currentHandStage = MasterGame.handStage.Shake;
	}

	void DoShakePhase() {
		Vector3 playerInput = new Vector3(0, Input.GetAxis ("Mouse Y") * shakeMouseSensitivity, 0);
		transform.position += playerInput;
		Vector3 MaxPos = Camera.main.ViewportToWorldPoint(new Vector2(1,1));
		Vector3 MinPos = Camera.main.ViewportToWorldPoint(new Vector2(0,0));
		float clampedY = Mathf.Clamp (transform.position.y, MinPos.y, MaxPos.y);
		transform.position = new Vector3 (transform.position.x, clampedY, transform.position.z);

		handSprite.sprite = claspedImageSucces;
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
			//handSprite.color = Color.green;
			// Move to next phase
			handSprite.sprite = claspedImageSucces;
			MasterGame.instance.currentHandStage = MasterGame.handStage.AutoCenter;

		} else {
			// Failed to grip correctly
			handSprite.sprite = claspedImageFailure;
			MasterGame.instance.currentHandStage = MasterGame.handStage.Failure;
		}

	}
}
