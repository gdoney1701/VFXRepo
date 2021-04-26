using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerHandler : MonoBehaviour
{
    PlayerControls controls;
    public Vector2 move;
    public float speedMod;

    private void Awake()
    {
        controls = new PlayerControls();
        controls.Movement.Jump.performed += ctx => Jump();

        controls.Movement.Move.performed += ctx => move = ctx.ReadValue<Vector2>();
        controls.Movement.Move.canceled += ctx => move = Vector2.zero;
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 m = new Vector3(move.x, 0, move.y) * Time.deltaTime * speedMod;
        transform.Translate(m, Space.World);
    }
    void Jump()
    {

    }

    private void OnEnable()
    {
        controls.Movement.Enable();
    }
    private void OnDisable()
    {
        controls.Movement.Disable();
    }
}
