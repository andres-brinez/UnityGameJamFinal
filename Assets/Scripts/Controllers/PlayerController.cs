using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float walkSpeed = 2f;
    [SerializeField] private float runSpeed = 10f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float rotationSpeed = 90f;
    [SerializeField] private float speed ; // Velocidad del personaje (walkSpeed o runSpeed)
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

        HandleRun(); // Cambia la velocidad según si se presiona Shift o no

        Vector3 velocity = transform.forward * moveZ * speed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + velocity);
    }

    void HandleRotation()
    {
        float rotation = Input.GetAxis("Horizontal"); // Usa el input horizontal para la rotación
        transform.Rotate(Vector3.up, rotation * rotationSpeed * Time.deltaTime); // Rotar el personaje
    }

    void HandleRun()
    {
        //speed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed; // Si se presiona la tecla Shift, la velocidad es runSpeed, de lo contrario, es walkSpeed

        // Cambia la velocidad según si se presiona Shift o no
        if (Input.GetKey(KeyCode.LeftShift))
        {
            speed = runSpeed;
            Debug.Log("Corriendo");
        }
        else
        {
            speed = walkSpeed;
        }
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
