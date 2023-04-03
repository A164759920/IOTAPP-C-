using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace IOTAPP
{

    public partial class canDevice
    {
        // 状态变化监视器 -- 用于同步本地数据
        public delegate void StateWatcher(string[] stateNames);
        public event StateWatcher EmitStateChangeEvent;

        // 状态变化上报器 -- 用于同步远程镜像数据
        public delegate void DeltaStateWatcher(string payload);
        public event DeltaStateWatcher EmitDeltaStateChangeEvent;

        // 控制模式监视器
        public delegate void ControlStateWatcher(string nowControlState);
        public event ControlStateWatcher EmitControlChangeEvent;
        public System.Threading.Timer LocalControlTimer = null;

        // 状态参数
        public  string controlState = "0";  // 0-本地控制(local)  1-远程控制(remote)
        public  string hotState = "0";
        public System.Threading.Timer hotTimer = null;

        public  string coldState = "0";
        public System.Threading.Timer coldTimer = null;

        public  string acidState = "0";
        public System.Threading.Timer acidTimer = null;

        public  string baseState = "0";
        public System.Threading.Timer baseTimer = null;

        public  string whiskState = "0";
        public System.Threading.Timer whiskTimer = null;

        // 温度
        public  Double temperature = 0;
        public  double[] tempRange = { 20, 28 };

        // 溶氧
        public  Double oxygen = 0;
        public  double[] oxygenRange = { 40, 50 };
        // pH
        public  Double pH = 0;
        public  double[] pHRange = { 6, 8 };
        // 泡沫
        public  Double foam = 0;
        public  double[] foamRange = { 40, 50 };

        /*
         * @description 生成指定范围 + 小数位数 的随机数
        */
        private  double NextDouble(Random ran, double minValue, double maxValue, int decimalPlace)
        {
            double randNum = ran.NextDouble() * (maxValue - minValue) + minValue;
            return Convert.ToDouble(randNum.ToString("f" + decimalPlace));
        }
        /*
         * @description 创建dp的payload
         */
        public  JArray createDpItem(double Vdata)
        {
            JObject vObject = new JObject();
            vObject.Add("v", Vdata);
            JArray vArray = new JArray();
            vArray.Add(vObject);
            return vArray;
        }

        /*
         * @description 创建上报payload
         */
        public string createJSONData(int id)
        {
            JObject DataObj = new JObject();
            var tempArray = createDpItem(temperature);
            var oxyArray = createDpItem(oxygen);
            var pHArray = createDpItem(pH);
            var foamArray = createDpItem(foam);
            DataObj.Add("temperature", tempArray);
            DataObj.Add("oxygen", oxyArray);
            DataObj.Add("pH", pHArray);
            DataObj.Add("foam", foamArray);
            JObject tempObj = new JObject();
            tempObj.Add(new JProperty("id", id));
            tempObj.Add(new JProperty("dp", DataObj));
            return tempObj.ToString();
        }

        /*
         * @description 初始化传感器参数
         */

        public  void InitDevice()
        {
            temperature = NextDouble(new Random(), tempRange[0], tempRange[1], 1);
            oxygen = NextDouble(new Random(), oxygenRange[0], oxygenRange[1], 1);
            pH = NextDouble(new Random(), pHRange[0], pHRange[1],1);
            foam = NextDouble(new Random(), foamRange[0], foamRange[1], 1);

            Console.WriteLine("温度" + temperature);
            Console.WriteLine("溶氧" + oxygen);
            Console.WriteLine("ph" + pH);
            Console.WriteLine("泡沫" + foam);/*

             */
        }

        /*
         * @description 手动同步状态并上报【控制模式选择控制器】
         */
        public  void SetState(string rawData)
        {
            // 若为本地控制模式，则不处理
            JObject rss = JObject.Parse(rawData);
            //controlState = (string)rss["state"]["desired"]["controlState"];
            if(controlState == "1")
            {
                hotState = (string)rss["state"]["desired"]["hotState"];
                coldState = (string)rss["state"]["desired"]["coldState"];
                acidState = (string)rss["state"]["desired"]["acidState"];
                baseState = (string)rss["state"]["desired"]["baseState"];
                whiskState = (string)rss["state"]["desired"]["whiskState"];
            }
            else if(controlState == "0")// 本地控制模式
            {
                Console.WriteLine("[定位]Remote_SetState为本地控制模式，不同步数据");
            }
            else
            {
                Console.WriteLine("[SetState]参数错误");
            }
            // 上报控制模式改变
            EmitControlChangeEvent(controlState);
        }

        /*
         * @description 自动处理Remote模式下的Delta信息
         */
        public void Remote_handleDelta(string rawData)
        {
            // 同步本地存在Delta的状态
             void syncLocalStateByDelta(string Name,string Value)
            {
                switch (Name)
                {
                    case "hotState":
                        hotState = Value;
                        break;
                    case "coldState":
                        coldState = Value;
                        break;
                    case "acidState":
                        acidState = Value;
                        break;
                    case "baseState":
                        baseState = Value;
                        break;
                    case "whiskState":
                        whiskState = Value;
                        break;
                    default:
                       Console.WriteLine("【syncLocalStateByDelta】未命中");
                        break;
                }
            }

            JObject rss = JObject.Parse(rawData);

            // 先更新controlState的状态
            string temp_controlState = null;
            Console.WriteLine(rss["state"]);
            try
            {
                temp_controlState = rss["state"]["controlState"].ToString();
            }
            catch (Exception exp)
            {
                Console.WriteLine("错误" + exp);
               
            }
            
             if (temp_controlState!=null)
            {

                controlState = temp_controlState;
                // 上报控制模式改变
                EmitControlChangeEvent(controlState);
            }
            // 遍历Delta数据的state的key-value 更新本地的state的信息
            Console.WriteLine("控制状态"+ controlState);
            if (controlState == "1")
            {
               
                foreach (JProperty item in rss["state"])
                {
                        Console.WriteLine("{0} : {1}", item.Name, item.Value);
                    // 同步本地state 
                    syncLocalStateByDelta(item.Name.ToString(), item.Value.ToString());
                    string[] stateName = { item.Name.ToString() };
                    // 同步状态后上报到【状态-参数映射控制器】
                    Console.WriteLine("Remote执行了" + stateName);
                    EmitStateChangeEvent(stateName);
                }

                // 同步状态后全量状态上报
                JObject reportedObj = new JObject();
                string[] allState = { "controlState", "hotState", "coldState", "acidState", "baseState", "whiskState" };
                reportedObj.Add("reported", createStateJSONData(allState));
                JObject stateObj = new JObject();
                stateObj.Add("state", reportedObj);
              
                EmitDeltaStateChangeEvent(stateObj.ToString());
                //MQTT_Client.State_Publish(stateObj.ToString());
            }
            else
            {
                Console.WriteLine("传感器为本地控制模式,忽略DELTA");
            }

        }
        
        /*
         * @description 创建镜像【state-reported】字段所需的数据
         */
        public JObject createStateJSONData(string[] fieldArray)
        {
            JObject reportedObj = new JObject();
            void AddJSONByName(string Name)
            {
                switch (Name)
                {
                    case "hotState":
                        reportedObj.Add("hotState", hotState);
                        break;
                    case "coldState":
                        reportedObj.Add("coldState", coldState);
                        break;
                    case "acidState":
                        reportedObj.Add("acidState", acidState);
                        break;
                    case "baseState":
                        reportedObj.Add("baseState", baseState);
                        break;
                    case "whiskState":
                        reportedObj.Add("whiskState", whiskState);
                        break;
                    case "controlState":
                        reportedObj.Add("controlState", controlState);
                        break;
                    default:
                        Console.WriteLine("【createStateJSONData】未命中");
                        break;
                }
            }
            foreach (var fieldName in fieldArray)
            {
                AddJSONByName(fieldName);
            }
            Console.WriteLine("[定位]createStateJSONData" + reportedObj);
            return reportedObj;
        }

    }
}
