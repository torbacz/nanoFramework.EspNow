//
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
//

using System;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using nanoFramework.Runtime.Events;

namespace nanoFramework.EspNow
{
    /// <summary>
    /// ESP-NOW controller class.
    /// </summary>
    public sealed class EspNowController : IDisposable
    {
        private const int MacAddressLength = 6;
        private const byte BroadcastMacByte = 0xff;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private static EspNowController s_instance;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private static readonly object s_syncLock = new object();

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private bool _disposed;

        // this is used as the lock object
        // a lock is required because multiple threads can access the EspNowController
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly object _syncLock = new object();

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly EspNowControllerEventListener _eventHandler;

        /// <summary>
        /// <see cref="DataSent"/> event handler type definition.
        /// </summary>
        public delegate void DataSendEventHandler(object sender, DataSentEventArgs eventArgs);

        /// <summary>
        /// Event raised after data sending completed.
        /// </summary>
        public event DataSendEventHandler DataSent;

        /// <summary>
        /// <see cref="DataReceived"/> event handler type definition.
        /// </summary>
        public delegate void DataReceivedEventHandler(object sender, DataReceivedEventArgs eventArgs);

        /// <summary>
        /// Event raised when data is received.
        /// </summary>
        public event DataReceivedEventHandler DataReceived;

        /// <summary>
        /// Represents an ESP-NOW controller.
        /// </summary>
        /// <exception cref="InvalidOperationException">Only one <see cref="EspNowController"/> instance is allowed per device.</exception>"
        public EspNowController()
        {
            lock (s_syncLock)
            {
                // Only allow one controller instance per device
                if (s_instance != null)
                {
                    throw new InvalidOperationException();
                }

                // call native init to allow HAL/PAL inits related with ESP-NOW hardware
                var initResult = NativeInitialize();

                if (initResult != 0)
                {
                    throw new EspNowException(initResult);
                }

                // Set this as the singleton instance
                s_instance = this;

                // Register with the event listener to receive callbacks from native interrupts
                _eventHandler = new EspNowControllerEventListener(this);
                EventSink.AddEventProcessor(EventCategory.EspNow, _eventHandler);
                EventSink.AddEventListener(EventCategory.EspNow, _eventHandler);
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
        /// <para><paramref name="peerMac"/> is <see langword="null"/>.</para>
        /// <para>-or-</para>
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
            if (peerMac == null)
            {
                throw new ArgumentException();
            }

            if (peerMac.Length != MacAddressLength)
            {
                throw new ArgumentException();
            }

            if (localMasterKey != null
                && localMasterKey.Length != 16)
            {
                throw new ArgumentException();
            }

            if (encrypted
                && IsBroadcastMac(peerMac))
            {
                throw new ArgumentException();
            }

            var addResult = NativeEspNowAddPeer(
                peerMac,
                channel,
                encrypted,
                localMasterKey);
            
            if (addResult != 0)
            {
                throw new EspNowException(addResult);
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
            DataReceivedEventHandler callbacks = null;

            lock (_syncLock)
            {
                if (!_disposed)
                {
                    callbacks = DataReceived;
                }
            }

            callbacks?.Invoke(this, new DataReceivedEventArgs(
                peerMac,
                data,
                dataLen));
        }

        internal void OnDataSent(byte[] peerMac, int sendStatus)
        {
            DataSendEventHandler callbacks = null;

            lock (_syncLock)
            {
                if (!_disposed)
                {
                    callbacks = DataSent;
                }
            }

            callbacks?.Invoke(this, new DataSentEventArgs(
                peerMac,
                (EspNowSendStatus)sendStatus));
        }

        private void Dispose(bool isDisposing)
        {
            if (!_disposed)
            {
                if (isDisposing)
                {
                    lock (s_syncLock)
                    {
                        // Clear the singleton instance
                        s_instance = null;

                        EventSink.RemoveEventProcessor(EventCategory.EspNow, _eventHandler);
                        EventSink.RemoveEventListener(EventCategory.EspNow, _eventHandler);
                    }
                }

                NativeDispose(isDisposing);

                _disposed = true;
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
            lock (_syncLock)
            {
                if (!_disposed)
                {
                    Dispose(true);

                    GC.SuppressFinalize(this);
                }
            }
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
}
