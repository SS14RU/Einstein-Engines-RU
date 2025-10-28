using Content.Shared._White.Bark;
using Content.Shared._White.Bark.Systems;
using Robust.Shared.Configuration;

namespace Content.Shared._White.CCVar;

public sealed partial class WhiteCVars
{
    /// <summary>
    /// Voice type for characters in-game (TTS / bark / none).
    /// </summary>
    public static readonly CVarDef<CharacterVoiceType> VoiceType =
        CVarDef.Create("voice.type", CharacterVoiceType.TTS, CVar.CLIENTONLY | CVar.ARCHIVE);
}
