using System.Text;
using TShockAPI;
using TShockAPI.DB;

namespace GodCheck
{
    public class Ban
    {
        static string BanningUser = "无敌检测";

        internal static AddBanResult AddBan(TSPlayer plr, string reason, Configuration.Bandata ban)
        {
            DateTime stop = DateTime.UtcNow.AddSeconds(ban.BanTime);
            var mess = new StringBuilder();

            AddBanResult acc = null!, uuid = null!, ip = null!;
            bool flag = true;

            // 根据配置决定是否执行封禁操作
            if (ban.BanAccount)
            {
                // 插入账号封禁
                acc = TShock.Bans.InsertBan("acc:" + plr.Name, reason, BanningUser, DateTime.UtcNow, stop);
                if (acc.Ban == null)
                {
                    flag = false;
                    mess.Append($"账号封禁失败: {acc.Message}; ");
                }
            }

            if (ban.BanIP)
            {
                // 插入IP封禁
                ip = TShock.Bans.InsertBan("ip:" + plr.IP, reason, BanningUser, DateTime.UtcNow, stop);
                if (ip.Ban == null)
                {
                    flag = false;
                    mess.Append($"IP封禁失败: {ip.Message}; ");
                }
            }

            if (ban.BanUUID)
            {
                // 插入UUID封禁
                uuid = TShock.Bans.InsertBan("uuid:" + plr.UUID, reason, BanningUser, DateTime.UtcNow, stop);
                if (uuid.Ban == null)
                {
                    flag = false;
                    mess.Append($"UUID封禁失败: {uuid.Message}; ");
                }
            }

            if (flag)
            {
                // 获取封禁ID
                string accBanId = ban.BanAccount ? acc.Ban.TicketNumber.ToString() : "";
                string ipBanId = ban.BanIP ? ip.Ban.TicketNumber.ToString() : "";
                string uuidBanId = ban.BanUUID ? uuid.Ban.TicketNumber.ToString() : "";

                // 记录封禁ID
                mess.Clear();
                mess.Append("[无敌检测] 已封禁 ").Append(plr.Name).Append(" ");
                if (ban.BanAccount) mess.Append($"acc:{accBanId}\n");
                if (ban.BanUUID) mess.Append($"UUID:{uuidBanId}\n");
                if (ban.BanIP) mess.Append($"IP:{ipBanId}\n");
                mess.Append($"依次输入以上ID进行解封：/ban del {accBanId} {uuidBanId} {ipBanId}\n" +
                    $"或通知该玩家 等待{ban.BanTime}秒 自动解封");

                TShock.Log.ConsoleInfo(mess.ToString());
                return acc ?? uuid ?? ip; // 返回第一个成功的结果
            }
            else
            {
                // 记录失败信息
                mess.Insert(0, "封禁" + plr.Name + "失败！原因: ");
                TShock.Log.ConsoleInfo(mess.ToString());
                return new AddBanResult { Message = mess.ToString() };
            }
        }

    }
}
