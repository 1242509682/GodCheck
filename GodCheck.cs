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
    public override Version Version => new Version(1, 0, 6);
    public override string Description => "涡轮增压不蒸鸭";
    #endregion

    #region 注册与释放
    public GodCheck(Main game) : base(game) { }
    public override void Initialize()
    {
        LoadConfig();
        On.Terraria.Player.Hurt += Player_Hurt;
        GeneralHooks.ReloadEvent += ReloadConfig;
        ServerApi.Hooks.NpcKilled.Register(this, NpcKilled);
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
            ServerApi.Hooks.NpcKilled.Deregister(this, NpcKilled);
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
                Spawn = true,
                MissCount = 0,
                StrikeTimer = DateTime.UtcNow
            });
        }
        else
        {
            var data = Data.player.FirstOrDefault(x => x.Name == plr.Name);
            if (data != null)
            {
                data.Progress = 0;
                data.MissCount = 0;
                data.StrikeTimer = DateTime.UtcNow;
                data.HealValue = 0;
                data.TotalHeal = 0;
                data.HealTimer = default;
                data.HealTimer2 = default;
                return;
            }
        }
    }
    #endregion

    #region 玩家生成事件 给进程0开启初始化
    private void PlayerSpawn(object o, GetDataHandlers.SpawnEventArgs args)
    {
        var plr = args.Player;
        var data = Data.player.FirstOrDefault(x => x.Name == plr.Name);

        if (plr == null || !plr.Active || !plr.IsLoggedIn || !Config.Enabled || data == null)
        {
            return;
        }
        else
        {
            data.StrikeTimer = DateTime.UtcNow;
            data.MissCount = 0;
            data.HealValue = 0;
            data.TotalHeal = 0;
            data.HealTimer = default;
            data.HealTimer2 = default;

            if (data.Progress == 2) //避免与刚加入时的进程冲突
            {
                data.Progress = 0;
                return;
            }
        }
    }
    #endregion

    #region 玩家受伤事件 获取玩家受伤值用于比对血量数据进一步判断
    private double Player_Hurt(On.Terraria.Player.orig_Hurt orig, Player plr, Terraria.DataStructures.PlayerDeathReason Source,
        int Damage, int hitDirection, bool pvp, bool quiet, bool Crit, int cooldownCounter, bool dodgeable)
    {
        if (Config.CheckDefense && Config.Enabled)
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
                    data.Hurt2 = true; //用于辅助判断无限闪避用的标识

                    if (BOSS) //排除暴击 只判断BOSS与弹幕
                    {
                        if (!Crit)
                        {
                            //减防伤害为 最小值为1 最大值为非暴击伤害 - 玩家防御
                            var RealDamage = Math.Max(1, Math.Min(Damage - plr.statDefense, Damage));

                            //如果全局免伤率不为0，则设置为全局免伤
                            if (Config.DamageReduction != 0)
                            {
                                //真实伤害为 减防伤害 - (减防伤害 * 70%免伤) 只取整数 耐力Buff-10% 蠕虫围巾-17% 海龟套-15% 冰冻海龟壳-25%
                                data.RealDamage = RealDamage - (int)(RealDamage * Config.DamageReduction);
                            }
                            else //否则遍历自动进度免伤表 根据表内NPC击败情况来赋值当前免伤率（按数值越大的那个算）
                            {
                                foreach (var npc2 in Config.BossList)
                                {
                                    //当NPC死亡时，这个标识自动会开启
                                    if (npc2.Enabled)
                                    {
                                        data.RealDamage = RealDamage - (int)(RealDamage * npc2.DamageReduction);
                                    }
                                }
                            }

                            //开启受伤标识
                            data.Hurt = true;

                            //播报伤害，过滤伤害信息：只播报超过25点减防伤害 
                            if (Config.MonHurt && RealDamage >= Config.MonHurtValue)
                            {
                                TShock.Utils.Broadcast($"[c/25CE9E:参考 |] 玩家:[c/E2E4C4:{plr.name}] 未减防:[c/F0BB77:{Damage}] 减防:[c/F25156:{RealDamage}] " +
                                    $"减免伤:[c/86B4E3:{data.RealDamage}] 来源:[c/97E587:{name}]", 222, 192, 223);
                            }
                        }
                    }
                }

            }
        }
        return orig.Invoke(plr, Source, Damage, hitDirection, pvp, quiet, Crit, cooldownCounter, dodgeable);
    }
    #endregion

    #region BOSS生成 更新所有玩家记录时间与进程
    private void NpcSpawn(NpcSpawnEventArgs args)
    {
        if (args.Handled || !Main.npc[args.NpcId].active || !Config.Enabled)
        {
            return;
        }

        if (Main.npc[args.NpcId].boss)
        {
            for (int i = 0; i < Data.player.Count; i++)
            {
                var data = Data.player[i];
                if (data == null) continue;
                if (data.Progress == 2)
                {
                    data.Spawn = true;
                    data.Progress = 0;
                    data.StrikeTimer = DateTime.UtcNow;
                    data.StrikeBoss = DateTime.UtcNow;
                    return;
                }
            }
        }
    }
    #endregion

    #region 伤害NPC 更新玩家记录时间与进程
    private void NpcStrike(NpcStrikeEventArgs args)
    {
        if (args.Handled || !Config.Enabled)
        {
            return;
        }

        var plr = args.Player;
        if (plr == null) return;

        var data = Data.player.FirstOrDefault(x => x.Name == plr.name);

        if (data != null && args.Npc.active)
        {
            if (args.Npc.boss) //是BOSS
            {
                // 与BOSS保持10格范围内 这里我加了个时间限制，对于无限血进程0无法检出它是否无敌，让它先过0再进2检防御和无限血
                if (Tool.BossRange(plr, args.Npc) && (DateTime.UtcNow - data.StrikeBoss).TotalSeconds > Config.StrikeBoss)
                {
                    if (data.Progress == 2)
                    {
                        data.Progress = 0;
                        data.StrikeBoss = DateTime.UtcNow;
                        return;
                    }
                }

                // 玩家与NPC距离在2格内,超过3秒 用于判断无限闪避
                else if (Tool.NPCRange(args.Player, args.Npc))
                {
                    data.NPCRange = true; //距离过近击中NPC标识
                    data.NpcName = args.Npc.FullName; //获取击中NPC名字
                    return;
                }
            }

            //不是BOSS 
            else
            {
                //时间到了再检
                if ((DateTime.UtcNow - data.StrikeTimer).TotalSeconds > Config.StrikeTimer)
                {
                    if (data.Progress == 2)
                    {
                        data.Progress = 0;
                        data.StrikeTimer = DateTime.UtcNow;
                        return;
                    }
                }

                //不是城镇NPC 假人 雕像怪  用于判断无限闪避
                if (!args.Npc.townNPC && args.Npc.netID != 488 && !args.Npc.SpawnedFromStatue)
                {
                    if (data.Progress == 2)
                    {
                        // 玩家与NPC距离在2格内,超过3秒
                        if (Tool.NPCRange(args.Player, args.Npc))
                        {
                            data.NPCRange = true; //距离过近击中NPC标识
                            data.NpcName = args.Npc.FullName; //获取击中NPC名字
                            return;
                        }
                    }
                }
            }
        }
    }
    #endregion

    #region NPC死亡事件 自动计算进度免伤率
    internal static Dictionary<int, HashSet<int>> BossDowned = new Dictionary<int, HashSet<int>>();
    private void NpcKilled(NpcKilledEventArgs args)
    {
        if (args.npc == null || !Config.Enabled) return;

        //循环遍历进度免伤表
        foreach (var npc in Config.BossList)
        {
            //如果是免伤表里的NPC
            if (npc.ID.Contains(args.npc.netID))
            {
                //不在死亡记录里
                if (!BossDowned.ContainsKey(npc.ID.First()))
                {
                    //给它加个哈希
                    BossDowned[npc.ID.First()] = new HashSet<int>();
                    npc.Enabled = true; //打开对应的死亡标识
                    Config.Write();
                }
            }
        }

        //月总死亡 
        if (args.npc.netID == Config.ResetBossListNPCID)
        {
            //关闭所有进度免伤的开关
            for (int i = 0; i < Config.BossList.Count; i++)
            {
                if (Config.BossList[i].Enabled)
                {
                    Config.BossList[i].Enabled = false;
                    Config.Write();
                }
            }
        }
    }
    #endregion

    #region 获取玩家血量与治疗数据，根据生命值的变化作为触发方法
    private void GetData(GetDataEventArgs args)
    {
        var plr = TShock.Players[args.Msg.whoAmI];
        if (plr == null || !plr.ConnectionAlive || !Config.Enabled ||
            plr.HasPermission("GodCheck") || plr.Group.Name == "owner")
        {
            return;
        }

        var data = Data.player.FirstOrDefault(x => x.Name == plr.Name);
        if (data == null || plr.IsBeingDisabled())
        {
            return;
        }

        //如果玩家死亡 违规、治疗数清空
        if (plr.Dead)
        {
            data.MissCount = 0;
            data.TotalHeal = 0;
            data.HealValue = 0;
        }

        // 当玩家生命变化时触发
        if (args.MsgID == PacketTypes.PlayerHp)
        {
            using var Reader = new BinaryReader(new MemoryStream(args.Msg.readBuffer, args.Index, args.Length));
            var b = Reader.ReadByte();
            var Life = Reader.ReadInt16();
            var MaxLife = Reader.ReadInt16();

            var now = DateTime.UtcNow; //初始化现在时间
            var LastTimer = 0.0; //初始化过去检查无敌的时间
            if (data.CheckGodTimer != default) //检查无敌时间不为空
            {
                //过去时间 = 现在-检查无敌时间
                LastTimer = (double)Math.Round((now - data.CheckGodTimer).TotalSeconds, 2);
            }

            // 进程0 例行检查 会不会扣血 用来给进程1 做反馈
            if (data.Progress == 0)
            {
                Progress_0(plr, data, Life, now);
            }

            // 进程1 主要检测是否无敌
            if (data.Progress == 1)
            {
                Progress_1(plr, data, Life, LastTimer);
                data.LastLife = data.Life; // 更新上次记录血量 返回
                data.Life = Life;
                data.TotalHeal = 0; //重置统计治疗量
            }

            // 进程2 对受伤与血量溢出 BOSS战时无伤的合理判断...
            if (data.Progress == 2)
            {
                Progress_2(plr, data, Life, MaxLife);
            }

            //更新记录血量
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
            var heal2 = Reader3.ReadInt16() / 256;

            Tool.Heal(plr, data, heal2);
            return;
        }

        //玩家闪避
        else if (args.MsgID == PacketTypes.PlayerDodge)
        {
            using var Reader = new BinaryReader(new MemoryStream(args.Msg.readBuffer, args.Index, args.Length));
            var Dodge = Reader.ReadInt16() / 256;

            if (Config.NPCRangeEnabled && data.NPCRange) //与怪物距离过近
                data.Dodge = true; //用于辅助判断无限闪避用的标识

            if (Config.MonDodge) //播报玩家闪避
                Tool.Dodge(plr, data, Dodge);

            return;
        }
    }
    #endregion

    #region 进程0 初始化 施加DeBuff给进程1作为检测 记录生命 
    private static void Progress_0(TSPlayer plr, MyData.PlayerData? data, short Life, DateTime now)
    {
        if (data == null) return;

        if (data.Spawn) //初始化
        {
            data.LastLife = Life; //上次记录生命 为当前生命
            data.Life = Life; //记录生命 为当前生命
            data.Spawn = false; //关闭重生标识
        }

        else //初始化后
        {
            if (Life > 30) //当前生命大于30点 开始检查
            {
                foreach (var buff in Config.BuffType)//从BUFF表遍历id
                {
                    plr.SetBuff(buff, Config.BuffTimer); //给一组DEBUFF检测一次
                }

                data.CheckGodTimer = now; //更新检查无敌时间
                data.LastLife = data.Life; //储存为上次血量记录
                data.Life = Life; //记录生命为当前生命值

                if (Config.MonGod) //播报检查：记录到的血量
                {
                    plr.SendMessage($"正在对玩家 [c/BB8DDC:{plr.Name}]([c/EB757D:{Life - data.TotalHeal}]) 检测ing.. ", 237, 234, 152);
                }

                data.Progress = 1;//进入进程1
                return;
            }

            else //血量不足30，避免自然回复每次卡到30血就不断施加debuff扣血使得伤愈不等，循环增加违规数。
            {
                plr.Heal(Config.heal); //回血
                data.Life = Life; //记录生命为当前生命值
                data.Residual = true; //开启残血标识

                if (Config.MonGod)
                {
                    plr.SendMessage($"[c/8AA0B5:残血 |] 玩家 [c/BB8DDC:{plr.Name}]([c/EB757D:{Life}] < [c/63A5DC:30]) 正在回血ing.. ", 237, 234, 152);
                }
                return;
            }
        }
    }
    #endregion

    #region 进程1 检测无敌 惩罚 放行进程2
    private static void Progress_1(TSPlayer plr, MyData.PlayerData? data, short Life, double LastTimer)
    {
        if (data.LastLife == 0 || data.Residual) //确保上次血量纪录不为0，残血时再检一次（因为治疗一次了,记录血量不准了）
        {
            data.Progress = 0; //回到进程0再来
            data.Residual = false; //关闭残血标识
            return;
        }

        //0.5秒后 当前生命值 - 治疗量 超过历史记录生命，视为:不会受伤
        if ((Life - data.TotalHeal) > data.LastLife && LastTimer > 0.5)
        {
            data.MissCount++; // 增加违规次数,
            data.Progress = 0; //回到进程0再来

            if (Config.MonGod) //播报检查：检查无敌中
                TShock.Utils.Broadcast($"[c/E62942:未过 |] 玩家:[c/1989BB:{plr.Name}] 治疗前生命:[c/6DD463:{Life - data.TotalHeal}] > " +
                $"{data.LastLife} 违规:[c/9D9EE7:{data.MissCount}] 间隔:{LastTimer}", 237, 234, 152);
            return;
        }

        //0.5秒后计算 生命-治疗量 少于之前记录生命 视为:受伤通过检查
        else if ((Life - data.TotalHeal) <= data.LastLife && LastTimer > 0.5)
        {
            plr.Heal(Config.heal); //回血
            data.MissCount = Math.Max(0, data.MissCount - 1);  //减少违规次数
            data.Progress = 2;  //进入进程2 检查受伤详情

            if (Config.MonGod) //播报检查：进入进程2
                TShock.Utils.Broadcast($"[c/57D726:通过 |] 玩家:[c/1989BB:{plr.Name}] 治疗前生命:[c/6DD463:{Life - data.TotalHeal}] <= " +
                $"{data.LastLife} 违规:[c/9D9EE7:{data.MissCount}] 间隔:{LastTimer}", 237, 234, 152);
            return;
        }

        // 违规次数达标 惩罚
        if (data.MissCount >= Config.TrialsCount)
        {
            var text = "无敌";
            Tool.Pun(plr, data, text);
            return;
        }

        return;
    }
    #endregion

    #region 进程2 检测血量负值与上限 判断受伤是否来源于无限血与修改防御等
    private static void Progress_2(TSPlayer plr, MyData.PlayerData? data, short Life, short MaxLife)
    {
        //忽略正在使用的物品
        var hasItem = plr.TPlayer.inventory.Take(50).Any(x => x != null && Config.IgnoreUseItem.Contains(x.type));
        if (plr.TPlayer.controlUseItem && hasItem && Config.IgnoreUseItem.Contains(plr.SelectedItem.type))
        {
            return;
        }

        // 老外要求加的 血量低于0 触发惩罚 并排除使用和谐法杖
        if (Life < 0 || data.Life < 0)
        {
            var text = $"因为生命负数 {Life} < 0 ";
            Tool.Pun(plr, data, text);
            return;
        }

        // 检查是否修改防御（包含无限血） 如果玩家有受到BOSS和弹幕伤害，不管死人
        if (Config.CheckDefense && data.Hurt && data.Life != 0)
        {
            CheckHurt(plr, data, Life);
            data.Hurt = false; // 关闭受伤标识，等待下次伤害回馈
            data.TotalHeal = 0; //重置统计治疗量
            return;
        }

        // 检查生命上限超出标准,加个没有使用物品的前提下去判断
        if (Config.MaxLifeSpill &&
            !plr.TPlayer.controlUseItem &&
            plr.TPlayer.ItemTimeIsZero &&
            data.MaxLife != 0 && MaxLife != data.MaxLife)
        {
            CheckLifeMax(plr, data, MaxLife);
        }

        //当检查配置中的闪避检查与小怪距离过近的标识开启时
        if (Config.NPCRangeEnabled && data.NPCRange)
        {
            CheckDodge(plr, data, Life, MaxLife);
            data.NPCRange = false;
            data.Hurt2 = false;
            data.Dodge = false;
            data.TotalHeal = 0;
            return;
        }
    }
    #endregion

    #region 检查闪避 只在与怪物距离过近、满血时触发
    private static void CheckDodge(TSPlayer plr, MyData.PlayerData? data, short Life, short MaxLife)
    {
        if (Life == MaxLife) //当前血量没有变化
        {
            data.MissCount++; // 增加违规次数,

            if (Config.MonDodge) //播报玩家闪避
                TShock.Utils.Broadcast($"[c/E62942:未过 |] 玩家:[c/1989BB:{plr.Name}]与{data.NpcName}距离[c/1989BB:{Config.NPCRange}格] 违规:[c/9D9EE7:{data.MissCount}]", 237, 234, 152);
        }
        else if (data.Hurt2 || data.Dodge || data.TotalHeal > 0) //受伤或闪避标识开启
        {
            data.MissCount = Math.Max(0, data.MissCount - 1);  //减少违规次数

            if (Config.MonDodge) //播报玩家闪避
                TShock.Utils.Broadcast($"[c/57D726:通过 |] 玩家:[c/1989BB:{plr.Name}]与{data.NpcName}距离[c/1989BB:{Config.NPCRange}格] 违规:[c/9D9EE7:{data.MissCount}]", 237, 234, 152);

            return;
        }

        // 违规次数达标 惩罚
        if (data.MissCount >= Config.TrialsCount)
        {
            var text = $"因无限闪避与{data.NpcName}距离{Config.NPCRange}格内未受伤";
            Tool.Pun(plr, data, text);
            return;
        }
        return;
    }
    #endregion

    #region 检测无限血与修改防御等方法
    private static void CheckHurt(TSPlayer plr, MyData.PlayerData? data, short Life)
    {
        // 预期生命: 记录血量 - 真实伤害 * 70% 免伤 (已经过防御计算)
        var ExpectLife = data.Life - data.RealDamage;

        // 如果真实伤害少于20点,因为实际打在身上的不会少于20点，主要过滤小弹幕伤害用
        if (data.RealDamage <= Config.IgnoringDamage)
        {
            return; //返回，如果玩家修改了防御 受伤值就不准了 只能靠比对血量变化
        }

        // 确保预期生命不为负数
        if (ExpectLife < 0)
        {
            ExpectLife = 0;
            return;
        }

        // 当前血量 - 治疗量 超过预期生命
        if ((Life - data.TotalHeal) > ExpectLife)
        {
            data.MissCount++; //增加违规数
            if (Config.MonHurtLife) // 播报受伤血量变化
                if (Config.DamageReduction != 0)
                {
                    TShock.Utils.Broadcast($"[c/E62942:未过 |] 玩家:[c/1989BB:{plr.Name}] 治疗前生命:[c/6DD463:{Life - data.TotalHeal}] >" +
                        $"预期血量:[c/D48FAF:{ExpectLife}] 伤值:[c/F25156:{data.RealDamage}] " +
                        $"免伤率:[c/F8E82C:{Config.DamageReduction}] 违规:[c/9D9EE7:{data.MissCount}]", 237, 234, 152);
                }
                else
                {
                    foreach (var npc2 in Config.BossList)
                    {
                        if (npc2.Enabled)
                        {
                            TShock.Utils.Broadcast($"[c/E62942:未过 |] 玩家:[c/1989BB:{plr.Name}] 治疗前生命:[c/6DD463:{Life - data.TotalHeal}] >" +
                                $"预期血量:[c/D48FAF:{ExpectLife}] 伤值:[c/F25156:{data.RealDamage}] " +
                                $"免伤率:[c/F8E82C:{npc2.DamageReduction}] 违规:[c/9D9EE7:{data.MissCount}]", 237, 234, 152);
                        }
                    }
                }
        }

        else if (plr.Dead)//没超过 或死亡
        {
            //减少违规数
            data.MissCount = Math.Max(0, data.MissCount - 1);
            data.Life = Life; //同步一下血量

            if (Config.MonHurtLife) // 播报受伤血量变化
                TShock.Utils.Broadcast($"[c/57D726:通过 |] 玩家:[c/1989BB:{plr.Name}] 治疗前生命:[c/6DD463:{Life - data.TotalHeal}] <" +
                $"预期血量:[c/D48FAF:{ExpectLife}] 违规:[c/9D9EE7:{data.MissCount}]", 237, 234, 152);
            return;
        }

        // 违规次数达标 开罚
        if (data.MissCount >= Config.TrialsCount)
        {
            var text = $"修改防御 实际生命: {Life} > 预期生命: {ExpectLife} ";
            Tool.Pun(plr, data, text);
            return;
        }
        return;
    }
    #endregion

    #region GK的原方法，检测血量上限是否正常
    private static void CheckLifeMax(TSPlayer plr, MyData.PlayerData? data, short MaxLife)
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
    #endregion

}