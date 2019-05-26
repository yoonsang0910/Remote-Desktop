using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System;
using System.Text;
using System.Threading;

public class screenTransferTest : MonoBehaviour
{
    public Material screenMat;
    void Start()
    {
        StartCoroutine(Run());
    }
    IEnumerator Run()
    {
        while (true)
        {
            IPAddress IP = IPAddress.Parse("172.24.224.197");
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
            //using (FileStream fStream = new FileStream("Assets/Resources/screen.png", FileMode.Create))
            //{
            //    while (total < length)
            //    {
            //        remaining = length - total;

            //        byteRecv = sender.Receive(messageReceived);
            //        fStream.Write(messageReceived, 0, byteRecv);
            //        fStream.Flush();
            //        if (remaining < size)
            //        {
            //            size = remaining;
            //            byteLastArray = new byte[size];
            //            Array.Copy(messageReceived, 0, byteLastArray, 0, size);
            //            byteLastArray.CopyTo(byteArray, total);
            //        }
            //        else
            //            messageReceived.CopyTo(byteArray, total);

            //        total += byteRecv;
            //    }
            //}
            //Texture2D sampleTexture = new Texture2D(2, 2);
            ////byte[] fileData = File.ReadAllBytes("Assets/Resources/screen.png");
            ////sampleTexture.LoadImage(fileData);
            //sampleTexture.LoadImage(byteArray);
            //screenMat.mainTexture = sampleTexture;

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
}
