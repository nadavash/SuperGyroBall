using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

public class PhoneManager : MonoBehaviour
{
    public GyroRotator GyroRotator;

    private const int PACKET_SIZE = 17;
    private UdpClient udpClient;
    private byte[] sendBuffer = new byte[PACKET_SIZE];
    private Thread receiveBroadcastThread;

    private volatile int numBroadcastBytes = 0;

    // Start is called before the first frame update
    void Start()
    {
        udpClient = new UdpClient();
        udpClient.EnableBroadcast = true;
        udpClient.Send(new byte[1] { 1 }, 1, new IPEndPoint(IPAddress.Broadcast, 8080));

        var serverEndpoint = new IPEndPoint(IPAddress.Any, 0);
        byte[] bytes = udpClient.Receive(ref serverEndpoint);
        Debug.Log("Broadcast response: " + bytes.Length);
        udpClient.EnableBroadcast = false;
        udpClient.Connect(serverEndpoint);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        using (var memStream = new MemoryStream(sendBuffer))
        {
            Vector3 rotation = GyroRotator.transform.rotation.eulerAngles;
            long currentTimestampMs = System.DateTime.Now.ToFileTimeUtc();
            memStream.Write(BitConverter.GetBytes((byte)0), 0, 1);
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

    protected void OnGUI()
    {
        GUI.skin.label.fontSize = Screen.width / 40;
        GUILayout.Label("Num bytes: " + numBroadcastBytes);
    }

    // private void receiveBroadcastLoop()
    // {
    //     while (true)
    //     {
    //         ++numBroadcastBytes;
    //         try
    //         {
    //             var hostEndpoint = new IPEndPoint(IPAddress.Any, 0);
    //             byte[] data = broadcastClient.Receive(ref hostEndpoint);
    //             numBroadcastBytes += data.Length;
    //             Debug.Log(string.Format("Data received: {0}", data));
    //         }
    //         catch (Exception)
    //         {
    //             break;
    //         }
    //     }
    // }
}
