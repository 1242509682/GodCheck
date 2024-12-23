﻿
namespace GodCheck
{
    internal class MyData
    {
        public List<PlayerData> player { get; set; } = new List<PlayerData>();

        #region 玩家数据结构
        public class PlayerData
        {
            //玩家名字
            public string? Name { get; set; }

            //玩家索引
            public int Index { get; set; }

            //玩家进程
            public int Progress { get; set; }

            //记录生命
            public int Life { get; set; }

            //上次记录生命
            public int LastLife { get; set; }

            //记录生命上限
            public int MaxLife { get; set; }

            //违规次数
            public int MissCount { get; set; }

            //真实伤害
            public int RealDamage { get; set; }

            //重生标识，初始化用
            public bool Spawn { get; set; }

            //遭遇BOSS，玩家受伤标识
            public bool Hurt { get; set; }

            //击中小怪名称
            public string NpcName { get; set; }

            //治疗量
            public int HealValue { get; set; }

            //统计治疗量
            public int TotalHeal { get; set; }

            //距离过近标识
            public bool NPCRange { get; set; }

            //距离过近受伤标识
            public bool Hurt2 { get; set; }

            //闪避标识
            public bool Dodge { get; set; }

            //残血标识
            public bool Residual { get; set; }

            //上次闪避时间
            public DateTime DodgeTimer { get; set; }

            //上次治疗时间
            public DateTime HealTimer { get; set; }

            //上次自我治疗时间
            public DateTime HealTimer2 { get; set; }

            //上次伤害怪物时间
            public DateTime StrikeTimer { get; set; }

            //上次近身伤害BOSS时间
            public DateTime StrikeBoss { get; set; }

            //上次检查无敌时间
            public DateTime CheckGodTimer { get; set; }

            public PlayerData() { }
        }
        #endregion
    }
}
