using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordSwing : MonoBehaviour {

	const float rv = 1500;
	const float rd = 170;
	float r1;
	float r2;

	bool swinging = true;

	void Start () {
		r1 = transform.rotation.eulerAngles.z;
		r2 = r1 + rd;
	}

	public void SetRotation(float rotInDeg){
		if(!swinging){
			Quaternion rot = Quaternion.Euler(new Vector3(0, 0, rd + rotInDeg));
			transform.localRotation = rot;
		}
	}

	public bool DoneSwinging(){
		return swinging == false;
	}

	void Update () {
		if(swinging){
			if(r1 < r2){
				r1 += rv * Time.deltaTime;
			} 

			if(r1 > r2){
				r1 = r2 + 1;
				swinging = false;
			}

			Vector3 r = gameObject.transform.localRotation.eulerAngles;
			r.z = r1;
			gameObject.transform.localRotation = Quaternion.Euler(r);
		}
	}
}
