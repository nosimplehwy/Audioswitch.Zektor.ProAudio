using Crestron.RAD.Common.Transports;
using Crestron.RAD.DeviceTypes.AudioVideoSwitcher;

namespace AudioSwitchZektorProAudio
{
    public class ZektorProAudioProtocol : AAudioVideoSwitcherProtocol
    {
        public ZektorProAudioProtocol(ISerialTransport transport, byte id)
        : base(transport, id)
        {
            
        }
    }
}