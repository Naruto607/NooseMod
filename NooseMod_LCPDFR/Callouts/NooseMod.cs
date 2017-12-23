//    NooseMod LCPDFR Plugin with Database System
//    NooseMod Main Callout: the Main script
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
//using Microsoft.DirectX.AudioVideoPlayback;
using NooseMod_LCPDFR.Mission_Controller;
using NooseMod_LCPDFR.Properties;
using NooseMod_LCPDFR.StatsDataSetTableAdapters;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.IO;
//using System.Media;
using System.Text;
#endregion

// Proposed plans:
// - Action syntax (thanks to LtFlash)
// - LCPDFR Internal Coding Bugfix and Bypass (to prevent script crashes)

namespace NooseMod_LCPDFR.Callouts
{
    /// <summary>
    /// The Main Callout Script
    /// </summary>
    //[CalloutInfo("Terrorist Activity", ECalloutProbability.VeryLow)]
    [CalloutInfo("NooseMod", ECalloutProbability.VeryLow)]
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
        /// Table Adapter for <see cref="StatsDataSet"/> representing Mission Stats table
        /// </summary>
        internal MissionStatsTableAdapter missionstatsadp = new MissionStatsTableAdapter();

        /// <summary>
        /// If terrorists fled, this determines whether the session turns to cop chase
        /// </summary>
        private bool isPursuitSessionActive = false;

        /// <summary>
        /// Indicates the player's last room so that it can be tracked
        /// </summary>
        private Room lastPlayerRoom;
        #endregion

        // Kills and Arrests
        #region Kills and Arrests
        /// <summary>
        /// List of dead suspects
        /// </summary>
        private List<LPed> deadSuspects = new List<LPed>();

        /// <summary>
        /// List of arrested suspects
        /// </summary>
        private List<LPed> arrestedSuspects = new List<LPed>();

        /// <summary>
        /// List of NOOSE Squad
        /// </summary>
        private List<LPed> squad = new List<LPed>();

        /// <summary>
        /// List of hostages
        /// </summary>
        private List<Ped> hostages = new List<Ped>();

        /// <summary>
        /// List of dead hostages
        /// </summary>
        private List<Ped> deadHostages = new List<Ped>();

        /// <summary>
        /// List of Mission Peds
        /// </summary>
        private List<LPed> missionPeds = new List<LPed>();

        /// <summary>
        /// Counter on how many suspects (terrorists) left
        /// </summary>
        private int suspectsLeft;
        #endregion

        // File Reader
        #region Audio Reader

        /// <summary>
        /// Audio Reader (used in old NooseMod)
        /// </summary>
        //private Audio myAudio1 = new Audio("LCPDFR\\Plugins\\NooseMod\\sounds\\epilogue.wma"); // reads the WMA at start than the old one

        /// <summary>
        /// Audio Reader for Wave Files
        /// </summary>
        //private SoundPlayer myAudio2 = new SoundPlayer("LCPDFR\\Plugins\\NooseMod\\sounds\\MP.wav"); // reads the WAV file (alternate LCPDFR end callout sound, thanks to Sam)
        #endregion

        // LCPDFR variables
        #region LCPDFR Variables
        /// <summary>
        /// The spawn position
        /// </summary>
        private Vector3 spawnPosition;

        /// <summary>
        /// Pursuit handle that is a part of the LCPDFR feature
        /// </summary>
        private LHandle pursuit;
        #endregion

        // Control for backup vehicles and peds
        #region Backup Vehicles and Peds Control
        /// <summary>
        /// The NOOSE vehicle
        /// </summary>
        private LVehicle nooseVeh1, nooseVeh2, nooseVeh3;

        /// <summary>
        /// Array of NOOSE Squads
        /// </summary>
        private LPed[] SquadTroops;

        /// <summary>
        /// Squad to spawn in
        /// </summary>
        private LPed SquadTroop, SquadTroop1, SquadTroop2;

        /// <summary>
        /// Model Information of NOOSE/SWAT trooper using LCPDFR basis
        /// </summary>
        private CModel SWATTrooper = new CModel(new Model("M_Y_SWAT"));

        //private CModel SWATTrooper = new CModel(new CModelInfo("M_Y_SWAT", 3290204350, EModelFlags.IsNoose));
        //private CModel SWATTrooper = new CModel(3290204350);
        // Using CModelInfo - look for model name and its hash here: https://www.gtamodding.com/wiki/List_of_models_hashes

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

        /// <summary>
        /// NOOSE Vehicle Model Names
        /// </summary>
        internal string[] nooseVehicleModels = { "NOOSE", "POLPATRIOT", "NSTOCKADE" };

        /*
        /// <summary>
        /// Script Stage
        /// </summary>
        Action stage;
         */ // Might as well try to consider suggestions from LtFlash, that means doing big-roundup with the code...
        #endregion

        #region LCPDFR External Procedures

        /*/// <summary>
        /// Plays Mission Complete sound when the threat is neutralized
        /// </summary>
        private void PlayFinishedMissionSound()
        {
            try
            {
                if (myAudio2.SoundLocation != null || myAudio2 != null) myAudio2.Load();
                else
                {
                    myAudio2.SoundLocation = "LCPDFR\\Plugins\\NooseMod\\sounds\\MP.wav";
                    myAudio2.Load();
                }
                Game.WaitInCurrentScript(300); // Hold 0.3 seconds
                myAudio2.Play();
            }
            catch (Exception ex) { Log.Error("Error in PlayFinishedMissionSound(): " + ex, this); }
        }*/

        /// <summary>
        /// Writes the current progress into the database.
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
            // Attempt to open
            try
            {
                missionstatsadp.Connection.Open();
            }
            catch (OleDbException ex)
            {
                Log.Error("Connection cannot be opened: " + ex, this); return;
            }
            catch (InvalidOperationException ex)
            {
                Log.Error("Invalid Operation: " + ex, this); return;
            }
            catch (Exception ex) // when other errors caught
            {
                Log.Error("Detected other error: " + ex, this); return;
            }

            StatsDataSet.MissionStatsDataTable missionstatsdt = missionstatsadp.GetData();

            // Write stats
            Log.Info("Writing Statistics...", this);
            try
            {
                // Because the value starts from 0 when loaded (list array function), it'll be incremented by 1
                missionstatsdt.FindByMission_Number(this.lastPlayedMission + 1).Suspects_Killed = KS;
                missionstatsdt.FindByMission_Number(this.lastPlayedMission + 1).Suspects_Arrested = AS;
                missionstatsdt.FindByMission_Number(this.lastPlayedMission + 1).Hostages_Killed = KH;
                missionstatsdt.FindByMission_Number(this.lastPlayedMission + 1).Hostages_Rescued = RH;
                missionstatsdt.FindByMission_Number(this.lastPlayedMission + 1).Officer_Casualties = OCst;
                missionstatsdt.FindByMission_Number(this.lastPlayedMission + 1).Squad_Casualties = SQCst;
                missionstatsdt.FindByMission_Number(this.lastPlayedMission + 1).Income = cash;
                missionstatsdt.AcceptChanges();

                int fin = missionstatsadp.Update(missionstatsdt);
                Log.Info("Write Successful with return value " + fin, this);
            }
            catch (Exception ex) { Log.Error("Error writing stats to database: " + ex, this); missionstatsdt.RejectChanges(); }

            // Close when done
            if (missionstatsadp.Connection.State == ConnectionState.Open) try { missionstatsadp.Connection.Close(); }
                catch (Exception ex) { Log.Error("Cannot close the table adapter: " + ex, this); }
        }

        /*/// <summary>
        /// Plays the external audio in the game
        /// </summary>
        /// <param name="play">Whether or not the sound will play</param>
        private void PlaySound(bool play)
        {
            if (play) try
                {
                    if (myAudio1 != null)
                    {
                        myAudio1.Ending += myAudio1_Ending;
                        myAudio1.Play();
                    }
                    else
                    {
                        myAudio1 = Audio.FromFile("LCPDFR\\Plugins\\NooseMod\\sounds\\epilogue.wma");
                        myAudio1.Ending += myAudio1_Ending;
                        myAudio1.Play();
                    }
                }
                catch (Exception ex) { Log.Error("Error in function PlaySound(bool play): " + ex, this); }
            else try { myAudio1.Stop(); }
                catch (Exception ex) { Log.Error("Error in function PlaySound(bool play): " + ex, this); }
        }

        /// <summary>
        /// When the audio reaches the end, this signals the script to reset the position to beginning
        /// (where there contains limitation on NAudio itself) and then replay.
        /// </summary>
        /// <param name="sender">Object that is sending</param>
        /// <param name="e">Event</param>
        private void myAudio1_Ending(object sender, EventArgs e)
        {
            myAudio1.SeekCurrentPosition((int)0, SeekPositionFlags.AbsolutePositioning);
            myAudio1.Play();
        }*/

        /// <summary>
        /// Gets the difficulty based on the settings you choose
        /// </summary>
        /// <returns><see cref="Difficulty"/> of a mission</returns>
        private Difficulty getDifficulty()
        {
            int value = SettingsIni.GetValueInteger("Difficulty", "GlobalSettings", 2);
            Difficulty fixedDiff;
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
            // Write the value into the text file
            // Directory is relative to the game path where ScriptHookDotNet and LCPDFR are installed
            try
            {
                StreamWriter streamWriter = new StreamWriter("LCPDFR\\Plugins\\NooseMod\\save.txt");
                streamWriter.WriteLine(this.lastPlayedMission.ToString());
                streamWriter.Close();
            }
            catch (Exception ex) { Log.Error("Unable to read or write save.txt: " + ex, this); }
        }
        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="NooseMod"/> class.
        /// </summary>
		public NooseMod()
        {
            // Load last saved game
            lastPlayedMission = controller.LoadGame();
            controller.GetMissionData_NoLog();

            // increment the current value (if -1, it will load the list zero)
            lastPlayedMission++;

            // Time check, if a mission is called on a specified time
            try
            {
                Mission currentMissionToPlayNow = controller.missionVariableCheckup(lastPlayedMission);
                Log.Debug("Initializing mission with the name: " + currentMissionToPlayNow.Name, this);

                // Get Mission Time
                TimeSpan currentTime = World.CurrentDayTime;
                bool isExceedingMissionList = lastPlayedMission >= controller.loadedMissions.Count;
                if (isExceedingMissionList == false)
                {
                    if (currentMissionToPlayNow.IsMissionTimeSpecified)
                    {
                        TimeSpan interval1 = currentMissionToPlayNow.MissionTime - TimeSpan.FromHours(1);
                        TimeSpan interval2 = currentMissionToPlayNow.MissionTime + TimeSpan.FromHours(1);
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
                            Log.Debug("Mission started between " + interval1 + " and " + interval2, this);
                            activeMission = controller.PlayMission(lastPlayedMission);
                        }

                        // Check if the mission starts at 00:00, as from 23:59 it will reset to 00:00
                        else if (currentTime >= interval1 && currentTime <= interval3 && currentTime >= new TimeSpan(0, 0, 0) && currentTime <= interval2)
                        {
                            Log.Debug("Mission started between " + interval1 + " and " + interval2, this);
                            activeMission = controller.PlayMission(lastPlayedMission);
                        }

                        // No resources loaded, so if not within specified time, end it from the call base
                        // and start another callout instead
                        else
                        {
                            Log.Debug("Mission ended: " + currentTime + " is not in specified interval", this);
                            base.End();
                            Functions.StartCallout("NooseModPursuit");
                            return;
                        }
                    }

                    // Start immediately the mission if the mission time is not specified
                    else
                    {
                        Log.Debug("Mission started without time check", this);
                        activeMission = controller.PlayMission(lastPlayedMission);
                    }
                }

                // End the callout from the base if the save file is exceeding the mission list
                // and start another callout instead
                else
                {
                    Log.Debug("Mission ended: " + lastPlayedMission + " exceeds the list of " + controller.loadedMissions.Count + " missions", this);
                    base.End();
                    Functions.StartCallout("NooseModPursuit");
                    return;
                }

                // Use Mission Location to start
                spawnPosition = controller.entryLoc.Position;
            }
            catch (Exception ex)
            {
                Log.Error("Initialization error, lastPlayedMission is: " + lastPlayedMission + " : " + ex, this);
                base.End();
                return;
            }

            // Register location
            ShowCalloutAreaBlipBeforeAccepting(spawnPosition, 30.0f);
            AddMinimumDistanceCheck((float)1000, spawnPosition);

            // Call it in
            int i = Common.GetRandomValue(0, 6);
            Log.Debug("Loaded audio and text ID " + i, this);
            switch (i)
            {
                case 0:
                    //this.CalloutMessage = string.Format(Functions.GetStringFromLanguageFile("CALLOUT_NOOSEMOD_TERRORIST_ACTIVITY"), Functions.GetAreaStringFromPosition(this.spawnPosition));
                    this.CalloutMessage = string.Format(Resources.CALLOUT_NOOSEMOD_TERRORIST_ACTIVITY, Functions.GetAreaStringFromPosition(this.spawnPosition));
                    Functions.PlaySoundUsingPosition("THIS_IS_CONTROL ATTENTION_ALL_UNITS INS_WE_HAVE_A_REPORT_OF_ERRR CRIM_TERRORIST_ACTIVITY IN_OR_ON_POSITION", this.spawnPosition);
                    break;
                case 1:
                    //this.CalloutMessage = string.Format(Functions.GetStringFromLanguageFile("CALLOUT_NOOSEMOD_TERRORIST_ACTIVITY_2"), Functions.GetAreaStringFromPosition(this.spawnPosition));
                    this.CalloutMessage = string.Format(Resources.CALLOUT_NOOSEMOD_TERRORIST_ACTIVITY_2, Functions.GetAreaStringFromPosition(this.spawnPosition));
                    Functions.PlaySoundUsingPosition("UNITS_PLEASE_BE_ADVISED INS_WE_HAVE A_LONGER INS_TRAFFIC_ALERT_FOR CRIM_A_CRIMINAL_ON_A_GUN_SPREE IN_OR_ON_POSITION", this.spawnPosition);
                    break;
                case 2:
                    //this.CalloutMessage = string.Format(Functions.GetStringFromLanguageFile("CALLOUT_NOOSEMOD_TERRORIST_ACTIVITY_3"), Functions.GetAreaStringFromPosition(this.spawnPosition));
                    this.CalloutMessage = string.Format(Resources.CALLOUT_NOOSEMOD_TERRORIST_ACTIVITY_3, Functions.GetAreaStringFromPosition(this.spawnPosition));
                    Functions.PlaySoundUsingPosition("ALL_UNITS_ALL_UNITS INS_WE_HAVE CRIM_AN_SOS FOR CRIM_AN_OFFICER_IN_NEED_OF_ASSISTANCE IN_OR_ON_POSITION", this.spawnPosition);
                    break;
                case 3:
                    //this.CalloutMessage = string.Format(Functions.GetStringFromLanguageFile("CALLOUT_NOOSEMOD_TERRORIST_ACTIVITY_4"), Functions.GetAreaStringFromPosition(this.spawnPosition));
                    this.CalloutMessage = string.Format(Resources.CALLOUT_NOOSEMOD_TERRORIST_ACTIVITY_4, Functions.GetAreaStringFromPosition(this.spawnPosition));
                    Functions.PlaySoundUsingPosition("THIS_IS_CONTROL INS_AVAILABLE_UNITS_RESPOND_TO ERR_ERRR_14 CRIM_POSSIBLE_TERRORIST_ACTIVITY IN_OR_ON_POSITION", this.spawnPosition);
                    break;
                case 4:
                    bool isBikerGangUsedInCallout = SettingsIni.GetValueBool("SwapCriminalsToBikerGangMembers", "GlobalSettings", false);
                    if (isBikerGangUsedInCallout == true)
                    {
                        //this.CalloutMessage = string.Format(Functions.GetStringFromLanguageFile("CALLOUT_NOOSEMOD_TERRORIST_ACTIVITY_5"), Functions.GetAreaStringFromPosition(this.spawnPosition));
                        this.CalloutMessage = string.Format(Resources.CALLOUT_NOOSEMOD_TERRORIST_ACTIVITY_5, Functions.GetAreaStringFromPosition(this.spawnPosition));
                        Functions.PlaySoundUsingPosition("ALL_UNITS INS_WEVE_GOT ERR_ERRR_14 INS_TRAFFIC_ALERT_FOR A_AH CRIM_GANG_RELATED_VIOLENCE IN_OR_ON_POSITION", this.spawnPosition);
                    }
                    else
                    {
                        //this.CalloutMessage = string.Format(Functions.GetStringFromLanguageFile("CALLOUT_NOOSEMOD_TERRORIST_ACTIVITY_6"), Functions.GetAreaStringFromPosition(this.spawnPosition));
                        this.CalloutMessage = string.Format(Resources.CALLOUT_NOOSEMOD_TERRORIST_ACTIVITY_6, Functions.GetAreaStringFromPosition(this.spawnPosition));
                        Functions.PlaySoundUsingPosition("INS_THIS_IS_CONTROL_I_NEED_ASSISTANCE_FOR CRIM_AN_SOS FOR ERR_ERRR_04 CRIM_ASSAULT_AND_BATTERY IN_OR_ON_POSITION", this.spawnPosition);
                    } break;
                case 5:
                    string audioMessage = Functions.CreateRandomAudioIntroString(EIntroReportedBy.Civilians);
                    string crimeMessage = "CRIM_TERRORIST_ACTIVITY";
                    if (Common.GetRandomBool(0, 2, 1))
                    {
                        crimeMessage = "CRIM_CRIMINALS_DISTURBING_THE_PEACE";
                    }
                    this.CalloutMessage = string.Format(Resources.CALLOUT_NOOSEMOD_TERRORIST_ACTIVITY_7, Functions.GetAreaStringFromPosition(this.spawnPosition));
                    Functions.PlaySoundUsingPosition(audioMessage + crimeMessage + " IN_OR_ON_POSITION", this.spawnPosition);
                    break;
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
            bool isReadyForMission = base.OnCalloutAccepted();
            lastPlayerRoom = LPlayer.LocalPlayer.Ped.CurrentRoom;

            // Clear resources first
            deadSuspects.Clear(); deadHostages.Clear(); hostages.Clear(); arrestedSuspects.Clear(); missionPeds.Clear(); squad.Clear(); PoliceOfficers.Clear();

            // Create squad to follow
            try
            {
                // Grab a check if using TBoGT weapons
                bool isUsingTbogtWeapons = SettingsIni.GetValueBool("EnableTBoGTWeapons", "GlobalSettings", false);

                if (isUsingTbogtWeapons) // Debug only
                    Log.Debug("OnCalloutAccepted: TBoGT Weapons used in platform " + Game.CurrentEpisode.ToString(), this);

                // Get partners in your group
                LHandle partMan = Functions.GetCurrentPartner();
                LPed[] partner = Functions.GetPartnerPeds(partMan);

                // Adds more action by adding additional squads in
                if (partner != null)
                {
                    // Latest LCPDFR features getting a partner up to 3 people, so if not 3, this is NOT processed.
                    if (partner.Length != 3)
                    {
                        // This may return error if player is not in a car
                        if (LPlayer.LocalPlayer.Ped.IsInVehicle() || LPlayer.LocalPlayer.Ped.IsSittingInVehicle())
                        {
                            LVehicle currentVeh = LPlayer.LocalPlayer.Ped.CurrentVehicle;
                            VehicleSeat seats = currentVeh.GetFreePassengerSeat();
                            if (seats == VehicleSeat.LeftRear)
                            {
                                SquadTroop = currentVeh.CreatePedOnSeat(VehicleSeat.LeftRear, SWATTrooper, RelationshipGroup.Cop);
                                SquadTroop.ChangeRelationship(RelationshipGroup.Cop, Relationship.Companion);
                                //SquadTroop.ChangeRelationship(RelationshipGroup.Criminal, Relationship.Hate);
                                //SquadTroop.ChangeRelationship(RelationshipGroup.Gang_Biker1, Relationship.Hate);
                                //SquadTroop.ChangeRelationship(RelationshipGroup.Gang_Biker2, Relationship.Hate);
                                SquadTroop.CantBeDamagedByRelationshipGroup(RelationshipGroup.Cop, true);
                                SquadTroop.PriorityTargetForEnemies = true;
                                //Functions.AddToScriptDeletionList(SquadTroop, this);
                                Functions.SetPedIsOwnedByScript(SquadTroop, this, true);
                                squad.Add(SquadTroop);
                                Log.Info("Partner is less than 3, squad created", this);
                            }
                            else if (seats == VehicleSeat.RightRear)
                            {
                                SquadTroop = currentVeh.CreatePedOnSeat(VehicleSeat.RightRear, SWATTrooper, RelationshipGroup.Cop);
                                SquadTroop.ChangeRelationship(RelationshipGroup.Cop, Relationship.Companion);
                                //SquadTroop.ChangeRelationship(RelationshipGroup.Criminal, Relationship.Hate);
                                //SquadTroop.ChangeRelationship(RelationshipGroup.Gang_Biker1, Relationship.Hate);
                                //SquadTroop.ChangeRelationship(RelationshipGroup.Gang_Biker2, Relationship.Hate);
                                SquadTroop.CantBeDamagedByRelationshipGroup(RelationshipGroup.Cop, true);
                                SquadTroop.PriorityTargetForEnemies = true;
                                //Functions.AddToScriptDeletionList(SquadTroop, this);
                                Functions.SetPedIsOwnedByScript(SquadTroop, this, true);
                                squad.Add(SquadTroop);
                                Log.Info("Partner is less than 3, squad created", this);
                            }
                            else if (seats == VehicleSeat.LeftRear && seats == VehicleSeat.RightRear)
                            {
                                SquadTroop1 = currentVeh.CreatePedOnSeat(VehicleSeat.LeftRear, SWATTrooper, RelationshipGroup.Cop);
                                SquadTroop2 = currentVeh.CreatePedOnSeat(VehicleSeat.RightRear, SWATTrooper, RelationshipGroup.Cop);
                                SquadTroop1.ChangeRelationship(RelationshipGroup.Cop, Relationship.Companion);
                                //SquadTroop1.ChangeRelationship(RelationshipGroup.Criminal, Relationship.Hate);
                                //SquadTroop1.ChangeRelationship(RelationshipGroup.Gang_Biker1, Relationship.Hate);
                                //SquadTroop1.ChangeRelationship(RelationshipGroup.Gang_Biker2, Relationship.Hate);
                                SquadTroop2.ChangeRelationship(RelationshipGroup.Cop, Relationship.Companion);
                                //SquadTroop2.ChangeRelationship(RelationshipGroup.Criminal, Relationship.Hate);
                                //SquadTroop2.ChangeRelationship(RelationshipGroup.Gang_Biker1, Relationship.Hate);
                                //SquadTroop2.ChangeRelationship(RelationshipGroup.Gang_Biker2, Relationship.Hate);
                                SquadTroop1.PriorityTargetForEnemies = true;
                                SquadTroop1.CantBeDamagedByRelationshipGroup(RelationshipGroup.Cop, true);
                                SquadTroop2.PriorityTargetForEnemies = true;
                                SquadTroop2.CantBeDamagedByRelationshipGroup(RelationshipGroup.Cop, true);
                                //Functions.AddToScriptDeletionList(SquadTroop1, this);
                                //Functions.AddToScriptDeletionList(SquadTroop2, this);
                                Functions.SetPedIsOwnedByScript(SquadTroop1, this, true);
                                Functions.SetPedIsOwnedByScript(SquadTroop2, this, true);
                                squad.Add(SquadTroop1);
                                squad.Add(SquadTroop2);
                                Log.Info("Partner is less than 3, squad created", this);
                            }
                            for (int i = 0; i < partner.Length; i++)
                            {
                                partner[i].PriorityTargetForEnemies = true;
                                squad.Add(partner[i]);
                            }
                        }
                        else
                        {
                            nooseVeh3 = LVehicle.FromGTAVehicle(World.CreateVehicle(LPlayer.LocalPlayer.Ped.Position));
                            nooseVeh3.PlaceOnNextStreetProperly();
                            nooseVeh3.MakeProofTo(true, true, true, true, true);
                            nooseVeh3.EngineRunning = true;
                            SquadTroop1 = nooseVeh3.CreatePedOnSeat(VehicleSeat.Driver, SWATTrooper, RelationshipGroup.Cop);
                            SquadTroop1.Task.DriveTo(this.spawnPosition, 9999.9999f, false, true);
                            SquadTroop2 = nooseVeh3.CreatePedOnSeat(VehicleSeat.RightFront, SWATTrooper, RelationshipGroup.Cop);
                            SquadTroop1.ChangeRelationship(RelationshipGroup.Cop, Relationship.Companion);
                            //SquadTroop1.ChangeRelationship(RelationshipGroup.Criminal, Relationship.Hate);
                            //SquadTroop1.ChangeRelationship(RelationshipGroup.Gang_Biker1, Relationship.Hate);
                            //SquadTroop1.ChangeRelationship(RelationshipGroup.Gang_Biker2, Relationship.Hate);
                            SquadTroop2.ChangeRelationship(RelationshipGroup.Cop, Relationship.Companion);
                            //SquadTroop2.ChangeRelationship(RelationshipGroup.Criminal, Relationship.Hate);
                            //SquadTroop2.ChangeRelationship(RelationshipGroup.Gang_Biker1, Relationship.Hate);
                            //SquadTroop2.ChangeRelationship(RelationshipGroup.Gang_Biker2, Relationship.Hate);
                            SquadTroop1.PriorityTargetForEnemies = true;
                            SquadTroop1.BlockPermanentEvents = true;
                            SquadTroop1.Task.AlwaysKeepTask = true;
                            SquadTroop1.CantBeDamagedByRelationshipGroup(RelationshipGroup.Cop, true);
                            SquadTroop2.PriorityTargetForEnemies = true;
                            SquadTroop2.BlockPermanentEvents = true;
                            SquadTroop2.Task.AlwaysKeepTask = true;
                            SquadTroop2.CantBeDamagedByRelationshipGroup(RelationshipGroup.Cop, true);
                            //Functions.AddToScriptDeletionList(SquadTroop1, this);
                            //Functions.AddToScriptDeletionList(SquadTroop2, this);
                            Functions.SetPedIsOwnedByScript(SquadTroop1, this, true);
                            Functions.SetPedIsOwnedByScript(SquadTroop2, this, true);
                            squad.Add(SquadTroop1);
                            squad.Add(SquadTroop2);
                            Log.Info("Partner is less than 3 but you are not in a car, squad created in any car", this);
                        }
                    }
                        // If you have at least 3 partners of the same model
                        // This is not removed as partners will be used in any event such as callouts
                    else for (int i = 0; i < partner.Length; i++)
                        {
                            partner[i].PriorityTargetForEnemies = true;
                            squad.Add(partner[i]);
                        }
                }
                else // If no partner, a NOOSE cruiser will be added near you with SWATs inside and
                    // made tracks to the crime scene
                {
                    Log.Info("Only you responds to the crime scene, creating car with NOOSE...", this);
                    nooseVeh1 = new LVehicle(World.GetNextPositionOnStreet(LPlayer.LocalPlayer.Ped.Position), Common.GetRandomCollectionValue<string>(nooseVehicleModels));
                    SquadTroops = new LPed[4] 
                    {
                        new LPed(nooseVeh1.Position, "M_Y_SWAT", LPed.EPedGroup.Cop),
                        new LPed(nooseVeh1.Position, "M_Y_SWAT", LPed.EPedGroup.Cop),
                        new LPed(nooseVeh1.Position, "M_Y_SWAT", LPed.EPedGroup.Cop),
                        new LPed(nooseVeh1.Position, "M_Y_SWAT", LPed.EPedGroup.Cop)
                    };
                    nooseVeh1.PlaceOnNextStreetProperly();
                    nooseVeh1.EngineRunning = true;
                    nooseVeh1.SirenActive = true;
                    nooseVeh1.AllowSirenWithoutDriver = true;
                    nooseVeh1.MakeProofTo(true, true, true, true, true); // Prevent the car from being damaged because of poor AI programming
                    Functions.AddToScriptDeletionList(nooseVeh1, this);
                    for (int i = 0; i < SquadTroops.Length; i++)
                    {
                        if (!nooseVeh1.HasDriver)
                        {
                            SquadTroops[i].WarpIntoVehicle(nooseVeh1, VehicleSeat.Driver);
                            SquadTroops[i].Task.DriveTo(spawnPosition, 9999.9999f, false, true);
                        }
                        else
                        {
                            SquadTroops[i].WarpIntoVehicle(nooseVeh1, VehicleSeat.AnyPassengerSeat);
                        }
                        SquadTroops[i].RelationshipGroup = RelationshipGroup.Cop;
                        SquadTroops[i].ChangeRelationship(RelationshipGroup.Cop, Relationship.Companion);
                        //SquadTroops[i].ChangeRelationship(RelationshipGroup.Criminal, Relationship.Hate);
                        //SquadTroops[i].ChangeRelationship(RelationshipGroup.Gang_Biker1, Relationship.Hate);
                        //SquadTroops[i].ChangeRelationship(RelationshipGroup.Gang_Biker2, Relationship.Hate);
                        SquadTroops[i].CantBeDamagedByRelationshipGroup(RelationshipGroup.Cop, true);
                        SquadTroops[i].PriorityTargetForEnemies = true;
                        SquadTroops[i].Weapons.RemoveAll();
                        int x = Common.GetRandomValue(1, 2+1), y = Common.GetRandomValue(3, 5+1);
                        if (isUsingTbogtWeapons && Game.CurrentEpisode == GameEpisode.TBOGT)
                        {
                            y = Common.GetRandomValue(3, 9+1);
                        }
                        SquadTroops[i].Weapons.FromType(Weapon.Thrown_Grenade).Ammo = 1;
                        SquadTroops[i].Weapons.FromType(Weapon.Melee_Knife);
                        SquadTroops[i].Weapons.FromType(controller.AssignCounterTerroristWeapon(x)).Ammo = 999;
                        SquadTroops[i].Weapons.FromType(controller.AssignCounterTerroristWeapon(y)).Ammo = 999;
                        SquadTroops[i].Weapons.Select(controller.AssignCounterTerroristWeapon(y));
                        //Functions.AddToScriptDeletionList(SquadTroops[i], this);
                        Functions.SetPedIsOwnedByScript(SquadTroops[i], this, true);
                        SquadTroops[i].BlockPermanentEvents = true;
                        SquadTroops[i].Task.AlwaysKeepTask = true;
                        // Register added squad into the Squads list
                        squad.Add(SquadTroops[i]);
                    }
                }
                isReadyForMission = true;
            }
            catch (Exception ex) { Log.Error("Error creating or assigning a ped: " + ex, this); isReadyForMission = false; }
            if (isReadyForMission)
            {
                InitializeStartingValues();
                mission = MissionState.GoingToMissionLocation;

                // OnDuty is not assigned
                Functions.PrintText(Functions.GetStringFromLanguageFile("CALLOUT_GET_TO_CRIME_SCENE"), 8000);
                LPlayer.LocalPlayer.Ped.PriorityTargetForEnemies = true;

                // Print information from Dispatch if the crime scene contains hostages
                if(activeMission.HostageLocations.Count != 0)
                {
                Functions.AddTextToTextwall("Be advised, police reports " + activeMission.HostageLocations.Count + " hostages in the area. "+
                    "Confirmed terrorist activity. Proceed with extreme prejudice.",
                    Functions.GetStringFromLanguageFile("POLICE_SCANNER_CONTROL"));
                }

                Log.Debug("OnCalloutAccepted: Initialized", this);
                //stage = WaitingForPlayer;
            }
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

            Log.Debug("OnCalloutNotAccepted: Resources cleared", this);

            mission = MissionState.Off;
        }

#region Actions
        Vector3 myPos;

        int timeForTerroristsToBreak, hostages_number_temp = 0;

        bool hostages_isAvailable = new Boolean();

        /// <summary>
        /// Initialize starting values used to process actions.
        /// </summary>
        private void InitializeStartingValues()
        {
            myPos = LPlayer.LocalPlayer.Ped.Position;
            if (activeMission.HostageLocations.Count == 0)
            {
                // Mission without hostages will start with 4x time limit than normal, useful if you are having a hard time to reach target location.
                // Example: distance from Middle Park to crime scene: 200, time: 200000*10 ms or 200 seconds (3 min. 20 sec.) -> 290 seconds (4 min. 50 sec.) after addition -> 1160 seconds (19 min. 20 sec.) if the crime scene doesn't contain hostages
                // The tick of an interval is per ten milliseconds (LCPDFR runs on 10 interval)
                timeForTerroristsToBreak = ((int)myPos.DistanceTo(this.spawnPosition) * 100 + 9000) * 4;
                hostages_isAvailable = false;
            }
            else
            {
                // Grab a distance check and multiply it into 100, then add 9000 (90 seconds)
                // Example: distance from Middle Park to crime scene: 200, time: 200000*10 ms or 200 seconds (3 min. 20 sec.) -> 290 seconds (4 min. 50 sec.) after addition
                // The timer depends on the location you are responding, so that you will not be so hustle
                //  (1 minute is rather extreme if you're responding in Alderney while the target location is at Broker).
                timeForTerroristsToBreak = (int)myPos.DistanceTo(this.spawnPosition) * 100 + 9000;
                hostages_number_temp = activeMission.HostageLocations.Count;
                hostages_isAvailable = true;
            }
            CurrentGameTime = World.CurrentDayTime;
        }

        int ticks, hostage_kill_list = 0;

        /// <summary>
        /// Estimated Time of Arrival to the Crime Scene
        /// </summary>
        TimeSpan ETA_ToCrimeScene;

        /// <summary>
        /// Game Time that is recorded when called in
        /// </summary>
        TimeSpan CurrentGameTime;

        /// <summary>
        /// This method is used to wait for the Player until he reaches the crime scene.
        /// </summary>
        private void WaitingForPlayer()
        {
            // Increase tick number when called (RegisterStateCallback made frequent calls in every tick)
            // The tick of the script interval is per ten milliseconds (LCPDFR runs on 10 interval)
            ticks++;
            int RemainingTime = timeForTerroristsToBreak - ticks;
            ETA_ToCrimeScene = CurrentGameTime + TimeSpan.FromTicks((Int64)timeForTerroristsToBreak * (10^1));
            Game.DisplayText("Time to Reach the Crime Scene: " + RemainingTime + ", ETA: " + ETA_ToCrimeScene);
            if (LPlayer.LocalPlayer.Ped.Position.DistanceTo(this.spawnPosition) > (float)150)
            {
                if (ticks >= timeForTerroristsToBreak)
                {
                    // Kill a hostage and increase time to 150 seconds (2 min. 30 sec.)
                    if (hostage_kill_list != hostages_number_temp && hostages_isAvailable)
                    {
                        hostage_kill_list++;
                        timeForTerroristsToBreak += 15000;
                        CurrentGameTime = World.CurrentDayTime;

                        // If reported three hostages killed (when exists), radio it in
                        // When it's more than 3 it'll NOT be radioed in as the Dispatch know the situation is
                        if (hostage_kill_list == 3)
                        {
                            //Functions.AddTextToTextwall(Functions.GetStringFromLanguageFile("CALLOUT_NOOSEMOD_HOSTAGES_BEING_EXECUTED"), Functions.GetStringFromLanguageFile("POLICE_SCANNER_CONTROL"));
                            Functions.AddTextToTextwall(Resources.CALLOUT_NOOSEMOD_HOSTAGES_BEING_EXECUTED, Functions.GetStringFromLanguageFile("POLICE_SCANNER_CONTROL"));
                            Functions.PlaySoundUsingPosition("ATTENTION_ALL_UNITS INS_WEVE_GOT CRIM_MULTIPLE_INJURIES REPEAT CRIM_MULTIPLE_INJURIES IN_OR_ON_POSITION", this.spawnPosition);
                        }
                        Log.Debug("WaitingForPlayer: Hostage executed by a terrorist, current kills: " + hostage_kill_list, this);
                    }
                    else
                    {
                        LPlayer.LocalPlayer.Ped.Money -= 5000;
                        Functions.PrintText("~r~Mission Failed~n~~w~You didn't reach the target location on time. You have lost $5000 for your incompetence.", 25000);
                        SetCalloutFinished(false, false, true); End(); mission = MissionState.Off;
                    }
                }
            }
            else //if (LPlayer.LocalPlayer.Ped.Position.DistanceTo(this.spawnPosition) <= 50)
            {
                // Create ambient police force (roadblocks and cops with Carbine Rifle and Semi-Auto Shotgun)
                // This is defined through the Safe Mode in the INI file
                // Road barriers (roadblock) are not implemented, yet (in later version it will)
                bool safeMode = SettingsIni.GetValueBool("SafeMode", "GlobalSettings", false);
                Log.Debug("WaitingForPlayer: Safe Mode is " + safeMode, this);
                if (safeMode == false) try
                    {
                        LVehicle copCarBlock1 = new LVehicle(World.GetPositionAround(spawnPosition, (float)Common.GetRandomValue(20, 50)), "POLICE");
                        copCarBlock1.PlaceOnGroundProperly();
                        copCarBlock1.SirenActive = true;
                        copCarBlock1.EngineRunning = true;
                        Functions.AddToScriptDeletionList(copCarBlock1, this);
                        LVehicle copCarBlock2 = new LVehicle(World.GetPositionAround(spawnPosition, (float)Common.GetRandomValue(20, 50)), "POLICE");
                        copCarBlock2.PlaceOnGroundProperly();
                        copCarBlock2.SirenActive = true;
                        copCarBlock2.EngineRunning = true;
                        Functions.AddToScriptDeletionList(copCarBlock2, this);
                        LVehicle copCarBlock3 = new LVehicle(World.GetPositionAround(spawnPosition, (float)Common.GetRandomValue(20, 50)), "POLICE");
                        copCarBlock3.PlaceOnGroundProperly();
                        copCarBlock3.SirenActive = true;
                        copCarBlock3.EngineRunning = true;
                        Functions.AddToScriptDeletionList(copCarBlock3, this);
                        for (int i = 0; i <= 6; i++) // 0 to 6, 7 peds allocated
                        {
                            PoliceOfficers.Add(new LPed(copCarBlock1.GetOffsetPosition(spawnPosition).Around((float)(Common.GetRandomValue(9, 20))), "M_Y_COP"));
                            PoliceOfficers[i].Weapons.RemoveAll();
                            PoliceOfficers[i].Weapons.FromType(Weapon.Handgun_Glock).Ammo = 999;
                            PoliceOfficers[i].Weapons.FromType(Weapon.Rifle_M4).Ammo = 300;
                            PoliceOfficers[i].Weapons.Select(Weapon.Rifle_M4);
                            PoliceOfficers[i].MaxHealth = 500;
                            PoliceOfficers[i].Health = 500;
                            PoliceOfficers[i].RelationshipGroup = RelationshipGroup.Cop;
                            PoliceOfficers[i].ChangeRelationship(RelationshipGroup.Cop, Relationship.Companion);
                            //PoliceOfficers[i].ChangeRelationship(RelationshipGroup.Criminal, Relationship.Hate);
                            //PoliceOfficers[i].ChangeRelationship(RelationshipGroup.Gang_Biker1, Relationship.Hate);
                            //PoliceOfficers[i].ChangeRelationship(RelationshipGroup.Gang_Biker2, Relationship.Hate);
                            //Functions.AddToScriptDeletionList(PoliceOfficers[i], this);
                            Functions.SetPedIsOwnedByScript(PoliceOfficers[i], this, true);
                        }
                        for (int i = 7; i <= 12; i++) // 7 to 12, 5 peds allocated
                        {
                            PoliceOfficers.Add(new LPed(copCarBlock2.GetOffsetPosition(spawnPosition).Around((float)(Common.GetRandomValue(9, 20))), "M_Y_COP"));
                            PoliceOfficers[i].Weapons.RemoveAll();
                            PoliceOfficers[i].Weapons.FromType(Weapon.Handgun_Glock).Ammo = 999;
                            PoliceOfficers[i].Weapons.FromType(Weapon.Shotgun_Baretta).Ammo = 40;
                            PoliceOfficers[i].Weapons.Select(Weapon.Shotgun_Baretta);
                            PoliceOfficers[i].MaxHealth = 500;
                            PoliceOfficers[i].Health = 500;
                            PoliceOfficers[i].RelationshipGroup = RelationshipGroup.Cop;
                            PoliceOfficers[i].ChangeRelationship(RelationshipGroup.Cop, Relationship.Companion);
                            //PoliceOfficers[i].ChangeRelationship(RelationshipGroup.Criminal, Relationship.Hate);
                            //PoliceOfficers[i].ChangeRelationship(RelationshipGroup.Gang_Biker1, Relationship.Hate);
                            //PoliceOfficers[i].ChangeRelationship(RelationshipGroup.Gang_Biker2, Relationship.Hate);
                            //Functions.AddToScriptDeletionList(PoliceOfficers[i], this);
                            Functions.SetPedIsOwnedByScript(PoliceOfficers[i], this, true);
                        }
                    }
                    catch (Exception ex) { Log.Error("Unable to spawn vehicles and peds: " + ex, this); }

                try
                {
                    // Grab a check if using TBoGT weapons
                    bool isUsingTbogtWeapons = SettingsIni.GetValueBool("EnableTBoGTWeapons", "GlobalSettings", false);

                    if (isUsingTbogtWeapons) // Debug only
                        Log.Debug("OnCalloutAccepted: TBoGT Weapons used in platform " + Game.CurrentEpisode.ToString(), this);

                    // Grab a check if Biker Gangs are used in this callout
                    bool isBikerGangUsedInCallout = SettingsIni.GetValueBool("SwapCriminalsToBikerGangMembers", "GlobalSettings", false);

                    if (isBikerGangUsedInCallout) // Debug only
                        Log.Debug("OnCalloutAccepted: Biker Gang is used in this callout", this);

                    // Spawn terrorists
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
                                default: throw new Exception("Invalid GangType: Must between AOD and TLMC");
                            }
                        }

                        LPed suspectPed = new LPed(suspectLocation, selectedModel, LPed.EPedGroup.MissionPed);

                        // Randomize
                        int num = 1;
                        if (isUsingTbogtWeapons && Game.CurrentEpisode == GameEpisode.TBOGT)
                        {
                            num = Common.GetRandomValue(1, 6 + 1);
                        }
                        else { num = Common.GetRandomValue(1, 4 + 1); }

                        // Manage things like in original NooseMod does (with SHDN updates compared to the old one)
                        if (isBikerGangUsedInCallout == false)
                        {
                            suspectPed.RelationshipGroup = RelationshipGroup.Criminal;
                            suspectPed.ChangeRelationship(RelationshipGroup.Criminal, Relationship.Respect);
                            suspectPed.ChangeRelationship(RelationshipGroup.Cop, Relationship.Hate);
                            suspectPed.CantBeDamagedByRelationshipGroup(RelationshipGroup.Criminal, true);
                        }
                        else switch (SettingsIni.GetValueString("GangType", "GlobalSettings", "AOD"))
                            {
                                case "AOD":
                                    suspectPed.RelationshipGroup = RelationshipGroup.Gang_Biker1;
                                    suspectPed.ChangeRelationship(RelationshipGroup.Gang_Biker1, Relationship.Companion);
                                    suspectPed.ChangeRelationship(RelationshipGroup.Cop, Relationship.Hate);
                                    suspectPed.ChangeRelationship(RelationshipGroup.Civillian_Male, Relationship.Dislike);
                                    suspectPed.ChangeRelationship(RelationshipGroup.Civillian_Female, Relationship.Dislike);
                                    suspectPed.CantBeDamagedByRelationshipGroup(RelationshipGroup.Gang_Biker1, true);
                                    break;
                                case "TLMC":
                                    suspectPed.RelationshipGroup = RelationshipGroup.Gang_Biker2;
                                    suspectPed.ChangeRelationship(RelationshipGroup.Gang_Biker2, Relationship.Companion);
                                    suspectPed.ChangeRelationship(RelationshipGroup.Cop, Relationship.Hate);
                                    suspectPed.ChangeRelationship(RelationshipGroup.Civillian_Male, Relationship.Dislike);
                                    suspectPed.ChangeRelationship(RelationshipGroup.Civillian_Female, Relationship.Dislike);
                                    suspectPed.CantBeDamagedByRelationshipGroup(RelationshipGroup.Gang_Biker2, true);
                                    break;
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
                            int rand = Common.GetRandomValue(7, 8 + 1);
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

                        // Last but not least
                        //Functions.AddToScriptDeletionList(suspectPed, this);
                        Functions.SetPedIsOwnedByScript(suspectPed, this, true);
                        suspectPed.BlockPermanentEvents = true;
                        suspectPed.Task.AlwaysKeepTask = true; // this will be reset to change the task
                        suspectPed.EquipWeapon();
                        missionPeds.Add(suspectPed);
                    }

                    // Spawn hostages
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

                    // Kill hostages
                    if (hostage_kill_list != 0) for (int i = 0; i < hostage_kill_list; i++)
                            if (hostages[i] != null || hostages[i].Exists())
                                if (hostages[i].isAliveAndWell && !hostages[i].isDead)
                                {
                                    hostages[i].Die();
                                    deadHostages.Add(hostages[i]);
                                    hostages.Remove(hostages[i]);
                                }
                                else continue;
                            else continue;

                    // Make sure the suspects are everything-proof first before attempting to charge in
                    foreach (LPed myped in missionPeds) if (myped != null || myped.Exists())
                            myped.MakeProofTo(true, true, true, true, true);
                        else continue;

                    // If the first NOOSE vehicle is not yet at the scene, warp
                    if (nooseVeh1 != null) try
                        {
                            if (nooseVeh1.Position.DistanceTo(this.spawnPosition) > (float)400 && nooseVeh1.Exists())
                            {
                                nooseVeh1.Position = this.spawnPosition.Around((float)399);
                                nooseVeh1.PlaceOnNextStreetProperly();
                            }
                        }
                        catch (Exception ex) { Log.Error("WaitingForPlayer(): Error getting vehicle data or object: " + ex, this); }

                    // If the third NOOSE vehicle is exists/available at start but not yet at the scene, warp
                    if (nooseVeh3 != null) try
                        {
                            if (nooseVeh3.Position.DistanceTo(this.spawnPosition) > (float)500 && nooseVeh3.Exists())
                            {
                                nooseVeh3.Position = this.spawnPosition.Around((float)499);
                                nooseVeh3.PlaceOnNextStreetProperly();
                            }
                        }
                        catch (Exception ex) { Log.Error("WaitingForPlayer(): Error getting vehicle data or object: " + ex, this); }
                }
                catch (Exception ex) { Log.Error("WaitingForPlayer(): Error spawning or getting vehicle/ped data or object: " + ex, this); }
                mission = MissionState.WaitForTeamInsertion;
            }
            //else return;
        }

        /// <summary>
        /// When <see cref="MissionState"/> is <see cref="MissionState.WaitForTeamInsertion"/>,
        /// a NOOSE squad and a police backup are dispatched to the target location. After all set, the shootout can begin.
        /// </summary>
        private void DispatchTeam()
        {
            /*// If last mission is confirmed, play Epilogue music
            int finalMission = controller.loadedMissions.Count - 1;
            if (lastPlayedMission == finalMission)
            {
                this.PlaySound(true);
                Log.Debug("DispatchTeam: Sound played", this);
            }*/

            //Functions.AddTextToTextwall(string.Format(Functions.GetStringFromLanguageFile("CALLOUT_NOOSEMOD_PLAYER_IN_POS"), Functions.GetAreaStringFromPosition(this.spawnPosition)), LPlayer.LocalPlayer.Name);
            Functions.AddTextToTextwall(string.Format(Resources.CALLOUT_NOOSEMOD_PLAYER_IN_POS, Functions.GetAreaStringFromPosition(this.spawnPosition)), LPlayer.LocalPlayer.Username);
            Game.WaitInCurrentScript(3000);
            //Function.Call("WAIT", new Parameter[] { 3000 });

            // When arrived in time, dispatch NOOSE Squad in with additional police force
            // Breach in when in position
            nooseVeh2 = new LVehicle(World.GetPositionAround(this.spawnPosition, (float)(2 ^ 9)), Common.GetRandomCollectionValue<string>(nooseVehicleModels));
            LPed nooseDisp1 = nooseVeh2.CreatePedOnSeat(VehicleSeat.Driver, SWATTrooper, RelationshipGroup.Cop);
            LPed nooseDisp2 = nooseVeh2.CreatePedOnSeat(VehicleSeat.AnyPassengerSeat, SWATTrooper, RelationshipGroup.Cop);
            LPed nooseDisp3 = nooseVeh2.CreatePedOnSeat(VehicleSeat.AnyPassengerSeat, SWATTrooper, RelationshipGroup.Cop);
            LPed nooseDisp4 = nooseVeh2.CreatePedOnSeat(VehicleSeat.AnyPassengerSeat, SWATTrooper, RelationshipGroup.Cop);
            nooseDisp1.Task.DriveTo(this.spawnPosition, 9999.9999f, false, true);
            SquadTroops = new LPed[4] { nooseDisp1, nooseDisp2, nooseDisp3, nooseDisp4 };
            for (int i = 0; i < SquadTroops.Length; i++)
            {
                SquadTroops[i].ChangeRelationship(RelationshipGroup.Cop, Relationship.Companion);
                //SquadTroops[i].ChangeRelationship(RelationshipGroup.Criminal, Relationship.Hate);
                //SquadTroops[i].ChangeRelationship(RelationshipGroup.Gang_Biker1, Relationship.Hate);
                //SquadTroops[i].ChangeRelationship(RelationshipGroup.Gang_Biker2, Relationship.Hate);
                SquadTroops[i].ChangeRelationship(RelationshipGroup.Civillian_Male, Relationship.Like);
                SquadTroops[i].ChangeRelationship(RelationshipGroup.Civillian_Female, Relationship.Like);
                SquadTroops[i].BlockPermanentEvents = true;
                SquadTroops[i].Task.AlwaysKeepTask = true;
                SquadTroops[i].Weapons.RemoveAll();
                SquadTroops[i].Weapons.FromType(Weapon.Melee_Knife);
                SquadTroops[i].Weapons.FromType(Weapon.Handgun_Glock).Ammo = 5 ^ 5;
                SquadTroops[i].Weapons.FromType(Weapon.SMG_Uzi).Ammo = 5 ^ 3;
                bool getSniperInsteadOfM4 = Common.GetRandomBool(0, 9, 0);
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
                //Functions.AddToScriptDeletionList(SquadTroops[i], this);
                Functions.SetPedIsOwnedByScript(SquadTroops[i], this, true);
            }
            nooseVeh2.AllowSirenWithoutDriver = true;
            Blip nooseVeh2PosNow = nooseVeh2.AttachBlip();
            nooseVeh2PosNow.Color = BlipColor.Cyan;
            nooseVeh2PosNow.Friendly = true;
            nooseVeh2PosNow.Name = "Squad Group 2";
            nooseVeh2.SirenActive = true;
            nooseVeh2.EngineRunning = true;
            nooseVeh2.PlaceOnNextStreetProperly();
            nooseVeh2.MakeProofTo(true, true, true, true, true);
            Functions.RequestPoliceBackupAtPosition(this.spawnPosition); // Police escort?
            Functions.PlaySoundUsingPosition("DFROM_DISPATCH_A_NOOSE_TEAM_FROM POSITION FOR CRIM_TERRORIST_ACTIVITY", nooseVeh2.Position);
            //Functions.PrintText(Functions.GetStringFromLanguageFile("CALLOUT_NOOSEMOD_STANDBY"), 25000);
            Functions.PrintText(Resources.CALLOUT_NOOSEMOD_STANDBY, 10000);
            Functions.AddToScriptDeletionList(nooseVeh2, this);

            mission = MissionState.Initialize;
        }

        /// <summary>
        /// Wait for the second team to arrive.
        /// </summary>
        private void WaitFor2ndTeam()
        {
            if (nooseVeh2.Position.DistanceTo(this.spawnPosition) <= (float)60 && mission == MissionState.Initialize)
            {
                if (nooseVeh2.Blip != null || nooseVeh2.Blip.Exists())
                    nooseVeh2.Blip.Delete();
                Initialize();
            }
            else return;
        }

        /// <summary>
        /// Here, the real action begins.
        /// </summary>
        private void Initialize()
        {
            for (int i = 0; i < squad.Count; i++)
            {
                if (squad[i] != null || squad[i].Exists())
                {
                    if (squad[i].IsInVehicle() && squad[i].Position.DistanceTo(this.spawnPosition) <= 80f)
                    {
                        squad[i].BlockPermanentEvents = false;
                        squad[i].Task.AlwaysKeepTask = false;
                        squad[i].LeaveVehicle();
                        squad[i].EquipWeapon();
                    }
                    else
                    {
                        squad[i].BlockPermanentEvents = false;
                        squad[i].Task.AlwaysKeepTask = false;
                        squad[i].EquipWeapon();
                    }
                }
                else
                {
                    squad.Remove(squad[i]);
                    continue;
                }
            }

            // Build Pursuit instance
            this.pursuit = Functions.CreatePursuit();
            Functions.SetPursuitCalledIn(this.pursuit, true);
            Functions.SetPursuitIsActiveForPlayer(this.pursuit, true);
            Functions.SetPursuitAllowWeaponsForSuspects(this.pursuit, true);
            Functions.SetPursuitAllowVehiclesForSuspects(this.pursuit, false);
            Functions.SetPursuitForceSuspectsToFight(this.pursuit, true);
            Functions.SetPursuitIsActiveDelayed(this.pursuit, 3000, 10000);
            Functions.SetPursuitCopsCanJoin(this.pursuit, false);
            controller.entryLoc.Delete();

            // Make sure they are exist before making adjustments
            foreach (LPed myped in missionPeds) if (myped != null || myped.Exists()) try
                    {
                        Functions.AddPedToPursuit(this.pursuit, myped);
                        myped.DisablePursuitAI = true;

                        // Set difficulty
                        if (controller.HardcoreModeIsActive() == false)
                        {
                            difficulty = getDifficulty();
                        }
                        else { difficulty = Difficulty.Hard; }

                        // Delete LCPDFR-generated blip on suspects because this will use custom blip
                        //if (myped.Blip != null || myped.Blip.Exists()) myped.Blip.Delete();

                        if (difficulty == Difficulty.Easy) // Blipped and unarmored
                        {
                            Blip suspectBlip = myped.AttachBlip();
                            suspectBlip.Color = BlipColor.Red;
                            suspectBlip.Display = BlipDisplay.ArrowAndMap;
                            suspectBlip.Friendly = false;
                            suspectBlip.Name = "Terrorist";
                            myped.Armor = 0;

                            // The method used in old NooseMod is deprecated - used safer instances instead
                            //suspectPed.GPed.SetMetadata<Blip>("blip",true,suspectBlip);
                        }
                        else if (difficulty == Difficulty.Hard) // Armored and 1.5x SHOOT accuracy
                            try
                            {
                                if (myped.Blip != null) myped.DeleteBlip();
                            }
                            catch (Exception ex)
                            {
                                Log.Error("Failed to remove blip on a terrorist ped: " + ex, this);
                            }
                            finally
                            {
                                myped.MaxHealth = 250;
                                myped.Health = 250;
                                myped.Armor = 100;
                                myped.Accuracy = 150;
                            }
                        else if (difficulty == Difficulty.Medium) try
                            {
                                if (myped.Blip != null) myped.DeleteBlip();
                            }
                            catch (Exception ex)
                            {
                                Log.Error("Failed to remove blip on a terrorist ped: " + ex, this);
                            }
                            finally
                            {
                                myped.Armor = 50;
                            }
                        myped.MakeProofTo(false, false, false, false, false);
                        myped.EquipWeapon();
                    }
                    catch (Exception ex) { Log.Error("InitiateShootout(): Failed to set properties on a ped: " + ex, this); continue; }
                else continue;

            // Make sure they are exist before making adjustments
            foreach (Ped myped in hostages) if (myped.Exists() || myped != null) try
                    {
                        if (difficulty == Difficulty.Easy)
                        {
                            Blip hostageBlip = myped.AttachBlip();
                            hostageBlip.Color = BlipColor.Cyan;
                            hostageBlip.Icon = BlipIcon.Misc_Ransom;
                            hostageBlip.Friendly = true;
                            hostageBlip.Name = "Hostage";

                            /*// Standard ped do not have option to grab a Blip instance, used Metadata instead
                            // Method is similar to the old NooseMod, but not for suspects
                            // TODO: Look for other alternative (to bypass deprecated method)
                            myped.SetMetadata<Blip>("blip", true, hostageBlip);*/
                        }
                        else if (difficulty == Difficulty.Hard)
                        {
                            myped.MaxHealth = 50;
                            myped.Health = 50;
                            //myped.AlwaysDiesOnLowHealth = false;
                        }
                    }
                    catch (Exception ex) { Log.Error("InitiateShootout(): Failed to set properties on a ped: " + ex, this); continue; }
                else continue;

            //Functions.PrintText(Functions.GetStringFromLanguageFile("CALLOUT_NOOSEMOD_FIGHT_SUSPECTS"), 25000);
            Functions.PrintText(Resources.CALLOUT_NOOSEMOD_FIGHT_SUSPECTS, 25000);
            Log.Debug("InitiateShootout: Initialized", this);

            if (nooseVeh1 != null || nooseVeh1.Exists()) try
                {
                    nooseVeh1.MakeProofTo(false, false, false, false, false);
                }
                catch (Exception ex) { Log.Error("InitiateShootout(): Failed to disable invulnerability on a vehicle: " + ex, this); }

            if (nooseVeh2 != null || nooseVeh2.Exists()) try
                {
                    nooseVeh2.MakeProofTo(false, false, false, false, false);
                }
                catch (Exception ex) { Log.Error("InitiateShootout(): Failed to disable invulnerability on a vehicle: " + ex, this); }

            if (nooseVeh3 != null || nooseVeh3.Exists()) try
                {
                    nooseVeh3.MakeProofTo(false, false, false, false, false);
                }
                catch (Exception ex) { Log.Error("InitiateShootout(): Failed to disable invulnerability on a vehicle: " + ex, this); }

            // Change state to track peds in current room
            mission = MissionState.DuringMission;
        }

        /// <summary>
        /// Action for the remaining terrorists to flee the crime scene because being outnumbered by cops and NOOSE.
        /// </summary>
        private bool CombatOrders_RemainingTerrorists()
        {
            Game.WaitInCurrentScript(300);

            // Start pursuit when at least 2 or 3 terrors left and is near the entrypoint
            // Conduct shootout with cops and steal a cop car if they have no choice
            //Functions.PrintText(Functions.GetStringFromLanguageFile("CALLOUT_NOOSEMOD_FLEEING_TERRORISTS"), 8000);
            Functions.PrintText(Resources.CALLOUT_NOOSEMOD_FLEEING_TERRORISTS, 8000);
            LPlayer.LocalPlayer.Ped.SayAmbientSpeech("CALL_IN_PURSUIT_01");
            Functions.AddTextToTextwall("Suspects are escaping. Officers, we could use some help here!", LPlayer.LocalPlayer.Username);

            /*// Build Pursuit instance
            this.pursuit = Functions.CreatePursuit();
            Functions.SetPursuitCalledIn(this.pursuit, true);
            Functions.SetPursuitIsActiveForPlayer(this.pursuit, true);
            Functions.SetPursuitAllowWeaponsForSuspects(this.pursuit, true);
            //Functions.SetPursuitAllowVehiclesForSuspects(this.pursuit, false);
            //Functions.SetPursuitForceSuspectsToFight(this.pursuit, true);
            Functions.SetPursuitIsActiveDelayed(this.pursuit, 1000, 10000);
            //Functions.SetPursuitCopsCanJoin(this.pursuit, false);
             * */

            // Allow suspects to flee in a car
            Functions.SetPursuitAllowVehiclesForSuspects(this.pursuit, true);
            Functions.SetPursuitForceSuspectsToFight(this.pursuit, false);
            Functions.SetPursuitCopsCanJoin(this.pursuit, true);

            bool safeMode = SettingsIni.GetValueBool("SafeMode", "GlobalSettings", false);
            if (safeMode == false)
            {
                Functions.SetPursuitMaximumUnits(this.pursuit, 20, 30);
                Functions.SetPursuitAllowMaxUnitsTolerance(this.pursuit, false);
            }
            else Functions.SetPursuitMaximumUnits(this.pursuit, 5, Common.GetRandomValue((short)2 ^ 3, (short)2 ^ 4));

            bool isRemainingTerroristsFled = new Boolean();

            foreach (LPed myped in missionPeds) try
                {
                    if (myped != null && myped.IsAlive && myped.Exists())
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
                            activePedBlip.Name = "Fleeing Terrorist";
                            activePedBlip.Friendly = false;
                        }

                        // Escape by any means necessary
                        foreach (Ped officers in World.GetAllPeds(new Model("M_Y_COP")))
                        {
                            if (officers != null && officers.Exists())
                            {
                                myped.Task.FleeFromChar(officers);
                                myped.Task.FightAgainst(officers, -1);
                            }
                        }
                        foreach (Ped squads in World.GetAllPeds(new Model("M_Y_SWAT")))
                        {
                            if (squads != null && squads.Exists())
                            {
                                myped.Task.FleeFromChar(squads);
                                myped.Task.FightAgainst(squads, -1);
                            }
                        }

                        // Enable Pursuit AI
                        myped.DisablePursuitAI = false;

                        //Functions.SetPursuitAllowWeaponsForSuspects(this.pursuit, true);
                        myped.Task.AlwaysKeepTask = true;
                        myped.BlockPermanentEvents = true;
                        isRemainingTerroristsFled = true;
                    }
                    else continue;
                }
                catch (Exception ex)
                {
                    Log.Error("CombatOrders_RemainingTerrorists(): Failed to assign mission peds a specified task: " + ex, this);
                    isRemainingTerroristsFled = false;
                }

            Game.WaitInCurrentScript(300);
            Functions.PlaySoundUsingPosition("THIS_IS_CONTROL ASSISTANCE_REQUIRED FOR CRIM_A_SUSPECT_RESISTING_ARREST IN_OR_ON_POSITION", this.spawnPosition);
            Log.Debug("CombatOrders_RemainingTerrorists: Terrorists fled the scene", this);

            return isRemainingTerroristsFled;
        }

        /// <summary>
        /// Tracks the peds in current room/interior so it won't be glitchy (invisible).
        /// </summary>
        private void TrackPedsInPlayerRoom()
        {
            bool roomAccessChanged = false;
            if (LPlayer.LocalPlayer.Ped.CurrentRoom != lastPlayerRoom)
            {
                lastPlayerRoom = LPlayer.LocalPlayer.Ped.CurrentRoom;
                roomAccessChanged = true;
            }
            else return;
            foreach (LPed myped in this.missionPeds)
            {
                if (roomAccessChanged && myped != null) myped.CurrentRoom = LPlayer.LocalPlayer.Ped.CurrentRoom;
                if (myped.IsInjured || myped.HasBeenArrested || myped.IsDead) try
                    {
                        // deprecated
                        /*Blip metadata = myped.GPed.GetMetadata<Blip>("blip");
                        if (metadata != null) metadata.Delete();*/

                        bool isBlipShownOnTarget = myped.Blip != null || myped.Blip.Exists();
                        if (isBlipShownOnTarget) myped.Blip.Delete();
                    }
                    catch (Exception ex) { Log.Error("Cannot delete suspect blip: " + ex, this); }
                else continue;
            }
            foreach (LPed myped in this.deadSuspects)
            {
                if (roomAccessChanged && myped != null) myped.CurrentRoom = LPlayer.LocalPlayer.Ped.CurrentRoom;
                else continue;
            }
            foreach (LPed myped in this.arrestedSuspects)
            {
                if (roomAccessChanged && myped != null) myped.CurrentRoom = LPlayer.LocalPlayer.Ped.CurrentRoom;
                else continue;
            }
            foreach (Ped myped in this.hostages)
            {
                if (roomAccessChanged && myped != null) myped.CurrentRoom = LPlayer.LocalPlayer.Ped.CurrentRoom;
                if (myped.isInjured || myped.isDead)
                {
                    /*// Standard ped do not have option to grab a Blip instance, used Metadata instead
                    // Method is similar to the old NooseMod, but not for suspects
                    // TODO: Look for other alternative (to bypass deprecated method)
                    Blip metadata = myped.GetMetadata<Blip>("blip");
                    if (metadata != null) metadata.Delete();*/

                    Blip[] myBlips = Blip.GetAllBlipsOfType(BlipType.Ped);
                    foreach (Blip myBlip in myBlips)
                    {
                        if (myBlip.GetAttachedItem() == myped) try
                            {
                                if (myBlip != null) myBlip.Delete();
                            }
                            catch (Exception ex) { Log.Error("Cannot delete hostage blip: " + ex, this); continue; }
                        else continue;
                    }
                }
                else continue;
            }
            foreach (Ped myped in this.deadHostages)
            {
                if (roomAccessChanged && myped != null) myped.CurrentRoom = LPlayer.LocalPlayer.Ped.CurrentRoom;
                else continue;
            }
        }

        /// <summary>
        /// Action to delay remaining and alive terrorists to flee the scene before <see cref="CombatOrders_RemainingTerrorists"/> is issued.
        /// </summary>
        /// <param name="number_of_terrorists">Remaining terrorists that are alive</param>
        private void HoldUntilTerrorists_Left(int number_of_terrorists)
        {
            suspectsLeft = missionPeds.Count;
            try
            {
                foreach (LPed myped in missionPeds) try
                    {
                        if (myped != null)
                        {
                            if (myped.Exists() && myped.IsDead)
                            {
                                deadSuspects.Add(myped);
                                missionPeds.Remove(myped);
                            }
                            // At some point, NOOSE intimidation has a chance for the terrorist to surrender
                            else if (myped.Exists() && myped.IsAlive && myped.HasBeenArrested)
                            {
                                arrestedSuspects.Add(myped);
                                missionPeds.Remove(myped);
                            }
                        }
                        else
                        {
                            // Remove from the mission ped list if "glitched" or the LCPDFR code that executed on a dead suspect
                            missionPeds.Remove(myped);
                            continue;
                        }
                    }
                    catch (Exception ex) { Log.Error("Error getting available peds in list: " + ex, this); }
            }
            catch (Exception ex)
            {
                // Script continues to play even though the error is logged
                // This is only used for debugging purposes, normally it'll should terminate
                // (but tend to glitch the world with terrorists shooting wildly - should come in handy for arresting/killing, but not recorded on Stats)
                Log.Error("CRITICAL ERROR: Error accessing array list or trouble with peds in list: " + ex, this);
                //Log.Error("Safely aborting the script...", this); this.End();
            }
            if (suspectsLeft == number_of_terrorists || suspectsLeft <= number_of_terrorists)
            {
                isPursuitSessionActive = CombatOrders_RemainingTerrorists();
            }
        }

        /// <summary>
        /// Casualties number
        /// </summary>
        int officers_killed, squad_killed;

        /// <summary>
        /// Gets the number of casualties for Officers and Squad. Casualties will be count when an officer
        /// or a squad is either injured and incapable to continue a fight (KO) or dead.
        /// </summary>
        private void GetCasualties()
        {
            // Store values first
            officers_killed = 0;
            squad_killed = 0;

            try
            {
                foreach (LPed officer in PoliceOfficers)
                {
                    if (officer != null)
                    {
                        if (officer.IsDead || officer.IsInjured)
                        {
                            PoliceOfficers.Remove(officer);
                            officer.NoLongerNeeded();
                            officers_killed++;
                        }
                        else continue;
                    }
                    else // null peds should be removed as it cannot be accessed again
                    {
                        PoliceOfficers.Remove(officer);
                        continue;
                    }
                }
            }
            catch (Exception ex)
            {
                // Script continues to play even though the error is logged
                Log.Error("CRITICAL ERROR: Error accessing array list or trouble with peds in list: " + ex, this);
            }

            try
            {
                foreach (LPed squad_onduty in squad)
                {
                    if (squad != null)
                    {
                        if (squad_onduty.IsDead || squad_onduty.IsInjured)
                        {
                            squad.Remove(squad_onduty);
                            squad_onduty.NoLongerNeeded();
                            squad_killed++;
                        }
                        else continue;
                    }
                    else // null peds should be removed as it cannot be accessed again
                    {
                        squad.Remove(squad_onduty);
                        continue;
                    }
                }
            }
            catch (Exception ex)
            {
                // Script continues to play even though the error is logged
                Log.Error("CRITICAL ERROR: Error accessing array list or trouble with peds in list: " + ex, this);
            }

            // Count on how many hostages need to be evacuated (or at least not being shot at to death)
            // This affects your arrival (hostages being executed if NOOSE arrives late)
            if (hostages.Count != 0) try
                {
                    foreach (Ped hostage in hostages)
                    {
                        if (hostage != null)
                        {
                            if (hostage.isDead)
                            {
                                hostages.Remove(hostage);
                                deadHostages.Add(hostage);
                            }
                            else continue; // continue the list (which it should consider as hostage has been rescued)
                        }
                        else continue; // continue the list (which it should consider as hostage has been rescued)
                    }
                }
                catch (Exception ex)
                {
                    // Script continues to play even though the error is logged
                    Log.Error("CRITICAL ERROR: Error accessing array list or trouble with peds in list: " + ex, this);
                }

            Log.Debug("Casualties: Officers: " + officers_killed + ", Squad member: " + squad_killed + ", Hostages: " + deadHostages.Count, this);
        }

        /// <summary>
        /// Gets the number of arrested terrorists when <see cref="isPursuitSessionActive"/> returns true in the process.
        /// </summary>
        private void GetArrestedTerrorists()
        {
            if (isPursuitSessionActive && missionPeds.Count != 0) try
                {
                    foreach (LPed myped in missionPeds) try
                        {
                            if (myped != null || myped.Exists())
                            {
                                if (myped.IsAlive && myped.HasBeenArrested)
                                {
                                    arrestedSuspects.Add(myped);
                                    missionPeds.Remove(myped);
                                }
                                // whether IsDead return true and HasBeenArrested return false
                                else if (myped.IsDead && !myped.HasBeenArrested)
                                {
                                    deadSuspects.Add(myped);
                                    missionPeds.Remove(myped);
                                }
                            }
                            // If the suspect has lost visual and escaped
                            else missionPeds.Remove(myped);
                        }
                        catch (Exception ex) { Log.Error("Cannot get data on peds: " + ex, this); }
                }
                catch (Exception ex)
                {
                    // Script continues to play even though the error is logged
                    Log.Error("CRITICAL ERROR: Error accessing array list or trouble with peds in list: " + ex, this);
                }
        }

        /// <summary>
        /// Chance for terrorists to surrender when a player is aiming on a shooting/fighting terrorist.
        /// Note that it has a VERY LOW chance of surrender because they fear no death.
        /// </summary>
        private void NOOSEIntimidation() // The function is not yet in use (as much as I concern, this is beta)
        {
            // Get targeted ped
            LPed myped = LPlayer.LocalPlayer.GetTargetedPed();
            if (myped == null) return;
            else try
                {
                    // Add a chance for the target to surrender
                    bool chanceToSurrender = Common.GetRandomBool(0, Common.GetRandomValue(300, 999), 0);
                    foreach (LPed terrorist in missionPeds) try
                        {
                            if (terrorist != null || terrorist.Exists())
                            {
                                if (terrorist.Gender == Gender.Female)
                                    // Different sex allows NOOSE intimidation much easier
                                    chanceToSurrender = Common.GetRandomBool(0, Common.GetRandomValue(100, 400), 0);
                                if (chanceToSurrender == true && myped == terrorist)
                                    // Make him surrender
                                    terrorist.Surrender();
                                else continue;
                            }
                        }
                        catch (Exception ex) { Log.Error("Error in Intimidation process: " + ex, this); }
                }
                catch (Exception ex)
                {
                    // Script continues to play even though the error is logged
                    Log.Error("CRITICAL ERROR: Error accessing array list or trouble with peds in list: " + ex, this);
                }
        }
#endregion

        /// <summary>
        /// Called every tick to process all script logic. Call base when overriding.
        /// </summary>
        public override void Process()
        {
            base.Process();

            if (mission == MissionState.GoingToMissionLocation)
            {
                WaitingForPlayer(); return;
            }

            if (mission == MissionState.WaitForTeamInsertion)
            {
                DispatchTeam(); return;
            }

            if (mission == MissionState.Initialize)
            {
                WaitFor2ndTeam(); return;
            }

            if (mission == MissionState.DuringMission) TrackPedsInPlayerRoom();

            // Count on how many terrors left to be finished
            if (isPursuitSessionActive == false)
            {
                // Should between 1 and 3 because Intro only include 4 terrorists and 3 hostages
                // Frequent use of Common.GetRandomValue "might" be unstable; consider changing it?
                HoldUntilTerrorists_Left(Common.GetRandomValue(1, 3+1));
                return;
            }

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
                    ", Squad Casualties: " + squad_killed + ", Income for your noble service: $" + cashGained, 10000);

                Functions.AddTextToTextwall("Squad, terrorist threat is neutralized. Mission complete!", LPlayer.LocalPlayer.Username);
                Functions.AddTextToTextwall(Functions.GetStringFromLanguageFile("CALLOUT_SHOOTOUT_END_TW"), Functions.GetStringFromLanguageFile("POLICE_SCANNER_CONTROL"));

                // Add cash
                LPlayer.LocalPlayer.Money += cashGained;

                // Write stats
                WriteStatistics(deadSuspects.Count, arrestedSuspects.Count, deadHostages.Count, hostages.Count, officers_killed, squad_killed, cashGained);

                // Save the progress
                SaveGame();

                Log.Debug("Process: Commands properly assigned", this);

                /*// End everything
                PlaySound(false);
                PlayFinishedMissionSound();
                SetCalloutFinished(true, false, false);*/
                SetCalloutFinished(true, true, false);
                End();
            }
		}

        /// <summary>
        /// Put all resource free logic here. This is either called by the calloutmanager to shutdown the callout or can be called by the 
        /// callout itself to execute the cleanup code. Call base to set state to None.
        /// </summary>
        public override void End()
        {
            base.End();

            try
            {
                // Delete hostages count (this is because the hostage is assigned without using LPed)
                if (hostages.Count != 0)
                {
                    for (int i = 0; i < hostages.Count; i++)
                    {
                        hostages[i].NoLongerNeeded();
                        continue;
                    }
                }

                if (deadHostages.Count != 0)
                {
                    for (int i = 0; i < deadHostages.Count; i++)
                    {
                        deadHostages[i].NoLongerNeeded();
                        continue;
                    }
                }

                foreach (LPed myped in deadSuspects)
                {
                    Functions.SetPedIsOwnedByScript(myped, this, false);
                    myped.NoLongerNeeded();
                }

                foreach (LPed myped in arrestedSuspects)
                {
                    Functions.SetPedIsOwnedByScript(myped, this, false);
                    myped.NoLongerNeeded();
                }

                foreach (LPed myped in PoliceOfficers)
                {
                    Functions.SetPedIsOwnedByScript(myped, this, false);
                    myped.NoLongerNeeded();
                }

                LHandle partnerManager = Functions.GetCurrentPartner();
                LPed[] partners = Functions.GetPartnerPeds(partnerManager);

                foreach (LPed myped in squad)
                {
                    foreach (LPed partner in partners) if (myped == partner)
                            Functions.SetPedIsOwnedByScript(myped, this, false);
                        else
                        {
                            Functions.SetPedIsOwnedByScript(myped, this, false);
                            myped.NoLongerNeeded();
                        }
                }
            }
            catch (Exception ex) { Log.Error("Cannot execute cleanup on peds: " + ex, this); }

            // Clear list
            deadSuspects.Clear(); deadHostages.Clear(); hostages.Clear(); arrestedSuspects.Clear(); missionPeds.Clear(); squad.Clear(); PoliceOfficers.Clear();
            Log.Debug("List Cleared!",this);

            // Reset enum
            mission = MissionState.OnDuty;
            LPlayer.LocalPlayer.Ped.PriorityTargetForEnemies = false;

            // End pursuit if still running
            if (this.pursuit != null)
            {
                Log.Debug("End(): a pursuit is still running, destroying...", this);
                Functions.ForceEndPursuit(this.pursuit);
            }

            // If there are still blips to target location (usually because of exception), delete it
            if (controller.entryLoc != null || controller.entryLoc.Exists()) controller.entryLoc.Delete();
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
            //Functions.RemoveFromDeletionList(ped, this);

            try
            {
                foreach (LPed myped in arrestedSuspects) if (myped == ped)
                    {
                        Functions.SetPedIsOwnedByScript(ped, this, false);
                        ped.NoLongerNeeded();
                        continue;
                    }
                    else continue;
                foreach (LPed myped in deadSuspects) if (myped == ped)
                    {
                        Functions.SetPedIsOwnedByScript(ped, this, false);
                        ped.NoLongerNeeded();
                        continue;
                    }
                    else continue;
            }
            catch (Exception ex)
            {
                // Script continues to play even though the error is logged
                Log.Error("CRITICAL ERROR: Error accessing array list or trouble with peds in list: " + ex, this);
            }
        }
    }
}
