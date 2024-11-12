# GodCheck

- Authors: 羽学
- Source: 无
- This is a Tshock server plugin, mainly used for：
- checking player invincibility and monitoring whether their health damage status is reasonable, 
- and to apply custom penalties if not.

## Update Log

```
v1.0.1
Removed the check for exceeding the health upper limit (because Tshock handles it itself)
Added a method to detect healing amounts, with customizable bans by account, UUID, or IP
Optimized punishment methods, handling teleportation punishment within a range
Automatically clears violation counts when a player dies, is kicked out, or banned
Changed the check for buffs to an array, allowing for customizable durations

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
  "Main Plugin Switch": true,
  "Check Invincible Player Buffs": [
    39,
    67,
    80,
    144
  ],
  "Check Buff Duration": 20,
  "Damage Trigger Invincibility Check Interval": 120.0,
  "Distance to Boss for Invincibility Check": 10.0,
  "Check Modified Defense": true,
  "Allowed Damage Reduction Rate": 0.7,
  "Damage Below Which Is Not Considered a Violation": 20,
  "Broadcast Player Health Changes": true,
  "Broadcast Player Damage Numbers": true,
  "Damage Below Which Will Not Be Broadcasted": 25,
  "Check Modified Healing": true,
  "Healing Above Which Triggers Punishment": 50,
  "Interval Below Which Triggers Punishment": 30,
  "Ignore Punishment Distance Near Nurses": 30.0,
  "Broadcast Player Healing": true,
  "Healing Below Which Will Not Be Broadcasted": 50,
  "Punish Unreasonable Health Upper Limit Increase": false,
  "Violation Count for Punishment": 2,
  "Punish Kick Out Players": true,
  "Ban Switch": false,
  "Ban Duration in Seconds": 600,
  "Ban by IP": false,
  "Ban by Account": true,
  "Ban by Device": false,
  "Punish Teleport Players": false,
  "Teleport Coordinates": {
    "X": 0,
    "Y": 0
  },
  "Punish Apply Buff": false,
  "Punish Buff Table": {
    "156": 3600,
    "122": 240
  }
}
```
## FeedBack
- Github Issue -> TShockPlugin Repo: https://github.com/UnrealMultiple/TShockPlugin
- TShock QQ Group: 816771079
- China Terraria Forum: trhub.cn, bbstr.net, tr.monika.love