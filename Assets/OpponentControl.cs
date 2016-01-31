using UnityEngine;
using System.Collections;

public class OpponentControl : MonoBehaviour {

	private BezierCurve path;

	public enum phase
	{
		approach,
		shake
	}
	public float tSpeed = 1.0f;
	public phase currentPhase = phase.approach;

	[Header("General")]
	public Sprite openImage;
	public Sprite claspedImageSucces;
	public Sprite claspedImageFailure;

	void OnAwake() {
		path = GameObject.Find ("OpponentCurve").GetComponent<BezierCurve> ();
	}

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void UpdateDISABLED () {
		if (currentPhase == phase.approach) {
			Joust ();
		}
	}

	void Joust() {
		StartCoroutine (DoJoust ());
	}

	IEnumerator DoJoust() {
		float t = 0;
		if (!path) {
			path = GameObject.Find ("OpponentCurve").GetComponent<BezierCurve> ();
		}
		while (MasterGame.instance.currentHandStage == MasterGame.handStage.Joust) {
			t += tSpeed * Time.deltaTime;
			t = Mathf.Clamp (t, 0, 0.99f);
			Vector2 p = path.GetPointAt (t);
			transform.position = p;
			yield return null;
		}
	}

	/*
	public void AutoCenter() {
		StartCoroutine (DoAutoCenter ());
	}

	// Coroutine that moves the hand towards the center of the screen
	IEnumerator DoAutoCenter() {
		bool complete = false;
		float maxCenterSpeed = MasterGame.instance.handAutoDragSpeed;

		while (Vector2.Distance(transform.position, Vector2.zero) > Mathf.Epsilon) {
			// Move towards the center
			transform.position = Vector2.MoveTowards (transform.position, Vector2.zero, maxCenterSpeed * Time.deltaTime);
			yield return null;
		}
		transform.position = Vector2.zero;
	}
	*/
}
