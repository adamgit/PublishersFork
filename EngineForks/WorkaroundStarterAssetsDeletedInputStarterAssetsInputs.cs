using UnityEngine;

namespace PublishersFork
{
	/// <summary>
	/// NOTE: This REQUIRES you to also use the class <see cref="WorkaroundStarterAssetsDeletedInputFirstPersonController"/>
	/// since Unity's design splits the implementation into two classes.
	///
	/// NOTE: If you want to support both old and new input then it is easy to take the modifications in this
	/// pair of classes and add them to Unity's official classes - use "diff" to see exactly which lines of code
	/// needed modification. Unity staff could have (should have!) done this themselves but chose not to.
	/// </summary>

	public class WorkaroundStarterAssetsDeletedInputStarterAssetsInputs : MonoBehaviour
	{
		[Header("Character Input Values")]
		public Vector2 move;
		public Vector2 look;
		public bool jump;
		public bool sprint;

		[Header("Movement Settings")]
		public bool analogMovement;

		[Header("Mouse Cursor Settings")]
		public bool cursorLocked = true;
		public bool cursorInputForLook = true;

		public void SampleInput()
		{
			move = Vector2.zero;
			if( Input.GetKey( KeyCode.LeftArrow  ) || Input.GetKey( KeyCode.A ) )
				move.x += -1f;
			if( Input.GetKey( KeyCode.RightArrow ) || Input.GetKey( KeyCode.D ))
				move.x += 1f;
			
			if( Input.GetKey( KeyCode.UpArrow ) || Input.GetKey( KeyCode.W ) )
				move.y += 1f;
			if( Input.GetKey( KeyCode.DownArrow ) || Input.GetKey( KeyCode.S ))
				move.y += -1f;
			
			look.x = Input.GetAxis( "Mouse X" );
			look.y = Input.GetAxis( "Mouse Y" );

			sprint = Input.GetKey( KeyCode.LeftShift );
			jump = Input.GetKeyDown( KeyCode.Space );
		}

#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
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
#endif


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