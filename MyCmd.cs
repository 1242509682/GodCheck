using System.Text;
using Microsoft.Xna.Framework;
using TShockAPI;
using TShockAPI.DB;
using Terraria.ID;
using static Plugin.Plugin;
using static Plugin.Utils;

namespace Plugin;

internal class MyCmd
{
    #region 指令参数
    // 指令
    public static string cmd => "pun";
    // 权限
    public static string prem => $"{cmd}.use";
    #endregion

    #region 主指令
    internal static void MainCmd(CommandArgs args)
    {
        var plr = args.Player;
        var parameters = args.Parameters;

        // 无参数或请求帮助
        if (parameters.Count == 0)
        {
            if (plr.RealPlayer)
            {
                plr.SendMessage($"\n{ItemIcon(ItemID.NebulaPickup3)}" +
                                $"[c/AD89D5:无][c/D68ACA:敌][c/DF909A:检][c/E5A894:测]" +
                                $"{ItemIcon(ItemID.NebulaPickup2)} " +
                                $"{ItemIcon(ItemID.FragmentVortex)}" +
                                $"[c/F2F2C7:开发] [c/BFDFEA:by] [c/00FFFF:羽学] " +
                                $"{ItemIcon(ItemID.FragmentStardust)}", color);
            }

            ShowPunList(plr);
            SendMess(plr, $"/{cmd} 玩家名 或 id");
            plr.SendMessage("(空值列出,存在移除,不在添加)", Color.Gray);
            return;
        }

        string input = parameters[0];
        UserAccount? acc = null;

        // 尝试按 ID 或名称查找
        if (int.TryParse(input, out int id))
            acc = TShock.UserAccounts.GetUserAccountByID(id);
        else
            acc = TShock.UserAccounts.GetUserAccountByName(input);

        if (acc == null)
        {
            SendMess(plr, $"未找到玩家: {input}");
            return;
        }

        string name = acc.Name;

        // 存在则移除，不存在则添加
        if (Config.PunList.Contains(name))
        {
            Config.PunList.Remove(name);
            Config.Write();
            Log($"【{PluginName}】{plr.Name} 移除了惩罚名单: {name}");
        }
        else
        {
            if (TShock.Groups.GetGroupByName(acc.Group).HasPermission(prem))
            {
                SendMess(plr, $"无法添加 管理员 {name} 进惩罚名单");
                return;
            }

            Config.PunList.Add(name);
            Config.Write();
            Log($"【{PluginName}】{plr.Name} 添加了惩罚名单: {name}");

            // 若玩家在线则立即踢出
            var tar = TShock.Players.FirstOrDefault(p => p != null && p.Name == name);
            if (tar != null && tar.Active)
            {
                tar.Disconnect($"你已被列入惩罚名单");
                Log($"【{PluginName}】{name} 因被添加到惩罚名单已被踢出");
            }
        }
    }
    #endregion

    #region 显示惩罚名单方法
    private static void ShowPunList(TSPlayer plr)
    {
        if (Config.PunList.Count == 0)
        {
            plr.SendInfoMessage("当前惩罚名单为空");
            return;
        }

        var sb = new StringBuilder();
        sb.AppendLine("惩罚名单列表:");
        foreach (var name in Config.PunList)
        {
            var acc = TShock.UserAccounts.GetUserAccountByName(name);
            if (acc == null) continue;
            sb.AppendLine($"{acc.ID} - {name}");
        }

        SendMess(plr, sb.ToString());
    }
    #endregion
}