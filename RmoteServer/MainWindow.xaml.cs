using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
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

namespace RmoteServer
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private UdpClient udpClient = new UdpClient(34000);
        private IPEndPoint ipEndPoint = null;

        private UdpClient udpServer = new UdpClient();
        private IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 34001);

        private byte[] receiveBytes = new byte[0];

        private bool key = false;

        Thread tRec, tdata;
        //IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Parse(ip), 34001);


        //private static FileStream fs;

        int countErorr = 0;

        public MainWindow()
        {

            InitializeComponent();

            tRec = new Thread(new ThreadStart(sendOk));
            tRec.Start();
            

        }


        public void sendOk()
        {
            bool b = true;
            IPAddress address;

            

            while (b)
            {
                byte[] data = udpClient.Receive(ref ipEndPoint);
                string returnData = Encoding.UTF8.GetString(data);
                Console.WriteLine("ip " + returnData);
                if (IPAddress.TryParse(returnData, out address))
                {
                    switch (address.AddressFamily)
                    {
                        case System.Net.Sockets.AddressFamily.InterNetwork:
                            b = false;
                            key = true;
                            RemoteIpEndPoint.Address = address;
                            break;
                        case System.Net.Sockets.AddressFamily.InterNetworkV6:
                            b = false;
                            key = true;
                            RemoteIpEndPoint.Address = address;
                            break;
                    }
                }
            }

            

            // Отправляем данные
            byte[] msg = Encoding.UTF8.GetBytes("OK");
            udpServer.Send(msg, msg.Length, RemoteIpEndPoint);

            Thread.Sleep(1000);

            //Thread.CurrentThread.Abort();

            tdata = new Thread(new ThreadStart(start));
            tdata.Start();
        }
        
        public void start()
        {
            tRec.Abort();
            tRec.Join(100);
            //receiveBytes = receivingUdpClient.Receive(ref RemoteIpEndPoint);
            //BitmapImage imgsource = new BitmapImage();
            //foreach(udpClient.Receive(ref RemoteIpEndPoint).)
            //udpClient.Receive(ref RemoteIpEndPoint).GetValue;
            FileStream fs;
            int coun = 0;
            while (true)
            {

                try
                {
                    MemoryStream memoryStream = new MemoryStream();
                    receiveBytes = udpClient.Receive(ref ipEndPoint);
                    memoryStream.Write(receiveBytes, 2, receiveBytes.Length - 2);

                    //ipEndPoint.Address = RemoteIpEndPoint.Address;

                    int countMsg = receiveBytes[0] - 1;
                    if (countMsg > 25)
                    {
                        throw new Exception("Потеря первого пакета");
                    }
                    for (int i = 0; i < countMsg; i++)
                    {
                        byte[] bt = udpClient.Receive(ref ipEndPoint);
                        memoryStream.Write(bt, 0, bt.Length);
                    }
                    
                    //coun++;

                    //fs = new FileStream("temp"+coun+".png", FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
                    //fs.Write(memoryStream.ToArray(), 0, memoryStream.ToArray().Length);
                    coun++;
                    
                    Dispatcher.BeginInvoke(new ThreadStart(delegate {
                         ConvertToTexture2D(memoryStream.ToArray());
                    }));
                    //Dispatcher.DisableProcessing();

                    //fs.Close();
                    memoryStream.Close();
                    //udpClient.Close();
                }
                catch (Exception ex)
                {
                    countErorr++;
                    Console.WriteLine("Возникло исключение: " + ex.ToString() + "\n  " + ex.Message);
                }
            }
        }

        private void ConvertToTexture2D(byte[] bytes)
        {
            //MemoryStream memoryStream = new MemoryStream(bytes);

            BitmapImage imgsource = new BitmapImage();

            using (MemoryStream memoryStream = new MemoryStream(bytes))
            {
                try
                {

                    imgsource.BeginInit();
                    imgsource.CacheOption = BitmapCacheOption.OnLoad;
                    imgsource.StreamSource = memoryStream;
                    imgsource.EndInit();
                    memoryStream.Close();
                    img.Source = imgsource;

                }
                catch (Exception ex)
                {
                    Console.WriteLine("Возникло исключение: " + ex.ToString() + "\n  " + ex.Message);
                }
            }
        }

        private void powerdown_Click(object sender, RoutedEventArgs e)
        {
            try {
                //UdpClient sendcomand = new UdpClient();

                //IPEndPoint resipEndPoint = new IPEndPoint(IPAddress.Parse(RemoteIpEndPoint.ToString()), 34001);

                byte[] bytes = Encoding.UTF8.GetBytes("Powerdown");

                if (key)//RemoteIpEndPoint.Address.ToString() != null || RemoteIpEndPoint.Address.ToString() != "")
                {
                    // Отправляем данные
                    udpServer.Send(bytes, bytes.Length, RemoteIpEndPoint);
                }
                else
                {
                    MessageBox.Show("Требуется ввести имя", "Ошибка при вводе имени", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {

            }

}

        private void reset_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //UdpClient sendcomand = new UdpClient();

                //IPEndPoint resipEndPoint = new IPEndPoint(IPAddress.Parse(RemoteIpEndPoint.ToString()), 34001);

                byte[] bytes = Encoding.UTF8.GetBytes("Reset");

                if (key)//RemoteIpEndPoint.Address.ToString() != null || RemoteIpEndPoint.Address.ToString() != "")
                {
                    // Отправляем данные
                    udpServer.Send(bytes, bytes.Length, RemoteIpEndPoint);
                }
                else
                {
                    MessageBox.Show("Требуется ввести имя", "Ошибка при вводе имени", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {

            }

        }

        private void sleep_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //UdpClient sendcomand = new UdpClient();

                //IPEndPoint resipEndPoint = new IPEndPoint(IPAddress.Parse(RemoteIpEndPoint.ToString()), 34001);

                byte[] bytes = Encoding.UTF8.GetBytes("LogOff");

                if (key)//RemoteIpEndPoint.Address.ToString() != null || RemoteIpEndPoint.Address.ToString() != "")
                {
                    // Отправляем данные
                    udpServer.Send(bytes, bytes.Length, RemoteIpEndPoint);
                }
                else
                {
                    MessageBox.Show("Требуется ввести имя", "Ошибка при вводе имени", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch(Exception ex)
            {

            }



        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                tdata.Abort();
                tdata.Join(10);

                udpClient.Close();
                udpServer.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Возникло исключение: " + ex.ToString() + "\n  " + ex.Message);
            }
        }
    }


}
