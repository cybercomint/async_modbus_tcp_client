using System;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace async_modbus_tcp_client {
    public class ModbusTcpClient {
        public string Hostname { get; }
        public ushort Port { get; }
        public byte UnitIdentifier { get; }
        private ushort transactionIdentifier;
        private TcpClient tcpClient;
        private NetworkStream networkStream;
        public bool Busy { get; private set; }
        public bool Connected { get; private set; }

        public ModbusTcpClient(string hostname, ushort port = 502, byte unitIdentifier = 0) {
            Hostname = hostname;
            Port = port;
            UnitIdentifier = unitIdentifier;
        }

        public async Task<bool> ConnectAsync() {
            if(Busy) {
                throw new ModbusTcpClientException(1, "ModbusTcpClient is busy.");
            }
            Busy = true;
            try {
                await Task.Run(delegate {
                    tcpClient = new TcpClient(Hostname, Port);
                    networkStream = tcpClient.GetStream();
                });
                return Connected = true;
            }
            catch(ArgumentException) {
                Connected = false;
                throw new ModbusTcpClientException(5, "Hostname or port is not valid.");
            }
            catch(SocketException) {
                Connected = false;
                throw new ModbusTcpClientException(6, "Connection failed.");
            }
            catch(InvalidOperationException) {
                Connected = false;
                throw new ModbusTcpClientException(7, "Network stream failed.");
            }
            finally {
                Busy = false;
            }
        }

        public bool Close() {
            if(Busy) {
                throw new ModbusTcpClientException(1, "ModbusTcpClient is busy.");
            }
            if(networkStream != null) {
                networkStream.Close();
            }
            if(tcpClient != null) {
                tcpClient.Close();
            }
            Connected = false;
            return !(Connected = false);
        }

        private async Task<byte[]> SendRequestAsync(byte[] adu) {
            if(Busy) {
                throw new ModbusTcpClientException(1, "ModbusTcpClient is busy.");
            }
            if(!Connected) {
                throw new ModbusTcpClientException(2, "ModbusTcpClient is not connected to a remote device.");
            }
            Busy = true;
            try {
                byte[] response = new byte[256];
                await networkStream.WriteAsync(adu, 0, adu.Length);
                await networkStream.ReadAsync(response, 0, response.Length);
                return response;
            }
            catch(IOException) {
                throw new ModbusTcpClientException(3, "Sending request failed.");
            }
            catch(ObjectDisposedException) {
                throw new ModbusTcpClientException(4, "Network stream is closed.");
            }
            finally {
                Busy = false;
            }
        }

        private void IncrementTransactionIdentifier() {
            if(transactionIdentifier < ushort.MaxValue) {
                transactionIdentifier++;
            }
            else {
                transactionIdentifier = 0;
            }
        }

        private byte[] CreateMBAPHeader(ushort length) {
            IncrementTransactionIdentifier();
            byte[] mbapHeader = new byte[7];
            mbapHeader[0] = (byte) (transactionIdentifier >> 8);
            mbapHeader[1] = (byte) (transactionIdentifier & 0xFF);
            mbapHeader[2] = 0x00;
            mbapHeader[3] = 0x00;
            mbapHeader[4] = (byte) (length >> 8);
            mbapHeader[5] = (byte) (length & 0xFF);
            mbapHeader[6] = UnitIdentifier;
            return mbapHeader;
        }

        public async Task<bool[]> ReadCoilsAsync(ushort startingAddress, ushort quantityOfCoils) {
            //exception
            if(quantityOfCoils == 0 || quantityOfCoils > (ushort) QuantityLimit.ReadCoils ) {
                throw new ModbusTcpClientException(14, "Quantity of coils is out of range.");
            }
            //protocol data unit
            byte[] pdu = new byte[5];
            pdu[0] = (byte) FunctionCode.ReadCoils;
            pdu[1] = (byte) (startingAddress >> 8);
            pdu[2] = (byte) (startingAddress & 0xFF);
            pdu[3] = (byte) (quantityOfCoils >> 8);
            pdu[4] = (byte) (quantityOfCoils & 0xFF);
            //modbus application protocol header
            ushort length = (ushort) (pdu.Length + 1);
            byte[] mbapHeader = CreateMBAPHeader(length);
            //application data unit
            byte[] adu = new byte[mbapHeader.Length + pdu.Length];
            mbapHeader.CopyTo(adu, 0);
            pdu.CopyTo(adu, mbapHeader.Length);
            //read
            byte[] response = await SendRequestAsync(adu);
            //handle response
            bool[] coils = new bool[quantityOfCoils];
            byte functionCode = response[7];
            switch(functionCode) {
                case (byte) FunctionCode.ReadCoils:
                    byte byteCount = response[8];
                    for(int byteIndex = 0; byteIndex < byteCount; byteIndex++) {
                        for(int bitIndex = 0; bitIndex < 8; bitIndex++) {
                            int coilIndex = byteIndex * 8 + bitIndex;
                            if(coilIndex < quantityOfCoils) {
                                coils[coilIndex] = (response[9 + byteIndex] & (1 << bitIndex)) != 0;
                            }
                        }
                    }
                    break;
                case (byte) ErrorCode.ReadCoils:
                    byte modbusErrorCode = response[8];
                    switch(modbusErrorCode) {
                        case (byte) ModbusErrorCode.IllegalFunction:
                            throw new ModbusTcpClientException(10, "Modbus exception, illegal function.");
                        case (byte) ModbusErrorCode.IllegalDataAccess:
                            throw new ModbusTcpClientException(11, "Modbus exception, illegal data access.");
                        case (byte) ModbusErrorCode.IllegalDataValue:
                            throw new ModbusTcpClientException(12, "Modbus exception, illegal data value.");
                        case (byte) ModbusErrorCode.ServerDeviceFailure:
                            throw new ModbusTcpClientException(13, "Modbus exception, server device failure.");
                        default:
                            throw new ModbusTcpClientException(9, "Modbus exception, unexpected modbus error code.");
                    }
                default:
                    throw new ModbusTcpClientException(8, "Unexpected function code.");
            }
            return coils;
        }

        public async Task<bool[]> ReadDiscreteInputsAsync(ushort startingAddress, ushort quantityOfDiscreteInputs) {
            //exception
            if(quantityOfDiscreteInputs == 0 || quantityOfDiscreteInputs > (ushort) QuantityLimit.ReadDiscreteInputs) {
                throw new ModbusTcpClientException(15, "Quantity of dicrete inputs is out of range.");
            }
            //protocol data unit
            byte[] pdu = new byte[5];
            pdu[0] = (byte) FunctionCode.ReadDiscreteInputs;
            pdu[1] = (byte) (startingAddress >> 8);
            pdu[2] = (byte) (startingAddress & 0xFF);
            pdu[3] = (byte) (quantityOfDiscreteInputs >> 8);
            pdu[4] = (byte) (quantityOfDiscreteInputs & 0xFF);
            //modbus application protocol header
            ushort length = (ushort) (pdu.Length + 1);
            byte[] mbapHeader = CreateMBAPHeader(length);
            //application data unit
            byte[] adu = new byte[mbapHeader.Length + pdu.Length];
            mbapHeader.CopyTo(adu, 0);
            pdu.CopyTo(adu, mbapHeader.Length);
            //read
            byte[] response = await SendRequestAsync(adu);
            //handle response
            bool[] discreteInputs = new bool[quantityOfDiscreteInputs];
            byte functionCode = response[7];
            switch(functionCode) {
                case (byte) FunctionCode.ReadDiscreteInputs:
                    byte byteCount = response[8];
                    for(int byteIndex = 0; byteIndex < byteCount; byteIndex++) {
                        for(int bitIndex = 0; bitIndex < 8; bitIndex++) {
                            int discreteInputIndex = byteIndex * 8 + bitIndex;
                            if(discreteInputIndex < quantityOfDiscreteInputs) {
                                discreteInputs[discreteInputIndex] = (response[9 + byteIndex] & (1 << bitIndex)) != 0;
                            }
                        }
                    }
                    break;
                case (byte) ErrorCode.ReadDiscreteInputs:
                    byte modbusErrorCode = response[8];
                    switch(modbusErrorCode) {
                        case (byte) ModbusErrorCode.IllegalFunction:
                            throw new ModbusTcpClientException(10, "Modbus exception, illegal function.");
                        case (byte) ModbusErrorCode.IllegalDataAccess:
                            throw new ModbusTcpClientException(11, "Modbus exception, illegal data access.");
                        case (byte) ModbusErrorCode.IllegalDataValue:
                            throw new ModbusTcpClientException(12, "Modbus exception, illegal data value.");
                        case (byte) ModbusErrorCode.ServerDeviceFailure:
                            throw new ModbusTcpClientException(13, "Modbus exception, server device failure.");
                        default:
                            throw new ModbusTcpClientException(9, "Modbus exception, unexpected modbus error code.");
                    }
                default:
                    throw new ModbusTcpClientException(8, "Unexpected function code.");
            }
            return discreteInputs;
        }

        public async Task<byte[]> ReadHoldingRegistersAsync(ushort startingAddress, ushort quantityOfHoldingRegisters) {
            //exception
            if(quantityOfHoldingRegisters == 0 || quantityOfHoldingRegisters > (ushort) QuantityLimit.ReadHoldingRegisters) {
                throw new ModbusTcpClientException(16, "Quantity of holding registers is out of range.");
            }
            //protocol data unit
            byte[] pdu = new byte[5];
            pdu[0] = (byte) FunctionCode.ReadHoldingRegisters;
            pdu[1] = (byte) (startingAddress >> 8);
            pdu[2] = (byte) (startingAddress & 0xFF);
            pdu[3] = (byte) (quantityOfHoldingRegisters >> 8);
            pdu[4] = (byte) (quantityOfHoldingRegisters & 0xFF);
            //modbus application protocol header
            ushort length = (ushort) (pdu.Length + 1);
            byte[] mbapHeader = CreateMBAPHeader(length);
            //application data unit
            byte[] adu = new byte[mbapHeader.Length + pdu.Length];
            mbapHeader.CopyTo(adu, 0);
            pdu.CopyTo(adu, mbapHeader.Length);
            //read
            byte[] response = await SendRequestAsync(adu);
            //handle response
            int byteCount = quantityOfHoldingRegisters * 2;
            byte[] bytesOfHoldingRegisters = new byte[byteCount];
            byte functionCode = response[7];
            switch(functionCode) {
                case (byte) FunctionCode.ReadHoldingRegisters:
                    for(int byteIndex = 0; byteIndex < byteCount; byteIndex++) {
                        bytesOfHoldingRegisters[0] = response[9 + byteIndex];
                    }
                    break;
                case (byte) ErrorCode.ReadHoldingRegisters:
                    byte modbusErrorCode = response[8];
                    switch(modbusErrorCode) {
                        case (byte) ModbusErrorCode.IllegalFunction:
                            throw new ModbusTcpClientException(10, "Modbus exception, illegal function.");
                        case (byte) ModbusErrorCode.IllegalDataAccess:
                            throw new ModbusTcpClientException(11, "Modbus exception, illegal data access.");
                        case (byte) ModbusErrorCode.IllegalDataValue:
                            throw new ModbusTcpClientException(12, "Modbus exception, illegal data value.");
                        case (byte) ModbusErrorCode.ServerDeviceFailure:
                            throw new ModbusTcpClientException(13, "Modbus exception, server device failure.");
                        default:
                            throw new ModbusTcpClientException(9, "Modbus exception, unexpected modbus error code.");
                    }
                default:
                    throw new ModbusTcpClientException(8, "Unexpected function code.");
            }
            return bytesOfHoldingRegisters;
        }

        public async Task<byte[]> ReadInputRegistersAsync(ushort startingAddress, ushort quantityOfInputRegisters) {
            //exception
            if(quantityOfInputRegisters == 0 || quantityOfInputRegisters > (ushort) QuantityLimit.ReadInputRegisters) {
                throw new ModbusTcpClientException(17, "Quantity of input registers is out of range.");
            }
            //pdu
            byte[] pdu = new byte[5];
            pdu[0] = (byte) FunctionCode.ReadInputRegisters;
            pdu[1] = (byte) (startingAddress >> 8);
            pdu[2] = (byte) (startingAddress & 0xFF);
            pdu[3] = (byte) (quantityOfInputRegisters >> 8);
            pdu[4] = (byte) (quantityOfInputRegisters & 0xFF);
            //mbap header
            ushort length = (ushort) (pdu.Length + 1);
            byte[] mbapHeader = CreateMBAPHeader(length);
            //adu
            byte[] adu = new byte[mbapHeader.Length + pdu.Length];
            mbapHeader.CopyTo(adu, 0);
            pdu.CopyTo(adu, mbapHeader.Length);
            //read
            byte[] response = await SendRequestAsync(adu);
            //handle response
            int byteCount = quantityOfInputRegisters * 2;
            byte[] bytesOfInputRegisters = new byte[byteCount];
            byte functionCode = response[7];
            switch(functionCode) {
                case (byte) FunctionCode.ReadInputRegisters:
                    for(int byteIndex = 0; byteIndex < byteCount; byteIndex++) {
                        bytesOfInputRegisters[byteIndex] = response[9 + byteIndex];
                    }
                    break;
                case (byte) ErrorCode.ReadInputRegisters:
                    byte modbusErrorCode = response[8];
                    switch(modbusErrorCode) {
                        case (byte) ModbusErrorCode.IllegalFunction:
                            throw new ModbusTcpClientException(10, "Modbus exception, illegal function.");
                        case (byte) ModbusErrorCode.IllegalDataAccess:
                            throw new ModbusTcpClientException(11, "Modbus exception, illegal data access.");
                        case (byte) ModbusErrorCode.IllegalDataValue:
                            throw new ModbusTcpClientException(12, "Modbus exception, illegal data value.");
                        case (byte) ModbusErrorCode.ServerDeviceFailure:
                            throw new ModbusTcpClientException(13, "Modbus exception, server device failure.");
                        default:
                            throw new ModbusTcpClientException(9, "Modbus exception, unexpected modbus error code.");
                    }
                default:
                    throw new ModbusTcpClientException(8, "Unexpected function code.");
            }
            return bytesOfInputRegisters;
        }

        public async Task<bool> WriteSingleCoilAsync(ushort coilAddress, bool coil) {
            //pdu
            byte[] pdu = new byte[5];
            pdu[0] = (byte) FunctionCode.WriteSingleCoil;
            pdu[1] = (byte) (coilAddress >> 8);
            pdu[2] = (byte) (coilAddress & 255);
            pdu[3] = (byte) (((ushort) (coil ? 0xFF00 : 0x0000)) >> 8);
            pdu[4] = (byte) (((ushort) (coil ? 0xFF00 : 0x0000)) & 255);
            //mbap header
            ushort length = (ushort) (pdu.Length + 1);
            byte[] mbapHeader = CreateMBAPHeader(length);
            //adu
            byte[] adu = new byte[mbapHeader.Length + pdu.Length];
            mbapHeader.CopyTo(adu, 0);
            pdu.CopyTo(adu, mbapHeader.Length);
            //write
            byte[] response = await SendRequestAsync(adu);
            //handle response
            byte functionCode = response[7];
            switch(functionCode) {
                case (byte) FunctionCode.WriteSingleCoil:
                    return true;
                case (byte) ErrorCode.WriteSingleCoil:
                    byte modbusErrorCode = response[8];
                    switch(modbusErrorCode) {
                        case (byte) ModbusErrorCode.IllegalFunction:
                            throw new ModbusTcpClientException(10, "Modbus exception, illegal function.");
                        case (byte) ModbusErrorCode.IllegalDataAccess:
                            throw new ModbusTcpClientException(11, "Modbus exception, illegal data access.");
                        case (byte) ModbusErrorCode.IllegalDataValue:
                            throw new ModbusTcpClientException(12, "Modbus exception, illegal data value.");
                        case (byte) ModbusErrorCode.ServerDeviceFailure:
                            throw new ModbusTcpClientException(13, "Modbus exception, server device failure.");
                        default:
                            throw new ModbusTcpClientException(9, "Modbus exception, unexpected modbus error code.");
                    }
                default:
                    throw new ModbusTcpClientException(8, "Unexpected function code.");
            }
        }

        public async Task<bool> WriteSingleRegisterAsync(ushort registerAddress, byte[] bytesOfRegister) {
            //exception
            byte byteCount = (byte) bytesOfRegister.Length;
            if(byteCount != 2) {
                throw new ModbusTcpClientException(18, "Byte count of register is out of range.");
            }
            //pdu
            byte[] pdu = new byte[5];
            pdu[0] = (byte) FunctionCode.WriteSingleRegister;
            pdu[1] = (byte) (registerAddress >> 8);
            pdu[2] = (byte) (registerAddress & 255);
            pdu[3] = bytesOfRegister[1];
            pdu[4] = bytesOfRegister[0];
            //mbap header
            ushort length = (ushort) (pdu.Length + 1);
            byte[] mbapHeader = CreateMBAPHeader(length);
            //adu
            byte[] adu = new byte[mbapHeader.Length + pdu.Length];
            mbapHeader.CopyTo(adu, 0);
            pdu.CopyTo(adu, mbapHeader.Length);
            //write
            byte[] response = await SendRequestAsync(adu);
            //handle response
            byte functionCode = response[7];
            switch(functionCode) {
                case (byte) FunctionCode.WriteSingleRegister:
                    return true;
                case (byte) ErrorCode.WriteSingleRegister:
                    byte modbusErrorCode = response[8];
                    switch(modbusErrorCode) {
                        case (byte) ModbusErrorCode.IllegalFunction:
                            throw new ModbusTcpClientException(10, "Modbus exception, illegal function.");
                        case (byte) ModbusErrorCode.IllegalDataAccess:
                            throw new ModbusTcpClientException(11, "Modbus exception, illegal data access.");
                        case (byte) ModbusErrorCode.IllegalDataValue:
                            throw new ModbusTcpClientException(12, "Modbus exception, illegal data value.");
                        case (byte) ModbusErrorCode.ServerDeviceFailure:
                            throw new ModbusTcpClientException(13, "Modbus exception, server device failure.");
                        default:
                            throw new ModbusTcpClientException(9, "Modbus exception, unexpected modbus error code.");
                    }
                default:
                    throw new ModbusTcpClientException(8, "Unexpected function code.");
            }
        }

        public async Task<bool> WriteMultipleCoilsAsync(ushort startingAddress, bool[] coils) {
            //exception
            ushort quantityOfCoils = (ushort) coils.Length;
            if(quantityOfCoils == 0 || quantityOfCoils > (ushort) QuantityLimit.WriteMultipleCoils) {
                throw new ModbusTcpClientException(19, "Quantity of coils is out of range.");
            }
            //bytes of coils
            byte byteCount = (byte) Math.Ceiling(coils.Length / 8.0);
            byte[] bytesOfCoils = new byte[byteCount];
            for(int byteIndex = 0; byteIndex < byteCount; byteIndex++) {
                for(int bitIndex = 0; bitIndex < 8; bitIndex++) {
                    int coilIndex = byteIndex * 8 + bitIndex;
                    if(coilIndex < coils.Length) {
                        if(coils[coilIndex]) {
                            bytesOfCoils[byteIndex] |= (byte) (1 << bitIndex);
                        }
                    }
                }
            }
            //pdu
            byte[] pdu = new byte[6 + byteCount];
            pdu[0] = (byte) FunctionCode.WriteMultipleCoils;
            pdu[1] = (byte) (startingAddress >> 8);
            pdu[2] = (byte) (startingAddress & 255);
            pdu[3] = (byte) (quantityOfCoils >> 8);
            pdu[4] = (byte) (quantityOfCoils & 255);
            pdu[5] = byteCount;
            bytesOfCoils.CopyTo(pdu, 6);
            //mbap header
            ushort length = (ushort) (pdu.Length + 1);
            byte[] mbapHeader = CreateMBAPHeader(length);
            //adu
            byte[] adu = new byte[mbapHeader.Length + pdu.Length];
            mbapHeader.CopyTo(adu, 0);
            pdu.CopyTo(adu, mbapHeader.Length);
            //write
            byte[] response = await SendRequestAsync(adu);
            //handle response
            byte functionCode = response[7];
            switch(functionCode) {
                case (byte) FunctionCode.WriteMultipleCoils:
                    return true;
                case (byte) ErrorCode.WriteMultipleCoils:
                    byte modbusErrorCode = response[8];
                    switch(modbusErrorCode) {
                        case (byte) ModbusErrorCode.IllegalFunction:
                            throw new ModbusTcpClientException(10, "Modbus exception, illegal function.");
                        case (byte) ModbusErrorCode.IllegalDataAccess:
                            throw new ModbusTcpClientException(11, "Modbus exception, illegal data access.");
                        case (byte) ModbusErrorCode.IllegalDataValue:
                            throw new ModbusTcpClientException(12, "Modbus exception, illegal data value.");
                        case (byte) ModbusErrorCode.ServerDeviceFailure:
                            throw new ModbusTcpClientException(13, "Modbus exception, server device failure.");
                        default:
                            throw new ModbusTcpClientException(9, "Modbus exception, unexpected modbus error code.");
                    }
                default:
                    throw new ModbusTcpClientException(8, "Unexpected function code.");
            }
        }

        public async Task<bool> WriteMultipleRegistersAsync(ushort startingAddress, byte[] bytesOfRegisters) {
            //exception
            byte byteCount = (byte) bytesOfRegisters.Length;
            if(byteCount % 2 != 0 || byteCount > (ushort) QuantityLimit.WriteMultipleRegisters) {
                throw new ModbusTcpClientException(20, "Byte count of registers is out of range.");
            }
            //pdu
            ushort quantityOfRegisters = (ushort) (byteCount / 2);
            byte[] pdu = new byte[6 + byteCount];
            pdu[0] = (byte) FunctionCode.WriteMultipleRegisters;
            pdu[1] = (byte) (startingAddress >> 8);
            pdu[2] = (byte) (startingAddress & 255);
            pdu[3] = (byte) (quantityOfRegisters >> 8);
            pdu[4] = (byte) (quantityOfRegisters & 255);
            pdu[5] = byteCount;
            bytesOfRegisters.CopyTo(pdu, 6);
            //mbap header
            ushort length = (ushort) (pdu.Length + 1);
            byte[] mbapHeader = CreateMBAPHeader(length);
            //adu
            byte[] adu = new byte[mbapHeader.Length + pdu.Length];
            mbapHeader.CopyTo(adu, 0);
            pdu.CopyTo(adu, mbapHeader.Length);
            //write
            byte[] response = await SendRequestAsync(adu);
            //handle response
            byte functionCode = response[7];
            switch(functionCode) {
                case (byte) FunctionCode.WriteMultipleRegisters:
                    return true;
                case (byte) ErrorCode.WriteMultipleRegisters:
                    byte modbusErrorCode = response[8];
                    switch(modbusErrorCode) {
                        case (byte) ModbusErrorCode.IllegalFunction:
                            throw new ModbusTcpClientException(10, "Modbus exception, illegal function.");
                        case (byte) ModbusErrorCode.IllegalDataAccess:
                            throw new ModbusTcpClientException(11, "Modbus exception, illegal data access.");
                        case (byte) ModbusErrorCode.IllegalDataValue:
                            throw new ModbusTcpClientException(12, "Modbus exception, illegal data value.");
                        case (byte) ModbusErrorCode.ServerDeviceFailure:
                            throw new ModbusTcpClientException(13, "Modbus exception, server device failure.");
                        default:
                            throw new ModbusTcpClientException(9, "Modbus exception, unexpected modbus error code.");
                    }
                default:
                    throw new ModbusTcpClientException(8, "Unexpected function code.");
            }
        }

        public ushort GetUShort(byte[] readBytes, ushort address) {
            byte[] bytesOfUShort = new byte[2];
            bytesOfUShort[0] = readBytes[address + 1];
            bytesOfUShort[1] = readBytes[address];
            return BitConverter.ToUInt16(bytesOfUShort, 0);
        }

        public void SetUShort(byte[] bytesToWrite, ushort address, ushort value) {
            bytesToWrite[address] = (byte) (value >> 8);
            bytesToWrite[address + 1] = (byte) (value & 255);
        }

        public uint GetUInt(byte[] readBytes, ushort address) {
            byte[] bytesOfUInt = new byte[4];
            bytesOfUInt[0] = readBytes[address + 3];
            bytesOfUInt[1] = readBytes[address + 2];
            bytesOfUInt[2] = readBytes[address + 1];
            bytesOfUInt[3] = readBytes[address];
            return BitConverter.ToUInt32(bytesOfUInt, 0);
        }

        public void SetUInt(byte[] bytesToWrite, ushort address, uint value) {
            bytesToWrite[address] = (byte) ((value >> 24) & 255);
            bytesToWrite[address + 1] = (byte) ((value >> 16) & 255);
            bytesToWrite[address + 2] = (byte) ((value >> 8) & 255);
            bytesToWrite[address + 3] = (byte) (value & 255);
        }

        public float GetFloat(byte[] readBytes, ushort address) {
            byte[] bytesOfFloat = new byte[4];
            bytesOfFloat[0] = readBytes[address + 3];
            bytesOfFloat[1] = readBytes[address + 2];
            bytesOfFloat[2] = readBytes[address + 1];
            bytesOfFloat[3] = readBytes[address];
            return BitConverter.ToSingle(bytesOfFloat, 0);
        }

        public void SetFloat(byte[] bytesToWrite, ushort address, float value) {
            byte[] bytesOfFloat = BitConverter.GetBytes(value);
            bytesToWrite[address] = bytesOfFloat[3];
            bytesToWrite[address + 1] = bytesOfFloat[2];
            bytesToWrite[address + 2] = bytesOfFloat[1];
            bytesToWrite[address + 3] = bytesOfFloat[0];
        }
    }
}
