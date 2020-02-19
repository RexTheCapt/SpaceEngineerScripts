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
        //COPY FROM HERE

        /*README
         * THIS SCRIPT FEATURES TWO FUNCTIONS:
         I.  Indicating current cargo load in %
         II. Indicating time and distance until ship's full stop
         
             Add a tag "[MAIN]" (without quotemarks) to the name of your ship's main cockpit or pilot's seat
             Add a tag "[INFO]" (without quotemarks) to a text or LCD panel you want to output your information to
             If needed add one more panel with "[DEBUG]" (without quotemarks) tag to output technical data
             
             Recompile and run this script. Do it again, if engines or containers get added or removed*/

        const String author = "THX-11-38";
        const String version = "v0.8";

        public IMyTerminalBlock controlBlock;
        public IMyTextPanel infoLcd;
        public IMyTextPanel debugLcd;

        public List<IMyTerminalBlock> gridBlocks = new List<IMyTerminalBlock>();
        public List<IMyTerminalBlock> gridBlocksUpdate = new List<IMyTerminalBlock>();
        public List<IMyThrust> reverseIonThrusters = new List<IMyThrust>();
        public List<IMyTerminalBlock> cargoContainers = new List<IMyTerminalBlock>();

        public VRage.MyFixedPoint maxCargoVolume = 0;
        public VRage.MyFixedPoint currentCargoVolume = 0;

        public IMyInventory containersVolume;

        public float reverseThrustersForce;
        public float totalMass;
        public float maxAcceleration;
        public double currentSpeed;
        public float brakingDistance;
        public float brakingTime;


        const bool REWRITE = false;

        public bool greeting = true;

        public string EOL = "\n";

        public StringBuilder sbInfo = new StringBuilder();
        public StringBuilder sbDebug = new StringBuilder();

        public Program()
        {
            GridTerminalSystem.GetBlocks(gridBlocks);
            controlBlock = gridBlocks.Find(x => x.CustomName.Contains("[MAIN]")); //FIND MAIN COCKPIT TAGGED [MAIN]
            if (controlBlock == null)
                Echo(" !!! No cockpit with [MAIN] tag found" + EOL +
                     "See README part of code"); // CHECK FOR EXCEPTION AND EXPLAIN

            foreach (IMyTerminalBlock _block in gridBlocks)
            {
                // Search for LCDs
                if (_block is IMyTextPanel)
                {
                    if (_block.CustomName.Contains("[INFO]")) infoLcd = _block as IMyTextPanel;
                    if (_block.CustomName.Contains("[DEBUG]")) debugLcd = _block as IMyTextPanel;
                }

                // Search for Thrusters:
                if (_block is IMyThrust && _block.WorldMatrix.Forward == controlBlock.WorldMatrix.Forward
                ) //if nozzle has same direction as cockpit
                {
                    reverseIonThrusters.Add(_block as IMyThrust); // add this one to a list
                    reverseThrustersForce +=
                        (_block as IMyThrust).MaxEffectiveThrust; // and add it's max force into float
                }

                // Search for Cargo containers, Connectors and Drills:                
                if (_block is IMyCargoContainer || _block is IMyShipConnector || _block is IMyShipDrill)
                {
                    cargoContainers.Add(_block as IMyTerminalBlock);
                }

            }

            Echo("ALL TAGGED BLOCKS FOUND" + EOL + "SCRIPT IS ACTIVE" + EOL + EOL + "by " + author);
            if (debugLcd != null) Echo("Optional DEBUG panel found");
            Runtime.UpdateFrequency = UpdateFrequency.Update10; // Perform 10 times per second
        }


        public void Main(string argument, UpdateType updateSource)
        {
            foreach (IMyTerminalBlock _container in cargoContainers)
            {
                maxCargoVolume += _container.GetInventory().MaxVolume;
                currentCargoVolume +=
                    _container.GetInventory()
                        .CurrentVolume; //Detect add use amount of total and used space in each container

                //Max cargo example when Inventory x1:
                // 2x Connectors        a 1152 L
                // 3x MedCargoConts     a 3375 L
                // 2x Drills            a 3375 L
                // 2x Ejectors          a 64 L
                // TOTAL                a 19307 L                
            }

            double cargoSpace =
                ((double)currentCargoVolume / (double)maxCargoVolume * 100); //Percentage of used space

            //BRAKING DISTANCE AND BRAKING TIME:
            totalMass = (controlBlock as IMyShipController).CalculateShipMass().TotalMass;
            currentSpeed = (controlBlock as IMyShipController).GetShipSpeed();
            maxAcceleration = reverseThrustersForce / totalMass;
            brakingTime = (float)currentSpeed / maxAcceleration;
            brakingDistance = (maxAcceleration * brakingTime * brakingTime) / 2;

            //INFO PANEL
            if ((controlBlock as IMyCockpit).IsUnderControl == true) //Works when pilot onboard
            {
                //if (currentSpeed < 2 ) sbInfo.Append(" CHECK OXYGEN + EOL + BOTTLES ONBOARD");
                if (cargoSpace > 0.5)
                {
                    sbInfo.Append(" Cargo Load: " + Math.Round(cargoSpace, 2) + " %" + EOL);
                    greeting = false;
                }

                if (currentSpeed > 2)
                {
                    sbInfo.Append(" Until stop: " + Math.Round(brakingTime) + " s. " + Math.Round(brakingDistance) +
                                  " m." + EOL);
                    greeting = false;
                }

                if (currentSpeed < 2 && cargoSpace <= 0.5 && greeting == true)
                    sbInfo.Append(" ALL SYSTEMS ONLINE" + EOL + " CHECK OXYGEN" + EOL + " BOTTLES ONBOARD");
            }

            //DEBUG PANEL
            sbDebug.Append(" Reverse thrusters amount: " + reverseIonThrusters.Count + EOL);
            sbDebug.Append(" Braking force, N = " + reverseThrustersForce + " N " + EOL);
            sbDebug.Append(" Mass, kg: " + totalMass + EOL);
            sbDebug.Append(" Speed, m/s: " + currentSpeed + EOL);
            sbDebug.Append(" Acceleration, m/ss: " + maxAcceleration + EOL + EOL);
            sbDebug.Append(" Containers: " + cargoContainers.Count + EOL); // amount of objects in list
            sbDebug.Append(" MaxVolume: " + maxCargoVolume.ToString() + EOL);
            sbDebug.Append(" CurrentVolume: " + currentCargoVolume.ToString() + EOL);
            sbDebug.Append(" Cargo: " + Math.Round(cargoSpace, 2) + " %" + EOL);
            sbDebug.Append(" ");
            sbDebug.Append(" ");
            sbDebug.Append(" ");

            //Zeroing values after this tick
            maxCargoVolume = 0;
            currentCargoVolume = 0;
            cargoSpace = 0;
            if ((controlBlock as IMyCockpit).IsUnderControl == false) greeting = true;

            if (infoLcd != null) //Solving exception during LCD identification by tag. If no LCD found, keep working
            {
                infoLcd.WritePublicText(sbInfo.ToString(), REWRITE); //What to write: sbInfo turned into text
                infoLcd.ShowPublicTextOnScreen(); //Show text on debugLcd_0
                sbInfo.Clear(); //Remove everything was displayed by sbInfo in this tick and write again in the next tick
            }
            else
            {
                Echo("No Info LCD found" + EOL + "See README part of code");
            }

            if (debugLcd != null) //Solving exception during LCD identification by tag. If no LCD found, keep working
            {
                debugLcd.WritePublicText(sbDebug.ToString(), REWRITE); //What to write: sbDebug turned into text
                debugLcd.ShowPublicTextOnScreen(); //Show text on debugLcd_0
                sbDebug.Clear(); //Remove everything was displayed by sbDebug in this tick and write again in the next tick
            }
        }

        //COPY TO HERE
    }
}