using Terraria;
using Terraria.DataStructures;
using TShockAPI;
using static Org.BouncyCastle.Math.EC.ECCurve;

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

        #region 惩罚方法
        internal static void Pun(TSPlayer plr, MyData.PlayerData? data, string text)
        {
            if (data == null) return;

            if (GodCheck.Config.Ban)
            {
                //自定义封禁时间
                plr.Disconnect($"你已被封禁！原因：{text}。");
                Ban.AddBan(plr , $"{text}", GodCheck.Config.BanTime);
            }

            else if (GodCheck.Config.Kick)
            {
                plr.Kick($"{plr.Name}" + text + "被踢出.", force: true, silent: false, "Server");
            }

            else if (GodCheck.Config.BuffID != null)
            {
                foreach (var buff in GodCheck.Config.BuffID)
                {
                    plr.SetBuff(buff.Key, buff.Value);
                }
            }

            else if (GodCheck.Config.TP) //直接拉到地狱左下角 别妨碍别人
            {
                var x = (GodCheck.Config.Position.X * 16 + GodCheck.Config.Position.X * 16) / 2f;
                var y = (GodCheck.Config.Position.Y * 16 + GodCheck.Config.Position.Y * 16) / 2f;

                // 直接传送到目标位置
                plr.Teleport(x, y, 10);
                TSPlayer.All.SendMessage($"{plr.Name}" + text + "被传送", 247, 242, 168);
            }

        }
        #endregion

        #region 判断玩家与BOSS之间范围
        public static bool BossRange(TSPlayer plr, NPC npc)
        {
            // 计算玩家和Boss之间的距离
            double dX = plr.TPlayer.position.X + (plr.TPlayer.width / 2) - (npc.position.X + (npc.width / 2));
            double dY = plr.TPlayer.position.Y + (plr.TPlayer.height / 2) - (npc.position.Y + (npc.height / 2));

            // 使用欧几里得距离公式计算实际距离
            double Range = Math.Sqrt(dX * dX + dY * dY);

            return Range <= GodCheck.Config.BossRange * 16f; //每格16像素
        }
        #endregion
    }
}
