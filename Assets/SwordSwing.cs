using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordSwing : MonoBehaviour {

	public bool reverse = false;

	public float aimAngle;
	public float arc = 180;
	public float swingTime = .5f;

	float r1, r2;
	float r;
	float creationTime;

	bool swinging = true;

	void Start () {
		creationTime = Time.time;

		r1 = aimAngle - arc/2 + 90;
		r2 = aimAngle + arc/2 + 90;

		if(reverse){
			float tmp = r1;
			r1 = r2;
			r2 = tmp;
		}
		SetRotation(r1);
	}

	void SetRotation(float rotInDeg){
		r = rotInDeg;

		Quaternion rot = Quaternion.Euler(new Vector3(0, 0, r));
		transform.localRotation = rot;
	}

	public void SetHeldRotation(float rotInDeg){
		if(!swinging){
			float offset = reverse ? -arc/2 + 90 : arc/2 + 90;
			SetRotation(rotInDeg + offset);
		}
	}

	public bool DoneSwinging(){
		return swinging == false;
	}

	void Update () {
		if(swinging){
			
			float timeAlive = Time.time - creationTime;
			float lifeFraction = Mathf.Clamp01(timeAlive / swingTime);

			if(lifeFraction > .9999f){
				swinging = false;
			}

			float desiredAngle = Mathf.Lerp(r1, r2, lifeFraction);
			SetRotation(desiredAngle);
		}
	}
}
