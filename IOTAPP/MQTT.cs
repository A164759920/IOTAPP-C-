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
namespace IOTAPP
{

   
    public class MQTT_Client 
    {
        public delegate void MessageWatcher(string Topic, string QoS, string text);
        public  event MessageWatcher OnMessageEvent;
        public string pid { get; set; }
        public string deviceName { get; set; }
        public static MqttClient mqttClient = null;
        public int port { get; set; }
        public string serverUrl { get; set; }
        public string userId { get; set; }
        public string password { get; set; }
        // 使用创建实例时的传入的参数进行配置
        public MQTT_Client(string Pid,string DeviceName, int Port, string ServerUrl, string UserId, string Password)
        {
            pid = Pid;
            deviceName = DeviceName;
            port = Port;
            serverUrl = ServerUrl;
            userId = UserId;
            password = Password;
        }
        public string[] SubsribeList = null;


        public  void Connet()
        {
          //  Console.WriteLine("Work >>Begin");
            // 需订阅的topic
            string[] tempList = {
                $"$sys/{pid}/{deviceName}/dp/post/json/+",
            $"$sys/{pid}/{deviceName}/image/get/accepted",
            $"$sys/{pid}/{deviceName}/image/get/rejected",
            $"$sys/{pid}/{deviceName}/image/update/accepted",
            $"$sys/{pid}/{deviceName}/image/update/rejected",
            $"$sys/{pid}/{deviceName}/image/update/delta"
            };
            SubsribeList = tempList;
            try
            {
                var factory = new MqttFactory();　　　　　　　　//声明一个MQTT客户端的标准步骤 的第一步
                mqttClient = factory.CreateMqttClient() as MqttClient;  //factory.CreateMqttClient()实际是一个接口类型（IMqttClient）,这里是把他的类型变了一下
                var options = new MqttClientOptionsBuilder()　　　　//实例化一个MqttClientOptionsBulider
                    .WithTcpServer(serverUrl, port)
                    .WithCredentials(userId, password)
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
           // Console.WriteLine("Work >>End");
        }
        public  void Disconnect()
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
         * @description 获取ONENET设备镜像状态
         */
        public async  void GetState()
        {
            var message = new MqttApplicationMessageBuilder()
                                .WithTopic($"$sys/{pid}/{deviceName}/image/get")
                                .WithPayload("")
                                .Build();
            await mqttClient.PublishAsync(message, CancellationToken.None);
        }

        /*
         *  @description 上报模拟传感器数据
         */
        public async  void Data_Publish(string payload)
        {
            var message = new MqttApplicationMessageBuilder()
                            .WithTopic($"$sys/{pid}/{deviceName}/dp/post/json")
                            .WithPayload(payload)
                            .Build();
            await mqttClient.PublishAsync(message,CancellationToken.None);
        }

        /*
         * @description 上报设备状态数据 
         */
        public async  void State_Publish(string payload)
        {
            var message = new MqttApplicationMessageBuilder()
                .WithTopic($"$sys/{pid}/{deviceName}/image/update")
                .WithPayload(payload)
                .Build();
            await mqttClient.PublishAsync(message, CancellationToken.None);
        }

        /*
         * @description 连接成功触发 
         */
        public async  Task Connected(MqttClientConnectedEventArgs e)
        {
            // 循环SubsribeList订阅所有Topic
            for (int i=0; i< SubsribeList.Length; i++)
            {
                await mqttClient.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic(SubsribeList[i]).Build());
            }
            Console.WriteLine("### SUBSCRIBED ###");
        }

        /*
         * @description 断开连接触发 
         */
        public async Task Disconnected(MqttClientDisconnectedEventArgs e)
        {
            Console.WriteLine("### DISCONNECTED ###");
        }

        /*
         * @description 接收消息触发
         */
        public  void MqttApplicationMessageReceived(MqttApplicationMessageReceivedEventArgs  e)
        {
            try
            {
                string text = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
                string Topic = e.ApplicationMessage.Topic;
                string QoS = e.ApplicationMessage.QualityOfServiceLevel.ToString();
                string Retained = e.ApplicationMessage.Retain.ToString();
                OnMessageEvent(Topic, QoS, text);
                // MessageReceiveController(Topic, QoS, text);
            }
            catch(Exception exp)
            {
                Console.WriteLine(exp.Message);
            }
        }
    }
}
