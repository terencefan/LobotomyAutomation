namespace AutoInority.Creature
{
    /// <summary>
    /// 宇宙碎片 O-03-60：counter==1 时基类闸门收紧为 GoodConfidence>0.99（只有 GOOD 结果不掉点），
    /// 此时 SkillSets 与 NormalSkillSets 相同是有意的——B/P 都能出 GOOD，无需在低计数时收窄技能集。
    /// NORM 50%/BAD 70% 的冷却随机掉点不建模（0.99 闸已兜底）。
    /// </summary>
    internal class CosmosExt : ExpectGoodAndNormalExt
    {
        protected override SkillTypeInfo[] NormalSkillSets { get; } = { Attachment, Repression };

        public CosmosExt(CreatureModel creature) : base(creature)
        {
        }
    }
}