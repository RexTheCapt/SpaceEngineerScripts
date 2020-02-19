﻿using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System;
using Sandbox.Game.Entities;
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

            Runtime.UpdateFrequency = UpdateFrequency.Update100;
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

        private bool regainedConnection = false;

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

            IMyLaserAntenna antenna = (IMyLaserAntenna)GridTerminalSystem.GetBlockWithName("Main Laser Antenna");
            IMyRemoteControl remoteControl = (IMyRemoteControl)GridTerminalSystem.GetBlockWithName("Main Remote Control");

            if (antenna != null && remoteControl != null)
            {
                if (antenna.Status != MyLaserAntennaStatus.Connected)
                {
                    if (antenna.Status != MyLaserAntennaStatus.RotatingToTarget)
                    {
                        if (antenna.Status != MyLaserAntennaStatus.SearchingTargetForAntenna)
                        {
                            if (antenna.Status != MyLaserAntennaStatus.Connecting)
                            {
                                antenna.Connect();
                            }
                        }
                    }

                    remoteControl.ClearWaypoints();
                    remoteControl.AddWaypoint(antenna.TargetCoords, "Antenna Target");
                    remoteControl.SpeedLimit = 10;
                    remoteControl.Direction = Base6Directions.Direction.Forward;
                    remoteControl.SetAutoPilotEnabled(true);
                    regainedConnection = false;
                }
                else if (!regainedConnection)
                {
                    remoteControl.SetAutoPilotEnabled(false);
                    regainedConnection = true;
                }
            }
        }
    }
}
