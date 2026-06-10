using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class AgentTestScript : MonoBehaviour
{
	[Header("Input")]
	[SerializeField] private InputActionAsset inputActions;
	[SerializeField] private string actionMapName = "Player";
	[SerializeField] private string moveActionName = "Move";

	[Header("Movement")]
	[SerializeField] private float moveSpeed = 5f;
	[SerializeField] private float turnSpeed = 720f;
	[SerializeField] private float facingOffsetY = 90f;

	private Rigidbody targetRigidbody;
	private InputAction moveAction;
	private Vector2 moveInput;

	private void Awake()
	{
		targetRigidbody = GetComponent<Rigidbody>();
		targetRigidbody.constraints |= RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
	}

	private void OnEnable()
	{
		if (inputActions == null)
		{
			Debug.LogError($"{nameof(AgentTestScript)} needs an InputActionAsset assigned.");
			enabled = false;
			return;
		}

		InputActionMap actionMap = inputActions.FindActionMap(actionMapName, true);
		moveAction = actionMap.FindAction(moveActionName, true);
		moveAction.performed += OnMove;
		moveAction.canceled += OnMove;
		moveAction.Enable();
	}

	private void OnDisable()
	{
		if (moveAction == null)
		{
			return;
		}

		moveAction.performed -= OnMove;
		moveAction.canceled -= OnMove;
		moveAction.Disable();
	}

	private void OnMove(InputAction.CallbackContext context)
	{
		moveInput = context.ReadValue<Vector2>();
	}

	private void FixedUpdate()
	{
		Vector3 moveDirection = new Vector3(-moveInput.y, 0f, moveInput.x);
		Vector3 movement = moveDirection * moveSpeed * Time.fixedDeltaTime;
		targetRigidbody.MovePosition(targetRigidbody.position + movement);

		if (moveDirection.sqrMagnitude > 0.0001f)
		{
			Quaternion targetRotation = Quaternion.LookRotation(moveDirection, Vector3.up) * Quaternion.Euler(0f, facingOffsetY, 0f);
			Quaternion nextRotation = Quaternion.RotateTowards(targetRigidbody.rotation, targetRotation, turnSpeed * Time.fixedDeltaTime);
			targetRigidbody.MoveRotation(nextRotation);
		}
	}
}
