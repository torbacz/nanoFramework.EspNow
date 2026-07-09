//
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
//

using System;

namespace nanoFramework.EspNow
{
    /// <summary>
    /// Container for DataSent event data.
    /// </summary>
    public class DataSentEventArgs : EventArgs
    {
        private readonly byte[] _peerMac;
        private readonly int _status;

        internal DataSentEventArgs(byte[] peerMac, int status)
        {
            _peerMac = peerMac;
            _status = status;
        }

        /// <summary>
        /// MAC address of peer data was sent to.
        /// </summary>
        public byte[] PeerMac { get => _peerMac; }

        /// <summary>
        /// Status of sending.
        /// See esp_now_send_status_t in esp_now.h
        /// </summary>
        public int Status { get => _status; }
    }
}
