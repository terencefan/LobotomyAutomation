using System.Collections.Generic;
using System.Reflection;

using AutoInority.Creature;

using UnityEngine;

namespace AutoInority.Extentions
{
    public static class CreatureModelExtensions
    {
        private static Dictionary<long, ICreatureExtension> _dict = new Dictionary<long, ICreatureExtension>();

        public static float CalculateWorkSuccessProb(this CreatureModel creature, AgentModel agent, SkillTypeInfo skill)
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
            prob = !(num2 > 0f) ? prob - num : prob - num2;
            if (creature.sefira.agentDeadPenaltyActivated)
            {
                prob -= 0.5f;
            }
            return prob;
        }

        public static ICreatureExtension GetCreatureExtension(this CreatureModel creature)
        {
            if (!_dict.TryGetValue(creature.metaInfo.id, out var ext))
            {
                ext = CreateCreatureExtension(creature);
                _dict[creature.metaInfo.id] = ext;
            }
            return ext;
        }

        public static int GoodBound(this CreatureModel creature) => creature.metaInfo.feelingStateCubeBounds.upperBounds[1];

        public static bool IsAvailable(this CreatureModel creature)
        {
            var getter = typeof(IsolateRoom).GetProperty("IsWorkAllocated", BindingFlags.NonPublic | BindingFlags.Instance);
            var IsWorkAllocated = (bool)getter.GetValue(creature.Unit.room, null);
            return creature.state == CreatureState.WAIT && creature.feelingState == CreatureFeelingState.NONE && !IsWorkAllocated;
        }

        public static bool IsKit(this CreatureModel creature) => creature.metaInfo.creatureWorkType == CreatureWorkType.KIT;

        public static bool IsUrgent(this CreatureModel creature)
        {
            return creature.isOverloaded || creature.GetCreatureExtension().IsUrgent();
        }

        public static int MaxCube(this CreatureModel creature) => creature.metaInfo.feelingStateCubeBounds.GetLastBound();

        public static int NormalBound(this CreatureModel creature) => creature.metaInfo.feelingStateCubeBounds.upperBounds[0];

        public static string Tag(this CreatureModel creature) => $"<color=#ef9696>{creature.metaInfo.name}</color>";

        public static bool TryGetEGOGift(this CreatureModel creature, out CreatureEquipmentMakeInfo gift)
        {
            gift = creature.metaInfo.equipMakeInfos.Find((x) => x.equipTypeInfo.type == EquipmentTypeInfo.EquipmentType.SPECIAL);
            return gift != null;
        }

        public static bool TryGetPortrait(this CreatureModel creature, out Sprite portrait)
        {
            portrait = Resources.Load<Sprite>(creature.metaInfo.portraitSrc);

            // TODO generate a custom portrait.
            return portrait != null;
        }

        private static ICreatureExtension CreateCreatureExtension(CreatureModel creature)
        {
            if (creature.script is HappyTeddy)
            {
                return new HappyTeddyExt(creature);
            }
            else if (creature.script is LittleWitch)
            {
                return new LaetitiaExt(creature);
            }
            else if (creature.script is Nothing)
            {
                return new NothingExt(creature);
            }
            else if (creature.script is RedShoes)
            {
                return new RedShoesExt(creature);
            }
            else if (creature.script is SingingMachine)
            {
                return new SingingMachineExt(creature);
            }
            else if (creature.script is Censored)
            {
                return new CensoredExt(creature);
            }
            else if (creature.script is BlackSwan)
            {
                return new BlackSwanExt(creature);
            }
            else if (creature.script is DontTouchMe)
            {
                return new DontTouchMeExt(creature);
            }
            else if (creature.script is BeautyBeast)
            {
                return new BeautyBeastExt(creature);
            }
            else if (creature.script is YoungPrince)
            {
                return new LittlePrince(creature);
            }
            else if (creature.script is QueenBee)
            {
                return new QueenBeeExt(creature);
            }
            else if (creature.script is ViscusSnake)
            {
                return new NakedNestExt(creature);
            }
            else if (creature.script is MagicalGirl) // Require a high work prob agent.
            {
                return new HatredQueen(creature);
            }
            else if (creature.script is SnowWhite)
            {
                return new SnowWhiteExt(creature);
            }
            else if (creature.script is LongBird)
            {
                return new JudgementBirdExt(creature);
            }
            else if (creature.script is FengYun)
            {
                return new CloudedMonkExt(creature);
            }
            else if (creature.script is FireBird)
            {
                return new FireBirdExt(creature);
            }
            else if (creature.script is Butterfly)
            {
                return new ButterflyExt(creature);
            }
            else if (creature.script is ShyThing)
            {
                return new ShyLook(creature);
            }
            else if (creature.script is Fairy)
            {
                return new FairyFestivalExt(creature);
            }
            else
            {
                return new BaseCreatureExt(creature);
            }
        }
    }
}