using System;
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
        //internal const string ApiDelimiter = "$";
        private const double VolumeStep = 1.0;

        private SoundUnitedVolumeController _zone1VolumeController;
        //private SoundUnitedVolumeController _zone2VolumeController;
        //private SoundUnitedVolumeController _zone3VolumeController;
        //private SoundUnitedVolumeController _zone4VolumeController;

        //private bool _disposed;
        //private bool _initialized;

        public ZektorProAudioProtocol(ISerialTransport transport, byte id)
        : base(transport, id)
        {
        }

        public override void Initialize(object driverData)
        {
            base.Initialize(driverData);

            _zone1VolumeController =
                new SoundUnitedVolumeController(Zone1IsMuted, ChangeZone1Volume, VolumeStep, TimeBetweenCommands);
            //_zone1VolumeController.VolumeLevel.PercentChanged += VolumeLevel_PercentChanged;
        }



        public override void ExtenderSetVolume(AudioVideoExtender extender, uint volume)
        {
            //base.ExtenderSetVolume(extender, volume);
            DriverLog.Log(EnableLogging, Log, LoggingLevel.Debug, "ExtenderSetVolume", String.Format($"{extender.ToString()}, vol {volume}"));
            try
            {
                var zone = Int32.Parse(extender.ApiIdentifier);
                DriverLog.Log(EnableLogging, Log, LoggingLevel.Debug, "ExtenderSetVolume", String.Format($"zone {zone}, vol {volume}"));
                VolumeControllerForCommand(zone).VolumeLevel.Percent = volume;
            }
            catch (Exception e)
            {
                DriverLog.Log(EnableLogging, Log, LoggingLevel.Error, "ExtenderSetVolume", String.Format($"Could not parse extender id, {e}"));
            }
        }

        private void VolumeLevel_PercentChanged(object sender, Crestron.RAD.Ext.Util.Scaling.LevelChangedEventArgs<uint> e)
        {
            DriverLog.Log(EnableLogging, Log, LoggingLevel.Debug, "Volume1PercentChanged", String.Format($"{e}"));
        }

        private bool Zone1IsMuted()
        {
            return false;
        }

        private void ChangeZone1Volume(double volume)
        {
            DriverLog.Log(EnableLogging, Log, LoggingLevel.Debug, "ChangeZone1Volume", String.Format($"vol {volume}"));
            ZoneChangeVolume(1, volume);
        }
        private void ZoneChangeVolume(int zone, double volume)
        {
            DriverLog.Log(EnableLogging, Log, LoggingLevel.Debug, "ChangeZoneVolume", String.Format($"zone {zone},vol {volume}"));

            // Input volume is already in correct scale, just need to cast it
            uint vol = (uint)volume;
            var command = new CommandSet(
                string.Format($"Zone{zone}SetVolume {vol}"),
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


        private SoundUnitedMuteVolController MuteVolControllerForCommand(int zone)
        {
            DriverLog.Log(EnableLogging, Log, LoggingLevel.Debug, "MuteVolControllerForCommand", "");
            return VolumeControllerForCommand(zone).MuteVol;
        }

        private SoundUnitedVolumeController VolumeControllerForCommand(int zone)
        {
            // Main zone commands use the CommonCommandGroupType for the command
            // Ones for zones use the zone-specific group, so default to the main
            // zone and return the other zone controllers only for those specific
            // command groups
            DriverLog.Log(EnableLogging, Log, LoggingLevel.Debug, "VolControllerForCommand", "");
            SoundUnitedVolumeController controller = _zone1VolumeController;
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
                            Action callback = MuteVolControllerForCommand(commandSet.CommandGroup).MuteCommandChanged;
                            commandSet.CallBack = ActionSequence.Chain(callback, commandSet.CallBack);
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
                            Action callback = MuteVolControllerForCommand(commandSet.CommandGroup).VolCommandSent;
                            commandSet.CallBack = ActionSequence.Chain(callback, commandSet.CallBack);
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

                commandSet.Command = commandSet.Command + ApiDelimiter;
                commandSet.CommandPrepared = true;
            }

            return base.PrepareStringThenSend(commandSet);
        }

        // Override this to delay sending certain commands until the device is
        // ready to receive them
        protected override bool CanSendCommand(CommandSet commandSet)
        {
            string unused;
            bool canSend;

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
                        canSend = MuteVolControllerForCommand(commandSet.CommandGroup).CanSendMute;
                        break;

                    // Check if volume commands are sendable
                    case StandardCommandsEnum.Vol:
                    case StandardCommandsEnum.VolPlus:
                    case StandardCommandsEnum.VolMinus:
                        canSend = MuteVolControllerForCommand(commandSet.CommandGroup).CanSendVol;
                        break;



                    default:
                        break;
                }
            }

            return canSend;
        }

        // Override this because the default implementation does not check if
        // a power-on command exists in the queue for the main zone, so the
        // default implementation will drop input-switch commands if the main
        // zone power-on command is queued since another zone was sent first
        // Also note that since we don't know which power-on command is in the
        // queue, we could potentally allow queueing an input command for the
        // main zone because an alternate zone's power on command is in the
        // queue. The only workaround would be a shadow copy of the "power on
        // command is in the queue" information which is just not worth it to
        // attempt to keep track of.
        protected override bool CanQueueCommand(CommandSet commandSet, bool powerOnCommandExistsInQueue)
        {
            string unused;
            return base.CanQueueCommand(commandSet, powerOnCommandExistsInQueue);


        }


    }
}