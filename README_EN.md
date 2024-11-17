# GodCheck

- Authors: 羽学
- Source: 无
- This is a Tshock server plugin, mainly used for：
- checking player invincibility and monitoring whether their health damage status is reasonable, 
- and to apply custom penalties if not.

## Update Log

```
v1.0.4
Reduced misjudgment rate:
- Fixed the issue where healing amount and last health were not reset upon respawn.
- When the last recorded health is 0, it will return to process 0 for re-inspection once.
- Added a configuration item for life recovery after passing the check.

v1.0.3
Corrected the healing punishment check, which would still detect even if the nurse did not exist; when the nurse exists, it would determine the penalty-free distance.
- Added logic to check for infinite dodging (triggered when the player is too close to a monster with full health).
- Dodging triggered by accessories naturally reduces the number of violations, while being in a state of full health at a close distance increases the violation count.

v1.0.2
Reoptimized logic:
- Added configuration options for broadcasting invincibility checks, damage boss invincibility check duration, etc.
- Added automatic adjustment of immunity rate based on NPC kill progress, enabled when the 'global immunity rate' is 0.
- The owner group has immunity check exemption permissions.
- There is still a possibility of misjudgment regarding high-frequency healing.

v1.0.1
Removed the detection of health exceeding the upper limit (as Tshock can handle this itself).
- Added methods for detecting healing amounts, allowing customizable bans for account, UUID, IP.
- Optimized punishment methods, with range retrieval handling for teleporting player punishments.
- Automatically clears violation counts when a player dies, is kicked, or banned.
- Changed the check for buffs to an array, allowing custom durations.

v1.0.0
Refactored the plugin from GK's 'Prevent Player Invincibility'.
- Customizable punishment methods, only one out of four can be enabled.
- Customizable ban duration, automatically unban after expiration.
- Detects whether player damage and health are reasonable.
- Checks if players are invincible when approaching a boss.
- This plugin has three process levels: level 0 and 1 check if the player is invincible, level 2 checks for player damage/health overflow/boss fight invincibility.
- When a player spawns or a boss appears, it will check once if they are invincible (i.e., process 0).
- Added conditions to judge negative health.
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
  "Invincibility Buff Check": [
    20,
    39,
    68
  ],
  "Duration to Check Invincibility Buffs": 30,
  "Health Recovery After Passing Check": 60,
  "Seconds to Trigger Invincibility Check on Damage": 300.0,
  "Seconds to Check Invincibility During Boss Fight": 30.0,
  "Grids to Check Invincibility Near Boss": 60.0,
  "Switch to Check Dodging Near NPCs": true,
  "Grids to Check Dodging Near NPCs": 1.0,
  "Seconds to Check Dodging Near NPCs": 3.0,
  "Broadcast Player Dodging": true,
  "Check Defense Modification": true,
  "Damage Below Which Is Not Considered a Violation": 20,
  "Global Immunity Rate": 0.0,
  "NPC ID to Reset Progress Immunity": 398,
  "Automatic Progress Immunity Rate": [
    {
      "Defeated Status": false,
      "Immunity Rate": 0.1,
      "Monster IDs": [
        1
      ]
    },
    {
      "Defeated Status": false,
      "Immunity Rate": 0.27,
      "Monster IDs": [
        13
      ]
    },
    {
      "Defeated Status": false,
      "Immunity Rate": 0.52,
      "Monster IDs": [
        113
      ]
    },
    {
      "Defeated Status": false,
      "Immunity Rate": 0.67,
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
  "Check Healing Modification": true,
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
      "Ban Device": false
    }
  ],
  "Punish by Teleporting Player": false,
  "Teleport Coordinates": {
    "X": 0,
    "Y": 0
  },
  "Punish by Applying Buffs": false,
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