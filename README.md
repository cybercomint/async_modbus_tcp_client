# Asynchronous Modbus Tcp Client Library For .Net

## Overview
async_modbus_tcp_client is is a simple, fast and asynchronous .Net library which communicates with remote devices using [Modbus Application Protocol V1.1b3](http://modbus.org/ "Modbus.org").

## Supported Modbus Functions
| Function Name | Function Code |
| :------------ | :-----------: |
| ReadCoils | 1 |
| ReadDiscreteInputs |2 |
| ReadHoldingRegisters |3 |
| ReadInputRegisters |4 |
| WriteSingleCoil |5 |
| WriteSingleRegister |6 |
| WriteMultipleCoils |15 |
| WriteMultipleRegisters |16 |

## Installation
Download the latest release from [here](https://github.com/ermanimer/modbus_tcp_client/releases "Releases") and add reference to your project or run the following command in the Nuget Package Manager Console.

    PM> Install-Package async_modbus_tcp_client

## Constructor
* #### ModbusTcpClient(string hostname, \[ushort port=502\], \[byte unitIdentifier=0\])
    * ##### Parameters:
        * **hostname**: Hostname of the remote device.
        * **port**: Port of the remote device. The default port is 502 for Modbus Tcp.
        * **unitIdentifier**: Unit identifier of the serial device if the communication will be made through a gateway. The default unit identifier is 0 for direct communication.
    * ##### Example:
        ```c#
        ModbusTcpClient modbusTcpClient = new ModbusTcpClient("192.168.1.6");
        ```

## Properties
* #### Busy:
    Gets a bool value indicating whether the ModbusTcpClient is running an asynchronous task.
    
* #### Connected:
    Gets a bool value indicating whether the ModbusTcpClient is connected to a remote device.

## Methods
* #### Connect()
    Connects to the remote device. Returns **Connected** property.
    * ##### Example:
        ```c#
        private async void buttonConnect_Click(object sender, EventArgs e) {
            try {
                //connect to the remote device
                bool result = await Task.Run(modbusTcpClient.Connect);

                //print result
                Debug.WriteLine(result.ToString());
            }
            catch(ModbusTcpClientException modbusTcpClientException) {
                Debug.WriteLine(modbusTcpClientException.ToString());
            }
        }
        ```
    * ##### Exceptions:
        Throws only ModbusTcpClientException.
        
        | Exception Code | Exception Message |
        |:--------------:| :---------------- |
        | 1 | ModbusTcpClient is busy. |
        | 5 | Hostname or port is not valid. |
        | 6 | Connection failed. |
        | 7 | Network stream failed. |

* #### Close()
    Disposes the tcp client instance and requests that the underlying tcp connection be closed. Returns a bool indicating if the task is successfully completed.
    * ##### Example:
        ```c#
        private async void buttonClose_Click(object sender, EventArgs e) {
            try {
                //close modbus tcp client
                bool result = await Task.Run(modbusTcpClient.Close);

                //print result
                Debug.WriteLine(result.ToString());
            }
            catch(ModbusTcpClientException modbusTcpClientException) {
                Debug.WriteLine(modbusTcpClientException.ToString());
            }
        }
        ```
    * ##### Exceptions:
        Throws only ModbusTcpClientException.
        
        | Exception Code | Exception Message |
        |:--------------:| :---------------- |
        | 1 | ModbusTcpClient is busy. |

* #### ReadCoilsAsync(ushort startingAddress, ushort quantityOfCoils)
    Reads coils from the remote device. Returns a bool array indicating each coil starting from the first coil.
    * ##### Parameters:
        * **startingAddress**: Starting adddress of the coils.
        * **quantityOfCoils**: Quantity of the coils to read.
    * ##### Example:
        ```c#
        private async void buttonReadCoils_Click(object sender, EventArgs e) {
            try {
                //read eight coils starting from the first coil
                bool[] coils = await modbusTcpClient.ReadCoilsAsync(0, 8);

                //print coils
                for(int index = 0; index < 8; index++) {
                    Debug.WriteLine($"Coil {index + 1} : {coils[index].ToString()}");
                }
            }
            catch(ModbusTcpClientException modbusTcpClientException) {
                Debug.WriteLine(modbusTcpClientException.ToString());
            }
        }
        ```
    * ##### Exceptions:
        Throws only ModbusTcpClientException.
        
        | Exception Code | Exception Message |
        |:--------------:| :---------------- |
        | 1 | ModbusTcpClient is busy. |
        | 2 | ModbusTcpClient is not connected to a remote device.  |
        | 3 | Sending request failed. |
        | 4 | Network stream is closed. |
        | 8 | Unexpected function code. |
        | 9 | Modbus exception, unexpected modbus error code. |
        | 10 | Modbus exception, illegal function. |
        | 11 | Modbus exception, illegal data access. |
        | 12 | Modbus exception, illegal data value. |
        | 13 | Modbus exception, server device failure. |
        | 14 | Quantity of coils is out of range. |

* #### ReadDiscreteInputsAsync(ushort startingAddress, ushort quantityOfDiscreteInputs)
    Reads discrete inputs from the remote device. Returns a bool array indicating each discrete input starting from the first discrete input.
    * ##### Parameters:
        * **startingAddress**: Starting adddress of the discrete inputs.
        * **quantityOfDiscreteInputs**: Quantity of the discrete inputs to read.
    * ##### Example:
        ```c#
        private async void buttonReadDiscreteInputs_Click(object sender, EventArgs e) {
            try {
                //read eight discrete inputs starting from the first discrete input
                bool[] discreteInputs = await modbusTcpClient.ReadDiscreteInputsAsync(0, 8);

                //print discrete inputs
                for(int index = 0; index < 8; index++) {
                    Debug.WriteLine($"Discrete Input {index + 1} : {discreteInputs[index].ToString()}");
                }
            }
            catch(ModbusTcpClientException modbusTcpClientException) {
                Debug.WriteLine(modbusTcpClientException.ToString());
            }
        }
        ```
    * ##### Exceptions:
        Throws only ModbusTcpClientException.
        
        | Exception Code | Exception Message |
        |:--------------:| :---------------- |
        | 1 | ModbusTcpClient is busy. |
        | 2 | ModbusTcpClient is not connected to a remote device.  |
        | 3 | Sending request failed. |
        | 4 | Network stream is closed. |
        | 8 | Unexpected function code. |
        | 9 | Modbus exception, unexpected modbus error code. |
        | 10 | Modbus exception, illegal function. |
        | 11 | Modbus exception, illegal data access. |
        | 12 | Modbus exception, illegal data value. |
        | 13 | Modbus exception, server device failure. |
        | 15 | Quantity of dicrete inputs is out of range. |

* #### ReadHoldingRegistersAsync(ushort startingAddress, ushort quantityOfHoldingRegisters)
    Reads holding registers from the remote device. Returns a byte array indicating each holding register starting from the first holding register.
    * ##### Parameters:
        * **startingAddress**: Starting adddress of the holding registers.
        * **quantityOfHoldingRegisters**: Quantity of the holding registers to read.
    * ##### Example:
        ```c#
        private async void buttonReadHoldingRegisters_Click(object sender, EventArgs e) {
            try {
                //read one ushort (16-bit unsigned integer), one uint (32-bit unsigned integer) 
                //and one float (32-bit floating point number) starting from the first holding register.
                ushort quantityOfRegisters = 1 + 2 + 2; //ushort + uint + float
                byte[] bytesOfHoldingRegisters = await modbusTcpClient.ReadHoldingRegistersAsync(0, quantityOfRegisters);

                //print ushort
                ushort _ushort = BitConverter.ToUInt16(bytesOfHoldingRegisters, 0);
                Debug.WriteLine($"ushort : {_ushort.ToString()}");

                //print uint
                uint _uint = BitConverter.ToUInt32(bytesOfHoldingRegisters, 2);
                Debug.WriteLine($"uint : {_uint.ToString()}");

                //print float
                float _float = BitConverter.ToSingle(bytesOfHoldingRegisters, 6);
                Debug.WriteLine($"float : {_float.ToString()}");
            }
            catch(ModbusTcpClientException modbusTcpClientException) {
                Debug.WriteLine(modbusTcpClientException.ToString());
            }
        }
        ```
    * ##### Exceptions:
        Throws only ModbusTcpClientException.
        
        | Exception Code | Exception Message |
        |:--------------:| :---------------- |
        | 1 | ModbusTcpClient is busy. |
        | 2 | ModbusTcpClient is not connected to a remote device.  |
        | 3 | Sending request failed. |
        | 4 | Network stream is closed. |
        | 8 | Unexpected function code. |
        | 9 | Modbus exception, unexpected modbus error code. |
        | 10 | Modbus exception, illegal function. |
        | 11 | Modbus exception, illegal data access. |
        | 12 | Modbus exception, illegal data value. |
        | 13 | Modbus exception, server device failure. |
        | 16 | Quantity of holding registers is out of range. |

* #### ReadInputRegistersAsync(ushort startingAddress, ushort quantityOfInputRegisters)
    Reads input registers from the remote device. Returns a byte array indicating each input register starting from the first input register.
    * ##### Parameters:
        * **startingAddress**: Starting adddress of the input registers.
        * **quantityOfInputRegisters**: Quantity of the input registers to read.
    * ##### Example:
        ```c#
        private async void buttonReadInputRegisters_Click(object sender, EventArgs e) {
            try {
                //read one ushort (16-bit unsigned integer), one uint (32-bit unsigned integer)
                //and one float (32-bit floating point number) starting from the first input register.
                ushort quantityOfRegisters = 1 + 2 + 2; //ushort + uint + float
                byte[] bytesOfInputRegisters = await modbusTcpClient.ReadInputRegistersAsync(0, quantityOfRegisters);

                //print ushort
                ushort _ushort = BitConverter.ToUInt16(bytesOfInputRegisters, 0);
                Debug.WriteLine($"ushort : {_ushort.ToString()}");

                //print uint
                uint _uint = BitConverter.ToUInt32(bytesOfInputRegisters, 2);
                Debug.WriteLine($"uint : {_uint.ToString()}");

                //print float
                float _float = BitConverter.ToSingle(bytesOfInputRegisters, 6);
                Debug.WriteLine($"float : {_float.ToString()}");
            }
            catch(ModbusTcpClientException modbusTcpClientException) {
                Debug.WriteLine(modbusTcpClientException.ToString());
            }
        }
        ```
    * ##### Exceptions:
        Throws only ModbusTcpClientException.
        
        | Exception Code | Exception Message |
        |:--------------:| :---------------- |
        | 1 | ModbusTcpClient is busy. |
        | 2 | ModbusTcpClient is not connected to a remote device.  |
        | 3 | Sending request failed. |
        | 4 | Network stream is closed. |
        | 8 | Unexpected function code. |
        | 9 | Modbus exception, unexpected modbus error code. |
        | 10 | Modbus exception, illegal function. |
        | 11 | Modbus exception, illegal data access. |
        | 12 | Modbus exception, illegal data value. |
        | 13 | Modbus exception, server device failure. |
        | 17 | Quantity of input registers is out of range. |

* #### WriteSingleCoilAsync(ushort coilAddress, bool coil)
    Writes a single coil to the remote device. Returns a bool indicating if the task is successfully completed.
    * ##### Parameters:
        * **coilAddress**: Adddress of the coil.
        * **coil**: Coil to write.
    * ##### Example:
        ```c#
        private async void buttonWriteSingleCoil_Click(object sender, EventArgs e) {
            try {
                //write true to the first coil 
                bool result = await modbusTcpClient.WriteSingleCoilAsync(1, true);

                //print result
                Debug.WriteLine(result.ToString());
            }
            catch(ModbusTcpClientException modbusTcpClientException) {
                Debug.WriteLine(modbusTcpClientException.ToString());
            }
        }
        ```
    * ##### Exceptions:
        Throws only ModbusTcpClientException.
        
        | Exception Code | Exception Message |
        |:--------------:| :---------------- |
        | 1 | ModbusTcpClient is busy. |
        | 2 | ModbusTcpClient is not connected to a remote device.  |
        | 3 | Sending request failed. |
        | 4 | Network stream is closed. |
        | 8 | Unexpected function code. |
        | 9 | Modbus exception, unexpected modbus error code. |
        | 10 | Modbus exception, illegal function. |
        | 11 | Modbus exception, illegal data access. |
        | 12 | Modbus exception, illegal data value. |
        | 13 | Modbus exception, server device failure. |

* #### WriteSingleRegisterAsync(ushort registerAddress, byte[] bytesOfRegister)
    Writes a single register to the remote device. Returns a bool indicating if the task is successfully completed.
    * ##### Parameters:
        * **registerAddress**: Adddress of the register.
        * **bytesOfRegister**: Bytes of the register to write.
    * ##### Example:
        ```c#
        private async void buttonWriteSingleRegister_Click(object sender, EventArgs e) {
            try {
                //write a ushort (16-bit unsigned integer) to the first register
                ushort _ushort = 1000;
                byte[] bytesOfRegister = BitConverter.GetBytes(_ushort);
                bool result = await modbusTcpClient.WriteSingleRegisterAsync(0, bytesOfRegister);

                //print result
                Debug.WriteLine(result.ToString());
            }
            catch(ModbusTcpClientException modbusTcpClientException) {
                Debug.WriteLine(modbusTcpClientException.ToString());
            }
        }
        ```
    * ##### Exceptions:
        Throws only ModbusTcpClientException.
        
        | Exception Code | Exception Message |
        |:--------------:| :---------------- |
        | 1 | ModbusTcpClient is busy. |
        | 2 | ModbusTcpClient is not connected to a remote device.  |
        | 3 | Sending request failed. |
        | 4 | Network stream is closed. |
        | 8 | Unexpected function code. |
        | 9 | Modbus exception, unexpected modbus error code. |
        | 10 | Modbus exception, illegal function. |
        | 11 | Modbus exception, illegal data access. |
        | 12 | Modbus exception, illegal data value. |
        | 13 | Modbus exception, server device failure. |
        | 18 | Byte count of register is out of range. |

* #### WriteMultipleCoilsAsync(ushort startingAddress, bool[] coils)
    Writes multiple coils to the remote device. Returns a bool indicating if the task is successfully completed.
    * ##### Parameters:
        * **coilAddress**: Startint address of the coils.
        * **coils**: Coils to write.
    * ##### Example:
        ```c#
        private async void buttonWriteMultipleCoils_Click(object sender, EventArgs e) {
            try {
                //write four cois starting from the first coil
                bool[] coils = new bool[4] { true, true, false, true };
                bool result = await modbusTcpClient.WriteMultipleCoilsAsync(0, coils);

                //print result
                Debug.WriteLine(result.ToString());
            }
            catch(ModbusTcpClientException modbusTcpClientException) {
                Debug.WriteLine(modbusTcpClientException.ToString());
            }
        }
        ```
    * ##### Exceptions:
        Throws only ModbusTcpClientException.
        
        | Exception Code | Exception Message |
        |:--------------:| :---------------- |
        | 1 | ModbusTcpClient is busy. |
        | 2 | ModbusTcpClient is not connected to a remote device.  |
        | 3 | Sending request failed. |
        | 4 | Network stream is closed. |
        | 8 | Unexpected function code. |
        | 9 | Modbus exception, unexpected modbus error code. |
        | 10 | Modbus exception, illegal function. |
        | 11 | Modbus exception, illegal data access. |
        | 12 | Modbus exception, illegal data value. |
        | 13 | Modbus exception, server device failure. |
        | 19 | Quantity of coils is out of range. |

* #### WriteMultipleRegistersAsync(ushort startingAddress, byte[] bytesOfRegisters)
    Writes multiple registers to the remote device. Returns a bool indicating if the writing task is successfully completed.
    * ##### Parameters:
        * **coilAddress**: Starting address of the registers.
        * **bytesOfRegisters**: Bytes of the registers to write.
    * ##### Example:
        ```c#
        private async void buttonWriteMultipleRegisters_Click(object sender, EventArgs e) {
            try {
                //write one ushort (16-bit unsigned integer), one uint (32-bit unsigned integer)
                //and one float (32-bit floating point number) starting from the first register.
                int byteCount = 2 + 4 + 4; //ushort + uint + float
                byte[] bytesOfRegisters = new byte[byteCount];

                //bytes of the ushort
                ushort _ushort = 1000;
                byte[] bytesOfUshort = BitConverter.GetBytes(_ushort);
                bytesOfUshort.CopyTo(bytesOfRegisters, 0);

                //bytes of uint
                uint _uint = 100000;
                byte[] bytesOfUint = BitConverter.GetBytes(_uint);
                bytesOfUint.CopyTo(bytesOfRegisters, 2);

                //bytes of float
                float _float = 3.14f;
                byte[] bytesOfFloat = BitConverter.GetBytes(_float);
                bytesOfFloat.CopyTo(bytesOfRegisters, 6);

                bool result = await modbusTcpClient.WriteMultipleRegistersAsync(0, bytesOfRegisters);

                //print result
                Debug.WriteLine(result.ToString());
            }
            catch(ModbusTcpClientException modbusTcpClientException) {
                Debug.WriteLine(modbusTcpClientException.ToString());
            }
        }
        ```
    * ##### Exceptions:
        Throws only ModbusTcpClientException.
        
        | Exception Code | Exception Message |
        |:--------------:| :---------------- |
        | 1 | ModbusTcpClient is busy. |
        | 2 | ModbusTcpClient is not connected to a remote device.  |
        | 3 | Sending request failed. |
        | 4 | Network stream is closed. |
        | 8 | Unexpected function code. |
        | 9 | Modbus exception, unexpected modbus error code. |
        | 10 | Modbus exception, illegal function. |
        | 11 | Modbus exception, illegal data access. |
        | 12 | Modbus exception, illegal data value. |
        | 13 | Modbus exception, server device failure. |
        | 20 | Byte count of registers is out of range. |

## All ModbusTcpClientException Codes and Messages
| Exception Code | Exception Message |
|:--------------:| :---------------- |
| 1 | ModbusTcpClient is busy. |
| 2 | ModbusTcpClient is not connected to a remote device.  |
| 3 | Sending request failed. |
| 4 | Network stream is closed. |
| 5 | Hostname or port is not valid. |
| 6 | Connection failed. |
| 7 | Network stream failed. |
| 8 | Unexpected function code. |
| 9 | Modbus exception, unexpected modbus error code. |
| 10 | Modbus exception, illegal function. |
| 11 | Modbus exception, illegal data access. |
| 12 | Modbus exception, illegal data value. |
| 13 | Modbus exception, server device failure. |
| 14 | Quantity of coils is out of range. |
| 15 | Quantity of dicrete inputs is out of range. |
| 16 | Quantity of holding registers is out of range. |
| 17 | Quantity of input registers is out of range. |
| 18 | Byte count of register is out of range. |
| 19 | Quantity of coils is out of range. |
| 20 | Byte count of registers is out of range. |

## PLC Test Results
* #### Delta DVP12SE:
    Tested on 22.03.2019

    | Function Name | Function Code | Result |
    | :------------ | :-----------: | :----- |
    | ReadCoils | 1 | Reads maximum 256 coils from bit memory S, starting from address 0.
    | ReadDiscreteInputs |2 | Reads maximum 256 coils from bit memory S, starting from address 0.
    | ReadHoldingRegisters |3 | Reads maximum 100 registers from register memory D, starting from address 0.
    | ReadInputRegisters |4 | Not implemented.
    | WriteSingleCoil |5 | Writes coil to bit memory S, starting from address 0.
    | WriteSingleRegister |6 | Writes register to register memety D, starting from address 0.
    | WriteMultipleCoils |15 | Writes coils to bit memory S, starting from address 0.
    | WriteMultipleRegisters |16 | Writes registers to register memety D, starting from address 0.
