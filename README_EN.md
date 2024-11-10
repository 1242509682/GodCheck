# GodCheck

- Authors: 羽学
- Source: 无
- This is a Tshock server plugin, mainly used for：
- checking player invincibility and monitoring whether their health damage status is reasonable, 
- and to apply custom penalties if not.

## Update Log

```
v1.0.0
Reconstructed the "Prevent Player Invincibility" plugin by GK
Added custom penalty methods,Only one of the four penalty methods can be enabled
Customizable ban duration with automatic unban after expiration.
Detects if player's damage and health levels are reasonable
Checks if players are invincible when approaching a BOSS
The plugin has 3 process levels; level 0 and 1 check if players are invincible, level 2 checks if players are invincible during damage, health overflow, or BOSS battles.
When a player spawns or a BOSS appears, it will check once if they are invincible (this is process level 0)
Added condition judgment for negative health
```

## Commands

| Syntax                             | Alias  |       Permission       |                   Description                   |
| -------------------------------- | :---: | :--------------: | :--------------------------------------: |
| /reload  | 无 |   tshock.cfg.reload    |    Reload the configuration file    |
| 无  | 无 |   GodCheck    |    Immune examination permission    |

## Configuration
> Configuration file location： tshock/无敌检测.json
```json
{
  "插件主要开关": true, // Main switch to enable or disable the plugin
  "打击玩家Buff": 39, // Used to detect if the player is invincible, no knockback, used in process levels 0 and 1
  "打击玩家间隔": 1.5, // Trigger interval for process level 0, in seconds
  "玩家杀怪间隔": 300.0, // Automatically checks once if the player hasn't killed a monster for over 5 minutes
  "BOSS期检间隔": 15.0, // When the player is at process level 2, this time will pass before checking again while the BOSS is alive
  "接近BOSS范围": 30.0, // Used with the above setting, judges the distance between the player and BOSS to decide whether to revert to process level 0
  "检测玩家防御": true, // Checks the damage received by the player, used for judgment in process level 2
  "监控玩家受伤": false, // Generally not used, it only observes real damage, which can be inaccurate for cheaters and should not be used as a reference.
  "最大报伤数值": 25, // When the total damage from BOSS or projectiles minus defense and immunity is less than 25 points, it starts reporting, used to filter report information with the above setting
  "监控玩家血量": true, // Reports the actual dynamic of the player's health, where `expected health` is recorded health minus real damage, if current health > recorded health then violation count +1, otherwise -1, triggers when reaching [violation count to penalize]
  "真实伤害减免": 0.7, // This is the damage reduction in real damage, taking 70% of Terraria's maximum damage reduction, e.g., 100 points of damage - 20 defense - (100 * 70%) = real damage
  "低于真伤不判": 20, // Used to filter out small projectiles in real damage, generally, real damage received by players exceeds 20 points, if a cheater modifies defense and receives only 1 point of damage, it also does not consider one point.
  "检查血量溢出": false, // Original method by GK, directly penalizes if health exceeds the limit
  "检查血量上限": false, // Original method by GK, added a judgment under the premise of using items: checks if health increases by 20 or 40 points between 100 and 400 health, and by 5 or 10 points above 400 health
  "踢出玩家": false, // One of the penalty methods, generally not very useful, a mild punishment
  "违规次数开罚": 2, // Condition to trigger penalty methods
  "封禁玩家": true, // One of the penalty methods, can control how long until unban according to the following [ban duration], simultaneously bans acc, ip, and uuid, and sends the ID number to the console or the log file in the ./tshock/logs folder after banning
  "封禁时长": 10, // Used with the above setting, in minutes, automatically unbans after 10 minutes to avoid misjudgment
  "传送玩家": false, // One of the penalty methods, used with the following [teleport coordinates], teleports cheaters to the lower left corner of hell
  "传送坐标": {
    "X": 0,  // Horizontal coordinate, sea left or right, large map is 8400, medium map 6200, small map 4200
    "Y": 2400 // Vertical coordinate, from space to hell
  },
  "施加BUFF": {
    "156": 3600, // One of the penalty methods, where [156] is the BUFF ID, 3600 is the duration of the BUFF, in frames (60 frames per second)
    "122": 240
  } //Only one of the four penalty methods can be enabled
}
```
## FeedBack
- Github Issue -> TShockPlugin Repo: https://github.com/UnrealMultiple/TShockPlugin
- TShock QQ Group: 816771079
- China Terraria Forum: trhub.cn, bbstr.net, tr.monika.love