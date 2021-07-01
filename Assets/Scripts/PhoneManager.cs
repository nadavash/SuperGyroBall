using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class PhoneManager : MonoBehaviour
{
    private const int PACKET_SIZE = 16;
    private UdpClient udpClient;
    private byte[] sendBuffer = new byte[PACKET_SIZE];
    public GyroRotator GyroRotator;

    // Start is called before the first frame update
    void Start()
    {
        udpClient = new UdpClient();
        udpClient.Connect(IPAddress.Parse("192.168.86.49"), 8080);    
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        using (var memStream = new MemoryStream(sendBuffer))
        {
            Vector3 rotation = GyroRotator.transform.rotation.eulerAngles;
            long currentTimestampMs = System.DateTime.Now.ToFileTimeUtc();
            memStream.Write(BitConverter.GetBytes(currentTimestampMs), 0, 8);
            memStream.Write(BitConverter.GetBytes(rotation.x), 0, 4);
            memStream.Write(BitConverter.GetBytes(rotation.z), 0, 4);
        }
        udpClient.Send(sendBuffer, PACKET_SIZE);
    }

    void OnDestroy()
    {
        udpClient.Close();
    }
}
