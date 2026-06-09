using TShockAPI;
using TerrariaApi.Server;
using static Plugin.Plugin;
using static Plugin.Utils;

namespace Plugin;

internal class PlayerState
{
    #region 玩家数据状态类（内存）
    public class MyData(int life, DateTime time)
    {
        public int Life { get; set; } = life;   // 当前生命
        public int Pun { get; set; } = 0;     // 违规次数
        public DateTime CheckTime { get; set; } = time;  // 违规日期
        public DateTime LastHurtTime { get; set; } = DateTime.MinValue; // 受伤时间
        public Queue<DateTime> HurtTamps { get; set; } = new(); // 存储最近受伤时间戳的队列
    }
    #endregion

    #region 获取数据方法
    public static Dictionary<string, MyData> PlrData = new();
    public static MyData? GetData(string name)
    {
        if (!PlrData.TryGetValue(name, out var data))
            return null;

        return data;
    }
    #endregion

    #region 玩家数据创建与清理方法
    internal static void OnJoin(JoinEventArgs args)
    {
        var plr = TShock.Players[args.Who];

        if (plr.HasPermission(MyCmd.prem)) return;

        if (Config.PunList.Contains(plr.Name))
        {
            plr.Disconnect($"{plr.Name} 因处于惩罚名单 已被踢出!");
            Log($"【{PluginName}】{plr.Name} 因处于惩罚名单 已被踢出!", false);
            Log($"移除惩罚名单: /{MyCmd.cmd} {plr.Name}");
            return;
        }

        if (GetData(plr.Name) == null)
            PlrData.TryAdd(plr.Name, new MyData(plr.TPlayer.statLife, DateTime.UtcNow));
    }

    internal static void OnLeave(LeaveEventArgs args)
    {
        var plr = TShock.Players[args.Who];

        if (plr != null)
        {
            if (plr.HasPermission(MyCmd.prem)) return;

            if (Config.PunList.Contains(plr.Name))
            {
                if (PlrData.ContainsKey(plr.Name))
                    PlrData.Remove(plr.Name);

                if (PunTP.Contains(plr))
                    PunTP.Remove(plr);
            }
        }
    }
    #endregion
}