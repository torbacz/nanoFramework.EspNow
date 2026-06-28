//
// Copyright (c) 2020 The nanoFramework project contributors
// See LICENSE file in the project root for full license information.
//

using nanoFramework.Runtime.Events;
using System;
using System.Runtime.CompilerServices;

namespace nanoFramework.EspNow
{
    internal sealed class EspNowEventHandler : IEventProcessor, IEventListener
    {
        private readonly EspNowController controllerInstance;

        public EspNowEventHandler(EspNowController controllerInstance)
        {
            this.controllerInstance = controllerInstance;
        }

        public void InitializeForEventSource()
        {
            // no op
        }

        public bool OnEvent(BaseEvent ev)
        {
            var dataRecvEvent = ev as DataRecvEventInternal;
            if (dataRecvEvent != null)
            {
                controllerInstance.OnDataReceived(dataRecvEvent.PeerMac, dataRecvEvent.Data, dataRecvEvent.DataLen);
                return true;
            }

            var dataSentEvent = ev as DataSentEventInternal;
            if (dataSentEvent != null)
            {
                controllerInstance.OnDataSent(dataSentEvent.PeerMac, dataSentEvent.Status);
                return true;
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.InternalCall)]
        extern public BaseEvent ProcessEvent(uint data1, uint data2, DateTime time);
    }
}
