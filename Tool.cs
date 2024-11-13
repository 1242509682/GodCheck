﻿using Terraria;
using Terraria.DataStructures;
using TShockAPI;
using static GodCheck.GodCheck;

namespace GodCheck
{
    internal class Tool
    {
        #region 获取伤害来源名称方法
        public static string GetHurtName(PlayerDeathReason damageSource)
        {
            //获取导致实体
            damageSource.TryGetCausingEntity(out var data);

            string name = "未知";
            if (data is Player player)
            {
                name = player.name;
            }
            else if (data is NPC npc)
            {
                name = npc.FullName;
            }
            else if (data is Projectile projectile)
            {
                var owner = projectile.owner;
                var tplr = TShock.Players.FirstOrDefault(p => p != null && p.Active && p.IsLoggedIn && p.Index == owner);
                if (tplr != null)
                {
                    name = tplr.Name;
                }
                else
                {
                    name = projectile.Name;
                }
            }
            return name;
        }
        #endregion

        #region 惩罚方法 这个方法涵盖了无敌、修改防御、修改血量、修改治疗量
        internal static void Pun(TSPlayer plr, MyData.PlayerData? data, string text)
        {
            if (data == null || !Config.Enabled) return;

            if (Config.Ban)
            {
                //自定义封禁时间
                plr.Disconnect($"你已被封禁！原因：{text}。");
                TSPlayer.All.SendMessage($"{plr.Name} 已被封禁！原因：{text}。", 247, 242, 168);

                foreach (var ban in Config.Banlist)
                {
                    Ban.AddBan(plr, $"{text}", ban);
                }

            }

            if (Config.Kick)
            {
                plr.Disconnect($"{plr.Name} 已被踢出！原因：{text}。");
                data.MissCount = 0;
                TSPlayer.All.SendMessage($"{plr.Name} 已被踢出！原因：{text}。", 247, 242, 168);
            }

            if (Config.PunBuff && Config.BuffID != null)
            {
                foreach (var buff in Config.BuffID)
                {
                    plr.SetBuff(buff.Key, buff.Value);
                }
            }

            if (Config.PunTP)
            {
                var centerX = Config.PunPosition.X * 16 + 8;
                var centerY = Config.PunPosition.Y * 16 + 8;
                var dx = centerX - plr.TPlayer.Center.X;
                var dy = centerY - plr.TPlayer.Center.Y;
                var distance = (float)Math.Sqrt(dx * dx + dy * dy);
                var targetX = centerX + dx * 2 / distance;
                var targetY = centerY + dy * 2 / distance;
                PullTP(plr, targetX, targetY, 2);
            }

            if (plr.Dead || plr.IsBeingDisabled())
            {
                data.MissCount = 0;
            }
        }
        #endregion

        #region 检测治疗方法
        public static void Heal(TSPlayer plr, MyData.PlayerData? data, int heal)
        {
            if (data == null) return;
            data.HealValue = heal; // 存储治疗值

            var now = DateTime.UtcNow; 
            var last = 0f;
            if (data.HealTimer != default)
            {
                //上次记录治疗时间
                last = (float)Math.Round((now - data.HealTimer).TotalSeconds, 2);
            }

            //播报玩家回血,过滤低于10点的回复量
            if (Config.MonHeal && data.HealValue >= Config.MonHealValue)
            {
                TShock.Utils.Broadcast($"玩家:[c/1989BB:{plr.Name}] 治疗:[c/6DD463:{heal}] 记录:[c/1989BB:{data.HealValue}] 间隔:[c/F25156:{last}]秒", 237, 234, 152);
            }

            if (Config.CheckHeal) //检查治疗量超标 
            {
                //如果治疗量超过50点，间隔少于45秒
                if (heal >= Config.HealValue && last <= Config.HealTimer)
                {
                    // 查找所有NPC
                    for (int i = 0; i < Main.npc.Length; i++)
                    {
                        var npc = Main.npc[i];

                        //不在护士的30格内范围内，进行处罚
                        if (npc.netID == 18 && npc.active && !NurseRange(plr, npc))
                        {
                            var text = $"修改治疗药水间隔 上次治疗: {last} < 45s ";
                            Pun(plr, data, text);
                            return;
                        }
                    }
                }
            }

            data.HealTimer = now; // 记录本次治疗时间
        }
        #endregion

        #region 判断玩家与护士之间范围
        public static bool NurseRange(TSPlayer plr, NPC npc)
        {
            // 计算玩家和护士之间的距离
            double dX = plr.TPlayer.position.X + (plr.TPlayer.width / 2) - (npc.position.X + (npc.width / 2));
            double dY = plr.TPlayer.position.Y + (plr.TPlayer.height / 2) - (npc.position.Y + (npc.height / 2));

            // 使用欧几里得距离公式计算实际距离
            double Range = Math.Sqrt(dX * dX + dY * dY);

            return Range <= Config.NurseRange * 16f; //每格16像素
        }
        #endregion

        #region 判断玩家与BOSS之间范围
        public static bool BossRange(Player plr, NPC npc)
        {
            // 计算玩家和Boss之间的距离
            double dX = plr.position.X + (plr.width / 2) - (npc.position.X + (npc.width / 2));
            double dY = plr.position.Y + (plr.height / 2) - (npc.position.Y + (npc.height / 2));

            // 使用欧几里得距离公式计算实际距离
            double Range = Math.Sqrt(dX * dX + dY * dY);

            return Range <= Config.BossRange * 16f; //每格16像素
        }
        #endregion

        #region 拉取玩家的方法
        public static void PullTP(TSPlayer plr, float x, float y, int r)
        {
            if (r <= 0)
            {
                plr.Teleport(x, y, 10);
                return;
            }
            float x2 = plr.TPlayer.Center.X;
            float y2 = plr.TPlayer.Center.Y;
            x2 -= x;
            y2 -= y;
            if (x2 != 0f || y2 != 0f)
            {
                double num = Math.Atan2(y2, x2) * 180.0 / Math.PI;
                x2 = (float)(r * Math.Cos(num * Math.PI / 180.0));
                y2 = (float)(r * Math.Sin(num * Math.PI / 180.0));
                x2 += x;
                y2 += y;
                plr.Teleport(x2, y2, 10);
            }
        }
        #endregion

        #region 判断BOSS在场 用于辅助检测修改防御时 治疗的容错值（暂时没用到）
        public static bool IsInBossFight()
        {
            // 检查是否有BOSS在场
            for (int i = 0; i < Main.npc.Length; i++)
            {
                var npc = Main.npc[i];
                if (npc.active && npc.boss)
                {
                    return true;
                }
            }
            return false;
        }
        #endregion
    }
}
