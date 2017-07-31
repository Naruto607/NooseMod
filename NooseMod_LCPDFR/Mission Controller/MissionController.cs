#region Uses
using GTA;
using LCPD_First_Response.Engine;
using LCPD_First_Response.Engine.Scripting.Plugins;
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
    public class MissionController
    {
        // Initialize
        #region Initialization
        /// <summary>
        /// Your last played mission
        /// </summary>
        public Int16 lastPlayedMission = -1;

        /// <summary>
        /// Missions that are loaded
        /// </summary>
        //public List<Mission> loadedMissions = new List<Mission>(2 ^ SettingsFile.Open("NooseMod.ini").GetValueInteger("Missions", "ListRanging", 3));
        internal List<Mission> loadedMissions = new List<Mission>();

        /// <summary>
        /// Entry Point blip (will be used together with roadblock positioning)
        /// </summary>
        public Blip entryLoc;

        /*
        /// <summary>
        /// The state of a mission
        /// </summary>
        private MissionState mission;
         * */
        #endregion

        // Code
        /// <summary>
        /// Constructs a new instance of <see cref="MissionController"/> class
        /// </summary>
        public MissionController()
        {

        }

        /// <summary>
        /// Assign a terrorist specified weapon (primary, secondary, and explosives are allowed)
        /// </summary>
        /// <param name="number">A number from the range of 1 to 9</param>
        /// <returns><see cref="GTA.Weapon"/></returns>
        public Weapon AssignTerroristWeapon(int number)
        {
            Weapon selectedWeap = new Weapon();
            switch (number)
            {
                case 1: selectedWeap = Weapon.Rifle_AK47; break; // AK rifle (seen in CS:GO)
                case 2: selectedWeap = Weapon.Shotgun_Basic; break; // Stub Shotgun (seen in CS:GO)
                case 3: selectedWeap = Weapon.Handgun_Glock; break; // Glock handgun (police issue)
                case 4: selectedWeap = Weapon.SMG_Uzi; break; // Uzi SMG
                case 5: selectedWeap = Weapon.TBOGT_AdvancedMG; break; // MG, if running TBoGT
                case 6: selectedWeap = Weapon.TBOGT_Pistol44; break; // .44A Handgun, if running TBoGT
                case 7: selectedWeap = Weapon.TLAD_Automatic9mm; break; // CZ75 (seen in CS:GO)
                case 8: selectedWeap = Weapon.TLAD_AssaultShotgun; break; // Striker shotgun
                case 9: selectedWeap = Weapon.Thrown_Grenade; break; // Hand Grenades
                default: throw new Exception("No weapons are assigned");
            }
            return selectedWeap;
        }

        /// <summary>
        /// Assign a Counter-Terrorist (NOOSE Squad) specified weapon (primary, secondary, and explosives are allowed)
        /// </summary>
        /// <param name="number">A number from the range of 1 to 9</param>
        /// <returns><see cref="GTA.Weapon"/></returns>
        public Weapon AssignCounterTerroristWeapon(int number)
        {
            Weapon selectedweap = new Weapon();
            switch (number)
            {
                case 1: selectedweap = Weapon.Handgun_Glock; break;
                case 2: selectedweap = Weapon.Handgun_DesertEagle; break;
                case 3: selectedweap = Weapon.Rifle_M4; break;
                case 4: selectedweap = Weapon.SMG_MP5; break;
                case 5: selectedweap = Weapon.SniperRifle_Basic; break;
                case 6: selectedweap = Weapon.TBOGT_AdvancedMG; break;
                case 7: selectedweap = Weapon.TBOGT_AssaultSMG; break;
                case 8: selectedweap = Weapon.TBOGT_NormalShotgun; break;
                case 9: selectedweap = Weapon.TBOGT_AdvancedSniper; break;
            }
            return selectedweap;
        }

        /// <summary>
        /// Runs the mission depending on the last saved game
        /// </summary>
        /// <param name="mission">The mission</param>
        /// <returns>Information on the mission it is played</returns>
        internal Mission PlayMission(int missionNumber)
        {
            Mission mission = loadedMissions[missionNumber];
            //TimeSpan missionTime = mission.MissionTime;
            //World.CurrentDayTime = mission.MissionTime;
            Blip spawnLocation = Blip.AddBlip(mission.Location);
            spawnLocation.Name = mission.Name;
            spawnLocation.RouteActive = true;
            spawnLocation.Color = BlipColor.Orange;
            entryLoc = spawnLocation;
            Functions.PrintText("Mission " + mission.Name + " started.", 3500);
            return mission;
        }

        /// <summary>
        /// Gather the mission data for use with the callout
        /// </summary>
        public void GetMissionData()
        {
            try
            {
                string[] files = Directory.GetFiles("LCPDFR\\Plugins\\NooseMod\\Missions"); int numOfFiles = 0; // Careful, this may include the template file
                do
                {
                    string file = files[numOfFiles];
                    loadedMissions.Add(new Mission(file));
                    numOfFiles++;
                } while (numOfFiles < files.Length);
                Log.Info("NooseMod successfully loaded " + this.loadedMissions.Count.ToString() + " missions into the list.", this);
            }
            // When an exception occured, log the error message
            catch (Exception ex) { Log.Error("Load failed: " + ex.Message, this); }
        }

        /// <summary>
        /// Used as a variable checkup, for example to acquire <see cref="Mission.missionTime"/> value
        /// </summary>
        /// <param name="missionNumber">Mission Number</param>
        /// <returns>Information on the mission it is loaded</returns>
        internal Mission missionVariableCheckup(int missionNumber)
        {
            return loadedMissions[missionNumber];
        }

        /// <summary>
        /// Loads last saved game
        /// </summary>
        public void LoadGame()
        {
            try
            {
                StreamReader streamReader = new StreamReader("LCPDFR\\Plugins\\NooseMod\\save.txt");
                this.lastPlayedMission = Int16.Parse(streamReader.ReadLine());
                streamReader.Close();

                // Debug message (testing only)
                Log.Debug("Game has been loaded with return value " + this.lastPlayedMission, this);
            }
            catch (Exception ex) { Log.Error("Unable to read or write save.txt: " + ex.Message, this); }
        }

        /// <summary>
        /// Detects if the Hardcore Mode is active
        /// </summary>
        /// <returns>True if active, false if otherwise</returns>
        public bool HardcoreModeIsActive()
        {
            // Though I personally hate it, there is no other way except accessing the LCPDFR Config File
            // through the SHDN as LCPDFR itself do not have a function to determine whether Hardcore Mode is active.
            bool Mode = SettingsFile.Open("LCPDFR\\LCPDFR.ini").GetValueBool("Enabled", "Hardcore", false);

            // Debug message (testing only)
            Log.Debug(string.Format(Functions.GetStringFromLanguageFile("LCPDMAIN_HARDMODE"), Mode), this);
            return Mode;
        }

        /// <summary>
        /// Loads a random criminal model name based on the settings file. If none, M_M_GUNNUT_01 will be returned as a default value.
        /// </summary>
        /// <returns>Model name of a criminal</returns>
        public string LoadRandomRegisteredPeds()
        {
            string[] ModelName = SettingsFile.Open("LCPDFR\\Plugins\\NooseMod.ini").
                GetValueString("CriminalModel", "WorldSettings", "M_M_GUNNUT_01").
                Split(new char[]
			{
				';'
			});
            int lastPointer = ModelName.Length;
            foreach (string text in ModelName)
            {
                if (text == null)
                {
                    Log.Warning("LoadRandomRegisteredPeds: Invalid entry", this);
                }
            }
            int randNum = 0;

            // Check if the length of the text is not null
            // When null, load on the pointer that is not null
                if (ModelName.Length != 0 && ModelName[lastPointer] != null)
                {
                    randNum = Common.GetRandomValue(0, ModelName.Length);
                }
                else if (ModelName.Length != 0 && ModelName[lastPointer] == null)
                {
                    int randNumPointer = ModelName.Length;
                    randNumPointer--;
                    randNum = Common.GetRandomValue(0, randNumPointer);
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
            string[] bikerModels_AOD;
            if (Game.CurrentEpisode == GameEpisode.TLAD)
            {
                bikerModels_AOD = new string[] { "M_M_GBIK_LO_03", "M_Y_GBIK02_LO_02", "M_Y_GBIK_HI_01", "M_Y_GBIK_HI_02", "M_Y_GANGELS_01",
                    "M_Y_GANGELS_02", "M_Y_GANGELS_03", "M_Y_GANGELS_04", "M_Y_GANGELS_05", "M_Y_GANGELS_06" };
            }
            else { bikerModels_AOD = new string[] { "M_M_GBIK_LO_03", "M_Y_GBIK02_LO_02", "M_Y_GBIK_HI_01", "M_Y_GBIK_HI_02" }; }
            //int randNum = Common.GetRandomValue(0, bikerModels_AOD.Length);
            //return bikerModels_AOD[randNum];
            return Common.GetRandomCollectionValue<string>(bikerModels_AOD);
        }

        /// <summary>
        /// Loads a random biker gang model name on The Lost MC side.
        /// </summary>
        /// <returns>Model name of a criminal</returns>
        public string LoadTLMCBikerGangPeds()
        {
            string[] bikerModels_TLMC;
            if (Game.CurrentEpisode == GameEpisode.TLAD)
            {
                bikerModels_TLMC = new string[] { "M_Y_GBIK_LO_01", "M_Y_GBIK_LO_02", "M_Y_GLOST_01", "M_Y_GLOST_02",
                    "M_Y_GLOST_03", "M_Y_GLOST_04", "M_Y_GLOST_05", "M_Y_GLOST_06", "LOSTBUDDY_01", "LOSTBUDDY_02", "LOSTBUDDY_03",
                    "LOSTBUDDY_04", "LOSTBUDDY_05", "LOSTBUDDY_06", "LOSTBUDDY_07", "LOSTBUDDY_08", "LOSTBUDDY_09", "LOSTBUDDY_10",
                    "LOSTBUDDY_11", "LOSTBUDDY_12", "LOSTBUDDY_13" };
            }
            else { bikerModels_TLMC = new string[] { "M_Y_GBIK_LO_01", "M_Y_GBIK_LO_02" }; }
            //int randNum = Common.GetRandomValue(0, bikerModels_TLMC.Length);
            //return bikerModels_TLMC[randNum];
            return Common.GetRandomCollectionValue<string>(bikerModels_TLMC);
        }

        /// <summary>
        /// Gets a string representation of this event.
        /// </summary>
        /// <returns>A string that represents current object</returns>
        public override string ToString()
        {
            return base.ToString();
        }
    }
}