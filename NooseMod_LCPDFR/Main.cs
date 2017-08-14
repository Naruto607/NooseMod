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

namespace NooseMod_LCPDFR
{
    #region Uses
    using GTA;
    using GTA.Native;
    using LCPD_First_Response.Engine;
    using LCPD_First_Response.Engine.Input;
    using LCPD_First_Response.Engine.Scripting.Plugins;
    using LCPD_First_Response.Engine.Timers;
    using LCPD_First_Response.LCPDFR.API;
    using NooseMod_LCPDFR.Callouts;
    using NooseMod_LCPDFR.Mission_Controller;
    using NooseMod_LCPDFR.StatsDataSetTableAdapters;
    using NooseMod_LCPDFR.World_Events; // WIP
    using SlimDX.XInput;
    using System;
    using System.Data;
    using System.Data.OleDb;
    using System.IO;
    using System.Text;
    using System.Windows.Forms;
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
        /// An LCPDFR Player
        /// </summary>
        private LPlayer lcpdfrPlayer = new LPlayer();

        /// <summary>
        /// An LCPDFR Vehicle
        /// </summary>
        private LVehicle currentVeh;

        /// <summary>
        /// Settings File
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
            controller.GetMissionData();

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
            catch (Exception ex) // when other errors catched
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
                overallstatsadp.Connection.Close();
                missionstatsadp.Connection.Close();
                Log.Info("Started without database system", this); 
            }
        }

        /// <summary>
        /// Called when player changed the on duty state.
        /// </summary>
        /// <param name="onDuty">The new on duty state.</param>
        public void Functions_OnOnDutyStateChanged(bool onDuty)
        {
            // Register callouts to LCPDFR depending on skin model
            if (onDuty && lcpdfrPlayer.Skin.Model == new Model("M_Y_SWAT") || lcpdfrPlayer.Skin.Model == new Model("M_Y_NHELIPILOT"))
            {
                // SWAT model used: register NooseMod
                Functions.RegisterCallout(typeof(NooseMod)); // Progressive through gameplay

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
            else if (onDuty && lcpdfrPlayer.Skin.Model != new Model("M_Y_SWAT") || lcpdfrPlayer.Skin.Model != new Model("M_Y_NHELIPILOT"))
            {
                // Otherwise register TerroristPursuit callout
                Functions.RegisterCallout(typeof(TerroristPursuit));
                Log.Debug("Played as a model other than SWAT, Terrorist Pursuit callout initialized", this);
            }
            Functions.AddWorldEvent(typeof(ProvostPatrol), "Provost Patrol"); // WIP
        }

        /// <summary>
        /// Called every tick to process all plugin logic.
        /// </summary>
        public override void Process()
        {
            ProcessEnforcerAbility();
            SummarizeOverallStats();
            ChiefOfPolice_DifficultySelection();
            InitializeReset();
        }

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
            if (overallstatsdt.Rows.Count != 0 && overallstatsdt.IsInitialized && lcpdfrPlayer.IsOnDuty)
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
        /// Gets a string representation of this event. Call Base to get the string.
        /// </summary>
        /// <returns>A string that represents current object</returns>
        public override string ToString()
        {
            return base.ToString();
        }

        #region LCPDFR External Procedures
        /// <summary>
        /// Processes difficulty change when inside the Chief of Police's Office Room
        /// </summary>
        private void ChiefOfPolice_DifficultySelection()
        {
            // If player enters any police station, enters the Chief of Police's office room, and enters a marker, set difficulty
            // Wait 10 seconds in script to prevent stuck at the restart point

            //Blip selectDifficultyBlip = Blip.AddBlip(ChiefOfPoliceRoom);
            bool isHardcoreModeActive = controller.HardcoreModeIsActive();
            int waitTime = 10000;
            if (lcpdfrPlayer.IsInPoliceDepartment && lcpdfrPlayer.Ped.Position.DistanceTo(ChiefOfPoliceRoom) < 1f && !isHardcoreModeActive
                && lcpdfrPlayer.Skin.Model == new Model("M_Y_SWAT") || lcpdfrPlayer.Skin.Model == new Model("M_Y_NHELIPILOT"))
            {
                // Play an animation
                this.isSelectingDifficulty = true;
                int[] tempDiff = { 1, 2, 3 };
                int keyPress = 0;
                //AnimationSet difficultyPlayAnim = new AnimationSet("cellphone");
                Functions.PrintHelp("Use left and right arrows to select a difficulty. When done, press ~KEY_ACCEPT_CALLOUT~ to confirm. Otherwise press ~KEY_ARREST_CALL_TRANSPORTER~ to quit.");
                while (this.isSelectingDifficulty)
                {
                    //lcpdfrPlayer.Ped.Animation.Play(difficultyPlayAnim, "", 1.0f);
                    //lcpdfrPlayer.Ped.Animation.WaitUntilFinished(difficultyPlayAnim, "");
                    lcpdfrPlayer.Ped.Task.UseMobilePhone(-1);
                    lcpdfrPlayer.Ped.Task.StandStill(-1);
                    if (Functions.IsKeyDown(Keys.Left))
                    {
                        keyPress--;
                        if (keyPress < 0) { keyPress = 3; }
                    }
                    else if (Functions.IsKeyDown(Keys.Right))
                    {
                        keyPress++;
                        if (keyPress > 2) { keyPress = 0; }
                    }
                    if (keyPress == 0)
                    { Game.DisplayText("Difficulty: Easy"); }
                    else if (keyPress == 1)
                    { Game.DisplayText("Difficulty: Medium"); }
                    else if (keyPress == 2)
                    { Game.DisplayText("Difficulty: Hard"); }

                    if (Functions.IsKeyDown(SettingsFile.Open("LCPDFR\\LCPDFR.ini").GetValueKey("AcceptCallout", "Keybindings", Keys.Y)))
                    {
                        SettingsIni.SetValue("Difficulty", "GlobalSettings", tempDiff[keyPress]);
                        SettingsIni.Save();
                        Functions.PrintText("Difficulty has been set",5000);
                        //lcpdfrPlayer.Ped.Task.ClearAllImmediately();
                        lcpdfrPlayer.Ped.Task.PutAwayMobilePhone();
                        lcpdfrPlayer.Ped.Task.ClearAll();
                        this.isSelectingDifficulty = false;
                        Game.WaitInCurrentScript(waitTime);
                    }
                    else if (Functions.IsKeyDown(SettingsFile.Open("LCPDFR\\LCPDFR.ini").GetValueKey("ArrestCallTransporter", "Keybindings", Keys.N)))
                    {
                        //lcpdfrPlayer.Ped.Task.ClearAllImmediately();
                        lcpdfrPlayer.Ped.Task.PutAwayMobilePhone();
                        lcpdfrPlayer.Ped.Task.ClearAll();
                        this.isSelectingDifficulty = false;
                        Game.WaitInCurrentScript(waitTime);
                    }
                }
            }
        }

        /// <summary>
        /// Processes the ability to rearm player when driving an Enforcer
        /// </summary>
        private void ProcessEnforcerAbility()
        {
            if (lcpdfrPlayer.Ped.IsInVehicle() && lcpdfrPlayer.IsOnDuty && lcpdfrPlayer.Skin.Model == new Model("M_Y_SWAT") || lcpdfrPlayer.Skin.Model == new Model("M_Y_NHELIPILOT"))
            {
                bool usePrefixPistolModel = SettingsIni.GetValueBool("UsePrefixPistolModel", "WorldSettings", false);
                if (lcpdfrPlayer.Ped.IsSittingInVehicle() || lcpdfrPlayer.Ped.IsInVehicle())
                {
                    currentVeh = lcpdfrPlayer.Ped.CurrentVehicle;
                    UInt64 hash = currentVeh.Model.ModelInfo.Hash;
                    do
                    {
                        if (hash == Enforcer)
                        {
                            Weapon PrimaryWeapon1, PrimaryWeapon2, PrefixPistol = new Weapon();
                            int PrimaryAmmo1, PrimaryAmmo2, PistolAmmo;
                            switch (SettingsIni.GetValueString("PrimaryWeapon1", "WorldSettings")) // case-sensitive
                            {
                                // Default (when no value is present), then cases
                                // Handles SMG, Rifles, Sniper Rifles, and Heavy Weapons
                                    // Runs on Episodic Check
                                default: PrimaryWeapon1 = Weapon.SMG_MP5; break;
                                case "Rifle_AK47": PrimaryWeapon1 = Weapon.Rifle_AK47; break;
                                case "Rifle_M4": PrimaryWeapon1 = Weapon.Rifle_M4; break;
                                case "TBOGT_AdvancedMG": if (currentGameEpPlatform == GameEpisode.TBOGT)
                                    {
                                        PrimaryWeapon1 = Weapon.TBOGT_AdvancedMG; // M249
                                    }
                                    else { PrimaryWeapon1 = Weapon.Rifle_M4; } break;
                                case "TBOGT_AdvancedSniper": if (currentGameEpPlatform == GameEpisode.TBOGT)
                                    {
                                        PrimaryWeapon1 = Weapon.TBOGT_AdvancedSniper; // DSR-1
                                    }
                                    else { PrimaryWeapon1 = Weapon.SniperRifle_M40A1; } break;
                                case "SMG_MP5": PrimaryWeapon1 = Weapon.SMG_MP5; break;
                                case "SniperRifle_Basic": PrimaryWeapon1 = Weapon.SniperRifle_Basic; break;
                                case "SniperRifle_M40A1": PrimaryWeapon1 = Weapon.SniperRifle_M40A1; break;
                                case "Heavy_RocketLauncher": PrimaryWeapon1 = Weapon.Heavy_RocketLauncher; break;
                                case "TLAD_GrenadeLauncher": if (currentGameEpPlatform == GameEpisode.TLAD)
                                    {
                                        PrimaryWeapon1 = Weapon.TLAD_GrenadeLauncher; // HK69A1 40mm
                                    }
                                    else { PrimaryWeapon1 = Weapon.None; } break;
                                case "TBOGT_GrenadeLauncher": if (currentGameEpPlatform == GameEpisode.TBOGT)
                                    {
                                        PrimaryWeapon1 = Weapon.TBOGT_GrenadeLauncher; // HK69A1 40mm
                                    }
                                    else { PrimaryWeapon1 = Weapon.None; } break;
                                case "TBOGT_AssaultSMG": if (currentGameEpPlatform == GameEpisode.TBOGT)
                                    {
                                        PrimaryWeapon1 = Weapon.TBOGT_AssaultSMG; // P90
                                    }
                                    else { PrimaryWeapon1 = Weapon.SMG_MP5; } break;
                            }
                            switch (SettingsIni.GetValueString("PrimaryWeapon2", "WorldSettings")) // case-sensitive
                            {
                                default: PrimaryWeapon2 = Weapon.Shotgun_Baretta; break;
                                case "Shotgun_Baretta": PrimaryWeapon2 = Weapon.Shotgun_Baretta; break;
                                case "Shotgun_Basic": PrimaryWeapon2 = Weapon.Shotgun_Basic; break;
                                case "TBOGT_ExplosiveShotgun": if (currentGameEpPlatform == GameEpisode.TBOGT)
                                    {
                                        PrimaryWeapon2 = Weapon.TBOGT_ExplosiveShotgun; // AA-12 12ga Frag-12
                                    }
                                    else { PrimaryWeapon2 = Weapon.Shotgun_Baretta; } break;
                                case "TBOGT_NormalShotgun": if (currentGameEpPlatform == GameEpisode.TBOGT)
                                    {
                                        PrimaryWeapon2 = Weapon.TBOGT_NormalShotgun; // AA-12 Shotgun
                                    }
                                    else { PrimaryWeapon2 = Weapon.Shotgun_Baretta; } break;
                                case "TLAD_AssaultShotgun": if (currentGameEpPlatform == GameEpisode.TLAD)
                                    {
                                        PrimaryWeapon2 = Weapon.TLAD_AssaultShotgun; // Striker
                                    }
                                    else { PrimaryWeapon2 = Weapon.Shotgun_Baretta; } break;
                                case "TLAD_SawedOffShotgun": if (currentGameEpPlatform == GameEpisode.TLAD)
                                    {
                                        PrimaryWeapon2 = Weapon.TLAD_SawedOffShotgun; // Lupara?
                                    }
                                    else { PrimaryWeapon2 = Weapon.Shotgun_Basic; } break;
                            }
                            PrimaryAmmo1 = SettingsIni.GetValueInteger("PrimaryAmmo1", "WorldSettings", 30);
                            PrimaryAmmo2 = SettingsIni.GetValueInteger("PrimaryAmmo2", "WorldSettings", 8);
                            PistolAmmo = SettingsIni.GetValueInteger("PistolAmmo", "WorldSettings", 15);

                            // Checks if the prefix pistol model value returned true
                            if (usePrefixPistolModel)
                            {
                                switch (SettingsIni.GetValueString("PrefixPistolModel", "WorldSettings"))
                                {
                                    default: PrefixPistol = Weapon.Handgun_Glock; break;
                                    case "Handgun_Glock": PrefixPistol = Weapon.Handgun_Glock; break;
                                    case "Handgun_DesertEagle": PrefixPistol = Weapon.Handgun_DesertEagle; break;
                                    case "TLAD_Automatic9mm": if (currentGameEpPlatform == GameEpisode.TLAD)
                                        {
                                            PrefixPistol = Weapon.TLAD_Automatic9mm; // CZ75-Auto
                                        }
                                        else { PrefixPistol = Weapon.Handgun_Glock; } break;
                                    case "TBOGT_Pistol44": if (currentGameEpPlatform == GameEpisode.TBOGT)
                                        {
                                            PrefixPistol = Weapon.TBOGT_Pistol44; // SA Auto .44
                                        }
                                        else { PrefixPistol = Weapon.Handgun_DesertEagle; } break;
                                }
                            }

                            // First primary depend on the weapon slot
                            lcpdfrPlayer.Ped.Weapons.FromType(PrimaryWeapon1).Ammo += PrimaryAmmo1;

                            // Second primary have their own slot: Shotguns
                            lcpdfrPlayer.Ped.Weapons.FromType(PrimaryWeapon2).Ammo += PrimaryAmmo2;

                            // Secondary is pistol
                            if (usePrefixPistolModel)
                            {
                                lcpdfrPlayer.Ped.Weapons.FromType(PrefixPistol).Ammo += PistolAmmo;
                            }
                            else
                            {
                                lcpdfrPlayer.Ped.Weapons.AnyHandgun.Ammo += PistolAmmo;
                            }

                            // Free Grenades! (Molotovs or other throwables are replaced)
                            lcpdfrPlayer.Ped.Weapons.FromType(Weapon.Thrown_Grenade).Ammo += 3;

                            // Refresh every 0.5 second
                            Game.WaitInCurrentScript(500);
                        }
                        else break;
                    } while (lcpdfrPlayer.Ped.IsInVehicle(currentVeh));
                }
                else return;
            }
            // If return is active, this method will loop around, making others not be processed
            //else return;
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
            if (overallstatsdt.Rows.Count != 0 && overallstatsdt.IsInitialized && lcpdfrPlayer.IsOnDuty && missionstatsdt.IsInitialized
                && lcpdfrPlayer.Skin.Model == new Model("M_Y_SWAT") || lcpdfrPlayer.Skin.Model == new Model("M_Y_NHELIPILOT"))
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

            if (lcpdfrPlayer.IsInPoliceDepartment && lcpdfrPlayer.Ped.Position.DistanceTo(PartnerRoom) < 1f)
            {
                Functions.PrintHelp("To reset the save progress, press ~KEY_ACCEPT_CALLOUT~ to confirm while standing.");
                Functions.PrintText("Reset NooseMod save progress?", 5000);
                if (Functions.IsKeyDown(SettingsFile.Open("LCPDFR\\LCPDFR.ini").GetValueKey("AcceptCallout", "Keybindings", Keys.Y)))
                {
                    // Reset status
                    StreamWriter streamWriter = new StreamWriter("NooseMod\\save.txt");
                    streamWriter.WriteLine(value.ToString());
                    streamWriter.Close();

                    // Notify user
                    Functions.PrintText("NooseMod save progress has been reset.", 5000);
                    Log.Info("Progress has been reset", this);
                    Function.Call("TRIGGER_MISSION_COMPLETE_AUDIO", new Parameter[] { 1 });
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
        // TODO: input console command for NooseMod showing GPL 3.0 license
        #endregion
    }
}