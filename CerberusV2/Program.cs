// CerberusV2

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
        // Config
        private string OreDisplayName = "LCD Panel 10 [LCD] [Cerberus]";
        private string IngotDisplayName = "LCD Panel 9 [LCD] [Cerberus]";
        private string MainCockpitName = "Flight Seat Bridge Captain [MAIN] [Cerberus]";
        private string JumpDriveDisplay = "Corner LCD Top 12 [LCD] [Cerberus]";
        private int AirlockTimer = 3;
        private string[] AirlockDoorNames = new[]
        {
            "Left Exterior Sliding Door [Cerberus];Left Exterior Sliding Door 2 [Cerberus]",
            "Right Exterior Sliding Door [Cerberus];Right Exterior Sliding Door 2 [Cerberus]"
        };
        // Config end

        private readonly List<Ore> ores = new List<Ore>()
        {
            new Ore("Cobalt"),
            new Ore("Gold"),
            new Ore("Ice"),
            new Ore("Scrap"),
            new Ore("Iron"),
            new Ore("Magnesium"),
            new Ore("Nickel"),
            new Ore("Platinum"),
            new Ore("Silicon"),
            new Ore("Silver"),
            new Ore("Uranium")
        }.OrderBy(x => x.SubtypeId).ToList();

        private readonly List<Ingot> ingots = new List<Ingot>()
        {
            new Ingot("Cobalt"),
            new Ingot("Gold"),
            new Ingot("Iron"),
            new Ingot("Magnesium"),
            new Ingot("Nickel"),
            new Ingot("Platinum"),
            new Ingot("Silicon"),
            new Ingot("Silver"),
            new Ingot("Stone"),
            new Ingot("Uranium")
        }.OrderBy(x => x.SubtypeId).ToList();

        private IMyCockpit mainCockpit;

        private List<Airlock> airlocks = new List<Airlock>();

        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update1;
            mainCockpit = (IMyCockpit)GridTerminalSystem.GetBlockWithName(MainCockpitName);

            foreach (string name in AirlockDoorNames)
            {
                string[] names = name.Split(';');
                Airlock airlock = new Airlock(names[0], names[1], AirlockTimer);
                airlock.D1 = GridTerminalSystem.GetBlockWithName(names[0]) as IMyDoor;
                airlock.D2 = GridTerminalSystem.GetBlockWithName(names[1]) as IMyDoor;
                airlocks.Add(airlock);
            }
        }

        public void Main(string argument, UpdateType updateSource)
        {
            if (mainCockpit != null)
            {
                IMyTextSurface surface = mainCockpit.GetSurface(0);
                surface.ContentType = ContentType.TEXT_AND_IMAGE;
                surface.FontSize = 7.7f;

                MyFixedPoint currentCargoVolume = 0;
                MyFixedPoint maxCargoVolume = 0;
                List<IMyTerminalBlock> cargoContainers = new List<IMyTerminalBlock>();
                IMyTerminalBlock controlBlock;
                List<IMyTerminalBlock> gridBlocks = new List<IMyTerminalBlock>();
                List<IMyThrust> reverseIonThrusters = new List<IMyThrust>();

                float brakingTime;
                float brakingDistance;
                float maxAcceleration;
                float reverseThrustersForce = 0;
                float totalMass;
                double currentSpeed;

                GridTerminalSystem.GetBlocks(gridBlocks);

                controlBlock = gridBlocks.Find(x => x.CustomName.Contains("[MAIN]"));

                foreach (IMyTerminalBlock _block in gridBlocks)
                {
                    if (_block is IMyCargoContainer || _block is IMyShipConnector || _block is IMyShipDrill)
                    {
                        cargoContainers.Add(_block as IMyTerminalBlock);
                    }

                    if (_block is IMyThrust && _block.WorldMatrix.Forward == controlBlock.WorldMatrix.Forward)
                    {
                        reverseIonThrusters.Add(_block as IMyThrust);
                        reverseThrustersForce += (_block as IMyThrust).MaxEffectiveThrust;
                    }
                }

                totalMass = (controlBlock as IMyShipController).CalculateShipMass().TotalMass;
                maxAcceleration = reverseThrustersForce / totalMass;
                currentSpeed = (controlBlock as IMyShipController).GetShipSpeed();
                brakingTime = (float)currentSpeed / maxAcceleration;
                brakingDistance = (maxAcceleration * brakingTime * brakingTime) / 2;

                foreach (IMyTerminalBlock _container in cargoContainers)
                {
                    maxCargoVolume += _container.GetInventory().MaxVolume;
                    currentCargoVolume += _container.GetInventory().CurrentVolume;
                }

                double cargoSpace = ((double)currentCargoVolume / (double)maxCargoVolume * 100);

                surface.WriteText($"Cargo Load: {Math.Round(cargoSpace, 2)}%\n" +
                                  $"Until stop: {Math.Round(brakingTime)} s. {Math.Round(brakingDistance)} m.");
            }

            List<IMyTerminalBlock> blocksWithInventory = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType(blocksWithInventory, x => x.HasInventory);

            foreach (IMyTerminalBlock block in blocksWithInventory)
            {
                IMyInventory inventory = block.GetInventory();
                List<MyInventoryItem> items = new List<MyInventoryItem>();
                inventory.GetItems(items);

                foreach (MyInventoryItem item in items)
                {
                    switch (item.Type.TypeId)
                    {
                        case "MyObjectBuilder_Ore":
                            Ore ore = ores.Find(x => x.SubtypeId == item.Type.SubtypeId);
                            if (ore != null)
                                ore.Amount += item.Amount;
                            else
                            {
                                Echo($"{item.Type.TypeId == "MyObjectBuilder_Ore"} - {item.Type.SubtypeId}");
                            }
                            break;
                        case "MyObjectBuilder_Ingot":
                            Ingot ingot = ingots.Find(x => x.SubtypeId == item.Type.SubtypeId);
                            if (ingot != null)
                                ingot.Amount += item.Amount;
                            else
                            {
                                Echo($"{item.Type.TypeId == "MyObjectBuilder_Ore"} - {item.Type.SubtypeId}");
                            }
                            break;
                    }
                }
            }

            IMyTextPanel oreDisplay = (IMyTextPanel)GridTerminalSystem.GetBlockWithName(OreDisplayName);
            IMyTextPanel ingotDisplay = (IMyTextPanel)GridTerminalSystem.GetBlockWithName(IngotDisplayName);

            if (oreDisplay != null)
            {
                string output = "Ores :\n";

                foreach (Ore ore in ores)
                {
                    output += $" {ore.SubtypeId} : {ore.Amount.ToIntSafe()}\n";
                    ore.Amount = 0;
                }

                oreDisplay.WriteText(output);
            }

            if (ingotDisplay != null)
            {
                string output = "Ingots:\n";

                foreach (Ingot ingot in ingots)
                {
                    output += $" {ingot.SubtypeId} : {ingot.Amount.ToIntSafe()}\n";
                    ingot.Amount = 0;
                }

                ingotDisplay.WriteText(output);
            }

            List<IMyJumpDrive> jumpDrives = new List<IMyJumpDrive>();
            GridTerminalSystem.GetBlocksOfType(jumpDrives);

            if (jumpDrives[0] != null)
            {
                IMyTextPanel text = (IMyTextPanel)GridTerminalSystem.GetBlockWithName(JumpDriveDisplay);

                if (text != null)
                {
                    float currentStoredPower = (jumpDrives[0].CurrentStoredPower / jumpDrives[0].MaxStoredPower) * 100;
                    text.WriteText($"Drive Charge: {currentStoredPower:000.00}%");

                    if (currentStoredPower == 100)
                    {
                        text.FontColor = Color.Green;
                    }
                    else if (currentStoredPower < 100)
                    {
                        text.FontColor = Color.Orange;
                    }
                }
            }

            foreach (Airlock airlock in airlocks)
            {
                airlock.Check();
            }
        }

        private class Airlock
        {
            public IMyDoor D1;
            public IMyDoor D2;
            private readonly int MaxSeconds;

            public Airlock(string door1, string door2, int airlockTimer)
            {
                MaxSeconds = airlockTimer;
            }

            private DateTime d1DateTime = DateTime.MinValue;
            private DateTime d2DateTime = DateTime.MinValue;

            public void Check()
            {
                if (D1.OpenRatio != 0)
                {
                    D2.Enabled = false;

                    if (d1DateTime == DateTime.MinValue)
                        d1DateTime = DateTime.Now;

                    if (d1DateTime.AddSeconds(MaxSeconds) < DateTime.Now)
                        D1.CloseDoor();
                }
                else
                {
                    D2.Enabled = true;
                    d1DateTime = DateTime.MinValue;
                }

                if (D2.OpenRatio != 0)
                {
                    D1.Enabled = false;

                    if (d2DateTime == DateTime.MinValue)
                        d2DateTime = DateTime.Now;

                    if (d2DateTime.AddSeconds(MaxSeconds) < DateTime.Now)
                        D2.CloseDoor();
                }
                else
                {
                    D1.Enabled = true;
                    d2DateTime = DateTime.MinValue;
                }
            }
        }

        private class Ore
        {
            public readonly string TypeId = "MyObjectBuilder_Ore";
            public readonly string SubtypeId;
            public MyFixedPoint Amount = MyFixedPoint.Zero;

            public Ore(string oreName)
            {
                SubtypeId = oreName;
            }
        }

        private class Ingot
        {
            public readonly string TypeId = "MyObjectBuilder_Ingot";
            public readonly string SubtypeId;
            public MyFixedPoint Amount = MyFixedPoint.Zero;

            public Ingot(string ingotName)
            {
                SubtypeId = ingotName;
            }
        }
    }
}
