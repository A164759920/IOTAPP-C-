using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using MQTT_API;

namespace IOTAPP
{
    public class canDevice
    {
        // 温度
        public static Double temperature = 0;
        public static double[] tempRange = { 20, 35 };
        // 溶氧
        public static Double oxygen = 0;
        public static double[] oxygenRange = { 40, 50 };
        // pH
        public static Double pH = 0;
        public static double[] pHRange = { 6, 7};
        // 泡沫
        public static Double foam = 0;
        public static double[] foamRange = { 40, 50 };

        /*
         * 生成指定范围 + 小数位数 的随机数
        */
        private static double NextDouble(Random ran, double minValue, double maxValue, int decimalPlace)
        {
            double randNum = ran.NextDouble() * (maxValue - minValue) + minValue;
            return Convert.ToDouble(randNum.ToString("f" + decimalPlace));
        }
        /*
         * 创建dp的payload
         */
        private static JArray createDpItem(double Vdata)
        {
            JObject vObject = new JObject();
            vObject.Add("v", Vdata);
            JArray vArray = new JArray();
            vArray.Add(vObject);
            return vArray;
        }
        /*
         * 创建上报payload
         */
        public static String createJSONData(int id)
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
         * 初始化设备参数
         */
        public static void InitDevice()
        {
            temperature = NextDouble(new Random(), tempRange[0], tempRange[1],2);
            oxygen = NextDouble(new Random(), oxygenRange[0], oxygenRange[1],2);
            pH = NextDouble(new Random(), pHRange[0], pHRange[1],2);
            foam = NextDouble(new Random(), foamRange[0], foamRange[1],2);
            /*
             *Console.WriteLine("温度"+ temperature);
             *Console.WriteLine("溶氧" + oxygen);
             *Console.WriteLine("ph" + pH);
             *Console.WriteLine("泡沫" + foam);
             */
        }
    }


}
