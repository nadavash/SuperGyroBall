using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

public class GameManager : MonoBehaviour, IRotationPublisher
{
    public RotationUpdate Current { get; set; }

    private UdpClient udpClient;
    private Thread incomingThread;
    private Thread broadcastThread;
    private volatile bool isDone;
    private RotationUpdate nextUpdate = new RotationUpdate();

    private volatile int numPackets = 0;
    private volatile float lastReceiveTime = 0f;
    private volatile float lastAveragePackets = 0f;

    // Start is called before the first frame update
    void Start()
    {
        Application.targetFrameRate = 60;
        udpClient = new UdpClient(8080);
        udpClient.EnableBroadcast = true;
        incomingThread = new Thread(new ThreadStart(processIncomingPacketsLoop));
        incomingThread.Start();

    }

    void FixedUpdate()
    {
        lock (nextUpdate)
        {
            Current = nextUpdate;
        }
    }

    private void processIncomingPacketsLoop()
    {
        byte[] data;
        while (!isDone)
        {
            try
            {
                IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
                data = udpClient.Receive(ref sender);
                if (data[0] == 1)
                {
                    udpClient.Send(new byte[] { 0, 1 }, 2, sender);
                    continue;
                }
                ++numPackets;
                RotationUpdate newUpdate = new RotationUpdate()
                {
                    Timestamp = System.BitConverter.ToInt64(data, 1),
                    X = System.BitConverter.ToSingle(data, 9),
                    Z = System.BitConverter.ToSingle(data, 13),
                };

                lock (nextUpdate)
                {
                    if (nextUpdate.Timestamp < newUpdate.Timestamp)
                    {
                        nextUpdate = newUpdate;
                    }
                }
            }
            catch (ObjectDisposedException) { break; }
            catch (SocketException e)
            {
                Debug.Log(string.Format("SocketException thrown: {0}", e));
                break;
            }
        }
        udpClient.Close();
    }

    protected void OnGUI()
    {
        GUI.skin.label.fontSize = Screen.width / 40;
        GUILayout.Label("Timestamp: " + nextUpdate.Timestamp);
        GUILayout.Label(string.Format("Packets per second: {0:0.00}", lastAveragePackets));
        float timeSinceReset = Time.fixedTime - lastReceiveTime;
        if (timeSinceReset > 0.1f)
        {
            lastAveragePackets = numPackets / timeSinceReset;
            lastReceiveTime = Time.fixedTime;
            numPackets = 0;
        }
    }

    private void OnDestroy()
    {
        isDone = true;
        udpClient.Close();
    }
}

public class RotationUpdate
{
    public long Timestamp;
    public float X;
    public float Z;
}

public interface IRotationPublisher
{
    RotationUpdate Current { get; set; }
}
