// ReSharper disable RedundantUsingDirective
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
    sealed class Program : MyGridProgram
    {
        private DateTime _initiationDateTime = DateTime.Now;

        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update1;
        }

        public void Save()
        {
            Storage = _arg;
        }

        private string _arg = "";

        public void Main(string argument, UpdateType updateSource)
        {
            if (!string.IsNullOrEmpty(argument))
            {
                _arg = argument;
                Save();
            }
            else
            {
                _arg = Storage;
            }

            if (string.IsNullOrEmpty(_arg))
            {
                Echo("Argument needed! Please copy the block name and paste it into the argument slot. CASE SENSITIVE");
                return;
            }

            string o = "";

            List<IMyTerminalBlock> doors = GetAllDoors();
            List<IMyProgrammableBlock> programmableBlocks = new List<IMyProgrammableBlock>();
            List<IMyLargeGatlingTurret> largeGatlingTurrets = new List<IMyLargeGatlingTurret>();
            List<IMyLargeMissileTurret> missileTurrets = new List<IMyLargeMissileTurret>();
            List<IMyLargeInteriorTurret> interiorTurrets = new List<IMyLargeInteriorTurret>();
            List<IMyPistonBase> pistonBases = new List<IMyPistonBase>();
            List<IMyShipMergeBlock> mergeBlocks = new List<IMyShipMergeBlock>();
            //List<IMyJumpDrive> jumpDrive = new List<>
            List<IMyAirVent> airVents = new List<IMyAirVent>();
            List<IMyGasTank> gasTanks = new List<IMyGasTank>();

            GridTerminalSystem.GetBlocksOfType(largeGatlingTurrets);
            GridTerminalSystem.GetBlocksOfType(missileTurrets);
            GridTerminalSystem.GetBlocksOfType(interiorTurrets);
            GridTerminalSystem.GetBlocksOfType(doors);
            GridTerminalSystem.GetBlocksOfType(programmableBlocks);
            GridTerminalSystem.GetBlocksOfType(pistonBases);
            GridTerminalSystem.GetBlocksOfType(mergeBlocks);
            GridTerminalSystem.GetBlocksOfType(airVents);
            GridTerminalSystem.GetBlocksOfType(gasTanks);

            foreach (IMyProgrammableBlock block in programmableBlocks)
            {
                if (block.CustomName != _arg)
                {
                    block.ApplyAction("OnOff_Off");
                }
            }

            foreach (IMyTerminalBlock door in doors)
            {
                if (door is IMyAirtightHangarDoor)
                {
                    IMyAirtightHangarDoor b = door as IMyAirtightHangarDoor;

                    b.OpenDoor();

                    if (b.Status == DoorStatus.Open)
                    {
                        b.ApplyAction("OnOff_Off");
                    }
                    else
                    {
                        b.ApplyAction("OnOff_On");
                    }
                }

                if (door is IMyDoor)
                {
                    IMyDoor b = door as IMyDoor;

                    b.OpenDoor();

                    if (b.Status == DoorStatus.Open)
                    {
                        b.ApplyAction("OnOff_Off");
                    }
                    else
                    {
                        b.ApplyAction("OnOff_On");
                    }
                }
            }

            foreach (IMyLargeGatlingTurret turret in largeGatlingTurrets)
            {
                turret.ApplyAction("OnOff_Off");
            }

            foreach (IMyLargeMissileTurret turret in missileTurrets)
            {
                turret.ApplyAction("OnOff_Off");
            }

            foreach (IMyLargeInteriorTurret turret in interiorTurrets)
            {
                turret.ApplyAction("OnOff_Off");
            }

            foreach (IMyPistonBase myPistonBase in pistonBases)
            {
                Single velocity = -5;

                myPistonBase.SetValue("Velocity", velocity);
                myPistonBase.MinLimit = 0;
            }

            foreach (IMyAirVent vent in airVents)
            {
                vent.Depressurize = true;

                //int oxygenTanks = 
            }

            // APPLY AT LAST //
            foreach (IMyShipMergeBlock block in mergeBlocks)
            {
                block.ApplyAction("OnOff_Off");
            }

            Echo(o);
        }

        private List<IMyTerminalBlock> GetAllDoors()
        {
            List<IMyAirtightHangarDoor> hangarDoors = new List<IMyAirtightHangarDoor>();

            GridTerminalSystem.GetBlocksOfType(hangarDoors);

            List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();

            foreach (IMyAirtightHangarDoor door in hangarDoors)
            {
                blocks.Add(door);
            }

            return blocks;
        }
    }
}