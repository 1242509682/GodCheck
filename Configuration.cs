using Newtonsoft.Json;
using TShockAPI;

namespace GodCheck
{
    internal class Configuration
    {
        #region 实例变量
        [JsonProperty("插件主要开关", Order = -15)]
        public bool Enabled { get; set; } = true;

        [JsonProperty("播报无敌检查", Order = -14)]
        public bool MonGod { get; set; } = true;

        [JsonProperty("触发惩罚违规次数", Order = -13)]
        public int TrialsCount = 4;

        [JsonProperty("检查无敌玩家Buff", Order = -12)]
        public int[] BuffType = new int[] { 20, 39, 68 };

        [JsonProperty("检查无敌的Buff时长", Order = -11)]
        public int BuffTimer { get; set; } = 30;

        [JsonProperty("通过检查后的回复生命", Order = -11)]
        public int heal { get; set; } = 60;

        [JsonProperty("伤怪触发检查无敌秒数", Order = -10)]
        public double StrikeTimer = 300.0;

        [JsonProperty("BOSS战的检查无敌秒数", Order = -10)]
        public double StrikeBoss = 60.0;

        [JsonProperty("靠近BOSS检查无敌格数", Order = -10)]
        public double BossRange { get; set; } = 60.0;

        [JsonProperty("靠近NPC检查闪避开关", Order = -9)]
        public bool NPCRangeEnabled { get; set; } = true;

        [JsonProperty("靠近NPC检查闪避格数", Order = -9)]
        public double NPCRange { get; set; } = 1.0;

        [JsonProperty("靠近NPC检查闪避秒数", Order = -9)]
        public double StrikeLastTimer { get; set; } = 3.0;

        [JsonProperty("播报玩家闪避", Order = -9)]
        public bool MonDodge { get; set; } = false;

        [JsonProperty("忽略使用物品", Order = -8)]
        public int[] IgnoreUseItem { get; set; } = new int[] { 1326 };

        [JsonProperty("检查修改防御", Order = -8)]
        public bool CheckDefense { get; set; } = true;

        [JsonProperty("受伤低于多少不算违规", Order = -7)]
        public int IgnoringDamage { get; set; } = 20;

        [JsonProperty("全局免伤率", Order = -6)]
        public double DamageReduction = 0f;

        [JsonProperty("重置进度免伤的NPC", Order = -6)]
        public int ResetBossListNPCID = 398;

        [JsonProperty("自动进度免伤率", Order = -6)]
        public List<BossData> BossList { get; set; } = new List<BossData>();

        [JsonProperty("播报玩家血量变化", Order = -5)]
        public bool MonHurtLife { get; set; } = true;

        [JsonProperty("播报玩家受伤数值", Order = -5)]
        public bool MonHurt { get; set; } = true;

        [JsonProperty("防伤低于多少不会播报", Order = -5)]
        public int MonHurtValue { get; set; } = 25;

        [JsonProperty("检查修改治疗", Order = -4)]
        public bool CheckHeal { get; set; } = true;

        [JsonProperty("治疗量超多少触发惩罚", Order = -3)]
        public int HealValue { get; set; } = 50;

        [JsonProperty("治疗间隔低于多少惩罚", Order = -2)]
        public int HealTimer { get; set; } = 30;

        [JsonProperty("靠近护士忽略惩罚格数", Order = -1)]
        public double NurseRange { get; set; } = 10;

        [JsonProperty("播报玩家治疗", Order = 0)]
        public bool MonHeal { get; set; } = true;

        [JsonProperty("治疗低于多少不会播报", Order = 0)]
        public int MonHealValue { get; set; } = 50;

        [JsonProperty("惩罚血量上限增幅不合理", Order = 10)]
        public bool MaxLifeSpill = false;

        [JsonProperty("惩罚踢出玩家", Order = 12)]
        public bool Kick = true;

        [JsonProperty("惩罚封禁开关", Order = 13)]
        public bool Ban = false;

        [JsonProperty("惩罚封禁表", Order = 14)]
        public List<Bandata> Banlist { get; set; } = new List<Bandata>();

        [JsonProperty("惩罚传送玩家", Order = 18)]
        public bool PunTP = false;

        [JsonProperty("传送坐标", Order = 19)]
        public Point PunPosition { get; set; }

        [JsonProperty("惩罚施加BUFF", Order = 20)]
        public bool PunBuff { get; set; } = false;

        [JsonProperty("惩罚BUFF表", Order = 20)]
        public Dictionary<int, int>? BuffID { get; set; }

        #endregion

        #region 预设参数方法
        public void Ints()
        {

#if DEBUG
            MonGod = true;
            MonHurtLife = true;
            MonHurt = true;
            MonHeal = true;
            MonDodge = true;
#else
            MonGod = false;
            MonHurtLife = false;
            MonHurt = false;
            MonHeal = false;
            MonDodge = false;
#endif

            PunPosition = new Point(0, 0);

            BuffID = new Dictionary<int, int>()
            {
                { 156, 3600 },
                { 122, 240  }
            };

            BossList = new List<BossData>
            {
                new BossData(false,new int []{ 1 },0.1),
                new BossData(false,new int []{ 13 },0.27),
                new BossData(false,new int []{ 113 },0.52),
                new BossData(false,new int []{ 125, 126, 127, 134 },0.67)
            };

            Banlist = new List<Bandata>
            {
                new Bandata(600,false,true,false)
            };
        }
        #endregion

        #region 进度免伤表结构
        public class BossData
        {
            [JsonProperty("击败状态", Order = -2)]
            public bool Enabled { get; set; }
            [JsonProperty("免伤率", Order = -1)]
            public double DamageReduction { get; set; }
            [JsonProperty("怪物ID", Order = 0)]
            public int[] ID { get; set; }

            public BossData(bool enabled, int[] id, double reduction)
            {
                Enabled = enabled;
                ID = id ?? new int[] { 1 };
                DamageReduction = reduction;
            }
        }
        #endregion

        #region 封禁数据表结构
        public class Bandata
        {
            [JsonProperty("封禁秒数", Order = 14)]
            public int BanTime { get; set; }

            [JsonProperty("封禁IP", Order = 15)]
            public bool BanIP { get; set; }

            [JsonProperty("封禁账号", Order = 16)]
            public bool BanAccount { get; set; }

            [JsonProperty("封禁设备", Order = 17)]
            public bool BanUUID { get; set; }

            public Bandata(int banTime, bool banIP, bool banAccount, bool banUUID)
            {
                BanTime = banTime;
                BanIP = banIP;
                BanAccount = banAccount;
                BanUUID = banUUID;
            }
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