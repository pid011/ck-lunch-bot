namespace CKLunchBot.Runner;

[Serializable]
public class EnvNotFoundException : Exception
{
    public string EnvKey { get; }

    public EnvNotFoundException(string envKey)
    {
        EnvKey = envKey;
    }
}
