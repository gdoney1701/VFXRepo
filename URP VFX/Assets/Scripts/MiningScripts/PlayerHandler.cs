using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerHandler : MonoBehaviour
{
    PlayerControls controls;
    public Vector2 move;
    public float speedMod;
    public float jumpMod;

    bool onGround;

    bool hoverMode = false;
    bool justJump = false;
    public float hoverDelay = 1;

    private void Awake()
    {
        Physics.gravity = new Vector3(0,-30,0);
        controls = new PlayerControls();
        if (!hoverMode)
        {
            controls.Movement.Jump.performed += ctx => Jump();
        }
        else
        {
            controls.Movement.Jump.started += ctx => Hover();
        }


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
        gameObject.GetComponent<Rigidbody>().velocity = new Vector3(0, jumpMod, 0);
        justJump = true;
        StartCoroutine(jumpCoolDownHandler()); 
    }
    void Hover()
    {
        Debug.Log("Hover Mode Active");
    }

    IEnumerator jumpCoolDownHandler()
    {
        if (!onGround)
        {
            yield return new WaitForSeconds(hoverDelay);
            hoverMode = true;
        }

    }

    private void OnEnable()
    {
        controls.Movement.Enable();
    }
    private void OnDisable()
    {
        controls.Movement.Disable();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            onGround = true;
            hoverMode = false;
            justJump = false;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if(collision.collider.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            onGround = false;
        }
    }
}
