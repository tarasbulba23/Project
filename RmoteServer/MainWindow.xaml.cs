using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Media.Imaging;

namespace RmoteServer
{
    /// <summary>
    /// Logic for interaction MainWindow.xaml
    /// </summary>
    /// <permission>public</permission>
    /// <remarks>Inheritance of the class Window</remarks>
    public partial class MainWindow : Window
    {

        private UdpClient udpClient = new UdpClient(34000);
        private IPEndPoint ipEndPoint = null;

        private UdpClient udpServer = new UdpClient();
        private IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 34001);

        private byte[] receiveBytes = new byte[0];

        private bool key = false;

        Thread tRec, tdata;

        int countErorr = 0;

        /// <summary>
        /// Starting point of the program
        /// </summary>
        /// <permission>public</permission>
        /// <returns>Void</returns>
        public MainWindow()
        {
            InitializeComponent();

            myip.Content = Dns.GetHostByName(Dns.GetHostName()).AddressList[0].ToString();

            tRec = new Thread(new ThreadStart(sendOk));
            tRec.Start();
        }


        /// <summary>
        /// Resived message from the client, and if correctness message sends a message about the successful delivery
        /// </summary>
        /// <permission>public</permission>
        /// <remarks>Launching a function 'start' in a new thread</remarks>
        /// <param >No required params</param>
        /// <returns>Void</returns>
        public void sendOk()
        {
            bool b = true;
            IPAddress address;
            
            while (b)
            {
                byte[] data = udpClient.Receive(ref ipEndPoint);
                string returnData = Encoding.UTF8.GetString(data);

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

            byte[] msg = Encoding.UTF8.GetBytes("OK");
            udpServer.Send(msg, msg.Length, RemoteIpEndPoint);

            Thread.Sleep(2000);

            tdata = new Thread(new ThreadStart(start));
            tdata.Start();
        }

        /// <summary>
        /// This function takes packets and writes in memory stream
        /// </summary>
        /// <exception>All posible exception, and write error in console</exception>
        /// <permission>public</permission>
        /// <remarks>Stops Thread - tRec, that means stop function 'sendOk'</remarks>
        /// <param>No required params</param>
        /// <returns>Void</returns>
        public void start()
        {
            tRec.Abort();
            tRec.Join(100);

            while (true)
            {

                try
                {
                    MemoryStream memoryStream = new MemoryStream();
                    receiveBytes = udpClient.Receive(ref ipEndPoint);
                    memoryStream.Write(receiveBytes, 2, receiveBytes.Length - 2);

                    int countMsg = receiveBytes[0] - 1;
                    if (countMsg > 25)
                    {
                        throw new Exception("Lost first package");
                    }
                    for (int i = 0; i < countMsg; i++)
                    {
                        byte[] bt = udpClient.Receive(ref ipEndPoint);
                        memoryStream.Write(bt, 0, bt.Length);
                    }
                                        
                    Dispatcher.BeginInvoke(new ThreadStart(delegate {
                         ConvertToTexture2D(memoryStream.ToArray());
                    }));

                    memoryStream.Close();
                }
                catch (Exception ex)
                {
                    countErorr++;
                    Console.WriteLine("Fatal Error: " + ex.ToString() + "\n  " + ex.Message);
                }
            }
        }

        /// <summary>
        /// Convert byte array to image, and displays that image
        /// </summary>
        /// <exception>All posible exception, and write error in console</exception>
        /// <permission>Private</permission>
        /// <remarks>Works in asynchronous mode</remarks>
        /// <param name="bytes">Image as a byte array</param>
        /// <returns>Void</returns>
        private void ConvertToTexture2D(byte[] bytes)
        {
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
                    Console.WriteLine("Fatal Error: " + ex.ToString() + "\n  " + ex.Message);
                }
            }
        }

        /// <summary>
        /// When you click on button "Powerdown", sent a message to client program
        /// </summary>
        /// <exception>All posible exception</exception>
        /// <permission>Private</permission>
        /// <param name="sender">Default value</param>
        /// <param name="e">Default value</param>
        /// <returns>Void</returns>
        private void powerdown_Click(object sender, RoutedEventArgs e)
        {
            try {
                byte[] bytes = Encoding.UTF8.GetBytes("Powerdown");

                if (key)
                {
                    udpServer.Send(bytes, bytes.Length, RemoteIpEndPoint);
                }
                else
                {
                    MessageBox.Show("Don`t have connection", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {

            }
        }

        /// <summary>
        /// When you click on button "Restart", sent a message to client program
        /// </summary>
        /// <exception>All posible exception</exception>
        /// <permission>Private</permission>
        /// <param name="sender">Default value</param>
        /// <param name="e">Default value</param>
        /// <returns>Void</returns>
        private void reset_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                byte[] bytes = Encoding.UTF8.GetBytes("Reset");

                if (key)
                {
                    udpServer.Send(bytes, bytes.Length, RemoteIpEndPoint);
                }
                else
                {
                    MessageBox.Show("Don`t have connection", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {

            }

        }

        /// <summary>
        /// When you click on button "LogOff", sent a message to client program
        /// </summary>
        /// <exception>All posible exception</exception>
        /// <permission>Private</permission>
        /// <param name="sender">Default value</param>
        /// <param name="e">Default value</param>
        /// <returns>Void</returns>
        private void sleep_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                byte[] bytes = Encoding.UTF8.GetBytes("LogOff");

                if (key)
                {
                    udpServer.Send(bytes, bytes.Length, RemoteIpEndPoint);
                }
                else
                {
                    MessageBox.Show("Don`t have connection", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch(Exception ex)
            {

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
                tdata.Abort();
                tdata.Join(10);

                udpClient.Close();
                udpServer.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Fatal Error: " + ex.ToString() + "\n  " + ex.Message);
            }
        }
    }


}
