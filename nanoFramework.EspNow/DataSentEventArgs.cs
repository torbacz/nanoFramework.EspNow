//
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
//

using System;

namespace nanoFramework.EspNow
{
    /// <summary>
    /// ESP-NOW send status.
    /// </summary>
    public enum EspNowSendStatus
    {
        /// <summary>
        /// Send completed successfully.
        /// </summary>
        Success = 0,

        /// <summary>
        /// Send failed.
        /// </summary>
        Fail = 1
    }

    /// <summary>
    /// Container for DataSent event data.
    /// </summary>
    public class DataSentEventArgs : EventArgs
    {
        /// <summary>
        /// MAC address of peer data was sent to.
        /// </summary>
        public byte[] PeerMac;

        /// <summary>
        /// Status of sending.
        /// See esp_now_send_status_t in esp_now.h
        /// </summary>
        public EspNowSendStatus Status;

        /// <summary>
        /// Constructor.
        /// </summary>
        public DataSentEventArgs(byte[] peerMac, EspNowSendStatus status)
        {
            this.PeerMac = peerMac;
            this.Status = status;
        }
    }
}
