using ScreenshotCaptureWithMouse.ScreenCapture;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RemoteClient
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public int width = (int)SystemParameters.PrimaryScreenWidth;
        public int height = (int)SystemParameters.PrimaryScreenHeight;

        //private IPEndPoint ipEndPoint;
        private UdpClient udpClient = new UdpClient();

        private long quality;

        private IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 34000);

        public MainWindow()
        {

            InitializeComponent();

            quality = Convert.ToInt64(comboBox.SelectionBoxItem.ToString());


            //byte[] lst = Encoding.UTF8.GetBytes("");
            //udpClient.Send(lst, lst.Length, ipEndPoint);

            //Thread tRec = new Thread(new ThreadStart(lisen));
            //tRec.Start();

            //InitializeComponent();
        }

        internal const int EWX_REBOOT = 0x00000002;
        internal const int EWX_SHUTDOWN = 0x00000001;
        internal const int EWX_LOGOFF = 0x00000000;
        public void lisen()
        {
            UdpClient receivingUdpClient = new UdpClient(34001);

            IPEndPoint RemoteIpEndPoint = null;

            bool b = true;

            try
            {

                while (b)
                {
                    // Ожидание дейтаграммы
                    byte[] receiveBytes = receivingUdpClient.Receive(
                       ref RemoteIpEndPoint);

                    // Преобразуем и отображаем данные
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


                    //Console.WriteLine(" --> " + returnData.ToString());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Возникло исключение: " + ex.ToString() + "\n  " + ex.Message);
            }


        }

        /// <summary>
        public void start()
        {

            Bitmap BackGround = new System.Drawing.Bitmap(width, height);

            Graphics graphics = Graphics.FromImage(BackGround);

            Stopwatch sw = new Stopwatch();
            sw.Start();

            //FileStream fs;

            while (true)
            {

                if (sw.ElapsedMilliseconds > 1000/3)
                {

                    graphics.CopyFromScreen(0, 0, 0, 0, new System.Drawing.Size(width, height));
                    byte[] bytes = VariousQuality(BackGround, quality);

                    List<byte[]> lst = CutMsg(bytes);
                    for (int i = 0; i < lst.Count; i++)
                    {
                        // Отправляем картинку клиенту
                        udpClient.Send(lst[i], lst[i].Length, ipEndPoint);
                    }
                    //Console.WriteLine(ipEndPoint.ToString());
                //fs = new FileStream("temp.png", FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
                //fs.Write(bytes, 0, bytes.Length);
                }
            }


            /*udpClient = new UdpClient();

            Bitmap BackGround = new System.Drawing.Bitmap(width, height);

            Graphics graphics = Graphics.FromImage(BackGround);

            /*while (true)
            {
                CaptureScreen.CaptureDesktop();

                // Получаем изображение в виде массива байтов
                //byte[] bytes = ConvertToByte(BackGround);
                List<byte[]> lst = CutMsg(bytes);
                for (int i = 0; i < lst.Count; i++)
                {
                    // Отправляем картинку клиенту
                    udpClient.Send(lst[i], lst[i].Length, ipEndPoint);
                }
            }*


            while (true)
            {
                    //BackGround = CaptureScreen.CaptureDesktopWithCursor();
                    graphics.CopyFromScreen(0, 0, 0, 0, new System.Drawing.Size(width, height));
                    byte[] bytes = VariousQuality(BackGround, quality);

                    List<byte[]> lst = CutMsg(bytes);
                    for (int i = 0; i < lst.Count; i++)
                    {
                        // Отправляем картинку клиенту
                        udpClient.Send(lst[i], lst[i].Length, ipEndPoint);
                    }
               /* for (int i = 0; i <= bytes.Length; i++)
                {
                    System.Console.WriteLine(bytes[i]);
                }

                if (System.)
                {

                }*
            }*/
        }

        private List<byte[]> CutMsg(byte[] bt)
        {
            int Lenght = bt.Length;
            byte[] temp;
            List<byte[]> msg = new List<byte[]>();

            MemoryStream memoryStream = new MemoryStream();
            // Записываем в первые 2 байта количество пакетов
            memoryStream.Write(BitConverter.GetBytes((short)((Lenght / 65500) + 1)), 0, 2);
            // Далее записываем первый пакет
            memoryStream.Write(bt, 0, bt.Length);

            memoryStream.Position = 0;
            // Пока все пакеты не разделили - делим КЭП
            while (Lenght > 0)
            {
                temp = new byte[65500];
                memoryStream.Read(temp, 0, 65500);
                msg.Add(temp);
                Lenght -= 65500;
            }

            return msg;
        }

        private byte[] VariousQuality(System.Drawing.Image original, long quality)
        {

            ImageCodecInfo jpgEncoder = null;
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();
            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == ImageFormat.Png.Guid)
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
                throw new Exception("Потеря");
            }
        }

        private void comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // ... Get the ComboBox.
            var comboBox = sender as ComboBox;
            long.TryParse(comboBox.SelectedItem as string, out quality);
        }

        private void submit_Click(object sender, RoutedEventArgs e)
        {
            if (ip.Text != "")
            {
                ipEndPoint.Address = IPAddress.Parse(ip.Text);
                ipEndPoint.Port = 34000;
                WindowState = WindowState.Minimized;
                Thread tRec = new Thread(new ThreadStart(start));
                tRec.Start();
                submit.IsEnabled = false;
                //Run();
            }
            else
            {
                MessageBox.Show("Требуется ввести имя", "Ошибка при вводе имени", MessageBoxButton.OK, MessageBoxImage.Error);
                //Run();
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {

            try { 
                udpClient.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Возникло исключение: " + ex.ToString() + "\n  " + ex.Message);
            }
        }

        /*public int IP
{
  get
  {
      return nazwisko;
  }
  set
  {
      if (value.Length > 20)
// zgłoszenie błędu
else
          nazwisko = value;
  }
}*/
        /// </summary>


    }

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

    class CaptureScreen
    {
        //This structure shall be used to keep the size of the screen.
        public struct SIZE
        {
            public int cx;
            public int cy;
        }

        public static Bitmap CaptureDesktop()
        {
            SIZE size;
            IntPtr hBitmap;
            IntPtr hDC = Win32Stuff.GetDC(Win32Stuff.GetDesktopWindow());
            IntPtr hMemDC = GDIStuff.CreateCompatibleDC(hDC);

            size.cx = Win32Stuff.GetSystemMetrics
                      (Win32Stuff.SM_CXSCREEN);

            size.cy = Win32Stuff.GetSystemMetrics
                      (Win32Stuff.SM_CYSCREEN);

            hBitmap = GDIStuff.CreateCompatibleBitmap(hDC, size.cx, size.cy);

            if (hBitmap != IntPtr.Zero)
            {
                IntPtr hOld = (IntPtr)GDIStuff.SelectObject
                                       (hMemDC, hBitmap);

                GDIStuff.BitBlt(hMemDC, 0, 0, size.cx, size.cy, hDC,
                                               0, 0, GDIStuff.SRCCOPY);

                GDIStuff.SelectObject(hMemDC, hOld);
                GDIStuff.DeleteDC(hMemDC);
                Win32Stuff.ReleaseDC(Win32Stuff.GetDesktopWindow(), hDC);
                Bitmap bmp = System.Drawing.Image.FromHbitmap(hBitmap);
                GDIStuff.DeleteObject(hBitmap);
                GC.Collect();
                return bmp;
            }
            return null;

        }


        public static Bitmap CaptureCursor(ref int x, ref int y)
        {
            Bitmap bmp;
            IntPtr hicon;
            Win32Stuff.CURSORINFO ci = new Win32Stuff.CURSORINFO();
            Win32Stuff.ICONINFO icInfo;
            ci.cbSize = Marshal.SizeOf(ci);
            if (Win32Stuff.GetCursorInfo(out ci))
            {
                if (ci.flags == Win32Stuff.CURSOR_SHOWING)
                {
                    hicon = Win32Stuff.CopyIcon(ci.hCursor);
                    if (Win32Stuff.GetIconInfo(hicon, out icInfo))
                    {
                        x = ci.ptScreenPos.x - ((int)icInfo.xHotspot);
                        y = ci.ptScreenPos.y - ((int)icInfo.yHotspot);

                        Icon ic = Icon.FromHandle(hicon);
                        bmp = ic.ToBitmap();
                        return bmp;
                    }
                }
            }

            return null;
        }

        public static Bitmap CaptureDesktopWithCursor()
        {
            int cursorX = 0;
            int cursorY = 0;
            Bitmap desktopBMP;
            Bitmap cursorBMP;
            Bitmap finalBMP;
            Graphics g;
            System.Drawing.Rectangle r;

            desktopBMP = CaptureDesktop();
            cursorBMP = CaptureCursor(ref cursorX, ref cursorY);
            if (desktopBMP != null)
            {
                if (cursorBMP != null)
                {
                    r = new System.Drawing.Rectangle(cursorX, cursorY, cursorBMP.Width, cursorBMP.Height);
                    g = Graphics.FromImage(desktopBMP);
                    g.DrawImage(cursorBMP, r);
                    g.Flush();

                    return desktopBMP;
                }
                else
                    return desktopBMP;
            }

            return null;

        }


    }
}
