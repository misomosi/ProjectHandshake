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
	private float t = 0;

	// Use this for initialization
	void Start () {
		path = GameObject.Find ("OpponentCurve").GetComponent<BezierCurve> ();
	}
	
	// Update is called once per frame
	void Update () {
		if (currentPhase == phase.approach) {
			DoApproach ();
		}
	}

	void DoApproach() {
		t += tSpeed * Time.deltaTime;
		t = Mathf.Clamp01 (t);
		Vector2 p = path.GetPointAt (t);
		transform.position = p;
	}

	void AutoCenter() {
		StartCoroutine (DoAutoCenter ());
	}

	// Coroutine that moves the hand towards the center of the screen
	IEnumerable DoAutoCenter() {
		bool complete = false;
		float maxCenterSpeed = 0.1f; // Should be a pub var!

		while (Vector2.Distance(transform.position, Vector2.zero) > Mathf.Epsilon) {
			// Move towards the center
			transform.position = Vector2.MoveTowards (transform.position, Vector2.zero, maxCenterSpeed * Time.deltaTime);
			yield return null;
		}
		transform.position = Vector2.zero;
	}
}
