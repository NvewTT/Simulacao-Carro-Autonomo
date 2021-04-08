using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System.Net;
using System;
using CodeMonkey.Utils;
using System.Text;

//Classe responsavel pela comunicação com o servidor socket no python
public class Client 
{
    //Função responsavel por enviar a imagem para o python, recebe um byte array codificado em png como argumento
    public static void StartClient(byte[] bytearray)
    {
        
        byte[] bytes = new byte[100000000];
        IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());

        IPEndPoint remoteEP = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 4545);
        //Converte o byte array codificado em png para base 64
        string data = Convert.ToBase64String(bytearray);

        // Create a TCP/IP  socket.    
        Socket sender = new Socket(remoteEP.Address.AddressFamily,
                SocketType.Stream, ProtocolType.Tcp);
        try
        { // Connect to the remote endpoint.  
            sender.Connect(remoteEP);

            //Debug.Log("Socket connected to " +
            //    sender.RemoteEndPoint.ToString());

            // Encode the data string into a byte array.  
            byte[] msg = Encoding.ASCII.GetBytes(data);

            // Send the data through the socket.  
            int bytesSent = sender.Send(msg);

            // Receive the response from the remote device.  
            int bytesRec = sender.Receive(bytes);
            float ster = float.Parse(Encoding.ASCII.GetString(bytes, 0, bytesRec));
            //Manda o valor do error enviado pelo python para o Objeto do corro
            Carro.SetSteerS(ster);
            
        }
        catch (Exception e)
        {
            Debug.Log("Exception caught." + e.ToString());
            
        }


    }
}
