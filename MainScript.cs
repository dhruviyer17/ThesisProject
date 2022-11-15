using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using TMPro;
using YoutubePlayer;
using System;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Text;

public class MainScript : MonoBehaviour
{
    // Start is called before the first frame update
    private MeshRenderer meshRenderer;
    public Text textMessage;
    public VideoPlayer video;

    System.Threading.Thread SocketThread;
    volatile bool keepReading = false;
    string data;

    void Start()
    {
        video.PlayYoutubeVideoAsync("https://www.youtube.com/watch?v=1PuGuqpHQGo");
        meshRenderer = this.gameObject.GetComponentInChildren<MeshRenderer>();
    }

    // Update is called once per frame
    void Update()
    {   StartCoroutine(HandleIt());
        var headPosition = Camera.main.transform.position;
        var gazeDirection = Camera.main.transform.forward;
        
        var gazePosition = GameObject.Find("GlobalCursor").transform.position; //target object

        RaycastHit hitInfo;

        if (Physics.Raycast(headPosition, gazeDirection, out hitInfo))
        {
            meshRenderer.enabled = true;

            this.transform.position = hitInfo.point;
            
            this.transform.rotation = Quaternion.FromToRotation(Vector3.up, hitInfo.normal);

            //Display distance (Hololens - Cursor)
            float distance = Vector3.Distance(headPosition, gazePosition);
            textMessage.text = string.Format("Distance: {0}", distance >= 1.0f ? distance.ToString("####0.0") + "m" : distance.ToString("0.00") + "cm" );
        }
        else
        {
            meshRenderer.enabled = false;
            textMessage.text = string.Empty;
        }
    }

    private IEnumerator HandleIt()
    {
        // process pre-yield
        if (data == "run<EOF>")
        {
        }
        else
        {
            if (data == "stand<EOF>")
            {
            }
            else
            {
                if (data == "shake<EOF>")
                {
                }
                else
                {
                    if (data == "reverse<EOF>")
                    {
                    }
                    else
                    {
                    }
                }
            }
        }
        // process post-yield
    }
    
    public void startServer()
    {
        SocketThread = new System.Threading.Thread(networkCode);
        SocketThread.IsBackground = true;
        SocketThread.Start();
    }


    private string getIPAddress()
    {
        IPHostEntry host;
        string localIP = "";

        IPAddress ipAddr = null;

        foreach (var netInterface in NetworkInterface.GetAllNetworkInterfaces())
        {
            if (netInterface.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 ||
                netInterface.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
            {
                foreach (var addrInfo in netInterface.GetIPProperties().UnicastAddresses)
                {
                    if (addrInfo.Address.AddressFamily == AddressFamily.InterNetwork)
                    {
                        ipAddr = addrInfo.Address;

                        // use ipAddress as needed ...
                    }
                }
            }
        }
        localIP = ipAddr.ToString();
        return localIP;
        
    }


    Socket listener;
    Socket handler;

    public void networkCode()
    {
        byte[] bytes = new Byte[1024];

        Debug.Log("Ip " + getIPAddress().ToString());
        IPAddress ipAddr = null;
        foreach (var netInterface in NetworkInterface.GetAllNetworkInterfaces())
        {
            if (netInterface.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 ||
                netInterface.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
            {
                foreach (var addrInfo in netInterface.GetIPProperties().UnicastAddresses)
                {
                    if (addrInfo.Address.AddressFamily == AddressFamily.InterNetwork)
                    {
                        ipAddr = addrInfo.Address;

                        // use ipAddress as needed ...
                    }
                }
            }
        }
        IPEndPoint localEndPoint = new IPEndPoint(ipAddr, 7000);

        listener = new Socket(ipAddr.AddressFamily,
            SocketType.Stream, ProtocolType.Tcp);


        try
        {
            listener.Bind(localEndPoint);
            listener.Listen(10);


            while (true)
            {
                keepReading = true;

                Debug.Log("Waiting for Connection");

                handler = listener.Accept();
                Debug.Log("Client Connected");
 
                
                while (keepReading)
                {
                    bytes = new byte[1024];
                    int bytesRec = handler.Receive(bytes);
                    Debug.Log("Received from Server");
                    data = null;

                    if (bytesRec <= 0)
                    {
                        keepReading = false;
                        handler.Disconnect(true);
                        break;
                    }

                    data += Encoding.ASCII.GetString(bytes, 0, bytesRec);
                    if (data.IndexOf("<EOF>") > -1)
                    {
                        break;
                    }

                    System.Threading.Thread.Sleep(1);
                }
                Debug.Log("Text received ->" + data.Replace("<EOF>", ""));
                
                byte[] message = Encoding.ASCII.GetBytes("Test Server");

                handler.Send(message);
                /*
                handler.Shutdown(SocketShutdown.Both);
                handler.Close();
                */

                System.Threading.Thread.Sleep(1);
            }
        }
        catch (Exception e)
        {
            Debug.Log(e.ToString());
        }
    }

    public void stopServer()
    {
        keepReading = false;

        if (SocketThread != null)
        {
            SocketThread.Abort();
        }

        if (handler != null && handler.Connected)
        {
            handler.Disconnect(false);
            Debug.Log("Disconnected!");
        }
    }

    public void OnDisable()
    {
        stopServer();
    }
        
}

