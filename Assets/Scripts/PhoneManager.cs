using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using UnityEngine;

public class PhoneManager : MonoBehaviour
{
    public string ServerAddress = "192.168.86.47";
    public int ServerPort = 8080;
    public GyroRotator GyroRotator;

    private const int PACKET_SIZE = 17;
    private UdpClient udpClient;
    private byte[] sendBuffer = new byte[PACKET_SIZE];

    private volatile bool isConnectionReady = false;

    // Start is called before the first frame update
    void Start()
    {
        Task.Run(ConnectToServer);
    }

    private void ConnectToServer()
    {
        udpClient = new UdpClient();
        // Special case on iOS since broadcasting does not work by default.
        if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            udpClient.Connect(ServerAddress, ServerPort);
        }
        else
        {
            udpClient.EnableBroadcast = true;
            udpClient.Send(new byte[1] { 1 }, 1, new IPEndPoint(IPAddress.Broadcast, 8080));

            var serverEndpoint = new IPEndPoint(IPAddress.Any, 0);
            byte[] bytes = udpClient.Receive(ref serverEndpoint);
            Debug.Log("Broadcast response: " + bytes.Length);
            udpClient.EnableBroadcast = false;
            udpClient.Connect(serverEndpoint);
        }
        isConnectionReady = true;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!isConnectionReady)
        {
            return;
        }
        
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
}
