using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using TMPro;

public class NetworkClient : MonoBehaviour
{
    public Transform eye;
    public Transform cursor;
    private string hitPointMessage = "";
    private string prevMessage = "";
    private string prevWord = "";
    public GameObject[] hightlighterGobj;
    public TextMeshProUGUI[] toggleButtonGobj;
    private string curGazedMonitor = "";
    public Material screenMat;
    public static bool killMonitor = false;
    public string IP_ADDRESS;
    public int PORT = 5005;
    void Awake()
    {
        updateMonitor();
    }
    void Update()
    {
        if (hightlighterGobj[0].activeSelf) //monitor1
        {
            if (toggleButtonGobj[0].text == "Mouse")
            {
                trackHandPosition();
                if (prevMessage != hitPointMessage)
                {
                    prevMessage = hitPointMessage;
                    Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                    //IPEndPoint ipAddress = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 5005);
                    //IPEndPoint ipAddress = new IPEndPoint(IPAddress.Parse("192.168.43.225"), 5005);
                    IPEndPoint ipAddress = new IPEndPoint(IPAddress.Parse(IP_ADDRESS), PORT);
                    StartCoroutine(sendData(server, ipAddress, hitPointMessage));
                }
            }
            else // Keyboard
            {

            }
            curGazedMonitor = "Monitor1";
        }
        //else if (hightlighterGobj[1].activeSelf)//monitor2
        //{
        //    if (toggleButtonGobj[1].text == "Mouse")
        //    {
        //        trackHandPosition();
        //        Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        //        //IPEndPoint ipAddress = new IPEndPoint(IPAddress.Parse("192.168.43.225"), 5006);
        //        IPEndPoint ipAddress = new IPEndPoint(IPAddress.Parse("172.24.224.219"), 5006);
        //        StartCoroutine(sendData(server, ipAddress, hitPointMessage));
        //    }
        //    else // Keyboard
        //    {

        //    }
        //    curGazedMonitor = "Monitor2";
        //}
        else
        {
            curGazedMonitor = "";
            killMonitor = true;
        }


    }
    private void updateMonitor()
    {
        StartCoroutine(staticMonitor());
        //killMonitor = false;
        //StartCoroutine(realitimeMonitor());
    }
    IEnumerator realitimeMonitor()
    {
        while (!killMonitor)
        {
            IPAddress IP = IPAddress.Parse(IP_ADDRESS);
            IPEndPoint localEndPoint = new IPEndPoint(IP, 8485);

            Socket sender = new Socket(IP.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            sender.Connect(localEndPoint);
            byte[] messageReceived = new byte[4096];
            int byteRecv = sender.Receive(messageReceived);

            int total = 0;
            int remaining = 0;
            int size = 4096;
            int length = Int32.Parse(Encoding.ASCII.GetString(messageReceived, 0, byteRecv));

            byte[] byteArray = new byte[length];
            byte[] byteLastArray;

            while (total < length)
            {
                remaining = length - total;

                byteRecv = sender.Receive(messageReceived);
                if (remaining < size)
                {
                    size = remaining;
                    byteLastArray = new byte[size];
                    Array.Copy(messageReceived, 0, byteLastArray, 0, size);
                    byteLastArray.CopyTo(byteArray, total);
                }
                else
                    messageReceived.CopyTo(byteArray, total);

                total += byteRecv;
            }

            Texture2D sampleTexture = new Texture2D(1, 1);
            sampleTexture.LoadImage(byteArray);
            screenMat.mainTexture = sampleTexture;

            yield return new WaitForSeconds(0.08f);
        }
    }
    IEnumerator staticMonitor()
    {
        yield return new WaitForSeconds(0.5f);
        try
        {
            IPAddress IP = IPAddress.Parse(IP_ADDRESS);
            IPEndPoint localEndPoint = new IPEndPoint(IP, 8485);

            Socket sender = new Socket(IP.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            sender.Connect(localEndPoint);
            byte[] messageReceived = new byte[4096];
            int byteRecv = sender.Receive(messageReceived);

            int total = 0;
            int remaining = 0;
            int size = 4096;
            int length = Int32.Parse(Encoding.ASCII.GetString(messageReceived, 0, byteRecv));

            byte[] byteArray = new byte[length];
            byte[] byteLastArray;

            while (total < length)
            {
                remaining = length - total;

                byteRecv = sender.Receive(messageReceived);
                if (remaining < size)
                {
                    size = remaining;
                    byteLastArray = new byte[size];
                    Array.Copy(messageReceived, 0, byteLastArray, 0, size);
                    byteLastArray.CopyTo(byteArray, total);
                }
                else
                    messageReceived.CopyTo(byteArray, total);

                total += byteRecv;
            }
            Texture2D sampleTexture = new Texture2D(1, 1);
            sampleTexture.LoadImage(byteArray);
            screenMat.mainTexture = sampleTexture;
        }
        catch (Exception error)
        {
            print("Socket error occurred::" + error.ToString());
        }
        yield return null;
    }
    public void sendData(string message, string command = "")
    {
        //print("MSG : " + message + " | CMD : " + command);
        if (curGazedMonitor == "")
            return;

        Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        IPEndPoint ipAddress;
        if (curGazedMonitor == "Monitor1")
            ipAddress = new IPEndPoint(IPAddress.Parse(IP_ADDRESS), PORT);
        else
            ipAddress = new IPEndPoint(IPAddress.Parse(IP_ADDRESS), PORT + 1);
        //if (curGazedMonitor == "Monitor1")
        //    ipAddress = new IPEndPoint(IPAddress.Parse("192.168.43.225"), 5005);
        //else
        //    ipAddress = new IPEndPoint(IPAddress.Parse("192.168.43.225"), 5006);
        //if (curGazedMonitor == "Monitor1")
        //    ipAddress = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 5005);
        //else
        //    ipAddress = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 5006);

        if (command == "delete")
            message = "X_" + prevWord;

        StartCoroutine(sendData(server, ipAddress, message));
    }
    IEnumerator sendData(Socket server, IPEndPoint ipAddress, string message)
    {
        //print("send data : " + message);
        if (message != "")
        {
            byte[] data = new byte[16];
            data = Encoding.ASCII.GetBytes(message);
            server.SendTo(data, data.Length, SocketFlags.None, ipAddress);
            server.Close();
            //print("Message : " + message + " sent");
        }
        //else
        //    print("no");

        prevWord = message.Substring(2);
        updateMonitor();
        yield return null;
    }
    private void trackHandPosition()
    {
        Vector3 direction = cursor.position - eye.position;
        //Debug.DrawRay(eye.transform.position, direction * 100.0f, Color.yellow);

        int layermask = 1 << 9; //only monitor
        bool wasHit = false;
        RaycastHit[] hits = Physics.RaycastAll(eye.transform.position, direction, Mathf.Infinity, layermask);
        if (hits.Length > 0)
        {
            foreach (RaycastHit hit in hits)
            {
                if (hit.transform.gameObject.name.StartsWith("Monitor"))
                {
                    wasHit = true;
                    Vector3 localPos = hit.transform.InverseTransformPoint(hit.point);
                    hitPointMessage = "M_" + (int)((localPos.x + 5) * 192.0f) + "," + (int)(1080 - (localPos.z + 5) * 108.0f);
                }
            }
            if (!wasHit)
                hitPointMessage = "";
            else
                wasHit = false;
        }



    }
}