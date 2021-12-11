using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveAround : MonoBehaviour {

	public float timer = 3;
	float counter = 0;
	Vector3 dir;
	// Use this for initialization
	void Start () {
		UpdateDirection ();
	}
	
	// Update is called once per frame
	void Update () {

		counter += Time.deltaTime;
		if (counter >= timer) {
			counter = 0;
			UpdateDirection();
		}
		this.gameObject.transform.position += dir * Time.deltaTime;
	}

	void UpdateDirection() {
		dir = new Vector3(
			Random.Range (-5, 5),
			Random.Range (-5, 5),
			Random.Range (-5, 5)
		);
	}
}
