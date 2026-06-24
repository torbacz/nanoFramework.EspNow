using System;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace nanoFramework.EspNow.Receiver
{
    public class Program
    {
        public static void Main()
        {
            Debug.WriteLine("ESP-NOW receiver starting...");

            using (var controller = new EspNowController())
            {
                controller.DataReceived += Controller_DataReceived;
                controller.DataSent += Controller_DataSent;

                controller.AddPeer(EspNowController.BROADCASTMAC, 0);

                Debug.WriteLine("Receiver ready");

                Thread.Sleep(Timeout.Infinite);
            }
        }

        private static void Controller_DataReceived(object sender, DataReceivedEventArgs e)
        {
            string text = Encoding.UTF8.GetString(e.Data, 0, e.DataLen);

            Debug.WriteLine(
                "RX from " +
                BitConverter.ToString(e.PeerMac) +
                " len=" + e.DataLen +
                " data=" + text);
        }

        private static void Controller_DataSent(object sender, DataSentEventArgs e)
        {
            Debug.WriteLine(
                "TX status to " +
                BitConverter.ToString(e.PeerMac) +
                ": " + e.Status);
        }
    }
}
