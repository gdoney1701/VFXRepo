using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerHandler : MonoBehaviour
{
    PlayerControls controls;
    Renderer playerMat;
    public Vector2 move;
    public float speedMod;
    public float jumpMod;

    bool onGround;
    bool hoverAvailable = false;
    bool justJump = false;
    public bool hovering = false;
    bool firingLaser = false;

    public float hoverDelay = 1;
    public float hoverCeilingMod = 10f;
    float currentHoverCeiling;

    public GameObject laserHole;
    public GameObject laserBeam;
    GameObject currentBeam;

    Rigidbody playerBod;


    private void Awake()
    {
        playerMat = gameObject.GetComponent<Renderer>();
        playerMat.material.color = Color.white;
        playerBod = gameObject.GetComponent<Rigidbody>();
        Physics.gravity = new Vector3(0,-30,0);
        controls = new PlayerControls();
        controls.Movement.Jump.started += ctx => Jump();
        controls.Movement.Jump.canceled += ctx => Drop();
        controls.Movement.Laser.started += ctx => FireMainWeapon();
        controls.Movement.Laser.canceled += ctx => EndMainWeapon();

        controls.Movement.Move.performed += ctx => move = ctx.ReadValue<Vector2>();
        controls.Movement.Move.canceled += ctx => move = Vector2.zero;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 m = new Vector3(move.x, 0, move.y) * Time.deltaTime * speedMod;
        transform.Translate(m, Space.World);
        if (hovering && transform.position.y >= currentHoverCeiling)
        {
            playerBod.constraints = RigidbodyConstraints.FreezePosition | RigidbodyConstraints.FreezeRotation;
        }

    }
    void Jump()
    {
        if (!hoverAvailable && onGround)
        {
            playerMat.material.color = Color.magenta;
            playerBod.velocity = new Vector3(0, jumpMod, 0);
            justJump = true;
            StartCoroutine(jumpCoolDownHandler());
        }
        if (!hovering && hoverAvailable)
        {
            playerBod.useGravity = false;
            if (justJump)
            {
                currentHoverCeiling = transform.position.y + hoverCeilingMod;            
            }
            playerBod.velocity += new Vector3(0, Mathf.Abs(playerBod.velocity.y), 0);
            Debug.Log(currentHoverCeiling);
            playerMat.material.color = Color.green;
            hovering = true;
        }

    }

    void Drop()
    {
        if (hoverAvailable && !onGround)
        {
            playerMat.material.color = Color.blue;
            playerBod.constraints = RigidbodyConstraints.FreezeRotation;
            playerBod.useGravity = true;
            justJump = false;
            hovering = false;
        }
    }

    IEnumerator jumpCoolDownHandler()
    {
        Debug.Log("Jumping Cooldown");
        yield return new WaitForSeconds(hoverDelay);
        Debug.Log("Cooldown Complete");
        hoverAvailable = true;
        playerMat.material.color = Color.yellow;

    }
    void FireMainWeapon()
    {
        if (hovering)
        {
            Debug.Log("Targeting Weapon");
            RaycastHit hit;
            Debug.DrawRay(gameObject.transform.position, transform.TransformDirection(Vector3.down)*transform.position.y, Color.red, 20);
            if (Physics.Raycast(gameObject.transform.position, transform.TransformDirection(Vector3.down), out hit, Mathf.Infinity))
            {
                
                Debug.Log("Target Acquired");
                firingLaser = true;
                //currentBeam =  Instantiate(laserBeam, gameObject.transform);
                GameObject hole = Instantiate(laserHole, new Vector3(transform.position.x, hit.transform.position.y, transform.position.z), gameObject.transform.rotation);
            }
        }
    }
    void EndMainWeapon()
    {
        if (firingLaser)
        {
            //Destroy(currentBeam);
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
            playerMat.material.color = Color.white;
            onGround = true;
            hoverAvailable = false;
            justJump = false;
            hovering = false;
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
