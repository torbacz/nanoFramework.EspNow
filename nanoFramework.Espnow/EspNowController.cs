//
// Copyright (c) 2020 The nanoFramework project contributors
// See LICENSE file in the project root for full license information.
//

using nanoFramework.Runtime.Events;
using System;
using System.Runtime.CompilerServices;

namespace nanoFramework.EspNow
{
    /// <summary>
    /// ESP-NOW controller class.
    /// </summary>
    public class EspNowController : IDisposable
    {
        // keep in sync with nf-interpreter:src/HAL/Include/nanoHAL_v2.h
        private const int EVENT_ESPNOW = 150;
        private const int MacAddressLength = 6;
        private const byte BroadcastMacByte = 0xff;

        /// <summary>
        /// DataSent event handler type definition.
        /// </summary>
        public delegate void DataSendEventHandler(object sender, DataSentEventArgs eventArgs);

        /// <summary>
        /// Event raised after sending completed.
        /// </summary>
        public event DataSendEventHandler DataSent;

        /// <summary>
        /// DataReceived event handler type definition.
        /// </summary>
        public delegate void DataReceivedEventHandler(object sender, DataReceivedEventArgs eventArgs);

        /// <summary>
        /// Event raised when data received.
        /// </summary>
        public event DataReceivedEventHandler DataReceived;

        private bool isDisposed;
        private EspNowEventHandler eventHandler;

        /// <summary>
        /// Controller.
        /// </summary>
        public EspNowController()
        {
            // Add a native event processor.
            eventHandler = new EspNowEventHandler(this);
            EventSink.AddEventProcessor((EventCategory)EVENT_ESPNOW, eventHandler);
            EventSink.AddEventListener((EventCategory)EVENT_ESPNOW, eventHandler);

            var nret = NativeInitialize();
            if (nret != 0)
            {
                throw new EspNowException(nret);
            }
        }

        /// <summary>
        /// Add peer to which data will be sent.
        /// </summary>
        /// <param name="peerMac">MAC address of peer.</param>
        /// <param name="channel">WiFi channel to be used.</param>
        /// <param name="encrypted"><see langword="true"/> to enable ESP-NOW encryption for this peer.</param>
        /// <param name="localMasterKey">16-byte local master key used when encryption is enabled.</param>
        /// <exception cref="ArgumentException">
        /// <para><paramref name="peerMac"/> is not 6 bytes long.</para>
        /// <para>-or-</para>
        /// <para><paramref name="localMasterKey"/> is not 16 bytes long.</para>
        /// <para>-or-</para>
        /// <para>Trying to enable encryption for a broadcast peer [FF-FF-FF-FF-FF-FF].</para>
        /// </exception>
        /// <exception cref="EspNowException">Native ESP-NOW peer registration failed.</exception>
        public void AddPeer(
            byte[] peerMac,
            byte channel,
            bool encrypted,
            byte[] localMasterKey)
        {
            if (peerMac != null && peerMac.Length != MacAddressLength)
            {
                throw new ArgumentException();
            }

            if (localMasterKey != null && localMasterKey.Length != 16)
            {
                throw new ArgumentException();
            }

            if (encrypted && IsBroadcastMac(peerMac))
            {
                throw new ArgumentException();
            }

            var nret = NativeEspNowAddPeer(peerMac, channel, encrypted, localMasterKey);
            if (nret != 0)
            {
                throw new EspNowException(nret);
            }
        }

        /// <summary>
        /// Send data to already registered peer.
        /// </summary>
        /// <param name="peerMac">MAC address of already added peer.</param>
        /// <param name="data">Data to be sent.</param>
        /// <param name="dataLen">Length of data.</param>
        public void Send(byte[] peerMac, byte[] data, int dataLen)
        {
            var nret = NativeEspNowSend(peerMac, data, dataLen);
            if (nret != 0)
            {
                throw new EspNowException(nret);
            }
        }

        internal void OnDataReceived(byte[] peerMac, byte[] data, int dataLen)
        {
            var eh = this.DataReceived;
            if (eh != null)
            {
                eh(this, new DataReceivedEventArgs(peerMac, data, dataLen));
            }
        }

        internal void OnDataSent(byte[] peerMac, int sendStatus)
        {
            var eh = this.DataSent;
            if (eh != null)
            {
                eh(this, new DataSentEventArgs(peerMac, sendStatus));
            }
        }

        /// <summary>
        /// Dispose()
        /// </summary>
        /// <param name="isDisposing">false on destructor call.</param>
        protected virtual void Dispose(bool isDisposing)
        {
            if (!isDisposed)
            {
                if (isDisposing)
                {
                    if (eventHandler != null)
                    {
                        EventSink.RemoveEventProcessor((EventCategory)EVENT_ESPNOW, eventHandler);
                    }
                }

                eventHandler = null;

                NativeDispose(isDisposing);

                isDisposed = true;
            }
        }

        /// <summary>
        /// Destructor to assure Dispose will be called.
        /// </summary>
        ~EspNowController()
        {
            Dispose(false);
        }

        /// <summary>
        /// Dispose()
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private static bool IsBroadcastMac(byte[] mac)
        {
            if (mac == null || mac.Length != MacAddressLength)
            {
                return false;
            }

            for (int i = 0; i < mac.Length; i++)
            {
                if (mac[i] != BroadcastMacByte)
                {
                    return false;
                }
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern int NativeInitialize();

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern void NativeDispose(bool isDisposing);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern int NativeEspNowSend(byte[] peerMac, byte[] data, int dataLen);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern int NativeEspNowAddPeer(byte[] peerMac, byte channel, bool encrypted, byte[] localMasterKey);
    }

    internal class DataSentEventInternal : BaseEvent
    {
        // these fields are set on native side
#pragma warning disable 0649
        public byte[] PeerMac;
        public int Status;
#pragma warning restore 0649

    }

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
