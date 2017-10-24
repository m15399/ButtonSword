using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput {

	public float moveAngle { get; private set; }
	public int xMove { get; private set; }
	public int yMove { get; private set; }
	public bool holdingMove { get; private set; } // TODO can be factored out

	public bool lockMoveButtons = false;

	public enum ButtonState {
		None,
		Off,
		Press,
		Hold,
		Release
	}

	public class Button {
		public ButtonState state { get; private set; }

		public float pressLeniencyTime = 0;
		public float releaseLeniencyTime = 0;

		private float pressTime;
		private float releaseTime;

		private bool oneFrameRelease = false;

		public Button(){
			ClearState();
		}

		public Button(float pressLeniency, float releaseLeniency){
			pressLeniencyTime = pressLeniency;
			releaseLeniencyTime = releaseLeniency;
			ClearState();
		}

		void ClearState(){
			state = ButtonState.Off;
			pressTime = releaseTime = -9000;
		}

		public void UpdateState(bool inputDown, bool inputPressed){

			ButtonState newState = ButtonState.None;

			switch(state){
			case ButtonState.None:
				Debug.Log("Should never be in this state!");
				break;

			case ButtonState.Off:
				if(inputPressed){
					newState = ButtonState.Press;
				}
				break;

			case ButtonState.Press:
				if(inputPressed){
					newState = ButtonState.Release;
					oneFrameRelease = true;
				} else {
					if (Time.time - pressTime > pressLeniencyTime){
						if (inputDown){
							newState = ButtonState.Hold;
						} else {
							newState = ButtonState.Release;
						}
					}
				}
				break;

			case ButtonState.Hold:
				if(inputPressed){
					newState = ButtonState.Press;
				} else if (!inputDown){
					newState = ButtonState.Release;
				}
				break;

			case ButtonState.Release:
				if(inputPressed || oneFrameRelease){
					newState = ButtonState.Press;
					oneFrameRelease = false;
				} else {
					if (Time.time - releaseTime > releaseLeniencyTime){
						newState = ButtonState.Off;
					}
				}
				break;
			}

			// Will transition, clear state
			//
			if(newState != ButtonState.None){
				ClearState();
//				Debug.Log("Transitioning: " + newState);
			}

			// Set up new state
			//
			switch(newState){
			case ButtonState.Press:
				pressTime = Time.time;
				break;
			case ButtonState.Release:
				releaseTime = Time.time;
				break;
			}

			if(newState != ButtonState.None){
				state = newState;
			}
		}
	}

	public Button swordButton = new Button(.15f, .0f);

	public void Init(){
		moveAngle = 0;
		xMove = yMove = 0;
		holdingMove = false;
	}

	public void UpdateInput () {
		if(!lockMoveButtons){
			xMove = 0;
			yMove = 0;

			if(Input.GetKey(KeyCode.A)) xMove--;
			if(Input.GetKey(KeyCode.D)) xMove++;
			if(Input.GetKey(KeyCode.W)) yMove++;
			if(Input.GetKey(KeyCode.S)) yMove--;
		}

		holdingMove = false;

		if(xMove != 0 || yMove != 0){
			holdingMove = true;
			moveAngle = Mathf.Atan2(yMove, xMove);
		}

		bool swordButtonHeld = Input.GetKey(KeyCode.Period);
		bool swordButtonPressed = Input.GetKeyDown(KeyCode.Period);
		swordButton.UpdateState(swordButtonHeld, swordButtonPressed);
	}
}
