using UnityEngine;
using System;
using MathNet.Numerics;
using System.Collections.Generic;

[Serializable]
public enum DriveType
{
	RearWheelDrive,
	FrontWheelDrive,
	AllWheelDrive
}

public class WheelDrive : MonoBehaviour
{
    [Tooltip("Maximum steering angle of the wheels")]
	public float maxAngle = 35f;
	[Tooltip("Maximum torque applied to the driving wheels")]
	public float maxTorque = 300f;
	[Tooltip("Maximum brake torque applied to the driving wheels")]
	public float brakeTorque = 30000f;
	[Tooltip("If you need the visual wheels to be attached automatically, drag the wheel shape here.")]
	public GameObject wheelShape;

	[Tooltip("The vehicle's speed when the physics engine can use different amount of sub-steps (in m/s).")]
	public float criticalSpeed = 5f;
	[Tooltip("Simulation sub-steps when the speed is above critical.")]
	public int stepsBelow = 5;
	[Tooltip("Simulation sub-steps when the speed is below critical.")]
	public int stepsAbove = 1;

	[Tooltip("The vehicle's drive type: rear-wheels drive, front-wheels drive or all-wheels drive.")]
	public DriveType driveType;

    private WheelCollider[] m_Wheels;
    public Agent myAgent;

    // Find all the WheelColliders down in the hierarchy.
	void Start()
	{
        needDirection = transform.position;
		m_Wheels = GetComponentsInChildren<WheelCollider>();

		for (int i = 0; i < m_Wheels.Length; ++i) 
		{
			var wheel = m_Wheels [i];

			// Create wheel shapes only when needed.
			if (wheelShape != null)
			{
				var ws = Instantiate (wheelShape);
				ws.transform.parent = wheel.transform;
			}
		}

        
	}

    private void OnDrawGizmos()
    {
        if (needDirection != null)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawSphere(transform.position + needDirection, 1);
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(transform.position + needTangent, 1);
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(transform.position + needOffset, 1);
        }
    }

    Vector3 needDirection = Vector3.zero;
    Vector3 needTangent = Vector3.zero;
    Vector3 needOffset = Vector3.zero;
    Vector3 needPosition = Vector3.zero;

    public void SetDirection(Vector3 v)
    {
        needDirection = v;
    }

    public void SetPosition(Vector3 p)
    {
        needPosition = p;
    }

    Vector3 lastPosition = Vector3.zero;
    public RoadContainer rc;

    public void NewCoords(Vector3 c)
    {
        needDirection = c;
    }

    public void SetVectors(Tuple<Vector3,Vector3> v, bool laneDirection)
    {
        float alpha = GetDirectionVectorPonderation(v.Item2.magnitude);

        needTangent = laneDirection ? -v.Item1:v.Item1;
        needOffset = v.Item2;
        needDirection = (1 - alpha) * needTangent.normalized + alpha * needOffset.normalized;
    }

    private float GetDirectionVectorPonderation(float dist)
    {
        //Debug.Log(dist);
        if (dist > 2) return 1;
        else if (dist < 1) return 0;
        else return dist - 1;
    }

    void Update()
	{
        if (myAgent.selfIndex == 0)
            myAgent.Update(this, true);
        else
            myAgent.Update(this, false);
        //Debug.Log("wd " + needDirection.ToString() + " " + needPosition.ToString());
        float velocity = (transform.position - lastPosition).magnitude / Time.deltaTime;
        lastPosition = transform.position;
        //Debug.Log("velocity " + velocity);
        float acc = 0;
        if (velocity > 5)
            acc = -1;
        else
            acc = Math.Min(3 - velocity,1);

        float angle2 = Vector3.Angle(needDirection, transform.forward);
        Vector3 cross = Vector3.Cross(needDirection, transform.forward);
        if (cross.y < 0) angle2 = -angle2;

        float test = Mathf.Clamp(-angle2 / 10, -1, 1);
        //Debug.Log(needTangent.normalized + " " + needOffset.normalized + " " + angle2 + " " + test);

        float angle = maxAngle * test;
		float torque = maxTorque * acc;

		float handBrake = Input.GetKey(KeyCode.X) ? brakeTorque : 0;

		foreach (WheelCollider wheel in m_Wheels)
		{
			// A simple car where front wheels steer while rear ones drive.
			if (wheel.transform.localPosition.z > 0)
				wheel.steerAngle = angle;

			if (wheel.transform.localPosition.z < 0)
			{
				wheel.brakeTorque = handBrake;
			}

			if (wheel.transform.localPosition.z < 0 && driveType != DriveType.FrontWheelDrive)
			{
				wheel.motorTorque = torque;
			}

			if (wheel.transform.localPosition.z >= 0 && driveType != DriveType.RearWheelDrive)
			{
				wheel.motorTorque = torque;
			}

			// Update visual wheels if any.
			if (wheelShape) 
			{
				Quaternion q;
				Vector3 p;
				wheel.GetWorldPose (out p, out q);

				// Assume that the only child of the wheelcollider is the wheel shape.
				Transform shapeTransform = wheel.transform.GetChild (0);
				shapeTransform.position = p;
				shapeTransform.rotation = q;
			}
		}
	}
}
