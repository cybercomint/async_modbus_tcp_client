using System;

namespace async_modbus_tcp_client {
    public class ModbusTcpClientException : Exception {
        public int Code { get; }
        public ModbusTcpClientException(int code, string message) : base(message) {
            Code = code;
        }
        public override string ToString() {
            return $"{Code.ToString()} : {Message}";
        }
    }
}
