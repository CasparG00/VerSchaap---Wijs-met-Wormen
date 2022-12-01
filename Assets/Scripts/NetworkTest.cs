using System;
using System.Net;
using System.Net.Sockets;
using Unity.VisualScripting;
using UnityEngine;

public class NetworkTest : MonoBehaviour
{
    private void Start()
    {
        if (Application.isEditor)
        {
            transform.AddComponent<HttpListenerTest>();
        }
        else
        {
            transform.AddComponent<HttpClientTest>();
        }
    }
    
    public static string GetLocalIP()
    {
        string localIP = "";
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (IPAddress ip in host.AddressList)
        {
            if (ip.AddressFamily != AddressFamily.InterNetwork) continue;
            return ip.ToString();
        }
        return localIP;
    }
}
