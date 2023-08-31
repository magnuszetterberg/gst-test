using UnityEngine;
using System.Collections;
using System.Net;
using System.Net.Sockets;

public class CameraStreamer : MonoBehaviour
{
    public Camera streamingCamera;
    public string targetIP = "127.0.0.1"; // Change to your target IP
    public int targetPort = 5000;        // Change to your target port

    public float streamingInterval = 0.1f; // Time interval between sending frames

    private Texture2D texture;
    private UdpClient udpClient;
    private float timeSinceLastStream = 0f;

    void Start()
    {
        texture = new Texture2D(streamingCamera.targetTexture.width, streamingCamera.targetTexture.height);
        udpClient = new UdpClient();
    }

void Update()
{
    timeSinceLastStream += Time.deltaTime;

    if (timeSinceLastStream >= streamingInterval)
    {
        RenderTexture.active = streamingCamera.targetTexture;
        texture.ReadPixels(new Rect(0, 0, streamingCamera.targetTexture.width, streamingCamera.targetTexture.height), 0, 0);
        texture.Apply();

        byte[] imageData = texture.EncodeToPNG();
        
        int chunkSize = 1024; // Specify the chunk size in bytes
        int totalChunks = Mathf.CeilToInt((float)imageData.Length / chunkSize);

        for (int chunkIndex = 0; chunkIndex < totalChunks; chunkIndex++)
        {
            int offset = chunkIndex * chunkSize;
            int remainingBytes = imageData.Length - offset;
            int chunkBytes = Mathf.Min(chunkSize, remainingBytes);

            byte[] chunkData = new byte[chunkBytes];
            System.Array.Copy(imageData, offset, chunkData, 0, chunkBytes);

            udpClient.Send(chunkData, chunkData.Length, targetIP, targetPort);
        }

        timeSinceLastStream = 0f;
    }
}



    void OnApplicationQuit()
    {
        if (udpClient != null)
        {
            udpClient.Close();
        }
    }
}
