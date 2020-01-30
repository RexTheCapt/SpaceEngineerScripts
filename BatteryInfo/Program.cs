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

            List<IMyBatteryBlock> batteryBlocks = new List<IMyBatteryBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyBatteryBlock>(batteryBlocks);

            float totalStoredPower = 0;
            float maxStoredPower = 0;
            float totalOutput = 0;
            float totalInput = 0;

            foreach (IMyBatteryBlock block in batteryBlocks)
            {
                totalStoredPower += block.CurrentStoredPower;
                maxStoredPower += block.MaxStoredPower;
                totalOutput += block.CurrentOutput;
                totalInput += block.CurrentInput;
            }

            float hoursLeft = (maxStoredPower - totalStoredPower) / (totalInput - totalOutput);
            float minLeft = (float) ((int) hoursLeft * 60) - (hoursLeft * 60);
            float secLeft = (float) ((int) minLeft * 60) - (hoursLeft * 60);

            List<IMyTextPanel> textPanels = new List<IMyTextPanel>();
            GridTerminalSystem.GetBlocksOfType(textPanels);

            TimeSpan timeLeft = new TimeSpan((int)hoursLeft,(int) minLeft,(int)secLeft);

            if (timeLeft.TotalSeconds < 0)
                timeLeft = new TimeSpan(0 - timeLeft.Days, 0 - timeLeft.Hours, 0 - timeLeft.Minutes, 0 - timeLeft.Seconds, 0 - timeLeft.Milliseconds);

            string outputText = $"Last update: {DateTime.Now}\n" +
                                $"Total: {totalStoredPower} MWh\n" +
                                $"Max: {maxStoredPower} MWh\n" +
                                $"Batteries: {batteryBlocks.Count}\n" +
                                $"Capacity: {((totalStoredPower / maxStoredPower) * 100):000.00}%\n" +
                                $"Output: {totalOutput:#.000} MW\n" +
                                $"Input: {totalInput:#.000} MW\n" +
                                $"Change: {(totalInput - totalOutput):#.000} MW\n" +
                                $"{(totalInput - totalOutput > 0 ? "Charged in:" : "Depleted in:")} {timeLeft:g}\n";

            foreach (IMyTextPanel panel in textPanels)
            {
                if (panel.CustomName.Contains("[Battery Info]"))
                {
                    panel.ContentType = ContentType.TEXT_AND_IMAGE;
                    panel.WriteText(outputText);
                }
            }

            Echo(outputText);
        }
    }
}
