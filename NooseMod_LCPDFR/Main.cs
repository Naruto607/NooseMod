//    NooseMod LCPDFR Plugin with Database System
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
    // World Event is not yet complete - DO NOT REMOVE COMMENT
    //using NooseMod_LCPDFR.World_Events;
    using SlimDX.XInput;
    using System;
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
        internal protected const Int32 Enforcer = 1911513875;

        /// <summary>
        /// Current Game Episode where the LCPDFR is running from
        /// </summary>
        private GameEpisode currentGameEpPlatform;

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
        /// NooseMod Statistics database
        /// </summary>
        private StatsDataSet stats = new StatsDataSet();

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

            Log.Info("Started", this);
        }

        /// <summary>
        /// Called when player changed the on duty state.
        /// For the NooseMod to work, you need to play as a NOOSE/SWAT member.
        /// </summary>
        /// <param name="onDuty">The new on duty state.</param>
        public void Functions_OnOnDutyStateChanged(bool onDuty)
        {
            // Register callouts to LCPDFR depending on skin model
            if (onDuty && lcpdfrPlayer.Skin.Model == new Model("M_Y_SWAT") || lcpdfrPlayer.Skin.Model == new Model("M_Y_NHELIPILOT"))
            {
                // SWAT model used: register NooseMod
                Functions.RegisterCallout(typeof(NooseMod)); // TODO: expand to 8 missions
                stats.OverallStats.AddOverallStatsRow(0, 0, 0, 0, 0, 0); // Build new Stats row
                loginNumbers = stats.OverallStats.Rows.Count; // Record how many rows made
            }
            else if (onDuty && lcpdfrPlayer.Skin.Model != new Model("M_Y_SWAT") || lcpdfrPlayer.Skin.Model != new Model("M_Y_NHELIPILOT"))
            {
                // Otherwise register TerroristPursuit callout
                //Game.DisplayText("You need to play as a NOOSE/SWAT member to start NooseMod plugin.");
                Functions.RegisterCallout(typeof(TerroristPursuit));
            }
            // World Event is not yet complete - DO NOT REMOVE COMMENT
            //Functions.AddWorldEvent(typeof(ProvostPatrol), "Provost Patrol");
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
            stats.Dispose();
            //Log.Info(stats.Disposed, this);
        }

        /// <summary>
        /// If implemented, this is used to start <see cref="NooseMod"/> callout through the ScriptHookDotNet (SHDN) Console.
        /// </summary>
        /// <param name="parameterCollection">Parameters to start certain callout</param>
        [ConsoleCommand("StartCallout", false)]
        private void StartCallout(ParameterCollection parameterCollection)
        {
            // Jack this for allowing missions to be loaded
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
            int tempVar1 = stats.OverallStats.FindBySession(loginNumbers).Suspects_Killed,
                tempVar2 = stats.OverallStats.FindBySession(loginNumbers).Suspects_Arrested,
                tempVar3 = stats.OverallStats.FindBySession(loginNumbers).Hostages_Killed,
                tempVar4 = stats.OverallStats.FindBySession(loginNumbers).Hostages_Rescued,
                tempVar5 = stats.OverallStats.FindBySession(loginNumbers).Officer_Casualties,
                tempVar6 = stats.OverallStats.FindBySession(loginNumbers).Squad_Casualties;

            Game.Console.Print("NooseMod Statistics");
            Game.Console.Print("---");
            Game.Console.Print("Your login session: "+loginNumbers);
            Game.Console.Print("Total Suspects killed: "+tempVar1);
            Game.Console.Print("Total Suspects arrested: "+tempVar2);
            Game.Console.Print("Total Hostages killed: " + tempVar3);
            Game.Console.Print("Total Hostages rescued: " + tempVar4);
            Game.Console.Print("Total Officers killed in mission: " + tempVar5);
            Game.Console.Print("Total Squad casualties: " + tempVar6);
        }

        #region LCPDFR External Procedures
        /// <summary>
        /// Processes difficulty change when inside the Chief of Police's Office Room
        /// </summary>
        private void ChiefOfPolice_DifficultySelection()
        {
            // If player enters any police station, enters the Chief of Police's office room, and enters a marker, set difficulty
            //Blip selectDifficultyBlip = Blip.AddBlip(ChiefOfPoliceRoom);
            bool isHardcoreModeActive = controller.HardcoreModeIsActive();
            GTA.Timer time = new GTA.Timer();
            int waitTime = 1500;
            if (lcpdfrPlayer.IsInPoliceDepartment && lcpdfrPlayer.IsOnDuty && lcpdfrPlayer.Ped.Position.DistanceTo(ChiefOfPoliceRoom) < 1f && !isHardcoreModeActive
                && lcpdfrPlayer.Skin.Model == new Model("M_Y_SWAT") || lcpdfrPlayer.Skin.Model == new Model("M_Y_NHELIPILOT"))
            {
                // Play an animation
                this.isSelectingDifficulty = true;
                int[] tempDiff = { 1, 2, 3 };
                int keyPress = 0;
                //AnimationSet difficultyPlayAnim = new AnimationSet("cellphone");
                Functions.PrintHelp("Use left and right arrows to select a difficulty. When done, press ~KEY_ARREST~ to confirm. Otherwise press ~KEY_DIALOG_4~ to quit.");
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

                    if (Functions.IsKeyDown(Keys.E))
                    {
                        SettingsIni.SetValue("Difficulty", "GlobalSettings", tempDiff[keyPress]);
                        SettingsIni.Save();
                        Functions.PrintText("Difficulty has been set",5000);
                        //lcpdfrPlayer.Ped.Task.ClearAllImmediately();
                        lcpdfrPlayer.Ped.Task.PutAwayMobilePhone();
                        lcpdfrPlayer.Ped.Task.ClearAll();
                        time.Start();
                        while (time.ElapsedTime < waitTime)
                        {
                            if (time.ElapsedTime >= waitTime)
                            {
                                time.Stop();
                                time.Interval = 0;
                                break;
                            }
                        }
                        this.isSelectingDifficulty = false;
                    }
                    else if (Functions.IsKeyDown(Keys.F12))
                    {
                        //lcpdfrPlayer.Ped.Task.ClearAllImmediately();
                        lcpdfrPlayer.Ped.Task.PutAwayMobilePhone();
                        lcpdfrPlayer.Ped.Task.ClearAll();
                        time.Start();
                        while (time.ElapsedTime < waitTime)
                        {
                            if (time.ElapsedTime >= waitTime)
                            {
                                time.Stop();
                                time.Interval = 0;
                                break;
                            }
                        }
                        this.isSelectingDifficulty = false;
                    }
                }
            }
        }

        /// <summary>
        /// Processes the ability to rearm player when driving an Enforcer
        /// </summary>
        private void ProcessEnforcerAbility()
        {
            bool usePrefixPistolModel = SettingsIni.GetValueBool("UsePrefixPistolModel", "WorldSettings", false);
            if (lcpdfrPlayer.Ped.IsInVehicle() && lcpdfrPlayer.IsOnDuty && lcpdfrPlayer.Skin.Model == new Model("M_Y_SWAT") || lcpdfrPlayer.Skin.Model == new Model("M_Y_NHELIPILOT"))
            {
                currentVeh = lcpdfrPlayer.Ped.CurrentVehicle;
                if (currentVeh.GetHashCode() == Enforcer)
                {
                    Weapon PrimaryWeapon1, PrimaryWeapon2, PrefixPistol = new Weapon();
                    int PrimaryAmmo1, PrimaryAmmo2, PistolAmmo;
                    switch (SettingsIni.GetValueString("PrimaryWeapon1", "WorldSettings")) // case-sensitive
                    {
                        // Default (when no value is present), then cases
                        // Handles SMG, Rifles, Sniper Rifles, and Heavy Weapons
                        default: PrimaryWeapon1 = Weapon.SMG_MP5; break;
                        case "Rifle_AK47": PrimaryWeapon1 = Weapon.Rifle_AK47; break;
                        case "Rifle_M4": PrimaryWeapon1 = Weapon.Rifle_M4; break;
                        case "TBOGT_AdvancedMG": if (currentGameEpPlatform == GameEpisode.TBOGT)
                            {
                                PrimaryWeapon1 = Weapon.TBOGT_AdvancedMG;
                            }
                            else { PrimaryWeapon1 = Weapon.Rifle_M4; } break;
                        case "TBOGT_AdvancedSniper": if (currentGameEpPlatform == GameEpisode.TBOGT)
                            {
                                PrimaryWeapon1 = Weapon.TBOGT_AdvancedSniper;
                            }
                            else { PrimaryWeapon1 = Weapon.SniperRifle_M40A1; } break;
                        case "SMG_MP5": PrimaryWeapon1 = Weapon.SMG_MP5; break;
                        case "SniperRifle_Basic": PrimaryWeapon1 = Weapon.SniperRifle_Basic; break;
                        case "SniperRifle_M40A1": PrimaryWeapon1 = Weapon.SniperRifle_M40A1; break;
                        case "Heavy_RocketLauncher": PrimaryWeapon1 = Weapon.Heavy_RocketLauncher; break;
                        case "TLAD_GrenadeLauncher": if (currentGameEpPlatform == GameEpisode.TLAD)
                            {
                                PrimaryWeapon1 = Weapon.TLAD_GrenadeLauncher;
                            }
                            else { PrimaryWeapon1 = Weapon.None; } break;
                        case "TBOGT_GrenadeLauncher": if (currentGameEpPlatform == GameEpisode.TBOGT)
                            {
                                PrimaryWeapon1 = Weapon.TBOGT_GrenadeLauncher;
                            }
                            else { PrimaryWeapon1 = Weapon.None; } break;
                        case "TBOGT_AssaultSMG": if (currentGameEpPlatform == GameEpisode.TBOGT)
                            {
                                PrimaryWeapon1 = Weapon.TBOGT_AssaultSMG;
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
                                PrimaryWeapon2 = Weapon.TBOGT_ExplosiveShotgun;
                            }
                            else { PrimaryWeapon2 = Weapon.Shotgun_Baretta; } break;
                        case "TBOGT_NormalShotgun": if (currentGameEpPlatform == GameEpisode.TBOGT)
                            {
                                PrimaryWeapon2 = Weapon.TBOGT_NormalShotgun;
                            }
                            else { PrimaryWeapon2 = Weapon.Shotgun_Baretta; } break;
                        case "TLAD_AssaultShotgun": if (currentGameEpPlatform == GameEpisode.TLAD)
                            {
                                PrimaryWeapon2 = Weapon.TLAD_AssaultShotgun;
                            }
                            else { PrimaryWeapon2 = Weapon.Shotgun_Baretta; } break;
                        case "TLAD_SawedOffShotgun": if (currentGameEpPlatform == GameEpisode.TLAD)
                            {
                                PrimaryWeapon2 = Weapon.TLAD_SawedOffShotgun;
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
                                    PrefixPistol = Weapon.TLAD_Automatic9mm;
                                }
                                else { PrefixPistol = Weapon.Handgun_Glock; } break;
                            case "TBOGT_Pistol44": if (currentGameEpPlatform == GameEpisode.TBOGT)
                                {
                                    PrefixPistol = Weapon.TBOGT_Pistol44;
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
                }
            }
            // Do nothing if not in an Enforcer
        }

        /// <summary>
        /// Summarizes all Mission Stats into an Overall Stats
        /// </summary>
        private void SummarizeOverallStats()
        {
            int tempVar1 = 0, tempVar2 = 0, tempVar3 = 0, tempVar4 = 0, tempVar5 = 0, tempVar6 = 0;
            //decimal tempVar7 = 0;
            // Get Values
            if (stats.OverallStats.Rows.Count != 0)
            try
            {
                for (int i = 1; i <= stats.MissionStats.Rows.Count; i++)
                {
                    tempVar1 = tempVar1 + stats.MissionStats.FindByMission_Number(i).Suspects_Killed;
                    tempVar2 = tempVar2 + stats.MissionStats.FindByMission_Number(i).Suspects_Arrested;
                    tempVar3 = tempVar3 + stats.MissionStats.FindByMission_Number(i).Hostages_Killed;
                    tempVar4 = tempVar4 + stats.MissionStats.FindByMission_Number(i).Hostages_Rescued;
                    tempVar5 = tempVar5 + stats.MissionStats.FindByMission_Number(i).Officer_Casualties;
                    tempVar6 = tempVar6 + stats.MissionStats.FindByMission_Number(i).Squad_Casualties;
                    //tempVar7 = tempVar7 + stats.MissionStats.FindByMission_Number(i).Income;
                }
                // Write the Summary
                stats.OverallStats.FindBySession(loginNumbers).Suspects_Killed = tempVar1;
                stats.OverallStats.FindBySession(loginNumbers).Suspects_Arrested = tempVar2;
                stats.OverallStats.FindBySession(loginNumbers).Hostages_Killed = tempVar3;
                stats.OverallStats.FindBySession(loginNumbers).Hostages_Rescued = tempVar4;
                stats.OverallStats.FindBySession(loginNumbers).Officer_Casualties = tempVar5;
                stats.OverallStats.FindBySession(loginNumbers).Squad_Casualties = tempVar6;
            }
            catch (Exception ex) { Log.Error("Error writing statistics to database: " + ex.Message, this); }
        }

        /// <summary>
        /// This method resets the save stats so that you can play the NooseMod callout again.
        /// </summary>
        private void InitializeReset()
        {
            int value = -1;
            // If player is near a terminal, show message
            //Blip resetZone = Blip.AddBlip(PartnerRoom);
            GTA.Timer time = new GTA.Timer();
            int timeToResetAgain = 9999;

            if (lcpdfrPlayer.IsInPoliceDepartment && lcpdfrPlayer.IsOnDuty && lcpdfrPlayer.Ped.Position.DistanceTo(PartnerRoom) < 1f)
            {
                Functions.PrintHelp("To reset the save progress, press ~KEY_ARREST~ to confirm while standing.");
                Functions.PrintText("Reset NooseMod save progress?", 5000);
                if (Functions.IsKeyDown(Keys.E))
                {
                    // Reset status
                    StreamWriter streamWriter = new StreamWriter("NooseMod\\save.txt");
                    streamWriter.WriteLine(value.ToString());
                    streamWriter.Close();

                    // Notify user
                    Functions.PrintText("NooseMod save progress has been reset.", 5000);
                    Log.Info("Progress has been reset", this);
                    time.Start();
                    while (time.ElapsedTime < timeToResetAgain)
                    {
                        if (time.ElapsedTime >= timeToResetAgain)
                        { time.Stop(); time.Interval = 0; break; }
                    }
                }
            }
        }
        // TODO: input console command for NooseMod showing GPL 3.0 license
        #endregion
    }
}