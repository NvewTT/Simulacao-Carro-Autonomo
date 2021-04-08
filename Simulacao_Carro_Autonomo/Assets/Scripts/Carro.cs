using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Carro : MonoBehaviour
{
    private static Carro Instance;

    public WheelCollider wheelColliderLeftFront;
    public WheelCollider wheelColliderRightFront;
    public WheelCollider wheelColliderLeftBack;
    public WheelCollider wheelColliderRightBack;

    private Rigidbody rigidbody;

    public Transform whellTransformLeftFront;
    public Transform whellTransformRightFront;
    public Transform whellTransformLeftBack;
    public Transform whellTransformRightBack;

    public float motorTorque = 100f;
    public float maxSteer = 20f;

    public float maxspeed = 100f;

    public float currentspeed;

    private float steer = 0f;
    private void Awake()
    {
        Instance = this;
        rigidbody = GetComponent<Rigidbody>();
    }

    private void SetSteer(float steer)
    {
        this.steer = steer/1000;
        
    }

    public static void SetSteerS(float steer)
    {
        Instance.SetSteer(steer);
    }

    private float getVelocit()
    {
        return (currentspeed);
    }

    public static float getVelocitS()
    {
        return (Instance.getVelocit());
    }

    private List<Vector3> GetAllpos()
    {
        List<Vector3> posalll = new List<Vector3>();
        posalll.Add(transform.position);
        posalll.Add(transform.rotation.eulerAngles);
        posalll.Add(rigidbody.velocity);
        posalll.Add(rigidbody.angularVelocity);
        return (posalll);
           
    }

    public static List<Vector3> GetAllposS()
    {
        return (Instance.GetAllpos());
    }

    private void FixedUpdate()
    {
        currentspeed = rigidbody.velocity.sqrMagnitude;
        if(currentspeed < maxspeed)
        {
            wheelColliderLeftBack.motorTorque = motorTorque;
            wheelColliderRightBack.motorTorque = motorTorque;
        }
        else
        {
            wheelColliderLeftBack.motorTorque = 0;
            wheelColliderRightBack.motorTorque = 0;
        }
        
        wheelColliderLeftFront.steerAngle = steer * maxSteer;
        wheelColliderRightFront.steerAngle = steer * maxSteer;
    }

    private void Update()
    {

        var pos = Vector3.zero;
        var rot = Quaternion.identity;

        wheelColliderLeftFront.GetWorldPose(out pos, out rot);
        whellTransformLeftFront.position = pos;
        whellTransformLeftFront.rotation = rot;

        wheelColliderRightFront.GetWorldPose(out pos, out rot);
        whellTransformRightFront.position = pos;
        whellTransformRightFront.rotation = rot * Quaternion.Euler(0,180,0);

        wheelColliderLeftBack.GetWorldPose(out pos, out rot);
        whellTransformLeftBack.position = pos;
        whellTransformLeftBack.rotation = rot;

        wheelColliderRightBack.GetWorldPose(out pos, out rot);
        whellTransformRightBack.position = pos;
        whellTransformRightBack.rotation = rot * Quaternion.Euler(0, 180, 0);
    }

}
