using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GripBar : MonoBehaviour {

	public float gripAmount = 0.0f; // 0 to 1


	private Image progressBar;
	// Use this for initialization
	void Start () {
		progressBar = GetComponentInChildren<Image> ();
	}
	
	// Update is called once per frame
	void Update () {
		progressBar.fillAmount = gripAmount;
	}
}
