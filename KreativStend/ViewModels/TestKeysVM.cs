using NModbus;
using NModbus.Serial;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace KreativStend.ViewModels
{
    public class TestKeysVM : WPFHelper.ViewModel
    {
        public TestKeysVM()
        {
            COMPORTS = ["COM8", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7"];
            COMPORT = Properties.Settings.Default.COMPORT;
            COMPORT2 = Properties.Settings.Default.COMPORT2;
            IP_Address = Properties.Settings.Default.IP_Address;
            Port = Properties.Settings.Default.Port;
            Ten = Properties.Settings.Default.Tens_On;
            MotorSpeed = Properties.Settings.Default.MotorSpeed;

            timer = new Timer(On_Tick);
            timer.Change(0, 500);
        }

        private SerialPort port;
        private SerialPort port2;
        private ModbusFactory modbusFactory;
        private IModbusMaster master;
        private Timer timer;
        private TcpClient tcpClient;

        public ObservableCollection<string> COMPORTS { get; set; }

        public bool IsConnected { get => Get<bool>(); set { Set(value); } }
        public string COMPORT { get => Get<string>(); set => Set(value); }
        public string COMPORT2 { get => Get<string>(); set => Set(value); }
        public string IP_Address { get => Get<string>(); set => Set(value); }
        public int Port { get => Get<int>(); set => Set(value); }
        public int MotorSpeed { get => Get<int>(); set => Set(value); }

        public bool UM1 { get => Get<bool>(); set => Set(value); }
        public bool UM2 { get => Get<bool>(); set => Set(value); }
        public bool L1 { get => Get<bool>(); set => Set(value); }
        public bool R1 { get => Get<bool>(); set => Set(value); }
        public bool L2 { get => Get<bool>(); set => Set(value); }
        public bool R2 { get => Get<bool>(); set => Set(value); }
		public bool Case_L_Open { get => Get<bool>(); set => Set(value); }
		public bool Case_L_Close { get => Get<bool>(); set => Set(value); }
		public bool Case_R_Open { get => Get<bool>(); set => Set(value); }
		public bool Case_R_Close { get => Get<bool>(); set => Set(value); }
		public bool Brake_L1_On { get => Get<bool>(); set => Set(value); }
		public bool Brake_L1_Off { get => Get<bool>(); set => Set(value); }
		public bool Brake_L2_On { get => Get<bool>(); set => Set(value); }
		public bool Brake_L2_Off { get => Get<bool>(); set => Set(value); }
		public bool Brake_R1_On { get => Get<bool>(); set => Set(value); }
		public bool Brake_R1_Off { get => Get<bool>(); set => Set(value); }
		public bool Brake_R2_On { get => Get<bool>(); set => Set(value); }
		public bool Brake_R2_Off { get => Get<bool>(); set => Set(value); }

		public bool[] Inputs { get => Get<bool[]>(); set => Set(value); }

        public double DD1_1 { get => Get<double>(); set => Set(value); }
        public double DD1_2 { get => Get<double>(); set => Set(value); }
        public double DD2_1 { get => Get<double>(); set => Set(value); }
        public double DD2_2 { get => Get<double>(); set => Set(value); }
        public double DD_Ger { get => Get<double>(); set => Set(value); }
        public double TM1 { get => Get<double>(); set => Set(value); }
        public ushort[] Counters { get => Get<ushort[]>(); set => Set(value); }
        //public string Torque { get => Get<string>(); set => Set(value); }
        //public string Speed { get => Get<string>(); set => Set(value); }

        public bool Ten { get => Get<bool>(); set => Set(value); }
        public bool M1 { get => Get<bool>(); set => Set(value); }
        public bool M2 { get => Get<bool>(); set => Set(value); }
        public bool M3 { get => Get<bool>(); set => Set(value); }
        public bool K1_M1 { get => Get<bool>(); set => Set(value); }
        public bool K2_M1 { get => Get<bool>(); set => Set(value); }
        public bool K1_M2 { get => Get<bool>(); set => Set(value); }
        public bool K2_M2 { get => Get<bool>(); set => Set(value); }
        public bool K1_M3 { get => Get<bool>(); set => Set(value); }
        public bool K2_M3 { get => Get<bool>(); set => Set(value); }
        public bool Q_1_Work { get => Get<bool>(); set => Set(value); }
        public bool Q_2_Work { get => Get<bool>(); set => Set(value); }
		public bool Q_3_Work { get => Get<bool>(); set => Set(value); }
		public bool Q_1_Clear { get => Get<bool>(); set => Set(value); }
		public bool Q_2_Clear { get => Get<bool>(); set => Set(value); }
		public bool Q_3_Clear { get => Get<bool>(); set => Set(value); }
        public bool Brake_L1 { get => Get<bool>(); set => Set(value); }
		public bool Brake_L2 { get => Get<bool>(); set => Set(value); }
		public bool Brake_R1 { get => Get<bool>(); set => Set(value); }
		public bool Brake_R2 { get => Get<bool>(); set => Set(value); }
		public bool Block_Dif_12 { get => Get<bool>(); set => Set(value); }
		public bool Block_Dif_24 { get => Get<bool>(); set => Set(value); }
		public bool Ger_On { get => Get<bool>(); set => Set(value); }
		public bool Ger_Off { get => Get<bool>(); set => Set(value); }
        public bool Motor_Case_L_Open { get => Get<bool>(); set => Set(value); }
		public bool Motor_Case_L_Close { get => Get<bool>(); set => Set(value); }
		public bool Motor_Case_R_Open { get => Get<bool>(); set => Set(value); }
		public bool Motor_Case_R_Close { get => Get<bool>(); set => Set(value); }
        public bool MotorTest { get => Get<bool>(); set => Set(value); }
        public bool MotorTest2 { get => Get<bool>(); set => Set(value); }
        public bool A3 { get => Get<bool>(); set => Set(value); }
        public bool A5 { get => Get<bool>(); set => Set(value); }


        bool SaveStart = false;
        bool SaveStop = false;
        //bool GoStart = false;
        int SaveSet_Speed = 0;
        bool GoStart = true;
        public bool Set_Speed { get => Get<bool>(); set => Set(value); }
        public bool StartMotor { get => Get<bool>(); set => Set(value); }
        public bool StopMotor { get => Get<bool>(); set => Set(value); }

        public bool ERROR { get => Get<bool>(); set => Set(value); }

        public int ErrorState { get => Get<int>(); set { Set(value); } }
        public bool Warning { get => Get<bool>(); set => Set(value); }

        public ErrorManager ErrorManager { get; } = new ErrorManager();

        private WPFHelper.Commands.ViewModelCommandHandler _OpenConnection = new();
        private WPFHelper.Commands.ViewModelCommandHandler _CloseConnection = new();

        public ICommand OpenConnection => _OpenConnection.Get(On_OpenConnection, () => !string.IsNullOrWhiteSpace(COMPORT) && !IsConnected);
        public ICommand CloseConnection => _CloseConnection.Get(On_CloseConnection, () => IsConnected);

        private async void On_Tick(object? state)
        {

            await CheckErrors();
            Warning = true;
            if (Warning)
            {
                //Application.Current.Dispatcher.Invoke(() =>
                //{
                //	MainWindowVM.Current.TriangleColor = new SolidColorBrush(Colors.Red);
                //});
            }

            //       if (port != null && port.IsOpen && false)
            //       {
            //           byte[] buffer = new byte[1024];

            //           // Отправка команды
            //           byte[] command = { 0x05, 0x01, 0x00, 0x00, 0x00, 0x00, 0x06, 0xAA }; // Пример Modbus команды
            //           port.Write(command, 0, command.Length);

            //           int count = port.Read(buffer, 0, buffer.Length);
            //           if (count == 19)
            //           {
            //               // Чтение ответа
            //               byte[] TorqueBuffer = new byte[7];
            //               Array.Copy(buffer, 2, TorqueBuffer, 0, 7);
            //               byte[] SpeedBuffer = new byte[6];
            //               Array.Copy(buffer, 9, SpeedBuffer, 0, 6);

            //               Torque = Encoding.ASCII.GetString(TorqueBuffer);
            //               Speed = Encoding.ASCII.GetString(SpeedBuffer);
            //           }
            //       }

            //       if (port2 != null && port2.IsOpen)
            //       {
            //           var SaveStart = false;
            //           var SaveStop = false;
            //           if (!Start & SaveStart)
            //           {
            //               SaveStart = false;
            //           }
            //           if (!Stop & SaveStop)
            //           {
            //               SaveStop = false;
            //           }
            //           if (Set_Speed)
            //           {
            //               Set_Speed = false;
            //               // Отправка команды
            //               //byte[] command = { 0x01, 0x06, 0x02, 0x0D, 0x05, 0xDC, 0xAA, 0xCB }; // 01 06 02 0D 02 BC 19 60
            //               //byte[] command = { 0x01, 0x06, 0x00, 0x02, 0x00, 0x1E, 0xA8, 0x02 };


            //               byte[] command = BuildSpeedCommand(MotorSpeed);


            //               //port2.DiscardInBuffer();
            ////byte[] command = { 0x01, 0x03, 0x00, 0x02, 0x00, 0x01, 0x25, 0xCA };


            //               port2.Write(command, 0, command.Length);


            //               //System.Threading.Thread.Sleep(100);

            //               //byte[] response = new byte[5];
            //               //int bytesRead = port2.Read(response, 0, 5);

            //               //if (bytesRead == 5 && response[0] == 1 && response[1] == 0x3)
            //               //{
            //                   //ushort statusWord = (ushort)((response[3] << 8) | response[4]);
            //                   //    Console.WriteLine($"Status Word: 0x{statusWord:X4} ({statusWord})");
            //               //}

            //           }
            //           if (Stop & !SaveStop)
            //           {
            //               // Отправка команды
            //               byte[] command = { 0x01, 0x06, 0x06, 0x05, 0x08, 0x81, 0x5E, 0xE3 }; //01 06 06 05 08 81 5E E3
            //               port2.Write(command, 0, command.Length);
            //               SaveStop = true;
            //           }
            //           else if (Start & !SaveStart)
            //           {
            //               // Отправка команды
            //               byte[] command = { 0x01, 0x06, 0x06, 0x05, 0x08, 0x82, 0x1E, 0xE2 }; //01 06 06 05 08 82 1E E2
            //               port2.Write(command, 0, command.Length);
            //               SaveStart = true;
            //           }
            //       }

            if (port2 != null && port2.IsOpen)
            {
                //SaveSet_Speed_1 = 0;
                if (!StartMotor & SaveStart)
                {
                    SaveStart = false;
                }
                if (!StopMotor & SaveStop)
                {
                    SaveStop = false;
                }
                if (SaveSet_Speed != MotorSpeed && !Set_Speed)
                {
                    Set_Speed = true;
                    SaveSet_Speed = MotorSpeed;
                }
                if (Set_Speed && MotorSpeed != 0)
                {
                    Set_Speed = false;
                    GoStart = false;
                    byte[] command = BuildSpeedCommand(MotorSpeed);

                    port2.Write(command, 0, command.Length);

                    await Task.Delay(500);
                    GoStart = true;
                }
                if (StopMotor && !SaveStop)
                {
                    // Отправка команды
                    SaveStop = true;
                    byte[] command = { 0x01, 0x06, 0x06, 0x05, 0x08, 0x81, 0x5E, 0xE3 }; //01 06 06 05 08 81 5E E3
                    port2.Write(command, 0, command.Length);
                }
                else if (StartMotor && !SaveStart && GoStart)
                {
                    // Отправка команды
                    SaveStart = true;
                    byte[] command = { 0x01, 0x06, 0x06, 0x05, 0x08, 0x82, 0x1E, 0xE2 }; //01 06 06 05 08 82 1E E2
                    port2.Write(command, 0, command.Length);
                }
                if(MotorSpeed == 0 && SaveStart)
                {
                    SaveStart = false;
                }
            }

            timer.Change(Timeout.Infinite, Timeout.Infinite);

            try
            {
                if (!IsConnected || master == null)
                {
                    UM1 = false;
                    UM2 = false;
                    L1 = false;
                    R1 = false;
                    L2 = false;
                    R2 = false;

                    ERROR = false;

					Case_L_Open = false;
					Case_L_Close = false;
					Case_R_Open = false;
					Case_R_Close = false;
					Brake_L1_On = false;
					Brake_L1_Off = false;
					Brake_L2_On = false;
					Brake_L2_Off = false;
					Brake_R1_On = false;
					Brake_R1_Off = false;
					Brake_R2_On = false;
					Brake_R2_Off = false;

					return;
                }

                var data = await master.ReadCoilsAsync(1, 0, 19);
                //var data = (await master.ReadHoldingRegistersAsync(16, 512, 10)).ToBoolArray();

                UM1 = data[0];
                UM2 = data[1];
                L1 = data[2];
                R1 = data[3];
                L2 = data[4];
                R2 = data[5];
                ERROR = data[6];
				Case_L_Open = data[7];
				Case_L_Close = data[8];
				Case_R_Open = data[9];
				Case_R_Close = data[10];
				Brake_L1_On = data[11];
				Brake_L1_Off = data[12];
				Brake_L2_On = data[13];
				Brake_L2_Off = data[14];
				Brake_R1_On = data[15];
				Brake_R1_Off = data[16];
				Brake_R2_On = data[17];
				Brake_R2_Off = data[18];


				Inputs = data;

                //ERROR = data[4];

                //DD1_1 = data[5];
                //DD1_2 = data[6];
                //DD2_1 = data[7];
                //DD2_2 = data[8];
                //TM1 = data[9];

                //var data_float = await master.ReadHoldingRegistersAsync(1, 24, 5);


                if (ERROR)
                {
                    Ten = false;

                    M1 = false;
                    M2 = false;
                    M3 = false;

                    K1_M1 = false;
                    K2_M1 = false;
                    K1_M2 = false;
                    K2_M2 = false;
                    K1_M3 = false;
                    K2_M3 = false;
                    Q_1_Work = false;
					Q_2_Work = false;
					Q_3_Work = false;
					Q_1_Clear = false;
					Q_2_Clear = false;
					Q_3_Clear = false;
					Brake_L1 = false;
					Brake_L2 = false;
					Brake_R1 = false;
					Brake_R2 = false;
					Block_Dif_12 = false;
					Block_Dif_24 = false;
					Ger_On = false;
                    Ger_Off = false;
                    Motor_Case_L_Open = false;
					Motor_Case_L_Close = false;
					Motor_Case_R_Open = false;
					Motor_Case_R_Close = false;
                    MotorTest = false;
                    MotorTest2 = false;
                }

                if(Case_L_Close)
                {
                    Motor_Case_L_Close = false;
                }
                if (Case_R_Close)
                {
                    Motor_Case_R_Close = false;
                }
                if (Case_L_Open)
                {
                    Motor_Case_L_Open = false;
                }
                if (Case_R_Open)
                {
                    Motor_Case_R_Open = false;
                }

                var sendData = new bool[32]
                {
                    Ten,
                    M1,
                    M2,
                    M3,
                    K1_M1,
                    K2_M1,
                    K1_M2,
                    K2_M2,
                    K1_M3,
                    K2_M3,
                    Q_1_Work,
					Q_2_Work,
					Q_3_Work,
					Q_1_Clear,
					Q_2_Clear,
					Q_3_Clear,
					Brake_L1,
					Brake_L2,
					Brake_R1,
					Brake_R2,
					Block_Dif_12,
					Block_Dif_24,
					Ger_On,
                    Ger_Off,
					Motor_Case_L_Open,
					Motor_Case_L_Close,
					Motor_Case_R_Open,
					Motor_Case_R_Close,
                    MotorTest,
                    MotorTest2,
                    A3,
                    A5
					//(bool)(Ten ? 1 : 0),
                    //(bool)(M1 ? 1u : 0),
                    //(bool)(M2 ? 1u : 0),
                    //(bool)(M3 ? 1u : 0),
                    //(bool)(K1_M1 ? 1u : 0),
                    //(bool)(K2_M1 ? 1u : 0),
                    //(bool)(K1_M2 ? 1u : 0),
                    //(bool)(K2_M2 ? 1 : 0),
                    //(bool)(K1_M3 ? 1 : 0),
                    //(bool)(K2_M3 ? 1 : 0)
                    //(ushort)(Y3 ? 1 : 0),
                    //(ushort)(Y4 ? 1 : 0),
                    //(ushort)(Y5 ? 1 : 0),
                    //(ushort)(Y6 ? 1 : 0),
                    //(ushort)(Y7 ? 1 : 0),
                    //(ushort)(Y8 ? 1 : 0),
                    //(ushort)(Y9 ? 1 : 0),
                    //(ushort)(Y10 ? 1 : 0)
                    //(ushort)(Y11 ? 1 : 0)
                };

                await master.WriteMultipleCoilsAsync(1, 32, sendData);

                var data_float = await master.ReadHoldingRegistersAsync(1, 4, 12);
                Counters = await master.ReadHoldingRegistersAsync(1, 16, 2);

                float[] Floats_From_PLC = new float[6];

                for (int i = 0; i < data_float.Length / 2; i++)
                {
                    byte[] floatVals1 = BitConverter.GetBytes(data_float[2 * i]);
                    byte[] floatVals2 = BitConverter.GetBytes(data_float[2 * i + 1]);

                    var floatVals = floatVals1.Concat(floatVals2).ToArray();

                    float f = BitConverter.ToSingle(floatVals, 0);

                    Floats_From_PLC[i] = f;
                }

                DD1_1 = Floats_From_PLC[0];
                DD1_2 = Floats_From_PLC[1];
                DD2_1 = Floats_From_PLC[2];
                DD2_2 = Floats_From_PLC[3];
                TM1 = Floats_From_PLC[4];
                DD_Ger = Floats_From_PLC[5];
            }
            catch (Exception e)
            {
                IsConnected = false;
                MessageBox.Show(e.ToString());
            }
            finally
            {
                timer.Change(0, 500);
            }
            Warning = false;
        }

        private async void On_OpenConnection()
        {
            try
            {
                if (tcpClient != null && tcpClient.Connected)
                {
                    IsConnected = true;
                }
                else
                {
                    modbusFactory = new ModbusFactory();
                    tcpClient = new TcpClient
                    {
                        SendTimeout = 2000,
                        ReceiveTimeout = 2000
                    };
                    tcpClient.Connect(IP_Address, Port);
                    IsConnected = tcpClient.Connected;
                }
                RaisePropertyChanged(nameof(OpenConnection));
                RaisePropertyChanged(nameof(CloseConnection));
            }
            catch (Exception e)
            {
                MessageBox.Show($"не удалось подключиться {Properties.Settings.Default.IP_Address}");
                MessageBox.Show(e.ToString());

                //if (port != null)
                //{
                //	port.Close();
                //	port.Dispose();
                //}
                //port = null;
                master = null;
                modbusFactory = null;
            }

            //try
            //{
            //    if (port != null && port.IsOpen)
            //    {
            //        IsConnected = true;
            //    }
            //    else
            //    {
            //        port = new SerialPort
            //        {
            //            PortName = Properties.Settings.Default.COMPORT,
            //            BaudRate = 9600,
            //            Parity = Parity.None,
            //            DataBits = 8,
            //            StopBits = StopBits.One,
            //            ReadTimeout = 1000,
            //            WriteTimeout = 1000,
            //            Encoding = Encoding.ASCII
            //        };
            //        port.Open();
            //    }
            //}
            //catch (Exception e)
            //{
            //    MessageBox.Show($"не удалось подключиться к {Properties.Settings.Default.COMPORT}");
            //    MessageBox.Show(e.ToString());

            //    if (port != null)
            //    {
            //        port.Close();
            //        port.Dispose();
            //    }
            //    port = null;
            //}

            try
            {
                if (port2 != null && port2.IsOpen)
                {
                    IsConnected = true;
                }
                else
                {
                    port2 = new SerialPort
                    {
                        PortName = Properties.Settings.Default.COMPORT2,
                        BaudRate = 9600,
                        Parity = Parity.None,
                        DataBits = 8,
                        StopBits = StopBits.One,
                        ReadTimeout = 2000,
                        WriteTimeout = 2000,
                        Encoding = Encoding.ASCII
                    };
                    port2.Open();

                    //Сразу отправка скорости.
                    //byte[] command = { 0x01, 0x06, 0x02, 0x0D, 0x02, 0xBC, 0x19, 0x60 }; // 01 06 02 0D 02 BC 19 60      700RPM
                    //byte[] command = { 0x01, 0x06, 0x02, 0x0D, 0x00, 0x28, 0x88, 0x6B }; // 01 06 00 02 00 28 C8 0B   или 01 06 02 0D 00 28 88 6B      40RPM
                    //port2.Write(command, 0, command.Length);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show($"не удалось подключиться к {Properties.Settings.Default.COMPORT2}");
                MessageBox.Show(e.ToString());

                if (port2 != null)
                {
                    port2.Close();
                    port2.Dispose();
                }
                port2 = null;
            }

            if (!IsConnected) return;

            try
            {
                if (master == null)
                {
                    master = modbusFactory.CreateMaster(tcpClient);
                }
            }
            catch (Exception e)
            {
                //port?.Close();
                //port?.Dispose();
                IsConnected = false;
                master = null;
                modbusFactory = null;
                //port = null;

                RaisePropertyChanged(nameof(OpenConnection));
                RaisePropertyChanged(nameof(CloseConnection));

                MessageBox.Show(e.ToString());
            }
        }
        private void On_CloseConnection()
        {
            port?.Close();
            port?.Dispose();
            IsConnected = false;
            master = null;
            modbusFactory = null;
            port = null;

            RaisePropertyChanged(nameof(OpenConnection));
            RaisePropertyChanged(nameof(CloseConnection));
        }

        private static byte[] BuildSpeedCommand(int MotorSpeed)
        {
            byte device = 0x01;
            byte function = 0x06;
            ushort register = 0x0002;

            byte[] frame = new byte[6];
            byte[] finalframe = new byte[8];

            frame[0] = device;
            frame[1] = function;
            frame[2] = (byte)(register >> 8);
            frame[3] = (byte)(register & 0xFF);
            frame[4] = (byte)(MotorSpeed >> 8);
            frame[5] = (byte)(MotorSpeed & 0xFF);

            Array.Copy(frame, 0, finalframe, 0, 6);

            ushort crc = ModbusCRC(frame);

            byte crcLo = (byte)(crc & 0xFF);
            byte crcHi = (byte)(crc >> 8);

            finalframe[6] = crcLo;
            finalframe[7] = crcHi;
            return finalframe;
        }

        public async Task ResetMarkirator()
        {
            var Check = false;
            while (!Check)
            {

                try
                {

                    port2.Write($"{ASCIIChars.CR}");
                    await Task.Delay(20);
                    port2.Write($"{ASCIIChars.FF}");
                    await Task.Delay(20);
                    Check = true;
                }
                catch
                {
                    await Task.Delay(1000);
                    Check = false;
                }
            }

        }

        public async Task CheckErrors()
        {
            if(true) //ЕСЛИ НЕ ВЫКЛЮЧЕН
            {
                ErrorManager.RaiseError(401);
                ErrorManager.RaiseError(601);

                if (DD1_1 > 2400 || DD1_1 < -100)
                {
                    //Error LVL 1 или 2 -> 1
                }
            }
            if (true) //ЕСЛИ НЕ ВЫКЛЮЧЕН
            {
                if (DD1_2 > 2400 || DD1_2 < -100)
                {
                    //Error LVL 1 или 2 -> 1
                }
            }
            if (true) //ЕСЛИ НЕ ВЫКЛЮЧЕН
            {
                if (DD2_1 > 2400 || DD2_1 < -100)
                {
                    //Error LVL 1 или 2 -> 1
                }
            }
            if (true) //ЕСЛИ НЕ ВЫКЛЮЧЕН
            {
                if (DD2_2 > 2400 || DD2_2 < -100)
                {
                    //Error LVL 1 или 2 -> 1
                }
            }
            if (true) //ЕСЛИ НЕ ВЫКЛЮЧЕН
            {
                if (DD_Ger > 6000 || DD_Ger < -10)
                {
                    //Error LVL 1
                }
            }
            if (true) //ЕСЛИ НЕ ВЫКЛЮЧЕН(?)
            {
                if (TM1 > 180 || TM1 < -60)
                {
                    //Error LVL 1 или 2->1(?) датчик
                }
                if (TM1 < 70)
                {
                    //Error LVL 1 температура масла
                }
            }
            if (true) //ВЫЛЕЗАЕТ ВСЕГДА. ФИЛЬТРЫ ЗАКАЧКИ
            {
                if (DD1_2 - DD1_1 > 0)
                {
                    //Error LVL 3
                }
            }
            if (true) //ВЫЛЕЗАЕТ ВСЕГДА. ФИЛЬТРЫ ОТКАЧКИ
            {
                if (DD2_2 - DD2_1 > 0)
                {
                    //Error LVL 3
                }
            }
            if (true) //ВЫЛЕЗАЕТ ВСЕГДА. УРОВНИ МАСЛА
            {
                //ЖДЕМ
            }
            if (!IsConnected)
            {
                //Error LVL 1 или 2 -> 1 
            }
        }



        public async Task SetMarkirator(string line1, string line2)
        {
            port2.Write(line1.GetCommand());
            await Task.Delay(20);
            port2.Write(line2.GetCommand());
            await Task.Delay(20);
        }


        static ushort ModbusCRC(byte[] data)
		{
			ushort crc = 0xFFFF;

			for (int i = 0; i < data.Length; i++)
			{
				crc ^= data[i];

				for (int j = 0; j < 8; j++)
				{
					if ((crc & 0x0001) != 0)
					{
						crc >>= 1;
						crc ^= 0xA001;
					}
					else
					{
						crc >>= 1;
					}
				}
			}

			return crc;
		}
	}
}
