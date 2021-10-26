using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using AudioSwitchZektorProAudio;
using Crestron.DeviceDrivers.Core.Capabilities;
using Crestron.RAD.Common.BasicDriver;
using Crestron.RAD.Common.Enums;
using Crestron.RAD.Common.Interfaces;
using Crestron.RAD.Common.Logging;
using Crestron.RAD.Common.Transports;
using Crestron.RAD.DeviceTypes.AudioVideoSwitcher;
using Crestron.RAD.DeviceTypes.AudioVideoSwitcher.Extender;
using Crestron.RAD.DeviceTypes.AudioVideoSwitcher.RootObject;
using Crestron.RAD.Drivers.AVReceivers;
using Crestron.RAD.Drivers.AVReceivers.SoundUnited;

namespace AudioSwitchZektorProAudio
{
    public class ZektorProAudioProtocol : AAudioVideoSwitcherProtocol
    {
        private const string ApiDelimiter = "$";
        private const double VolumeStep = 1.0;
        private const string _delimiterPattern = "($)";

        private Dictionary<string,SoundUnitedVolumeController> _controllers;
        private StringBuilder _responseBuffer = new StringBuilder();

        private bool _disposed;
        //private bool _initialized;

        public ZektorProAudioProtocol(ISerialTransport transport, byte id)
        : base(transport, id)
        {
        }

        public override void Initialize(object driverData)
        {
            base.Initialize(driverData);

            _controllers = new Dictionary<string, SoundUnitedVolumeController>()
            {
                {"1",new SoundUnitedVolumeController(Zone1IsMuted,ChangeZone1Volume, VolumeStep, TimeBetweenCommands)},
                {"2",new SoundUnitedVolumeController(Zone2IsMuted,ChangeZone2Volume, VolumeStep, TimeBetweenCommands)},
                {"3",new SoundUnitedVolumeController(Zone3IsMuted,ChangeZone3Volume, VolumeStep, TimeBetweenCommands)},
                {"4",new SoundUnitedVolumeController(Zone4IsMuted,ChangeZone4Volume, VolumeStep, TimeBetweenCommands)},
                {"5",new SoundUnitedVolumeController(Zone5IsMuted,ChangeZone5Volume, VolumeStep, TimeBetweenCommands)},
                {"6",new SoundUnitedVolumeController(Zone6IsMuted,ChangeZone6Volume, VolumeStep, TimeBetweenCommands)},
                {"7",new SoundUnitedVolumeController(Zone7IsMuted,ChangeZone7Volume, VolumeStep, TimeBetweenCommands)},
                {"8",new SoundUnitedVolumeController(Zone8IsMuted,ChangeZone8Volume, VolumeStep, TimeBetweenCommands)},
                {"9",new SoundUnitedVolumeController(Zone9IsMuted,ChangeZone9Volume, VolumeStep, TimeBetweenCommands)},
                {"10",new SoundUnitedVolumeController(Zone10IsMuted,ChangeZone10Volume, VolumeStep, TimeBetweenCommands)},
                {"11",new SoundUnitedVolumeController(Zone11IsMuted,ChangeZone11Volume, VolumeStep, TimeBetweenCommands)},
                {"12",new SoundUnitedVolumeController(Zone12IsMuted,ChangeZone12Volume, VolumeStep, TimeBetweenCommands)},
                {"13",new SoundUnitedVolumeController(Zone13IsMuted,ChangeZone13Volume, VolumeStep, TimeBetweenCommands)},
                {"14",new SoundUnitedVolumeController(Zone14IsMuted,ChangeZone14Volume, VolumeStep, TimeBetweenCommands)},
                {"15",new SoundUnitedVolumeController(Zone15IsMuted,ChangeZone15Volume, VolumeStep, TimeBetweenCommands)},
                {"16",new SoundUnitedVolumeController(Zone16IsMuted,ChangeZone16Volume, VolumeStep, TimeBetweenCommands)},
                {"17",new SoundUnitedVolumeController(Zone17IsMuted,ChangeZone17Volume, VolumeStep, TimeBetweenCommands)},
                {"18",new SoundUnitedVolumeController(Zone18IsMuted,ChangeZone18Volume, VolumeStep, TimeBetweenCommands)},
                {"19",new SoundUnitedVolumeController(Zone19IsMuted,ChangeZone19Volume, VolumeStep, TimeBetweenCommands)},
                {"20",new SoundUnitedVolumeController(Zone20IsMuted,ChangeZone20Volume, VolumeStep, TimeBetweenCommands)},
                {"21",new SoundUnitedVolumeController(Zone21IsMuted,ChangeZone21Volume, VolumeStep, TimeBetweenCommands)},
                {"22",new SoundUnitedVolumeController(Zone22IsMuted,ChangeZone22Volume, VolumeStep, TimeBetweenCommands)},
                {"23",new SoundUnitedVolumeController(Zone23IsMuted,ChangeZone23Volume, VolumeStep, TimeBetweenCommands)},
                {"24",new SoundUnitedVolumeController(Zone24IsMuted,ChangeZone24Volume, VolumeStep, TimeBetweenCommands)},
                {"25",new SoundUnitedVolumeController(Zone25IsMuted,ChangeZone25Volume, VolumeStep, TimeBetweenCommands)},
                {"26",new SoundUnitedVolumeController(Zone26IsMuted,ChangeZone26Volume, VolumeStep, TimeBetweenCommands)},
                {"27",new SoundUnitedVolumeController(Zone27IsMuted,ChangeZone27Volume, VolumeStep, TimeBetweenCommands)},
                {"28",new SoundUnitedVolumeController(Zone28IsMuted,ChangeZone28Volume, VolumeStep, TimeBetweenCommands)},
                {"29",new SoundUnitedVolumeController(Zone29IsMuted,ChangeZone29Volume, VolumeStep, TimeBetweenCommands)},
                {"30",new SoundUnitedVolumeController(Zone30IsMuted,ChangeZone30Volume, VolumeStep, TimeBetweenCommands)},
                {"31",new SoundUnitedVolumeController(Zone31IsMuted,ChangeZone31Volume, VolumeStep, TimeBetweenCommands)},
                {"32",new SoundUnitedVolumeController(Zone32IsMuted,ChangeZone32Volume, VolumeStep, TimeBetweenCommands)},
            };

            foreach (var controller in _controllers)
            {
               _controllers[controller.Key].VolumeLevel.PercentChanged  += VolumeLevel_PercentChanged;
            }

        }

        private void ZektorProAudioProtocol_RoutableOutputsChanged(object sender, Crestron.RAD.Common.Events.ListChangedEventArgs<IAudioVideoExtender> e)
        {
            throw new NotImplementedException();
        }

        public override void Dispose()
        {
            if (!_disposed)
            {
                foreach (var controller in _controllers)
                {
                    if (_controllers[controller.Key] != null)
                    {
                        _controllers[controller.Key].VolumeLevel.PercentChanged -= VolumeLevel_PercentChanged;
                        _controllers[controller.Key].Dispose();

                    }
                }

                _disposed = true;
            }

            base.Dispose();
        }

        public override void ExtenderSetVolume(AudioVideoExtender extender, uint volume)
        {
            //base.ExtenderSetVolume(extender, volume);
            DriverLog.Log(EnableLogging, Log, LoggingLevel.Debug, "ExtenderSetVolume", String.Format($"{extender.ApiIdentifier}, vol {volume}"));
            VolumeControllerForCommand(extender.ApiIdentifier).VolumeLevel.Percent = volume;
        }

        private void VolumeLevel_PercentChanged(object sender, Crestron.RAD.Ext.Util.Scaling.LevelChangedEventArgs<uint> e)
        {
            DriverLog.Log(EnableLogging, Log, LoggingLevel.Debug, "VolumePercentChanged", String.Format($"{e}"));
        }


        #region ChangeVolumeHelpers
        private void ChangeZone1Volume(double volume)
        {
            ZoneChangeVolume("1",volume);
        }
        private void ChangeZone2Volume(double volume)
        {
            ZoneChangeVolume("2", volume);
        }
        private void ChangeZone3Volume(double volume)
        {
            ZoneChangeVolume("3", volume);
        }
        private void ChangeZone4Volume(double volume)
        {
            ZoneChangeVolume("4", volume);
        }
        private void ChangeZone5Volume(double volume)
        {
            ZoneChangeVolume("5", volume);
        }
        private void ChangeZone6Volume(double volume)
        {
            ZoneChangeVolume("6", volume);
        }
        private void ChangeZone7Volume(double volume)
        {
            ZoneChangeVolume("7", volume);
        }
        private void ChangeZone8Volume(double volume)
        {
            ZoneChangeVolume("8", volume);
        }
        private void ChangeZone9Volume(double volume)
        {
            ZoneChangeVolume("9", volume);
        }
        private void ChangeZone10Volume(double volume)
        {
            ZoneChangeVolume("10", volume);
        }
        private void ChangeZone11Volume(double volume)
        {
            ZoneChangeVolume("11", volume);
        }
        private void ChangeZone12Volume(double volume)
        {
            ZoneChangeVolume("12", volume);
        }
        private void ChangeZone13Volume(double volume)
        {
            ZoneChangeVolume("13", volume);
        }
        private void ChangeZone14Volume(double volume)
        {
            ZoneChangeVolume("14", volume);
        }
        private void ChangeZone15Volume(double volume)
        {
            ZoneChangeVolume("15", volume);
        }
        private void ChangeZone16Volume(double volume)
        {
            ZoneChangeVolume("16", volume);
        }
        private void ChangeZone17Volume(double volume)
        {
            ZoneChangeVolume("17", volume);
        }
        private void ChangeZone18Volume(double volume)
        {
            ZoneChangeVolume("18", volume);
        }
        private void ChangeZone19Volume(double volume)
        {
            ZoneChangeVolume("19", volume);
        }
        private void ChangeZone20Volume(double volume) => ZoneChangeVolume("20", volume);
        private void ChangeZone21Volume(double volume) => ZoneChangeVolume("21", volume);
        private void ChangeZone22Volume(double volume) => ZoneChangeVolume("22", volume);
        private void ChangeZone23Volume(double volume) => ZoneChangeVolume("23", volume);
        private void ChangeZone24Volume(double volume) => ZoneChangeVolume("24", volume);
        private void ChangeZone25Volume(double volume) => ZoneChangeVolume("25", volume);
        private void ChangeZone26Volume(double volume) => ZoneChangeVolume("26", volume);

        private void ChangeZone27Volume(double volume) => ZoneChangeVolume("27", volume);

        private void ChangeZone28Volume(double volume) => ZoneChangeVolume("28", volume);
        private void ChangeZone29Volume(double volume) => ZoneChangeVolume("29", volume);
        private void ChangeZone30Volume(double volume) => ZoneChangeVolume("30", volume);
        private void ChangeZone31Volume(double volume) => ZoneChangeVolume("31", volume);
        private void ChangeZone32Volume(double volume) => ZoneChangeVolume("32", volume);

        #endregion

        #region IsMuted Helpers

        private bool Zone1IsMuted()
        {
            var extender = GetExtenderByApiIdentifier("1");
            return extender.Muted;
        }
        private bool Zone2IsMuted()
        {
            var extender = GetExtenderByApiIdentifier("2");
            return extender.Muted;
        }
        private bool Zone3IsMuted()
        {
            var extender = GetExtenderByApiIdentifier("3");
            return extender.Muted;
        }
        private bool Zone4IsMuted()
        {
            var extender = GetExtenderByApiIdentifier("4");
            return extender.Muted;
        }
        private bool Zone5IsMuted()
        {
            var extender = GetExtenderByApiIdentifier("5");
            return extender.Muted;
        }
        private bool Zone6IsMuted()
        {
            var extender = GetExtenderByApiIdentifier("6");
            return extender.Muted;
        }
        private bool Zone7IsMuted()
        {
            var extender = GetExtenderByApiIdentifier("7");
            return extender.Muted;
        }
        private bool Zone8IsMuted()
        {
            var extender = GetExtenderByApiIdentifier("8");
            return extender.Muted;
        }
        private bool Zone9IsMuted()
        {
            var extender = GetExtenderByApiIdentifier("9");
            return extender.Muted;
        }
        private bool Zone10IsMuted()
        {
            var extender = GetExtenderByApiIdentifier("10");
            return extender.Muted;
        }
        private bool Zone11IsMuted()
        {
            var extender = GetExtenderByApiIdentifier("11");
            return extender.Muted;
        }
        private bool Zone12IsMuted()
        {
            var extender = GetExtenderByApiIdentifier("12");
            return extender.Muted;
        }
        private bool Zone13IsMuted()
        {
            var extender = GetExtenderByApiIdentifier("13");
            return extender.Muted;
        }
        private bool Zone14IsMuted()
        {
            var extender = GetExtenderByApiIdentifier("14");
            return extender.Muted;
        }
        private bool Zone15IsMuted()
        {
            var extender = GetExtenderByApiIdentifier("15");
            return extender.Muted;
        }
        private bool Zone16IsMuted()
        {
            var extender = GetExtenderByApiIdentifier("16");
            return extender.Muted;
        }
        private bool Zone17IsMuted()
        {
            var extender = GetExtenderByApiIdentifier("17");
            return extender.Muted;
        }
        private bool Zone18IsMuted()
        {
            var extender = GetExtenderByApiIdentifier("18");
            return extender.Muted;
        }
        private bool Zone19IsMuted()
        {
            var extender = GetExtenderByApiIdentifier("19");
            return extender.Muted;
        }
        private bool Zone20IsMuted()
        {
            var extender = GetExtenderByApiIdentifier("20");
            return extender.Muted;
        }
        private bool Zone21IsMuted()
        {
            var extender = GetExtenderByApiIdentifier("21");
            return extender.Muted;
        }
        private bool Zone22IsMuted()
        {
            var extender = GetExtenderByApiIdentifier("22");
            return extender.Muted;
        }
        private bool Zone23IsMuted()
        {
            var extender = GetExtenderByApiIdentifier("23");
            return extender.Muted;
        }
        private bool Zone24IsMuted()
        {
            var extender = GetExtenderByApiIdentifier("24");
            return extender.Muted;
        }
        private bool Zone25IsMuted()
        {
            var extender = GetExtenderByApiIdentifier("25");
            return extender.Muted;
        }
        private bool Zone26IsMuted()
        {
            var extender = GetExtenderByApiIdentifier("26");
            return extender.Muted;
        }
        private bool Zone27IsMuted()
        {
            var extender = GetExtenderByApiIdentifier("27");
            return extender.Muted;
        }
        private bool Zone28IsMuted()
        {
            var extender = GetExtenderByApiIdentifier("28");
            return extender.Muted;
        }
        private bool Zone29IsMuted()
        {
            var extender = GetExtenderByApiIdentifier("29");
            return extender.Muted;
        }
        private bool Zone30IsMuted()
        {
            var extender = GetExtenderByApiIdentifier("30");
            return extender.Muted;
        }
        private bool Zone31IsMuted()
        {
            var extender = GetExtenderByApiIdentifier("31");
            return extender.Muted;
        }
        private bool Zone32IsMuted()
        {
            var extender = GetExtenderByApiIdentifier("32");
            return extender.Muted;
        }

        #endregion

        private void ZoneChangeVolume(string zone, double volume)
        {
            DriverLog.Log(EnableLogging, Log, LoggingLevel.Debug, "ChangeZoneVolume", String.Format($"zone {zone},vol {volume}"));

            // Input volume is already in correct scale, just need to cast it
            uint vol = (uint)volume;
            var command = new CommandSet(
                zone,
                string.Format($"^VPZ @{zone},{vol}$"),
                CommonCommandGroupType.AudioVideoExtender,
                null,
                false,
                CommandPriority.Normal,
                StandardCommandsEnum.Vol)
            {
                AllowIsQueueableOverride = true,
                AllowIsSendableOverride = true,
                AllowRemoveCommandOverride = true
            };

            // Notify volume controller that we're sending a volume command
            // which means it needs to ensure we poll later even if this
            // command is not queued. Otherwise, fake feedback will leave the
            // application with the wrong volume value.
            MuteVolControllerForCommand(zone).StartControllingVolume();

            SendCommand(command);
        }

        private SoundUnitedMuteVolController MuteVolControllerForCommand(string zone)
        {
            DriverLog.Log(EnableLogging, Log, LoggingLevel.Debug, "MuteVolControllerForCommand", "");
            return VolumeControllerForCommand(zone).MuteVol;
        }

        private SoundUnitedVolumeController VolumeControllerForCommand(string zone)
        {
            // Main zone commands use the CommonCommandGroupType for the command
            // Ones for zones use the zone-specific group, so default to the main
            // zone and return the other zone controllers only for those specific
            // command groups
            DriverLog.Log(EnableLogging, Log, LoggingLevel.Debug, "VolControllerForCommand", "");
            SoundUnitedVolumeController controller = _controllers[zone];
            //TODO need a list of volume controllers

            return controller;
        }

        protected override bool PrepareStringThenSend(CommandSet commandSet)
        {
            DriverLog.Log(EnableLogging, Log, LoggingLevel.Debug, "PrepareStringThenSend", "");
            if (!commandSet.CommandPrepared)
            {
                DriverLog.Log(EnableLogging, Log, LoggingLevel.Debug, "PrepareStringThenSend", String.Format($"Switch {commandSet.StandardCommand.ToString()}"));
                // Attach callback functions to certain commands
                switch (commandSet.StandardCommand)
                {

                    // Track when the last mute command was sent.
                    case StandardCommandsEnum.Mute:
                    case StandardCommandsEnum.MuteOff:
                    case StandardCommandsEnum.MuteOn:
                        {
                            // At the time of writing, the framework does not use this callback, but we
                            // use the chained calls anyway in case the framework later is updated
                            // to need the callback, too.
                            //Action callback = MuteVolControllerForCommand(commandSet.CommandGroup).MuteCommandChanged;
                            //commandSet.CallBack = ActionSequence.Chain(callback, commandSet.CallBack);
                        }
                        break;

                    // Track volume commands because if mute is on while they
                    // are sent, the device will unmute and may miss a second
                    // volume command immediately after it during the unmuting process
                    case StandardCommandsEnum.Vol:
                    case StandardCommandsEnum.VolPlus:
                    case StandardCommandsEnum.VolMinus:
                        {
                            // At the time of writing, the framework does not use this callback, but we
                            // use the chained calls anyway in case the framework later is updated
                            // to need the callback, too.
                           // Action callback = MuteVolControllerForCommand(commandSet.CommandGroup).VolCommandSent;
                           // commandSet.CallBack = ActionSequence.Chain(callback, commandSet.CallBack);
                        }
                        break;

                    // Also need to know when PowerOn commands are sent to
                    // appropriately space out power-on commands for zones
                    case StandardCommandsEnum.PowerOn:
                        {
                            // First use our PowerOnCommandCallback, then call the
                            // default callback from the framework to complete warmup
                            //commandSet.CallBack = ActionSequence.Chain(PowerOnCommandCallback, commandSet.CallBack);
                        }
                        break;

                    default:
                        break;
                }

                commandSet.Command = commandSet.Command;
                commandSet.CommandPrepared = true;
            }

            return base.PrepareStringThenSend(commandSet);
        }

        // Override this to delay sending certain commands until the device is
        // ready to receive them
        protected override bool CanSendCommand(CommandSet commandSet)
        {
            bool canSend;
            DriverLog.Log(EnableLogging, Log, LoggingLevel.Debug, "CanSendCommand", "");

            canSend = base.CanSendCommand(commandSet);

            if (canSend)
            {
                // Check if the command is sendable based on device-specific
                // workaround requirements
                switch (commandSet.StandardCommand)
                {
                    // Check if we can send mute commands
                    case StandardCommandsEnum.Mute:
                    case StandardCommandsEnum.MuteOff:
                    case StandardCommandsEnum.MuteOn:
                        canSend = MuteVolControllerForCommand(commandSet.CommandName).CanSendMute;
                        break;

                    // Check if volume commands are sendable
                    case StandardCommandsEnum.Vol:
                    case StandardCommandsEnum.VolPlus:
                    case StandardCommandsEnum.VolMinus:
                       canSend = MuteVolControllerForCommand(commandSet.CommandName).CanSendVol;
                        break;



                    default:
                        break;
                }
            }

            return canSend;
        }

        protected override bool CanQueueCommand(CommandSet commandSet, bool powerOnCommandExistsInQueue)
        {
            DriverLog.Log(EnableLogging, Log, LoggingLevel.Debug, "CanQueueCommand", "");
        
            return base.CanQueueCommand(commandSet, powerOnCommandExistsInQueue);
        }

        protected override void DeConstructExtenderMute(AudioVideoExtender extender, string response)
        {
            base.DeConstructExtenderMute(extender, response);
            DriverLog.Log(EnableLogging, Log, LoggingLevel.Debug, "DeConstructExtenderMute", String.Format($"Extender{extender.ApiIdentifier} Mute State Response:{response}"));

        }

        protected override void DeConstructExtenderVolume(AudioVideoExtender extender, string response)
        {
            base.DeConstructExtenderVolume(extender, response);
            DriverLog.Log(EnableLogging, Log, LoggingLevel.Debug, "DeConstructExtenderVolume", String.Format($"Extender{extender.ApiIdentifier} Response:{response}"));
        }

        public override void DataHandler(string rx)
        {
            DriverLog.Log(EnableLogging, Log, LoggingLevel.Debug, "DataHandler", rx);

            // Split all received messages so that ResponseValidator gets one response at a time
            // Handle case where response doesn't end in delimiter but contains it
            if (rx.Contains(ApiDelimiter))
            {
                _responseBuffer.Append(rx);
                var splitResponses = _responseBuffer.ToString().Split(new string[]{"$\r"},StringSplitOptions.None);
                _responseBuffer.Length = 0;

                for (int i = 0; i < splitResponses.Length; i++)
                {
                    if(splitResponses[i].Contains("^="))
                     base.DataHandler(splitResponses[i]);
                }
            }
            else
            {
                _responseBuffer.Append(rx);
            }


        }

        protected override void ChooseDeconstructMethod(ValidatedRxData validatedData)
        {
            DriverLog.Log(EnableLogging, Log, LoggingLevel.Debug, "ChooseDeconstructMethod", "");
            base.ChooseDeconstructMethod(validatedData);
        }

        protected override ValidatedRxData ResponseValidator(string response, CommonCommandGroupType commandGroup)
        {
            DriverLog.Log(EnableLogging, Log, LoggingLevel.Debug, "ResponseValidator", String.Format($"Response Received: {response}"));
            response = response.Trim();
        
            //Return with base call
            return base.ResponseValidator(response, commandGroup);
        }

        protected override void DeConstructSwitcherRoute(string response)
        {
            // Receiving: ^=SZ @001,001$

            var routePath = response.Split(',');
            AudioVideoExtender inputExtender = null;
            AudioVideoExtender outputExtender = null;


            // We can get the extender objects here using the API identifier set
            // in the embedded file.
            // We can also get the extender objects by their unique ID using GetExtenderById
            try
            {
                var output = Int32.Parse(routePath[0]);
                var input = routePath[1].Substring(0, routePath[1].Length - 1);
                outputExtender = GetExtenderByApiIdentifier(output.ToString());
                inputExtender = routePath.Length > 1 ? GetExtenderByApiIdentifier(input) : null;
                DriverLog.Log(EnableLogging, Log, LoggingLevel.Debug, "DeConstructSwitcherRoute", String.Format($"Input {input} is routed to Output {output}"));

            }
            catch (Exception exception)
            {
                DriverLog.Log(EnableLogging, Log, LoggingLevel.Error, "DeConstructSwitcherRoute", exception.ToString());
            }

            // Figured out which input is routed to the specified output
            // Now update the output extender with the current source routed to it
            // The framework will figure out if this was a real change or not if it is not done here.
            if (outputExtender != null)
            {
                outputExtender.AudioSourceExtenderId = inputExtender?.Id;
            }

        }

        private void PollExtenderRoute(string zone)
        {
            DriverLog.Log(EnableLogging, Log, LoggingLevel.Debug, "PollExtenderRoute", String.Format($"zone {zone}"));

            var command = new CommandSet(
                String.Format($"Poll zone {zone} route"),
                string.Format($"^SZ @{zone},?$"),
                CommonCommandGroupType.AudioVideoExtender,
                null,
                false,
                CommandPriority.Normal,
                StandardCommandsEnum.AudioInputPoll)
            {
                AllowIsQueueableOverride = true,
                AllowIsSendableOverride = true,
                AllowRemoveCommandOverride = true
            };


            SendCommand(command);
        }


        protected override void Poll()
        {
            //poll extender routes
            //for (int output = 1; output <= 32; output++)
            //{
            //    if(GetExtenderByApiIdentifier(output.ToString()) != null ? 
            //}
            base.Poll();
        }
    }
}