using Unity.Options;

namespace UnityEngine.Options.Tests.AnotherReference
{
    [ProgramOptions]
    public sealed class OtherOptions
    {
        public static string Name = "ralph";
    }

    [ProgramOptions]
    public sealed class OtherOptions2
    {
        public static string Animal = "dog";
    }
}
