# ProjectHelix_UnityARPG
A Unity-based Action RPG framework focused on scalable architecture and gameplay systems.



### 动作

部分动画开启RootMotion， 移动代码驱动，战斗动画驱动

```
┌──────────────────────────────────────────┐
│              Game Logic Layer             │
│         输入处理 / 技能触发 / AI            │
└─────────────────┬────────────────────────┘
                  │
┌─────────────────▼────────────────────────┐
│              FSM Layer                    │
│    状态定义 / 状态转换条件 / 状态生命周期    │
└─────────────────┬────────────────────────┘
                  │
┌─────────────────▼────────────────────────┐
│           Animation Layer                 │
│         Playables Graph 管理              │
│    ClipPlayable / Mixer / 权重控制         │
└─────────────────┬────────────────────────┘
                  │
┌─────────────────▼────────────────────────┐
│            Combat Layer                   │
│     HitBox / HurtBox / 判定窗口           │
│       Animation Event 驱动               │
└──────────────────────────────────────────┘
```



#### 技术路线

Code FSM + Animancer(封装版playable)

状态是行为语义，不是动作。

比如

行为 = 状态，动作 = 数据  

- NormalAttackState   ← 内部管理 N 段连招数据  
- SkillState          ← 内部查表选择哪个技能动作  
- MoveState           ← 内部管理 走/跑/冲刺 blend

动作按**行为语义**分组，挂在对应状态下作为数据

后续考虑升级为GAS（Gameplay Ability System）

#### 动作分类

- 基础动画角色共用
- 其他动画根据不同的武器共用，如刀，剑等武器
- 角色可以拥有特定的动画



### 资源管理

YooAsset
