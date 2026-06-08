using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.Hooks;
using static Plugin.PlayerState;
using static Plugin.Utils;

namespace Plugin;

[ApiVersion(2, 1)]
public class Plugin(Main game) : TerrariaPlugin(game)
{
    #region 插件信息
    public static string PluginName => "无敌检测";
    public override string Name => PluginName;
    public override string Author => "羽学";
    public override Version Version => new(2, 0, 0);
    public override string Description => "使用实体碰撞箱来检测玩家无敌状态";
    #endregion

    #region 注册与释放
    public override void Initialize()
    {
        LoadConfig();
        GeneralHooks.ReloadEvent += ReloadConfig;
        ServerApi.Hooks.ServerJoin.Register(this, OnJoin);
        ServerApi.Hooks.ServerLeave.Register(this, OnLeave);
        ServerApi.Hooks.NpcAIUpdate.Register(this, OnNpcAI);
        ServerApi.Hooks.ProjectileAIUpdate.Register(this, OnProjAI);
        ServerApi.Hooks.GameUpdate.Register(this, OnGameUpdate);
        GetDataHandlers.PlayerDamage.Register(OnPlrDmg);
        On.Terraria.Projectile.Kill += OnProjKill;
        On.Terraria.Projectile.NewProjectile_IEntitySource_float_float_float_float_int_int_float_int_float_float_float_NewProjectileModifier += OnNewProj;
        TShockAPI.Commands.ChatCommands.Add(new Command(MyCmd.prem, MyCmd.MainCmd, MyCmd.cmd));
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            GeneralHooks.ReloadEvent -= ReloadConfig;
            ServerApi.Hooks.ServerJoin.Deregister(this, OnJoin);
            ServerApi.Hooks.ServerLeave.Deregister(this, OnLeave);
            ServerApi.Hooks.NpcAIUpdate.Deregister(this, OnNpcAI);
            ServerApi.Hooks.ProjectileAIUpdate.Deregister(this, OnProjAI);
            ServerApi.Hooks.GameUpdate.Deregister(this, OnGameUpdate);
            GetDataHandlers.PlayerDamage.UnRegister(OnPlrDmg);
            On.Terraria.Projectile.Kill -= OnProjKill;
            On.Terraria.Projectile.NewProjectile_IEntitySource_float_float_float_float_int_int_float_int_float_float_float_NewProjectileModifier -= OnNewProj;
            TShockAPI.Commands.ChatCommands.RemoveAll(x => x.CommandDelegate == MyCmd.MainCmd);
        }
        base.Dispose(disposing);
    }
    #endregion

    #region 配置重载读取与写入方法
    internal static Configuration Config = new(); // 配置文件实例
    private static void ReloadConfig(ReloadEventArgs args)
    {
        LoadConfig();
        args.Player.SendMessage($"[{PluginName}]重新加载配置完毕。", color);
    }

    private static void LoadConfig()
    {
        try
        {
            Config = Configuration.Read();
            Config.Write();
        }
        catch (Exception ex)
        {
            TShock.Log.ConsoleError($"[{PluginName}] 配置文件加载失败：\n{ex.Message}");
        }
    }
    #endregion

    #region 判断玩家是否会受伤，使用NPC与敌对弹幕碰撞箱检测
    private void OnNpcAI(NpcAiUpdateEventArgs args)
    {
        if (!Config.Enabled) return;

        var npc = args.Npc;

        // 排除无效、城镇、可捕捉、友好、傀儡
        if (npc == null || !npc.active || npc.townNPC || npc.catchItem > 0 ||
            npc.friendly || npc.type == NPCID.TargetDummy) return;

        var plr = TShock.Players.FirstOrDefault(p => p != null && p.Active && p.TPlayer.Hitbox.Intersects(npc.Hitbox));

        // 排除无效玩家 、服务器开启的无敌、 拥有皇家凝胶
        if (plr == null || !plr.Active || plr.HasPermission(MyCmd.prem) || plr.GodMode || plr.TPlayer.npcTypeNoAggro[npc.type]) return;

        var data = GetData(plr.Name);
        if (data == null) return;

        if (plr.TPlayer.Hitbox.Intersects(npc.Hitbox))
            PunMess(plr, data);
    }

    private void OnProjAI(ProjectileAiUpdateEventArgs args)
    {
        if (!Config.Enabled) return;

        var proj = args.Projectile;
        if (proj is null || !proj.hostile || proj.friendly || !projMap.ContainsKey(proj.whoAmI)) return;

        var plr = TShock.Players.FirstOrDefault(p => p != null && p.Active && p.TPlayer.Hitbox.Intersects(proj.Hitbox));
        if (plr == null || !plr.Active || plr.HasPermission(MyCmd.prem) || plr.GodMode) return;

        var data = GetData(plr.Name);
        if (data == null) return;

        // 敌对弹幕处理
        if (projMap.TryGetValue(proj.whoAmI, out int npcIdx))
        {
            var npc = Main.npc[npcIdx];
            if (npc == null || !npc.active) return;

            // 判断是否拥有皇家凝胶 对史莱姆免疫 如果免疫 则弹幕不处理
            if (plr.TPlayer.npcTypeNoAggro[npc.type]) return;
        }

        if (plr.TPlayer.Hitbox.Intersects(proj.Hitbox))
            PunMess(plr, data);
    }

    // 可以被攻击受伤则返回
    private void OnPlrDmg(object? sender, GetDataHandlers.PlayerDamageEventArgs e)
    {
        if (!Config.Enabled) return;

        var plr = e.Player;
        if (plr == null || !plr.Active || plr.HasPermission(MyCmd.prem) ||  plr.GodMode) return;

        var src = e.PlayerDeathReason;
        if (!src.TryGetCausingEntity(out var ent) || ent == null) return;

        var data = GetData(plr.Name);
        if (data == null) return;

        var npc = GetNPC(ent);
        if (npc != null) return;

        PunMess(plr, data);
    }
    #endregion

    #region 获取NPC攻击方法
    private static NPC? GetNPC(Entity ent)
    {
        NPC? npc = null;

        if (ent is NPC n)
        {
            npc = n;
        }
        else if (ent is Projectile proj && proj.hostile)
        {
            if (!projMap.TryGetValue(proj.whoAmI, out int idx))
                return null;

            // 从预先生成的映射中查找发射该弹幕的 NPC
            if (idx >= 0 && idx < Main.maxNPCs)
                npc = Main.npc[idx];
        }

        return npc;
    }
    #endregion

    #region 预惩罚信息公告
    private static void PunMess(TSPlayer plr, MyData data)
    {
        if (data.Pun >= Config.PunCount)
        {
            if (!Config.PunList.Contains(plr.Name))
            {
                if (!Config.Kick)
                {
                    PunTP.Add(plr);
                    Log($"【{PluginName}】 {plr.Name} 已传送世界左上角!");
                    Log($"请踢出该玩家: /kick {plr.Index}");
                    Log($"移除惩罚名单: /pun {plr.Account.ID}");
                }
                else
                {
                    plr.Disconnect($"[{PluginName}] {plr.Name} 已被自动踢出！");
                    Log($"【{PluginName}】 {plr.Name} 已被自动踢出！");
                    Log($"移除惩罚名单: /pun {plr.Account.ID}");
                }


                Config.PunList.Add(plr.Name);
                Config.Write();
            }

            return;
        }

        var now = DateTime.UtcNow;
        var elapsed = (now - data.CheckTime).TotalSeconds;
        if (elapsed >= Config.CoolDown)
        {
            var p = plr.TPlayer;
            var life = plr.TPlayer.statLife;

            if (data.Life == life || life < 0 || data.Life < 0)
            {
                data.Pun++;
                Log($"【{PluginName}】 {plr.Name} 违规 {data.Pun} 次 距离上次: {elapsed:F2}秒");
            }

            data.Life = life;
            data.CheckTime = now;
        }
    }
    #endregion

    #region 传送惩罚方法（游戏更新事件触发)
    private int Time = 0;
    public static HashSet<TSPlayer> PunTP = new();
    private void OnGameUpdate(EventArgs args)
    {
        if (!Config.Enabled) return;

        if (++Time < 60) return;
        Time = 0;

        if (PunTP.Count > 0)
        {
            foreach (TSPlayer plr in PunTP)
            {
                if (plr is null || !plr.Active) continue;
                if (new Point(plr.TileX, plr.TileY) == Point.Zero) continue;
                plr.Teleport(Point.Zero.X * 16, Point.Zero.Y * 16);
            }
        }
    }
    #endregion

    #region 弹幕发射者获取方法
    private static Dictionary<int, int> projMap = new();     // 弹幕索引 → 发射者NPC索引的缓存
    // 弹幕生成时记录发射者NPC
    private int OnNewProj(On.Terraria.Projectile.orig_NewProjectile_IEntitySource_float_float_float_float_int_int_float_int_float_float_float_NewProjectileModifier orig,
                          IEntitySource src, float x, float y, float spx, float spy,
                          int type, int dmg, float kb, int owner, float ai0, float ai1, float ai2, NewProjectileModifier modifer)
    {
        // 调用原版生成弹幕，获取弹幕索引
        int idx = orig(src, x, y, spx, spy, type, dmg, kb, owner, ai0, ai1, ai2, modifer);

        if (!Config.Enabled) return idx;

        Projectile p = Main.projectile[idx];
        if (!p.active) return idx;

        NPC? npc = null;

        // 发射源是NPC实体（例如怪物直接发射）
        if (src is EntitySource_Parent es && es.Entity is NPC n && n.active)
            npc = n;

        // 如果找到了有效的NPC，则记录映射（弹幕索引 → NPC索引）
        if (npc != null && npc.active &&
           !npc.townNPC && !npc.friendly)
            projMap[idx] = npc.whoAmI;

        return idx;
    }

    // 弹幕消失时清理缓存
    private void OnProjKill(On.Terraria.Projectile.orig_Kill orig, Projectile proj)
    {
        if (Config.Enabled && projMap.ContainsKey(proj.whoAmI))
            projMap.Remove(proj.whoAmI);

        orig(proj);
    }
    #endregion
}