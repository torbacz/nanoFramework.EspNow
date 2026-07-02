//
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
//

using System;

namespace nanoFramework.EspNow
{
    /// <summary>
    /// EspNow related exception
    /// </summary>
    public class EspNowException : Exception
    {
        public const int ErrorEspNowInit = 10001;
        public const int ErrorInvalidPeer = 10002;
        public const int ErrorAddPeer = 10003;

        /// <summary>
        /// Native ESP-NOW error code.
        /// </summary>
        public int esp_err;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="esp_err">Native ESP-NOW error code.</param>
        public EspNowException(int esp_err)
            : base(GetMessage(esp_err))
        {
            this.esp_err = esp_err;
        }

        private static string GetMessage(int esp_err)
        {
            switch (esp_err)
            {
                case ErrorEspNowInit:
                    return "ESP-NOW init failed";
                case ErrorInvalidPeer:
                    return "Invalid ESP-NOW peer";
                case ErrorAddPeer:
                    return "ESP-NOW add peer failed";
                default:
                    return esp_err.ToString();
            }
        }
    }
}
