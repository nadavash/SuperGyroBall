using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

public class GameManager : MonoBehaviour, IRotationPublisher
{
    private UdpClient udpClient;
    private Thread incomingThread;

    private volatile bool isDone;

    private RotationUpdate nextUpdate = new RotationUpdate();

    public RotationUpdate Current { get; set; }

    // Start is called before the first frame update
    void Start()
    {
        udpClient = new UdpClient(8080);
        incomingThread = new Thread(new ThreadStart(processIncomingPackets));
        incomingThread.Start();
    }

    void FixedUpdate()
    {
        lock(nextUpdate)
        {
            Current = nextUpdate;
        }
    }

    private void processIncomingPackets()
    {
        byte[] data;
        while (!isDone)
        {
            IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
            data = udpClient.Receive(ref sender);
            RotationUpdate newUpdate = new RotationUpdate() 
            {
                Timestamp = System.BitConverter.ToInt64(data, 0),
                X = System.BitConverter.ToSingle(data, 8),
                Z = System.BitConverter.ToSingle(data, 12),
            };

            lock(nextUpdate)
            {
                if (nextUpdate.Timestamp < newUpdate.Timestamp)
                {
                    nextUpdate = newUpdate;
                }
            }
        }
        udpClient.Close();
    }

    protected void OnGUI()
    {
        GUI.skin.label.fontSize = Screen.width / 40;

        GUILayout.Label("Timestamp: " + nextUpdate.Timestamp);
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
