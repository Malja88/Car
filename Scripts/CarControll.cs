using System;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

public enum Axel
{ 
    Front, Rear
}

public class CarControll : MonoBehaviour
{
    [Serializable]
    public struct Wheel
    {
        public GameObject wheelType;
        public WheelCollider wheelCollider;
        public Axel axel;
    }

    [Header("Car movement variables")]
    [SerializeField] public float maxAcceleration = 30;
    [SerializeField] private float breakAcceleration = 50;
    [SerializeField] public float turnSensitivity = 1.0f;
    [SerializeField] public float maxSteerAngle = 30.0f;
    [SerializeField] public float speed;

    [Header("Input variables")]
    [HideInInspector] private float moveInput;
    [HideInInspector] private float steerInput;
    [SerializeField] private Controls controls;

    [Header("Miscelanious")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] public List<Wheel> wheelList;

    [Header("Disposables for Updates")]
    public CompositeDisposable moveDisposable = new CompositeDisposable();
    public CompositeDisposable steerDisposable = new CompositeDisposable();
    public CompositeDisposable brakeDisposable = new CompositeDisposable();
    public CompositeDisposable moveControlsDisposable = new CompositeDisposable();
    void Awake()
    {
        controls = new Controls();
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = Vector3.zero; 
        controls.Car.Acceleration.Enable();
        controls.Car.Steering.Enable();         
    }
    private void Start()
    {
        Observable.EveryLateUpdate().Subscribe(_ => { Move(); }).AddTo(moveDisposable);
        Observable.EveryLateUpdate().Subscribe(_ => { Steer(); }).AddTo(steerDisposable);
        Observable.EveryLateUpdate().Subscribe(_ => { Brake(); }).AddTo(brakeDisposable);
        Observable.EveryUpdate().Subscribe(_ => { MoveControls(); }).AddTo(moveControlsDisposable);
    }

    void MoveControls()
    {
        Vector2 accelerationAxis = controls.Car.Acceleration.ReadValue<Vector2>();
        moveInput = accelerationAxis.y;
        Vector2 steeringAxis = controls.Car.Steering.ReadValue<Vector2>();
        steerInput = steeringAxis.x;
    }

    void Move()
    {
        foreach (var wheel in wheelList)
        {
            wheel.wheelCollider.motorTorque = moveInput * speed * maxAcceleration * Time.deltaTime;
        }
    }

    void Steer()
    {
        foreach (var wheel in wheelList)
        {
            if (wheel.axel == Axel.Front)
            {
                var _steerAngle = steerInput * turnSensitivity * maxSteerAngle;
                wheel.wheelCollider.steerAngle = Mathf.Lerp(wheel.wheelCollider.steerAngle, _steerAngle, 0.6f);
                
            }
        }
    }

    public void Brake()
    {
        if (Input.GetKey(KeyCode.Space) || moveInput == 0)
        {
            foreach (var wheel in wheelList)
            {
                wheel.wheelCollider.brakeTorque = 300 * breakAcceleration * Time.deltaTime;
            }
        }
        else
        {
            foreach (var wheel in wheelList)
            {
                wheel.wheelCollider.brakeTorque = 0;
            }
        }
    }
}
