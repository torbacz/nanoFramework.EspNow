//
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
//

using nanoFramework.Runtime.Events;

namespace nanoFramework.EspNow
{
    internal class DataRecvEventInternal : BaseEvent
    {
        // these fields are set on native side
#pragma warning disable 0649
        public byte[] PeerMac;
        public byte[] Data;
        public int DataLen;
#pragma warning restore 0649
    }
}
