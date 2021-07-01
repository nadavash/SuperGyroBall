using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkGyroRotator : MonoBehaviour
{
    public IRotationPublisher RotationPublisher;

    // Start is called before the first frame update
    void Start()
    {
        RotationPublisher = GameObject.FindObjectOfType<GameManager>();
    }

    // Update is called once per frame
    void Update()
    {
        transform.eulerAngles = 
            new Vector3(
                RotationPublisher.Current.X, 0, 
                RotationPublisher.Current.Z);
    }
}
