using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DataPanel : MonoBehaviour
{
    public Text GyroscopeText;
    public Text InitialText;

    public GyroRotator GyroRotator;

    void Start()
    {
        Input.gyro.enabled = true;
    }

    void Update()
    {
        GyroscopeText.text = Helpers.GyroToUnity(Input.gyro.attitude).ToString();
        InitialText.text = GyroRotator.InitialAttitude.ToString();
    }
}
