namespace AutoInority.Creature
{
    internal class LightsHammerExt : BaseCreatureExt
    {
        public LightsHammerExt(CreatureModel creature) : base(creature)
        {
        }

        public override bool CanWorkWith(AgentModel agent, SkillTypeInfo skill, out string message)
        {
            // 它的「工作」是处刑技能：威胁分足够高的员工进房即被强制变锤战士
            // 追杀全部出逃异想体，OnStageEnd 必死且无恢复路径——人命换清场由主管手动决定
            message = null;
            return false;
        }
    }
}
