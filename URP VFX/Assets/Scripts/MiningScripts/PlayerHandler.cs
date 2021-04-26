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

    public bool onGround;

    public bool hoverAvailable = false;
    public bool justJump = false;
    public bool hovering = false;
    public float hoverDelay = 1;
    public float hoverCeilingMod = 10f;
    float currentHoverCeiling;

    Rigidbody playerBod;

    private void Awake()
    {
        playerBod = gameObject.GetComponent<Rigidbody>();
        Physics.gravity = new Vector3(0,-30,0);
        controls = new PlayerControls();
        controls.Movement.Jump.started += ctx => Jump();
        controls.Movement.Jump.canceled += ctx => Drop();



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
        if (hovering && Mathf.Approximately(transform.position.y, currentHoverCeiling))
        {
            playerBod.constraints = RigidbodyConstraints.FreezePosition | RigidbodyConstraints.FreezeRotation;
        }

    }
    void Jump()
    {
        if (!hoverAvailable && onGround)
        {
            playerBod.velocity = new Vector3(0, jumpMod, 0);
            justJump = true;
            StartCoroutine(jumpCoolDownHandler());
            //hoverAvailable = true;
        }
        if (justJump && hoverAvailable)
        {
            playerBod.useGravity = false;
            currentHoverCeiling = transform.position.y + hoverCeilingMod;
            Debug.Log(currentHoverCeiling);
        }

    }

    void Drop()
    {
        if (hoverAvailable && !onGround)
        {
            playerBod.constraints = RigidbodyConstraints.FreezeRotation;
            playerBod.useGravity = true;
        }
    }

    IEnumerator jumpCoolDownHandler()
    {
        if (!onGround)
        {
            yield return new WaitForSeconds(hoverDelay);       
        }
        hoverAvailable = true;

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
            hoverAvailable = false;
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
