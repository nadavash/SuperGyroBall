using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameDataPanel : MonoBehaviour
{
    public Text RotationValues;
    public Text IPValue;

    void Start()
    {
        // RotationPublisher = GameObject.FindObjectOfType<GameManager>();
        IPValue.text = IPManager.GetIP(AddressFamily.IPv4);
    }
}
