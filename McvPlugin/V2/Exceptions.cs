namespace Mcv.PluginV2;

[Serializable]
public class ChromeCookiesFileNotFoundException : Exception
{
    public ChromeCookiesFileNotFoundException(string message)
        : base(message)
    { }

    public ChromeCookiesFileNotFoundException(string message, Exception innerException)
        : base(message, innerException)
    { }
}
[Serializable]
public class FirefoxProfileIniNotFoundException : Exception
{
    public FirefoxProfileIniNotFoundException(string message)
        : base(message)
    { }

    public FirefoxProfileIniNotFoundException(string message, Exception innerException)
        : base(message, innerException)
    { }
}
