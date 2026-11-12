using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace StarterAssets
{
	public class StarterAssetsInputs : MonoBehaviour
	{
		[Header("Character Input Values")]
		public Vector2 move;
		public Vector2 look;
		public bool jump;
		public bool sprint;
		public bool interact;
		public bool interactHeld;
		public bool playerHit;
		public bool IsInteractHeld => interactHeld;

		[Header("Movement Settings")]
		public bool analogMovement;

		[Header("Mouse Cursor Settings")]
		public bool cursorLocked = true;
		public bool cursorInputForLook = true;

#if ENABLE_INPUT_SYSTEM
		public void OnMove(InputValue value)
		{
			MoveInput(value.Get<Vector2>());
		}

		public void OnLook(InputValue value)
		{
			if(cursorInputForLook)
			{
				LookInput(value.Get<Vector2>());
			}
		}

		public void OnJump(InputValue value)
		{
			JumpInput(value.isPressed);
		}

		public void OnSprint(InputValue value)
		{
			SprintInput(value.isPressed);
		}
		
		public void OnPickUp(InputValue value)
		{
			PickUpInput(value.isPressed);
		}

		public void OnPlayerHit(InputValue value)
		{
			PlayerHitInput(value.isPressed);
		}
#endif

		public bool ConsumeInteract()
		{
			if(!interact)
			{
				return false;
			}

			interact = false;
			return true;
		}

		public bool ConsumePlayerHit()
		{
			if(!playerHit)
			{
				return false;
			}

			playerHit = false;
			return true;
		}

		public void PickUpInput(bool newE)
		{
			Debug.Log("PickUpInput: " + newE);
			interactHeld = newE;
			if (newE) interact = true;
		}

		public void PlayerHitInput(bool click)
		{
			playerHit = click;
		}

		public void MoveInput(Vector2 newMoveDirection)
		{
			move = newMoveDirection;
		} 

		public void LookInput(Vector2 newLookDirection)
		{
			look = newLookDirection;
		}

		public void JumpInput(bool newJumpState)
		{
			jump = newJumpState;
		}

		public void SprintInput(bool newSprintState)
		{
			sprint = newSprintState;
		}
		
		private void OnApplicationFocus(bool hasFocus)
		{
			SetCursorState(cursorLocked);
		}

		private void SetCursorState(bool newState)
		{
			Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
		}
	}
	
}