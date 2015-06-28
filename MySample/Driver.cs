using System;
using System.IO.Ports;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using FtdAdapter;
using Modbus.Data;
using Modbus.Device;
using Modbus.Utility;

namespace MySample
{
	/// <summary>
	/// Demonstration of NModbus
	/// </summary>
	public class Driver
	{
        public static bool _shouldpoll = true;
		static void Main(string[] args)
		{
			log4net.Config.XmlConfigurator.Configure();

			try
			{
				//ModbusTcpMasterReadInputs();
				//SimplePerfTest();
				//ModbusSerialRtuMasterWriteRegisters();
				//ModbusSerialAsciiMasterReadRegisters();
				//ModbusTcpMasterReadInputs();				
				//StartModbusAsciiSlave();
				ModbusTcpMasterReadInputsFromModbusSlave();
				//ModbusSerialAsciiMasterReadRegistersFromModbusSlave();
				//StartModbusTcpSlave();
				//StartModbusUdpSlave();
				//StartModbusAsciiSlave();
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
			}

			Console.ReadKey();
		}

		/// <summary>
		/// Simple Modbus serial RTU master write holding registers example.
		/// </summary>
		public static void ModbusSerialRtuMasterWriteRegisters()
		{
			using (SerialPort port = new SerialPort("COM1"))
			{
				// configure serial port
				port.BaudRate = 9600;
				port.DataBits = 8;
				port.Parity = Parity.None;
				port.StopBits = StopBits.One;
				port.Open();

				// create modbus master
				IModbusSerialMaster master = ModbusSerialMaster.CreateRtu(port);

				byte slaveId = 1;
				ushort startAddress = 100;
				ushort[] registers = new ushort[] { 1, 2, 3 };

				// write three registers
				master.WriteMultipleRegisters(slaveId, startAddress, registers);
			}
		}

		/// <summary>
		/// Simple Modbus serial ASCII master read holding registers example.
		/// </summary>
		public static void ModbusSerialAsciiMasterReadRegisters()
		{
			using (SerialPort port = new SerialPort("COM1"))
			{
				// configure serial port
				port.BaudRate = 9600;
				port.DataBits = 8;
				port.Parity = Parity.None;
				port.StopBits = StopBits.One;
				port.Open();

				// create modbus master
				IModbusSerialMaster master = ModbusSerialMaster.CreateAscii(port);

				byte slaveId = 1;
				ushort startAddress = 1;
				ushort numRegisters = 5;

				// read five registers		
				ushort[] registers = master.ReadHoldingRegisters(slaveId, startAddress, numRegisters);

				for (int i = 0; i < numRegisters; i++)
					Console.WriteLine("Register {0}={1}", startAddress + i, registers[i]);
			}

			// output: 
			// Register 1=0
			// Register 2=0
			// Register 3=0
			// Register 4=0
			// Register 5=0
		}

		/// <summary>
		/// Simple Modbus serial USB RTU master write multiple coils example.
		/// </summary>
		public static void ModbusSerialUsbRtuMasterWriteCoils()
		{
			using (FtdUsbPort port = new FtdUsbPort())
			{
				// configure usb port
				port.BaudRate = 9600;
				port.DataBits = 8;
				port.Parity = FtdParity.None;
				port.StopBits = FtdStopBits.One;
				port.OpenByIndex(0);

				// create modbus master
				IModbusSerialMaster master = ModbusSerialMaster.CreateRtu(port);

				byte slaveId = 1;
				ushort startAddress = 1;

				// write three coils
				master.WriteMultipleCoils(slaveId, startAddress, new bool[] { true, false, true });
			}
		}

		/// <summary>
		/// Simple Modbus serial USB ASCII master write multiple coils example.
		/// </summary>
		public static void ModbusSerialUsbAsciiMasterWriteCoils()
		{
			using (FtdUsbPort port = new FtdUsbPort())
			{
				// configure usb port
				port.BaudRate = 9600;
				port.DataBits = 8;
				port.Parity = FtdParity.None;
				port.StopBits = FtdStopBits.One;
				port.OpenByIndex(0);

				// create modbus master
				IModbusSerialMaster master = ModbusSerialMaster.CreateAscii(port);

				byte slaveId = 1;
				ushort startAddress = 1;

				// write three coils
				master.WriteMultipleCoils(slaveId, startAddress, new bool[] { true, false, true });
			}
		}

		/// <summary>
		/// Simple Modbus TCP master read inputs example.
		/// </summary>
		public static void ModbusTcpMasterReadInputs()
		{
			using (TcpClient client = new TcpClient("127.0.0.1", 502))
			{
				ModbusIpMaster master = ModbusIpMaster.CreateIp(client);

				// read five input values
				ushort startAddress = 100;
				ushort numInputs = 5;
				bool[] inputs = master.ReadInputs(startAddress, numInputs);

				for (int i = 0; i < numInputs; i++)
					Console.WriteLine("Input {0}={1}", startAddress + i, inputs[i] ? 1 : 0);
			}

			// output: 
			// Input 100=0
			// Input 101=0
			// Input 102=0
			// Input 103=0
			// Input 104=0
		}

		/// <summary>
		/// Simple Modbus UDP master write coils example.
		/// </summary>
		public static void ModbusUdpMasterWriteCoils()
		{			
			using (UdpClient client = new UdpClient())
			{
				IPEndPoint endPoint = new IPEndPoint(new IPAddress(new byte[] { 127, 0, 0, 1 }), 502);
				client.Connect(endPoint);

				ModbusIpMaster master = ModbusIpMaster.CreateIp(client);

				ushort startAddress = 1;

				// write three coils
				master.WriteMultipleCoils(startAddress, new bool[] { true, false, true });
			}
		}

		/// <summary>
		/// Simple Modbus serial ASCII slave example.
		/// </summary>
		public static void StartModbusSerialAsciiSlave()
		{
			using (SerialPort slavePort = new SerialPort("COM2"))
			{
				// configure serial port
				slavePort.BaudRate = 9600;
				slavePort.DataBits = 8;
				slavePort.Parity = Parity.None;
				slavePort.StopBits = StopBits.One;
				slavePort.Open();

				byte unitId = 1;

				// create modbus slave
				ModbusSlave slave = ModbusSerialSlave.CreateAscii(unitId, slavePort);
				slave.DataStore = DataStoreFactory.CreateDefaultDataStore();

				slave.Listen();
			}
		}

		/// <summary>
		/// Simple Modbus serial RTU slave example.
		/// </summary>
		public static void StartModbusSerialRtuSlave()
		{
			using (SerialPort slavePort = new SerialPort("COM2"))
			{
				// configure serial port
				slavePort.BaudRate = 9600;
				slavePort.DataBits = 8;
				slavePort.Parity = Parity.None;
				slavePort.StopBits = StopBits.One;
				slavePort.Open();

				byte unitId = 1;

				// create modbus slave
				ModbusSlave slave = ModbusSerialSlave.CreateRtu(unitId, slavePort);
				slave.DataStore = DataStoreFactory.CreateDefaultDataStore();

				slave.Listen();
			}
		}

		/// <summary>
		/// Simple Modbus serial USB ASCII slave example.
		/// </summary>
		public static void StartModbusSerialUsbAsciiSlave()
		{
			// TODO
		}

		/// <summary>
		/// Simple Modbus serial USB RTU slave example.
		/// </summary>
		public static void StartModbusSerialUsbRtuSlave()
		{
			// TODO
		}

		/// <summary>
		/// Simple Modbus TCP slave example.
		/// </summary>
		public static void StartModbusTcpSlave()
		{
			byte slaveId = 1;
			int port = 502;
			IPAddress address = new IPAddress(new byte[] { 127, 0, 0, 1 });

			// create and start the TCP slave
			TcpListener slaveTcpListener = new TcpListener(address, port);
			slaveTcpListener.Start();

			ModbusSlave slave = ModbusTcpSlave.CreateTcp(slaveId, slaveTcpListener);
			slave.DataStore = DataStoreFactory.CreateDefaultDataStore();

			slave.Listen();

			// prevent the main thread from exiting
			Thread.Sleep(Timeout.Infinite);
		}

		/// <summary>
		/// Simple Modbus UDP slave example.
		/// </summary>
		public static void StartModbusUdpSlave()
		{
			using (UdpClient client = new UdpClient(502))
			{
				ModbusUdpSlave slave = ModbusUdpSlave.CreateUdp(client);
				slave.DataStore = DataStoreFactory.CreateDefaultDataStore();

				slave.Listen();

				// prevent the main thread from exiting
				Thread.Sleep(Timeout.Infinite);
			}
		}

		/// <summary>
		/// Modbus TCP master and slave example.
		/// </summary>
		public static void ModbusTcpMasterReadInputsFromModbusSlave()
		{
			byte slaveId = 1;
			int port = 502;
			IPAddress address = new IPAddress(new byte[] { 127, 0, 0, 1 });

			// create and start the TCP slave
			TcpListener slaveTcpListener = new TcpListener(address, port);
			slaveTcpListener.Start();
			ModbusSlave slave = ModbusTcpSlave.CreateTcp(slaveId, slaveTcpListener);
			Thread slaveThread = new Thread(slave.Listen);
			slaveThread.Start();

			// create the master
			TcpClient masterTcpClient = new TcpClient(address.ToString(), port);
			ModbusIpMaster master = ModbusIpMaster.CreateIp(masterTcpClient);

			ushort numInputs = 5;
			ushort startAddress = 100;

			// read five register values
			ushort[] inputs = master.ReadInputRegisters(startAddress, numInputs);

			for (int i = 0; i < numInputs; i++)
				Console.WriteLine("Register {0}={1}", startAddress + i, inputs[i]);

			// clean up
            
			masterTcpClient.Close();
			slaveTcpListener.Stop();

			// output
			// Register 100=0
			// Register 101=0
			// Register 102=0
			// Register 103=0
			// Register 104=0
		}

        /// <summary>
        /// Modbus TCP master and slave Configuration for OmniID application.
        /// </summary>
        public static void ModbusTcpMasterSlaveConfigOmniID(IPAddress ModBusSlaveIP, byte SlaveId, int Port)
        {
            byte slaveId = SlaveId; //1
            int port = Port;// 502;

            // create and start the TCP slave
            TcpListener slaveTcpListener = new TcpListener(ModBusSlaveIP, port);
            slaveTcpListener.Start();
            ModbusSlave slave = ModbusTcpSlave.CreateTcp(slaveId, slaveTcpListener);
            Thread slaveThread = new Thread(slave.Listen);
            slaveThread.Start();

            // create the master
            TcpClient masterTcpClient = new TcpClient(ModBusSlaveIP.ToString(), port);
            ModbusIpMaster OmniMaster = ModbusIpMaster.CreateIp(masterTcpClient);

            ushort numFlagInputs = 1;
            ushort pollingAddressStart = 4000;
            ushort dataAddressStart = 4001;
            int numInputsTagID = 5;
            int numInputsWeight = 5;
            ushort numInputs = (ushort)(numInputsTagID + numInputsWeight);

            Thread pollingThreadOmni = new Thread(() => PollSlaveOmniId(OmniMaster, pollingAddressStart, numFlagInputs, dataAddressStart, numInputs));
            pollingThreadOmni.Start();
            

            Console.ReadKey();
            // clean up

            _shouldpoll = false;
            pollingThreadOmni.Join();
            masterTcpClient.Close();
            slaveTcpListener.Stop();

            // output
            // Register 100=0
            // Register 101=0
            // Register 102=0
            // Register 103=0
            // Register 104=0
        }

        /// <summary>
        /// use the master to poll the slave, investigating the flag register. on flag condition read information, parse and send to
        /// database.
        /// </summary>
        /// <param name="OmniMaster">Masters object</param>
        /// <param name="startAddress">start register of polling flag</param>
        /// <param name="numInputs"># of registers for the polling flag</param>
        /// <param name="dataStartAddress">start register of information</param>
        /// <param name="dataNumInputs"># of registers for the data</param>
        public static void PollSlaveOmniId(ModbusIpMaster OmniMaster, ushort startAddress, ushort numInputs, ushort dataStartAddress, ushort dataNumInputs)
        {

            // read polling register and do action when set high
            while (_shouldpoll)
            {
                ushort[] inputs = OmniMaster.ReadInputRegisters(startAddress, numInputs);
                int flag = inputs[0];

                if (flag > 0)
                {
                    ReadSlaveOmniID(OmniMaster, dataStartAddress, dataNumInputs);
                }
            }
     
        }

        /// <summary>
        /// read the data from the PLC and parse it and save it to the database
        /// </summary>
        /// <param name="OmniMaster">Masters object</param>
        /// <param name="startAddress">start register of information (start offset)</param>
        /// <param name="numInputs">number of registers containing information</param>
        public static void ReadSlaveOmniID(ModbusIpMaster OmniMaster, ushort startAddress, ushort numInputs)
        {
            ushort[] inputs = OmniMaster.ReadInputRegisters(startAddress, numInputs);

            //parsing logic and send information somewhere
            for (int i = 0; i < numInputs; i++)
            {
				Console.WriteLine("Register {0}={1}", startAddress + i, inputs[i]);
            }
            

        }

        public static void CloseConnectionsOmniMaster()
        {
            //fill in when the variables are made global
            // clean up

            //_shouldpoll = false;
            //pollingThreadOmni.Join();
            //masterTcpClient.Close();
            //slaveTcpListener.Stop();
        }

		/// <summary>
		/// Modbus serial ASCII master and slave example.
		/// </summary>
		public static void ModbusSerialAsciiMasterReadRegistersFromModbusSlave()
		{
			using (SerialPort masterPort = new SerialPort("COM1"))
			using (SerialPort slavePort = new SerialPort("COM2"))
			{
				// configure serial ports
				masterPort.BaudRate = slavePort.BaudRate = 9600;
				masterPort.DataBits = slavePort.DataBits = 8;
				masterPort.Parity = slavePort.Parity = Parity.None;
				masterPort.StopBits = slavePort.StopBits = StopBits.One;
				masterPort.Open();
				slavePort.Open();

				// create modbus slave on seperate thread
				byte slaveId = 1;
				ModbusSlave slave = ModbusSerialSlave.CreateAscii(slaveId, slavePort);
				Thread slaveThread = new Thread(new ThreadStart(slave.Listen));
				slaveThread.Start();

				// create modbus master
				ModbusSerialMaster master = ModbusSerialMaster.CreateAscii(masterPort);

				master.Transport.Retries = 5;
				ushort startAddress = 100;
				ushort numRegisters = 5;

				// read five register values
				ushort[] registers = master.ReadHoldingRegisters(slaveId, startAddress, numRegisters);

				for (int i = 0; i < numRegisters; i++)
					Console.WriteLine("Register {0}={1}", startAddress + i, registers[i]);
			}

			// output
			// Register 100=0
			// Register 101=0
			// Register 102=0
			// Register 103=0
			// Register 104=0
		}

		/// <summary>
		/// Write a 32 bit value.
		/// </summary>
		public static void ReadWrite32BitValue()
		{
			using (SerialPort port = new SerialPort("COM1"))
			{
				// configure serial port
				port.BaudRate = 9600;
				port.DataBits = 8;
				port.Parity = Parity.None;
				port.StopBits = StopBits.One;
				port.Open();

				// create modbus master
				ModbusSerialMaster master = ModbusSerialMaster.CreateRtu(port);

				byte slaveId = 1;
				ushort startAddress = 1008;
				uint largeValue = UInt16.MaxValue + 5;

				ushort lowOrderValue = BitConverter.ToUInt16(BitConverter.GetBytes(largeValue), 0);
				ushort highOrderValue = BitConverter.ToUInt16(BitConverter.GetBytes(largeValue), 2);

				// write large value in two 16 bit chunks
				master.WriteMultipleRegisters(slaveId, startAddress, new ushort[] { lowOrderValue, highOrderValue });

				// read large value in two 16 bit chunks and perform conversion
				ushort[] registers = master.ReadHoldingRegisters(slaveId, startAddress, 2);
				uint value = ModbusUtility.GetUInt32(registers[1], registers[0]);
			}
		}
	}
}
