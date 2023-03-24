using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MQTTnet;
namespace IOTAPP
{
   // public delegate void MessageWatcher(string Topic, string QoS, string text);
    public partial class Form1 : Form
    {
       
        public class Device
        {
            public canDevice canInstance = new canDevice();
            public MQTT_Client mqttInstance = new MQTT_Client();
            public void MessageReceiveController(string Topic, string QoS, string text)
            {
                Console.WriteLine("MessageReceived >>Topic:" + Topic + "; QoS: " + QoS);
                Console.WriteLine("MessageReceived >>Msg: " + text);
                switch (Topic)
                {
                    case "$sys/583419/mqtt-can1/dp/post/json/+":
                        Console.WriteLine("传感器数据上报消息成功");
                        break;
                    case "$sys/583419/mqtt-can1/image/get/accepted":
                        // 同步远端镜像状态成功
                        canInstance.Remote_SetState(text);
                        break;
                    case "$sys/583419/mqtt-can1/image/get/rejected":
                        Console.WriteLine("同步状态失败");
                        break;
                    case "$sys/583419/mqtt-can1/image/update/delta":
                        // 检测到状态差值
                        canInstance.Remote_handleDelta(text);
                        break;
                    default:
                        Console.WriteLine("未命中");
                        break;
                }
            }
        }
        public Device can1 = new Device();
        public Form1()
        {
            InitializeComponent();

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Device can1 = new Device();
            can1.mqttInstance.Connet();
            can1.canInstance.initFuzzy();
            can1.mqttInstance.OnMessageEvent += can1.MessageReceiveController;
        }

        private void button1_Click(object sender, EventArgs e)
        {
           
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var Payload = can1.canInstance.createJSONData(123);
            can1.mqttInstance.Data_Publish(Payload);
            //var Payload = canDevice.createJSONData(123);
            //MQTT_Client.Data_Publish(Payload);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            can1.mqttInstance.Disconnect();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            can1.mqttInstance.GetState();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            Console.WriteLine("定位" + can1.canInstance.controlState);
        }

        private void button6_Click(object sender, EventArgs e)
        {
          //   Console.WriteLine("结果" + can1.canInstance.controlTemp(25));
            Console.WriteLine(can1.canInstance.controlPH(7.5));
            Console.WriteLine(can1.canInstance.controlWhisk(40,50));

        }
    }
}
