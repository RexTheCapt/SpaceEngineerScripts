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
        /* How to use this script:  
1. create a programming block and load this script into it.  
2. Create a timer block, set it to run the programming block with no arguments and to "trigger now" itself  
3. Create any number of LCD displays or text displays.  
a. Name the displays according to the view you want it to show:  
screen_diagnostic_x_y where X is the rotation (0, 1, or 2) and Y is the style (0=scan, 1=flat)  
OR  
screen_diagnostic_main - this display shows one view that can be changed with buttons (discussed later) 
ALSO
Add an _h, an _v, or an _hv to show multiple views on the same screen.
Example : screen_diagnostic_main_hv or screen_diagnostic_5_1_v
FURTHERMORE 
Name a display screen_diagnostic_reports to get some basic information about your ships health.
4. Start your timer and your displays should buzz to life.  
5. You can create buttons or toolbar slots that execute this script with arguments to perform special functions.  
Argument : Function that happens  
reset : Resets the program, tells the script that your ship is brand new as it is right now.  
rotate : Rotates the view on ship_diagnostic_main.  
mode : Changes the display style on ship_diagnostic_main between flat/scan styles 
save : Save the ship data manually  (usually happens automatically)  
load : Load the ship data manually (usually happens automatically)  
cursor : No longer does anything. Had to cut it! It may come back. For now use "show on hud" in terminal
*/
        //Customization:======================================================================
        string panel_prefix = "screen_diagnostic_";
        string alarm_prefix = "diag_alarm_";
        int alarm_refresh_rate = 30;
        int alarm_tracker = 0;

        int xscale = 1;
        int yscale = 1;
        int zscale = 1;

        int render_slowness = 4; // lower is faster animation (0 is minimum)

        int border_width = 2; //how much space to give around the edges (REQUIRES RESET)

        //Pixels
        char gone = ''; //red
        char normal = '';  //green
        char none = '';   //black
        char hacked = '';  //blue
        char broken = '';  //yellow
        char shadow = '';//gray
        char cursor = '';  //blue

        ///Performance tweaks=================================================================
        /*change these to change performance of the script   
            If the combination of these three are too high you may hit an instruction limit and the script will break   
            If any one of these is too high you may experience performance issues (game slows down)   
            If any of these are too low you may get some delay before blocks are updated.   
            The script will never update more times per tick than the number of blocks in the ship   
            so this is really only important for huge ships.   
            I have found 100/100/10 to be a good balance.   
            As a general rule, it doesn't make sense to render faster than you update.
            Rendering only happens when changes are made. 
            max_terminal_status_checks_per_tick should be like 1/4 of what the other two are.   
        */

        int max_renders_per_tick = 100; //this many blocks' pixels will be refreshed every tick   
        int max_updates_per_tick = 50; //this many blocks will be checked for destruction per tick   
        int max_terminal_status_checks_per_tick = 10; //script will check for hacking/damage to this many terminal blocks per tick.   
        int initial_limit = 250;

        //=====================================================================================
        //Everything below should not be touched unless you REALLY know what you're doing
        /////////////////

        List<Vector3I> coords_ship_full = new List<Vector3I> { };
        List<Vector3I> coords_ship_now = new List<Vector3I> { };
        List<IMyTerminalBlock> blocks_terminal = new List<IMyTerminalBlock> { };
        List<Vector3I> coords_terminal_full = new List<Vector3I> { };
        List<Vector3I> coords_terminal_now = new List<Vector3I> { };
        List<Vector3I> coords_terminal_working = new List<Vector3I> { };
        List<Vector3I> coords_terminal_nothacked = new List<Vector3I> { };
        List<Vector3I> coords_ship_checked = new List<Vector3I> { };
        List<Vector3I> coords_ship_to_check = new List<Vector3I> { };
        List<IMyTerminalBlock> blocks_ship_text_panel = new List<IMyTerminalBlock> { };
        List<Vector3I> coords_to_render = new List<Vector3I> { };

        List<Vector3I> coords_gyro_full = new List<Vector3I> { };
        List<Vector3I> coords_gyro_now = new List<Vector3I> { };
        List<Vector3I> coords_thrust_full = new List<Vector3I> { };
        List<Vector3I> coords_thrust_now = new List<Vector3I> { };
        List<Vector3I> coords_weapon_full = new List<Vector3I> { };
        List<Vector3I> coords_weapon_now = new List<Vector3I> { };
        List<Vector3I> coords_power_full = new List<Vector3I> { };
        List<Vector3I> coords_power_now = new List<Vector3I> { };

        Vector3I shipCoordMin = new Vector3I { };
        Vector3I shipCoordMax = new Vector3I { };
        Vector3I shipSizeVector = new Vector3I { };
        Vector3I UpVec = new Vector3I(1, 0, 0);
        Vector3I DnVec = new Vector3I(-1, 0, 0);
        Vector3I LfVec = new Vector3I(0, 1, 0);
        Vector3I RtVec = new Vector3I(0, -1, 0);
        Vector3I FwVec = new Vector3I(0, 0, 1);
        Vector3I BkVec = new Vector3I(0, 0, -1);

        IMyCubeGrid myGrid;

        int rotation = 0;
        int render_mode = 0;
        List<int> cycling_index = new List<int> { 0, 0, 0, 0, 0, 0 };
        int state = 5;
        int num_blocks_in_ship = 0;
        int num_terminals_in_ship = 0;
        int full_blocks_index = 0; //keeps track of all loops that go over all blocks in ship  
        int terminal_blocks_index = 0; //keeps track of how far we are through checking terminal blocks  
                                       //int render_index =0; //keeps track of how far we are in the render process  
        int render_skipper = 0; //index to track the render_slowness  
        int divider_position = 0;//used for scanning through data from Storage   
                                 //int damage_iterator=0; 
                                 //int total_blocks_to_render=0; 
        List<int> vert_rot_mapper = new List<int> { 1, 0, 3, 2, 5, 4 };
        List<int> hor_rot_mapper = new List<int> { 5, 2, 1, 4, 3, 0 };

        int gyrohealth = 0;
        int thrusterhealth = 0;
        int hullhealth = 0;
        int weaponhealth = 0;
        int systemhealth = 0;
        int powerhealth = 0;

        Vector3I cursor_vec = new Vector3I(0, 0, 0);//x,y  

        string[][][] rendered_images;//The jagged array of strings (images) that represent the ship  
        string[][][] rendered_shadows;//The jagged array of strings (images) that represent the ship's shadow.  

        //bool show_damage= true;  
        bool shadows_rendered = false;
        bool shadows_baked = false;
        bool drawcursor = false;

        public Program()
        {
            //firstBlock = Me;  
            myGrid = Me.CubeGrid;
            shipCoordMin = myGrid.Min;
            shipCoordMax = myGrid.Max;
            if (border_width < 1) border_width = 1;
        }

        public void Main(string argument)
        {
            switch (state)
            {
                case 0: //terminal block status initalization  
                    shadows_rendered = false;
                    shadows_baked = false;
                    get_terminal_blocks();
                    coords_terminal_now = new List<Vector3I>(coords_terminal_full);
                    update_terminal_block_status(50); //this actually doesn't iterate more than one tick. Doesn't need to.
                    if (Storage.Length == 0) coords_ship_to_check.AddRange(coords_terminal_full);
                    state = 1;
                    break;
                case 1:  //all other block intitialization  
                    Echo("Initializing" + coords_ship_to_check.Count.ToString());
                    if (Storage.Length == 0) initialize_ship();
                    if (coords_ship_to_check.Count == 0)
                    {
                        state = 2;
                        num_blocks_in_ship = coords_ship_full.Count;
                        coords_to_render = new List<Vector3I>(coords_ship_full);
                        full_blocks_index = 0;
                    }
                    break;
                case 2: //generate blank background images  
                    Echo("STATE2");
                    shadows_rendered = false;
                    determine_render_limits(initial_limit); //testing this out. 100 should be safe. otherwise max_renders_per_tick.
                    if (full_blocks_index >= num_blocks_in_ship)
                    {
                        generate_backgrounds();
                        if (Storage.Length > 0)
                        {
                            state = 3;
                            //render_index=0; 
                            coords_to_render = new List<Vector3I>(coords_ship_full);
                        }
                        if (Storage.Length == 0 && num_blocks_in_ship > 0)
                        {
                            full_blocks_index = 0;
                            state = 4;
                        }
                    }
                    break;
                case 3: //Main state - drawing, updating, and watching for inputs  
                    string arg = argument.ToLower();
                    alarm_tracker--;
                    if (alarm_tracker <= 0)
                    {
                        trigger_external_timers();
                        alarm_tracker = alarm_refresh_rate;
                    }
                    switch (arg)
                    {
                        case "reset":
                            state = 6;
                            break;
                        case "rotate":
                            rotation = (rotation + 1) % 6;
                            break;
                        case "mode":
                            render_mode = (render_mode + 1) % 2;
                            break;
                        case "save":
                            full_blocks_index = 0;//counter for saving  
                            state = 4;
                            break;
                        case "load":
                            divider_position = 0;//counter for loading  
                            if (Storage.Length > 0) state = 5;
                            break;
                        case "cursor":
                            drawcursor = !drawcursor;
                            break;
                        case "showdamaged":
                            show_damage_on_hud();
                            break;
                        case "":
                            break;
                        default:
                            set_cursor_position(argument);
                            break;
                    }
                    update_ship_block_existence(max_updates_per_tick);
                    update_terminal_block_status(max_terminal_status_checks_per_tick);
                    update_rendered_images(max_renders_per_tick);
                    render_skipper++;
                    if (render_skipper > render_slowness)
                    {
                        Echo("Main loop:");
                        Echo("Terminals: " + coords_terminal_full.Count.ToString() + "/" + coords_terminal_now.Count.ToString());
                        Echo("Updating: " + coords_ship_now.Count.ToString() + "/" + coords_ship_full.Count.ToString());
                        Echo("Rendering: " + coords_to_render.Count.ToString());
                        cycling_index[0] = (cycling_index[0] + 1) % Math.Abs(Rotate3I(shipSizeVector, 0).Z);
                        cycling_index[1] = (cycling_index[1] + 1) % Math.Abs(Rotate3I(shipSizeVector, 1).Z);
                        cycling_index[2] = (cycling_index[2] + 1) % Math.Abs(Rotate3I(shipSizeVector, 2).Z);
                        cycling_index[3] = (cycling_index[3] + 1) % Math.Abs(Rotate3I(shipSizeVector, 3).Z);
                        cycling_index[4] = (cycling_index[4] + 1) % Math.Abs(Rotate3I(shipSizeVector, 4).Z);
                        cycling_index[5] = (cycling_index[5] + 1) % Math.Abs(Rotate3I(shipSizeVector, 5).Z);
                        draw_images_to_screens();
                        render_skipper = 0;
                    }
                    update_healths();
                    break;
                case 4: //save data  
                        //full_blocks_index should be set to 0 before switching to this state.  
                    Echo("saving");
                    save_data(initial_limit);
                    break;
                case 5: //load Data (replaces initialize ship)  
                        //divider_position should be set to 0 before switching to this state.  
                    Echo("Loading");
                    if (Storage.Length == 0)
                    {
                        state = 0;
                        break;
                    }
                    load_data(initial_limit);
                    break;
                case 6: //delete
                    Storage = "";
                    coords_ship_full = new List<Vector3I> { };
                    coords_ship_now = new List<Vector3I> { };
                    blocks_terminal = new List<IMyTerminalBlock> { };
                    coords_terminal_full = new List<Vector3I> { };
                    coords_terminal_now = new List<Vector3I> { };
                    coords_terminal_working = new List<Vector3I> { };
                    coords_terminal_nothacked = new List<Vector3I> { };
                    coords_ship_checked = new List<Vector3I> { };
                    coords_ship_to_check = new List<Vector3I> { };
                    coords_gyro_full = new List<Vector3I> { };
                    coords_gyro_now = new List<Vector3I> { };
                    coords_thrust_full = new List<Vector3I> { };
                    coords_thrust_now = new List<Vector3I> { };
                    coords_weapon_full = new List<Vector3I> { };
                    coords_weapon_now = new List<Vector3I> { };
                    coords_power_now = new List<Vector3I> { };
                    coords_power_full = new List<Vector3I> { };
                    blocks_ship_text_panel = new List<IMyTerminalBlock> { };
                    coords_to_render = new List<Vector3I> { };
                    shipCoordMin = myGrid.Min;
                    shipCoordMax = myGrid.Max;
                    state = 0;
                    break;
                default:
                    break;
            }
        }

        private void get_terminal_blocks()
        {
            blocks_terminal = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(blocks_terminal);
            foreach (IMyTerminalBlock block in blocks_terminal)
            {
                if (block.CubeGrid == Me.CubeGrid)
                {
                    coords_terminal_full.Add(block.Position);
                    if (block is IMyThrust) { coords_thrust_full.Add(block.Position); }
                    if (block is IMyGyro) { coords_gyro_full.Add(block.Position); }
                    if (is_weapon(block)) { coords_weapon_full.Add(block.Position); }
                }
            }
            num_terminals_in_ship = blocks_terminal.Count;
        }

        private void update_terminal_block_status(int limit)
        {
            int count = 0;
            blocks_terminal = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(blocks_terminal);
            if (terminal_blocks_index >= blocks_terminal.Count)
            {
                terminal_blocks_index = 0;
            }
            while (count < limit && terminal_blocks_index < blocks_terminal.Count)
            {
                count++;
                IMyTerminalBlock block = blocks_terminal[terminal_blocks_index];
                Vector3I pos = block.Position;
                if (!coords_ship_full.Contains(pos))
                {  //ignore blocks we didn't find at initialization. (safer)   
                    terminal_blocks_index++;
                    continue;
                }
                if (!myGrid.CubeExists(block.Position))
                { //Note :if the block is gone we can still reference it.
                    safely_remove(coords_terminal_now, pos);
                    safely_remove(coords_terminal_working, pos);
                    safely_remove(coords_terminal_nothacked, pos);
                    safely_remove(coords_gyro_now, pos);
                    safely_remove(coords_thrust_now, pos);
                    safely_remove(coords_weapon_now, pos);
                    safely_remove(coords_power_now, pos);
                }
                else if (coords_terminal_full.Contains(pos))
                {
                    safely_add(coords_terminal_now, pos);
                    if (block.IsFunctional) { safely_add(coords_terminal_working, pos); }
                    if (!block.IsBeingHacked) { safely_add(coords_terminal_nothacked, pos); }
                    if (!block.IsFunctional) { safely_remove(coords_terminal_working, pos); }
                    if (block.IsBeingHacked) { safely_remove(coords_terminal_nothacked, pos); }
                    if (block is IMyThrust) { safely_add(coords_thrust_now, pos); safely_add(coords_thrust_full, pos); }
                    if (block is IMyGyro) { safely_add(coords_gyro_now, pos); safely_add(coords_gyro_full, pos); }
                    if (is_weapon(block)) { safely_add(coords_weapon_now, pos); safely_add(coords_weapon_full, pos); }
                    if (is_power(block)) { safely_add(coords_power_now, pos); safely_add(coords_power_full, pos); }
                }
                terminal_blocks_index++;
            }
        }

        private void update_ship_block_existence(int limit)
        {
            int count = 0;
            if (full_blocks_index >= num_blocks_in_ship) full_blocks_index = 0;
            while (count < limit && full_blocks_index < num_blocks_in_ship)
            {
                Vector3I testvec = coords_ship_full[full_blocks_index];
                if (!myGrid.CubeExists(testvec))
                {
                    safely_remove(coords_ship_now, testvec);
                    safely_remove(coords_terminal_working, testvec);
                    safely_remove(coords_terminal_nothacked, testvec);
                    safely_remove(coords_terminal_now, testvec);
                    safely_remove(coords_gyro_now, testvec);
                    safely_remove(coords_thrust_now, testvec);
                    safely_remove(coords_weapon_now, testvec);
                    safely_remove(coords_power_now, testvec);
                }
                else
                {
                    safely_add(coords_ship_now, testvec);
                }
                full_blocks_index++;
                count++;
            }
        }

        private void determine_render_limits(int limit)
        {
            Echo("determining limits");
            int count = 0;
            Vector3I border_vector = new Vector3I(border_width, border_width, border_width);
            Vector3I vec = new Vector3I { };
            while (count < limit && full_blocks_index < num_blocks_in_ship)
            {
                vec = coords_ship_full[full_blocks_index];
                if (vec.X < shipCoordMin.X) shipCoordMin.X = vec.X;
                if (vec.X > shipCoordMax.X) shipCoordMax.X = vec.X;
                if (vec.Y < shipCoordMin.Y) shipCoordMin.Y = vec.Y;
                if (vec.Y > shipCoordMax.Y) shipCoordMax.Y = vec.Y;
                if (vec.Z < shipCoordMin.Z) shipCoordMin.Z = vec.Z;
                if (vec.Z > shipCoordMax.Z) shipCoordMax.Z = vec.Z;
                count++;
                full_blocks_index++;
            }
            if (full_blocks_index < num_blocks_in_ship) return;
            shipCoordMin -= border_vector;
            shipCoordMax += border_vector;
            shipSizeVector = shipCoordMax - shipCoordMin;
            rendered_images = new String[6][][];
            rendered_shadows = new String[6][][];
            for (int r = 0; r < 6; r++)
            {
                Vector3I absShipSize = Vector3I.Abs(Rotate3I(shipSizeVector, r));
                int[] tempSizeArray = { absShipSize.X, absShipSize.Y, absShipSize.Z };
                rendered_images[r] = new String[2][];
                rendered_shadows[r] = new String[2][];
                for (int j = 0; j < 2; j++)
                {
                    rendered_images[r][j] = new String[tempSizeArray[2]];
                    rendered_shadows[r][j] = new String[tempSizeArray[2]];
                }
            }
        }

        private void generate_backgrounds()
        {
            Echo("Generating backgrounds");
            //Fill the image strings with empty characters  
            //the following should be pretty darn fast.  
            for (int r = 0; r < 6; r++)
            {
                Vector3I rotvec = Vector3I.Abs(Rotate3I(shipSizeVector, r));
                //construct one horizontal line.  
                string line_string = new String(none, rotvec.X) + "\n";
                //Construct a block from horizontal lines  
                string block_string = "";
                for (int i = 0; i < rotvec.Y; i++)
                {
                    block_string = block_string + line_string;
                }
                //set background for flat image mode (1)  
                rendered_images[r][1][0] = block_string;
                rendered_shadows[r][1][0] = block_string;
                for (int i = 0; i < rotvec.Z; i++)
                {
                    //set background for cross section mode (2)  
                    rendered_images[r][0][i] = block_string +
                        rotvec.X.ToString() + "x" + rotvec.Y.ToString() +
                        "@Z:" + i.ToString() + " r=" + r.ToString();
                }
            }
        }

        private void update_rendered_images(int limit)
        {
            int count = 0;
            char newchar = 'X';
            if (!shadows_baked) { limit = initial_limit; }
            if (coords_to_render.Count == 0)
            {
                //render_index=0;  
                //show_damage=false; 
                //damage_iterator=(damage_iterator+1)%4; 
                //if (damage_iterator==0) show_damage=false; 
                if (shadows_rendered) shadows_baked = true;
                shadows_rendered = true;
            }
            if (shadows_rendered && !shadows_baked)
            {
                //The reason we're doing this is to avoid crazy render state dependency for a one-time function.   
                bake_shadows();
                coords_to_render = new List<Vector3I>(coords_ship_full);
                return;
            }
            while (count < limit && coords_to_render.Count > 0)
            {
                count++;
                Vector3I testvec = coords_to_render[0];
                newchar = normal;
                if (!coords_ship_now.Contains(testvec)) newchar = gone;
                if (coords_terminal_now.Contains(testvec))
                {
                    if (!coords_terminal_working.Contains(testvec)) newchar = broken;
                    if (!coords_terminal_nothacked.Contains(testvec)) newchar = hacked;
                }
                for (int r = 0; r < 6; r++)
                {
                    Vector3I rotsize = Vector3I.Abs(Rotate3I(shipSizeVector, r));
                    Vector3I rotscanned = Vector3I.Abs(Rotate3I(testvec - shipCoordMin, r));
                    Vector3I rotcursor = Vector3I.Abs(Rotate3I(cursor_vec - shipCoordMin, r));
                    if (drawcursor && (
                        rotscanned.X == rotcursor.X || rotscanned.Y == rotcursor.Y)) newchar = cursor;
                    int xyindex = rotscanned.X + (rotsize.Y - rotscanned.Y) * (rotsize.X + 1);
                    char oldchar = GetCharAt(rendered_images[r][0][rotscanned.Z], xyindex);
                    char oldchar_flat = GetCharAt(rendered_images[r][1][0], xyindex);
                    if (!shadows_rendered)
                    {
                        rendered_shadows[r][1][0] = ReplaceAt(rendered_shadows[r][1][0], xyindex, shadow);
                        continue;
                    }
                    for (int i = 0; i < 2; i++)
                    {
                        if (i == 1)
                        {
                            rendered_images[r][i][0] = ReplaceAt(rendered_images[r][i][0], xyindex, newchar);
                        }
                        if (i == 0)
                        {
                            rendered_images[r][i][rotscanned.Z] = ReplaceAt(rendered_images[r][i][rotscanned.Z], xyindex, newchar);
                        }
                    }
                }
                coords_to_render.Remove(testvec);
            }
        }

        private void bake_shadows()
        {
            shadows_baked = true;
            Echo("Baking");
            for (int r = 0; r < 6; r++)
            {
                Vector3I rotsize = Vector3I.Abs(Rotate3I(shipSizeVector, r));
                for (int i = 0; i < rotsize.Z; i++)
                {
                    int progress = Convert.ToInt32(i * (rotsize.X / rotsize.Z));
                    rendered_images[r][0][i] = rendered_shadows[r][1][0] + new string(cursor, progress);
                }
            }
        }

        private void draw_images_to_screens()
        {
            Echo("Updating screens");
            blocks_ship_text_panel = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyTextPanel>(blocks_ship_text_panel);
            foreach (IMyTextPanel txt in blocks_ship_text_panel)
            {
                string cname = txt.CustomName.ToLower();
                //Echo(cname);
                int namestart = cname.IndexOf(panel_prefix);
                if (namestart >= 0)
                {
                    Echo("Found screen: " + cname);
                    txt.ShowPublicTextOnScreen();
                    if (cname.IndexOf("report") > 0)
                    {
                        draw_health_report(txt);
                        continue;
                    }
                    int rot, mod, ind, indh, indv, indhv;
                    if (cname.IndexOf("main") >= 0)
                    {
                        rot = rotation;
                        mod = render_mode;
                        ind = cycling_index[rot];
                        indh = cycling_index[hor_rot_mapper[rot]];
                        indv = cycling_index[vert_rot_mapper[rot]];
                        indhv = cycling_index[hor_rot_mapper[vert_rot_mapper[rot]]];
                        if (mod > 0) { ind = 0; indh = 0; indv = 0; indhv = 0; mod = 1; }
                    }
                    else
                    {
                        if (cname.Length >= (namestart + panel_prefix.Length + 1))
                        {
                            Int32.TryParse(cname.Substring(namestart + panel_prefix.Length, 1), out rot);
                        }
                        else { rot = 0; }
                        if (cname.Length >= (namestart + panel_prefix.Length + 3))
                        {
                            Int32.TryParse(cname.Substring(namestart + panel_prefix.Length + 2, 1), out mod);
                        }
                        else { mod = 0; }
                        if (rot > 5 || rot < 0) rot = 0;
                        ind = cycling_index[rot];
                        indh = cycling_index[hor_rot_mapper[rot]];
                        indv = cycling_index[vert_rot_mapper[rot]];
                        indhv = cycling_index[hor_rot_mapper[vert_rot_mapper[rot]]];
                        if (mod > 0) { ind = 0; indh = 0; indv = 0; indhv = 0; mod = 1; }
                    }
                    Echo("[" + rot.ToString() + "," + mod.ToString() + "]");
                    txt.WritePublicText(rendered_images[rot][mod][ind]);
                    if (cname.IndexOf("_h") >= 0)
                    {
                        txt.WritePublicText(
                            concat_horizontal(rendered_images[rot][mod][ind], rendered_images[hor_rot_mapper[rot]][mod][indh]));
                    }
                    if (cname.IndexOf("_v") >= 0)
                    {
                        txt.WritePublicText(rendered_images[rot][mod][ind]);
                        txt.WritePublicText(rendered_images[vert_rot_mapper[rot]][mod][indv], true);
                    }
                    if (cname.IndexOf("_hv") >= 0)
                    {
                        txt.WritePublicText(
                            concat_horizontal(rendered_images[rot][mod][ind], rendered_images[hor_rot_mapper[rot]][mod][indh]));
                        txt.WritePublicText(
                            concat_horizontal(rendered_images[vert_rot_mapper[rot]][mod][indv],
                                rendered_images[hor_rot_mapper[vert_rot_mapper[rot]]][mod][indhv]), true);
                    }
                }
            }
        }

        private Vector3I Rotate3I(Vector3I vec, int axis)
        {
            int xsiz = shipSizeVector.X;
            int ysiz = shipSizeVector.Y;
            int zsiz = shipSizeVector.Z;
            //rotates differently depending on ships dimensions. Tries to get wide on horizontal. 
            switch (axis)
            {
                case 0: //x  
                    return new Vector3I(vec.Z * zscale, -vec.X * xscale, vec.Y * yscale);
                case 1: //y  
                    return new Vector3I(-vec.Z * zscale, vec.Y * yscale, vec.X * xscale);
                case 2: //z  
                    return new Vector3I(vec.X * xscale, vec.Y * yscale, vec.Z * zscale);
                case 3: //x  rot 90 
                    return new Vector3I(vec.X * xscale, -vec.Z * zscale, vec.Y * yscale);
                case 4: //y  rot 90 
                    return new Vector3I(-vec.Y * yscale, vec.Z * zscale, vec.X * xscale);
                case 5: //z  rot 90 
                    return new Vector3I(vec.Y * yscale, vec.X * xscale, vec.Z * zscale);
                default:
                    return vec;
            }
        }

        public string ReplaceAt(string input, int index, char newChar)
        {
            //index will be something like x + y*(width+1) (the +1 is because of the \n)  
            char[] chars = input.ToCharArray();
            chars[index] = newChar;
            return new string(chars);
        }

        public char GetCharAt(string input, int index)
        {
            char[] chars = input.ToCharArray();
            return chars[index];
        }

        private void initialize_ship()
        {
            int count = 0;//limiter on instruction count.  
            Vector3I c;
            while (coords_ship_to_check.Count > 0)
            {
                c = coords_ship_to_check[0];
                coords_ship_to_check.Remove(c);
                if (!coords_ship_checked.Contains(c))
                {
                    if (myGrid.CubeExists(c))
                    {
                        if (!coords_ship_full.Contains(c)) { coords_ship_full.Add(c); }
                        if (!coords_ship_checked.Contains(c + UpVec)) coords_ship_to_check.Add(c + UpVec);
                        if (!coords_ship_checked.Contains(c + DnVec)) coords_ship_to_check.Add(c + DnVec);
                        if (!coords_ship_checked.Contains(c + LfVec)) coords_ship_to_check.Add(c + LfVec);
                        if (!coords_ship_checked.Contains(c + RtVec)) coords_ship_to_check.Add(c + RtVec);
                        if (!coords_ship_checked.Contains(c + FwVec)) coords_ship_to_check.Add(c + FwVec);
                        if (!coords_ship_checked.Contains(c + BkVec)) coords_ship_to_check.Add(c + BkVec);
                    }
                    coords_ship_checked.Add(c);
                }
                count++;
                if (count > initial_limit) break;
            }
        }

        private void save_data(int limit)
        {
            Echo("Saving");
            int count = 0;
            if (full_blocks_index >= num_blocks_in_ship) state = 3;
            while (count < limit && full_blocks_index < num_blocks_in_ship)
            {
                //Storage MUST end with a "|" character for loading to work.  
                Storage = Storage + coords_ship_full[full_blocks_index].ToString();
                full_blocks_index++;
                count++;
            }
        }

        private void load_data(int limit)
        {
            Echo("loading" + divider_position.ToString());
            int count = 0;
            //Echo((Storage.IndexOf('[',divider_position+1)).ToString());  
            //Echo((Storage.IndexOf('[',divider_position+2)).ToString());  
            int next_position;
            //if there are no more instances of the divider OR we are at/beyond the end  
            if (Storage.IndexOf('[', divider_position + 1) == -1 || divider_position >= Storage.Length)
            {
                coords_ship_checked = new List<Vector3I> { };//nothing to check after loading.  
                state = 0;
            }
            while (count < limit && Storage.IndexOf(']', divider_position + 1) != -1 && divider_position < Storage.Length)
            {
                //[X:xx, Y:yy, Z:zz][X:xx, Y:yy, Z:zz]...  
                next_position = Storage.IndexOf('[', divider_position + 2);
                if (next_position == -1) break;
                int x, y, z;
                int[] xpos = new int[2], ypos = new int[2], zpos = new int[2];
                xpos[0] = Storage.IndexOf("X:", divider_position) + 2;
                xpos[1] = Storage.IndexOf(", Y:", divider_position);
                ypos[0] = Storage.IndexOf(", Y:", divider_position) + 4;
                ypos[1] = Storage.IndexOf(", Z:", divider_position);
                zpos[0] = Storage.IndexOf(", Z:", divider_position) + 4;
                zpos[1] = Storage.IndexOf("]", divider_position);
                string xs, ys, zs;
                xs = Storage.Substring(xpos[0], xpos[1] - xpos[0]);
                ys = Storage.Substring(ypos[0], ypos[1] - ypos[0]);
                zs = Storage.Substring(zpos[0], zpos[1] - zpos[0]);
                //Echo(xs);  
                Int32.TryParse(xs, out x);
                Int32.TryParse(ys, out y);
                Int32.TryParse(zs, out z);
                safely_add(coords_ship_full, new Vector3I(x, y, z));
                divider_position = next_position;
                count++;
            }
        }

        private void set_cursor_position(string name)
        {
            //find a block by name and point to it on the screen  
            Echo("CURSING");
            IMyTerminalBlock block = GridTerminalSystem.GetBlockWithName(name);
            cursor_vec = block.Position;
            Echo("Found at:" + cursor_vec.ToString());
        }

        private string concat_horizontal(string s1, string s2)
        {
            int a1 = 0;
            int a2 = 0;
            int b1 = 0;
            int b2 = 0;
            string sub1 = "";
            string sub2 = "";
            string outstring = "";
            while (a1 < s1.Length && b1 < s2.Length)
            {
                a2 = s1.IndexOf("\n", a1 + 1);
                b2 = s2.IndexOf("\n", b1 + 1);
                if (a2 == -1 || b2 == -1)
                {
                    a2 = s1.Length;
                    b2 = s2.Length;
                }
                sub1 = s1.Substring(a1, a2 - a1);
                sub2 = s2.Substring(b1, b2 - b1);
                outstring += sub1 + sub2 + "\n";
                a1 = a2 + 1;
                b1 = b2 + 1;
            }
            return outstring;
        }

        private string percent_bar(int min, int max, int val, int width)
        {
            double percent = Math.Floor(((double)val - (double)min) / ((double)max - (double)min) * 100);
            int fill_value = Convert.ToInt32(((double)val - (double)min) / ((double)max - (double)min) * (double)width);
            string outstring = percent.ToString() + "%\n";
            outstring += "[" + new String('|', fill_value) + new String('`', width - fill_value) + "]";
            return outstring;
        }

        private void safely_add(List<Vector3I> A, Vector3I V)
        {
            if (!A.Contains(V))
            {
                A.Add(V);
                if (!coords_to_render.Contains(V)) coords_to_render.Add(V);
            }
        }

        private void safely_remove(List<Vector3I> A, Vector3I V)
        {
            if (A.Contains(V))
            {
                A.Remove(V);
                if (!coords_to_render.Contains(V)) coords_to_render.Add(V);
            }
        }

        private bool is_weapon(IMyTerminalBlock block)
        {
            if (block is IMySmallMissileLauncher || block is IMySmallMissileLauncherReload ||
                block is IMyLargeMissileTurret || block is IMyLargeGatlingTurret || block is IMyLargeInteriorTurret ||
                block is IMySmallGatlingGun) { return true; }
            else { return false; }
        }

        private bool is_power(IMyTerminalBlock block)
        {
            if (block is IMyBatteryBlock || block is IMyReactor || block is IMySolarPanel) { return true; }
            else { return false; }
        }

        List<Vector3I> intersect(List<Vector3I> L1, List<Vector3I> L2)
        {
            List<Vector3I> temp = new List<Vector3I> { };
            foreach (Vector3I coord in L2)
            {
                if (L1.Contains(coord) && L2.Contains(coord)) temp.Add(coord);
            }
            return temp;
        }

        private void show_damage_on_hud()
        {
            blocks_terminal = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(blocks_terminal);
            foreach (IMyTerminalBlock block in blocks_terminal)
            {
                if (!coords_terminal_working.Contains(block.Position))
                {
                    //if (block.Has("ShowOnHUD")) block.SetValue("ShowOnHUD", true);
                }
                else
                {
                    //if (block.hasProperty("ShowOnHUD")) block.SetValue("ShowOnHUD", false);
                }
            }
        }

        private void update_healths()
        {
            if (coords_gyro_full.Count > 0)
                gyrohealth = Convert.ToInt32(100 * (double)intersect(coords_gyro_now, coords_terminal_working).Count /
                    (double)coords_gyro_full.Count);
            if (coords_thrust_full.Count > 0)
                thrusterhealth = Convert.ToInt32(100 * (double)intersect(coords_thrust_now, coords_terminal_working).Count /
                    (double)coords_thrust_full.Count);
            if (coords_ship_full.Count > 0)
                hullhealth = Convert.ToInt32(100 * (double)coords_ship_now.Count /
                    (double)coords_ship_full.Count);
            if (coords_weapon_full.Count > 0)
                weaponhealth = Convert.ToInt32(100 * (double)intersect(coords_weapon_now, coords_terminal_working).Count /
                    (double)coords_weapon_full.Count);
            if (coords_terminal_full.Count > 0)
                systemhealth = Convert.ToInt32(100 * (double)coords_terminal_working.Count /
                    (double)coords_terminal_full.Count);
            if (coords_power_full.Count > 0)
                powerhealth = Convert.ToInt32(100 * (double)coords_power_now.Count /
                    (double)coords_power_full.Count);
        }

        private void draw_health_report(IMyTextPanel txt)
        {
            string hull_health = percent_bar(0, coords_ship_full.Count, coords_ship_now.Count, 50);
            string system_health = percent_bar(0, 100, systemhealth, 50);
            string gyro_health = percent_bar(0, 100, gyrohealth, 50);
            string thrust_health = percent_bar(0, 100, thrusterhealth, 50);
            string weapon_health = percent_bar(0, 100, weaponhealth, 50);
            string power_health = percent_bar(0, 100, powerhealth, 50);
            txt.WritePublicText("Hull integrity: " + hull_health + "\n", false);
            txt.WritePublicText("System Status: " + system_health + "\n", true);
            txt.WritePublicText("Gyros : " + gyro_health + "\n", true);
            txt.WritePublicText("Thrusters : " + thrust_health + "\n", true);
            txt.WritePublicText("Weapons : " + weapon_health + "\n", true);
            txt.WritePublicText("Power Systems : " + power_health + "\n", true);
        }

        private void trigger_external_timers()
        {
            List<IMyTerminalBlock> blocks_ship_timer = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyTimerBlock>(blocks_ship_timer);
            foreach (IMyTimerBlock tim in blocks_ship_timer)
            {
                string cname = tim.CustomName.ToLower();
                if (cname.IndexOf(alarm_prefix) >= 0)
                {
                    if (check_set_alarm(cname, "power_", powerhealth) ||
                        check_set_alarm(cname, "system_", systemhealth) ||
                        check_set_alarm(cname, "hull_", hullhealth) ||
                        check_set_alarm(cname, "thruster_", thrusterhealth) ||
                        check_set_alarm(cname, "gyro_", gyrohealth) ||
                        check_set_alarm(cname, "weapon_", weaponhealth))
                    {
                        if (!tim.IsCountingDown) { tim.GetActionWithName("Start").Apply(tim); }
                    }
                    else { tim.GetActionWithName("Stop").Apply(tim); }
                }
            }
        }

        private bool check_set_alarm(string cname, string key, int cvalue)
        {
            int len = key.Length;
            int index = cname.IndexOf(key);
            int val = 100;
            if (index > 0)
            {
                string sub = cname.Substring(index + len);
                if (sub.IndexOf("_") > 0) sub = sub.Substring(0, sub.IndexOf("_"));
                Int32.TryParse(sub, out val);
                if (cvalue <= val) { return true; }
                if (cvalue > val) { return false; }
            }
            return false;
        }
    }
}
