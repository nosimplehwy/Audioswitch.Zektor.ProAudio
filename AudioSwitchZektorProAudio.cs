using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Crestron.RAD.Common.BasicDriver;
using Crestron.RAD.Common.Enums;
using Crestron.RAD.Common.Events;
using Crestron.RAD.Common.Interfaces;
using Crestron.SimplSharp;
using Crestron.SimplSharp.Reflection;
using Crestron.RAD.DeviceTypes.AudioVideoSwitcher;
using Crestron.RAD.Common.Transports;
using Independentsoft.Exchange;

namespace AudioSwitchZektorProAudio
{
    public class AudioSwitchZektorProAudio : AAudioVideoSwitcher, ITcp
    {
        public AudioSwitchZektorProAudio()
        {
            
        }

        #region ITcp Members

        public void Initialize(IPAddress ipAddress, int port)
        {
            var tcpTransport = new TcpTransport()
            {
                EnableAutoReconnect = EnableAutoReconnect,
                EnableLogging = InternalEnableLogging,
                CustomLogger = InternalCustomLogger,
                EnableRxDebug = InternalEnableRxDebug,
                EnableTxDebug = InternalEnableTxDebug
            };

            tcpTransport.Initialize(ipAddress,port);
            ConnectionTransport = tcpTransport;

            AudioVideoSwitcherProtocol = new ZektorProAudioProtocol(ConnectionTransport, Id)
            {
                EnableLogging = InternalEnableLogging,
                CustomLogger = InternalCustomLogger
            };
            AudioVideoSwitcherProtocol.RxOut += SendRxOut;
            AudioVideoSwitcherProtocol.Initialize(AudioVideoSwitcherData);
        }

        #endregion

    }
}
