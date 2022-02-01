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

            #region Ribborn

            { 400050, 3 }, // Scarecrow
            { 400031, 4 }, // Galaxy Boy

            #endregion Neck

            #region Cheek

            { 400005, 5 }, // Nothing

            #endregion Cheek

            #region Chest

            { 400023, 5 }, // Snow White's Apple

            #endregion

            #region Glove

            { 400049, 4 }, // Look At Me
            { 400040, 5 }, // Little Prince
            { 400046, 5 }, // Viscus Snake

            #endregion

            #region Eye

            { 400035, 4 }, // Judgement Bird
            { 400019, 5 }, // Silent Orchestra
            { 400056, 5 }, // Censored
            { 400042, 5 }, // Smiling Body Mountain

            #endregion
        };

        public static int GetPriority(this EquipmentTypeInfo equipment) => _egoPriority.TryGetValue(equipment.id, out int v) ? v : 1;

        public static string Tag(this EquipmentTypeInfo equipment) => $"<color=#efca83>{equipment.Name}</color>";
    }
}