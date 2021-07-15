using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkGyroRotator : MonoBehaviour
{
    public float LerpFractionalSpeed = 0.1f;
    public RotationUpdate RotationUpdate { 
        get { return rotationUpdate; }
        set {
            if (value.Timestamp > rotationUpdate.Timestamp)
            {
                rotationUpdate = value;
            }
        }
    }
    
    private RotationUpdate rotationUpdate;

    private void Start()
    {
        rotationUpdate = new RotationUpdate();
    }

    // Update is called once per frame
    void Update()
    {
        transform.rotation = Quaternion.Lerp(
            transform.rotation,
            Quaternion.Euler(RotationUpdate.X, 0, RotationUpdate.Z),
            LerpFractionalSpeed);
            
    }
}
