using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using AutoInority.Creature;

using UnityEngine;

using WhiteNightSpace;

namespace AutoInority.Extentions
{
    public static class CreatureModelExtensions
    {
        private static Dictionary<CreatureModel, ICreatureExtension> _dict = new Dictionary<CreatureModel, ICreatureExtension>();

        private static Dictionary<CreatureModel, ICreatureKitExtension> _kitDict = new Dictionary<CreatureModel, ICreatureKitExtension>();

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
            return prob > 0 ? prob : 0;
        }

        public static IEnumerable<CreatureModel> FilterUrgent(this IEnumerable<CreatureModel> creatures, int riskLevel)
        {
            return creatures.Where(x => x.IsCreature() && x.GetRiskLevel() == riskLevel && x.IsUrgent() && x.IsAvailable());
        }

        public static List<Candidate> FindCandidates(this IEnumerable<CreatureModel> creatures, int distance)
        {
            var candidates = new List<Candidate>();
            foreach (var creature in creatures)
            {
                var agents = creature.GetExtension().FindAgents(distance);
                candidates.AddRange(Candidate.Suggest(agents, creature));
            }
            return candidates;
        }

        public static ICreatureExtension GetExtension(this CreatureModel model)
        {
            if (model.IsKit())
            {
                throw new ArgumentException($"{model.metaInfo.name} is not a creature");
            }
            if (!_dict.TryGetValue(model, out var ext))
            {
                ext = BuildExtension(model);
                _dict[model] = ext;
            }
            return ext;
        }

        public static ICreatureKitExtension GetKitExtension(this CreatureModel model)
        {
            if (!model.IsKit())
            {
                throw new ArgumentException($"{model.metaInfo.name} is not a kit");
            }
            if (!_kitDict.TryGetValue(model, out var ext))
            {
                ext = BuildKitExtension(model);
                _kitDict[model] = ext;
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

        public static bool IsCreature(this CreatureModel creature) => creature.metaInfo.creatureWorkType == CreatureWorkType.NORMAL;

        public static bool IsKit(this CreatureModel creature) => creature.metaInfo.creatureWorkType == CreatureWorkType.KIT;

        public static bool IsUrgent(this CreatureModel creature)
        {
            return creature.isOverloaded || creature.GetExtension().IsUrgent;
        }

        public static int MaxCube(this CreatureModel creature) => creature.metaInfo.feelingStateCubeBounds.GetLastBound();

        public static int NormalBound(this CreatureModel creature) => creature.metaInfo.feelingStateCubeBounds.upperBounds[0];

        public static string Tag(this CreatureModel creature) => $"<color=#ef9696>{creature.metaInfo.name}</color>";

        public static bool TryGetPortrait(this CreatureModel creature, out Sprite portrait)
        {
            portrait = Resources.Load<Sprite>(creature.metaInfo.portraitSrc);

            // TODO generate a custom portrait.
            return portrait != null;
        }

        private static ICreatureExtension BuildAlephExt(CreatureModel model)
        {
            if (model.GetRiskLevel() != 5)
            {
                throw new Exception("Risk level mismatch");
            }
            else if (model.script is Nothing)
            {
                return new NothingExt(model);
            }
            else if (model.script is Censored)
            {
                return new CensoredExt(model);
            }
            else if (model.script is DeathAngel)
            {
                return new WhiteNightExt(model);
            }
            else if (model.script is BlueStar)
            {
                return new BlueStarExt(model);
            }
            return new ExpectGoodAndNormalExt(model);
        }

        private static ICreatureExtension BuildExtension(CreatureModel model)
        {
            switch (model.GetRiskLevel())
            {
                case 1:
                    return BuildZayinExt(model);
                case 2:
                    return BuildTethExt(model);
                case 3:
                    return BuildHeExt(model);
                case 4:
                    return BuildWawExt(model);
                case 5:
                    return BuildAlephExt(model);
                default:
                    return new ExpectGoodAndNormalExt(model);
            }
        }

        private static ICreatureExtension BuildHeExt(CreatureModel model)
        {
            if (model.GetRiskLevel() != 3)
            {
                throw new Exception("Risk level mismatch");
            }
            else if (model.script is HappyTeddy)
            {
                return new HappyTeddyExt(model);
            }
            else if (model.script is LittleWitch)
            {
                return new LaetitiaExt(model);
            }
            else if (model.script is RedShoes)
            {
                return new RedShoesExt(model);
            }
            else if (model.script is SingingMachine)
            {
                return new SingingMachineExt(model);
            }
            else if (model.script is Butterfly)
            {
                return new ButterflyExt(model);
            }
            else if (model.script is Freischutz)
            {
                return new DerFreischutzExt(model);
            }
            else if (model.script is SnowQueen)
            {
                return new SnowQueenExt(model);
            }
            else if (model.script is Scarecrow)
            {
                return new ScarecrowExt(model);
            }
            return new ExpectGoodAndNormalExt(model);
        }

        private static ICreatureKitExtension BuildKitExtension(this CreatureModel model)
        {
            var script = model.script;
            if (script is BigTreeSap) // won't support
            {
            }
            else if (script is DesireHeart)
            {
            }
            else if (script is HealthBracelet)
            {
                return new LuminousBraceletExt(model);
            }
            else if (script is HellTrain)
            {
            }
            else if (script is IronMaiden) // won't support
            {
            }
            else if (script is JusticeReceiver)
            {
                return new BehaviorExt(model);
            }
            else if (script is MeatIdol)
            {
                return new MeatIdolExt(model);
            }
            else if (script is OtherWorldPortrait)
            {
            }
            else if (script is PromiseAndFaith)
            {
            }
            else if (script is ProphecyOfSkin)
            {
            }
            else if (script is ResearcherNote)
            {
                return new ResearcherNoteExt(model);
            }
            else if (script is ResetMirror)
            {
            }
            else if (script is ReverseClock)
            {
            }
            else if (script is Shelter)
            {
                return new ShelterExt(model);
            }
            else if (script is Theresia)
            {
            }
            else if (script is Yang)
            {
            }
            else if (script is YouMustHappy)
            {
                return new YouMustBeHappyExt(model);
            }
            return new BaseKitExt(model);
        }

        private static ICreatureExtension BuildTethExt(CreatureModel model)
        {
            if (model.GetRiskLevel() != 2)
            {
                throw new Exception("Risk level mismatch");
            }
            else if (model.script is ShyThing)
            {
                return new ShyLook(model);
            }
            else if (model.script is BeautyBeast)
            {
                return new BeautyBeastExt(model);
            }
            else if (model.script is StraitJacket)
            {
                return new MurdererExt(model);
            }
            else if (model.script is MatchGirl)
            {
                return new ScorchedGirlExt(model);
            }
            else if (model.script is Baku)
            {
                return new VoidDreamExt(model);
            }
            else if (model.script is BloodBath)
            {
                return new BloodBathExt(model);
            }
            else if (model.script is Sakura)
            {
                return new SakuraExt(model);
            }
            else if (model.script is SmallBird)
            {
                return new PunishingBirdExt(model);
            }
            else if (model.script is OldLady)
            {
                return new OldLadyExt(model);
            }
            else if (model.script is LadyLookingAtWall)
            {
                return new LadyLookingAtWallExt(model);
            }
            else if (model.script is ArmorCreature)
            {
                return new CrumblingArmorExt(model);
            }
            return new ExpectGoodAndNormalExt(model);
        }

        private static ICreatureExtension BuildWawExt(CreatureModel model)
        {
            if (model.GetRiskLevel() != 4)
            {
                throw new Exception("Risk level mismatch");
            }
            else if (model.script is BlackSwan)
            {
                return new BlackSwanExt(model);
            }
            else if (model.script is YoungPrince)
            {
                return new LittlePrinceExt(model);
            }
            else if (model.script is QueenBee)
            {
                return new QueenBeeExt(model);
            }
            else if (model.script is ViscusSnake)
            {
                return new NakedNestExt(model);
            }
            else if (model.script is MagicalGirl) // Require a high work prob agent.
            {
                return new HatredQueen(model);
            }
            else if (model.script is SnowWhite)
            {
                return new SnowWhiteExt(model);
            }
            else if (model.script is LongBird)
            {
                return new JudgementBirdExt(model);
            }
            else if (model.script is FengYun)
            {
                return new CloudedMonkExt(model);
            }
            else if (model.script is FireBird)
            {
                return new FireBirdExt(model);
            }
            else if (model.script is Shark)
            {
                return new DreamingCurrent(model);
            }
            else if (model.script is KnightOfDespair)
            {
                return new KnightOfDespairExt(model);
            }
            else if (model.script is MagicalGirl_2)
            {
                return new KingOfGreedExt(model);
            }
            else if (model.script is Wraith)
            {
                return new DimensionExt(model);
            }
            else
            {
                return new ExpectGoodAndNormalExt(model);
            }
        }

        private static ICreatureExtension BuildZayinExt(CreatureModel model)
        {
            if (model.GetRiskLevel() != 1)
            {
                throw new Exception("Risk level mismatch");
            }
            else if (model.script is OneBadManyGood)
            {
                return new OneBadManyGoodExt(model);
            }
            else if (model.script is Wellcheers)
            {
                return new WellcheersExt(model);
            }
            else if (model.script is DontTouchMe)
            {
                return new DontTouchMeExt(model);
            }
            else if (model.script is Fairy)
            {
                return new FairyFestivalExt(model);
            }
            else if (model.script is PinkCorps)
            {
                return new ArmyInBlackExt(model);
            }
            else if (model.script is Bald)
            {
                return new BaldExt(model);
            }
            return new ExpectGoodAndNormalExt(model);
        }
    }
}