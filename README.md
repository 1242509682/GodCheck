# GodCheck 无敌检查插件

- 作者: 羽学
- 出处: 无
- 这是一个Tshock服务器插件，主要用于：对玩家无敌检查，并对其生命受伤状态播报是否合理作出自定义处罚。

## 更新日志

```
v1.0.1
移除了血量溢出上限的检测（因为Tshock自己就能处理）
加入了检测治疗量的方法，封禁可自定义只封账号、UUID、IP
优化了惩罚方法，对传送玩家惩罚进行范围拉取处理
当玩家死亡或踢出、封禁时自动清空违规数
检查BUFF改为数组，可自定义时长


v1.0.0
对GK的《阻止玩家无敌》插件进行重构
自定义惩罚方法,4种惩罚方法只能开一个
自定义封禁时长，过时自动解封。
检测玩家受伤与血量是否合理
检测玩家接近BOSS范围内是否处于无敌
本插件分3个进程级，0和1级是检测玩家是否无敌，2级是检测玩家受伤/血量溢出/BOSS战期间是否无敌
当玩家生成、BOSS出现时都会检测一遍是否无敌（也就是进程0）
加入了对负血量的条件判断
```

## 指令

| 语法                             | 别名  |       权限       |                   说明                   |
| -------------------------------- | :---: | :--------------: | :--------------------------------------: |
| /reload  | 无 |   tshock.cfg.reload    |    重载配置文件    |
| 无  | 无 |   GodCheck    |    免疫检查权限    |

## 配置
> 配置文件位置：tshock/无敌检测.json
```json
{
  "插件主要开关": true,
  "检查无敌玩家Buff": [
    39,
    67,
    80,
    144
  ],
  "检查Buff时长": 20,
  "伤怪触发检查无敌秒数": 120.0,
  "靠近BOSS检查无敌格数": 10.0,
  "检查修改防御": true,
  "允许受伤计算的免伤率": 0.7,
  "受伤低于多少不算违规": 20,
  "播报玩家血量变化": true,
  "播报玩家受伤数值": true,
  "受伤低于多少不会播报": 25,
  "检查修改治疗": true,
  "治疗超过多少触发惩罚": 50,
  "间隔低于多少触发惩罚": 30,
  "靠近护士忽略惩罚格数": 30.0,
  "播报玩家治疗": true,
  "治疗低于多少不会播报": 50,
  "惩罚血量上限增幅不合理": false,
  "惩罚违规次数": 2,
  "惩罚踢出玩家": true,
  "惩罚封禁开关": false,
  "封禁秒数": 600,
  "封禁IP": false,
  "封禁账号": true,
  "封禁设备": false,
  "惩罚传送玩家": false,
  "传送坐标": {
    "X": 0,
    "Y": 0
  },
  "惩罚施加BUFF": false,
  "惩罚BUFF表": {
    "156": 3600,
    "122": 240
  }
}
```
## 反馈
- 优先发issued -> 共同维护的插件库：https://github.com/UnrealMultiple/TShockPlugin
- 次优先：TShock官方群：816771079
- 大概率看不到但是也可以：国内社区trhub.cn ，bbstr.net , tr.monika.love