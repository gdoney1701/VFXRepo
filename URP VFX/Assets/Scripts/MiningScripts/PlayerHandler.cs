using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;

public class PlayerHandler : MonoBehaviour
{
    PlayerControls controls;
    ImpactWaveHandler waveHandler;
    public CinemachineVirtualCamera followCam;
    public CinemachineVirtualCamera overheadCam;
    Renderer playerMat;
    Vector2 move;
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

    public float maxDownThrust = 10;
    float currentThrustStored = 0;
    float thrustTime = 0;
    bool thrustCharge = false;

    Rigidbody playerBod;


    private void Awake()
    {
        waveHandler = gameObject.GetComponent<ImpactWaveHandler>();
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

        controls.Movement.Prospect.started += ctx => DownSmash(true);
        controls.Movement.Prospect.canceled += ctx => DownSmash(false);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector3 m = new Vector3(move.x, 0, move.y) * Time.deltaTime * speedMod;
        if (thrustCharge)
        {

            thrustTime += Time.deltaTime;
            if (currentThrustStored < maxDownThrust)
            {
                currentThrustStored += Mathf.Pow(thrustTime, 2);
                overheadCam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_AmplitudeGain = Mathf.Lerp(0, 4, currentThrustStored / maxDownThrust);
                overheadCam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_FrequencyGain = Mathf.Lerp(0, 10, currentThrustStored / maxDownThrust);
            }
        }
        if (onGround)
        {        
            transform.Translate(m, Space.World);
        }
        if (!onGround && !hovering)
        {
            playerBod.velocity += m*1.5f;
        }
        if (hovering)
        {
            playerBod.velocity += m * 3;
        }
        
        if (hovering && transform.position.y >= currentHoverCeiling)
        {
            playerBod.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotation;
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
            playerMat.material.color = Color.green;
            hovering = true;
            playerBod.drag = 5;
            overheadCam.gameObject.SetActive(true);
        }

    }
    void DownSmash(bool charging)
    {
        if (hovering)
        {
            if (charging)
            {
                playerBod.constraints = RigidbodyConstraints.FreezePosition | RigidbodyConstraints.FreezeRotation;
                Debug.Log("Charging");
                thrustCharge = true;

            }
            else
            {
                thrustCharge = false;
                float velToAdd = currentThrustStored;
                Drop();
                waveHandler.expectImpact = true;
                playerBod.velocity += new Vector3(0, -velToAdd, 0);
                overheadCam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_AmplitudeGain = 0;
                overheadCam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_FrequencyGain = 0;
            }

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
            playerBod.drag = 0;
            overheadCam.gameObject.SetActive(false);
            currentThrustStored = 0;
            thrustTime = 0;
        }
    }

    IEnumerator jumpCoolDownHandler()
    {
        yield return new WaitForSeconds(hoverDelay);
        hoverAvailable = true;
        playerMat.material.color = Color.yellow;

    }
    void FireMainWeapon()
    {
        if (hovering)
        {
            RaycastHit hit;
            Debug.DrawRay(gameObject.transform.position, transform.TransformDirection(Vector3.down)*transform.position.y, Color.red, 20);
            if (Physics.Raycast(gameObject.transform.position, transform.TransformDirection(Vector3.down), out hit, Mathf.Infinity))
            {
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
            overheadCam.gameObject.SetActive(false);
            currentThrustStored = 0;
            thrustTime = 0;
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
