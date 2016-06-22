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

        UdpClient udpClient = new UdpClient(34000);
        IPEndPoint RemoteIpEndPoint = null;
        IPEndPoint ipEndPoint;
        byte[] receiveBytes = new byte[0];

        //IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Parse(ip), 34001);


        private static FileStream fs;

        int countErorr = 0;

        public MainWindow()
        {

            InitializeComponent();

            Thread tRec = new Thread(new ThreadStart(start));
            tRec.Start();

           

        }
        
        public void start()
        {



            //receiveBytes = receivingUdpClient.Receive(ref RemoteIpEndPoint);
            //BitmapImage imgsource = new BitmapImage();
            while (true)
            {

                try
                {
                    MemoryStream memoryStream = new MemoryStream();
                    receiveBytes = udpClient.Receive(ref RemoteIpEndPoint);
                    memoryStream.Write(receiveBytes, 2, receiveBytes.Length - 2);

                    //ipEndPoint.Address = RemoteIpEndPoint.Address;

                    int countMsg = receiveBytes[0] - 1;
                    if (countMsg > 10)
                    {
                        throw new Exception("Потеря первого пакета");
                    }
                    for (int i = 0; i < countMsg; i++)
                    {
                        byte[] bt = udpClient.Receive(ref RemoteIpEndPoint);
                        memoryStream.Write(bt, 0, bt.Length);
                    }

                    //ConvertToTexture2D(memoryStream.ToArray());
                    fs = new FileStream("temp.png", FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
                    fs.Write(memoryStream.ToArray(), 0, memoryStream.ToArray().Length);


                    /*imgsource.BeginInit();
                    imgsource.StreamSource = memoryStream;
                    imgsource.EndInit();
                    //imgsource.ClearValue()
                    //System.Drawing.Image.FromStream(
                    //memoryStream.Close();
                    img.Source = imgsource;*/

                    //img.Source = BitmapFrame.Create(memoryStream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);



                    
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

            //return imgsource;

            /*BitmapImage bitmapImage = new BitmapImage();
            using (var mem = new MemoryStream(bytes))
            {
                bitmapImage.BeginInit();
                bitmapImage.CrateOptions = BitmapCreateOptions.PreservePixelFormat;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.StreamSource = mem;
                bitmapImage.EndInit();
                return bitmapImage;
            }*/

            //return imgsource;


            //const string ImagePath = @"C:\";

            /*MemoryStream memoryStream = new MemoryStream(bytes);
            try
            {
                /*System.Drawing.Bitmap bmp = (System.Drawing.Bitmap)System.Drawing.Bitmap.FromStream(memoryStream);
                memoryStream = new MemoryStream();
                string fileOut = System.IO.Path.Combine(ImagePath, "quality_" + ".png");
                FileStream ms = new FileStream(fileOut, FileMode.Create, FileAccess.Write);
                bmp.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
                ms.Flush();
                ms.Close();*

                System.Drawing.Bitmap bmp = (System.Drawing.Bitmap)System.Drawing.Bitmap.FromStream(memoryStream);
                memoryStream = new MemoryStream();



                bmp.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Jpeg);

                Bitmap bm = new Bitmap(memoryStream);

                IntPtr bmpPt = bm.GetHbitmap();
                BitmapSource bitmapSource =
                 System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                       bmpPt,
                       IntPtr.Zero,
                       Int32Rect.Empty,
                       BitmapSizeOptions.FromEmptyOptions());
                bitmapSource.Freeze();
                //DeleteObject(bmpPt);

                img.Source = bitmapSource;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Возникло исключение: " + ex.ToString() + "\n  " + ex.Message);
            }

            /*try
            {
                var bitmap = new Bitmap(System.Drawing.Image.FromStream(memoryStream));
                IntPtr bmpPt = bitmap.GetHbitmap();
                BitmapSource bitmapSource =
                 System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                       bmpPt,
                       IntPtr.Zero,
                       Int32Rect.Empty,
                       BitmapSizeOptions.FromEmptyOptions());
                bitmapSource.Freeze();
                //DeleteObject(bmpPt);

                img.Source = bitmapSource;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Возникло исключение: " + ex.ToString() + "\n  " + ex.Message);
            }
            /*freeze bitmapSource and clear memory to avoid memory leaks
            bitmapSource.Freeze();
            //DeleteObject(bmpPt);

            img.Source = bitmapSource;*/

            //GetImageStream(System.Drawing.Image.FromStream(memoryStream));
        }


        public void GetImageStream(System.Drawing.Image myImage)
        {
            var bitmap = new Bitmap(myImage);
            IntPtr bmpPt = bitmap.GetHbitmap();
            BitmapSource bitmapSource =
             System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                   bmpPt,
                   IntPtr.Zero,
                   Int32Rect.Empty,
                   BitmapSizeOptions.FromEmptyOptions());

            //freeze bitmapSource and clear memory to avoid memory leaks
            bitmapSource.Freeze();
            //DeleteObject(bmpPt);

            img.Source = bitmapSource;

            //return bitmapSource;
        }

        /*protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            if (BackGround != null)
            {
                spriteBatch.Begin();
                spriteBatch.Draw(BackGround,
                    new Rectangle(0, 0, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight),
                    new Rectangle(0, 0, BackGround.Width, BackGround.Height),
                    Color.White);
                spriteBatch.End();
                this.Window.Title = "Потеряно пакетов: " + countErorr.ToString();
            }

            base.Draw(gameTime);
        }*

        int countErorr = 0;
        private void AsyncReceiver()
        {
            IPEndPoint ep = new IPEndPoint(IPAddress.Loopback, 0);

            while (true)
            {
                try
                {
                    MemoryStream memoryStream = new MemoryStream();
                    byte[] bytes = udpClient.Receive(ref ep);
                    memoryStream.Write(bytes, 2, bytes.Length - 2);

                    int countMsg = bytes[0] - 1;
                    if (countMsg > 10)
                        throw new Exception("Потеря первого пакета");
                    for (int i = 0; i < countMsg; i++)
                    {
                        byte[] bt = udpClient.Receive(ref ep);
                        memoryStream.Write(bt, 0, bt.Length);
                    }

                    ConvertToTexture2D(memoryStream.ToArray());
                    memoryStream.Close();
                }
                catch (Exception ex)
                {
                    countErorr++;
                }
            }
        }

        /*private void Receive_GetData(byte[] Date)
        {
            BackGround = ConvertToTexture2D(Date);
        }*/

        private void ConvertToTexture2D1(byte[] bytes)
        {
            //const string ImagePath = @"C:\";

            MemoryStream memoryStream = new MemoryStream(bytes);
            try
            {
                /*System.Drawing.Bitmap bmp = (System.Drawing.Bitmap)System.Drawing.Bitmap.FromStream(memoryStream);
                memoryStream = new MemoryStream();
                string fileOut = System.IO.Path.Combine(ImagePath, "quality_" + ".png");
                FileStream ms = new FileStream(fileOut, FileMode.Create, FileAccess.Write);
                bmp.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
                ms.Flush();
                ms.Close();*/

                System.Drawing.Bitmap bmp = (System.Drawing.Bitmap)System.Drawing.Bitmap.FromStream(memoryStream);
                memoryStream = new MemoryStream();

                

                bmp.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
            }
            catch (Exception ex)
            {
            }

            try
            {
                var bitmap = new Bitmap(System.Drawing.Image.FromStream(memoryStream));
                IntPtr bmpPt = bitmap.GetHbitmap();
                BitmapSource bitmapSource =
                 System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                       bmpPt,
                       IntPtr.Zero,
                       Int32Rect.Empty,
                       BitmapSizeOptions.FromEmptyOptions());
                bitmapSource.Freeze();
                //DeleteObject(bmpPt);

                img.Source = bitmapSource;
            }
            catch (Exception ex)
            {

            }
            /*freeze bitmapSource and clear memory to avoid memory leaks
            bitmapSource.Freeze();
            //DeleteObject(bmpPt);

            img.Source = bitmapSource;*/

            //GetImageStream(System.Drawing.Image.FromStream(memoryStream));
        }

        private void powerdown_Click(object sender, RoutedEventArgs e)
        {
            UdpClient sendcomand = new UdpClient();
            string ip = RemoteIpEndPoint.ToString();

            ipEndPoint = new IPEndPoint(IPAddress.Parse(ip), 34001);

            byte[] bytes = Encoding.UTF8.GetBytes("Powerdown");

            if (ipEndPoint.Address.ToString() != null || ipEndPoint.Address.ToString() != "")
            {
                // Отправляем данные
                sendcomand.Send(bytes, bytes.Length, ipEndPoint);
            }
            else
            {
                MessageBox.Show("Требуется ввести имя", "Ошибка при вводе имени", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        private void reset_Click(object sender, RoutedEventArgs e)
        {
            UdpClient sendcomand = new UdpClient();

            string ip = RemoteIpEndPoint.ToString();

            ipEndPoint = new IPEndPoint(IPAddress.Parse(ip), 34001);

            byte[] bytes = Encoding.UTF8.GetBytes("Reset");

            if (ipEndPoint.Address.ToString() != null || ipEndPoint.Address.ToString() != "")
            {
                // Отправляем данные
                sendcomand.Send(bytes, bytes.Length, ipEndPoint);
            }
            else
            {
                MessageBox.Show("Требуется ввести имя", "Ошибка при вводе имени", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        private void sleep_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                UdpClient sendcomand = new UdpClient();

                string ip = RemoteIpEndPoint.ToString();

                ipEndPoint = new IPEndPoint(IPAddress.Parse(ip), 34001);

                byte[] bytes = Encoding.UTF8.GetBytes("LogOff");

                if (ipEndPoint.Address.ToString() != null || ipEndPoint.Address.ToString() != "")
                {
                    // Отправляем данные
                    sendcomand.Send(bytes, bytes.Length, ipEndPoint);
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
    }



   /* public class Receive
    {
        //private GraphicsDeviceManager graphics;
       // private SpriteBatch spriteBatch;
        private System.Drawing.Image BackGround;
        private UdpClient udpClient;

        private delegate void NetEvent(byte[] Date);
        private delegate void AsyncWork();
        private event NetEvent GetData;

        public Receive()
        {
            //graphics = new GraphicsDeviceManager(this);
            udpClient = new UdpClient(34000);
            GetData += new NetEvent(Receive_GetData);
            new AsyncWork(AsyncReceiver).BeginInvoke(null, null);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            if (BackGround != null)
            {
                spriteBatch.Begin();
                spriteBatch.Draw(BackGround,
                    new Rectangle(0, 0, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight),
                    new Rectangle(0, 0, BackGround.Width, BackGround.Height),
                    Color.White);
                spriteBatch.End();
                this.Window.Title = "Потеряно пакетов: " + countErorr.ToString();
            }

            base.Draw(gameTime);
        }

        int countErorr = 0;
        private void AsyncReceiver()
        {
            IPEndPoint ep = new IPEndPoint(IPAddress.Loopback, 0);

            while (true)
            {
                try
                {
                    MemoryStream memoryStream = new MemoryStream();
                    byte[] bytes = udpClient.Receive(ref ep);
                    memoryStream.Write(bytes, 2, bytes.Length - 2);

                    int countMsg = bytes[0] - 1;
                    if (countMsg > 10)
                        throw new Exception("Потеря первого пакета");
                    for (int i = 0; i < countMsg; i++)
                    {
                        byte[] bt = udpClient.Receive(ref ep);
                        memoryStream.Write(bt, 0, bt.Length);
                    }

                    GetData(memoryStream.ToArray());
                    memoryStream.Close();
                }
                catch (Exception ex)
                {
                    countErorr++;
                }
            }
        }

        private void Receive_GetData(byte[] Date)
        {
            BackGround = ConvertToTexture2D(Date);
        }

        private Texture2D ConvertToTexture2D(byte[] bytes)
        {
            MemoryStream memoryStream = new MemoryStream(bytes);
            try
            {
                System.Drawing.Bitmap bmp = (System.Drawing.Bitmap)System.Drawing.Bitmap.FromStream(memoryStream);
                memoryStream = new MemoryStream();
                bmp.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
            }
            catch (Exception ex)
            {
            }
            return Texture2D.FromStream(GraphicsDevice, memoryStream);
        }
    }
    */


}
