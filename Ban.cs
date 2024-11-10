using TShockAPI;
using TShockAPI.DB;

namespace GodCheck
{
    public class Ban
    {
        static string BanningUser = "无敌检测";

        public static AddBanResult AddBan(TSPlayer plr, string reason, int timer)
        {
            DateTime expiration = DateTime.UtcNow.AddMinutes(timer);

            // 插入账号封禁
            AddBanResult acc = TShock.Bans.InsertBan("acc:" + plr.Name, reason, BanningUser, DateTime.UtcNow, expiration);
            // 插入UUID封禁
            AddBanResult uuid = TShock.Bans.InsertBan("uuid:" + plr.UUID, reason, BanningUser, DateTime.UtcNow, expiration);
            // 插入IP封禁
            AddBanResult ip = TShock.Bans.InsertBan("ip:" + plr.IP, reason, BanningUser, DateTime.UtcNow, expiration);

            // 检查封禁结果
            if (acc.Ban != null && uuid.Ban != null && ip.Ban != null)
            {
                // 获取封禁ID
                string accBanId = acc.Ban.TicketNumber.ToString();
                string uuidBanId = uuid.Ban.TicketNumber.ToString();
                string ipBanId = ip.Ban.TicketNumber.ToString();

                // 记录封禁ID
                TShock.Log.ConsoleInfo($"[无敌检测] 已封禁 {plr.Name} acc:{accBanId} UUID:{uuidBanId} IP:{ipBanId}");
                TShock.Log.ConsoleInfo($"依次输入以上3个ID进行解封：/ban del {accBanId} {uuidBanId} {ipBanId}");
                TShock.Log.ConsoleInfo($"或通知该玩家 等待{GodCheck.Config.BanTime}分钟 自动解封");

                // 返回成功的封禁结果
                return acc;
            }
            else
            {
                // 记录失败信息
                string errorMessage = "封禁" + plr.Name + "失败！原因: ";
                if (acc.Ban == null) errorMessage += $"账号封禁失败: {acc.Message}; ";
                if (uuid.Ban == null) errorMessage += $"UUID封禁失败: {uuid.Message}; ";
                if (ip.Ban == null) errorMessage += $"IP封禁失败: {ip.Message}; ";

                TShock.Log.ConsoleInfo(errorMessage);
                return new AddBanResult { Message = errorMessage };
            }
        }

    }
}
