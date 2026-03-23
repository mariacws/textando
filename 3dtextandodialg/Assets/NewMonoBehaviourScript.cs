using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private Rigidbody rb;
    private InputSystem_Actions inputActions;
    public float speed = 10f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        inputActions = new InputSystem_Actions();
        inputActions.Player.Enable();
    }

    void FixedUpdate()
    {
        Vector2 move = inputActions.Player.Move.ReadValue<Vector2>();
        rb.AddTorque(new Vector3(move.y, 0, -move.x) * speed);
    }

    void OnDestroy()
    {
        inputActions.Dispose();
    }
}
