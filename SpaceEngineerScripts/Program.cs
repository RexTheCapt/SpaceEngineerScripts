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
using SpaceEngineers.Game.Entities.Blocks;
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
        private DateTime nameChangerDateTime = DateTime.Now;

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
            DateTime startDateTime = DateTime.Now;
            List<IMyWarhead> warheads = new List<IMyWarhead>();
            GridTerminalSystem.GetBlocksOfType(warheads);

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
                string o = "";

                if (warheads.Count > 0)
                    o += $"WARNING: SHIP CONTAINS {warheads.Count} WARHEADS! TIMER WILL BE SET TO 30 SEC\n";

                o += "Argument needed! Please copy the block name and paste it into the argument slot. CASE SENSITIVE";
                Echo(o);
                return;
            }

            List<IMyTerminalBlock> doors = GetAllDoors();
            List<IMyProgrammableBlock> programmableBlocks = new List<IMyProgrammableBlock>();
            List<IMyLargeGatlingTurret> largeGatlingTurrets = new List<IMyLargeGatlingTurret>();
            List<IMyLargeMissileTurret> missileTurrets = new List<IMyLargeMissileTurret>();
            List<IMyLargeInteriorTurret> interiorTurrets = new List<IMyLargeInteriorTurret>();
            List<IMyPistonBase> pistonBases = new List<IMyPistonBase>();
            List<IMyShipMergeBlock> mergeBlocks = new List<IMyShipMergeBlock>();
            List<IMyJumpDrive> jumpDrive = new List<IMyJumpDrive>();
            List<IMyAirVent> airVents = new List<IMyAirVent>();
            List<IMyGasTank> gasTanks = new List<IMyGasTank>();
            List<IMyMedicalRoom> medicalRooms = new List<IMyMedicalRoom>();
            List<IMyBatteryBlock> batteryBlocks = new List<IMyBatteryBlock>();
            List<IMySolarPanel> solarPanels = new List<IMySolarPanel>();
            List<IMyThrust> thrusts = new List<IMyThrust>();
            List<IMyGasGenerator> gasGenerators = new List<IMyGasGenerator>();
            List<IMyTimerBlock> timerBlocks = new List<IMyTimerBlock>();
            List<IMyGyro> gyros = new List<IMyGyro>();
            List<IMyGravityGenerator> gravityGenerators = new List<IMyGravityGenerator>();
            List<IMyGravityGeneratorSphere> gravityGeneratorSpheres = new List<IMyGravityGeneratorSphere>();
            List<IMyRadioAntenna> antennae = new List<IMyRadioAntenna>();
            List<IMyTextPanel> textPanels = new List<IMyTextPanel>();

            GridTerminalSystem.GetBlocksOfType(largeGatlingTurrets);
            GridTerminalSystem.GetBlocksOfType(missileTurrets);
            GridTerminalSystem.GetBlocksOfType(interiorTurrets);
            GridTerminalSystem.GetBlocksOfType(doors);
            GridTerminalSystem.GetBlocksOfType(programmableBlocks);
            GridTerminalSystem.GetBlocksOfType(pistonBases);
            GridTerminalSystem.GetBlocksOfType(mergeBlocks);
            GridTerminalSystem.GetBlocksOfType(jumpDrive);
            GridTerminalSystem.GetBlocksOfType(airVents);
            GridTerminalSystem.GetBlocksOfType(gasTanks);
            GridTerminalSystem.GetBlocksOfType(medicalRooms);
            GridTerminalSystem.GetBlocksOfType(batteryBlocks);
            GridTerminalSystem.GetBlocksOfType(solarPanels);
            GridTerminalSystem.GetBlocksOfType(thrusts);
            GridTerminalSystem.GetBlocksOfType(gasGenerators);
            GridTerminalSystem.GetBlocksOfType(timerBlocks);
            GridTerminalSystem.GetBlocksOfType(gyros);
            GridTerminalSystem.GetBlocksOfType(gravityGenerators);
            GridTerminalSystem.GetBlocksOfType(gravityGeneratorSpheres);
            GridTerminalSystem.GetBlocksOfType(antennae);
            GridTerminalSystem.GetBlocksOfType(textPanels);

            foreach (IMyProgrammableBlock block in programmableBlocks)
            {
                if (block.CustomName != _arg.Trim())
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

            foreach (IMyJumpDrive block in jumpDrive)
            {
                block.ApplyAction("OnOff_Off");
            }

            foreach (IMyAirVent vent in airVents)
            {
                vent.Depressurize = true;

                foreach (IMyGasTank tank in gasTanks)
                {
                    tank.AutoRefillBottles = false;
                    tank.Stockpile = true;
                }
            }

            foreach (IMyMedicalRoom block in medicalRooms)
            {
                block.ApplyAction("OnOff_Off");
            }

            foreach (IMyThrust thrust in thrusts)
            {
                thrust.ApplyAction("OnOff_Off");
            }
            
            foreach (IMyGasGenerator block in gasGenerators)
            {
                block.ApplyAction("OnOff_Off");
            }

            foreach (IMyTimerBlock block in timerBlocks)
            {
                block.ApplyAction("OnOff_Off");
            }

            foreach (IMyWarhead warhead in warheads)
            {
                if (!warhead.IsCountingDown)
                {
                    if (warhead.CustomData != "ARMED FUCKERS!!!")
                    {
                        warhead.DetonationTime = 30;
                        warhead.CustomData = "ARMED FUCKERS!!!";
                    }

                    warhead.StartCountdown();
                }
            }

            foreach (IMyGyro block in gyros)
            {
                block.ApplyAction("OnOff_Off");
            }

            foreach (IMyGravityGenerator generator in gravityGenerators)
            {
                generator.FieldSize = new Vector3(150,150,150);
                generator.Enabled = false;
                generator.GravityAcceleration = 10;
            }
            
            foreach (IMyGravityGeneratorSphere generator in gravityGeneratorSpheres)
            {
                generator.Radius = 400;
                generator.Enabled = false;
                generator.GravityAcceleration = 10;
            }

            if ((DateTime.Now - nameChangerDateTime).TotalSeconds > 1)
            {
                nameChangerDateTime = DateTime.Now;

                List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();

                GridTerminalSystem.GetBlocksOfType(blocks);

                foreach (IMyTerminalBlock block in blocks)
                {
                    block.ShowInInventory = false;
                    block.ShowInTerminal = false;
                    block.ShowInToolbarConfig = false;

                    if (block is IMyCargoContainer)
                        block.ShowOnHUD = true;
                    else
                        block.ShowOnHUD = false;

                    if (block.CustomName != _arg)
                    {
                        block.CustomName = $"{_arg} ";
                    }
                }
            }

            foreach (IMyRadioAntenna antenna in antennae)
            {
                antenna.Enabled = true;
                antenna.EnableBroadcasting = true;
                antenna.Radius = Single.MaxValue;
            }

            foreach (IMyTextPanel panel in textPanels)
            {
                panel.WritePublicTitle("HACKED");
                panel.ContentType = ContentType.TEXT_AND_IMAGE;
                panel.WriteText("HACKED");
            }

            // REMOVE ALL POWER GENS //
            /*foreach (IMyBatteryBlock block in batteryBlocks)
            {
                block.ApplyAction("OnOff_Off");
            }

            foreach (IMySolarPanel block in solarPanels)
            {
                block.ApplyAction("OnOff_Off");                
            }*/

            // APPLY AT LAST //
            foreach (IMyShipMergeBlock block in mergeBlocks)
            {
                block.ApplyAction("OnOff_Off");
            }

            Echo($"Script used {(DateTime.Now - startDateTime):g} to run");
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