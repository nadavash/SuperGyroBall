using LiteNetLib;
using LiteNetLib.Utils;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using UnityEngine;

public class PhoneManager : MonoBehaviour
{
    public string ServerAddress = "169.254.53.124";//"192.168.86.47";
    public int ServerPort = 8080;
    public GyroRotator GyroRotator;

    private NetManager netClient;
    private const int PACKET_SIZE = 17;
    
    private UdpClient udpClient;
    private byte[] sendBuffer = new byte[PACKET_SIZE];

    private volatile bool isConnected = false;

    // Start is called before the first frame update
    void Start()
    {
        Application.targetFrameRate = 60;
        Task.Run(ConnectToServer);
    }

    private void ConnectToServer()
    {
        EventBasedNetListener listener = new EventBasedNetListener();
        netClient = new NetManager(listener);
        netClient.Start();

        udpClient = new UdpClient();
        // Special case on iOS since broadcasting does not work by default.
        if (Application.isEditor)
        {
            netClient.Connect(ServerAddress, ServerPort, "SomeKey");
        }
        else if (Globals.UseConnectionScreenData)
        {
            netClient.Connect(Globals.GameHostAddress, Globals.GamePortAddress, "SomeKey");
        }
        else
        {
            // netClient.SendBroadcast()
            // udpClient.Send(new byte[1] { 1 }, 1, new IPEndPoint(IPAddress.Broadcast, 8080));

            // var serverEndpoint = new IPEndPoint(IPAddress.Any, 0);
            // byte[] bytes = udpClient.Receive(ref serverEndpoint);
            // Debug.Log("Broadcast response: " + bytes.Length);
            // udpClient.EnableBroadcast = false;
            // udpClient.Connect(serverEndpoint);
        }
        isConnected = true;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!isConnected)
        {
            return;
        }

        netClient.PollEvents();
        
        NetDataWriter dataWriter = new NetDataWriter(false, PACKET_SIZE);
        Vector3 rotation = GyroRotator.transform.rotation.eulerAngles;
        long currentTimestampMs = System.DateTime.Now.ToFileTimeUtc();
        dataWriter.Put((byte)0B0);
        dataWriter.Put(currentTimestampMs);
        dataWriter.Put(rotation.x);
        dataWriter.Put(rotation.z);
        netClient.FirstPeer?.Send(dataWriter, DeliveryMethod.Unreliable);
    }

    void OnDestroy()
    {
        netClient.DisconnectAll();
        netClient.Stop();  
        isConnected = false;         
    }
}
