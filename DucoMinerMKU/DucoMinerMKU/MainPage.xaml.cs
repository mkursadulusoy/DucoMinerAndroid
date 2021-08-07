using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace DucoMinerMKU
{
    public partial class MainPage : ContentPage
    {

        public class poollar
        {
            public string name { get; set; }
            public string ip { get; set; }
            public string port { get; set; }
            public int connections { get; set; }
        }
        public MainPage()
        {
            InitializeComponent();

        }

      
        void Handle_Clicked(object sender, System.EventArgs e)
        {
            Stopwatch stopWatch = new Stopwatch();
            altyazi.Text = "server.duinocoin.com";
            string serverip = "server.duinocoin.com";
            int serverport = 2814;
            var nameValue = kullaniciAdi.Text;
            var json = new WebClient().DownloadString("https://server.duinocoin.com/getPool");
            altyazi.Text = json;
            poollar poollistesi = JsonSerializer.Deserialize<poollar>(json);
            serverip = poollistesi.ip;
            serverport = Convert.ToInt32(poollistesi.port);
            Socket s = new Socket(AddressFamily.InterNetwork,
           SocketType.Stream,
           ProtocolType.Tcp);
            altyazi.Text = "Sunucuya bağlantı sağlanıyor.";
            s.Connect(serverip, serverport);
            altyazi.Text = "Bağlantı Sağlandı";

            while (true)
            {
                byte[] b = new byte[90];
                int k = s.Receive(b);
                string szReceived = Encoding.ASCII.GetString(b, 0, k);
                Console.Write("The answer from server:");
                Console.WriteLine(Convert.ToString(szReceived));

                if (szReceived.Length > 0)
                {
                    if (Convert.ToString(szReceived[0]) == "2")
                    {
                        Console.Write("Current Server Version:");
                        Console.WriteLine(szReceived);
                        byte[] byData = System.Text.Encoding.ASCII.GetBytes("JOB," + nameValue + ",LOW");
                        Console.WriteLine(byData);
                        s.Send(byData);


                    }
                    else if (szReceived.Substring(0, 4) == "GOOD")
                    {
                        Console.WriteLine("İş Doğru Şekilde Teslim Edildi");
                        Console.WriteLine("Yeni iş İsteniyor");
                        byte[] byData = System.Text.Encoding.ASCII.GetBytes("JOB," + nameValue + ",LOW");
                        s.Send(byData);

                    }
                    else if (szReceived.Substring(0, 3) == "BAD")
                    {
                        Console.WriteLine("İş Doğru Şekilde Teslim Edilemedi");
                        Console.WriteLine("Yeni iş İsteniyor");
                        byte[] byData = System.Text.Encoding.ASCII.GetBytes("JOB," + nameValue + ",LOW");
                        s.Send(byData);

                    }
                    else
                    {
                        Console.WriteLine("Yeni İş Kabul Edildi");
                        Console.WriteLine(szReceived);
                        //işi parçalara ayırıp zorluğu seçiyoruz
                        string[] is_parcalari = szReceived.Split(',');
                        int difficulty = Convert.ToInt32(is_parcalari[2]);
                        stopWatch.Start();
                        for (int result = 0; result < 100 * difficulty + 1; result++)
                        {
                            var data = Encoding.ASCII.GetBytes(is_parcalari[0] + result);
                            var hash = new SHA1Managed().ComputeHash(data);
                            var shash = string.Empty;
                            foreach (var ba in hash)
                            {
                                shash += ba.ToString("x2");
                            }


                            if (is_parcalari[1] == shash)
                            {
                                Console.WriteLine("Hash Çözüldü");
                                stopWatch.Stop();
                                decimal zaman = stopWatch.ElapsedMilliseconds / 1000;
                                if (zaman == 0) zaman = 0.00000000000000000001M;
                                var calchashrate = decimal.Round((result / zaman), 2, MidpointRounding.AwayFromZero);
                                Console.Write("Yapılan işe ait Hash değeri");
                                Console.WriteLine(calchashrate);
                                Console.WriteLine("Cevap sunucuya gönderiliyor.");
                                byte[] byData = System.Text.Encoding.ASCII.GetBytes(result + "," + calchashrate + ",C# Duino Miner by mkursadulusoy," + "C# Miner");
                                Console.WriteLine(byData);
                                s.Send(byData);

                                break;
                            }



                        }


                    }

                }
                else
                {
                    Console.WriteLine("Düzgün cevap alınamadı. Tekrar deneniyor.");
                }


            }

        }
    }
}
