using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;


public class CarController : NetworkBehaviour
{

    

    PlayerControls carControl;
    public GameObject camera;
    public GameObject cameraPosition;


    public bool isBoosting, applyBoost, stopped, raceend, reverseSteer;
    public int boostCounter, hitCounter;
    private float boosttime, holdDownTime, correctedSpeed;
    public float maxSpeed, standtime, reverseValue, reverseSteerTime;

    private float horizontalInput;
    private float verticalInput;
    private float currentSteerAngle;
    private float currentbreakForce;
    public bool isBreaking;
    private float massCenter = -0.2f;
    private Rigidbody carBody;
    public Vector2 moveDirection;
  


    [SerializeField] private float motorForce;
    [SerializeField] private float breakForce;
    [SerializeField] private float maxSteerAngle;

    [SerializeField] private WheelCollider frontLeftWheelCollider;
    [SerializeField] private WheelCollider frontRightWheelCollider;
    [SerializeField] private WheelCollider rearLeftWheelCollider;
    [SerializeField] private WheelCollider rearRightWheelCollider;

    [SerializeField] private Transform frontLeftWheelTransform;
    [SerializeField] private Transform frontRightWheeTransform;
    [SerializeField] private Transform rearLeftWheelTransform;
    [SerializeField] private Transform rearRightWheelTransform;
    [SerializeField] private Transform frontLeft;
    [SerializeField] private Transform frontright;
    [SerializeField] private Transform rearleft;
    [SerializeField] private Transform rearright;

    [SerializeField] private GameObject boostBody;
    [SerializeField] private GameObject WheelPrefab;
    [SerializeField] private GameObject WheelPrefab2;
    [SerializeField] private GameObject wheelRightsPrefab;
    [SerializeField] private GameObject wheelRightsPrefab2;
    [SerializeField] private GameObject wheelParentPrefab;
    [SerializeField] private GameObject wheelParentPrefab2;
    [SerializeField] private GameObject wheelParentPrefab3;
    [SerializeField] private GameObject wheelParentPrefab4;
    [SerializeField] private GameObject steeringWheel;
    public GameObject transforms;


    

    private void Awake()
    {
        carControl = new PlayerControls();
        carControl.carControlling_onKeyboard.move.performed += ctx => moveDirection = ctx.ReadValue<Vector2>();
        carControl.carControlling_onKeyboard.move.canceled += ctx => moveDirection = Vector2.zero;
    }

    private void Start()
    {
        
       
        //if (IsHost) carSetup();
        carBody = GetComponent<Rigidbody>();
        carBody.centerOfMass = new Vector3(0, massCenter, 0);
        isBoosting = false;
        applyBoost = false;
        holdDownTime = 0.5f;
        stopped = false;
        standtime = 3f;
        reverseSteerTime = 4f;
        reverseValue = 1f;
        maxSpeed = 20f;




    }
    private void FixedUpdate()
    {
        


            GetInput();
            HandleMotor();
            HandleSteering();
            UpdateWheels();
            bending();
            clampspeed();
            hitCount();
            cameraFollow();
            carBody.isKinematic = false;


            if (isBoosting == true && boostCounter <= 2)
            {

                countboost();
                
            }
            if (applyBoost == true && boostCounter > 0)
            {
                applyboost();
                cameraBoost();
            }


            if (reverseSteer == true)
            {
                reverseSteering();
            }
            if (reverseSteer == false)
            {
                reverseSteerTime = 4f;
            }
        


    }


    private void GetInput()
    {
        horizontalInput = moveDirection.x;
        verticalInput = moveDirection.y;
    }

    private void HandleMotor()
    {
        var velocity = carBody.velocity;
        var localvel = carBody.transform.InverseTransformDirection(velocity);


        if(localvel.z > 0 && moveDirection.y < 0)
        {
            currentbreakForce = moveDirection.y*-1*breakForce;
            ApplyBreaking();
            isBreaking = true;
        
        }

        else if (localvel.z < -2 && moveDirection.y > 0)
        {
            currentbreakForce = moveDirection.y * 1 * breakForce;
            ApplyBreaking();
            isBreaking = true;
        }

        else
        {
            frontLeftWheelCollider.motorTorque = verticalInput * motorForce;
            frontRightWheelCollider.motorTorque = verticalInput * motorForce;
            rearLeftWheelCollider.motorTorque = verticalInput * motorForce;
            rearRightWheelCollider.motorTorque = verticalInput * motorForce;
            currentbreakForce = 0;
            isBreaking = false;
            ApplyBreaking();

        }
     
    }

    private void ApplyBreaking()
    {
        frontRightWheelCollider.brakeTorque = currentbreakForce;
        frontLeftWheelCollider.brakeTorque = currentbreakForce;
        rearLeftWheelCollider.brakeTorque = currentbreakForce;
        rearRightWheelCollider.brakeTorque = currentbreakForce;
    }

    private void HandleSteering()
    {
        lerpSteering();
        currentSteerAngle = maxSteerAngle * horizontalInput* reverseValue;
        frontLeftWheelCollider.steerAngle = currentSteerAngle;
        frontRightWheelCollider.steerAngle = currentSteerAngle;
    }

    private void UpdateWheels()
    {
        UpdateSingleWheel(frontLeftWheelCollider, frontLeftWheelTransform);
        UpdateSingleWheel(frontRightWheelCollider, frontRightWheeTransform);
        UpdateSingleWheel(rearRightWheelCollider, rearRightWheelTransform);
        UpdateSingleWheel(rearLeftWheelCollider, rearLeftWheelTransform);
    }

    private void UpdateSingleWheel(WheelCollider wheelCollider, Transform wheelTransform)
    {
        Vector3 pos;
        Quaternion rot
;       wheelCollider.GetWorldPose(out pos, out rot);
        wheelTransform.rotation = rot;
        wheelTransform.position = pos;



    }

    private void bending()
    {
        frontLeft.transform.Rotate(0, 0, horizontalInput * 20 * -1, Space.Self);
        frontright.transform.Rotate(0, 0, horizontalInput * 20 * -1, Space.Self);
        rearleft.transform.Rotate(0, 0, horizontalInput * 20 * -1, Space.Self);
        rearright.transform.Rotate(0, 0, horizontalInput * 20 * -1, Space.Self);
    }
    void clampspeed()
    {
        if (carBody.velocity.magnitude > correctedSpeed)
        {
            carBody.velocity = Vector3.ClampMagnitude(carBody.velocity, correctedSpeed);
        }
    }
    void countboost()
    {
        if (verticalInput > 0)
        {

            holdDownTime -= Time.deltaTime;
            if (holdDownTime <= 0)
            {
                boostCounter += 1;
                holdDownTime = 0.5f;
                boosttime += 1;

            }
        }

    }
    void applyboost()
    {

        if (boosttime > 0)
        {

            boostBody.SetActive(true);
           maxSpeed = 50;
           carBody.AddRelativeForce(Vector3.forward * 30000);

            


            boosttime -= Time.deltaTime;
        }
        if (boosttime <= 0)
        {
            boostBody.SetActive(false);
            boostCounter = 0;
            maxSpeed = 20f;
            boosttime = 0;
            applyBoost = false;



        }


    }


    private void hitCount()
    {
        if (hitCounter > 3) hitCounter = 3;
        if(hitCounter <= 3 && maxSpeed > 0)
        {
            correctedSpeed = maxSpeed -hitCounter * 3;
        }
        if(maxSpeed == 0)
        {
            correctedSpeed = 0;
        }

    }



    void cameraFollow()
    {

        camera.SetActive(true);
        camera.transform.position = cameraPosition.transform.position;
        camera.transform.localRotation = Quaternion.Euler(cameraPosition.transform.localRotation.x, cameraPosition.transform.localRotation.y - moveDirection.x * 2, cameraPosition.transform.localRotation.z);
    }



    void lerpSteering()
    {
        if(carBody.velocity.magnitude < 25f) maxSteerAngle = 30 - carBody.velocity.magnitude;
        if (carBody.velocity.magnitude > 25f) maxSteerAngle = 5;
    }


    void cameraBoost()
    {
        if(isBoosting == true)
        {
            Animator cameraAnim = cameraPosition.GetComponent<Animator>();
            cameraAnim.speed = 1f;
            cameraAnim.Play("Camera");

        }
    }

    void reverseSteering()
    {
        reverseSteerTime -= Time.deltaTime;
        if (reverseSteerTime >= 0) reverseValue = -1f;
        if(reverseSteerTime <0)
        {
            reverseValue = 1f;
            reverseSteer = false;
        }

    }

    private void OnEnable()
    {
        carControl.carControlling_onKeyboard.Enable();
    }
    private void OnDisable()
    {
        carControl.carControlling_onKeyboard.Disable();
    }




    /*[ServerRpc]
    private void carSetupServerRpc()
    {
        Instantiate(wheelParentPrefab);
        wheelParentPrefab.GetComponent<NetworkObject>().SpawnWithOwnership(NetworkManager.Singleton.LocalClientId);
        wheelParentPrefab.GetComponent<NetworkObject>().TrySetParent(gameObject);
        wheelParentPrefab.transform.localPosition = new Vector3(-0.802f, 0.464f, 1.537f);

       
        Instantiate(WheelPrefab);
        WheelPrefab.GetComponent<NetworkObject>().SpawnWithOwnership(NetworkManager.Singleton.LocalClientId);
        WheelPrefab.GetComponent<NetworkObject>().TrySetParent(frontLeft);
        WheelPrefab.transform.localPosition = new Vector3(0, 0, 0);
        WheelPrefab.transform.localScale = new Vector3(200f,200f,200f);




        Instantiate(wheelParentPrefab2);
        wheelParentPrefab2.GetComponent<NetworkObject>().SpawnWithOwnership(NetworkManager.Singleton.LocalClientId);
        wheelParentPrefab2.GetComponent<NetworkObject>().TrySetParent(gameObject);
        wheelParentPrefab2.transform.localPosition = new Vector3(0.846f, 0.464f, 1.537f);



        Instantiate(wheelRightsPrefab);
        wheelRightsPrefab.GetComponent<NetworkObject>().SpawnWithOwnership(NetworkManager.Singleton.LocalClientId);
        wheelRightsPrefab.GetComponent<NetworkObject>().TrySetParent(frontright);
        wheelRightsPrefab.transform.localPosition = new Vector3(0, 0, 0);
        wheelRightsPrefab.transform.rotation = Quaternion.Euler(transform.rotation.x, 180f, transform.rotation.z);
        wheelRightsPrefab.transform.localScale = new Vector3(200f, 200f, 200f);





        Instantiate(wheelParentPrefab3);
        wheelParentPrefab3.GetComponent<NetworkObject>().SpawnWithOwnership(NetworkManager.Singleton.LocalClientId);
        wheelParentPrefab3.GetComponent<NetworkObject>().TrySetParent(gameObject);
        wheelParentPrefab3.transform.localPosition = new Vector3(-0.802f, 0.464f, -0.872f);



        Instantiate(WheelPrefab2);
        WheelPrefab2.GetComponent<NetworkObject>().SpawnWithOwnership(NetworkManager.Singleton.LocalClientId);
        WheelPrefab2.GetComponent<NetworkObject>().TrySetParent(rearleft);
        WheelPrefab2.transform.localPosition = new Vector3(0, 0, 0);
        WheelPrefab2.transform.localScale = new Vector3(200f, 200f, 200f);




        Instantiate(wheelParentPrefab4);
        wheelParentPrefab4.GetComponent<NetworkObject>().SpawnWithOwnership(NetworkManager.Singleton.LocalClientId);
        wheelParentPrefab4.GetComponent<NetworkObject>().TrySetParent(gameObject);
        wheelParentPrefab4.transform.localPosition = new Vector3(0.846f, 0.464f, -0.872f);



        Instantiate(wheelRightsPrefab2);
        wheelRightsPrefab2.GetComponent<NetworkObject>().SpawnWithOwnership(NetworkManager.Singleton.LocalClientId);
        wheelRightsPrefab2.GetComponent<NetworkObject>().TrySetParent(rearright);
        wheelRightsPrefab2.transform.localPosition = new Vector3(0, 0, 0);
        wheelRightsPrefab2.transform.rotation = Quaternion.Euler(transform.rotation.x, 180f, transform.rotation.z);
        wheelRightsPrefab2.transform.localScale = new Vector3(200f, 200f, 200f);





        Instantiate(steeringWheel);
        steeringWheel.GetComponent<NetworkObject>().SpawnWithOwnership(NetworkManager.Singleton.LocalClientId);
        steeringWheel.GetComponent<NetworkObject>().TrySetParent(gameObject);
        steeringWheel.transform.localPosition = new Vector3(0.03160782f, 0.9668704f, 0.7517098f);
        steeringWheel.transform.localScale = new Vector3(3.555391f, 4.577719f, 3.033239f);
        steeringWheel.transform.localRotation = Quaternion.Euler(-0.762f, -90f, -89.102f);


    }*/


    /*private void carSetup()
    {
        transforms.SetActive(false);
        
        GameObject wheelParent1 = Instantiate(wheelParentPrefab);
        wheelParent1.GetComponent<NetworkObject>().Spawn();
        wheelParent1.GetComponent<NetworkObject>().TrySetParent(gameObject);
        wheelParent1.transform.localPosition = new Vector3(-0.802f, 0.464f, 1.537f);

        frontLeft = wheelParent1.transform;
        GameObject frontLeftWheel = Instantiate(WheelPrefab);
        frontLeftWheel.GetComponent<NetworkObject>().Spawn();
        frontLeftWheel.GetComponent<NetworkObject>().TrySetParent(frontLeft);
        frontLeftWheel.transform.localPosition = new Vector3(0, 0, 0);
        frontLeftWheel.transform.localScale = new Vector3(200f, 200f, 200f);
        frontLeftWheelTransform = frontLeftWheel.transform;




        GameObject wheelParent2 = Instantiate(wheelParentPrefab2);
        wheelParent2.GetComponent<NetworkObject>().Spawn();
        wheelParent2.GetComponent<NetworkObject>().TrySetParent(gameObject);
        wheelParent2.transform.localPosition = new Vector3(0.846f, 0.464f, 1.537f);


        frontright = wheelParent2.transform;
        GameObject frontRightWheel = Instantiate(wheelRightsPrefab);
        frontRightWheel.GetComponent<NetworkObject>().Spawn();
        frontRightWheel.GetComponent<NetworkObject>().TrySetParent(frontright);
        frontRightWheel.transform.localPosition = new Vector3(0, 0, 0);
        frontRightWheel.transform.rotation = Quaternion.Euler(transform.rotation.x, 180f, transform.rotation.z);
        frontRightWheel.transform.localScale = new Vector3(200f, 200f, 200f);
        frontRightWheeTransform = frontRightWheel.transform;




        GameObject wheelParent3 = Instantiate(wheelParentPrefab3);
        wheelParent3.GetComponent<NetworkObject>().Spawn();
        wheelParent3.GetComponent<NetworkObject>().TrySetParent(gameObject);
        wheelParent3.transform.localPosition = new Vector3(-0.802f, 0.464f, -0.872f);
        rearleft = wheelParent3.transform;
        GameObject rearLeftWheel = Instantiate(WheelPrefab2, rearleft);
        rearLeftWheel.GetComponent<NetworkObject>().Spawn();
        rearLeftWheel.GetComponent<NetworkObject>().TrySetParent(rearleft);
        rearLeftWheel.transform.localPosition = new Vector3(0, 0, 0);
        rearLeftWheel.transform.localScale = new Vector3(200f, 200f, 200f);
        rearLeftWheelTransform = rearLeftWheel.transform;



        GameObject wheelParent4 = Instantiate(wheelParentPrefab4);
        wheelParent4.GetComponent<NetworkObject>().Spawn();
        wheelParent4.GetComponent<NetworkObject>().TrySetParent(gameObject);
        wheelParent4.transform.localPosition = new Vector3(0.846f, 0.464f, -0.872f);
        rearright = wheelParent4.transform;
        GameObject rearRightWheel = Instantiate(wheelRightsPrefab2, rearright);
        rearRightWheel.GetComponent<NetworkObject>().Spawn();
        rearRightWheel.GetComponent<NetworkObject>().TrySetParent(rearright);
        rearRightWheel.transform.localPosition = new Vector3(0, 0, 0);
        frontRightWheel.transform.rotation = Quaternion.Euler(transform.rotation.x, 180f, transform.rotation.z);
        rearRightWheel.transform.localScale = new Vector3(200f, 200f, 200f);
        rearRightWheelTransform = rearRightWheel.transform;




        GameObject wheelPref = Instantiate(steeringWheel);
        wheelPref.GetComponent<NetworkObject>().Spawn();
        wheelPref.GetComponent<NetworkObject>().TrySetParent(gameObject);
        wheelPref.transform.localPosition = new Vector3(0.03160782f, 0.9668704f, 0.7517098f);
        wheelPref.transform.localScale = new Vector3(3.555391f, 4.577719f, 3.033239f);
        wheelPref.transform.localRotation = Quaternion.Euler(-0.762f, -90f, -89.102f);
        gameObject.GetComponent<steeringWheel>().steeringWheelPrefab = steeringWheel;

    }*/




}
