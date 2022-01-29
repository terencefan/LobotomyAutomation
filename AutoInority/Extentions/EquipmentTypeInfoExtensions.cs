using System.Collections.Generic;

namespace AutoInority.Extentions
{
    public static class EquipmentTypeInfoExtensions
    {
        private static readonly Dictionary<int, int> _egoPriority = new Dictionary<int, int>()
        {
#region Hat

            { 400064, 5 }, // Pink Corps
            { 400026, 4 }, // Queen Bee
            { 400008, 4 }, // Big Bird
            { 400055, 4 }, // Wraith
            { 400062, 3 }, // Yggdrasil
            { 400041, 3 }, // Laetitia
            { 400044, 3 }, // Bloody Tree

#endregion Hat
        };

        public static int GetPriority(this EquipmentTypeInfo equipment)
        {
            if (_egoPriority.TryGetValue(equipment.id, out var v))
            {
                return v;
            }
            return 1;
        }

        public static string Tag(this EquipmentTypeInfo equipment) => $"<color=#efca83>{equipment.Name}</color>";
    }
}