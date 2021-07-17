using LiteNetLib;
using LiteNetLib.Utils;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public const int ServerPort = 8080;

    public PlayerField PlayerFieldPrefab;
    public Transform CameraTransform;

    private Vector3 targetCameraPosition;
    private NetManager server;
    private Thread incomingThread;
    private Thread broadcastThread;
    private int numPackets = 0;
    private float lastReceiveTime = 0f;
    private float lastAveragePackets = 0f;
    private Dictionary<int, PlayerField> playerFields;
    private PlayerFieldOrganizer fieldOrganizer;

    // Start is called before the first frame update
    void Start()
    {
        targetCameraPosition = CameraTransform.position;
        playerFields = new Dictionary<int, PlayerField>();
        fieldOrganizer = new PlayerFieldOrganizer();

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
        PlayerField field = GameObject.Instantiate(PlayerFieldPrefab);
        playerFields[peer.Id] = field;
        fieldOrganizer.Add(field);
        targetCameraPosition.x = fieldOrganizer.MiddleX;
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

        playerFields[peer.Id].TargetRotation = newUpdate;
    }

    private void OnPeerDisconnected(NetPeer peer, DisconnectInfo info)
    {
        Debug.Log(
            string.Format("Player disconnected. Id = {0}, Reason = {1}", peer.Id, info.Reason));
        PlayerField field = playerFields[peer.Id];
        fieldOrganizer.Remove(field);
        playerFields.Remove(peer.Id);
        GameObject.Destroy(field.gameObject);
        targetCameraPosition.x = fieldOrganizer.MiddleX;
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
