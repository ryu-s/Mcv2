namespace BouyomiPlugin;

interface ITalker : IDisposable
{
    void TalkText(string text);

    void TalkText(string text, Int16 voiceSpeed, Int16 voiceTone, Int16 voiceVolume, FNF.Utility.VoiceType voiceType);
}
