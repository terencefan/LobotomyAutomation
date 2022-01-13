namespace AutoInority
{
    public static class SkillTypeInfoExtensions
    {
        public static string Tag(this SkillTypeInfo skill) => $"<color=#84bd36>{skill.calledName}</color>";
    }
}
