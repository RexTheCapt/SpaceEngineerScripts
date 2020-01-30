using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System;
using VRage.Collections;
using VRage.Game.Components;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ObjectBuilders.Definitions;
using VRage.Game;
using VRage;
using VRageMath;

namespace IngameScript
{
    partial class Program : MyGridProgram
    {
        // This file contains your actual script.
        //
        // You can either keep all your code here, or you can create separate
        // code files to make your program easier to navigate while coding.
        //
        // In order to add a new utility class, right-click on your project, 
        // select 'New' then 'Add Item...'. Now find the 'Space Engineers'
        // category under 'Visual C# Items' on the left hand side, and select
        // 'Utility Class' in the main area. Name it in the box below, and
        // press OK. This utility class will be merged in with your code when
        // deploying your final script.
        //
        // You can also simply create a new utility class manually, you don't
        // have to use the template if you don't want to. Just do so the first
        // time to see what a utility class looks like.
        // 
        // Go to:
        // https://github.com/malware-dev/MDK-SE/wiki/Quick-Introduction-to-Space-Engineers-Ingame-Scripts
        //
        // to learn more about ingame scripts.
        
        private List<IMyAirVent> airVents = new List<IMyAirVent>();
        private List<IMyDoor> doors = new List<IMyDoor>();
        private List<IMyInteriorLight> lights = new List<IMyInteriorLight>();
        private List<IMyTextPanel> textPanels = new List<IMyTextPanel>();

        public Program()
        {
            // The constructor, called only once every session and
            // always before any other method is called. Use it to
            // initialize your script. 
            //     
            // The constructor is optional and can be removed if not
            // needed.
            // 
            // It's recommended to set Runtime.UpdateFrequency 
            // here, which will allow your script to run itself without a 
            // timer block.

            GetAllBlocks();

            foreach (IMyAirVent vent in airVents)
            {
                VentData vd = new VentData(vent);

                if (vd.IsAirlock)
                {
                    vd.Reset();
                    Echo($"Reset {vd.Name}");
                }
            }

            Runtime.UpdateFrequency = UpdateFrequency.Update1;
        }

        public void Main(string argument, UpdateType updateSource)
        {
            // The main entry point of the script, invoked every time
            // one of the programmable block's Run actions are invoked,
            // or the script updates itself. The updateSource argument
            // describes where the update came from. Be aware that the
            // updateSource is a  bitfield  and might contain more than 
            // one update type.
            // 
            // The method itself is required, but the arguments above
            // can be removed if not needed.

            GetAllBlocks();

            // Drain or fill airlock
            foreach (IMyAirVent airVent in airVents)
            {
                VentData vd = new VentData(airVent);

                foreach (IMyInteriorLight light in lights)
                {
                    //Echo($"T:[Airlock-{vd.Group}");
                    if (light.CustomName.Contains($"[Airlock-{vd.Group}"))
                    {
                        if (light.Intensity > 4)
                        {
                            vd.Direction = VentData.Direct.Inn;
                            Echo("Inn");
                            vd.Save();
                        }
                        else if (light.Intensity < 4)
                        {
                            vd.Direction = VentData.Direct.Out;
                            Echo("Out");
                            vd.Save();
                        }

                        light.Intensity = 4;
                        light.Falloff = 1;

                        Echo($"Works, {light.Intensity.ToString()}");
                    }
                }

                if (vd.IsAirlock)
                {
                    foreach (IMyDoor door in doors)
                    {
                        if (door.CustomName.Contains($"[Airlock-{vd.Group}"))
                        {
                            switch (vd.Direction)
                            {
                                case VentData.Direct.Inn:
                                    airVent.Depressurize = false;

                                    if (door.CustomName.Contains("Inn"))
                                    {
                                        if ((!airVent.CanPressurize || airVent.GetOxygenLevel() < 0.95))
                                        {
                                            if(door.OpenRatio != 0)
                                            {
                                                door.Enabled = true;
                                                door.CloseDoor();
                                            }
                                            else
                                            {
                                                door.Enabled = false;
                                            }
                                        }
                                        else if (airVent.GetOxygenLevel() > 0.95)
                                            if (door.OpenRatio != 1)
                                            {
                                                door.Enabled = true;
                                                door.OpenDoor();
                                            }
                                            else
                                            {
                                                door.Enabled = false;
                                            }
                                    }
                                    else if (door.CustomName.Contains("Out"))
                                    {
                                        if ((!airVent.CanPressurize || airVent.GetOxygenLevel() < 0.95))
                                        {
                                            if (door.OpenRatio != 0)
                                            {
                                                door.Enabled = true;
                                                door.CloseDoor();
                                            }
                                            else
                                            {
                                                door.Enabled = false;
                                            }
                                        }
                                        else if (airVent.GetOxygenLevel() > 0.95)
                                            if (door.OpenRatio != 0)
                                            {
                                                door.Enabled = true;
                                                door.OpenDoor();
                                            }
                                            else
                                            {
                                                door.Enabled = false;
                                            }
                                    }

                                    if (airVent.GetOxygenLevel() == 1)
                                    {
                                        SetLights(vd.Group, AirlockStatus.Inn);
                                        SetLcd(vd.Group, AirlockStatus.Inn, airVent);
                                    }
                                    else if (!airVent.CanPressurize || airVent.GetOxygenLevel() < 0.95)
                                    {
                                        SetLights(vd.Group, AirlockStatus.Working);
                                        SetLcd(vd.Group, AirlockStatus.Working, airVent);
                                    }
                                    break;
                                case VentData.Direct.Out:
                                    if (door.CustomName.Contains("Inn"))
                                    {
                                        if (door.OpenRatio != 0)
                                        {
                                            door.Enabled = true;
                                            door.CloseDoor();
                                        }
                                        else
                                        {
                                            door.Enabled = false;
                                            airVent.Depressurize = true;
                                        }
                                    }
                                    else if (door.CustomName.Contains("Out"))
                                    {
                                        if (airVent.GetOxygenLevel() > 0.05)
                                        {
                                            if (door.OpenRatio != 0)
                                            {
                                                door.Enabled = true;
                                                door.CloseDoor();
                                            }
                                            else
                                            {
                                                door.Enabled = false;
                                            }
                                        }
                                        else if (!airVent.CanPressurize || airVent.GetOxygenLevel() == 0)
                                        {
                                            if (door.OpenRatio != 1)
                                            {
                                                door.Enabled = true;
                                                door.OpenDoor();
                                            }
                                            else
                                            {
                                                door.Enabled = false;
                                            }
                                        }
                                    }

                                    if(airVent.GetOxygenLevel() > 0)
                                    {
                                        SetLights(vd.Group, AirlockStatus.Working);
                                        SetLcd(vd.Group, AirlockStatus.Working,airVent);
                                    }
                                    else if (airVent.GetOxygenLevel() == 0 || !airVent.CanPressurize)
                                    {
                                        SetLights(vd.Group, AirlockStatus.Out);
                                        SetLcd(vd.Group, AirlockStatus.Out,airVent);
                                    }
                                    break;
                            }
                        }
                    }
                }
            }
        }

        private void SetLcd(string group, AirlockStatus airlockStatus, IMyAirVent airVent)
        {
            foreach (IMyTextPanel panel in textPanels)
            {
                if (panel.CustomName.Contains($"[Airlock-{group}"))
                {
                    switch (airlockStatus)
                    {
                        case AirlockStatus.Working:
                            panel.ContentType = ContentType.TEXT_AND_IMAGE;
                            panel.FontColor = Color.Orange;
                            panel.Alignment = TextAlignment.CENTER;
                            panel.WriteText($"Cycling...\n{(airVent.GetOxygenLevel() * 100):000.00}%");
                            break;
                        case AirlockStatus.Inn:
                            panel.ContentType = ContentType.TEXT_AND_IMAGE;
                            panel.FontColor = Color.Green;
                            panel.Alignment = TextAlignment.CENTER;
                            panel.WriteText($"Breathing\n{(airVent.GetOxygenLevel() * 100):000.00}%");
                            break;
                        case AirlockStatus.Out:
                            panel.ContentType = ContentType.TEXT_AND_IMAGE;
                            panel.FontColor = Color.Green;
                            panel.Alignment = TextAlignment.CENTER;
                            panel.WriteText($"Choking\n{(airVent.GetOxygenLevel() * 100):000.00}%");
                            break;
                    }
                }
            }
        }

        private bool IsAllDoorsClosed(string group)
        {
            foreach (IMyDoor door in doors)
            {
                if (door.CustomName.Contains($"[Airlock-{group}"))
                {
                    if (door.OpenRatio != 0)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private void SetLights(string group, AirlockStatus status)
        {
            List<IMyInteriorLight> innLights = new List<IMyInteriorLight>();
            List<IMyInteriorLight> outLights = new List<IMyInteriorLight>();
            List<IMyInteriorLight> allLights = new List<IMyInteriorLight>();

            foreach (IMyInteriorLight light in lights)
            {
                if (light.CustomName.Contains($"[Airlock-{group}"))
                {
                    if(light.CustomName.Contains("Inn"))
                        innLights.Add(light);
                    else if(light.CustomName.Contains("Out"))
                        outLights.Add(light);
                }
            }

            allLights.AddRange(innLights);
            allLights.AddRange(outLights);

            switch (status)
            {
                case AirlockStatus.Inn:
                    foreach (IMyInteriorLight light in innLights)
                    {
                        light.Color = Color.Green;
                        light.BlinkIntervalSeconds = 0;
                        light.BlinkLength = 0;
                    }

                    foreach (IMyInteriorLight light in outLights)
                    {
                        light.Color = Color.DarkRed;
                        light.BlinkIntervalSeconds = 1;
                        light.BlinkLength = 25;
                    }
                    break;
                case AirlockStatus.Out:
                    foreach (IMyInteriorLight light in outLights)
                    {
                        light.Color = Color.Green;
                        light.BlinkIntervalSeconds = 0;
                        light.BlinkLength = 0;
                    }

                    foreach (IMyInteriorLight light in innLights)
                    {
                        light.Color = Color.DarkRed;
                        light.BlinkIntervalSeconds = 1;
                        light.BlinkLength = 25;
                    }
                    break;
                case AirlockStatus.Working:
                    foreach (IMyInteriorLight light in allLights)
                    {
                        light.Color = Color.Orange;
                        light.BlinkIntervalSeconds = 0;
                        light.BlinkLength = 0;
                    }
                    break;
            }
        }

        private enum AirlockStatus
        {
            Inn,
            Out,
            Working,
            Error
        }

        public void Save()
        {
            // Called when the program needs to save its state. Use
            // this method to save your state to the Storage field
            // or some other means. 
            // 
            // This method is optional and can be removed if not
            // needed.
        }

        private void GetAllBlocks()
        {
            GridTerminalSystem.GetBlocksOfType(airVents);
            GridTerminalSystem.GetBlocksOfType(doors);
            GridTerminalSystem.GetBlocksOfType(lights);
            GridTerminalSystem.GetBlocksOfType(textPanels);
        }
    }

    internal class VentData
    {
        public string Name => AirVent.CustomName;
        public bool IsAirlock => Name.Contains("[Airlock");
        public string Group => Name.Substring(Name.LastIndexOf('-') + 1).Split(' ')[0].Replace("]", "");
                            // Corner Light - Double 4 [Airlock-Main Out]

        public IMyAirVent AirVent;

        public Direct Direction;

        public VentData(IMyAirVent vent)
        {
            AirVent = vent;

            string lines = vent.CustomData;

            if (!string.IsNullOrEmpty(vent.CustomData))
            {
                Direction = lines.Split(':')[1] == "Inn" ? Direct.Inn : Direct.Out;
            }
            else
            {
                Reset();
            }
        }

        public void Reset()
        {
            AirVent.CustomData = $"dir:Inn";
        }

        public void Save()
        {
            AirVent.CustomData = $"dir:{(Direction == Direct.Inn ? "Inn" : "Out")}";
        }

        public enum Direct
        {
            Inn,
            Out
        }
    }
}
