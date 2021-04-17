using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Razer.Chroma.Broadcast;
using RGBKit.Core;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace MQTTConnect
{
    public class Worker : BackgroundService
    {
        static string MQTChroma_Topic = "MQTTConnect";
        static string MQTChroma_Broker = "";
        static int MQTChroma_port = int.Parse("1883");
        static string MQTChroma_Username = "";
        static string MQTChroma_Password = "";
        static MqttClient client = new MqttClient(MQTChroma_Broker, MQTChroma_port, false, MqttSslProtocols.None, null, null);
        private readonly ILogger<Worker> _logger;
        private readonly IConfiguration _configuration;
        private readonly IRGBKitService _rgbKit;
        private readonly bool _performanceMetricsEnabled;
        private readonly Stopwatch _performanceMetricsStopwatch;
        public Worker(ILogger<Worker> logger, IConfiguration configuration, IRGBKitService rgbKit)
        {
            _logger = logger;
            _configuration = configuration;
            _rgbKit = rgbKit;
            _performanceMetricsEnabled = (bool)_configuration.GetValue(typeof(bool), "PerformanceMetricsEnabled");
            _performanceMetricsStopwatch = new Stopwatch();
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        { }
            public static void API_Init() 
        {
            RzChromaBroadcastAPI _api;
            _api = new RzChromaBroadcastAPI();
            _api.ConnectionChanged += Api_ConnectionChanged;
            _api.ColorChanged += Api_ColorChanged;
            _api.Init(Guid.Parse("e6bef332-95b8-76ec-a6d0-9f402bad244c"));
        }
        public static void MQTT_Init()
        {
            try
            {
                client.ProtocolVersion = MqttProtocolVersion.Version_3_1;
                byte code = client.Connect(Guid.NewGuid().ToString(), MQTChroma_Username, MQTChroma_Password);
                if (code == 0)
                {
                    client.Subscribe(new string[] { MQTChroma_Topic + "/RGB", MQTChroma_Topic + "/A", MQTChroma_Topic + "/status"}, new byte[] { 0, 0, 0});
                    client.Publish(MQTChroma_Topic + "/status", Encoding.UTF8.GetBytes("ON"));                    
                }
                else Console.WriteLine("MQTT Connect Fail");
            }

            catch (Exception)
            {
                Console.WriteLine("MQTT Connect Fail");
            }
        }
        public static void MQTT_Publish(MqttClient Mclient, string Topic,int R,int G,int B,int A)
        {
            Mclient.Publish(Topic + "/RGB", Encoding.UTF8.GetBytes(R+","+G+","+B));
            Mclient.Publish(Topic + "/A", Encoding.UTF8.GetBytes(""+ A));
        }
        private static void Api_ConnectionChanged(object sender, RzChromaBroadcastConnectionChangedEventArgs e)
        {
            Console.WriteLine("Razer Chroma Broadcast API connected");
            Worker.MQTT_Init();
        }
        private static void Api_ColorChanged(object sender, RzChromaBroadcastColorChangedEventArgs e)
        {     
            string str = ""+ e.Colors[0];
            str = str.Substring(7);
            str = str.Remove(str.Length - 1, 1);            
            string[] ARGB = str.Split(',');
            int A = Int32.Parse(ARGB[0].Substring(2));
            int R = Int32.Parse(ARGB[1].Substring(3));
            int G = Int32.Parse(ARGB[2].Substring(3));
            int B = Int32.Parse(ARGB[3].Substring(3));
            MQTT_Publish(client, MQTChroma_Topic, R, G, B, A);
        }
    }
}
