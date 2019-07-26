using DIH_core.Src.WAV;

namespace DIH_core.Src.MethodsInterface
{
    public interface IHidingMethod
    {
        DigitalAudio Embed(DigitalAudio cover, byte[] message);
        byte[] Extract(DigitalAudio stegoAudio);
        long GetAvailableSpace(DigitalAudio stegoAudio);
        string Description();
    }
}