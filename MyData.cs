using Newtonsoft.Json;

namespace GodCheck
{
    internal class MyData
    {
        [JsonProperty("玩家数据", Order = 20)]
        public List<PlayerData> player { get; set; } = new List<PlayerData>();

        #region 玩家数据结构
        public class PlayerData
        {
            [JsonProperty("名字", Order = 0)]
            public string? Name { get; set; }

            [JsonProperty("索引", Order = 0)]
            public int Index { get; set; }

            [JsonProperty("进程", Order = 1)]
            public int Progress { get; set; }

            [JsonProperty("生命值", Order = 3)]
            public int Life { get; set; }

            [JsonProperty("生命上限", Order = 3)]
            public int MaxLife { get; set; }

            [JsonProperty("违规次数", Order = 3)]
            public int MissCount { get; set; }

            [JsonProperty("实际伤害", Order = 4)]
            public int RealDamage { get; set; }

            [JsonProperty("受伤标记", Order = 4)]
            public bool Hurt { get; set; }

            [JsonProperty("治疗标识", Order = 4)]
            public bool Heal { get; set; }
            
            [JsonProperty("治疗数值", Order = 4)]
            public int HealValue { get; set; }

            [JsonProperty("上次治疗时间", Order = 5)]
            public DateTime HealTimer { get; set; }

            [JsonProperty("上次伤怪时间", Order = 5)]
            public DateTime StrikeTimer { get; set; }

            [JsonProperty("上次检查时间", Order = 5)]
            public DateTime CheckTimer { get; set; }

            public PlayerData() { }
        }
        #endregion
    }
}
