namespace AutoInority
{
    /// <summary>
    /// 游戏装备 id（锚点：游戏 BaseEquipment.txt 与各装备 script 内的 const）。
    /// id 规律 = 异想体图鉴号 100xxx 的段位替换：2xxxxx 武器、3xxxxx 装甲、4xxxxx 特殊装备（special）。
    /// </summary>
    internal enum EquipmentId
    {
        /// <summary>
        /// 白夜（T-03-46）的特殊武器「失乐园」。白夜装甲 OnTakeDamage 持有它时会额外做
        /// 小伤害归零并回血回精神（有副作用），装备伤害修正层须跳过该组合
        /// （游戏 DeathAngelArmor._DEATH_ANGEL_GIFT 同值）。
        /// </summary>
        ParadiseLost = 400015,
    }
}
