using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerField : MonoBehaviour
{
    public float TargetPositionSpeedFraction = 0.1f;

    public Vector3 TargetPosition;

    public RotationUpdate TargetRotation
    {
        set
        {
            GetComponentInChildren<NetworkGyroRotator>().RotationUpdate = value;
        }
    }

    void Update()
    {
        transform.position = Vector3.Lerp(
            transform.position, TargetPosition, TargetPositionSpeedFraction);
    }
}
