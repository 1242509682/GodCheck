# GodCheck

- Authors: 羽学
- Source: 无
- This is a Tshock server plugin, mainly used for：
- checking player invincibility and monitoring whether their health damage status is reasonable, 
- and to apply custom penalties if not.

## Update Log

```
v1.0.2
- Optimized logic
- Added configuration options for broadcasting invincibility checks, checking invincibility during boss fights, etc.
- Added automatic adjustment of damage reduction rate based on NPC kill progress when the global damage reduction rate is 0.
- Owner group has immunity from checks.
- There is still a possibility of false positives in high-frequency healing detection.

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
  "Broadcast Invincibility Check": true,
  "Violation Count to Trigger Punishment": 2,
  "Invincibility Buffs to Check": [
    39,
    67,
    80,
    144
  ],
  "Duration to Check Invincibility Buffs": 30,
  "Seconds to Check Invincibility After Damage": 120.0,
  "Seconds to Check Invincibility During Boss Fights": 30.0,
  "Grids to Check Invincibility Near Boss": 60.0,
  "Check Modified Defense": true,
  "Damage Below Which Is Not Considered Violation": 20,
  "Global Damage Reduction Rate": 0.0,
  "NPC to Reset Progress Damage Reduction": 398,
  "Automatic Progress Damage Reduction Rates": [
    {
      "Defeated Status": false,
      "Damage Reduction Rate": 0.1,
      "Monster IDs": [
        1
      ]
    },
    {
      "Defeated Status": false,
      "Damage Reduction Rate": 0.27,
      "Monster IDs": [
        13
      ]
    },
    {
      "Defeated Status": false,
      "Damage Reduction Rate": 0.52,
      "Monster IDs": [
        113
      ]
    },
    {
      "Defeated Status": false,
      "Damage Reduction Rate": 0.67,
      "Monster IDs": [
        125,
        126,
        127,
        134
      ]
    }
  ],
  "Broadcast Player Health Changes": true,
  "Broadcast Player Damage Amount": true,
  "Damage Below Which Will Not Be Broadcasted": 25,
  "Check Modified Healing": true,
  "Healing Amount Exceeding Which Triggers Punishment": 50,
  "Healing Interval Below Which Triggers Punishment": 30,
  "Grids to Ignore Punishment Near Nurse": 10.0,
  "Broadcast Player Healing": true,
  "Healing Below Which Will Not Be Broadcasted": 20,
  "Punish Unreasonable Health Upper Limit Increase": false,
  "Punish by Kicking Player": true,
  "Ban Switch": false,
  "Ban Table": [
    {
      "Ban Duration (seconds)": 600,
      "Ban IP": false,
      "Ban Account": true,
      "Ban UUID": false
    }
  ],
  "Punish by Teleporting Player": false,
  "Teleport Coordinates": {
    "X": 0,
    "Y": 0
  },
  "Punish by Applying Buff": false,
  "Punishment Buff Table": {
    "156": 3600,
    "122": 240
  }
}
```
## FeedBack
- Github Issue -> TShockPlugin Repo: https://github.com/UnrealMultiple/TShockPlugin
- TShock QQ Group: 816771079
- China Terraria Forum: trhub.cn, bbstr.net, tr.monika.love