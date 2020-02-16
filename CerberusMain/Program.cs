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
        private IMyTextPanel CockpitDriveChargeStatusTextPanel = null;
        private IMyTextPanel textPanelJumpDriveDetailedInfo = null;
        private IMyTextPanel textPanelIngotDisplay = null;
        private IMyTextPanel textPanelOreDisplay = null;
        private IMyTextPanel textPanelHydrogenDisplay = null;

        private static class Ingot
        {
            public static int MaxNickel, MaxPlatinum, MaxSilicon, MaxCobalt, MaxIron, MaxStone, MaxSilver, MaxGold;
        }

        private static class Ore
        {
            public static int MaxNickel, MaxPlatinum, MaxSilicon, MaxCobalt, MaxIron, MaxStone, MaxSilver, MaxGold;
        }

        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update100;

            if (string.IsNullOrEmpty(Me.CustomData))
            {
                Echo("Custom data reset, please edit...");
                Me.CustomData =
                    $"// Display jump drive charge in cockpit\n" +
                    $"CockpitDriveChargeStatusTextPanel=ENTER NAME\n" +
                    $"\n" +
                    $"// Display detailed info about the jump drive\n" +
                    $"textPanelJumpDriveDetailedInfo=ENTER NAME\n" +
                    $"\n" +
                    $"// Display info about ingots\n" +
                    $"textPanelIngotDisplay=ENTER NAME\n" +
                    $"\n" +
                    $"// Same as ingots but with ores\n" +
                    $"textPanelOreDisplay=ENTER NAME\n" +
                    $"\n" +
                    $"// Hydrogen display\n" +
                    $"textPanelHydrogenDisplay=ENTER NAME\n" +
                    $"\n" +
                    $"// Ingot settings\n" +
                    $"Ingot:Nickel=1000\n" +
                    $"Ingot:Platinum=1000\n" +
                    $"Ingot:Silicon=1000\n" +
                    $"Ingot:Cobalt=1000\n" +
                    $"Ingot:Iron=1000\n" +
                    $"Ingot:Stone=1000\n" +
                    $"Ingot:Silver=1000\n" +
                    $"Ingot:Gold=1000\n" +
                    $"\n" +
                    $"// Ore settings\n" +
                    $"Ore:Nickel=1000\n" +
                    $"Ore:Platinum=1000\n" +
                    $"Ore:Silicon=1000\n" +
                    $"Ore:Cobalt=1000\n" +
                    $"Ore:Iron=1000\n" +
                    $"Ore:Stone=1000\n" +
                    $"Ore:Silver=1000\n" +
                    $"Ore:Gold=1000\n";
            }
            else
            {
                string[] customDataStrings = Me.CustomData.Split('\n');

                foreach (string s in customDataStrings)
                {
                    if (s.StartsWith("//") || string.IsNullOrEmpty(s))
                    {
                        continue;
                    }

                    string[] value = s.Split('=');

                    #region Set variables
#pragma warning disable CS1717 // Assignment made to same variable
                    switch (value[0])
                    {
                        #region Ingot
                        case "Ingot:Nickel":
                            if (int.TryParse(value[1], out Ingot.MaxNickel))
                            {
                                Ingot.MaxNickel = Ingot.MaxNickel;
                            }
                            else
                            {
                                Ingot.MaxNickel = 1000;
                            }
                            break;
                        case "Ingot:Platinum":
                            if (int.TryParse(value[1], out Ingot.MaxPlatinum))
                            {
                                Ingot.MaxPlatinum = Ingot.MaxPlatinum;
                            }
                            else
                            {
                                Ingot.MaxPlatinum = 1000;
                            }
                            break;
                        case "Ingot:Silicon":
                            if (int.TryParse(value[1], out Ingot.MaxSilicon))
                            {
                                Ingot.MaxSilicon = Ingot.MaxSilicon;
                            }
                            else
                            {
                                Ingot.MaxSilicon = 1000;
                            }
                            break;
                        case "Ingot:Cobalt":
                            if (int.TryParse(value[1], out Ingot.MaxCobalt))
                            {
                                Ingot.MaxCobalt = Ingot.MaxCobalt;
                            }
                            else
                            {
                                Ingot.MaxCobalt = 1000;
                            }
                            break;
                        case "Ingot:Iron":
                            if (int.TryParse(value[1], out Ingot.MaxIron))
                            {
                                Ingot.MaxIron = Ingot.MaxIron;
                            }
                            else
                            {
                                Ingot.MaxIron = 1000;
                            }
                            break;
                        case "Ingot:Stone":
                            if (int.TryParse(value[1], out Ingot.MaxStone))
                            {
                                Ingot.MaxStone = Ingot.MaxStone;
                            }
                            else
                            {
                                Ingot.MaxStone = 1000;
                            }
                            break;
                        case "Ingot:Silver":
                            if (int.TryParse(value[1], out Ingot.MaxSilver))
                            {
                                Ingot.MaxSilver = Ingot.MaxSilver;
                            }
                            else
                            {
                                Ingot.MaxSilver = 1000;
                            }
                            break;
                        case "Ingot:Gold":
                            if (int.TryParse(value[1], out Ingot.MaxGold))
                            {
                                Ingot.MaxGold = Ingot.MaxGold;
                            }
                            else
                            {
                                Ingot.MaxGold = 1000;
                            }
                            break;
                        #endregion
                        #region Ores
                        case "Ore:Nickel":
                            if (int.TryParse(value[1], out Ore.MaxNickel))
                            {
                                Ore.MaxNickel = Ore.MaxNickel;
                            }
                            else
                            {
                                Ore.MaxNickel = 1000;
                            }
                            break;
                        case "Ore:Platinum":
                            if (int.TryParse(value[1], out Ore.MaxPlatinum))
                            {
                                Ore.MaxPlatinum = Ore.MaxPlatinum;
                            }
                            else
                            {
                                Ore.MaxPlatinum = 1000;
                            }
                            break;
                        case "Ore:Silicon":
                            if (int.TryParse(value[1], out Ore.MaxSilicon))
                            {
                                Ore.MaxSilicon = Ore.MaxSilicon;
                            }
                            else
                            {
                                Ore.MaxSilicon = 1000;
                            }
                            break;
                        case "Ore:Cobalt":
                            if (int.TryParse(value[1], out Ore.MaxCobalt))
                            {
                                Ore.MaxCobalt = Ore.MaxCobalt;
                            }
                            else
                            {
                                Ore.MaxCobalt = 1000;
                            }
                            break;
                        case "Ore:Iron":
                            if (int.TryParse(value[1], out Ore.MaxIron))
                            {
                                Ore.MaxIron = Ore.MaxIron;
                            }
                            else
                            {
                                Ore.MaxIron = 1000;
                            }
                            break;
                        case "Ore:Stone":
                            if (int.TryParse(value[1], out Ore.MaxStone))
                            {
                                Ore.MaxStone = Ore.MaxStone;
                            }
                            else
                            {
                                Ore.MaxStone = 1000;
                            }
                            break;
                        case "Ore:Silver":
                            if (int.TryParse(value[1], out Ore.MaxSilver))
                            {
                                Ore.MaxSilver = Ore.MaxSilver;
                            }
                            else
                            {
                                Ore.MaxSilver = 1000;
                            }
                            break;
                        case "Ore:Gold":
                            if (int.TryParse(value[1], out Ore.MaxGold))
                            {
                                Ore.MaxGold = Ore.MaxGold;
                            }
                            else
                            {
                                Ore.MaxGold = 1000;
                            }
                            break;
                        #endregion
                        #region displays
                        case "CockpitDriveChargeStatusTextPanel":
                            CockpitDriveChargeStatusTextPanel = (IMyTextPanel)GridTerminalSystem.GetBlockWithName(value[1]);
                            break;
                        case "textPanelJumpDriveDetailedInfo":
                            textPanelJumpDriveDetailedInfo = (IMyTextPanel)GridTerminalSystem.GetBlockWithName(value[1]);
                            break;
                        case "textPanelIngotDisplay":
                            textPanelIngotDisplay = (IMyTextPanel)GridTerminalSystem.GetBlockWithName(value[1]);
                            break;
                        case "textPanelOreDisplay":
                            textPanelOreDisplay = (IMyTextPanel)GridTerminalSystem.GetBlockWithName(value[1]);
                            break;
                        case "textPanelHydrogenDisplay":
                            textPanelOreDisplay = (IMyTextPanel)GridTerminalSystem.GetBlockWithName(value[1]);
                            break;
                        #endregion
                        default:
                            Echo("Config did not match : \n" +
                                 $"\"{value[0]}\"");
                            break;
                    }
#pragma warning restore CS1717 // Assignment made to same variable
                    #endregion
                }
            }
        }

        public void Main(string argument, UpdateType updateSource)
        {
            Echo($"Last run: {DateTime.Now}");
            /*
        private IMyTextPanel CockpitDriveChargeStatusTextPanel = null;
        private IMyTextPanel textPanelJumpDriveDetailedInfo = null;
        private IMyTextPanel textPanelIngotDisplay = null;
        private IMyTextPanel textPanelOreDisplay = null;
        */
            if (CockpitDriveChargeStatusTextPanel != null)
                Echo(CockpitDriveChargeStatusTextPanel.CustomName);
            if (textPanelJumpDriveDetailedInfo != null)
                Echo(textPanelJumpDriveDetailedInfo.CustomName);
            if (textPanelIngotDisplay != null)
                Echo(textPanelIngotDisplay.CustomName);
            if (textPanelOreDisplay != null)
                Echo(textPanelOreDisplay.CustomName);
            if (textPanelHydrogenDisplay != null)
                Echo(textPanelHydrogenDisplay.CustomName);

            if (CockpitDriveChargeStatusTextPanel != null)
            {
                List<IMyJumpDrive> jumpDrives = new List<IMyJumpDrive>();
                GridTerminalSystem.GetBlocksOfType(jumpDrives, x => x.IsSameConstructAs(Me));

                float[] driveCharge = new float[2];

                foreach (IMyJumpDrive drive in jumpDrives)
                {
                    driveCharge[0] += drive.MaxStoredPower;
                    driveCharge[1] += drive.CurrentStoredPower;
                }

                int barLength = 91;

                string outText = $"Drive Charge: {((driveCharge[1] / driveCharge[0]) * 100):000.00}%\n" +
                                 $"[";

                float onePercent = driveCharge[0] / barLength;

                for (float i = 0; i < barLength; i++)
                {
                    if (onePercent * i < driveCharge[1])
                        outText += "|";
                    else
                        outText += "'";
                }

                outText += "]";

                CockpitDriveChargeStatusTextPanel.FontColor =
                    driveCharge[0] == driveCharge[1] ? Color.Green : Color.Orange;
                CockpitDriveChargeStatusTextPanel.ContentType = ContentType.TEXT_AND_IMAGE;
                CockpitDriveChargeStatusTextPanel.WriteText(outText);

                if (textPanelJumpDriveDetailedInfo != null)
                    textPanelJumpDriveDetailedInfo.WriteText(jumpDrives[0].DetailedInfo);
                else
                    Echo("!textPanelJumpDriveDetailedInfo");
            }

            if (textPanelIngotDisplay != null)
            {
                List<IMyCargoContainer> cargoContainers = new List<IMyCargoContainer>();
                GridTerminalSystem.GetBlocksOfType(cargoContainers, x=>x.IsSameConstructAs(Me));

                    List<ItemInfo> itemInfos = new List<ItemInfo>();
                foreach (IMyCargoContainer container in cargoContainers)
                {
                    IMyInventory inventory = container.GetInventory();
                    List<MyInventoryItem> items = new List<MyInventoryItem>();
                    inventory.GetItems(items, x=>x.Type.TypeId == "MyObjectBuilder_Ingot");

                    foreach (MyInventoryItem item in items)
                    {
                        if(itemInfos.FindAll(x=>x.SubtypeId == item.Type.SubtypeId).Count == 0)
                        {
                            itemInfos.Add(new ItemInfo(item));
                        }
                        else
                        {
                            foreach (ItemInfo info in itemInfos)
                            {
                                if (info.SubtypeId == item.Type.SubtypeId)
                                {
                                    info.Amount += info.Amount;
                                }
                            }
                        }
                    }
                }

                string output = "   Ingots\n";
                foreach (ItemInfo info in itemInfos)
                {
                    output += $"{info.SubtypeId} : {info.Amount.ToIntSafe()}\n";
                    switch (info.SubtypeId)
                    {
                        case "Platinum":
                            output += $"{WriteProgressBar('=', ' ', info.Amount, 24, Ingot.MaxPlatinum)}\n";
                            break;
                        case "Nickel":
                            output += $"{WriteProgressBar('=', ' ', info.Amount, 24, Ingot.MaxNickel)}\n";
                            break;
                        case "Silicon":
                            output += $"{WriteProgressBar('=', ' ', info.Amount, 24, Ingot.MaxSilicon)}\n";
                            break;
                        case "Cobalt":
                            output += $"{WriteProgressBar('=', ' ', info.Amount, 24, Ingot.MaxCobalt)}\n";
                            break;
                        case "Iron":
                            output += $"{WriteProgressBar('=', ' ', info.Amount, 24, Ingot.MaxIron)}\n";
                            break;
                        case "Stone":
                            output += $"{WriteProgressBar('=', ' ', info.Amount, 24, Ingot.MaxStone)}\n";
                            break;
                        case "Silver":
                            output += $"{WriteProgressBar('=', ' ', info.Amount, 24, Ingot.MaxSilver)}\n";
                            break;
                        case "Gold":
                            output += $"{WriteProgressBar('=', ' ', info.Amount, 24, Ingot.MaxGold)}\n";
                            break;
                        default:
                            output += $"{WriteProgressBar('=', ' ', info.Amount, 24, 1000)}\n";
                            break;
                    }
                }

                textPanelIngotDisplay.WriteText(output);
            }

            if (textPanelOreDisplay != null)
            {
                Echo("Ores");

                List<IMyCargoContainer> cargoContainers = new List<IMyCargoContainer>();
                GridTerminalSystem.GetBlocksOfType(cargoContainers, x => x.IsSameConstructAs(Me));

                List<ItemInfo> itemInfos = new List<ItemInfo>();
                foreach (IMyCargoContainer container in cargoContainers)
                {
                    IMyInventory inventory = container.GetInventory();
                    List<MyInventoryItem> items = new List<MyInventoryItem>();
                    inventory.GetItems(items, x => x.Type.TypeId == "MyObjectBuilder_Ore");

                    foreach (MyInventoryItem item in items)
                    {
                        if (itemInfos.FindAll(x => x.SubtypeId == item.Type.SubtypeId).Count == 0)
                        {
                            itemInfos.Add(new ItemInfo(item));
                        }
                        else
                        {
                            foreach (ItemInfo info in itemInfos)
                            {
                                if (info.SubtypeId == item.Type.SubtypeId)
                                {
                                    info.Amount += info.Amount;
                                }
                            }
                        }
                    }
                }

                string output = "   Ores\n";
                foreach (ItemInfo info in itemInfos)
                {
                    output += $"{info.SubtypeId} : {info.Amount.ToIntSafe()}\n";
                    switch (info.SubtypeId)
                    {
                        case "Platinum":
                            output += $"{WriteProgressBar('=', ' ', info.Amount, 24, Ore.MaxPlatinum)}\n";
                            break;
                        case "Nickel":
                            output += $"{WriteProgressBar('=', ' ', info.Amount, 24, Ore.MaxNickel)}\n";
                            break;
                        case "Silicon":
                            output += $"{WriteProgressBar('=', ' ', info.Amount, 24, Ore.MaxSilicon)}\n";
                            break;
                        case "Cobalt":
                            output += $"{WriteProgressBar('=', ' ', info.Amount, 24, Ore.MaxCobalt)}\n";
                            break;
                        case "Iron":
                            output += $"{WriteProgressBar('=', ' ', info.Amount, 24, Ore.MaxIron)}\n";
                            break;
                        case "Stone":
                            output += $"{WriteProgressBar('=', ' ', info.Amount, 24, Ore.MaxStone)}\n";
                            break;
                        case "Silver":
                            output += $"{WriteProgressBar('=', ' ', info.Amount, 24, Ore.MaxSilver)}\n";
                            break;
                        case "Gold":
                            output += $"{WriteProgressBar('=', ' ', info.Amount, 24, Ore.MaxGold)}\n";
                            break;
                        default:
                            output += $"{WriteProgressBar('=', ' ', info.Amount, 24, 1000)}\n";
                            break;
                    }
                }

                textPanelOreDisplay.WriteText(output);
            }

            if (textPanelHydrogenDisplay != null)
            {
                List<IMyGasTank> gasTanks = new List<IMyGasTank>();
                GridTerminalSystem.GetBlocksOfType(gasTanks, x=>x.CustomName.Contains("Hydrogen Tank"));

                double filledRatio = 0;

                foreach (IMyGasTank tank in gasTanks)
                {
                    filledRatio += tank.FilledRatio;
                }

                textPanelHydrogenDisplay.WriteText(
                    $"Hydrogen:\n{WriteProgressBar('|', '\'', (int)(filledRatio * 100), 92, 100 * gasTanks.Count)}");
            }

            //IMyTextPanel powerUse = (IMyTextPanel) GridTerminalSystem.GetBlockWithName("Screen_Diagnostic_444_hv");

            //if (powerUse != null)
            //{
            //    List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();
            //    GridTerminalSystem.GetBlocksOfType(blocks);

            //    string output = "";

            //    foreach (IMyTerminalBlock block in blocks)
            //    {
            //        if (block.IsFunctional)
            //        {
            //            block.
            //        }
            //    }
            //}
        }

        private string WriteProgressBar(char filled, char empty, MyFixedPoint current, int length, MyFixedPoint max)
        {
            string s = "[";
            float onePercent = max.ToIntSafe() / (float)100;

            for (int i = 0; i < length; i++)
            {
                if (i * onePercent < current.ToIntSafe())
                    s += filled;
                else
                    s += empty;
            }

            return $"{s}]";
        }

        private class ItemInfo
        {
            public string TypeId;
            public string SubtypeId;
            public MyFixedPoint Amount;

            public ItemInfo(MyInventoryItem item)
            {
                TypeId = item.Type.TypeId;
                SubtypeId = item.Type.SubtypeId;
                Amount = item.Amount;
            }
        }
    }
}
