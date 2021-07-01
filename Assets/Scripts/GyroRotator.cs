using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GyroRotator : MonoBehaviour
{
    public Quaternion InitialAttitude = Quaternion.identity;

    // Start is called before the first frame update
    void Start()
    {
        Input.gyro.enabled = true;

        Invoke("SetInitialAttitude", .1f);
    }

    // Update is called once per frame
    void Update()
    {
        if (SystemInfo.supportsGyroscope)
        {
            Quaternion gyro = Helpers.GyroToUnity(Input.gyro.attitude);
            Quaternion diff = gyro * Quaternion.Inverse(InitialAttitude);
            diff = Quaternion.Euler(0, 0, -diff.eulerAngles.z) * diff;
            transform.eulerAngles = new Vector3(diff.eulerAngles.x, 0, diff.eulerAngles.y);
        }
    }

    public void SetInitialAttitude() {
        InitialAttitude = Helpers.GyroToUnity(Input.gyro.attitude);
    }
}
