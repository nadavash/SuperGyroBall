using LiteNetLib;
using LiteNetLib.Utils;
using System.Threading;
using UnityEngine;

public class GameManager : MonoBehaviour, IRotationPublisher
{
    public const int ServerPort = 8080;
    [field:SerializeField]
    public RotationUpdate Current { get; set; } = new RotationUpdate();

    private NetManager server;
    private Thread incomingThread;
    private Thread broadcastThread;
    private int numPackets = 0;
    private float lastReceiveTime = 0f;
    private float lastAveragePackets = 0f;

    // Start is called before the first frame update
    void Start()
    {
        Application.targetFrameRate = 60;
        var listener = new EventBasedNetListener();
        server = new NetManager(listener);
        server.Start(ServerPort);

        listener.ConnectionRequestEvent += OnConnectionRequest;
        listener.PeerConnectedEvent += OnPeerConnected;
        listener.NetworkReceiveEvent += OnNetworkReceive;
        // udpClient = new UdpClient(8080);
        // udpClient.EnableBroadcast = true;
        // incomingThread = new Thread(new ThreadStart(processIncomingPacketsLoop));
        // incomingThread.Start();
    }

    private void OnConnectionRequest(ConnectionRequest request)
    {
        Debug.Log("Connection requested.");
        request.Accept();
    }

    private void OnPeerConnected(NetPeer peer)
    {
        Debug.Log("Player connected.");
        var writer = new NetDataWriter();
        writer.Put("ready");
        peer.Send(writer, DeliveryMethod.ReliableOrdered);
    }

    private void OnNetworkReceive(NetPeer peer, NetDataReader reader, DeliveryMethod method)
    {
        Debug.Log("Message received.");
        ++numPackets;

        reader.SkipBytes(1);
        // TODO: Handle packet type here.
        // byte type = reader.GetByte();

        RotationUpdate newUpdate = new RotationUpdate()
        {
            Timestamp = reader.GetLong(),
            X = reader.GetFloat(),
            Z = reader.GetFloat(),
        };

        if (Current.Timestamp < newUpdate.Timestamp)
        {
            Current = newUpdate;
        }
    }

    void FixedUpdate()
    {
        server.PollEvents();
    }

    protected void OnGUI()
    {
        GUI.skin.label.fontSize = Screen.width / 40;
        GUILayout.Label("Num clients: " + server.ConnectedPeersCount);
        GUILayout.Label("Timestamp: " + Current?.Timestamp);
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
        server.Stop();
    }
}

[System.Serializable]
public class RotationUpdate
{
    public long Timestamp;
    public float X;
    public float Z;
}

public interface IRotationPublisher
{
    RotationUpdate Current { get; }
}
