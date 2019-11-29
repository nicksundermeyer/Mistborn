using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour {
    public CharacterController controller;

    [Header ("Power Settings")]
    [SerializeField] private float pushStrength = 1000f;
    [SerializeField] private float launchVelocity = 100f;
    [SerializeField] private float launchRadius = 1f;
    [SerializeField] private float launchDistance = 5f;

    private bool launching = false;

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

    void Start () {
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update () {
        MoveCamera ();
        MovePlayer ();

        // launching
        if (launching) {
            RaycastHit hit;
            // Debug.DrawRay (mainCamera.transform.position, -Vector3.up, Color.green, 2f);

            Vector3 SphereCastStart = groundCheck.position;
            SphereCastStart.y = groundCheck.position.y + launchRadius;
            if (Physics.SphereCast (SphereCastStart, launchRadius, -Vector3.up, out hit, launchDistance, metalMask)) {
                var launch = Mathf.Clamp (launchVelocity / ((hit.transform.position - groundCheck.transform.position).magnitude), 0f, launchVelocity / 2);
                velocity.y = Mathf.Max (velocity.y, launch);
            }
        }
    }

    public void Pull (InputAction.CallbackContext context) {
        if (context.phase == InputActionPhase.Started) {
            RaycastHit hit;
            Ray ray = new Ray (mainCamera.transform.position, mainCamera.transform.forward);
            // Does the ray intersect any objects excluding the player layer
            if (Physics.Raycast (ray, out hit, Mathf.Infinity, metalMask)) {
                // Debug.DrawRay (ray.origin, ray.direction, Color.green, 2f);
                hit.rigidbody.AddForce (pushStrength * -ray.direction);
            }
        }
    }

    public void Push (InputAction.CallbackContext context) {
        if (context.phase == InputActionPhase.Started) {
            RaycastHit hit;
            Ray ray = new Ray (mainCamera.transform.position, mainCamera.transform.forward);
            // Does the ray intersect any objects excluding the player layer
            if (Physics.Raycast (ray, out hit, Mathf.Infinity, metalMask)) {
                // Debug.DrawRay (ray.origin, ray.direction, Color.green, 2f);
                hit.rigidbody.AddForce (pushStrength * ray.direction);
            }
        }
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

    public void Move (InputAction.CallbackContext context) {
        movementInput = context.ReadValue<Vector2> ();
    }

    public void Look (InputAction.CallbackContext context) {
        lookInput = context.ReadValue<Vector2> ();
    }

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

        mainCamera.transform.localRotation = Quaternion.Euler (xRotation, 0f, 0f);
        playerBody.Rotate (Vector3.up * mouseX);
    }
}