namespace async_modbus_tcp_client {
    public enum QuantityLimit {
        ReadCoils = 2000,
        ReadDiscreteInputs = 2000,
        ReadHoldingRegisters = 125,
        ReadInputRegisters = 125,
        WriteMultipleCoils = 1968,
        WriteMultipleRegisters = 246,
    }

    public enum FunctionCode {
        ReadCoils = 1,
        ReadDiscreteInputs = 2,
        ReadHoldingRegisters = 3,
        ReadInputRegisters = 4,
        WriteSingleCoil = 5,
        WriteSingleRegister = 6,
        WriteMultipleCoils = 15,
        WriteMultipleRegisters = 16,
    }

    public enum ErrorCode {
        ReadCoils = 129,
        ReadDiscreteInputs = 130,
        ReadHoldingRegisters = 131,
        ReadInputRegisters = 132,
        WriteSingleCoil = 133,
        WriteSingleRegister =134,
        WriteMultipleCoils = 143,
        WriteMultipleRegisters = 144,
    }

    enum ModbusErrorCode {
        IllegalFunction = 1,
        IllegalDataAccess = 2,
        IllegalDataValue = 3,
        ServerDeviceFailure = 4,
    }
}
