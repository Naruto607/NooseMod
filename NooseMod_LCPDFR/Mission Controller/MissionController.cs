//    NooseMod LCPDFR Plugin with Database System - Controller Package
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

#region Uses
using GTA;
using LCPD_First_Response.Engine;
using LCPD_First_Response.LCPDFR.API;
using System;
using System.Collections.Generic;
using System.IO;
#endregion

namespace NooseMod_LCPDFR.Mission_Controller
{
    /// <summary>
    /// The controller to handle NooseMod missions, where it can load and save last game
    /// </summary>
    internal class MissionController
    {
        // Initialize
        #region Initialization
        /// <summary>
        /// Missions that are loaded
        /// </summary>
        //public List<Mission> loadedMissions = new List<Mission>(2 ^ SettingsFile.Open("NooseMod.ini").GetValueInteger("Missions", "ListRanging", 3));
        internal List<Mission> loadedMissions = new List<Mission>();

        /// <summary>
        /// Entry Point blip (will be used together with roadblock positioning)
        /// </summary>
        public Blip entryLoc;
        #endregion

        // Code
        /// <summary>
        /// Constructs a new instance of <see cref="MissionController"/> class.
        /// No logic has been put, since this is used to control the main callout script.
        /// </summary>
        internal MissionController() { }

        /// <summary>
        /// Assign a terrorist specified weapon (primary, secondary, and explosives are allowed),
        /// where this uses ALL weapons used in GTA.
        /// </summary>
        /// <param name="number">A number from the range of 1 to 9</param>
        /// <returns><see cref="GTA.Weapon"/></returns>
        public Weapon AssignTerroristWeapon(int number)
        {
            Weapon selectedWeap = new Weapon();
            try
            {
                switch (number)
                {
                    // Change your fav Terrorist weapons here.
                    // 1 to 4 are GTA IV weapons
                    case 1: selectedWeap = Weapon.Rifle_AK47; break; // AK rifle (seen in CS:GO)
                    case 2: selectedWeap = Weapon.Shotgun_Basic; break; // Stub/Sawed-off Shotgun (seen in CS:GO)
                    case 3: selectedWeap = Weapon.Handgun_Glock; break; // Glock handgun (police issue)
                    case 4: selectedWeap = Weapon.SMG_Uzi; break; // Uzi SMG

                    // 5 and 6 are TBoGT weapons (called when the Game Episode is TBoGT)
                    case 5: selectedWeap = Weapon.TBOGT_AdvancedMG; break; // MG, if running TBoGT
                    case 6: selectedWeap = Weapon.TBOGT_Pistol44; break; // .44A Handgun, if running TBoGT

                    // 7 and 8 are TLAD weapons (called when the Game Episode is TLAD)
                    case 7: selectedWeap = Weapon.TLAD_Automatic9mm; break; // CZ75 (seen in CS:GO)
                    case 8: selectedWeap = Weapon.TLAD_AssaultShotgun; break; // Striker shotgun

                    // 9 is throwable - you can change between Molotov and Grenade here
                    case 9: selectedWeap = Weapon.Thrown_Grenade; break; // Hand Grenades
                    default: throw new Exception("No weapons are assigned");
                }
            }
            catch (Exception ex)
            {
                // When the error caught, default it to no weapons (unarmed)
                Log.Error("Error getting weapon data: " + ex, this.ToString());
                Log.Error("Defaulting Weapon to Unarmed...", this.ToString());
                selectedWeap = Weapon.Unarmed;
            }
            return selectedWeap;
        }

        /// <summary>
        /// Assign a Counter-Terrorist (NOOSE Squad) specified weapon (primary, secondary, and explosives are allowed),
        /// where this uses ALL weapons used in GTA.
        /// </summary>
        /// <param name="number">A number from the range of 1 to 9</param>
        /// <returns><see cref="GTA.Weapon"/></returns>
        public Weapon AssignCounterTerroristWeapon(int number)
        {
            Weapon selectedWeap = new Weapon();
            try
            {
                switch (number)
                {
                    // Change your fav Counter-Terrorist weapons here.
                    // 1 to 5 are GTA IV weapons (also called when the Game Episode is TLAD,
                    // because TLAD weapons are Terrorist-specific weapons)
                    case 1: selectedWeap = Weapon.Handgun_Glock; break;
                    case 2: selectedWeap = Weapon.Handgun_DesertEagle; break;
                    case 3: selectedWeap = Weapon.Rifle_M4; break;
                    case 4: selectedWeap = Weapon.SMG_MP5; break;
                    case 5: selectedWeap = Weapon.SniperRifle_Basic; break;

                    // 6 to 9 are TBoGT weapons (called when the Game Episode is TBoGT)
                    case 6: selectedWeap = Weapon.TBOGT_AdvancedMG; break;
                    case 7: selectedWeap = Weapon.TBOGT_AssaultSMG; break;
                    case 8: selectedWeap = Weapon.TBOGT_NormalShotgun; break;
                    case 9: selectedWeap = Weapon.TBOGT_AdvancedSniper; break;
                    default: throw new Exception("No weapons are assigned");
                }
            }
            catch (Exception ex)
            {
                // When the error caught, default it to no weapons (unarmed)
                Log.Error("Error getting weapon data: " + ex, this.ToString());
                Log.Error("Defaulting Weapon to Unarmed...", this.ToString());
                selectedWeap = Weapon.Unarmed;
            }
            return selectedWeap;
        }

        /// <summary>
        /// Runs the mission depending on the last saved game.
        /// </summary>
        /// <param name="mission">The mission</param>
        /// <returns>Information on the mission it is played</returns>
        internal Mission PlayMission(int missionNumber)
        {
            Mission mission = loadedMissions[missionNumber];
            Blip spawnLocation = Blip.AddBlip(mission.Location);
            spawnLocation.Name = mission.Name;
            spawnLocation.RouteActive = true;
            spawnLocation.Color = BlipColor.Orange;
            entryLoc = spawnLocation;
            Functions.PrintText("Mission " + mission.Name + " started.", 3500);
            return mission;
        }

        /// <summary>
        /// Gather mission data for use with the callout and register it into the list.
        /// </summary>
        public void GetMissionData()
        {
            try
            {
                // Read the text files
                // Directory is relative to the game path where ScriptHookDotNet and LCPDFR are installed
                string[] files = Directory.GetFiles("LCPDFR\\Plugins\\NooseMod\\Missions");
                int numOfFiles = 0; // Careful, this may include the template file
                do
                {
                    string file = files[numOfFiles];
                    loadedMissions.Add(new Mission(file));
                    numOfFiles++;
                }
                while (numOfFiles < files.Length);
                Log.Info("NooseMod successfully loaded " + this.loadedMissions.Count.ToString() + " missions into the list.", this.ToString());
            }

            // When an exception occured, log the error message
            // Error may cause script to get ruined, because no missions are loaded
            catch (Exception ex) { Log.Error("Load failed: " + ex, this.ToString()); }
        }

        /// <summary>
        /// Gather mission data for use with the callout and register it into the list.
        /// NOTE: This method records no logs.
        /// </summary>
        public void GetMissionData_NoLog()
        {
            try
            {
                // Read the text files
                // Directory is relative to the game path where ScriptHookDotNet and LCPDFR are installed
                string[] files = Directory.GetFiles("LCPDFR\\Plugins\\NooseMod\\Missions"); int numOfFiles = 0; // Careful, this may include the template file
                do
                {
                    string file = files[numOfFiles];
                    loadedMissions.Add(new Mission(file));
                    numOfFiles++;
                } while (numOfFiles < files.Length);
            }

            // When an exception occured, log the error message
            // Error may cause script to get ruined, because no missions are loaded
            catch (Exception ex) { Log.Error("Load failed: " + ex, this.ToString()); }
        }

        /// <summary>
        /// Used as a mission variable checkup.
        /// This is used to satisfy the need of a callout or to check if the desired value condition is met.
        /// </summary>
        /// <param name="missionNumber">Mission Number</param>
        /// <returns>Information on the mission it is loaded</returns>
        internal Mission missionVariableCheckup(int missionNumber)
        {
            return loadedMissions[missionNumber];
        }

        /// <summary>
        /// Loads last saved game.
        /// </summary>
        /// <returns>Mission number that starts from -1</returns>
        public Int16 LoadGame()
        {
            Int16 tempVal = -1;
            try
            {
                StreamReader streamReader = new StreamReader("LCPDFR\\Plugins\\NooseMod\\save.txt");
                tempVal = Int16.Parse(streamReader.ReadLine());
                streamReader.Close();
                Log.Debug("Game has been loaded with return value " + tempVal, this.ToString());
            }
            catch (Exception ex) { Log.Error("Unable to read or write save.txt: " + ex, this.ToString()); }
            return tempVal;
        }

        /// <summary>
        /// Detects if the Hardcore Mode is active. LCPDFR uses Hardcore Mode that adds realism gameplay, and therefore
        /// NooseMod will prove even more difficult when it's active.
        /// </summary>
        /// <returns>True if active, false if otherwise</returns>
        public bool HardcoreModeIsActive()
        {
            // Though I personally hate it, there is no other way except accessing the LCPDFR Config File
            // through the SHDN as LCPDFR itself do not have a function to determine whether Hardcore Mode is active.
            // Directory is relative to the game path where ScriptHookDotNet and LCPDFR are installed
            return SettingsFile.Open("LCPDFR\\LCPDFR.ini").GetValueBool("Enabled", "Hardcore", false);
        }

        /// <summary>
        /// Loads a random criminal model name based on the settings file. If none, M_M_GUNNUT_01 will be returned as a default value.
        /// </summary>
        /// <returns>Model name of a criminal</returns>
        public string LoadRandomRegisteredPeds()
        {
            string[] ModelName = SettingsFile.Open("LCPDFR\\Plugins\\NooseMod.ini").
                GetValueString("CriminalModel", "WorldSettings", "M_M_GUNNUT_01;").
                Split(new char[] { ';' });
            int lastPointer = ModelName.Length - 1;
            foreach (string text in ModelName)
            {
                if (text == null || text == "")
                {
                    Log.Warning("LoadRandomRegisteredPeds(): Invalid entry", this.ToString());
                }
            }
            int randNum = 0;

            // Check if the length of the text is not null
            // When null, load on the pointer that is not null
            if (ModelName.Length != 0 && ModelName[lastPointer] != null && ModelName[lastPointer] != "")
            {
                randNum = Common.GetRandomValue(0, ModelName.Length);
            }
            else if (ModelName.Length != 0 && ModelName[lastPointer] == null // null identifier
                || ModelName[lastPointer] == "") // if no text (empty spaces may return an error)
            {
                int tempVar = ModelName.Length - 1;
                if (tempVar == 0) randNum = 0;
                else randNum = Common.GetRandomValue(0, tempVar);
            }
            else { randNum = 0; }

            return ModelName[randNum];
        }

        /// <summary>
        /// Loads a random biker gang model name on Angels of Death side.
        /// </summary>
        /// <returns>Model name of a criminal</returns>
        public string LoadAODBikerGangPeds()
        {
            // Note: Biker Gangs can affiliate with terrorists, so a likely chance a biker is accompanied by one or more terrorists.
            string[] bikerModels_AOD;
            if (Game.CurrentEpisode == GameEpisode.TLAD)
            {
                // When playing LCPDFR in TLAD platform, this will load standard and unique AOD Biker models.
                // You can try adding the females here (if you think ladies can afford a gunfight), see TLAD peds.ide for model names.
                bikerModels_AOD = new string[] { "M_M_GBIK_LO_03", "M_Y_GBIK02_LO_02", "M_Y_GBIK_HI_01", "M_Y_GBIK_HI_02", "M_Y_GANGELS_01",
                    "M_Y_GANGELS_02", "M_Y_GANGELS_03", "M_Y_GANGELS_04", "M_Y_GANGELS_05", "M_Y_GANGELS_06", "M_M_GUNNUT_01" };
            }
            // Otherwise standard AOD Biker models will be used. TBoGT do not have unique biker model.
            else { bikerModels_AOD = new string[] { "M_M_GBIK_LO_03", "M_Y_GBIK02_LO_02", "M_Y_GBIK_HI_01", "M_Y_GBIK_HI_02", "M_M_GUNNUT_01" }; }
            return Common.GetRandomCollectionValue<string>(bikerModels_AOD);
        }

        /// <summary>
        /// Loads a random biker gang model name on The Lost MC side.
        /// </summary>
        /// <returns>Model name of a criminal</returns>
        public string LoadTLMCBikerGangPeds()
        {
            // Note: Biker Gangs can affiliate with terrorists, so a likely chance a biker is accompanied by one or more terrorists.
            string[] bikerModels_TLMC;
            if (Game.CurrentEpisode == GameEpisode.TLAD)
            {
                // When playing LCPDFR in TLAD platform, this will load standard and unique TLMC models.
                // This may include Gang War buddies because TLAD is focused on TLMC theme (it do not include mission peds).
                // You can try adding the females here (if you think ladies can afford a gunfight), see TLAD peds.ide for model names.
                bikerModels_TLMC = new string[] { "M_Y_GBIK_LO_01", "M_Y_GBIK_LO_02", "M_Y_GLOST_01", "M_Y_GLOST_02",
                    "M_Y_GLOST_03", "M_Y_GLOST_04", "M_Y_GLOST_05", "M_Y_GLOST_06", "LOSTBUDDY_01", "LOSTBUDDY_02", "LOSTBUDDY_03",
                    "LOSTBUDDY_04", "LOSTBUDDY_05", "LOSTBUDDY_06", "LOSTBUDDY_07", "LOSTBUDDY_08", "LOSTBUDDY_09", "LOSTBUDDY_10",
                    "LOSTBUDDY_11", "LOSTBUDDY_12", "LOSTBUDDY_13", "M_M_GUNNUT_01" };
            }
            // Otherwise standard TLMC models will be used. TBoGT do not have unique biker model.
            else { bikerModels_TLMC = new string[] { "M_Y_GBIK_LO_01", "M_Y_GBIK_LO_02", "M_M_GUNNUT_01" }; }
            return Common.GetRandomCollectionValue<string>(bikerModels_TLMC);
        }

        /// <summary>
        /// Returns the definition of the <see cref="MissionController"/> class
        /// </summary>
        /// <returns>Long name of the class</returns>
        public override string ToString()
        {
            return "NooseMod Controller";
        }
    }
}