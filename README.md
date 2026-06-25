# nanoFramework.EspNow
.NET nanoFramework class library for ESP-NOW on ESP32 targets.

## Encrypted sender

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
        private static readonly byte[] TargetMac = new byte[] { 0x24, 0x6F, 0x28, 0x11, 0x22, 0x33 };
        private static readonly byte[] LocalMasterKey = new byte[]
        {
            0x10, 0x11, 0x12, 0x13,
            0x14, 0x15, 0x16, 0x17,
            0x18, 0x19, 0x1A, 0x1B,
            0x1C, 0x1D, 0x1E, 0x1F
        };

        private const byte Channel = 1;

        public static void Main()
        {
            using (var controller = new EspNowController())
            {
                controller.AddPeer(TargetMac, Channel, false, LocalMasterKey);

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

## Encrypted receiver

```csharp
using System.Diagnostics;
using System.Text;
using System.Threading;
using nanoFramework.EspNow;

namespace nanoFramework.EspNow.Receiver
{
    public class Program
    {
        private static readonly byte[] SenderMac = new byte[] { 0x24, 0x6F, 0x28, 0x44, 0x55, 0x66 };

        private const byte Channel = 1;

        public static void Main()
        {
            using (var controller = new EspNowController())
            {
                controller.AddPeer(SenderMac, Channel, false, LocalMasterKey);
                controller.DataReceived += (s, e) =>
                {
                    Debug.WriteLine(
                        "rx " +
                        BitConverter.ToString(e.PeerMac) +
                        " len=" + e.DataLen +
                        " data=" + Encoding.UTF8.GetString(e.Data, 0, e.DataLen));
                };

                Thread.Sleep(Timeout.Infinite);
            }
        }
    }
}
```
