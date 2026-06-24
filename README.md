# nanoFramework.EspNow
.NET nanoFramework class library for ESP-NOW on ESP32 targets.

## Sender

```csharp
using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using nanoFramework.EspNow;

namespace nanoFramework.EspNow.Sender
{
    public class Program
    {
        private static readonly byte[] TargetMac = EspNowController.BROADCASTMAC;
        private const byte Channel = 1;

        public static void Main()
        {
            using (var controller = new EspNowController())
            {
                controller.AddPeer(TargetMac, Channel);

                int counter = 0;
                while (true)
                {
                    var payload = Encoding.UTF8.GetBytes("ping " + counter++);
                    controller.Send(TargetMac, payload, payload.Length);
                    Debug.WriteLine("sent");
                    Thread.Sleep(1000);
                }
            }
        }
    }
}
```

## Receiver

```csharp
using System.Diagnostics;
using System.Threading;
using nanoFramework.EspNow;

namespace nanoFramework.EspNow.Receiver
{
    public class Program
    {
        public static void Main()
        {
            using (var controller = new EspNowController())
            {
                controller.DataReceived += (s, e) =>
                {
                    Debug.WriteLine(
                        "rx " +
                        BitConverter.ToString(e.PeerMac) +
                        " len=" + e.DataLen);
                };

                Thread.Sleep(Timeout.Infinite);
            }
        }
    }
}
```
