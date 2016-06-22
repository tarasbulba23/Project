using ScreenshotCaptureWithMouse.ScreenCapture;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace RemoteClient
{
	/// <summary>
    /// Logic for interaction MainWindow.xaml
    /// </summary>
    /// <permission>public</permission>
    /// <remarks>Inheritance of the class Window</remarks>
    public partial class MainWindow : Window
    {

        public int width = (int)SystemParameters.PrimaryScreenWidth;
        public int height = (int)SystemParameters.PrimaryScreenHeight;
        
        private UdpClient udpClient = new UdpClient();
        private IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 34000);

        private UdpClient receivingUdpClient = new UdpClient(34001);
        IPEndPoint sendIpEndPoint = null;

        private long quality;

        Thread sendF, procLisen, procStart, lisenF;
		
		/// <summary>
        /// Starting point of the program
        /// </summary>
        /// <permission>public</permission>
        /// <returns>Void</returns>
        public MainWindow()
        {
            InitializeComponent();

            quality = 100;
        }

		/// <summary>
        /// Sends the message until it receives confirmation from the server
        /// </summary>
        /// <permission>public</permission>
        /// <remarks>When server sent message, this theard has stop</remarks>
        /// <param>No required params</param>
        /// <returns>Void</returns>
        public void sendFirst()
        {
            String host = System.Net.Dns.GetHostName();
            IPAddress ip = Dns.GetHostByName(host).AddressList[0];

            byte[] lst = Encoding.UTF8.GetBytes(ip.ToString());

            while (true)
            {
                udpClient.Send(lst, lst.Length, ipEndPoint);
            }            
        }

		/// <summary>
        /// Waiting for a response from the server
        /// </summary>
        /// <exception>All posible exception</exception>
        /// <permission>public</permission>
        /// <remarks>When server send response, starts new theard function "lisen" and "start"</remarks>
        /// <param>No required params</param>
        /// <returns>Void</returns>
        public void lisFirst()
        {
            byte[] lst1;
            bool b = true;
            try
            {
                while (b)
                {

                    lst1 = receivingUdpClient.Receive(ref sendIpEndPoint);
                    string returnData = Encoding.UTF8.GetString(lst1);

                    if (returnData == "OK")
                    {
                        try
                        {
                            sendF.Abort();
                            sendF.Join(100);
                            b = false;
                        }
                        catch { }

                    }
                }
            }
            catch { }

            procLisen = new Thread(new ThreadStart(start));
            procLisen.Start();
            procStart = new Thread(new ThreadStart(lisen));
            procStart.Start();

        }

		/// <summary>
        /// When the server sends a message to a computer, he or shuts down or restart or logoff
        /// </summary>
        /// <exception>All posible exception, and write error in console</exception>
        /// <permission>public</permission>
        /// <remarks>Using Win32API</remarks>
        /// <param>No required params</param>
        /// <returns>Void</returns>
        internal const int EWX_REBOOT = 0x00000002;
        internal const int EWX_SHUTDOWN = 0x00000001;
        internal const int EWX_LOGOFF = 0x00000000;
        public void lisen()
        {
            bool b = true;

            try
            {

                while (b)
                {
                    byte[] receiveBytes = receivingUdpClient.Receive(ref sendIpEndPoint);

                    string returnData = Encoding.UTF8.GetString(receiveBytes);

                    if (returnData == "Powerdown")
                    {
                        ChangePC.DoExitWin(EWX_SHUTDOWN);
                        b = false;
                    }
                    else if (returnData == "Reset")
                    {
                        ChangePC.DoExitWin(EWX_REBOOT);
                        b = false;
                    }
                    else if (returnData == "LogOff")
                    {
                        ChangePC.DoExitWin(EWX_LOGOFF);
                        b = false;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Fatal Error: " + ex.ToString() + "\n  " + ex.Message);
            }


        }

        /// <summary>
        /// Make screenshot and resive to server program
        /// </summary>
        /// <exception>All posible exception, and write error in console</exception>
        /// <permission>public</permission>
        /// <param>No required params</param>
        /// <returns>Void</returns>
        public void start()
        {
            lisenF.Abort();
            lisenF.Join(100);
            
            Bitmap BackGround = new Bitmap(width, height);

            Stopwatch sw = new Stopwatch();
            sw.Start();

            while (true)
            {

                if (sw.ElapsedMilliseconds > 1000/6)
                {
                    BackGround = CaptureScreen.CaptureDesktopWithCursor();
                    byte[] bytes = VariousQuality(BackGround, quality);

                    List<byte[]> lst = CutMsg(bytes);

                    for (int i = 0; i < lst.Count; i++)
                    {
                        udpClient.Send(lst[i], lst[i].Length, ipEndPoint);
                    }
                }
            }

            udpClient.Close();

        }

		/// <summary>
        /// Breaks byte array to packages of 64K
        /// </summary>
        /// <permission>private</permission>
        /// <param name="bt">Takes pictures in a byte array</param>
        /// <returns>Returns packages as list array</returns>
        private List<byte[]> CutMsg(byte[] bt)
        {
            int Lenght = bt.Length;
            byte[] temp;
            List<byte[]> msg = new List<byte[]>();

            MemoryStream memoryStream = new MemoryStream();
            memoryStream.Write(BitConverter.GetBytes((short)((Lenght / 65500) + 1)), 0, 2);
            memoryStream.Write(bt, 0, bt.Length);

            memoryStream.Position = 0;
			
            while (Lenght > 0)
            {
                temp = new byte[65500];
                memoryStream.Read(temp, 0, 65500);
                msg.Add(temp);
                Lenght -= 65500;
            }

            return msg;
        }

		/// <summary>
        /// Set photo quality and conver to bytes array
        /// </summary>
        /// <exception>All posible exception</exception>
        /// <permission>private</permission>
        /// <param name="original">Takes image in format System.Drawing.Image</param>
        /// <param name="quality">Takes long type</param>
        /// <returns>Return bytes array</returns>
        private static byte[] VariousQuality(System.Drawing.Image original, long quality)
        {
            ImageCodecInfo jpgEncoder = null;
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();
            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == ImageFormat.Jpeg.Guid)
                {
                    jpgEncoder = codec;
                    break;
                }
            }
            if (jpgEncoder != null)
            {
                System.Drawing.Imaging.Encoder encoder = System.Drawing.Imaging.Encoder.Quality;
                EncoderParameters encoderParameters = new EncoderParameters(1);

                EncoderParameter encoderParameter = new EncoderParameter(encoder, quality);
                encoderParameters.Param[0] = encoderParameter;

                MemoryStream memoryStream = new MemoryStream();
                original.Save(memoryStream, jpgEncoder, encoderParameters);

                return memoryStream.ToArray();

            }
            else
            {
                throw new Exception("Error");
            }
        }

		/// <summary>
        /// Set quality image in real world time
        /// </summary>
        /// <permission>private</permission>
		/// <remarks>Works in asynchronous mode</remarks>
        /// <param name="sender">Default value</param>
        /// <param name="e">Default value</param>
        /// <returns>Void</returns>
        private void comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (comboBox.SelectedIndex == 0)
            {
                quality = 10;
            }
            else if (comboBox.SelectedIndex == 1)
            {
                quality = 50;
            }
            else if (comboBox.SelectedIndex == 2)
            {
                quality = 100;
            }
            else
            {
                quality = 100;
            }
        }

		/// <summary>
        /// User should write IP server, where data will be sent
        /// </summary>
        /// <permission>private</permission>
        /// <param name="sender">Default value</param>
        /// <param name="e">Default value</param>
        /// <returns>Void</returns>
        private void submit_Click(object sender, RoutedEventArgs e)
        {
            if (ip.Text != "")
            {
                ipEndPoint.Address = IPAddress.Parse(ip.Text);
                ipEndPoint.Port = 34000;
                WindowState = WindowState.Minimized;
                sendF = new Thread(new ThreadStart(sendFirst));
                sendF.Start();
                lisenF = new Thread(new ThreadStart(lisFirst));
                lisenF.Start();
                submit.IsEnabled = false;
                ip.IsEnabled = false;
            }
            else
            {
                MessageBox.Show("You should to write IP", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

		/// <summary>
        /// On closing window, stop all proces, and closing connection
        /// </summary>
        /// <exception>All posible exception, and write error in console</exception>
        /// <permission>private</permission>
        /// <param name="sender">Default value</param>
        /// <param name="e">Default value</param>
        /// <returns>Void</returns>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                procLisen.Abort();
                procLisen.Join(10);

                procStart.Abort();
                procStart.Join(10);

                udpClient.Close();
                receivingUdpClient.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Fatal Error: " + ex.ToString() + "\n  " + ex.Message);
            }
        }
    }

	/// <summary>
	/// This class takes Win32API
	/// </summary>
    class ChangePC
    {
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        internal struct TokPriv1Luid
        {
            public int Count;
            public long Luid;
            public int Attr;
        }

        [DllImport("kernel32.dll", ExactSpelling = true)]
        internal static extern IntPtr GetCurrentProcess();

        [DllImport("advapi32.dll", ExactSpelling = true, SetLastError = true)]
        internal static extern bool OpenProcessToken(IntPtr h, int acc, ref IntPtr
        phtok);

        [DllImport("advapi32.dll", SetLastError = true)]
        internal static extern bool LookupPrivilegeValue(string host, string name,
        ref long pluid);

        [DllImport("advapi32.dll", ExactSpelling = true, SetLastError = true)]
        internal static extern bool AdjustTokenPrivileges(IntPtr htok, bool disall,
        ref TokPriv1Luid newst, int len, IntPtr prev, IntPtr relen);

        [DllImport("user32.dll", ExactSpelling = true, SetLastError = true)]
        internal static extern bool ExitWindowsEx(int flg, int rea);

        internal const int SE_PRIVILEGE_ENABLED = 0x00000002;
        internal const int TOKEN_QUERY = 0x00000008;
        internal const int TOKEN_ADJUST_PRIVILEGES = 0x00000020;
        internal const string SE_SHUTDOWN_NAME = "SeShutdownPrivilege";
        internal const int EWX_LOGOFF = 0x00000000;
        internal const int EWX_SHUTDOWN = 0x00000001;
        internal const int EWX_REBOOT = 0x00000002;
        internal const int EWX_FORCE = 0x00000004;
        internal const int EWX_POWEROFF = 0x00000008;
        internal const int EWX_FORCEIFHUNG = 0x00000010;

		/// <summary>
        /// Reboot or Power Down or Log Off
        /// </summary>
        /// <permission>public</permission>
        /// <param name="flg">What do in this PC</param>
        /// <returns>Void</returns>
        public static void DoExitWin(int flg)
        {
            bool ok;
            TokPriv1Luid tp;
            IntPtr hproc = GetCurrentProcess();
            IntPtr htok = IntPtr.Zero;
            ok = OpenProcessToken(hproc, TOKEN_ADJUST_PRIVILEGES | TOKEN_QUERY, ref htok);
            tp.Count = 1;
            tp.Luid = 0;
            tp.Attr = SE_PRIVILEGE_ENABLED;
            ok = LookupPrivilegeValue(null, SE_SHUTDOWN_NAME, ref tp.Luid);
            ok = AdjustTokenPrivileges(htok, false, ref tp, 0, IntPtr.Zero, IntPtr.Zero);
            ok = ExitWindowsEx(flg, 0);
        }
    }

}
