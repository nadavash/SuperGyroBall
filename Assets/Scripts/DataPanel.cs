using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DataPanel : MonoBehaviour
{
    public Text GyroscopeText;

    // Update is called once per frame
    void Update()
    {
        GyroscopeText.text = GyroToUnity(Input.gyro.attitude).ToString();
    }

    private static Quaternion GyroToUnity(Quaternion q)
    {
        return new Quaternion(q.x, q.y, -q.z, -q.w);
    }
}
