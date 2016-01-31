using UnityEngine;
using System.Collections;

public class ShakeZone : MonoBehaviour {

	private bool isOutside = false;
	private float percentHealth;
	private Vector3 startPosition;
	private float minHealth = 0.0f;

	public float maxHealth = 1000.0f;
	public float currentHealth;
	public float damageRate = 1.0f;
	public float regenRate = 1.0f;
	public float mapSpeed = 10.0f;
	public Color fullColor = Color.green;
	public Color emptyColor = Color.red;

	void Start () {
		startPosition = transform.position;
		currentHealth = maxHealth;
	}

	void Update () {
		
		Vector3 moveDirection = new Vector3 (-mapSpeed,0,0);
		transform.position += moveDirection * Time.deltaTime;

		if (isOutside) {
			currentHealth -= damageRate;
			currentHealth = Mathf.Max(currentHealth, minHealth);
		} else {
			currentHealth += regenRate;
			currentHealth = Mathf.Min(currentHealth, maxHealth);
		}

		percentHealth = currentHealth / maxHealth;

		print ("Current Health: " + percentHealth);

		if (currentHealth <= 0) {
			print ("GAME OVER");
		}

		GetComponent<Ferr2DT_PathTerrain> ().vertexColor = Color.Lerp (emptyColor, fullColor, percentHealth);
		GetComponent<Ferr2DT_PathTerrain> ().Build ();
	}

	void OnTriggerEnter2D (Collider2D other) {
		isOutside = false;
	}
	void OnTriggerExit2D (Collider2D other) {
		isOutside = true;
	}
}
