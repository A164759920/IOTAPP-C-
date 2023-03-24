using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AI.Fuzzy.Library;
namespace IOTAPP
{
    public partial class canDevice 
    {
        // 模糊控制系统
        MamdaniFuzzySystem fuzzy_temperature = null;
        MamdaniFuzzySystem fuzzy_ph = null;
        MamdaniFuzzySystem fuzzy_whisk = null;
        MamdaniFuzzySystem CreateTempSystem()
        {
            
            MamdaniFuzzySystem _fuzzy_temperature = new MamdaniFuzzySystem();
            // 定义输入
            FuzzyVariable fsTemperature = new FuzzyVariable("leveltemperature", tempRange[0], tempRange[1]);
            double averTemperature = (tempRange[0] + tempRange[1]) / 2;

            fsTemperature.Terms.Add(new FuzzyTerm("cold", new TriangularMembershipFunction(tempRange[0] - 2.0, tempRange[0], tempRange[0] + 2.0)));
            fsTemperature.Terms.Add(new FuzzyTerm("normal", new TriangularMembershipFunction(averTemperature - 2.0, averTemperature, averTemperature + 2.0)));
            fsTemperature.Terms.Add(new FuzzyTerm("hot", new TriangularMembershipFunction(tempRange[1] - 2.0, tempRange[1], tempRange[1] + 2.0)));
            _fuzzy_temperature.Input.Add(fsTemperature);

            // 定义输出
            FuzzyVariable fvState = new FuzzyVariable("state", 0.0, 30.0);
            fvState.Terms.Add(new FuzzyTerm("hotState", new TriangularMembershipFunction(0.0, 5.0, 10.0)));
            fvState.Terms.Add(new FuzzyTerm("0", new TriangularMembershipFunction(10.0, 15.0,20.0)));
            fvState.Terms.Add(new FuzzyTerm("coldState", new TriangularMembershipFunction(20.0, 25.0, 30.0)));
            _fuzzy_temperature.Output.Add(fvState);
            try
            {
                MamdaniFuzzyRule rule1 = _fuzzy_temperature.ParseRule("if (leveltemperature is cold) then state is hotState");
               MamdaniFuzzyRule rule2 = _fuzzy_temperature.ParseRule("if (leveltemperature is normal) then state is 0");
               MamdaniFuzzyRule rule3 = _fuzzy_temperature.ParseRule("if (leveltemperature is hot) then state is coldState");
                _fuzzy_temperature.Rules.Add(rule1);
              _fuzzy_temperature.Rules.Add(rule2);
               _fuzzy_temperature.Rules.Add(rule3);

            }
            catch (Exception ex)
            {
                Console.WriteLine("创建温度模糊失败" + ex);
                return null;
            }
            return _fuzzy_temperature;
        }
        MamdaniFuzzySystem CreatephSystem()
        {

            MamdaniFuzzySystem _fuzzy_ph = new MamdaniFuzzySystem();
            // 定义输入
            FuzzyVariable fsPH = new FuzzyVariable("levelph", pHRange[0], pHRange[1]);
            double averPH = (pHRange[0] + pHRange[1]) / 2;
            fsPH.Terms.Add(new FuzzyTerm("acid", new TriangularMembershipFunction(pHRange[0] - 1.0, pHRange[0], pHRange[0] + 1.0)));
            fsPH.Terms.Add(new FuzzyTerm("normal", new TriangularMembershipFunction(averPH - 1, averPH, averPH + 1)));
            fsPH.Terms.Add(new FuzzyTerm("base", new TriangularMembershipFunction(pHRange[1] - 1.0, pHRange[1], pHRange[1] + 1.0)));
            _fuzzy_ph.Input.Add(fsPH);

            // 定义输出
            FuzzyVariable fvState = new FuzzyVariable("state", 0.0, 30.0);
            fvState.Terms.Add(new FuzzyTerm("baseState", new TriangularMembershipFunction(0.0, 5.0, 10.0)));
            fvState.Terms.Add(new FuzzyTerm("middleState", new TriangularMembershipFunction(10.0, 15.0, 20.0)));
            fvState.Terms.Add(new FuzzyTerm("acidState", new TriangularMembershipFunction(20.0, 25.0, 30.0)));
            _fuzzy_ph.Output.Add(fvState);
            try
            {
                MamdaniFuzzyRule rule1 = _fuzzy_ph.ParseRule("if (levelph is acid) then state is baseState");
                MamdaniFuzzyRule rule2 = _fuzzy_ph.ParseRule("if (levelph is normal) then state is middleState");
                MamdaniFuzzyRule rule3 = _fuzzy_ph.ParseRule("if (levelph is base) then state is acidState");
                _fuzzy_ph.Rules.Add(rule1);
                _fuzzy_ph.Rules.Add(rule2);
                _fuzzy_ph.Rules.Add(rule3);

            }
            catch (Exception ex)
            {
                Console.WriteLine("创建PH模糊失败" + ex);
                return null;
            }
            return _fuzzy_ph;
        }
        MamdaniFuzzySystem CreateWhiskSystem()
        {
            MamdaniFuzzySystem _fuzzy_whisk = new MamdaniFuzzySystem();
            // 定义输入
            // 溶氧
            FuzzyVariable fsOxygen = new FuzzyVariable("leveloxygen", oxygenRange[0], oxygenRange[1]);
            double averOxygen = (oxygenRange[0] + oxygenRange[1]) / 2;
            fsOxygen.Terms.Add(new FuzzyTerm("Osmall", new TriangularMembershipFunction(oxygenRange[0] - 1.0, oxygenRange[0], oxygenRange[0] + 1.0)));
            fsOxygen.Terms.Add(new FuzzyTerm("Onormal", new TriangularMembershipFunction(averOxygen - 1.0, averOxygen, averOxygen)));
            fsOxygen.Terms.Add(new FuzzyTerm("Olarge", new TriangularMembershipFunction(oxygenRange[1] - 1.0, oxygenRange[1], oxygenRange[1] + 1.0)));
            _fuzzy_whisk.Input.Add(fsOxygen);

            // 泡沫
            FuzzyVariable fsFoam = new FuzzyVariable("levelfoam", foamRange[0], foamRange[1]);
            double averFoam = (foamRange[0] + foamRange[1]) / 2;
            fsFoam.Terms.Add(new FuzzyTerm("Fsmall", new TriangularMembershipFunction(foamRange[0] - 1.0, foamRange[0], foamRange[0] + 1.0)));
            fsFoam.Terms.Add(new FuzzyTerm("Fnormal", new TriangularMembershipFunction(averFoam - 1.0, averFoam, averFoam + 1.0)));
            fsFoam.Terms.Add(new FuzzyTerm("Flarge", new TriangularMembershipFunction(foamRange[1] - 1.0, foamRange[1], foamRange[1] + 1.0)));
            _fuzzy_whisk.Input.Add(fsFoam);


            // 定义输出
            FuzzyVariable fvState = new FuzzyVariable("state", 0.0, 40.0);
            fvState.Terms.Add(new FuzzyTerm("off", new TriangularMembershipFunction(0.0, 5.0, 10.0)));
            fvState.Terms.Add(new FuzzyTerm("lowSpeed", new TriangularMembershipFunction(10.0, 15.0, 20.0)));
            fvState.Terms.Add(new FuzzyTerm("normalSpeed", new TriangularMembershipFunction(20.0, 25.0, 30.0)));
            fvState.Terms.Add(new FuzzyTerm("highSpeed", new TriangularMembershipFunction(30.0, 35.0, 40.0)));
            _fuzzy_whisk.Output.Add(fvState);
            try
            {
                MamdaniFuzzyRule rule1 = _fuzzy_whisk.ParseRule("if (leveloxygen is Osmall) and (levelfoam is Fsmall) then state is lowSpeed");
                MamdaniFuzzyRule rule2 = _fuzzy_whisk.ParseRule("if (leveloxygen is Osmall) and (levelfoam is Fnormal) then state is normalSpeed");
                MamdaniFuzzyRule rule3 = _fuzzy_whisk.ParseRule("if (leveloxygen is Osmall) and (levelfoam is Flarge) then state is highSpeed");
                MamdaniFuzzyRule rule4 = _fuzzy_whisk.ParseRule("if (leveloxygen is Olarge) and (levelfoam is Fsmall) then state is off");
                MamdaniFuzzyRule rule5 = _fuzzy_whisk.ParseRule("if (leveloxygen is Olarge) and (levelfoam is Fnormal) then state is normalSpeed");
                MamdaniFuzzyRule rule6 = _fuzzy_whisk.ParseRule("if (leveloxygen is Olarge) and (levelfoam is Flarge) then state is highSpeed");
                _fuzzy_whisk.Rules.Add(rule1);
                _fuzzy_whisk.Rules.Add(rule2);
                _fuzzy_whisk.Rules.Add(rule3);
                _fuzzy_whisk.Rules.Add(rule4);
                _fuzzy_whisk.Rules.Add(rule5);
                _fuzzy_whisk.Rules.Add(rule6);

            }
            catch (Exception ex)
            {
                Console.WriteLine("创建PH模糊失败" + ex);
                return null;
            }
            return _fuzzy_whisk;
        }

        public void initFuzzy()
        {
           fuzzy_temperature = CreateTempSystem();
            fuzzy_ph = CreatephSystem();
            fuzzy_whisk = CreateWhiskSystem();
        }
        public double controlTemp(double temp)
        {
            FuzzyVariable fvTemperature = fuzzy_temperature.InputByName("leveltemperature");
            FuzzyVariable fvState = fuzzy_temperature.OutputByName("state");
            Dictionary<FuzzyVariable, double> inputValues = new Dictionary<FuzzyVariable, double>();
            inputValues.Add(fvTemperature, temp);
            Dictionary<FuzzyVariable, double> result = fuzzy_temperature.Calculate(inputValues);
            return result[fvState];
        }
        public double controlPH(double ph)
        {
            FuzzyVariable fvPH = fuzzy_ph.InputByName("levelph");
            FuzzyVariable fvState = fuzzy_ph.OutputByName("state");
            Dictionary<FuzzyVariable, double> inputValues = new Dictionary<FuzzyVariable, double>();
            inputValues.Add(fvPH, ph);
            Dictionary<FuzzyVariable, double> result = fuzzy_ph.Calculate(inputValues);
            return result[fvState];
        }
        public double controlWhisk(double oxygen,double foam)
        {
            FuzzyVariable fvOyxgen = fuzzy_whisk.InputByName("leveloxygen");
            FuzzyVariable fvFoam = fuzzy_whisk.InputByName("levelfoam");
            FuzzyVariable fvState = fuzzy_whisk.OutputByName("state");
            Dictionary<FuzzyVariable, double> inputValues = new Dictionary<FuzzyVariable, double>();
            inputValues.Add(fvOyxgen, oxygen);
            inputValues.Add(fvFoam, foam);
            Dictionary<FuzzyVariable, double> result = fuzzy_whisk.Calculate(inputValues);
            return result[fvState];
        }
    }
}
