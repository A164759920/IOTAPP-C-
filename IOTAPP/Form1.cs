using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
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
            // 状态-参数映射 控制器
            public void StateChangeController(string[] stateNames)
            {
                void switchTimerByState(string stateName)
                {
                    switch (stateName)
                    {
                        case "hotState":
                           if( canInstance.hotState == "1")
                            {
                                if(canInstance.hotTimer == null && canInstance.coldTimer ==null)
                                {
                                    void timerMethod(object obj)
                                    {
                                       
                                        canInstance.temperature += 1.0;
                                        Console.WriteLine("当前温度:{0}", canInstance.temperature);
                                    }
                                    canInstance.hotTimer = new System.Threading.Timer(timerMethod,null,0,3000);
                                }
                                else
                                {
                                    Console.WriteLine("加热器已在运行");
                                }
                            }
                            else if(canInstance.hotState == "0")
                            {
                                if(canInstance.hotTimer != null)
                                {
                                    canInstance.hotTimer.Dispose(); // 结束原定时器线程
                                    canInstance.hotTimer = null;
                                }
                            }
                            else
                            {
                                Console.WriteLine("[StateChangeController]加热器错误");
                            }
                            break;
                        case "coldState":
                            if (canInstance.coldState == "1")
                            {
                                if (canInstance.coldTimer == null && canInstance.hotTimer == null)
                                {
                                    void timerMethod(object obj)
                                    {
                                        canInstance.temperature -= 1.0;
                                        Console.WriteLine("当前温度:{0}", canInstance.temperature);
                                    }
                                    canInstance.coldTimer = new System.Threading.Timer(timerMethod, null, 0, 3000);
                                }
                                else
                                {
                                    Console.WriteLine("制冷器已在运行");
                                }
                            }
                            else if (canInstance.coldState == "0")
                            {
                                if (canInstance.coldTimer != null)
                                {
                                    canInstance.coldTimer.Dispose(); // 结束原定时器线程
                                    canInstance.coldTimer = null;
                                }
                            }
                            else
                            {
                                Console.WriteLine("[StateChangeController]制冷器错误");
                            }
                            break;
                        case "acidState":
                            if (canInstance.acidState == "1")
                            {
                                if (canInstance.acidTimer == null)
                                {
                                    void timerMethod(object obj)
                                    {
                                        canInstance.pH -= 0.5;
                                    }
                                    canInstance.acidTimer = new System.Threading.Timer(timerMethod, null, 0, 1000);
                                }
                                else
                                {
                                    Console.WriteLine("加酸器已在运行");
                                }
                            }
                            else if (canInstance.acidState == "0")
                            {
                                if (canInstance.acidTimer != null)
                                {
                                    canInstance.acidTimer.Dispose(); // 结束原定时器线程
                                    canInstance.acidTimer = null;
                                }
                            }
                            else
                            {
                                Console.WriteLine("[StateChangeController]加酸器错误");
                            }
                            break;
                        case "baseState":
                            if (canInstance.baseState == "1")
                            {
                                if (canInstance.baseTimer == null)
                                {
                                    void timerMethod(object obj)
                                    {
                                        canInstance.pH += 0.5;
                                    }
                                    canInstance.baseTimer = new System.Threading.Timer(timerMethod, null, 0, 1000);
                                }
                                else
                                {
                                    Console.WriteLine("加碱器已在运行");
                                }
                            }
                            else if (canInstance.baseState == "0")
                            {
                                if (canInstance.baseTimer != null)
                                {
                                    canInstance.baseTimer.Dispose(); // 结束原定时器线程
                                    canInstance.baseTimer = null;
                                }
                            }
                            else
                            {
                                Console.WriteLine("[StateChangeController]加碱器错误");
                            }
                            break;
                        case "whiskState":
                            if (canInstance.whiskState == "1" )
                            {
                                if (canInstance.whiskTimer == null)
                                {
                                    void timerMethod(object obj)
                                    {
                                        canInstance.temperature -= 1.0;
                                    }
                                    // FIXME:此处逻辑需要重写
                                    canInstance.whiskTimer = new System.Threading.Timer(timerMethod, null, 0, 1000);
                                }
                                else
                                {
                                    Console.WriteLine("搅拌器已在运行");
                                }
                            }
                            else if (canInstance.whiskState == "0")
                            {
                                if (canInstance.whiskTimer != null)
                                {
                                    canInstance.whiskTimer.Dispose(); // 结束原定时器线程
                                    canInstance.whiskTimer = null;
                                }
                            }
                            else
                            {
                                Console.WriteLine("[StateChangeController]搅拌器错误");
                            }
                            break;
                    }
                }
                foreach(string item in stateNames)
                {
                    switchTimerByState(item);
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
            can1.canInstance.InitDevice();
            can1.canInstance.initFuzzy();
            can1.canInstance.EmitStateChangeEvent += can1.StateChangeController;
            System.Threading.Timer openControl = new System.Threading.Timer((state) => {
                can1.canInstance.Local_controlTemp(can1.canInstance.temperature);
            }, null, 0, 800);

        }

        private void button1_Click(object sender, EventArgs e)
        {
            can1.mqttInstance.OnMessageEvent += can1.MessageReceiveController;
            System.Threading.Timer timerpublish = new System.Threading.Timer((state) => {
                var Payload = can1.canInstance.createJSONData(123);
                can1.mqttInstance.Data_Publish(Payload);
            }, null, 0, 2000);
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
            can1.canInstance.InitDevice();
            Console.WriteLine("定位" + can1.canInstance.controlState);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            can1.canInstance.Local_controlTemp(can1.canInstance.temperature);
          //  can1.canInstance.Local_controlPH(can1.canInstance.pH);
          //   Console.WriteLine("结果" + can1.canInstance.controlTemp(25));
            // Console.WriteLine(can1.canInstance.controlPH(7.5));
            // Console.WriteLine(can1.canInstance.controlWhisk(40,50));

        }
    }
}
