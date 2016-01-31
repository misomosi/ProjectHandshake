using UnityEngine;
using System.Collections;

public class OpponentEmotion : MonoBehaviour {

	public float angerShakeSpeed = 1.0f;
	public float angerShakeAmount = 1.0f;

	private Vector3 initialPosition;

	[Header("General")]
	public Sprite neutralFace;
	public Sprite angryFace;
	public Sprite happyFace;

	private SpriteRenderer oppSprite;

	// Use this for initialization
	void Start () {
		oppSprite = GetComponentInChildren<SpriteRenderer> ();
		oppSprite.sprite = neutralFace;
		initialPosition = transform.position;
	}
	
	// Update is called once per frame
	void Update () {
		if (MasterGame.instance.currentHandStage == MasterGame.handStage.Failure) {
			//if failed, change face to angry
			oppSprite.sprite = angryFace;
			//anger wobble
			Vector3 temp = transform.position;
			temp.x = Mathf.Sin(Time.time * angerShakeSpeed) * angerShakeAmount + initialPosition.x;
			transform.position = temp;
		}
		if (MasterGame.instance.currentHandStage == MasterGame.handStage.AutoCenter) {
			//if failed, change face to happy
			oppSprite.sprite = happyFace;
		}
	}
}
