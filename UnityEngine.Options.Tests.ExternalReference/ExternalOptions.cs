using Unity.Options;

namespace UnityEngine.Options.Tests.ExternalReference
{
    [ProgramOptions]
    public sealed class ExternalOptions
    {
        public static int Value = 7;
    }

    // This is just to make sure this assembly references AnotherReference.dll
    public sealed class UseIt
    {
        public static AnotherReference.OtherOptions Check;
    }
}
