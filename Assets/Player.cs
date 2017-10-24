using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {
	
	public SwordSwing swordSwing;
	public GameObject visual;

	Rigidbody2D rb2d;

	PlayerInput input = new PlayerInput();

	bool lockFacingAngle = false;
	float _facingAngle;
	float facingAngle {
		get { return _facingAngle; }
		set {
			if(!lockFacingAngle){
				_facingAngle = value;
			}
		}
	}

	class Speed {
		public float acc, maxV, fricMul;

		public Speed(){}

		public Speed(float acc, float maxV, float fricMul){
			this.acc = acc;
			this.maxV = maxV;
			this.fricMul = fricMul;
		}

		public static Speed Lerp(Speed a, Speed b, float t){
			Speed ret = new Speed();
			ret.acc = Mathf.Lerp(a.acc, b.acc, t);
			ret.maxV = Mathf.Lerp(a.maxV, b.maxV, t);
			ret.fricMul = Mathf.Lerp(a.fricMul, b.fricMul, t);
			return ret;
		}
	}

	Speed walkingSpeed = new Speed{
		acc = 1.3f,
		maxV = 2.5f,
		fricMul = .83f
	};
	Speed dashingSpeed = new Speed{
		acc = 9000,
		maxV = 11 * 1f,
		fricMul = 1
	};
	Speed currSpeed;

	SwordSwing currSwordSwing = null;
	float timeHeldAtFull = 0;
	bool releasedCurrSwordSwing = false;
	SwordSwing dashSwing = null;

	float dashTime = .3f * 1;
	float dashButtonHoldTime = .0001f;
	float dashAmount = 0;

	void Start () {
		input.Init();
		rb2d = GetComponent<Rigidbody2D>();
	}

	void StopDashing(){
		input.lockMoveButtons = false;
		dashAmount = 0;

		if(dashSwing){
			GameObject.Destroy(dashSwing.gameObject);
		}
	}

	void Update(){

		input.UpdateInput();

		if(input.holdingMove){
			facingAngle = input.moveAngle;
		}

		// Move sword around with player while held
		if(currSwordSwing){
			currSwordSwing.SetHeldRotation(Mathf.Rad2Deg * facingAngle);
		}

		// Rotate visual around
		visual.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, Mathf.Rad2Deg * facingAngle));

		PlayerInput.ButtonState swordButton = input.swordButton.state;
		lockFacingAngle = false;

		if(!currSwordSwing){ // No sword out
			
			if(swordButton == PlayerInput.ButtonState.Press && dashAmount == 0){
				// Swing sword
				currSwordSwing = GameObject.Instantiate(swordSwing, transform.position, Quaternion.identity, transform);
				currSwordSwing.aimAngle = facingAngle * Mathf.Rad2Deg;
				currSwordSwing.arc = 170;
				currSwordSwing.swingTime = .18f;
				currSwordSwing.reverse = false;

				timeHeldAtFull = 0;
				releasedCurrSwordSwing = false;

			}
		} else { // Sword out

			bool releasing = swordButton == PlayerInput.ButtonState.Release;
			bool holding = swordButton == PlayerInput.ButtonState.Hold || swordButton == PlayerInput.ButtonState.Press;

			if(releasing){
				releasedCurrSwordSwing = true;
			}
			if(releasedCurrSwordSwing){
				holding = false;
			}

			bool couldDash = timeHeldAtFull >= dashButtonHoldTime;
				
			if(!currSwordSwing.DoneSwinging()){
				lockFacingAngle = true;
			}

			if(holding){ // Holding
				
				if(currSwordSwing.DoneSwinging()){
					timeHeldAtFull += Time.deltaTime;
				}

			} else { // Released
				
				if(couldDash && input.holdingMove){
					// Dash
					input.lockMoveButtons = true;
					dashAmount = 1;
					Invoke("StopDashing", dashTime);

					dashSwing = GameObject.Instantiate(swordSwing, transform.position, Quaternion.identity, transform);
					dashSwing.aimAngle = facingAngle * Mathf.Rad2Deg;
					dashSwing.arc = 190;
					dashSwing.swingTime = dashTime - .1f;
					dashSwing.reverse = true;
				}

				// Destroy sword when done with the whole swing
				//
				if(currSwordSwing.DoneSwinging()){
					GameObject.Destroy(currSwordSwing.gameObject);
					currSwordSwing = null;
					timeHeldAtFull = 0;
				}
			}
		}
	}
	
	void FixedUpdate () {

		Vector2 v = rb2d.velocity;

		currSpeed = Speed.Lerp(walkingSpeed, dashingSpeed, dashAmount);

		v *= currSpeed.fricMul;

		v += currSpeed.acc * new Vector2(input.xMove, input.yMove).normalized;

		v = Vector2.ClampMagnitude(v, currSpeed.maxV);

		rb2d.velocity = v;

	}
}
