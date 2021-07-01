using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameDataPanel : MonoBehaviour
{
    public Text RotationValues;
    public Text IPValue;

    public IRotationPublisher RotationPublisher;

    void Start()
    {
        RotationPublisher = GameObject.FindObjectOfType<GameManager>();
        IPValue.text = IPManager.GetIP(AddressFamily.IPv4);
    }

    void Update()
    {
        RotationValues.text = string.Format(
            "X = {0:0.00}, Z = {1:0.00}", 
            RotationPublisher.Current.X, 
            RotationPublisher.Current.Z);
    }
}
