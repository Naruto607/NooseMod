//    NooseMod LCPDFR Plugin with Database System
//    Main Script: Register callouts in when player is on duty and starts database system
//    Copyright (C) 2017 Naruto 607

//    This program is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
//    (at your option) any later version.

//    This program is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.

//    You should have received a copy of the GNU General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.

//    Greetings to Sam @ LCPDFR.com for this wonderful API feature.

using GTA.Native;
using NooseMod_LCPDFR.Mission_Controller;
using NooseMod_LCPDFR.StatsDataSetTableAdapters;
using NooseMod_LCPDFR.Global_Controller;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Text;

namespace NooseMod_LCPDFR
{
    #region Uses
    using System;
    using System.Windows.Forms;

    using NooseMod_LCPDFR.Callouts;
    using NooseMod_LCPDFR.World_Events; // WIP

    using GTA;

    using LCPDFR.Networking.User;

    using LCPD_First_Response.Engine;
    using LCPD_First_Response.Engine.Input;
    using LCPD_First_Response.Engine.Networking;
    using LCPD_First_Response.Engine.Scripting.Plugins;
    using LCPD_First_Response.Engine.Timers;
    using LCPD_First_Response.LCPDFR.API;

    using LCPDFR.Networking;

    using SlimDX.XInput;
    #endregion

    /// <summary>
    /// NooseMod Main Script using LCPDFR API
    /// </summary>
    [PluginInfo("NooseMod", false, true)]
    public class Main : Plugin
    {
        #region Initialization
        /// <summary>
        /// The model hash of an Enforcer (NOOSE Stockade - SWAT vehicle - used in LCPDFR)
        /// </summary>
        internal protected const UInt32 Enforcer = 1911513875;

        /// <summary>
        /// Current Game Episode where the LCPDFR is running from
        /// </summary>
        private GameEpisode currentGameEpPlatform = Game.CurrentEpisode;

        /// <summary>
        /// NooseMod Settings File
        /// </summary>
        internal SettingsFile SettingsIni = SettingsFile.Open("LCPDFR\\Plugins\\NooseMod.ini");

        /// <summary>
        /// How many logins you made
        /// </summary>
        private int loginNumbers;

        /// <summary>
        /// Checks on whether the player is selecting difficulty
        /// </summary>
        private bool isSelectingDifficulty;

        /// <summary>
        /// Location of Chief of Police's Office Room
        /// </summary>
        private Vector3 ChiefOfPoliceRoom = new Vector3(122.6f, -669.60f, 14.81f);

        /// <summary>
        /// Location of Partner's Room and Replenishment Area
        /// </summary>
        private Vector3 PartnerRoom = new Vector3(111.36f, -691.52f, 14.77f);

        /// <summary>
        /// NooseMod Mission Controller
        /// </summary>
        private MissionController controller = new MissionController();

        /// <summary>
        /// Table Adapter for <see cref="StatsDataSet"/> representing Overall Stats table
        /// </summary>
        internal OverallStatsTableAdapter overallstatsadp = new OverallStatsTableAdapter();

        /// <summary>
        /// Table Adapter for <see cref="StatsDataSet"/> representing Mission Stats table
        /// </summary>
        internal MissionStatsTableAdapter missionstatsadp = new MissionStatsTableAdapter();

        Blip ChiefOfPoliceRoomBlip, PartnerRoomBlip;
        bool blipCreated = new Boolean();

        /*/// <summary>
        /// The Stats Form, not sure how am I gonna use this...
        /// </summary>
        internal StatsForm statsForm = new StatsForm();
        // */
        #endregion

        /// <summary>
        /// Called when the plugin has been created successfully.
        /// </summary>
        public override void Initialize()
        {
            // Bind console commands
            this.RegisterConsoleCommands();

            // Listen for on duty event
            Functions.OnOnDutyStateChanged += this.Functions_OnOnDutyStateChanged;

            // Listen when player has joined network game
            Networking.JoinedNetworkGame += this.Networking_JoinedNetworkGame;

            // Check if Stats file exists
            // Actually NooseMod folder contains Stats already, if not exists, it'll throw an error automatically
            //if (File.Exists(Game.InstallFolder + "\\LCPDFR\\Plugins\\NooseMod\\Stats.mdb"))

            // Open connection adapters (no conditions set)
            try
            {
                overallstatsadp.Connection.Open();
                missionstatsadp.Connection.Open();
                overallstatsadp.Disposed += this.overallstatsadp_Disposed;
                missionstatsadp.Disposed += this.missionstatsadp_Disposed;
            }
            catch (OleDbException ex)
            {
                Log.Error("Connection cannot be opened: " + ex, this);
            }
            catch (InvalidOperationException ex)
            {
                Log.Error("Invalid Operation: " + ex, this);
            }
            catch (Exception ex) // when other errors caught
            {
                Log.Error("Detected other error: " + ex, this);
            }

            if (overallstatsadp.Connection.State == ConnectionState.Open && missionstatsadp.Connection.State == ConnectionState.Open)
            {
                Log.Info("Started", this);
                Log.Info("Mission Stats Adapter details: " + missionstatsadp.Connection.ConnectionString, this); // this will return a path to NooseMod folder in LCPDFR.log
                Log.Info("Server Version:  " + missionstatsadp.Connection.ServerVersion, this);
                Log.Info("Overall Stats Adapter details: " + overallstatsadp.Connection.ConnectionString, this); // this will return a path to NooseMod folder in LCPDFR.log
                Log.Info("Server Version:  " + overallstatsadp.Connection.ServerVersion, this);
            }
            else
            {
                try
                {
                    overallstatsadp.Connection.Close();
                    missionstatsadp.Connection.Close();
                }
                catch (Exception ex) { Log.Error("Cannot close the connection: " + ex, this); }
                Log.Info("Started without database system", this);
            }

            // Just to make sure
            SettingsIni.Load();
        }

        /// <summary>
        /// Called when player changed the on duty state.
        /// </summary>
        /// <param name="onDuty">The new on duty state.</param>
        public void Functions_OnOnDutyStateChanged(bool onDuty)
        {
            // Register callouts to LCPDFR depending on skin model
            if (onDuty && (LPlayer.LocalPlayer.Skin.Model == new Model("M_Y_SWAT") | LPlayer.LocalPlayer.Skin.Model == new Model("M_Y_NHELIPILOT")))
            {
                // SWAT model used: register NooseMod
                Functions.RegisterCallout(typeof(NooseMod)); // Progressive through gameplay
                Functions.RegisterCallout(typeof(Pursuit)); // Regular Pursuit for bypassing the time check

                if (overallstatsadp.Connection.State == ConnectionState.Open)
                {
                    // Get the data from the Table Adapter
                    StatsDataSet.OverallStatsDataTable overallstatsdt = overallstatsadp.GetData();

                    // Build new Stats row
                    overallstatsdt.AddOverallStatsRow(0, 0, 0, 0, 0, 0);
                    overallstatsdt.AcceptChanges();
                    int fin = overallstatsadp.Update(overallstatsdt);

                    // Record how many rows made
                    loginNumbers = overallstatsdt.Rows.Count;

                    Log.Debug("NooseMod callout initialized, return value is " + fin, this);
                }
                else throw new Exception("Database is in closed state, or is in use");
            }
            else if (onDuty && (LPlayer.LocalPlayer.Skin.Model != new Model("M_Y_SWAT") | LPlayer.LocalPlayer.Skin.Model != new Model("M_Y_NHELIPILOT")))
            {
                // Otherwise register TerroristPursuit callout
                Functions.RegisterCallout(typeof(TerroristPursuit));
                Log.Debug("Played as a model other than SWAT, Terrorist Pursuit callout initialized", this);
            }
            Functions.AddWorldEvent(typeof(ProvostPatrol), "Provost Patrol"); // WIP
        }

        /// <summary>
        /// Called when player has joined a network game.
        /// </summary>
        private void Networking_JoinedNetworkGame()
        {
            // The codes below are taken from the latest pull of LCPDFR API on GitHub (or latest download from LCPDFR website).
            // If you are the developer feel free to have fun with this handler (or add one other handler and mod it your way).

            // Just to be sure.
            if (Networking.IsInSession && Networking.IsConnected)
            {
                // When client, listen for messages. Of course, this could be changed to work for host as well.
                // It's recommended to use the same string identifier all the time.
                if (!Networking.IsHost)
                {
                    Client client = Networking.GetClientInstance();
                    client.AddUserDataHandler(this.ToString(), NetworkMessages.RequestBackup, this.NeedBackupHandlerFunction);
                }
            }
        }

        /// <summary>
        /// Called when <see cref="NetworkMessages.RequestBackup"/> has been received.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="message">The message.</param>
        private void NeedBackupHandlerFunction(NetworkServer sender, ReceivedUserMessage message)
        {
            // The codes below are taken from the latest pull of LCPDFR API on GitHub (or latest download from LCPDFR website).
            // If you are the developer feel free to have fun with this handler (or add one other handler and mod it your way).

            // Read position and get associated area.
            Vector3 position = message.ReadVector3();
            string area = Functions.GetAreaStringFromPosition(position);

            // Display message and blip.
            Functions.PrintText(string.Format("Officer {0} requests backup at {1}", sender.SafeName, area), 10000);
            Blip areaBlip = Functions.CreateBlipForArea(position, 25f);

            // Play sound.
            int rand = Common.GetRandomValue(0, 4);
            switch (rand)
            {
                case 0:
                    Functions.PlaySoundUsingPosition("THIS_IS_CONTROL UNITS_PLEASE_BE_ADVISED INS_I_NEED_A_UNIT_FOR CRIM_AN_OFFICER_IN_NEED_OF_ASSISTANCE IN_OR_ON_POSITION", position);
                    break;
                case 1:
                    Functions.PlaySoundUsingPosition("THIS_IS_CONTROL ASSISTANCE_REQUIRED FOR CRIM_AN_OFFICER_IN_NEED_OF_ASSISTANCE IN_OR_ON_POSITION", position);
                    break;
                case 2:
                    Functions.PlaySoundUsingPosition("INS_THIS_IS_CONTROL_WE_HAVE CRIM_AN_OFFICER_IN_NEED_OF_ASSISTANCE IN_OR_ON_POSITION", position);
                    break;
                case 3:
                    Functions.PlaySoundUsingPosition("ALL_UNITS_ALL_UNITS INS_WE_HAVE CRIM_AN_SOS FOR CRIM_AN_OFFICER_IN_NEED_OF_ASSISTANCE IN_OR_ON_POSITION", position);
                    break;
            }
            Log.Info("Client: Server issued a backup request on coordinates " + position.ToString() + " (Area: " + area + ")", this);

            // Cleanup.
            DelayedCaller.Call(parameter => areaBlip.Delete(), this, 20000);
        }

        // The function for this method is still beta
        #region Blip Controller
        private void PoliceDept_AddBlip()
        {
            if (LPlayer.LocalPlayer.IsInPoliceDepartment && blipCreated == false)
            {
                ChiefOfPoliceRoomBlip = Blip.AddBlipContact(ChiefOfPoliceRoom);
                ChiefOfPoliceRoomBlip.Display = BlipDisplay.ArrowOnly;
                ChiefOfPoliceRoomBlip.Icon = BlipIcon.Misc_Boss;
                ChiefOfPoliceRoomBlip.Name = "Difficulty Selection";
                ChiefOfPoliceRoomBlip.Color = BlipColor.Red;
                PartnerRoomBlip = Blip.AddBlipContact(PartnerRoom);
                PartnerRoomBlip.Display = BlipDisplay.ArrowOnly;
                PartnerRoomBlip.Icon = BlipIcon.Building_Safehouse;
                PartnerRoomBlip.Name = "Reset NooseMod Mission Progress";
                PartnerRoomBlip.Color = BlipColor.Cyan;
                blipCreated = true;
            }
            else if (!LPlayer.LocalPlayer.IsInPoliceDepartment)
            {
                if (ChiefOfPoliceRoomBlip != null && ChiefOfPoliceRoomBlip.Exists()) ChiefOfPoliceRoomBlip.Delete(); else return;
                if (PartnerRoomBlip != null && PartnerRoomBlip.Exists()) PartnerRoomBlip.Delete(); else return;
                blipCreated = false;
            }
        }
        #endregion

        /// <summary>
        /// Called every tick to process all plugin logic.
        /// </summary>
        public override void Process()
        {
            ProcessEnforcerAbility();
            SummarizeOverallStats();
            ChiefOfPolice_DifficultySelection();
            InitializeReset();
            //ShowStatsFormWhileUsingPoliceComputer();
            //PoliceDept_AddBlip();
            NetworkGame_RequestBackupHandler();
        }

        /*
        /// <summary>
        /// Displays Stats Form while using the police computer.
        /// </summary>
        private void ShowStatsFormWhileUsingPoliceComputer()
        {
            if (LPlayer.LocalPlayer.Ped.IsInVehicle())
            {
                if (LPlayer.LocalPlayer.Ped.CurrentVehicle.Model.ModelInfo.ModelFlags == LCPD_First_Response.Engine.Scripting.Entities.EModelFlags.IsCopCar)
                {
                    if (Functions.IsKeyDown(SettingsFile.Open("LCPDFR\\LCPDFR.ini").GetValueKey("PoliceComputer", "Keybindings", Keys.E)) |
                        Functions.IsControllerKeyDown((GamepadButtonFlags)Enum.Parse(typeof(GamepadButtonFlags), SettingsFile.Open("LCPDFR\\LCPDFR.ini").GetValueString("PoliceComputer", "KeybindingsController", "LeftShoulder"), true)))
                        try
                        {
                            Functions.SetExternalTextInputActive(true);
                            statsForm.DrawToBitmap(new System.Drawing.Bitmap(Game.Resolution.Width, Game.Resolution.Height), new System.Drawing.Rectangle(150, 200, Game.Resolution.Width, Game.Resolution.Height));
                            statsForm.Show();
                        }
                        catch (Exception ex) { Log.Error("Form cannot be initialized: " + ex, this); }
                }
            }
        }
         // */

        /// <summary>
        /// Called when the plugin is being disposed, e.g. because an unhandled exception occured in Process. Free all resources here!
        /// </summary>
        public override void Finally()
        {
            // TODO: Cleanup! (if any)
            missionstatsadp.Dispose();
            overallstatsadp.Dispose();
        }

        /// <summary>
        /// If implemented, this is used to start <see cref="NooseMod"/> callout through the ScriptHookDotNet (SHDN) Console.
        /// </summary>
        /// <param name="parameterCollection">Parameters to start certain callout</param>
        [ConsoleCommand("StartCallout", false)]
        private void StartCallout(ParameterCollection parameterCollection)
        {
            if (parameterCollection.Count > 0)
            {
                string name = parameterCollection[0];
                Functions.StartCallout(name);
            }
            else
            {
                Game.Console.Print("StartCallout: No argument given.");
            }
        }

        /// <summary>
        /// Prints the NooseMod Overall Stats into the console window
        /// </summary>
        /// <param name="parameterCollection">Parameters</param>
        [ConsoleCommand("ShowNooseModStats", false)]
        private void ShowNooseModStats(ParameterCollection parameterCollection)
        {
            // Get the data from the Table Adapter
            StatsDataSet.OverallStatsDataTable overallstatsdt = overallstatsadp.GetData();
            if (overallstatsdt.Rows.Count != 0 && overallstatsdt.IsInitialized && LPlayer.LocalPlayer.IsOnDuty)
            {
                try
                {
                    int tempVar1 = overallstatsdt.FindBySession(loginNumbers).Suspects_Killed,
                        tempVar2 = overallstatsdt.FindBySession(loginNumbers).Suspects_Arrested,
                        tempVar3 = overallstatsdt.FindBySession(loginNumbers).Hostages_Killed,
                        tempVar4 = overallstatsdt.FindBySession(loginNumbers).Hostages_Rescued,
                        tempVar5 = overallstatsdt.FindBySession(loginNumbers).Officer_Casualties,
                        tempVar6 = overallstatsdt.FindBySession(loginNumbers).Squad_Casualties;

                    // Print the statistics to console
                    Game.Console.Print("NooseMod Statistics");
                    Game.Console.Print("---");
                    Game.Console.Print("Your login session: " + loginNumbers);
                    Game.Console.Print("Total Suspects killed: " + tempVar1);
                    Game.Console.Print("Total Suspects arrested: " + tempVar2);
                    Game.Console.Print("Total Hostages killed: " + tempVar3);
                    Game.Console.Print("Total Hostages rescued: " + tempVar4);
                    Game.Console.Print("Total Officers killed in mission: " + tempVar5);
                    Game.Console.Print("Total Squad casualties: " + tempVar6);
                }
                catch (Exception ex) { Game.Console.Print("NooseMod Statistics: Error loading stats: " + ex); }
            }
            else { Game.Console.Print("NooseMod Statistics: no session is loaded"); }
        }

        /// <summary>
        /// Returns this plugin name when called.
        /// </summary>
        /// <returns>Plugin name.</returns>
        public override string ToString()
        {
            return "NooseMod";
        }

        #region LCPDFR External Procedures
        /// <summary>
        /// Processes difficulty change when inside the Chief of Police's Office Room.
        /// </summary>
        private void ChiefOfPolice_DifficultySelection()
        {
            // If player enters any police station, enters the Chief of Police's office room, and enters a marker, set difficulty
            // Wait 10 seconds in script to prevent stuck at the restart point

            //Blip selectDifficultyBlip = Blip.AddBlip(ChiefOfPoliceRoom);
            bool isHardcoreModeActive = controller.HardcoreModeIsActive();
            int waitTime = 10000; // 10 seconds refresh
            if (isHardcoreModeActive == false)
                if (LPlayer.LocalPlayer.IsInPoliceDepartment && LPlayer.LocalPlayer.Ped.Position.DistanceTo(ChiefOfPoliceRoom) < 1f &&
                (LPlayer.LocalPlayer.Skin.Model == new Model("M_Y_SWAT") | LPlayer.LocalPlayer.Skin.Model == new Model("M_Y_NHELIPILOT")))
            {
                this.isSelectingDifficulty = true;
                int[] tempDiff = { 1, 2, 3 };
                int keyPress = 0;
                //AnimationSet difficultyPlayAnim = new AnimationSet("cellphone");
                Functions.PrintHelp("Use left and right arrows to select a difficulty. When done, press ~KEY_ACCEPT_CALLOUT~ to confirm. Otherwise press ~KEY_ARREST_CALL_TRANSPORTER~ to quit.");
                TaskSequence mytask = new TaskSequence();
                mytask.AddTask.StandStill(-1);
                mytask.AddTask.UseMobilePhone(-1);
                mytask.Perform(LPlayer.LocalPlayer.Ped);
                //LPlayer.LocalPlayer.CanControlCharacter = false;
                while (this.isSelectingDifficulty)
                {
                    if (Functions.IsKeyDown(Keys.Left))
                    {
                        keyPress--;
                        if (keyPress < 0) { keyPress = 2; }
                    }
                    else if (Functions.IsKeyDown(Keys.Right))
                    {
                        keyPress++;
                        if (keyPress > 2) { keyPress = 0; }
                    }
                    if (keyPress == 0)
                    { Game.DisplayText("Difficulty: Easy", 1000); }
                    else if (keyPress == 1)
                    { Game.DisplayText("Difficulty: Medium", 1000); }
                    else if (keyPress == 2)
                    { Game.DisplayText("Difficulty: Hard", 1000); }

                    if (Functions.IsKeyDown(SettingsFile.Open("LCPDFR\\LCPDFR.ini").GetValueKey("AcceptCallout", "Keybindings", Keys.Y)))
                    {
                        SettingsIni.SetValue("Difficulty", "GlobalSettings", tempDiff[keyPress]);
                        SettingsIni.Save();
                        Functions.PrintText("Difficulty has been set", 5000);
                        LPlayer.LocalPlayer.CanControlCharacter = true;
                        LPlayer.LocalPlayer.Ped.Task.ClearAll();
                        this.isSelectingDifficulty = false;
                        Function.Call("TRIGGER_MISSION_COMPLETE_AUDIO", new Parameter[] { 1 });
                        Game.WaitInCurrentScript(waitTime);
                    }
                    else if (Functions.IsKeyDown(SettingsFile.Open("LCPDFR\\LCPDFR.ini").GetValueKey("ArrestCallTransporter", "Keybindings", Keys.N)))
                    {
                        //LPlayer.LocalPlayer.CanControlCharacter = true;
                        LPlayer.LocalPlayer.Ped.Task.ClearAll();
                        this.isSelectingDifficulty = false;
                        Game.WaitInCurrentScript(waitTime);
                    }
                    Game.WaitInCurrentScript(0); // yield
                }
            }
        }

        /// <summary>
        /// Allows the host of the plugin requests backup when or when not in a mission.
        /// </summary>
        private void NetworkGame_RequestBackupHandler()
        {
            // The codes below are taken from the latest pull of LCPDFR API on GitHub (or latest download from LCPDFR website).
            // If you are the developer feel free to have fun with this handler (or add one other handler and mod it your way).

            bool isNetworkingBackupSystemActive = SettingsIni.GetValueBool("EnableNetworkingBackupSys", "GlobalSettings", false);
            // Send RequestBackup message in network game.
            if (Functions.IsKeyDown(SettingsIni.GetValueKey("RequestBackupInNetworkKeybind", "GlobalSettings", Keys.Back))
                & isNetworkingBackupSystemActive == true)
            {
                if (Networking.IsInSession && Networking.IsConnected)
                {
                    if (Networking.IsHost)
                    {
                        Vector3 position = LPlayer.LocalPlayer.Ped.Position;

                        // Tell client we need backup.
                        DynamicData dynamicData = new DynamicData(Networking.GetServerInstance());
                        dynamicData.Write(position);
                        Networking.GetServerInstance().Send(this.ToString(), NetworkMessages.RequestBackup, dynamicData);
                        Log.Info("Server: Message sent to Client", this);
                    }
                }
            }
        }

        /// <summary>
        /// Gets the Enforcer Armory State for allowing player to rearm his weapons
        /// </summary>
        /// <returns>True if within 3 "single" values (<see cref="System.Single"/>) and the vehicle is a "pure" Enforcer, otherwise false</returns>
        private bool GetEnforcerArmoryState()
        {
            LVehicle closestVehicle = LVehicle.FromGTAVehicle(World.GetClosestVehicle(LPlayer.LocalPlayer.Ped.Position, 3f));
            return ValidityCheck.isObjectValid(closestVehicle) && closestVehicle.Model.ModelInfo.Hash == Enforcer;
        }

        /// <summary>
        /// Processes the ability to rearm player when driving an Enforcer.
        /// </summary>
        private void ProcessEnforcerAbility()
        {
            if (LPlayer.LocalPlayer.IsOnDuty && GetEnforcerArmoryState() &&
                (LPlayer.LocalPlayer.Skin.Model == new Model("M_Y_SWAT") | LPlayer.LocalPlayer.Skin.Model == new Model("M_Y_NHELIPILOT")))
            {
                // Fixed the state where you cannot get back into the Enforcer - used the customized method of the original NooseMod
                if (!LPlayer.LocalPlayer.Ped.IsInVehicle() && GetEnforcerArmoryState())
                {
                    Functions.PrintHelp("Press ~KEY_OPEN_TRUNK~ to rearm your weapon or ~INPUT_ENTER~ to get in the Enforcer.");
                    if (Functions.IsKeyDown(SettingsFile.Open("LCPDFR\\LCPDFR.ini").GetValueKey("OpenTrunk", "Keybindings", Keys.E)))
                    {
                        TaskSequence mytask = new TaskSequence();
                        mytask.AddTask.StandStill(-1);
                        mytask.AddTask.PlayAnimation(new AnimationSet("playidles_std"), "gun_cock_weapon", 8f, AnimationFlags.Unknown05);
                        mytask.Perform(LPlayer.LocalPlayer.Ped);
                        Functions.PrintText("Rearming...", 5000);
                        Game.WaitInCurrentScript(3000);

                        Rearm();

                        Game.WaitInCurrentScript(1500);
                        LPlayer.LocalPlayer.Ped.Task.ClearAllImmediately();
                        Game.WaitInCurrentScript(500);
                        Functions.PrintText("Complete", 5000);
                        Function.Call("TRIGGER_MISSION_COMPLETE_AUDIO", new Parameter[] { 27 });
                        Game.WaitInCurrentScript(10000);
                    }
                }
            }
            // If return is active, this method will loop around, making others not be processed
            //else return;
        }

        private void Rearm()
        {
            bool usePrefixPistolModel = SettingsIni.GetValueBool("UsePrefixPistolModel", "WorldSettings", false);

            Weapon PrimaryWeapon1, PrimaryWeapon2, PrefixPistol = new Weapon();
            int PrimaryAmmo1, PrimaryAmmo2, PistolAmmo;

            try
            {
                switch ((Weapon)Enum.Parse(typeof(Weapon), SettingsIni.GetValueString("PrimaryWeapon1", "WorldSettings"), true))
                {
                    // Default (when no value is present), then cases
                    // Handles SMG, Rifles, Sniper Rifles, and Heavy Weapons
                    // Runs on Episodic Check
                    default: PrimaryWeapon1 = Weapon.SMG_MP5; break;
                    case Weapon.Rifle_AK47: PrimaryWeapon1 = Weapon.Rifle_AK47; break;
                    case Weapon.Rifle_M4: PrimaryWeapon1 = Weapon.Rifle_M4; break;
                    case Weapon.TBOGT_AdvancedMG:
                        if (currentGameEpPlatform == GameEpisode.TBOGT)
                            PrimaryWeapon1 = Weapon.TBOGT_AdvancedMG; // M249
                        else PrimaryWeapon1 = Weapon.Rifle_M4;
                        break;
                    case Weapon.TBOGT_AdvancedSniper:
                        if (currentGameEpPlatform == GameEpisode.TBOGT)
                            PrimaryWeapon1 = Weapon.TBOGT_AdvancedSniper; // DSR-1
                        else PrimaryWeapon1 = Weapon.SniperRifle_M40A1;
                        break;
                    case Weapon.SMG_MP5: PrimaryWeapon1 = Weapon.SMG_MP5; break;
                    case Weapon.SniperRifle_Basic: PrimaryWeapon1 = Weapon.SniperRifle_Basic; break;
                    case Weapon.SniperRifle_M40A1: PrimaryWeapon1 = Weapon.SniperRifle_M40A1; break;
                    case Weapon.Heavy_RocketLauncher: PrimaryWeapon1 = Weapon.Heavy_RocketLauncher; break;
                    case Weapon.TBOGT_AssaultSMG:
                        if (currentGameEpPlatform == GameEpisode.TBOGT)
                            PrimaryWeapon1 = Weapon.TBOGT_AssaultSMG; // P90
                        else PrimaryWeapon1 = Weapon.SMG_MP5;
                        break;
                    case (Weapon)21: // HK69A1 40mm (this is, however, assigned in Rifle class if using My Realistic WeaponInfo v5)
                        if (currentGameEpPlatform == GameEpisode.TLAD)
                            PrimaryWeapon1 = Weapon.TLAD_GrenadeLauncher;
                        else if (currentGameEpPlatform == GameEpisode.TBOGT)
                            PrimaryWeapon1 = Weapon.TBOGT_GrenadeLauncher;
                        else PrimaryWeapon1 = Weapon.Heavy_RocketLauncher; // Defaulted to the Rocket Launcher if you are using vanilla WeaponInfo
                        break;
                }
                switch ((Weapon)Enum.Parse(typeof(Weapon), SettingsIni.GetValueString("PrimaryWeapon2", "WorldSettings"), true))
                {
                    default: PrimaryWeapon2 = Weapon.Shotgun_Baretta; break;
                    case Weapon.Shotgun_Baretta: PrimaryWeapon2 = Weapon.Shotgun_Baretta; break;
                    case Weapon.Shotgun_Basic: PrimaryWeapon2 = Weapon.Shotgun_Basic; break;
                    case Weapon.TBOGT_ExplosiveShotgun:
                        if (currentGameEpPlatform == GameEpisode.TBOGT)
                            PrimaryWeapon2 = Weapon.TBOGT_ExplosiveShotgun; // AA-12 12ga Frag-12
                        else PrimaryWeapon2 = Weapon.Shotgun_Baretta;
                        break;
                    case Weapon.TBOGT_NormalShotgun:
                        if (currentGameEpPlatform == GameEpisode.TBOGT)
                            PrimaryWeapon2 = Weapon.TBOGT_NormalShotgun; // AA-12 Shotgun
                        else PrimaryWeapon2 = Weapon.Shotgun_Baretta;
                        break;
                    case Weapon.TLAD_AssaultShotgun:
                        if (currentGameEpPlatform == GameEpisode.TLAD)
                            PrimaryWeapon2 = Weapon.TLAD_AssaultShotgun; // Striker
                        else PrimaryWeapon2 = Weapon.Shotgun_Baretta;
                        break;
                    case Weapon.TLAD_SawedOffShotgun:
                        if (currentGameEpPlatform == GameEpisode.TLAD)
                            PrimaryWeapon2 = Weapon.TLAD_SawedOffShotgun; // Lupara?
                        else PrimaryWeapon2 = Weapon.Shotgun_Basic;
                        break;
                }
            }
            catch (Exception ex)
            {
                Log.Error("Error on getting weapon data: " + ex, this);
                return;
            }

            try
            {
                // Get ammo counts (if empty will be defaulted to original clip size based on My Realistic Weapon Mod v5)
                // First Primary Weapon will be categorized for its weapon type (Heavy Weapons will have ammo set to 3, regardless of settings made)
                if (PrimaryWeapon1 == Weapon.Heavy_RocketLauncher || PrimaryWeapon1 == (Weapon)21)
                    PrimaryAmmo1 = 3;
                else PrimaryAmmo1 = SettingsIni.GetValueInteger("PrimaryAmmo1", "WorldSettings", 30);
                PrimaryAmmo2 = SettingsIni.GetValueInteger("PrimaryAmmo2", "WorldSettings", 8);
                PistolAmmo = SettingsIni.GetValueInteger("PistolAmmo", "WorldSettings", 15);
            }
            catch (Exception ex)
            {
                Log.Error("Error on getting weapon ammunition data: " + ex, this);
                return;
            }

            // Checks if the prefix pistol model value returned true
            if (usePrefixPistolModel) try
                {
                    switch ((Weapon)Enum.Parse(typeof(Weapon), SettingsIni.GetValueString("PrefixPistolModel", "WorldSettings"), true))
                    {
                        default: PrefixPistol = Weapon.Handgun_Glock; break;
                        case Weapon.Handgun_Glock: PrefixPistol = Weapon.Handgun_Glock; break;
                        case Weapon.Handgun_DesertEagle: PrefixPistol = Weapon.Handgun_DesertEagle; break;
                        case Weapon.TLAD_Automatic9mm:
                            if (currentGameEpPlatform == GameEpisode.TLAD)
                                PrefixPistol = Weapon.TLAD_Automatic9mm; // CZ75-Auto
                            else PrefixPistol = Weapon.Handgun_Glock;
                            break;
                        case Weapon.TBOGT_Pistol44:
                            if (currentGameEpPlatform == GameEpisode.TBOGT)
                                PrefixPistol = Weapon.TBOGT_Pistol44; // SA Auto .44
                            else PrefixPistol = Weapon.Handgun_DesertEagle;
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Log.Error("Error on getting weapon data: " + ex, this);
                    return;
                }

            try
            {
                // First primary depend on the weapon slot
                LPlayer.LocalPlayer.Ped.Weapons.FromType(PrimaryWeapon1).Ammo += PrimaryAmmo1;

                // Second primary have their own slot: Shotguns
                LPlayer.LocalPlayer.Ped.Weapons.FromType(PrimaryWeapon2).Ammo += PrimaryAmmo2;

                // Secondary is pistol
                if (usePrefixPistolModel)
                    LPlayer.LocalPlayer.Ped.Weapons.FromType(PrefixPistol).Ammo += PistolAmmo;
                else if (LPlayer.LocalPlayer.Ped.Weapons.inSlot(WeaponSlot.Handgun) == null)
                    LPlayer.LocalPlayer.Ped.Weapons.Glock.Ammo = PistolAmmo;
                else
                    LPlayer.LocalPlayer.Ped.Weapons.inSlot(WeaponSlot.Handgun).Ammo += PistolAmmo;

                // Free Grenades! (If you have other throwables, it'll be added)
                if (LPlayer.LocalPlayer.Ped.Weapons.inSlot(WeaponSlot.Thrown) == null)
                    LPlayer.LocalPlayer.Ped.Weapons.Grenades.Ammo = 3;
                else
                    LPlayer.LocalPlayer.Ped.Weapons.inSlot(WeaponSlot.Thrown).Ammo += 3;
            }
            catch (Exception ex)
            {
                Log.Error("Cannot apply weapons to player: " + ex, this);
                Functions.PrintText("Cannot apply weapons to player, check LCPDFR.log for errors", 3000);
            }
        }

        /// <summary>
        /// Summarizes all Mission Stats into an Overall Stats
        /// </summary>
        private void SummarizeOverallStats()
        {
            int tempVar1 = 0, tempVar2 = 0, tempVar3 = 0, tempVar4 = 0, tempVar5 = 0, tempVar6 = 0;
            //decimal tempVar7 = 0;
            StatsDataSet.OverallStatsDataTable overallstatsdt;
            StatsDataSet.MissionStatsDataTable missionstatsdt;

            // Get the data from the Table Adapter
            if (overallstatsadp.Connection.State == ConnectionState.Open && missionstatsadp.Connection.State == ConnectionState.Open)
            {
                overallstatsdt = overallstatsadp.GetData();
                missionstatsdt = missionstatsadp.GetData();
            }
            else return;

            // Get Values
            if (overallstatsdt.Rows.Count != 0 && overallstatsdt.IsInitialized && LPlayer.LocalPlayer.IsOnDuty && missionstatsdt.IsInitialized
                && (LPlayer.LocalPlayer.Skin.Model == new Model("M_Y_SWAT") | LPlayer.LocalPlayer.Skin.Model == new Model("M_Y_NHELIPILOT")))
                try
                {
                    for (int i = 1; i <= missionstatsdt.Rows.Count; i++)
                    {
                        tempVar1 = tempVar1 + missionstatsdt.FindByMission_Number(i).Suspects_Killed;
                        tempVar2 = tempVar2 + missionstatsdt.FindByMission_Number(i).Suspects_Arrested;
                        tempVar3 = tempVar3 + missionstatsdt.FindByMission_Number(i).Hostages_Killed;
                        tempVar4 = tempVar4 + missionstatsdt.FindByMission_Number(i).Hostages_Rescued;
                        tempVar5 = tempVar5 + missionstatsdt.FindByMission_Number(i).Officer_Casualties;
                        tempVar6 = tempVar6 + missionstatsdt.FindByMission_Number(i).Squad_Casualties;
                        //tempVar7 = tempVar7 + missionstatsdt.FindByMission_Number(i).Income;
                    }
                    // Write the Summary
                    overallstatsdt.FindBySession(loginNumbers).Suspects_Killed = tempVar1;
                    overallstatsdt.FindBySession(loginNumbers).Suspects_Arrested = tempVar2;
                    overallstatsdt.FindBySession(loginNumbers).Hostages_Killed = tempVar3;
                    overallstatsdt.FindBySession(loginNumbers).Hostages_Rescued = tempVar4;
                    overallstatsdt.FindBySession(loginNumbers).Officer_Casualties = tempVar5;
                    overallstatsdt.FindBySession(loginNumbers).Squad_Casualties = tempVar6;
                    overallstatsdt.AcceptChanges();
                    overallstatsadp.Update(overallstatsdt);
                }
                catch (Exception ex) { Log.Error("Error writing statistics to database: " + ex, this); overallstatsdt.RejectChanges(); }
        }

        /// <summary>
        /// This method resets the save stats so that you can play the NooseMod callout again.
        /// </summary>
        private void InitializeReset()
        {
            int value = -1;
            int timeToResetAgain = 10000;
            // If player is near a terminal, show message

            if (LPlayer.LocalPlayer.IsInPoliceDepartment && LPlayer.LocalPlayer.Ped.Position.DistanceTo(PartnerRoom) < 1f
                && (LPlayer.LocalPlayer.Skin.Model == new Model("M_Y_SWAT") | LPlayer.LocalPlayer.Skin.Model == new Model("M_Y_NHELIPILOT")))
            {
                Functions.PrintHelp("To reset the save progress, press ~KEY_ACCEPT_CALLOUT~ to confirm while standing.");
                Functions.PrintText("Reset NooseMod save progress?", 5000);
                if (Functions.IsKeyDown(SettingsFile.Open("LCPDFR\\LCPDFR.ini").GetValueKey("AcceptCallout", "Keybindings", Keys.Y))) try
                    {
                        // Reset status
                        StreamWriter streamWriter = new StreamWriter("LCPDFR\\Plugins\\NooseMod\\save.txt");
                        streamWriter.WriteLine(value.ToString());
                        streamWriter.Close();

                        // Notify user
                        Functions.PrintText("NooseMod save progress has been reset.", 5000);
                        Log.Info("Progress has been reset", this);
                        Function.Call("TRIGGER_MISSION_COMPLETE_AUDIO", new Parameter[] { 1 });
                        Game.WaitInCurrentScript(timeToResetAgain);
                    }
                    catch (Exception ex) 
                    {
                        Log.Error("Unable to read or write save.txt: " + ex, this);
                        Functions.PrintText("Unable to read or write save.txt, check LCPDFR.log for errors", 5000);
                        Game.WaitInCurrentScript(timeToResetAgain);
                    }
                else Game.WaitInCurrentScript(300); // hold each 0.3 second
            }
        }

        /// <summary>
        /// Called when the Table Adapter needs to be disposed.
        /// </summary>
        /// <param name="sender">Object that is sending</param>
        /// <param name="e">Event Arguments</param>
        internal void overallstatsadp_Disposed(object sender, EventArgs e)
        {
            // Close connection
            overallstatsadp.Connection.Close();
        }

        /// <summary>
        /// Called when the Table Adapter needs to be disposed.
        /// </summary>
        /// <param name="sender">Object that is sending</param>
        /// <param name="e">Event Arguments</param>
        internal void missionstatsadp_Disposed(object sender, EventArgs e)
        {
            // Close connection
            missionstatsadp.Connection.Close();
        }
        #endregion
    }
}