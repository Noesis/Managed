namespace Noesis
{
    public class Library
    {
#if (UNITY_IOS || UNITY_WEBGL) && !UNITY_EDITOR
        public const string Name = "__Internal";
#else
        public const string Name = "Noesis";
#endif
    }
}