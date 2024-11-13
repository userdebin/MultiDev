using System.Net;
using UnityEngine;

public class GetHostIP : MonoBehaviour
{
    public static string GetLocalIPAddress()
    {
        string localIP = "";
        var host = Dns.GetHostEntry(Dns.GetHostName());

        foreach (var ip in host.AddressList)
        {
            // Hanya ambil IPv4 yang sesuai dengan jaringan lokal
            if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            {
                localIP = ip.ToString();
                break;
            }
        }

        return localIP;
    }
}
