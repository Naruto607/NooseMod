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

#region Uses
using GTA;
using GTA.Native;
using LCPD_First_Response.Engine;
using LCPD_First_Response.Engine.Scripting.Entities;
using LCPD_First_Response.LCPDFR.API;
using LCPD_First_Response.LCPDFR.Callouts;
using NAudio;
using NAudio.Wave;
using NooseMod_LCPDFR.Mission_Controller;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
//using System.Windows.Forms;
#endregion

namespace NooseMod_LCPDFR.Callouts
{
    /// <summary>
    /// The Main Callout Script
    /// </summary>
    [CalloutInfo("Terrorist Activity", ECalloutProbability.VeryLow)]
    // A very low chance for these terror guys to make an assault to one place
    // They require careful planning and by any chances, if succeed, police will radio it in
	internal class NooseMod : Callout
    {
        #region Initialization
        // Monetary
        #region Monetary
        /// <summary>
        /// How much money gained for killed suspects (terrorists)
        /// </summary>
        private const int MoneyForEachDeadSuspect = 300;

        /// <summary>
        /// How much money lost when a hostage killed during neutralization
        /// </summary>
        private const int PenaltyForDeadHostage = 600;

        /// <summary>
        /// If during Normal Pursuit, how much money gained for suspects (terrorists) that
        /// are arrested (LCPDFR feature)
        /// </summary>
        private const int MoneyForEachArrestedSuspect = 750;

        /// <summary>
        /// How much money gained for hostages that are rescued
        /// </summary>
        private const int RewardForRescuedHostages = 450;

        /// <summary>
        /// How much money lost when a cop is killed
        /// </summary>
        private const int PenaltyForOfficerCasualties = 90;

        /// <summary>
        /// How much money lost when a squad is killed
        /// </summary>
        private const int PenaltyForSquadCasualties = 150;
        #endregion

        // Mission Control variables
        #region Mission Control
        /// <summary>
        /// The state of a mission
        /// </summary>
		private MissionState mission;

        /// <summary>
        /// NooseMod Mission Difficulty
        /// </summary>
        private Difficulty difficulty = Difficulty.Medium;

        /// <summary>
        /// Currently active mission
        /// </summary>
        private Mission activeMission;

        /// <summary>
        /// Your last played mission
        /// </summary>
        private Int16 lastPlayedMission;

        /// <summary>
        /// NooseMod Mission Controller
        /// </summary>
        private MissionController controller = new MissionController();

        /// <summary>
        /// NooseMod Statistics database
        /// </summary>
        private StatsDataSet stats = new StatsDataSet();

        /// <summary>
        /// If terrorists fled, this determines whether the session turns to cop chase
        /// </summary>
        private bool isPursuitSessionActive = false;
        #endregion

        // Kills and Arrests
        #region Kills and Arrests
        /// <summary>
        /// List of dead suspects
        /// </summary>
        //private List<LPed> deadSuspects = new List<LPed>(2 ^ SettingsIni.GetValueInteger("MissionPeds","ListRanging",5));
        private List<LPed> deadSuspects = new List<LPed>();

        /// <summary>
        /// List of arrested suspects
        /// </summary>
        //private List<LPed> arrestedSuspects = new List<LPed>(2 ^ SettingsIni.GetValueInteger("MissionPeds", "ListRanging", 5));
        private List<LPed> arrestedSuspects = new List<LPed>();

        /// <summary>
        /// List of NOOSE Squad
        /// </summary>
        //private List<LPed> squad = new List<LPed>(2 ^ SettingsIni.GetValueInteger("SquadPeds", "ListRanging", 5));
        private List<LPed> squad = new List<LPed>();

        /// <summary>
        /// List of hostages
        /// </summary>
        //private List<Ped> hostages = new List<Ped>(2 ^ SettingsIni.GetValueInteger("HostagePeds", "ListRanging", 5));
        private List<Ped> hostages = new List<Ped>();

        /// <summary>
        /// List of dead hostages
        /// </summary>
        //private List<Ped> deadHostages = new List<Ped>(2 ^ SettingsIni.GetValueInteger("HostagePeds", "ListRanging", 5));
        private List<Ped> deadHostages = new List<Ped>();

        /// <summary>
        /// List of Mission Peds
        /// </summary>
        //private List<LPed> missionPeds = new List<LPed>(2 ^ SettingsIni.GetValueInteger("MissionPeds", "ListRanging", 5));
        private List<LPed> missionPeds = new List<LPed>();

        /// <summary>
        /// Counter on how many suspects (terrorists) left
        /// </summary>
        private int suspectsLeft;
        #endregion

        // File Reader
        #region Audio Reader
        /// <summary>
        /// Reads the MP3 file from NooseMod directory (for Epilogue BGM)
        /// </summary>
        private Mp3FileReader myAudio1 = new Mp3FileReader("LCPDFR\\Plugins\\NooseMod\\sounds\\epilogue.mp3");

        /// <summary>
        /// Reads the WAV file from NooseMod directory (alternate LCPDFR end callout sound, thanks to Sam)
        /// </summary>
        private WaveFileReader myAudio2 = new WaveFileReader("LCPDFR\\Plugins\\NooseMod\\sounds\\MP.wav");
        #endregion

        // LCPDFR variables
        #region LCPDFR Variables
        /// <summary>
        /// The spawn position
        /// </summary>
        private Vector3 spawnPosition;

        /// <summary>
        /// Whether the remaining terrors want to flee the scene in a shootout with cops
        /// blocking the entry point
        /// </summary>
        private LHandle pursuit;
        #endregion

        // Control for backup vehicles and peds
        #region Backup Vehicles and Peds Control
        /// <summary>
        /// The first NOOSE vehicle
        /// </summary>
        private LVehicle nooseVeh1;

        /// <summary>
        /// The second NOOSE vehicle
        /// </summary>
        private LVehicle nooseVeh2;

        /// <summary>
        /// Array of NOOSE Squads
        /// </summary>
        private LPed[] SquadTroops;

        /// <summary>
        /// Squad to spawn in
        /// </summary>
        private LPed SquadTroop;

        /// <summary>
        /// Squad to spawn in
        /// </summary>
        private LPed SquadTroop1;

        /// <summary>
        /// Squad to spawn in
        /// </summary>
        private LPed SquadTroop2;

        // Using CModelInfo - look for model name and its hash here: https://www.gtamodding.com/wiki/List_of_models_hashes
        /// <summary>
        /// Model Information of NOOSE/SWAT trooper using LCPDFR basis
        /// </summary>
        private CModel SWATTrooper = new CModel(new CModelInfo("M_Y_SWAT", 3290204350, EModelFlags.IsNoose));

        /// <summary>
        /// List of Police Officers in the crime scene
        /// </summary>
        //private List<LPed> PoliceOfficers = new List<LPed>(2 ^ SettingsIni.GetValueInteger("PoliceOfficers", "ListRanging", 5));
        private List<LPed> PoliceOfficers = new List<LPed>();
        #endregion

        /// <summary>
        /// Settings File
        /// </summary>
        internal SettingsFile SettingsIni = SettingsFile.Open("LCPDFR\\Plugins\\NooseMod.ini");
        #endregion

        #region LCPDFR External Procedures
        // TODO: Implement new settings for suspect blip like original NooseMod does
        /*internal dynamic BlipSettings(string t, bool b, Blip q)
        {
            
        }*/

        /// <summary>
        /// Plays Mission Complete sound when the threat is neutralized
        /// </summary>
        private void PlayFinishedMissionSound()
        {
            /*Function.Call("TRIGGER_MISSION_COMPLETE_AUDIO", new Parameter[]
			{
				new Parameter(1)
			});*/
            WaveChannel32 aud = new WaveChannel32(myAudio2);
            DirectSoundOut MPlayer = new DirectSoundOut();
            MPlayer.Init(aud);
            MPlayer.Play();
        }

        /// <summary>
        /// Writes the current progress into the database
        /// </summary>
        /// <param name="KS">Suspects Killed</param>
        /// <param name="AS">Suspects Arrested</param>
        /// <param name="KH">Hostages Killed</param>
        /// <param name="RH">Hostages Rescued</param>
        /// <param name="OCst">Police Officer Casualties</param>
        /// <param name="SQCst">NOOSE Squad Casualties</param>
        /// <param name="cash">Money Earned</param>
        private void WriteStatistics(int KS, int AS, int KH, int RH, int OCst, int SQCst, decimal cash)
        {
            Log.Info("Writing Statistics...", this);
            try
            {
                stats.MissionStats.FindByMission_Number(this.lastPlayedMission).Suspects_Killed = KS;
                stats.MissionStats.FindByMission_Number(this.lastPlayedMission).Suspects_Arrested = AS;
                stats.MissionStats.FindByMission_Number(this.lastPlayedMission).Hostages_Killed = KH;
                stats.MissionStats.FindByMission_Number(this.lastPlayedMission).Hostages_Rescued = RH;
                stats.MissionStats.FindByMission_Number(this.lastPlayedMission).Officer_Casualties = OCst;
                stats.MissionStats.FindByMission_Number(this.lastPlayedMission).Squad_Casualties = SQCst;
                stats.MissionStats.FindByMission_Number(this.lastPlayedMission).Income = cash;
                Log.Info("Write Successful", this);
            }
            catch (Exception ex) { Log.Error("Error writing stats to database: " + ex.Message, this); }
        }

        /// <summary>
        /// Plays the external audio in the game
        /// </summary>
        /// <param name="play">Whether or not the sound will play</param>
        private void PlaySound(bool play)
        {
            WaveChannel32 aud = new WaveChannel32(myAudio1);
            DirectSoundOut MPlayer = new DirectSoundOut();
            MPlayer.Init(aud);
            MPlayer.Volume = 1.15f; // 115% sound volume
            if (play)
            {
                MPlayer.Play();
            }
            else { MPlayer.Stop(); }
        }

        /// <summary>
        /// Gets the difficulty based on the settings you choose
        /// </summary>
        /// <returns><see cref="Difficulty"/> of a mission</returns>
        private Difficulty getDifficulty()
        {
            int value = SettingsIni.GetValueInteger("Difficulty", "GlobalSettings", 2);
            Difficulty fixedDiff = new Difficulty();
            switch (value)
            {
                case 1: fixedDiff = Difficulty.Easy; break;
                case 2: fixedDiff = Difficulty.Medium; break;
                case 3: fixedDiff = Difficulty.Hard; break;
                default: throw new Exception("No difficulty is set");
            }
            return fixedDiff;
        }

        /// <summary>
        /// Saves the game so that you will not lost track on the progress you made
        /// </summary>
        private void SaveGame()
        {
            try
            {
                StreamWriter streamWriter = new StreamWriter("LCPDFR\\Plugins\\NooseMod\\save.txt");
                streamWriter.WriteLine(this.lastPlayedMission.ToString());
                streamWriter.Close();
            }
            catch (Exception ex) { Log.Error("Unable to read or write save.txt: " + ex.Message, this); }
        }
        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="NooseMod"/> class.
        /// </summary>
		public NooseMod()
        {
            // Load last saved game
            controller.LoadGame();
            lastPlayedMission = controller.lastPlayedMission;

            // increment the current value (if -1, it will load the list zero)
            lastPlayedMission++;

            // Time check, if a mission is called on a specified time
            Mission currentMissionToPlayNow = controller.missionVariableCheckup(lastPlayedMission);

            // Get Mission Time
            TimeSpan currentTime = World.CurrentDayTime;
            bool isExceedingMissionList = lastPlayedMission >= controller.loadedMissions.Count;
            if (isExceedingMissionList == false)
            {
                if (currentMissionToPlayNow.IsMissionTimeSpecified)
                {
                    TimeSpan interval1 = currentMissionToPlayNow.MissionTime - TimeSpan.FromHours(1.0f);
                    TimeSpan interval2 = currentMissionToPlayNow.MissionTime + TimeSpan.FromHours(1.0f);
                    TimeSpan interval3 = new TimeSpan(23, 59, 0);

                    if (interval1.Hours == -1)
                    {
                        interval1 = new TimeSpan(23, 0, 0);
                    }

                    // Automatically start when the game clock is between the defined intervals.
                    // For example, if the mission time defined is at 00:00 precise, callout will start
                    //  between 23:00 and 01:00. For the best effect and response time, using Time Lock is recommended,
                    //  but it will limit access to some features, for example, if you have The Wasteland plugin.
                    if (currentTime >= interval1 && currentTime <= interval2)
                    {
                        activeMission = controller.PlayMission(lastPlayedMission);
                    }

                    // Check if the mission starts at 00:00, as from 23:59 it will reset to 00:00
                    else if (currentTime >= interval1 && currentTime <= interval3 && currentTime >= new TimeSpan(0, 0, 0) && currentTime <= interval2)
                    {
                        activeMission = controller.PlayMission(lastPlayedMission);
                    }

                    // No resources loaded, so if not within specified time, end it from the call base
                    else { base.End(); return; }
                }

                // Start immediately the mission if the mission time is not specified
                else { activeMission = controller.PlayMission(lastPlayedMission); }
            }

                // End the callout from the base if the save file is exceeding the mission list
            else { base.End(); return; }

            // Use Mission Location to start
            spawnPosition = controller.entryLoc.Position;

            // Register location
            ShowCalloutAreaBlipBeforeAccepting(spawnPosition, 30.0f);
            AddMinimumDistanceCheck(300, spawnPosition);

            // Call it in
            int i = Common.GetRandomValue(0, 4);
            switch (i)
            {
                case 0:
                    this.CalloutMessage = string.Format(Functions.GetStringFromLanguageFile("CALLOUT_NOOSEMOD_TERRORIST_ACTIVITY"), Functions.GetAreaStringFromPosition(this.spawnPosition));
                    Functions.PlaySoundUsingPosition("THIS_IS_CONTROL ATTENTION_ALL_UNITS INS_WE_HAVE_A_REPORT_OF_ERRR CRIM_TERRORIST_ACTIVITY IN_OR_ON_POSITION", this.spawnPosition);
                    break;
                case 1:
                    this.CalloutMessage = string.Format(Functions.GetStringFromLanguageFile("CALLOUT_NOOSEMOD_TERRORIST_ACTIVITY_2"), Functions.GetAreaStringFromPosition(this.spawnPosition));
                    Functions.PlaySoundUsingPosition("UNITS_PLEASE_BE_ADVISED INS_WE_HAVE A_LONGER INS_TRAFFIC_ALERT_FOR CRIM_A_CRIMINAL_ON_A_GUN_SPREE IN_OR_ON_POSITION", this.spawnPosition);
                    break;
                case 2:
                    this.CalloutMessage = string.Format(Functions.GetStringFromLanguageFile("CALLOUT_NOOSEMOD_TERRORIST_ACTIVITY_3"), Functions.GetAreaStringFromPosition(this.spawnPosition));
                    Functions.PlaySoundUsingPosition("ALL_UNITS_ALL_UNITS INS_WE_HAVE CRIM_AN_SOS FOR CRIM_AN_OFFICER_IN_NEED_OF_ASSISTANCE IN_OR_ON_POSITION", this.spawnPosition);
                    break;
                case 3:
                    this.CalloutMessage = string.Format(Functions.GetStringFromLanguageFile("CALLOUT_NOOSEMOD_TERRORIST_ACTIVITY_4"), Functions.GetAreaStringFromPosition(this.spawnPosition));
                    Functions.PlaySoundUsingPosition("THIS_IS_CONTROL INS_AVAILABLE_UNITS_RESPOND_TO ERR_ERRR_14 CRIM_POSSIBLE_TERRORIST_ACTIVITY IN_OR_ON_POSITION", this.spawnPosition);
                    break;
                case 4:
                    bool isBikerGangUsedInCallout = SettingsIni.GetValueBool("SwapCriminalsToBikerGangMembers", "GlobalSettings", false);
                    if (isBikerGangUsedInCallout == true)
                    {
                        this.CalloutMessage = string.Format(Functions.GetStringFromLanguageFile("CALLOUT_NOOSEMOD_TERRORIST_ACTIVITY_5"), Functions.GetAreaStringFromPosition(this.spawnPosition));
                        Functions.PlaySoundUsingPosition("ALL_UNITS INS_WEVE_GOT ERR_ERRR_14 INS_TRAFFIC_ALERT_FOR A_AH CRIM_GANG_RELATED_VIOLENCE IN_OR_ON_POSITION", this.spawnPosition);
                    }
                    else
                    {
                        this.CalloutMessage = string.Format(Functions.GetStringFromLanguageFile("CALLOUT_NOOSEMOD_TERRORIST_ACTIVITY_6"), Functions.GetAreaStringFromPosition(this.spawnPosition));
                        Functions.PlaySoundUsingPosition("THIS_IS_CONTROL INS_ATTENTION_ALL_UNITS_WE_HAVE CRIM_AN_SOS FOR ERR_ERRR_04 CRIM_ASSAULT_AND_BATTERY IN_OR_ON_POSITION", this.spawnPosition);
                    } break;
            }

            mission = MissionState.OnDuty;
		}
        
        /// <summary>
        /// Called when the callout has been accepted. Call base to set state to Running.
        /// </summary>
        /// <returns>
        /// True if callout was setup properly, false if it failed. Calls <see cref="End"/> when failed.
        /// </returns>
        public override bool OnCalloutAccepted()
        {
            base.OnCalloutAccepted();
            bool isReadyForMission = true;
            mission = MissionState.GoingToMissionLocation;
            // Set up terrorists, hostages, and entry point
            // then add extra squad to follow if you don't have a partner.
            // If all set up, return isReadyForMission value as true.

            // Clear resources first
            deadSuspects.Clear(); deadHostages.Clear(); hostages.Clear(); arrestedSuspects.Clear(); missionPeds.Clear(); squad.Clear(); PoliceOfficers.Clear();

            // Grab a check if using TBoGT weapons
            bool isUsingTbogtWeapons = SettingsIni.GetValueBool("EnableTBoGTWeapons", "GlobalSettings", false);

            // Grab a check if Biker Gangs are used in this callout
            bool isBikerGangUsedInCallout = SettingsIni.GetValueBool("SwapCriminalsToBikerGangMembers", "GlobalSettings", false);

            // Assign terrorists
            try
            {
                foreach (Vector3 suspectLocation in activeMission.SuspectLocations)
                {
                    string selectedModel = null;
                    if (isBikerGangUsedInCallout == false) { selectedModel = controller.LoadRandomRegisteredPeds(); }
                    else
                    {
                        switch (SettingsIni.GetValueString("GangType", "GlobalSettings", "AOD"))
                        {
                            case "AOD": selectedModel = controller.LoadAODBikerGangPeds(); break;
                            case "TLMC": selectedModel = controller.LoadTLMCBikerGangPeds(); break;
                        }
                    }

                    LPed suspectPed = new LPed(suspectLocation, selectedModel, LPed.EPedGroup.MissionPed);

                    // Randomize
                    int num = 1;
                    if (isUsingTbogtWeapons && Game.CurrentEpisode == GameEpisode.TBOGT)
                    {
                        num = Common.GetRandomValue(1, 6);
                    }
                    else { num = Common.GetRandomValue(1, 4); }
                    // Manage things like in original NooseMod does (with SHDN updates compared to the old one)
                    if (isBikerGangUsedInCallout == false)
                    {
                        suspectPed.GPed.RelationshipGroup = RelationshipGroup.Criminal;
                        suspectPed.GPed.ChangeRelationship(RelationshipGroup.Criminal, Relationship.Respect);
                        suspectPed.GPed.ChangeRelationship(RelationshipGroup.Cop, Relationship.Hate);
                    }
                    else
                    {
                        switch (SettingsIni.GetValueString("GangType", "GlobalSettings", "AOD"))
                        {
                            case "AOD":
                                suspectPed.GPed.RelationshipGroup = RelationshipGroup.Gang_Biker1;
                                suspectPed.GPed.ChangeRelationship(RelationshipGroup.Gang_Biker1, Relationship.Companion);
                                suspectPed.GPed.ChangeRelationship(RelationshipGroup.Cop, Relationship.Hate);
                                suspectPed.GPed.ChangeRelationship(RelationshipGroup.Civillian_Male, Relationship.Dislike);
                                suspectPed.GPed.ChangeRelationship(RelationshipGroup.Civillian_Female, Relationship.Dislike);
                                break;
                            case "TLMC":
                                suspectPed.GPed.RelationshipGroup = RelationshipGroup.Gang_Biker2;
                                suspectPed.GPed.ChangeRelationship(RelationshipGroup.Gang_Biker2, Relationship.Companion);
                                suspectPed.GPed.ChangeRelationship(RelationshipGroup.Cop, Relationship.Hate);
                                suspectPed.GPed.ChangeRelationship(RelationshipGroup.Civillian_Male, Relationship.Dislike);
                                suspectPed.GPed.ChangeRelationship(RelationshipGroup.Civillian_Female, Relationship.Dislike);
                                break;
                        }
                    }
                    suspectPed.Task.GuardCurrentPosition();
                    suspectPed.Enemy = true;
                    suspectPed.WantedByPolice = true;
                    suspectPed.AlwaysDiesOnLowHealth = true;
                    suspectPed.GPed.SetPathfinding(true, true, true);

                    // Register Weapons
                    suspectPed.Weapons.FromType(Weapon.Melee_Knife);
                    if (isBikerGangUsedInCallout == false && Game.CurrentEpisode != GameEpisode.TLAD)
                        // If not running TLAD and Biker Gang is not used, assign random weapon
                    {
                        suspectPed.Weapons.FromType(controller.AssignTerroristWeapon(9)).Ammo = 1;
                        suspectPed.Weapons.FromType(controller.AssignTerroristWeapon(num)).Ammo = 9999;
                        suspectPed.Weapons.Select(controller.AssignTerroristWeapon(num));
                    }
                    else if (isBikerGangUsedInCallout == true && Game.CurrentEpisode == GameEpisode.TLAD)
                        // Running TLAD with Biker Gang used adds ONLY ability to assign TLAD-specific weapons
                    {
                        int rand = Common.GetRandomValue(7, 8);
                        suspectPed.Weapons.FromType(Weapon.TLAD_PipeBomb).Ammo = 2 ^ 2;
                        // 1 by 6 chance a guy gets IV weapon
                        bool getIVWeapons = Common.GetRandomBool(0, 7, 1);
                        if (getIVWeapons)
                        {
                            suspectPed.Weapons.FromType(controller.AssignTerroristWeapon(num)).Ammo = 2 ^ 12;
                            suspectPed.Weapons.Select(controller.AssignTerroristWeapon(num));
                        }
                        else
                        {
                            suspectPed.Weapons.FromType(controller.AssignTerroristWeapon(rand)).Ammo = 2 ^ 10;
                            suspectPed.Weapons.Select(controller.AssignTerroristWeapon(rand));
                        }
                    }
                    else
                        // TBoGT, IV, and between used or not, the Biker Gang
                    {
                        suspectPed.Weapons.FromType(controller.AssignTerroristWeapon(9)).Ammo = 1;
                        suspectPed.Weapons.FromType(controller.AssignTerroristWeapon(num)).Ammo = 9999;
                        suspectPed.Weapons.Select(controller.AssignTerroristWeapon(num));
                    }

                    // Set difficulty
                    if (controller.HardcoreModeIsActive() == false)
                    {
                        difficulty = getDifficulty();
                    }
                    else { difficulty = Difficulty.Hard; }

                    if (difficulty == Difficulty.Easy) // Blipped and unarmored
                    {
                        Blip suspectBlip = suspectPed.AttachBlip();
                        suspectBlip.Color = BlipColor.Red;
                        //suspectPed.MaxHealth = 300;
                        suspectPed.Armor = 0;
                        //suspectBlip.SetMetadata<Blip>("blip",true,suspectBlip);
                    }
                    else if (difficulty == Difficulty.Hard) // Armored and 1.5x SHOOT accuracy
                    {
                        suspectPed.Armor = 100;
                        suspectPed.Accuracy = 150;
                    }
                    // Last but not least
                    Functions.AddToScriptDeletionList(suspectPed, this);
                    suspectPed.BlockPermanentEvents = true;
                    suspectPed.Task.AlwaysKeepTask = true; // this will be reset to change the task
                    missionPeds.Add(suspectPed);
                }

                foreach (Vector3 hostageLocation in activeMission.HostageLocations)
                {
                    // Standard ped cannot be removed from the callout; it is freed by the end of the callout instead
                    Ped hostage = World.CreatePed(hostageLocation);
                    hostage.AlwaysDiesOnLowHealth = true;
                    //hostage.Animation.Play(new AnimationSet("cop"), "armsup_loop", -1);
                    hostage.Task.HandsUp(-1);
                    hostage.CowerInsteadOfFleeing = true;
                    hostages.Add(hostage);
                }

                // Adds more action by adding additional squads in
                LHandle partMan = Functions.GetCurrentPartner();
                LPed[] partner = Functions.GetPartnerPeds(partMan);
                if (partner != null)
                {
                    LVehicle currentVeh = LPlayer.LocalPlayer.Ped.CurrentVehicle;
                    VehicleSeat seats = currentVeh.GetFreePassengerSeat();
                    if (seats == VehicleSeat.LeftRear)
                    {
                        SquadTroop = currentVeh.CreatePedOnSeat(VehicleSeat.LeftRear, SWATTrooper, RelationshipGroup.Cop);
                        SquadTroop.ChangeRelationship(RelationshipGroup.Cop, Relationship.Companion);
                        SquadTroop.ChangeRelationship(RelationshipGroup.Criminal, Relationship.Hate);
                        SquadTroop.ChangeRelationship(RelationshipGroup.Gang_Biker1, Relationship.Hate);
                        SquadTroop.ChangeRelationship(RelationshipGroup.Gang_Biker2, Relationship.Hate);
                        Functions.AddToScriptDeletionList(SquadTroop, this);
                        squad.Add(SquadTroop);
                    }
                    else if (seats == VehicleSeat.RightRear)
                    {
                        SquadTroop = currentVeh.CreatePedOnSeat(VehicleSeat.RightRear, SWATTrooper, RelationshipGroup.Cop);
                        SquadTroop.ChangeRelationship(RelationshipGroup.Cop, Relationship.Companion);
                        SquadTroop.ChangeRelationship(RelationshipGroup.Criminal, Relationship.Hate);
                        SquadTroop.ChangeRelationship(RelationshipGroup.Gang_Biker1, Relationship.Hate);
                        SquadTroop.ChangeRelationship(RelationshipGroup.Gang_Biker2, Relationship.Hate);
                        Functions.AddToScriptDeletionList(SquadTroop, this);
                        squad.Add(SquadTroop);
                    }
                    else if (seats == VehicleSeat.LeftRear && seats == VehicleSeat.RightRear)
                    {
                        SquadTroop1 = currentVeh.CreatePedOnSeat(VehicleSeat.LeftRear, SWATTrooper, RelationshipGroup.Cop);
                        SquadTroop2 = currentVeh.CreatePedOnSeat(VehicleSeat.RightRear, SWATTrooper, RelationshipGroup.Cop);
                        SquadTroop1.ChangeRelationship(RelationshipGroup.Cop, Relationship.Companion);
                        SquadTroop1.ChangeRelationship(RelationshipGroup.Criminal, Relationship.Hate);
                        SquadTroop1.ChangeRelationship(RelationshipGroup.Gang_Biker1, Relationship.Hate);
                        SquadTroop1.ChangeRelationship(RelationshipGroup.Gang_Biker2, Relationship.Hate);
                        SquadTroop2.ChangeRelationship(RelationshipGroup.Cop, Relationship.Companion);
                        SquadTroop2.ChangeRelationship(RelationshipGroup.Criminal, Relationship.Hate);
                        SquadTroop2.ChangeRelationship(RelationshipGroup.Gang_Biker1, Relationship.Hate);
                        SquadTroop2.ChangeRelationship(RelationshipGroup.Gang_Biker2, Relationship.Hate);
                        Functions.AddToScriptDeletionList(SquadTroop1, this);
                        Functions.AddToScriptDeletionList(SquadTroop2, this);
                        squad.Add(SquadTroop1);
                        squad.Add(SquadTroop2);
                    }
                    for (int i = 0; i <= partner.Length; i++)
                    {
                        squad.Add(partner[i]);
                    }
                }
                else // If no partner, a NOOSE cruiser will be added near you with SWATs inside and
                    // made tracks to the crime scene
                {
                    nooseVeh1 = new LVehicle(World.GetNextPositionOnStreet(LPlayer.LocalPlayer.Ped.Position), "NOOSE");
                    SquadTroops = new LPed[4] 
                    {
                        new LPed(nooseVeh1.Position, "M_Y_SWAT", LPed.EPedGroup.Cop),
                        new LPed(nooseVeh1.Position, "M_Y_SWAT", LPed.EPedGroup.Cop),
                        new LPed(nooseVeh1.Position, "M_Y_SWAT", LPed.EPedGroup.Cop),
                        new LPed(nooseVeh1.Position, "M_Y_SWAT", LPed.EPedGroup.Cop)
                    };
                    nooseVeh1.PlaceOnNextStreetProperly();
                    nooseVeh1.SirenActive = true;
                    nooseVeh1.AllowSirenWithoutDriver = true;
                    SquadTroops[0].WarpIntoVehicle(nooseVeh1, VehicleSeat.Driver);
                    SquadTroops[0].Task.DriveTo(spawnPosition, 2 ^ 8, false, true);
                    for (int i = 1; i <= SquadTroops.Length; i++)
                    {
                        SquadTroops[i].WarpIntoVehicle(nooseVeh1, VehicleSeat.AnyPassengerSeat);
                    }
                    for (int i = 0; i <= SquadTroops.Length; i++)
                    {
                        SquadTroops[i].RelationshipGroup = RelationshipGroup.Cop;
                        SquadTroops[i].ChangeRelationship(RelationshipGroup.Cop, Relationship.Respect);
                        SquadTroops[i].ChangeRelationship(RelationshipGroup.Criminal, Relationship.Hate);
                        SquadTroops[i].ChangeRelationship(RelationshipGroup.Gang_Biker1, Relationship.Hate);
                        SquadTroops[i].ChangeRelationship(RelationshipGroup.Gang_Biker2, Relationship.Hate);
                        SquadTroops[i].PriorityTargetForEnemies = true;
                        SquadTroops[i].Weapons.RemoveAll();
                        int x = Common.GetRandomValue(1, 2), y = Common.GetRandomValue(3, 5);
                        if (isUsingTbogtWeapons && Game.CurrentEpisode == GameEpisode.TBOGT)
                        {
                            y = Common.GetRandomValue(3, 9);
                        }
                        SquadTroops[i].Weapons.FromType(Weapon.Thrown_Grenade).Ammo = 1;
                        SquadTroops[i].Weapons.FromType(Weapon.Melee_Knife);
                        SquadTroops[i].Weapons.FromType(controller.AssignCounterTerroristWeapon(x)).Ammo = 999;
                        SquadTroops[i].Weapons.FromType(controller.AssignCounterTerroristWeapon(y)).Ammo = 999;
                        SquadTroops[i].Weapons.Select(controller.AssignCounterTerroristWeapon(y));
                        Functions.AddToScriptDeletionList(SquadTroops[i], this);
                        Functions.AddToScriptDeletionList(nooseVeh1, this);
                        // Register added squad into the Squads list
                        squad.Add(SquadTroops[i]);
                    }
                }
            }
            catch (Exception ex) { Log.Error("Error creating or assigning a ped: " + ex.Message, this); isReadyForMission = false; }
            if (isReadyForMission)
            {
                // Mission ready: register state callback and rock and roll!
                base.RegisterStateCallback(MissionState.GoingToMissionLocation, new Action(WaitingForPlayer));
                base.RegisterStateCallback(MissionState.WaitForTeamInsertion, new Action(DispatchTeam));
                base.RegisterStateCallback(MissionState.DuringMission, new Action(InitiateShootout));
                base.RegisterStateCallback(MissionState.Off, new Action(End));
                // OnDuty is not assigned
                Functions.PrintText(Functions.GetStringFromLanguageFile("CALLOUT_GET_TO_CRIME_SCENE"), 8000);

                // Print information from Dispatch if the crime scene contains hostages
                if(hostages.Count != 0)
                {
                Functions.AddTextToTextwall("Be advised, police reports " + hostages.Count + " hostages are in the area. "+
                    "Confirmed terrorist activity. Proceed with extreme prejudice.",
                    Functions.GetStringFromLanguageFile("POLICE_SCANNER_CONTROL"));
                }
            }
            base.State = mission;
            return isReadyForMission;
        }

        /// <summary>
        /// Called when the callout has not been accepted. Player will then resume whatever the task he is doing.
        /// Call base to set state to None.
        /// </summary>
        public override void OnCalloutNotAccepted()
        {
            base.OnCalloutNotAccepted();

            // Delete the entry location because we don't need it (and callout has not been accepted)
            controller.entryLoc.Delete();

            // Decrement loaded mission (to prevent loading the next mission)
            lastPlayedMission--;
        }

#region Actions
        // These actions are called from RegisterStateCallback to prevent method looping in the process.
        // Will be unused when a new MissionState is assigned.

        /// <summary>
        /// This method is used to wait for the Player until he reaches the crime scene.
        /// </summary>
        private void WaitingForPlayer()
        {
            Timer time = new Timer(0);
            Vector3 myPos = LPlayer.LocalPlayer.Ped.Position;
            int timeToReachCrimeScene = int.Parse((myPos.DistanceTo(this.spawnPosition).ToString())) + 5000;
            if (hostages.Count == 0)
            { timeToReachCrimeScene = int.Parse((myPos.DistanceTo(this.spawnPosition).ToString())) + 5000 * 2; }

            // Start timer, if one hostage killed (when exists), start another, keep repeating until the terrors break loose through the scene,
            //  failing the mission and reduces your cash for your incompetence.
            // From there, the callout ended with loss.
            // Mission without hostages will start with 2x time limit than normal, useful if you are having a hard time to reach target location.
            // The timer depends on the location you are responding, so that you will not be so hustle
            //  (1 minute is rather extreme if you're responding in Alderney while the target location is at Broker).
            time.Start();
            int hostage_kill_list = 0;
            int hostages_number_temp = hostages.Count;
            bool hostages_isAvailable = hostages.Count != 0;
            while (time.isRunning)
            {
                if (LPlayer.LocalPlayer.Ped.Position.DistanceTo2D(this.spawnPosition) < 50.0f)
                {
                    time.Stop();
                    break; // This is used when a player is getting close to the crime scene or mission failure.
                }
                else if (LPlayer.LocalPlayer.Ped.Position.DistanceTo2D(this.spawnPosition) >= 50.0f && time.ElapsedTime >= timeToReachCrimeScene)
                {
                    // Kill a hostage if any
                    if (hostages_isAvailable && hostage_kill_list != hostages_number_temp && hostages[hostage_kill_list].isAliveAndWell)
                    {
                        hostages[hostage_kill_list].Die();
                        //deadHostages.Add(hostages[hostage_kill_list]); // This is handled at Casualties Count
                        hostage_kill_list++;
                        timeToReachCrimeScene += 5000;
                        if (hostage_kill_list == 2 || hostage_kill_list > 2)
                        {
                            // If reported more than two hostages killed (when exists), radio it in
                            //this.CalloutMessage = Functions.GetStringFromLanguageFile("CALLOUT_NOOSEMOD_HOSTAGES_BEING_EXECUTED");
                            Functions.AddTextToTextwall(Functions.GetStringFromLanguageFile("CALLOUT_NOOSEMOD_HOSTAGES_BEING_EXECUTED"), Functions.GetStringFromLanguageFile("POLICE_SCANNER_CONTROL"));
                            Functions.PlaySoundUsingPosition("ATTENTION_ALL_UNITS INS_WEVE_GOT CRIM_MULTIPLE_INJURIES I_REPEAT CRIM_MULTIPLE_INJURIES IN_OR_ON_POSITION", this.spawnPosition);
                        }
                    }
                    else
                    {
                        LPlayer.LocalPlayer.Ped.Money -= 5000;
                        Functions.PrintText("~r~Mission Failed~n~~w~You didn't reach the target location on time. You have lost $5000 for your incompetence.", 25000);
                        time.Stop(); SetCalloutFinished(false, false, true); End(); mission = MissionState.Off;
                        break; // This is used when a player is getting close to the crime scene or mission failure.
                    }
                }
            }
            // Change enum state when outside the loop
            mission = MissionState.WaitForTeamInsertion;

            // Create ambient police force (roadblocks and cops with Carbine Rifle and Semi-Auto Shotgun)
            // This is defined through the Safe Mode in the INI file
            bool safeMode = SettingsIni.GetValueBool("SafeMode", "GlobalSettings", false);
            if (safeMode == false)
            {
                LVehicle copCarBlock1 = new LVehicle(World.GetPositionAround(spawnPosition,6.5f), "POLICE");
                copCarBlock1.PlaceOnGroundProperly();
                copCarBlock1.SirenActive = true;
                copCarBlock1.EngineRunning = true;
                Functions.AddToScriptDeletionList(copCarBlock1, this);
                LVehicle copCarBlock2 = new LVehicle(World.GetPositionAround(spawnPosition, 5.3f), "POLICE");
                copCarBlock2.PlaceOnGroundProperly();
                copCarBlock2.SirenActive = true;
                copCarBlock2.EngineRunning = true;
                Functions.AddToScriptDeletionList(copCarBlock2, this);
                for (int i = 0; i <= 6; i++)
                {
                    PoliceOfficers.Add(new LPed(copCarBlock1.GetOffsetPosition(spawnPosition), "M_Y_COP"));
                    PoliceOfficers[i].Weapons.RemoveAll();
                    PoliceOfficers[i].Weapons.FromType(Weapon.Handgun_DesertEagle).Ammo = 999;
                    PoliceOfficers[i].Weapons.FromType(Weapon.Rifle_M4).Ammo = 300;
                    PoliceOfficers[i].Weapons.Select(Weapon.Rifle_M4);
                    PoliceOfficers[i].Health = 500;
                    PoliceOfficers[i].RelationshipGroup = RelationshipGroup.Cop;
                    PoliceOfficers[i].ChangeRelationship(RelationshipGroup.Cop, Relationship.Companion);
                    PoliceOfficers[i].ChangeRelationship(RelationshipGroup.Criminal, Relationship.Hate);
                    PoliceOfficers[i].ChangeRelationship(RelationshipGroup.Gang_Biker1, Relationship.Hate);
                    PoliceOfficers[i].ChangeRelationship(RelationshipGroup.Gang_Biker2, Relationship.Hate);
                    Functions.AddToScriptDeletionList(PoliceOfficers[i], this);
                }
                for (int i = 7; i <= 12; i++)
                {
                    PoliceOfficers.Add(new LPed(copCarBlock2.Position.Around(3.5f), "M_Y_COP"));
                    PoliceOfficers[i].Weapons.RemoveAll();
                    PoliceOfficers[i].Weapons.FromType(Weapon.Handgun_DesertEagle).Ammo = 999;
                    PoliceOfficers[i].Weapons.FromType(Weapon.Shotgun_Baretta).Ammo = 40;
                    PoliceOfficers[i].Weapons.Select(Weapon.Shotgun_Baretta);
                    PoliceOfficers[i].Health = 500;
                    PoliceOfficers[i].RelationshipGroup = RelationshipGroup.Cop;
                    PoliceOfficers[i].ChangeRelationship(RelationshipGroup.Cop, Relationship.Companion);
                    PoliceOfficers[i].ChangeRelationship(RelationshipGroup.Criminal, Relationship.Hate);
                    PoliceOfficers[i].ChangeRelationship(RelationshipGroup.Gang_Biker1, Relationship.Hate);
                    PoliceOfficers[i].ChangeRelationship(RelationshipGroup.Gang_Biker2, Relationship.Hate);
                    Functions.AddToScriptDeletionList(PoliceOfficers[i], this);
                }
            }
            base.State = mission;
        }

        /// <summary>
        /// When <see cref="MissionState"/> is <see cref="MissionState.WaitForTeamInsertion"/>,
        /// a NOOSE squad and a police backup are dispatched to the target location. After all set, the shootout can begin.
        /// </summary>
        private void DispatchTeam()
        {
            // If last mission is confirmed, play Epilogue music
            int finalMission = controller.loadedMissions.Count;
            finalMission--;
            if (lastPlayedMission == finalMission)
            {
                this.PlaySound(true);
            }

            Functions.AddTextToTextwall(string.Format(Functions.GetStringFromLanguageFile("CALLOUT_NOOSEMOD_PLAYER_IN_POS"), Functions.GetAreaStringFromPosition(this.spawnPosition)), LPlayer.LocalPlayer.Name);

            // When arrived in time, dispatch NOOSE Squad in with additional police force
            // Breach in when in position
            nooseVeh2 = new LVehicle(World.GetPositionAround(this.spawnPosition, 3 ^ 5), "NSTOCKADE");
            LPed nooseDisp1 = nooseVeh2.CreatePedOnSeat(VehicleSeat.Driver, SWATTrooper, RelationshipGroup.Cop);
            LPed nooseDisp2 = nooseVeh2.CreatePedOnSeat(VehicleSeat.AnyPassengerSeat, SWATTrooper, RelationshipGroup.Cop);
            LPed nooseDisp3 = nooseVeh2.CreatePedOnSeat(VehicleSeat.AnyPassengerSeat, SWATTrooper, RelationshipGroup.Cop);
            LPed nooseDisp4 = nooseVeh2.CreatePedOnSeat(VehicleSeat.AnyPassengerSeat, SWATTrooper, RelationshipGroup.Cop);
            nooseDisp1.Task.DriveTo(this.spawnPosition, 2 ^ 8, false, true);
            SquadTroops = new LPed[4] { nooseDisp1, nooseDisp2, nooseDisp3, nooseDisp4 };
            for (int i = 0; i <= SquadTroops.Length; i++)
            {
                SquadTroops[i].Weapons.RemoveAll();
                SquadTroops[i].Weapons.FromType(Weapon.Melee_Knife);
                SquadTroops[i].Weapons.FromType(Weapon.Handgun_Glock).Ammo = 5 ^ 5;
                SquadTroops[i].Weapons.FromType(Weapon.SMG_Uzi).Ammo = 5 ^ 3;
                bool getSniperInsteadOfM4 = Common.GetRandomBool(0, 9, 1);
                if (getSniperInsteadOfM4)
                {
                    SquadTroops[i].Weapons.FromType(Weapon.SniperRifle_Basic).Ammo = 5 ^ 3;
                    SquadTroops[i].Weapons.Select(Weapon.SniperRifle_Basic);
                }
                else
                {
                    SquadTroops[i].Weapons.FromType(Weapon.Rifle_M4).Ammo = 6 ^ 5;
                    SquadTroops[i].Weapons.Select(Weapon.Rifle_M4);
                }
                squad.Add(SquadTroops[i]);
                Functions.AddToScriptDeletionList(SquadTroops[i], this);
            }
            nooseVeh2.AllowSirenWithoutDriver = true;
            Blip nooseVeh2PosNow = nooseVeh2.AttachBlip();
            nooseVeh2PosNow.Color = BlipColor.Cyan;
            nooseVeh2PosNow.Friendly = true;
            nooseVeh2PosNow.Name = "Squad Group 2";
            nooseVeh2.SirenActive = true;
            nooseVeh2.PlaceOnNextStreetProperly();
            Functions.RequestPoliceBackupAtPosition(this.spawnPosition);
            Functions.PlaySoundUsingPosition("DFROM_DISPATCH_A_NOOSE_TEAM_FROM IN_OR_ON_POSITION FOR CRIM_TERRORIST_ACTIVITY", nooseVeh2.Position);
            Functions.PrintText("CALLOUT_NOOSEMOD_STANDBY", 25000);
            Functions.AddToScriptDeletionList(nooseVeh2, this);

            while (nooseDisp1.Position.DistanceTo2D(this.spawnPosition) > 10.0f)
            {
                if (nooseDisp1.Position.DistanceTo2D(this.spawnPosition) <= 10.0f)
                {
                    nooseVeh2PosNow.Delete();
                    break;
                }
            }

            for (int i = 0; i < squad.Count; i++)
            {
                if (squad[i].IsInVehicle() && squad[i].Position.DistanceTo2D(this.spawnPosition) <= 30.9f)
                {
                    squad[i].LeaveVehicle();
                }
            }
            mission = MissionState.DuringMission;
            base.State = mission;
        }

        /// <summary>
        /// Here, the real action begins.
        /// </summary>
        private void InitiateShootout()
        {
            // Build Pursuit instance
            this.pursuit = Functions.CreatePursuit();
            Functions.SetPursuitCalledIn(this.pursuit, true);
            Functions.SetPursuitIsActiveForPlayer(this.pursuit, true);
            Functions.SetPursuitAllowWeaponsForSuspects(this.pursuit, true);
            Functions.SetPursuitAllowVehiclesForSuspects(this.pursuit, false);
            Functions.SetPursuitForceSuspectsToFight(this.pursuit, true);
            controller.entryLoc.Delete();
            foreach (LPed myped in missionPeds)
            {
                Functions.AddPedToPursuit(this.pursuit, myped);
                myped.DisablePursuitAI = true;
            }
            //CALLOUT_NOOSEMOD_FIGHT_SUSPECTS
            Functions.PrintText(Functions.GetStringFromLanguageFile("CALLOUT_NOOSEMOD_FIGHT_SUSPECTS"), 25000);
        }

        /// <summary>
        /// Action for the remaining terrorists to flee the crime scene because being outnumbered by cops and NOOSE.
        /// </summary>
        private bool CombatOrders_RemainingTerrorists()
        {
            // Start pursuit when at least 2 or 3 terrors left and is near the entrypoint
            // Conduct shootout with cops and steal a cop car if they have no choice
            Functions.PrintText(Functions.GetStringFromLanguageFile("CALLOUT_NOOSEMOD_FLEEING_TERRORISTS"), 8000);
            LPlayer.LocalPlayer.Ped.SayAmbientSpeech("CALL_IN_PURSUIT_01");
            Functions.PlaySoundUsingPosition("THIS_IS_CONTROL ASSISTANCE_REQUIRED FOR CRIM_A_SUSPECT_RESISTING_ARREST IN_OR_ON_POSITION", this.spawnPosition);

            // Allow suspects to flee in a car
            Functions.SetPursuitAllowVehiclesForSuspects(this.pursuit, true);
            Functions.SetPursuitForceSuspectsToFight(this.pursuit, false);

            bool isRemainingTerroristsFled = true;

            foreach (LPed myped in missionPeds)
            {
                if (myped.IsAlive && myped.Exists())
                {
                    // Reset the task
                    myped.Task.AlwaysKeepTask = false;
                    myped.BlockGestures = false;
                    myped.BlockPermanentEvents = false;
                    myped.Task.ClearAllImmediately();
                    if (difficulty == Difficulty.Hard || difficulty == Difficulty.Medium)
                    {
                        // Make it blip again with custom name
                        Blip activePedBlip = myped.AttachBlip();
                        activePedBlip.Color = BlipColor.Red;
                        activePedBlip.Name = "Fleeing Terrorists";
                    }

                    // Escape by any means necessary
                    foreach (Ped officers in World.GetAllPeds(new Model("M_Y_COP")))
                    {
                        myped.Task.FleeFromChar(officers);
                        myped.Task.FightAgainst(officers, -1);
                    }
                    foreach (Ped squads in World.GetAllPeds(new Model("M_Y_SWAT")))
                    {
                        myped.Task.FleeFromChar(squads);
                        myped.Task.FightAgainst(squads, -1);
                    }

                    // Enable Pursuit AI
                    myped.DisablePursuitAI = false;

                    /*
                    // Enter any vehicle it founds (alternative version)
                    Vehicle[] availableCars = World.GetAllVehicles();
                    int randomVeh = Common.GetRandomValue(0, availableCars.Length);

                    // A random chance terrorist may hijack one another, this is because
                    // they wanted to escape from the cops (alternative version)
                    myped.Task.EnterVehicle(availableCars[randomVeh], VehicleSeat.Driver);
                    myped.Task.CruiseWithVehicle(availableCars[randomVeh], 2 ^ 8, false);
                     */

                    //Functions.SetPursuitAllowWeaponsForSuspects(this.pursuit, true);
                    myped.Task.AlwaysKeepTask = true;
                    myped.BlockPermanentEvents = true;
                    isRemainingTerroristsFled = true;
                }
                //else { isRemainingTerroristsFled = false; }
            }
            return isRemainingTerroristsFled;
        }

        /// <summary>
        /// Action to delay remaining and alive terrorists to flee the scene before <see cref="CombatOrders_RemainingTerrorists"/> is issued.
        /// </summary>
        /// <param name="number_of_terrorists">Remaining terrorists that are alive</param>
        private void HoldUntilTerrorists_Left(int number_of_terrorists)
        {
            suspectsLeft = missionPeds.Count;
            foreach (LPed myped in missionPeds)
            {
                if (myped.IsDead && myped.Exists())
                {
                    missionPeds.Remove(myped);
                    deadSuspects.Add(myped);
                }
            }
            if (suspectsLeft == number_of_terrorists || suspectsLeft <= number_of_terrorists)
            {
                isPursuitSessionActive = CombatOrders_RemainingTerrorists();
            }
        }

        /// <summary>
        /// Casualties number
        /// </summary>
        private int officers_killed, squad_killed;

        /// <summary>
        /// Gets the number of casualties for Officers and Squad
        /// </summary>
        private void GetCasualties()
        {
            // Store values first
            officers_killed = 0;
            squad_killed = 0;

            foreach (LPed officer in PoliceOfficers)
            {
                if (officer.IsDead || officer.IsInjured && officer.Exists())
                {
                    PoliceOfficers.Remove(officer);
                    officer.NoLongerNeeded();
                    officers_killed++;
                }
            }

            foreach (LPed squad_onduty in squad)
            {
                if (squad_onduty.IsDead || squad_onduty.IsInjured && squad_onduty.Exists())
                {
                    squad.Remove(squad_onduty);
                    squad_onduty.NoLongerNeeded();
                    squad_killed++;
                }
            }

            // Count on how many hostages need to be evacuated (or at least not being shot at to death)
            // This affects your arrival (hostages being executed if NOOSE arrives late)
            if (hostages.Count != 0)
            {
                foreach (Ped hostage in hostages)
                {
                    if (hostage.isDead && hostage.Exists())
                    {
                        hostages.Remove(hostage);
                        deadHostages.Add(hostage);
                    }
                }
            }
        }

        /// <summary>
        /// Gets the number of arrested terrorists when <see cref="isPursuitSessionActive"/> returns true in the process.
        /// </summary>
        private void GetArrestedTerrorists()
        {
            if (isPursuitSessionActive && missionPeds.Count != 0)
            {
                foreach (LPed myped in missionPeds)
                {
                    if (myped.IsAlive && myped.HasBeenArrested && myped.Exists())
                    {
                        missionPeds.Remove(myped);
                        arrestedSuspects.Add(myped);
                    }
                        // whether IsDead return true and HasBeenArrested return false
                    else
                        if (myped.IsDead && !myped.HasBeenArrested && myped.Exists())
                    { missionPeds.Remove(myped); deadSuspects.Add(myped); }
                }
            }
        }
#endregion

        //private void NooseMod_Tick(object sender, EventArgs e)
        /// <summary>
        /// Called every tick to process all script logic. Call base when overriding.
        /// </summary>
        public override void Process()
        {
            base.Process();
            // Count on how many terrors left to be finished
            do
            {
                // Should between 1 and 3 because Intro only include 4 terrorists and 3 hostages
                HoldUntilTerrorists_Left(Common.GetRandomValue(1, 3));
            } while (isPursuitSessionActive == false);

            GetArrestedTerrorists();

            // If no more terrorists are active (arrested or killed in the process), end the callout
            if (missionPeds.Count == 0)
            {
                // Get casualties on officers and squad
                GetCasualties();

                // Sum cash gained
                int cashGained = (deadSuspects.Count * MoneyForEachDeadSuspect) + (arrestedSuspects.Count * MoneyForEachArrestedSuspect) +
                    (hostages.Count * RewardForRescuedHostages) - (deadHostages.Count * PenaltyForDeadHostage) - (officers_killed * PenaltyForOfficerCasualties) -
                    (squad_killed * PenaltyForSquadCasualties);

                // Inform player on screen
                Functions.PrintText("Mission complete! Terrorists killed: " + deadSuspects.Count + ", Terrorists arrested: " + arrestedSuspects.Count +
                    " Hostages killed: " + deadHostages.Count + ", Hostages rescued: " + hostages.Count + ", Officer Casualties: " + officers_killed +
                    ", Squad Casualties: " + squad_killed + ", Income for your noble service: $" + cashGained, 30000);

                // Add cash
                LPlayer.LocalPlayer.Money += cashGained;

                // Write stats
                WriteStatistics(deadSuspects.Count, arrestedSuspects.Count, deadHostages.Count, hostages.Count, officers_killed, squad_killed, cashGained);

                // Save the progress
                SaveGame();

                // End everything
                PlaySound(false);
                PlayFinishedMissionSound();
                SetCalloutFinished(true, false, false);
                End();
            }

            // End this script is pursuit is no longer running, e.g. because all suspects are dead
            if (!Functions.IsPursuitStillRunning(this.pursuit))
            {
                // If situation permits
                PlaySound(false);
                PlayFinishedMissionSound();
                this.SetCalloutFinished(true, false, true);
                this.End();
            }
		}

        /// <summary>
        /// Put all resource free logic here. This is either called by the calloutmanager to shutdown the callout or can be called by the 
        /// callout itself to execute the cleanup code. Call base to set state to None.
        /// </summary>
        public override void End()
        {
            base.End();

            // Delete hostages count (this is because the hostage is assigned without using LPed)
            if (hostages.Count != 0)
            {
                for (int i = 0; i < hostages.Count; i++)
                {
                    hostages[i].NoLongerNeeded();
                }
            }

            if (deadHostages.Count != 0)
            {
                for (int i = 0; i < deadHostages.Count; i++)
                {
                    deadHostages[i].NoLongerNeeded();
                }
            }

            // Clear list
            deadSuspects.Clear(); deadHostages.Clear(); hostages.Clear(); arrestedSuspects.Clear(); missionPeds.Clear(); squad.Clear(); PoliceOfficers.Clear();

            // Reset enum
            mission = MissionState.OnDuty;

            // End pursuit if still running
            if (this.pursuit != null)
            {
                Functions.ForceEndPursuit(this.pursuit);
            }
        }

        /// <summary>
        /// Called when a ped assigned to the current script has left the script due to a more important action, such as being arrested by the player.
        /// This is invoked right before control is granted to the new script, so perform all necessary freeing actions right here.
        /// </summary>
        /// <param name="ped">The ped</param>
        public override void PedLeftScript(LPed ped)
        {
            base.PedLeftScript(ped);

            // Free ped
            Functions.RemoveFromDeletionList(ped, this);
            Functions.SetPedIsOwnedByScript(ped, this, false);
        }
	}
}
