using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MotoController : MonoBehaviour
{
    [SerializeField]
    private float motorForce = 4000;
    [SerializeField]
    private float acceleration = 0.5f;
    [SerializeField]
    private float maxAcceleration = 5;
    private float currentAcceleration = 0;
    [SerializeField]
    private float rotateForce = 800;
    [SerializeField]
    private float wheelRotate = 0.65f;
    [SerializeField]
    private float suspensionRange = 1;
    [SerializeField]
    private float suspensionStrenght = 250;
    [SerializeField]
    [Range(0,0.1f)]
    private float procentFriction = 0.012f;

    [SerializeField]
    private Transform[] wheels;
    [SerializeField]
    private Transform[] wheelsRayPos;

    [SerializeField]
    private Transform center;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        //disable rigidbody sleeping to always detect collision
        //rb.sleepThreshold = 0.0f;
        rb.centerOfMass = center.localPosition;
    }

    protected void LateUpdate()
    {
        //freeze z rotation of moto
        transform.eulerAngles = new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y, 0);
    }

    void FixedUpdate()
    {
        //rotate wheel
        float speed = Vector3.Dot(rb.velocity, transform.forward);
        foreach(Transform wheel in wheels)
            wheel.Rotate(wheelRotate * speed, 0.0f,0.0f,Space.Self);

        //rotate moto to right and left
        if (Input.GetKey(KeyCode.A))
            rb.AddRelativeTorque(new Vector3(0, -rotateForce, 0));
        if (Input.GetKey(KeyCode.D))
            rb.AddRelativeTorque(new Vector3(0, rotateForce, 0));
        Suspension(wheels, wheelsRayPos);
    }

    private void Suspension(Transform[] wheels, Transform[] wheelsRayPos)
    {
        RaycastHit hit;

        //wheels to the starting position
        wheels[0].localPosition = new Vector3(wheels[0].localPosition.x, 0, wheels[0].localPosition.z);
        wheels[1].localPosition = new Vector3(wheels[1].localPosition.x, 0, wheels[1].localPosition.z);
        for (int i = 0; i < wheelsRayPos.Length; i++)
        {
            //touch ground
            if (Physics.Raycast(wheelsRayPos[i].transform.position, transform.up * -1, out hit, suspensionRange)){
                //suspension
                float distancePercentage = Mathf.Lerp(1.5f,5f,1-(hit.distance / suspensionRange));
                rb.AddForceAtPosition(transform.up * suspensionStrenght * distancePercentage, wheelsRayPos[i].transform.position);

                //move wheels to suspension
                wheels[i].localPosition = new Vector3(wheels[i].localPosition.x, suspensionRange - hit.distance, wheels[i].localPosition.z);

                //move moto forward
                if (Input.GetKey(KeyCode.W))
                    currentAcceleration += acceleration * Time.deltaTime;
                else if (Input.GetKey(KeyCode.S))
                    currentAcceleration -= acceleration * Time.deltaTime;
                else
                {
                    currentAcceleration = Mathf.MoveTowards(currentAcceleration, 0,Time.deltaTime * 2);
                }
                //speed limit
                if (currentAcceleration > maxAcceleration)
                    currentAcceleration = maxAcceleration;
                if (currentAcceleration < -maxAcceleration)
                    currentAcceleration = -maxAcceleration;
                rb.AddForce(transform.forward * currentAcceleration * motorForce);

                //friction
                rb.velocity -= (rb.velocity * procentFriction);
            }
            Debug.DrawRay(wheelsRayPos[i].transform.position, transform.up * -1 * suspensionRange, Color.red);
        }
    }
}
