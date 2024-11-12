using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.Hooks;

namespace GodCheck;

[ApiVersion(2, 1)]
public class GodCheck : TerrariaPlugin
{
    #region 插件信息
    public override string Name => "无敌检测";
    public override string Author => "羽学";
    public override Version Version => new Version(1, 0, 1);
    public override string Description => "涡轮增压不蒸鸭";
    #endregion

    #region 注册与释放
    public GodCheck(Main game) : base(game) { }
    public override void Initialize()
    {
        LoadConfig();
        On.Terraria.Player.Hurt += Player_Hurt;
        GeneralHooks.ReloadEvent += ReloadConfig;
        GetDataHandlers.PlayerSpawn.Register(this.PlayerSpawn!);
        ServerApi.Hooks.NetGetData.Register(this, this.GetData);
        ServerApi.Hooks.NpcSpawn.Register(this, this.NpcSpawn);
        ServerApi.Hooks.NpcStrike.Register(this, this.NpcStrike);
        ServerApi.Hooks.NetGreetPlayer.Register(this, this.OnGreetPlayer);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            On.Terraria.Player.Hurt -= Player_Hurt;
            GeneralHooks.ReloadEvent -= ReloadConfig;
            GetDataHandlers.PlayerSpawn.UnRegister(this.PlayerSpawn!);
            ServerApi.Hooks.NetGetData.Deregister(this, this.GetData);
            ServerApi.Hooks.NpcSpawn.Deregister(this, this.NpcSpawn);
            ServerApi.Hooks.NpcStrike.Deregister(this, this.NpcStrike);
            ServerApi.Hooks.NetGreetPlayer.Deregister(this, this.OnGreetPlayer);
        }
        base.Dispose(disposing);
    }
    #endregion

    #region 配置重载读取与写入方法
    internal static Configuration Config = new();
    private static void ReloadConfig(ReloadEventArgs args = null!)
    {
        LoadConfig();
        args.Player.SendInfoMessage("[无敌检测]重新加载配置完毕。");
    }
    private static void LoadConfig()
    {
        Config = Configuration.Read();
        Config.Write();
    }
    #endregion

    #region 给新玩家创建数据结构
    internal static MyData Data = new();
    private void OnGreetPlayer(GreetPlayerEventArgs args)
    {
        var plr = TShock.Players[args.Who];
        if (plr == null || !plr.Active || !plr.IsLoggedIn || !Config.Enabled)
        {
            return;
        }

        // 如果玩家不在数据表中，则创建新的数据结构
        if (!Data.player.Any(p => p.Name == plr.Name))
        {
            Data.player.Add(new MyData.PlayerData()
            {
                Name = plr.Name,
                Index = plr.Index,
                Progress = 0,
                MissCount = 0,
            });
        }
    }
    #endregion

    #region 玩家受伤获取实际伤害
    private double Player_Hurt(On.Terraria.Player.orig_Hurt orig, Player plr, Terraria.DataStructures.PlayerDeathReason Source,
        int Damage, int hitDirection, bool pvp, bool quiet, bool Crit, int cooldownCounter, bool dodgeable)
    {
        if (Config.CheckDefense)
        {
            //从数据表找相同名字的玩家
            var data = Data.player.FirstOrDefault(x => x.Name == plr.name);

            //获取伤害来源名称
            string name = Tool.GetHurtName(Source);

            //只获取来自BOSS与弹幕的伤害
            var BOSS = false;
            Source.TryGetCausingEntity(out var damage);
            if (damage is NPC npc && npc.boss || damage is Projectile)
            {
                BOSS = true;
            }

            if (data != null)
            {
                if (dodgeable) //触发无敌帧
                {
                    if (!Crit && BOSS) //排除暴击 只判断BOSS与弹幕
                    {
                        //全部伤害为 最小值为1 最大值为非暴击伤害 - 玩家防御
                        var RealDamage = Math.Max(1, Math.Min(Damage - plr.statDefense, Damage));

                        //真实伤害为 全部伤害 - (全伤 * 70%免伤) 只取整数 耐力Buff-10% 蠕虫围巾-17% 海龟套-15% 冰冻海龟壳-25%
                        data.RealDamage = RealDamage - (int)(RealDamage * Config.DamageReduction);

                        //开启受伤标识
                        data.Hurt = true;

                        //播报伤害，过滤伤害信息：只播报超过25点真实伤害 
                        if (Config.MonHurt && RealDamage >= Config.MonHurtValue)
                        {
                            TShock.Utils.Broadcast($"玩家:[c/E2E4C4:{plr.name}] 全伤:[c/F25156:{RealDamage}] " +
                                $"真伤:[c/86B4E3:{data.RealDamage}] 来源:[c/97E587:{name}]", 222, 192, 223);
                        }
                    }
                }
            }
        }
        return orig.Invoke(plr, Source, Damage, hitDirection, pvp, quiet, Crit, cooldownCounter, dodgeable);
    }
    #endregion

    #region 玩家生成事件 更新上次记录时间
    private void PlayerSpawn(object o, GetDataHandlers.SpawnEventArgs args)
    {
        var plr = args.Player;
        var data = Data.player.FirstOrDefault(x => x.Name == plr.Name);

        if (plr == null || !plr.Active || !plr.IsLoggedIn)
        {
            return;
        }

        if (data != null)
        {
            data.Progress = 0;
        }
    }
    #endregion

    #region BOSS生成 更新玩家记录时间与进程
    private void NpcSpawn(NpcSpawnEventArgs args)
    {
        if (args.Handled || !Main.npc[args.NpcId].boss || !Main.npc[args.NpcId].active)
        {
            return;
        }

        for (int i = 0; i < Data.player.Count; i++)
        {
            var plr = Data.player[i];
            if (plr == null) continue;
            if (plr.Progress == 2)
            {
                plr.Progress = 0;
                plr.StrikeTimer = DateTime.Now;
            }
        }
    }
    #endregion

    #region 伤害NPC 更新玩家记录时间与进程
    private void NpcStrike(NpcStrikeEventArgs args)
    {
        if (args.Handled)
        {
            return;
        }

        var data = Data.player.FirstOrDefault(x => x.Name == args.Player.name);

        if (data != null)
        {
            if ((DateTime.UtcNow - data.StrikeTimer).TotalSeconds > Config.StrikeTimer)
            {
                data.Progress = 0;
                data.StrikeTimer = DateTime.UtcNow;
                return;
            }

            if (args.Npc.boss && args.Npc.active)
            {
                // 与BOSS保持10格范围内
                if (Tool.BossRange(args.Player, args.Npc))
                {
                    // 回到进程0检查是否无敌
                    if (data.Progress == 2)
                    {
                        data.Progress = 0;
                        return;
                    }
                }
            }
        }
    }
    #endregion

    #region 通过进程处理玩家血量 进程0或1（检测是否无敌） 进程2（检测受伤合理 生命上限增幅合理 BOSS战时无伤）
    private void GetData(GetDataEventArgs args)
    {
        var plr = TShock.Players[args.Msg.whoAmI];
        if (plr == null || !plr.ConnectionAlive ||
            plr.HasPermission("GodCheck") || plr.Group.Name == "owner")
        {
            return;
        }

        var data = Data.player.FirstOrDefault(x => x.Name == plr.Name);
        if (data == null || plr.IsBeingDisabled())
        {
            return;
        }

        // 当玩家生命变化
        if (args.MsgID == PacketTypes.PlayerHp)
        {
            using var Reader = new BinaryReader(new MemoryStream(args.Msg.readBuffer, args.Index, args.Length));
            var b = Reader.ReadByte();
            var Life = Reader.ReadInt16();
            var MaxLife = Reader.ReadInt16();

            // 进程0 例行检查 会不会扣血 用来给进程1 做反馈
            if (data.Progress == 0)
            {
                CheckGOD(plr, data, Life);
            }

            // 进程1 主要检测是否无敌
            if (data.Progress == 1 && !data.Heal)
            {
                //加入容错值避免误判
                const int Tolerance = 5;
                if (Life > (data.Life - 10 + Tolerance)) // 不会受伤
                {
                    // 违规次数达标 惩罚
                    if (data.MissCount >= Config.TrialsCount)
                    {
                        var text = "无敌";
                        Tool.Pun(plr, data, text);
                        return;
                    }

                    // 增加违规次数,回到进程0
                    data.MissCount++;
                    data.Progress = 0;
                    CheckGOD(plr, data, Life);
                }

                else //会受伤
                {
                    //进入进程2
                    data.Progress = 2;
                    //清空违规次数
                    data.MissCount = 0;
                    //回血
                    plr.Heal(50);
                    return;
                }
            }

            // 进程2 对受伤与血量溢出 BOSS战时无伤的合理判断...
            if (data.Progress == 2)
            {
                // 老外要求加的 血量低于0 触发惩罚
                if (Life < 0 || data.Life < 0)
                {
                    var text = $"因为生命负数 {Life} < 0 ";
                    Tool.Pun(plr, data, text);
                    return;
                }

                // 检查是否修改防御 如果玩家有受到BOSS和弹幕伤害，不管死人
                if (Config.CheckDefense && data.Hurt && data.Life != 0)
                {
                    // 预期生命: 记录生命 - 真实伤害 * 70% 免伤 (已经过防御计算)
                    var ExpectLife = data.Life - data.RealDamage;

                    // 如果真实伤害少于20点 返回
                    if (data.RealDamage <= Config.IgnoringDamage)
                    {
                        return; // 因为实际打在身上的不会少于20点，主要过滤小弹幕伤害用
                    }

                    // 确保预期生命不会变成负数
                    if (ExpectLife < 0)
                    {
                        ExpectLife = 0;
                    }

                    // 如果玩家正在回血，跳过本次检查
                    if (data.Heal)
                    {
                        //确保不超过生命上限
                        ExpectLife = (short)Math.Min(ExpectLife + data.HealValue, MaxLife);
                        data.Heal = false; // 重置回血标志
                        data.HealValue = 0; // 重置治疗值
                        return;
                    }

                    // 血量超过预期生命 增加违规次数
                    if (Life > ExpectLife)
                    {
                        data.MissCount++;
                    }
                    else // 血量没超过预期生命 合理减少1次违规
                    {
                        data.MissCount = Math.Max(0, data.MissCount - 1);
                    }

                    // 播报血量变化 只播报有违规数的玩家
                    if (Config.MonLife)
                    {
                        TShock.Utils.Broadcast($"玩家:[c/1989BB:{plr.Name}] 生命:[c/6DD463:{Life}] " +
                        $"预期生命:[c/D48FAF:{ExpectLife}] 应受伤:[c/F25156:{data.RealDamage}] 违规数:[c/9D9EE7:{data.MissCount}]", 237, 234, 152);
                    }

                    // 违规次数达标 开罚
                    if (data.MissCount >= Config.TrialsCount)
                    {
                        var text = $"修改防御 实际生命: {Life} > 预期生命: {ExpectLife} ";
                        Tool.Pun(plr, data, text);
                        return;
                    }

                    data.Hurt = false; // 关闭标识进入，等待下次伤害回馈
                    return;
                }

                // 检查生命上限超出标准,加个没有使用物品的前提下去判断
                if (Config.MaxLifeSpill &&
                    !plr.TPlayer.controlUseItem &&
                    plr.TPlayer.ItemTimeIsZero &&
                    data.MaxLife != 0 && MaxLife != data.MaxLife)
                {
                    // 若玩家原血量上限在400至500之间
                    if (data.MaxLife >= 400 && data.MaxLife < 500)
                    {
                        // 新血量上限不是增加5点或10点，则视为异常
                        if (MaxLife != data.MaxLife + 5 && MaxLife != data.MaxLife + 10)
                        {
                            var text = $"修改血量上限({data.MaxLife} > {MaxLife - data.MaxLife} > {MaxLife})";
                            Tool.Pun(plr, data, text);
                            return;
                        }
                    }

                    // 若玩家原血量上限小于400
                    else if (data.MaxLife < 400)
                    {
                        // 新血量上限不是增加20点或40点，则视为异常
                        if (MaxLife != data.MaxLife + 20 && MaxLife != data.MaxLife + 40)
                        {
                            var text = $"修改血量上限({data.MaxLife} > {MaxLife - data.MaxLife} > {MaxLife})";
                            Tool.Pun(plr, data, text);
                            return;
                        }
                    }

                    // 最大血量超过记录值
                    else if (MaxLife > data.MaxLife)
                    {
                        var text = $"修改血量上限({data.MaxLife} > {MaxLife - data.MaxLife} > {MaxLife})";
                        Tool.Pun(plr, data, text);
                        return;
                    }
                }
            }

            data.Life = Life;
            data.MaxLife = MaxLife;
            return;
        }

        // 玩家喝血瓶的治疗（包括服务器给玩家的治疗，也算在玩家自己的治疗内）
        else if (args.MsgID == PacketTypes.EffectHeal)
        {
            using var Reader2 = new BinaryReader(new MemoryStream(args.Msg.readBuffer, args.Index, args.Length));
            var heal = Reader2.ReadInt16() / 256;

            Tool.Heal(plr, data, heal);
            return;
        }

        // 其他方式给玩家的治疗(如幽灵套弹幕,捡到小心心)
        else if (args.MsgID == PacketTypes.PlayerHealOther)
        {
            using var Reader3 = new BinaryReader(new MemoryStream(args.Msg.readBuffer, args.Index, args.Length));
            var heal = Reader3.ReadInt16() / 256;

            Tool.Heal(plr, data, heal);
            return;
        }
    }
    #endregion

    #region 检查无敌
    private static void CheckGOD(TSPlayer plr, MyData.PlayerData? data, short Life)
    {
        //检查标识打开 生命大于30点 没有在治疗
        if (Life > 30)
        {
            //给个10帧的DEBUFF检测一次
            foreach (var buff in Config.BuffType)
            {
                plr.SetBuff(buff, Config.BuffTimer);
            }

            //记录当前血量
            data.Life = Life;

            //进入进程1
            data.Progress = 1;
            plr.SendMessage($"正在对玩家 [c/BB8DDC:{plr.Name}]([c/EB757D:{data.Life}]) 检测ing.. ", 237, 234, 152);
            return;
        }
    } 
    #endregion
}