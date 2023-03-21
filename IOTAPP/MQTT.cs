using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Disconnecting;
using MQTTnet.Client.Receiving;
namespace MQTT_API
{


    public static class MQTT_Client
    {
        private static string pid = "583419";
        private static string deviceName = "mqtt-can1";
        private static MqttClient mqttClient = null;
        private static string ServerUrl = "mqtts.heclouds.com";
        private static int Port = 1883;
        private static string UserId = "583419";
        private static string Password = "";
        private static string[] SubsribeList = {
            "$sys/583419/mqtt-can1/dp/post/json/+",
            "$sys/583419/mqtt-can1/image/get/accepted",
            "$sys/583419/mqtt-can1/image/get/rejected",

            };
        public static JArray createDpItem(int Vdata)
        {
            JObject vObject = new JObject();
            vObject.Add("v", Vdata);
            JArray vArray = new JArray();
            vArray.Add(vObject);
            return vArray;
        }


        public static void Connet()
        {
            Console.WriteLine("Work >>Begin");
            try
            {
                var factory = new MqttFactory();　　　　　　　　//声明一个MQTT客户端的标准步骤 的第一步
                mqttClient = factory.CreateMqttClient() as MqttClient;  //factory.CreateMqttClient()实际是一个接口类型（IMqttClient）,这里是把他的类型变了一下
                var options = new MqttClientOptionsBuilder()　　　　//实例化一个MqttClientOptionsBulider
                    .WithTcpServer(ServerUrl, Port)
                    .WithCredentials(UserId, Password)
                    .WithClientId("mqtt-can1")
                    .Build();
                mqttClient.ConnectAsync(options);      //连接服务器
                // 定义状态Handler
                mqttClient.ConnectedHandler = new MqttClientConnectedHandlerDelegate(new Func<MqttClientConnectedEventArgs, Task>(Connected));
                mqttClient.DisconnectedHandler = new MqttClientDisconnectedHandlerDelegate(new Func<MqttClientDisconnectedEventArgs, Task>(Disconnected));
                mqttClient.ApplicationMessageReceivedHandler = new MqttApplicationMessageReceivedHandlerDelegate(new Action<MqttApplicationMessageReceivedEventArgs>(MqttApplicationMessageReceived));

            }
            catch (Exception exp)
            {

                Console.WriteLine(exp);
            }
            Console.WriteLine("Work >>End");
        }
        public static void Disconnect()
        {
            if(mqttClient != null)
            {
                try
                {
                    mqttClient.DisconnectAsync();
                }
                catch(Exception exp)
                {
                    Console.WriteLine("Disconnect Function" + exp);
                }
            }
        }
        /*
         * 获取ONENET设备镜像状态
         */
        public async static void GetStatus()
        {
            var message = new MqttApplicationMessageBuilder()
                                .WithTopic($"$sys/{pid}/{deviceName}/image/get")
                                .WithPayload("")
                                .Build();
            await mqttClient.PublishAsync(message, CancellationToken.None);
        }

        // 上报模拟传感器数据
        public async static void Data_Publish(String payload)
        {
            var message = new MqttApplicationMessageBuilder()
                            .WithTopic($"$sys/{pid}/{deviceName}/dp/post/json")
                            .WithPayload(payload)
                            .Build();
            await mqttClient.PublishAsync(message,CancellationToken.None);
        }

        // 连接成功触发
        private static async Task Connected(MqttClientConnectedEventArgs e)
        {
            // 循环SubsribeList订阅所有Topic
            for (int i=0; i< SubsribeList.Length; i++)
            {
                await mqttClient.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic(SubsribeList[i]).Build());
            }
            Console.WriteLine("### SUBSCRIBED ###");
        }

        // 断开连接触发
        private static async Task Disconnected(MqttClientDisconnectedEventArgs e)
        {
            Console.WriteLine("### DISCONNECTED ###");
        }

        // 接收消息触发
        private static void MqttApplicationMessageReceived(MqttApplicationMessageReceivedEventArgs  e)
        {
            /*
            Console.WriteLine("### RECEIVED APPLICATION MESSAGE ###");
            Console.WriteLine($"+ Topic = {e.ApplicationMessage.Topic}");
            Console.WriteLine($"+ Payload = {Encoding.UTF8.GetString(e.ApplicationMessage.Payload)}");
            Console.WriteLine($"+ QoS = {e.ApplicationMessage.QualityOfServiceLevel}");
            Console.WriteLine($"+ Retain = {e.ApplicationMessage.Retain}");
            Console.WriteLine();
             */
            try
            {
                string text = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
                string Topic = e.ApplicationMessage.Topic; string QoS = e.ApplicationMessage.QualityOfServiceLevel.ToString();
                string Retained = e.ApplicationMessage.Retain.ToString();
                Console.WriteLine("MessageReceived >>Topic:" + Topic + "; QoS: " + QoS + "; Retained: " + Retained + ";");
                Console.WriteLine("MessageReceived >>Msg: " + text);

            }
            catch(Exception exp)
            {
                Console.WriteLine(exp.Message);
            }
        }
    }
}
