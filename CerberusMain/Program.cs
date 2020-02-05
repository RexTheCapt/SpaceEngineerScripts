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

        private int IngotMaxNickel,
            IngotMaxPlatinum,
            IngotMaxSilicon,
            IngotMaxCobalt,
            IngotMaxIron,
            IngotMaxStone,
            IngotMaxSilver,
            IngotMaxGold;

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
                    $"// Ingot settings\n" +
                    $"Ingot:Nickel=1000\n" +
                    $"Ingot:Platinum=1000\n" +
                    $"Ingot:Silicon=1000\n" +
                    $"Ingot:Cobalt=1000\n" +
                    $"Ingot:Iron=1000\n" +
                    $"Ingot:Stone=1000\n" +
                    $"Ingot:Silver=1000\n" +
                    $"Ingot:Gold=1000\n";
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

                    switch (value[0])
                    {
                        case "Ingot:Nickel":
                            if (int.TryParse(value[1], out IngotMaxNickel))
                            {
                                //IngotMaxNickel = val;
                            }
                            else
                            {
                                IngotMaxNickel = 1000;
                            }
                            break;
                        case "Ingot:Platinum":
                            if (int.TryParse(value[1], out IngotMaxPlatinum))
                            {
                                //IngotMaxPlatinum = val;
                            }
                            else
                            {
                                IngotMaxPlatinum = 1000;
                            }
                            break;
                        case "Ingot:Silicon":
                            if (int.TryParse(value[1], out IngotMaxSilicon))
                            {
                                //IngotMaxSilicon = val;
                            }
                            else
                            {
                                IngotMaxSilicon = 1000;
                            }
                            break;
                        case "Ingot:Cobalt":
                            if (int.TryParse(value[1], out IngotMaxCobalt))
                            {
                                //IngotMaxCobalt = val;
                            }
                            else
                            {
                                IngotMaxCobalt = 1000;
                            }
                            break;
                        case "Ingot:Iron":
                            if (int.TryParse(value[1], out IngotMaxIron))
                            {
                                //IngotMaxIron = val;
                            }
                            else
                            {
                                IngotMaxIron = 1000;
                            }
                            break;
                        case "Ingot:Stone":
                            if (int.TryParse(value[1], out IngotMaxStone))
                            {
                                //IngotMaxStone = val;
                            }
                            else
                            {
                                IngotMaxStone = 1000;
                            }
                            break;
                        case "Ingot:Silver":
                            if (int.TryParse(value[1], out IngotMaxSilver))
                            {
                                //IngotMaxSilver = val;
                            }
                            else
                            {
                                IngotMaxSilver = 1000;
                            }
                            break;
                        case "Ingot:Gold":
                            if (int.TryParse(value[1], out IngotMaxNickel))
                            {
                                //IngotMaxNickel = val;
                            }
                            else
                            {
                                IngotMaxNickel = 1000;
                            }
                            break;
                        case "CockpitDriveChargeStatusTextPanel":
                            CockpitDriveChargeStatusTextPanel = (IMyTextPanel) GridTerminalSystem.GetBlockWithName(value[1]);
                            break;
                        case "textPanelJumpDriveDetailedInfo":
                            textPanelJumpDriveDetailedInfo = (IMyTextPanel) GridTerminalSystem.GetBlockWithName(value[1]);
                            break;
                        case "textPanelIngotDisplay":
                            textPanelIngotDisplay = (IMyTextPanel) GridTerminalSystem.GetBlockWithName(value[1]);
                            break;
                        case "textPanelOreDisplay":
                            textPanelOreDisplay = (IMyTextPanel) GridTerminalSystem.GetBlockWithName(value[1]);
                            break;
                        default:
                            Echo("Config did not match : \n" +
                                 $"\"{value[0]}\"");
                            break;
                    }
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

                bool append = false;

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
                    output += $"{info.SubtypeId}\n";
                    switch (info.SubtypeId)
                    {
                        case "Platinum":
                            output += $"{WriteProgressBar('=', ' ', info.Amount, 24, IngotMaxPlatinum)}\n";
                            break;
                        case "Nickel":
                            output += $"{WriteProgressBar('=', ' ', info.Amount, 24, IngotMaxNickel)}\n";
                            break;
                        case "Silicon":
                            output += $"{WriteProgressBar('=', ' ', info.Amount, 24, IngotMaxSilicon)}\n";
                            break;
                        case "Cobalt":
                            output += $"{WriteProgressBar('=', ' ', info.Amount, 24, IngotMaxCobalt)}\n";
                            break;
                        case "Iron":
                            output += $"{WriteProgressBar('=', ' ', info.Amount, 24, IngotMaxIron)}\n";
                            break;
                        case "Stone":
                            output += $"{WriteProgressBar('=', ' ', info.Amount, 24, IngotMaxStone)}\n";
                            break;
                        case "Silver":
                            output += $"{WriteProgressBar('=', ' ', info.Amount, 24, IngotMaxSilver)}\n";
                            break;
                        case "Gold":
                            output += $"{WriteProgressBar('=', ' ', info.Amount, 24, IngotMaxGold)}\n";
                            break;
                        default:
                            output += $"{WriteProgressBar('=', ' ', info.Amount, 24, 1000)}\n";
                            break;
                    }
                }

                textPanelIngotDisplay.WriteText(output);
            }
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
