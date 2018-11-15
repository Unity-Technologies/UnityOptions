# UnityOptions

A library to help make command line argument parsing easy

```

public static void Main(string[] args)
{
    if (!Options.InitAndSetup(args))
        return;

    ...
    Do stuff
    ...
}

[ProgramOptions]
public static class Options
{
    [HelpDetails("This is the first option")]
    public static string SomeValue;

    [HelpDetails("This is the first option")]
    public static string AnotherValue;

    public static void SetToDefaults()
    {
    }

    public static string NameFor(string fieldName)
    {
        return OptionsParser.OptionNameFor(typeof(Options), fieldName);
    }

    public static bool InitAndSetup(string[] args)
    {
        SetToDefaults();

        if (OptionsParser.HelpRequested(args))
        {
            OptionsParser.DisplayHelp(typeof(Program).Assembly, false);
            return false;
        }

        var unknownArgs = OptionsParser.Prepare(args, typeof(Program).Assembly, false).ToList();

        if (unknownArgs.Count > 0)
        {
            Console.WriteLine("Unknown arguments : ");
            foreach (var remain in unknownArgs)
            {
                Console.WriteLine("\t {0}", remain);
            }

            return false;
        }

        ValidateArguments();

        return true;
    }
}
```
