using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkGyroRotator : MonoBehaviour
{
    public float LerpFractionalSpeed = 0.1f;
    public IRotationPublisher RotationPublisher;

    // Start is called before the first frame update
    void Start()
    {
        RotationPublisher = GameObject.FindObjectOfType<GameManager>();
    }

    // Update is called once per frame
    void Update()
    {
        transform.rotation = Quaternion.Lerp(
            transform.rotation,
            Quaternion.Euler(RotationPublisher.Current.X, 0, RotationPublisher.Current.Z),
            LerpFractionalSpeed);
            
    }
}
