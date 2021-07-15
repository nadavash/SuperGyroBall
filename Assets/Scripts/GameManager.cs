using LiteNetLib;
using LiteNetLib.Utils;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public const int ServerPort = 8080;

    public GameObject PlayerFieldPrefab;
    public float PlayerFieldSpacing = 10f;
    public Transform CameraTransform;

    private Vector3 targetCameraPosition;

    private NetManager server;
    private Thread incomingThread;
    private Thread broadcastThread;
    private int numPackets = 0;
    private float lastReceiveTime = 0f;
    private float lastAveragePackets = 0f;
    private Dictionary<int, GameObject> playerFields;

    // Start is called before the first frame update
    void Start()
    {
        targetCameraPosition = CameraTransform.position;
        playerFields = new Dictionary<int, GameObject>();
        Application.targetFrameRate = 60;
        var listener = new EventBasedNetListener();
        server = new NetManager(listener);
        server.Start(ServerPort);

        listener.ConnectionRequestEvent += OnConnectionRequest;
        listener.PeerConnectedEvent += OnPeerConnected;
        listener.NetworkReceiveEvent += OnNetworkReceive;
        listener.PeerDisconnectedEvent += OnPeerDisconnected;
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
        float xPos = (server.ConnectedPeersCount - 1) * PlayerFieldSpacing;
        playerFields[peer.Id] = GameObject.Instantiate(
            PlayerFieldPrefab,
            Vector3.right * xPos,
            Quaternion.identity
        );
        targetCameraPosition = CameraTransform.position;
        targetCameraPosition.x = xPos / 2f;
    }

    private void OnNetworkReceive(NetPeer peer, NetDataReader reader, DeliveryMethod method)
    {
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

        playerFields[peer.Id].GetComponentInChildren<NetworkGyroRotator>().RotationUpdate = newUpdate;
    }

    private void OnPeerDisconnected(NetPeer peer, DisconnectInfo info)
    {
        GameObject.Destroy(playerFields[peer.Id]);
        playerFields.Remove(peer.Id);

        float xPos = (server.ConnectedPeersCount - 1) * PlayerFieldSpacing;
        targetCameraPosition = CameraTransform.position;
        targetCameraPosition.x = xPos / 2f;
    }

    void FixedUpdate()
    {
        server.PollEvents();

        CameraTransform.position = Vector3.Lerp(
            CameraTransform.position,
            targetCameraPosition,
            0.1f
        );
    }

    protected void OnGUI()
    {
        GUI.skin.label.fontSize = Screen.width / 40;
        GUILayout.Label("Num clients: " + server.ConnectedPeersCount);
        // GUILayout.Label("Timestamp: " + Current?.Timestamp);
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
