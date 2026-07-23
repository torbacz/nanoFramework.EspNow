//
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
//

using System;
using System.Runtime.CompilerServices;
using nanoFramework.Runtime.Events;

namespace nanoFramework.EspNow
{
    internal sealed class EspNowControllerEventListener : IEventProcessor, IEventListener
    {
        private readonly EspNowController _controller;

        public EspNowControllerEventListener(EspNowController controller)
        {
            _controller = controller;
        }

        public void InitializeForEventSource()
        {
        }

        public bool OnEvent(BaseEvent ev)
        {
            if (ev is DataRecvEventInternal)
            {
                _controller.OnDataAvailable();
                return true;
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern BaseEvent ProcessEvent(uint data1, uint data2, DateTime time);
    }
}
