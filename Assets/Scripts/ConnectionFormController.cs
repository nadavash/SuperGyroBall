using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ConnectionFormController : MonoBehaviour
{
    public InputField IPField;
    public Button ConnectButton;

    void Start()
    {
        ConnectButton.onClick.AddListener(ConnectToGame);
    }

    private void ConnectToGame()
    {
        Globals.GameHost = IPField.text;
        UnityEngine.SceneManagement.SceneManager.LoadScene("PhoneScene");
    }
}
