namespace BouyomiPlugin;

[Serializable]
public class TalkException : Exception
{
    public TalkException() { }
    public TalkException(string message) : base(message) { }
    public TalkException(string message, Exception inner) : base(message, inner) { }
}
