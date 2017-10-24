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
		maxV = 2.3f,
		fricMul = .83f
	};
	Speed dashingSpeed = new Speed{
		acc = 9000,
		maxV = 15 * 1f,
		fricMul = 1
	};
	Speed currSpeed;

	SwordSwing currSwordSwing = null;
	float timeHeldAtFull = 0;
	bool releasedCurrSwordSwing = false;
	bool releasedCurrSwordSwingDuringWindow = false;

	float dashTime = .2f * 1;
	float dashButtonHoldTime = .0001f;
	float dashAmount = 0;

	void Start () {
		input.Init();
		rb2d = GetComponent<Rigidbody2D>();
	}

	void StopDashing(){
		input.lockMoveButtons = false;
		dashAmount = 0;
	}

	void Update(){

		input.UpdateInput();

		if(input.holdingMove){
			facingAngle = input.moveAngle;
		}

		// Move sword around with player while held
		if(currSwordSwing){
			currSwordSwing.SetRotation(Mathf.Rad2Deg * facingAngle);
		}

		// Rotate visual around
		visual.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, Mathf.Rad2Deg * facingAngle));

		PlayerInput.ButtonState swordButton = input.swordButton.state;
		lockFacingAngle = false;

		if(!currSwordSwing){ // No sword out
			
			if(swordButton == PlayerInput.ButtonState.Press && dashAmount == 0){
				// Swing sword
				Quaternion rot = Quaternion.Euler(new Vector3(0, 0, Mathf.Rad2Deg * facingAngle));
				currSwordSwing = GameObject.Instantiate(swordSwing, transform.position, rot, transform);
				timeHeldAtFull = 0;
				releasedCurrSwordSwing = false;
				releasedCurrSwordSwingDuringWindow = false;
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
//			bool almostCouldDash = timeHeldAtFull >= 0.001f; // Holding button at all while swing done is considered a dash
			bool almostCouldDash = false; // No window

			// Give the player a small window where they can release the button early and still dash
			//
			if(releasing && almostCouldDash && !couldDash){
				releasedCurrSwordSwingDuringWindow = true;
				input.lockMoveButtons = true;
			}
				
			if(!currSwordSwing.DoneSwinging() || releasedCurrSwordSwingDuringWindow){
				lockFacingAngle = true;
			}

			if(holding || releasedCurrSwordSwingDuringWindow){ // Holding
				
				if(currSwordSwing.DoneSwinging()){
					timeHeldAtFull += Time.deltaTime; // TODO issue where can't turn during window?

					// Cancel the artificial wait once dash is possible
					//
					if(timeHeldAtFull >= dashButtonHoldTime){
						releasedCurrSwordSwingDuringWindow = false;
					}
				}

			} else { // Released
				
				if(couldDash && input.holdingMove){
					// Dash
					input.lockMoveButtons = true;
					dashAmount = 1;
					Invoke("StopDashing", dashTime);
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

		v.x += currSpeed.acc * input.xMove;
		v.y += currSpeed.acc * input.yMove;

		v.x = Mathf.Clamp(v.x, -currSpeed.maxV, currSpeed.maxV);
		v.y = Mathf.Clamp(v.y, -currSpeed.maxV, currSpeed.maxV);

		rb2d.velocity = v;

	}
}
