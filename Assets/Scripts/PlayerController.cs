using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour {
    private CharacterController controller;
    public GameManager gameManager;

    [Header ("Power Settings")]
    [SerializeField] private Transform pullPosition;
    [SerializeField] private float pushStrength = 1000f;
    [SerializeField] private float pushRadius = 1f;
    [SerializeField] private float pullStrength = 0.5f;
    [SerializeField] private float launchVelocity = 100f;
    [SerializeField] private float launchRadius = 1f;
    [SerializeField] private float launchDistance = 5f;

    private bool launching = false;
    private bool pulling = false;
    private bool holding = false;
    private Rigidbody pulledObject;

    [Header ("Player Movement")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundDistance = 0.4f;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private LayerMask metalMask;
    [SerializeField] private float jumpHeight = 3f;

    [SerializeField] private float speed = 12f;
    [SerializeField] private float gravity = -9.81f;

    private Vector3 velocity;
    private bool isGrounded;

    private Vector2 movementInput = new Vector2 (0f, 0f);
    private Vector2 lookInput = new Vector2 (0f, 0f);

    [Header ("Camera Movement")]
    [SerializeField] private float mouseSensitivity = 100f;
    [SerializeField] private Transform playerBody;
    [SerializeField] private Camera mainCamera;

    private float xRotation;

    private void Start () {
        Cursor.lockState = CursorLockMode.Locked;
        controller = GetComponent<CharacterController> ();
    }

    // Update is called once per frame
    private void Update () {
        MoveCamera ();
        MovePlayer ();

        // launching
        if (launching && !holding) {
            RaycastHit hit;

            Vector3 SphereCastStart = groundCheck.position;
            SphereCastStart.y = groundCheck.position.y + launchRadius;
            if (Physics.SphereCast (SphereCastStart, launchRadius, -Vector3.up, out hit, launchDistance, metalMask)) {
                var launch = Mathf.Clamp ((launchVelocity * 2) / ((hit.transform.position - groundCheck.transform.position).magnitude), 0f, launchVelocity);
                velocity.y = Mathf.Max (velocity.y, launch);
            }
        }

        // pulling
        if (pulling && !holding) {
            if ((pulledObject.position - pullPosition.position).magnitude < 0.2f) {
                pulledObject.transform.parent = pullPosition;
                pulledObject.transform.localPosition = Vector3.zero;
                pulledObject.isKinematic = true;
                holding = true;
            } else {
                pulledObject.position = Vector3.MoveTowards (pulledObject.position, pullPosition.position, pullStrength / ((pulledObject.position - pullPosition.position).magnitude * pulledObject.mass));
                pulledObject.isKinematic = false;
            }
        }
    }

    // Player Movement

    private void MovePlayer () {
        // player movement
        isGrounded = Physics.CheckSphere (groundCheck.position, groundDistance, groundMask);
        if (isGrounded && velocity.y < 0) {
            velocity.y = -2f;
        }

        // player control
        Vector3 move = transform.right * movementInput.x + transform.forward * movementInput.y;

        controller.Move (move * speed * Time.deltaTime);

        // gravity
        velocity.y += gravity * Time.deltaTime;

        controller.Move (velocity * Time.deltaTime);
    }

    private void MoveCamera () {
        // camera movement
        float mouseX = lookInput.x * mouseSensitivity * Time.deltaTime;
        float mouseY = lookInput.y * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp (xRotation, -90f, 90f);

        mainCamera.transform.localRotation = Quaternion.Euler (xRotation, 0f, 0f); // vertical camera rotation
        playerBody.Rotate (Vector3.up * mouseX); // body rotation
    }

    public void Jump (InputAction.CallbackContext context) {
        if (isGrounded) {
            velocity.y = Mathf.Sqrt (jumpHeight * -2f * gravity);
        } else {
            if (context.phase == InputActionPhase.Started) {
                launching = true;
            } else if (context.phase == InputActionPhase.Canceled) {
                launching = false;
            }
        }
    }

    private void DropObject () {
        pulling = false;
        holding = false;
        pulledObject.isKinematic = false;
        pulledObject.transform.parent = null;
        pulledObject = null;
    }

    // Collision callback
    private void OnControllerColliderHit (ControllerColliderHit hit) {
        if (hit.gameObject.tag == "Danger") {
            gameManager.RestartLevel ();
        }
    }

    // Power callback functions

    public void Pull (InputAction.CallbackContext context) {
        if (context.phase == InputActionPhase.Started) {
            RaycastHit hit;
            if (Physics.SphereCast (mainCamera.transform.position, pushRadius, mainCamera.transform.forward, out hit, Mathf.Infinity, metalMask)) {
                pulling = true;
                pulledObject = hit.rigidbody;
                pulledObject.isKinematic = true;
            }
        } else if (context.phase == InputActionPhase.Canceled && pulledObject != null) {
            DropObject ();
        }

    }

    public void Push (InputAction.CallbackContext context) {
        if (context.phase == InputActionPhase.Started) {
            RaycastHit hit;
            if (Physics.SphereCast (mainCamera.transform.position, pushRadius, mainCamera.transform.forward, out hit, Mathf.Infinity, metalMask)) {
                if (pulledObject != null) {
                    DropObject ();
                }
                if (hit.rigidbody.velocity.magnitude < 10f) {
                    hit.rigidbody.AddForce ((pushStrength * mainCamera.transform.forward) / (hit.rigidbody.position - transform.position).magnitude);
                }
            }
        }
    }

    // Input Callback Functions

    public void Move (InputAction.CallbackContext context) {
        movementInput = context.ReadValue<Vector2> ();
    }

    public void Look (InputAction.CallbackContext context) {
        lookInput = context.ReadValue<Vector2> ();
    }
}