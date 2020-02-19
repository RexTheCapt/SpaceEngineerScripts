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

            Runtime.UpdateFrequency = UpdateFrequency.Once;
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

            List<IMyCargoContainer> cargoContainers = new List<IMyCargoContainer>();
            GridTerminalSystem.GetBlocksOfType(blocks: cargoContainers);

            List<MyInventoryItem> inventoryItems = new List<MyInventoryItem>();

            foreach (IMyCargoContainer container in cargoContainers)
            {
                IMyInventory myInventory = container.GetInventory();
                List<MyInventoryItem> items = new List<MyInventoryItem>();
                myInventory.GetItems(items: items);

                foreach (MyInventoryItem item in items)
                {
                    inventoryItems.Add(item: item);
                }
            }

            List<ItemCount> itemCounts = new List<ItemCount>();

            foreach (MyInventoryItem item in inventoryItems)
            {
                if (itemCounts.All(x => x.Type != item.Type))
                {
                    ItemCount itemCount = new ItemCount(item.Type);
                    itemCounts.Add(itemCount);

                    itemCount.Amount += item.Amount;
                }
                else
                {
                    ItemCount itemCount = itemCounts.FirstOrDefault(x => x.Type == item.Type);

                    if (itemCount != null)
                    {
                        itemCount.Amount += item.Amount;
                    }
                }
            }

            foreach (ItemCount count in itemCounts.OrderBy(x => x.Type).ToList())
            {
                Echo("1");
                Echo($"{count} : {count.Amount}");
            }
        }

        class ItemCount
        {
            public readonly MyItemType Type;

            public ItemCount(MyItemType type)
            {
                Type = type;
            }

            public MyFixedPoint Amount = 0;

            public override string ToString()
            {
                string output = "";

                switch (Type.TypeId)
                {
                    case "MyObjectBuilder_PhysicalGunObject":
                        output += "[Tool]";
                        break;
                    case "MyObjectBuilder_Component":
                        output += "[Comp]";
                        break;
                    case "MyObjectBuilder_Ore":
                        output += "[Ore]";
                        break;
                    case "MyObjectBuilder_Ingot":
                        output += "[Ingot]";
                        break;
                    case "MyObjectBuilder_Datapad":
                        output += "[Pad]";
                        break;
                    case "MyObjectBuilder_AmmoMagazine":
                        output += "[Ammo]";
                        break;
                    case "MyObjectBuilder_PhysicalObject":
                        output += "[Object]";
                        break;
                    case "MyObjectBuilder_ConsumableItem":
                        output += "[Consume]";
                        break;
                    default:
                        output += $"[{Type.TypeId}]";
                        break;
                }

                return $"{output} {Type.SubtypeId}";
            }
        }
    }
}
