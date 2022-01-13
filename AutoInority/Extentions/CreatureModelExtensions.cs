using UnityEngine;

namespace AutoInority
{
    public static class CreatureModelExtensions
    {
        public static bool Available(this CreatureModel creature)
        {
            return creature.state == CreatureState.WAIT && creature.feelingState == CreatureFeelingState.NONE;
        }

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

        public static string Tag(this CreatureModel creature) => $"<color=#ef9696>{creature.metaInfo.name}</color>";
    }
}
