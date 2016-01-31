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
		Vector2 p = path.GetPointAt (t);
		transform.position = p;
	}
}
