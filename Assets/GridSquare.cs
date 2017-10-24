using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridSquare : MonoBehaviour {

	public int replicateX = 0;
	public int replicateY = 0;

	public int wrap = 10;

	void Start () {
		if(replicateX > 0){
			GridSquare child = GameObject.Instantiate(
					this,
					transform.position + new Vector3(1, 0, 0), 
					Quaternion.identity,
					transform.parent);
			child.replicateX = replicateX - 1;
			child.replicateY = replicateY;
		}
		if(replicateY > 0){
			GridSquare child = GameObject.Instantiate(
				this,
				transform.position + new Vector3(0, 1, 0), 
				Quaternion.identity,
				transform.parent);
			child.replicateX = 0;
			child.replicateY = replicateY - 1;
		}
	}
	
	void Update () {
		Vector3 pos = transform.localPosition - Camera.main.transform.position;
		pos.x = Mathf.Repeat(pos.x, wrap);
		pos.y = Mathf.Repeat(pos.y, wrap);

		transform.localPosition = pos + Camera.main.transform.position;
	}
}
