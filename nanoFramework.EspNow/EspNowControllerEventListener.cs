//
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
//

using nanoFramework.Runtime.Events;
using System;

namespace nanoFramework.EspNow
{
    internal class EspNowControllerEventListener : IEventProcessor, IEventListener
    {
        private EspNowController _controller;

        public EspNowControllerEventListener()
        {
            EventSink.AddEventProcessor(
                EventCategory.EspNow,
                this);

            EventSink.AddEventListener(
                EventCategory.EspNow,
                this);
        }

        public BaseEvent ProcessEvent(
            uint data1,
            uint data2,
            DateTime time)
        {
            // Determine event type from data2
            var eventType = (EspNowEventType)(data2 & 0xFF);

            if (eventType == EspNowEventType.DataSent)
            {
                return new DataSentEventInternal();
            }
            else if (eventType == EspNowEventType.DataReceived)
            {
                return new DataRecvEventInternal();
            }

            return null;
        }

        public void InitializeForEventSource()
        {
        }

        public bool OnEvent(BaseEvent ev)
        {
            var controller = _controller;

            if (controller == null)
            {
                return false;
            }

            if (ev is DataRecvEventInternal dataRecvEvent)
            {
                controller.OnDataReceived(
                    dataRecvEvent.PeerMac,
                    dataRecvEvent.Data,
                    dataRecvEvent.DataLen);

                return true;
            }

            if (ev is DataSentEventInternal dataSentEvent)
            {
                controller.OnDataSent(
                    dataSentEvent.PeerMac,
                    dataSentEvent.Status);

                return true;
            }

            return false;
        }

        public void SetController(EspNowController controller)
        {
            _controller = controller;
        }

        public void ClearController()
        {
            _controller = null;
        }
    }
}
