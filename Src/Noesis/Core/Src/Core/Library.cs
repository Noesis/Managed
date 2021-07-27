namespace Noesis
{
    public class Library
    {
#if (UNITY_IOS || UNITY_TVOS || UNITY_WEBGL || UNITY_SWITCH) && !UNITY_EDITOR
        public const string Name = "__Internal";
#else
        public const string Name = "Noesis";
#endif
    }
}