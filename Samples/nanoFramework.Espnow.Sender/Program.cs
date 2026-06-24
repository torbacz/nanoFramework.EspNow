using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using nanoFramework.EspNow;

namespace nanoFramework.EspNow.Sender
{
    public class Program
    {
        public static void Main()
        {
            Debug.WriteLine("ESP-NOW sender starting...");

            using (var controller = new EspNowController())
            {
                controller.DataSent += Controller_DataSent;

                controller.AddPeer(EspNowController.BROADCASTMAC, 0);

                int counter = 0;

                while (true)
                {
                    string message = "ping " + counter++;
                    byte[] payload = Encoding.UTF8.GetBytes(message);

                    controller.Send(EspNowController.BROADCASTMAC, payload, payload.Length);
                    Debug.WriteLine("Sent: " + message);

                    Thread.Sleep(1000);
                }
            }
        }

        private static void Controller_DataSent(object sender, DataSentEventArgs e)
        {
            Debug.WriteLine("DataSent status: " + e.Status);
        }
    }
}
