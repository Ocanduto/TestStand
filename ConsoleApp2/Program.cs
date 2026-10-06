using NModbus;
using NModbus.Serial;

using System.Diagnostics.Metrics;
using System.IO.Ports;
using System.Runtime.Intrinsics.Arm;
using System.Text;

using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ConsoleApp2;

class Program
{

    static async Task Main(string[] args)
    {
        var modbusFactory = new ModbusFactory();
        var port = new SerialPort
        {
            PortName = "COM8",
            BaudRate = 9600,
            Parity = Parity.None,
            DataBits = 8,
            StopBits = StopBits.One,
            ReadTimeout = 500,
            WriteTimeout = 500
        };
        port.Open();

        Console.WriteLine(port.IsOpen);

        Console.ReadLine();

        IModbusSerialMaster master = modbusFactory.CreateRtuMaster(port);

        var data = (await master.ReadHoldingRegistersAsync(16, 512, 10)).ToBoolArray();

        foreach (var b in data)
        {
            Console.WriteLine($"{b}");
        }

        //var sendData = new ushort[17]
        //{
        //    0,
        //    0,
        //    0,
        //    0,
        //    0,
        //    0,
        //    0,
        //    1, // MOTOR
        //    0,
        //    0,
        //    0,
        //    0,
        //    0,
        //    0,
        //    0,
        //    0,
        //    0
        //};

        //await master.WriteMultipleRegistersAsync(16, 535, sendData);

        //await Task.Delay(3000);

        //sendData = new ushort[17]
        //{
        //    0,
        //    0,
        //    0,
        //    0,
        //    0,
        //    0,
        //    0,
        //    0, // MOTOR
        //    0,
        //    0,
        //    0,
        //    0,
        //    0,
        //    0,
        //    0,
        //    0,
        //    0
        //};

        //await master.WriteMultipleRegistersAsync(16, 535, sendData);


        //var sepValues = new char[] { '\u0002', '\u0003', '\u0004' };

        //Console.ForegroundColor = ConsoleColor.Gray;
        //Console.WriteLine("TEST PORTS");

        //var port = new SerialPort
        //{
        //    PortName = "COM4",
        //    BaudRate = 9600,
        //    Parity = Parity.None,
        //    DataBits = 8,
        //    StopBits = StopBits.One,
        //    ReadTimeout = 500,
        //    WriteTimeout = 500,
        //    Encoding = Encoding.ASCII
        //};

        //port.Open();

        //ConsoleKey key;
        //while ((key = Console.ReadKey().Key) != ConsoleKey.Escape)
        //{
        //    Console.WriteLine(key);

        //    port.Write($"{ASCIIChars.CR}");
        //    await Task.Delay(20);

        //    if (key == ConsoleKey.A)
        //    {
        //        Console.WriteLine("181.02.82.810".GetCommand());
        //        port.Write("181.02.82.810".GetCommand());
        //        await Task.Delay(20);
        //        Console.WriteLine("12345    2024".GetCommand());
        //        port.Write("12345    2024".GetCommand());
        //    }
        //}

        //await Task.Delay(1000);
        //port.Close();

        Console.ReadLine();
        port.Dispose();
    }
}

public static class ASCIIChars
{
    public const char SOH = '\u0001';
    public const char STX = '\u0002';
    public const char ETX = '\u0003';
    public const char EOT = '\u0004';
    public const char ENQ = '\u0005';
    public const char ETB = '\u0023';
    public const char CR = '\u000D';
    public const char FF = '\u000C';
    private static char[] chars =
    [
        '\u002e',
        '\u0030',
        '\u0031',
        '\u0032',
        '\u0033',
        '\u0034',
        '\u0035',
        '\u0036',
        '\u0037',
        '\u0038',
        '\u0039',
        '\u0020'
    ];
    public static string GetText(this string text)
    {
        var b = new StringBuilder();
        foreach (char c in text)
        {
            switch (c)
            {
                default: continue;
                case '.': b.Append(chars[0]); continue;
                case '0': b.Append(chars[1]); continue;
                case '1': b.Append(chars[2]); continue;
                case '2': b.Append(chars[3]); continue;
                case '3': b.Append(chars[4]); continue;
                case '4': b.Append(chars[5]); continue;
                case '5': b.Append(chars[6]); continue;
                case '6': b.Append(chars[7]); continue;
                case '7': b.Append(chars[8]); continue;
                case '8': b.Append(chars[9]); continue;
                case '9': b.Append(chars[10]); continue;
                case ' ': b.Append(chars[11]); continue;
            }
        }
        return b.ToString();
    }
    public static string GetCommand(this string text)
    {
        return $"{STX}{text.GetText()}{ETX}";
    }
}

public static class DataConverter
{
    public static bool ToBool(this ushort source) =>
        source != 0;

    public static bool[] ToBoolArray(this ushort[] source) =>
        source.Select(q => q.ToBool()).ToArray();
}
