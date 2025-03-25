using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float walkSpeed = 2f;
    [SerializeField] private float runSpeed = 10f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float rotationSpeed = 40f; 
    private Rigidbody rb;
    public bool isGrounded = true;
    private bool jumpInput = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("El Rigidbody no está asignado.");
        }

        // Congelar la rotación en los ejes X e Y
        rb.freezeRotation = true;
    }

    void Update()
    {
        jumpInput = Input.GetKeyDown(KeyCode.Space);
        HandleRotation(); 
    }

    void FixedUpdate()
    {
        Move();
        Jump();
    }

    void Move()
    {
        float moveZ = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(0, 0, moveZ).normalized; // Solo movimiento adelante/atrás
        float speed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;

        Vector3 velocity = transform.forward * moveZ * speed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + velocity);
    }

    void HandleRotation()
    {
        float rotation = Input.GetAxis("Horizontal"); // Usa el input horizontal para la rotación
        transform.Rotate(Vector3.up, rotation * rotationSpeed * Time.deltaTime); // Rotar el personaje
    }

    void Jump()
    {
        if (jumpInput && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false;
            jumpInput = false;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Está tocando el suelo");
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }

    void OnCollisionExit(Collision collision)
    {
        Debug.Log("No está tocando el suelo");
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }

}
