namespace AutoInority
{
    public class Common
    {
        internal static float CalculateWorkSuccessProb(AgentModel agent, CreatureModel creature, SkillTypeInfo skill)
        {
            float prob = creature.GetWorkSuccessProb(agent, skill);
            prob += creature.GetObserveBonusProb() / 100f;
            prob += creature.script.OnBonusWorkProb() / 100f;
            prob += agent.workProb / 500f;
            prob += agent.Equipment.GetWorkProbSpecialBonus(agent, skill) / 500f;
            if (agent.GetUnitBufList().Count > 0)
            {
                foreach (UnitBuf unitBuf in agent.GetUnitBufList())
                {
                    prob += unitBuf.GetWorkProbSpecialBonus(agent, skill) / 100f;
                }
            }
            prob = creature.script.TranformWorkProb(prob);
            if (prob > 0.95f)
            {
                prob = 0.95f;
            }
            float num = creature.GetRedusedWorkProbByCounter() / 100f;
            float num2 = creature.ProbReductionValue / 100f;
            Log.Warning($"num: {num}, num2: {num2}");
            prob = ((!(num2 > 0f)) ? (prob - num) : (prob - num2));
            if (creature.sefira.agentDeadPenaltyActivated)
            {
                prob -= 0.5f;
            }
            return prob;
        }

        internal static bool CanWorkWithCreature(AgentModel agent, CreatureModel creature, SkillTypeInfo skill, bool silent = false)
        {
            // TODO only work if observe state == IV

            var agentTag = agent.Tag();
            var creatureTag = creature.Tag();
            var skillTag = skill.Tag();

            if (creature.script is HappyTeddy)
            {
                var record = CenterBrain.FindLastRecord(creature);
                if (record != null && record.Agent.name == agent.name)
                {
                    if (!silent)
                    {
                        var message = string.Format(Angela.Creatures.HappyTeddy, agentTag, creatureTag, skillTag);
                        Angela.Say(message);
                    }
                    return false;
                }
            }
            else if (creature.script is LittleWitch) // Laetitia
            {
                if (agent.GetUnitBufByType(UnitBufType.LITTLEWITCH_HEART) != null)
                {
                    if (!silent)
                    {
                        var message = string.Format(Angela.Creatures.Laetitia, agentTag, creatureTag, skillTag);
                        Angela.Say(message);
                    }
                    return false;
                }
            }
            else if (creature.script is RedShoes)
            {
                if (agent.temperanceLevel < 3)
                {
                    if (!silent)
                    {
                        var message = string.Format(Angela.Creatures.RedShoes, agentTag, creatureTag, skillTag);
                        Angela.Say(message);
                    }
                    return false;
                }
            }
            else if (creature.script is SingingMachine)
            {
                if (agent.Rstat > 3 || agent.Bstat < 3)
                {
                    if (!silent)
                    {
                        var message = string.Format(Angela.Creatures.SiningMachine, agentTag, creatureTag, skillTag);
                        Angela.Say(message);
                    }
                }
                return false;
            }
            else if (creature.script is Nothing)
            {
                if (agent.justiceLevel < 4)
                {
                    if (!silent)
                    {
                        var message = string.Format(Angela.Creatures.Nothing, agentTag, creatureTag, skillTag);
                        Angela.Say(message);
                    }
                    return false;
                }

                var fixer = -0.1;
                var workSuccessProb = CalculateWorkSuccessProb(agent, creature, skill);
                var lastBound = creature.metaInfo.feelingStateCubeBounds.GetLastBound();
                var firstBound = creature.metaInfo.feelingStateCubeBounds.upperBounds[0];
                var expectedSuccessCount = (workSuccessProb + fixer) * lastBound;
                Log.Warning($"prob: {workSuccessProb}, first: {firstBound}, last: {lastBound}");

                if (expectedSuccessCount < firstBound)
                {
                    if (!silent)
                    {
                        var message = string.Format(Angela.Creatures.Nothing, agentTag, creatureTag, skillTag);
                        Angela.Say(message);
                    }
                    return false;
                }
            }
            return true;
        }
    }
}