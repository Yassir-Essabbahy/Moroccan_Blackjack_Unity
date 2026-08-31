using UnityEngine;

public class CarController : MonoBehaviour
{
    [Header("Movement")]
    public float acceleration = 10f;
    public float maxSpeed = 6f;
    public float turnSpeed = 8f;

    [Header("Grounding")]
    public float rayLength = 2.5f;
    public LayerMask groundLayer;
    public Rigidbody rb;

    private float moveInput;
    private float turnInput;
    private bool isGrounded;

    public float tiltSmoothness = 1f;

    void Start()
    {
        // Move center of mass down a little to prevent weird movement
        rb.centerOfMass = new Vector3(0f, -1f, 0f);

        // Lock Cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        moveInput = Input.GetAxis("Vertical");
        turnInput = Input.GetAxis("Horizontal");
    }

    private void FixedUpdate()
    {
        // Raycast to ground
        RaycastHit hit;

        isGrounded = Physics.Raycast(
            transform.position,
            Vector3.down,
            out hit,
            rayLength,
            groundLayer
        );

        if (isGrounded)
        {
            // Movement
            if (Mathf.Abs(moveInput) > 0.1f)
            {
                // Add force in the direction the car is facing
                rb.AddForce(
                    transform.forward * moveInput * acceleration,
                    ForceMode.Acceleration
                );
            }

            // Steering only when moving
            if (rb.linearVelocity.magnitude > 0.5f)
            {
                // If moving backward, reverse the steering
                float direction =
                    Vector3.Dot(rb.linearVelocity, transform.forward) > 0 ? 1 : -1;

                float turn =
                    turnInput * turnSpeed * direction * Time.fixedDeltaTime;

                transform.Rotate(0, turn, 0);
            }

            // Calculate the rotation we want to be at
            Quaternion targetRotation =
                Quaternion.FromToRotation(transform.up, hit.normal)
                * transform.rotation;

            // Force Z to 0 so we don't roll sideways
            Vector3 euler = targetRotation.eulerAngles;
            euler.z = 0;

            Quaternion finalTarget = Quaternion.Euler(euler);

            // Make the car lean on hills and bumps smoothly
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                finalTarget,
                tiltSmoothness * Time.fixedDeltaTime
            );
        }

        // Max speed clamp
        if (rb.linearVelocity.magnitude > maxSpeed)
        {
            rb.linearVelocity =
                rb.linearVelocity.normalized * maxSpeed;
        }

        // Anti-slide / fake brakes
        if (moveInput == 0 && rb.linearVelocity.magnitude < 2f)
        {
            rb.linearVelocity = Vector3.Lerp(
                rb.linearVelocity,
                Vector3.zero,
                Time.fixedDeltaTime * 5f
            );
        }
    }
}