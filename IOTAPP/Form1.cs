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
    public partial class Form1 : Form
    {
       
        public class Device
        {
            public canDevice canInstance = new canDevice();
            public MQTT_Client mqttInstance = new MQTT_Client();
            public void MessageReceiveController(string Topic, string QoS, string text)
            {
                Console.WriteLine("MessageReceived >>Topic:" + Topic + "; QoS: " + QoS);
             //   Console.WriteLine("MessageReceived >>Msg: " + text);
                switch (Topic)
                {
                    case "$sys/583419/mqtt-can1/dp/post/json/+":
                        Console.WriteLine("传感器数据上报消息成功");
                        break;
                    case "$sys/583419/mqtt-can1/image/get/accepted":
                        // 同步远端镜像状态成功
                        canInstance.SetState(text);
                        break;
                    case "$sys/583419/mqtt-can1/image/get/rejected":
                        Console.WriteLine("同步状态失败");
                        break;
                    case "$sys/583419/mqtt-can1/image/update/delta":
                        // 检测到状态差值
                        canInstance.Remote_handleDelta(text);
                        break;
                    default:
                       // Console.WriteLine("未命中");
                        break;
                }
            }
            // 状态-模拟参数映射 控制器
            public void State_Params_ChangeController(string[] stateNames)
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
                                        // 阈值控制
                                        if(canInstance.temperature<= canInstance.tempRange[1] - 1.0)
                                        {
                                            canInstance.temperature += 1.0;
                                        }
                                       
                                     
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
                                        if (canInstance.temperature >= canInstance.tempRange[0] + 1.0)
                                        {
                                            canInstance.temperature -= 1.0;
                                        }     
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
                                      if(canInstance.pH >= canInstance.pHRange[0] + 0.5)
                                        {
                                            canInstance.pH -= 0.5;
                                        }                                        
                                        Console.WriteLine("当前ph:{0}", canInstance.pH);
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
                                        if (canInstance.pH <= canInstance.pHRange[1] - 0.5)
                                        {
                                            canInstance.pH += 0.5;
                                        }
                                        Console.WriteLine("当前ph:{0}", canInstance.pH);
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
                        case "controlState":
                            Console.WriteLine("命中控制模式变化");
                            break;
                        default:
                            Console.WriteLine("[State_Params_ChangeController]未命中");
                            break;
                    }
                }
                foreach(string item in stateNames)
                {
                    switchTimerByState(item);
                }
            }
            // 选择控制模式
            public void selectControlStateController(string State)
            {
                // 判断控制模式
                switch (State)
                {
                    case "0":
                        // 本地控制模式
                        canInstance.LocalControlTimer = new System.Threading.Timer((state) => {
                            canInstance.Local_controlPH(canInstance.pH);
                            canInstance.Local_controlTemp(canInstance.temperature);
                           // canInstance.Local_controlWhisk(canInstance.oxygen, canInstance.foam);
                        }, null, 0, 800);
                        break;
                    case "1":
                        // 远程控制模式
                        // 若本地控制的计时器已打开，则先关闭
                        if(canInstance.LocalControlTimer != null)
                        {
                            canInstance.LocalControlTimer.Dispose();
                            canInstance.LocalControlTimer = null;
                        }
                        // TODO:远程控制
                        Console.WriteLine("远程控制模式");
                        break;
                    default:
                        Console.WriteLine("[openControlParam]控制模式未命中");
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
            can1.mqttInstance.Connet();
            can1.canInstance.InitDevice();
            can1.canInstance.initFuzzy();
            can1.selectControlStateController(can1.canInstance.controlState);
            can1.canInstance.EmitStateChangeEvent += can1.State_Params_ChangeController;
            can1.canInstance.EmitControlChangeEvent += can1.selectControlStateController;
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // mqtt订阅分发委托
            can1.mqttInstance.OnMessageEvent += can1.MessageReceiveController;
            // 上报Delta状态委托
            can1.canInstance.EmitDeltaStateChangeEvent += can1.mqttInstance.State_Publish;
            // 上报传感器数据
            System.Threading.Timer timerpublish = new System.Threading.Timer((state) => {
               var Payload = can1.canInstance.createJSONData(123);
               // can1.mqttInstance.Data_Publish(Payload);
               // Console.WriteLine("当前温度:{0}", can1.canInstance.temperature);
            }, null, 0, 2000);
            Thread uodateThread = new Thread(new ThreadStart(UpdateLabel));
            uodateThread.Start();

        }
        // 动态更新页面参数值到对应label
        private void UpdateLabel()
        {
            while (true)
            {
                try
                {
                    Label tempLabel = this.Controls.Find("label1", true).FirstOrDefault() as Label;
                    tempLabel.Invoke(new Action(() => {
                        tempLabel.Text = can1.canInstance.temperature.ToString();
                    }));
                    Label oxygenLabel = this.Controls.Find("label2", true).FirstOrDefault() as Label;
                    oxygenLabel.Invoke(new Action(() => {
                        oxygenLabel.Text = can1.canInstance.oxygen.ToString();
                    }));
                    Label phLabel = this.Controls.Find("label3", true).FirstOrDefault() as Label;
                    phLabel.Invoke(new Action(() => {
                        phLabel.Text = can1.canInstance.pH.ToString();
                    }));
                    Label foamLabel = this.Controls.Find("label4", true).FirstOrDefault() as Label;
                    foamLabel.Invoke(new Action(() => {
                        foamLabel.Text = can1.canInstance.foam.ToString();
                    }));

                    Label hotLabel = this.Controls.Find("hotLabel", true).FirstOrDefault() as Label;
                    hotLabel.Invoke(new Action(() => {
                        if (can1.canInstance.hotState.ToString() == "0")
                        {
                            hotLabel.BackColor = Color.Red;
                            hotLabel.Text = "OFF";
                        }
                        else
                        {
                            hotLabel.BackColor = Color.Green;
                            hotLabel.Text = "ON";
                        }
                    }));

                    Label coldLabel = this.Controls.Find("coldLabel", true).FirstOrDefault() as Label;
                    coldLabel.Invoke(new Action(() => {
                        if (can1.canInstance.coldState.ToString() == "0")
                        {
                            coldLabel.BackColor = Color.Red;
                            coldLabel.Text = "OFF";
                        }
                        else
                        {
                            coldLabel.BackColor = Color.Green;
                            coldLabel.Text = "ON";
                        }
                    }));
                    Label acidLabel = this.Controls.Find("acidLabel", true).FirstOrDefault() as Label;
                    acidLabel.Invoke(new Action(() => {
                        if (can1.canInstance.acidState.ToString() == "0")
                        {
                            acidLabel.BackColor = Color.Red;
                            acidLabel.Text = "OFF";
                        }
                        else
                        {
                            acidLabel.BackColor = Color.Green;
                            acidLabel.Text = "ON";
                        }
                    }));
                    Label baseLabel = this.Controls.Find("baseLabel", true).FirstOrDefault() as Label;
                    baseLabel.Invoke(new Action(() => {
                        if (can1.canInstance.baseState.ToString() == "0")
                        {
                            baseLabel.BackColor = Color.Red;
                            baseLabel.Text = "OFF";
                        }
                        else
                        {
                            baseLabel.BackColor = Color.Green;
                            baseLabel.Text = "ON";
                        }
                    }));
                    Label whiskLabel = this.Controls.Find("whiskLabel", true).FirstOrDefault() as Label;
                    whiskLabel.Invoke(new Action(() => {
                        if (can1.canInstance.whiskState.ToString() == "0")
                        {
                            whiskLabel.BackColor = Color.Red;
                            whiskLabel.Text = "OFF";
                        }
                        else
                        {
                            whiskLabel.BackColor = Color.Green;
                            whiskLabel.Text = "ON";
                        }
                    }));
                    Thread.Sleep(1000);
                }
                catch (Exception exp)
                {
                    // 防止强制关闭报错
                    Console.WriteLine("[updateLabel]主窗口已关闭");
                    return;
                }


            }

        }
        private void button2_Click(object sender, EventArgs e)
        {
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

        }

        private void button6_Click(object sender, EventArgs e)
        {

            can1.selectControlStateController("0");
          //   Console.WriteLine("结果" + can1.canInstance.controlTemp(25));
            // Console.WriteLine(can1.canInstance.controlPH(7.5));
            // Console.WriteLine(can1.canInstance.controlWhisk(40,50));

        }

        private void button7_Click(object sender, EventArgs e)
        {
            can1.selectControlStateController("1");
        }

        private void button8_Click(object sender, EventArgs e)
        {
            Console.WriteLine("控制状态" + can1.canInstance.controlState);
            Console.WriteLine("加热器状态" + can1.canInstance.hotState);
            Console.WriteLine("制冷器状态" + can1.canInstance.coldState);
            Console.WriteLine("加酸状态" + can1.canInstance.acidState);
            Console.WriteLine("加碱状态" + can1.canInstance.baseState);
        }


    }
}
