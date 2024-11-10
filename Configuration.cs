using Newtonsoft.Json;
using TShockAPI;

namespace GodCheck
{
    internal class Configuration
    {
        #region 实例变量
        [JsonProperty("插件主要开关", Order = -12)]
        public bool Enabled { get; set; } = true;

        [JsonProperty("打击玩家Buff", Order = -11)]
        public int BuffType = 39;

        [JsonProperty("打击玩家间隔", Order = -10)]
        public double SkipTimer = 1.5;

        [JsonProperty("玩家杀怪间隔", Order = -9)]
        public double StrikeTimer = 300.0;

        [JsonProperty("BOSS期检间隔", Order = -9)]
        public double BossRangeTimer { get; set; } = 15.0;

        [JsonProperty("接近BOSS范围", Order = -9)]
        public double BossRange { get; set; } = 30.0;

        [JsonProperty("检测玩家防御", Order = -8)]
        public bool CheckDefense { get; set; } = true;

        [JsonProperty("监控玩家受伤", Order = -7)]
        public bool MonHurt { get; set; } = false;

        [JsonProperty("最大报伤数值", Order = -6)]
        public int MonHurtValue { get; set; } = 25;

        [JsonProperty("监控玩家血量", Order = -5)]
        public bool MonLife { get; set; } = true;

        [JsonProperty("真实伤害减免", Order = -3)]
        public float DamageReduction = 0.7f;

        [JsonProperty("低于真伤不判", Order = -2)]
        public int IgnoringDamage { get; set; } = 20;

        [JsonProperty("检查血量溢出", Order = -1)]
        public bool LifeSpill = false;

        [JsonProperty("检查血量上限", Order = -1)]
        public bool MaxLifeSpill = false;

        [JsonProperty("违规次数开罚", Order = 0)]
        public int TrialsCount = 2;

        [JsonProperty("踢出玩家", Order = 1)]
        public bool Kick = false;

        [JsonProperty("封禁玩家", Order = 2)]
        public bool Ban = true;

        [JsonProperty("封禁时长", Order = 3)]
        public int BanTime = 10;

        [JsonProperty("传送玩家", Order = 4)]
        public bool TP = false;

        [JsonProperty("传送坐标", Order = 5)]
        public Point Position { get; set; }

        [JsonProperty("施加BUFF", Order = 6)]
        public Dictionary<int, int>? BuffID { get; set; }
        #endregion

        #region 预设参数方法
        public void Ints()
        {
            Position = new Point(0, 2400);

            BuffID = new Dictionary<int, int>() 
            { 
                { 156, 3600 }, 
                { 122, 240  } 
            };
        }
        #endregion

        #region 坐标结构
        public struct Point
        {
            public int X { get; set; }
            public int Y { get; set; }

            public Point(int x, int y)
            {
                X = x;
                Y = y;
            }
        }
        #endregion

        #region 读取与创建配置文件方法
        public static readonly string FilePath = Path.Combine(TShock.SavePath, "无敌检测.json");

        public void Write()
        {
            var json = JsonConvert.SerializeObject(this, Formatting.Indented); 
            File.WriteAllText(FilePath, json);
        }

        public static Configuration Read()
        {
            if (!File.Exists(FilePath))
            {
                var NewConfig = new Configuration();
                NewConfig.Ints();
                new Configuration().Write();
                return NewConfig;
            }
            else
            {
                var jsonContent = File.ReadAllText(FilePath);
                return JsonConvert.DeserializeObject<Configuration>(jsonContent)!;
            }
        }
        #endregion
    }
}