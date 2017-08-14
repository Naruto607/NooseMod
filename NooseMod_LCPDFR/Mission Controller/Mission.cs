using GTA;
using LCPD_First_Response.Engine;
using LCPD_First_Response.LCPDFR.API;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
namespace NooseMod_LCPDFR.Mission_Controller
{
    /// <summary>
    /// Class that controls the number of missions available
    /// </summary>
	internal class Mission
	{
        /// <summary>
        /// The name of a mission
        /// </summary>
		private string name;

        /// <summary>
        /// Spawn location
        /// </summary>
		private Vector3 location;

        /// <summary>
        /// Hostage locations
        /// </summary>
        //private List<Vector3> hostageLocations = new List<Vector3>(2 ^ SettingsFile.Open("NooseMod.ini").GetValueInteger("HostagePeds", "ListRanging", 5));
        private List<Vector3> hostageLocations = new List<Vector3>();

        /// <summary>
        /// Suspect (Terrorist) locations
        /// </summary>
        //private List<Vector3> suspectLocations = new List<Vector3>(2 ^ SettingsFile.Open("NooseMod.ini").GetValueInteger("MissionPeds", "ListRanging", 5));
        private List<Vector3> suspectLocations = new List<Vector3>();

        /// <summary>
        /// Mission Time
        /// </summary>
		private TimeSpan missionTime = TimeSpan.Zero;

        /// <summary>
        /// Checks whether the Mission Time is specified
        /// </summary>
		private bool isMissionTimeSpecified;

        /// <summary>
        /// Object that is sending (to prevent log details being null)
        /// </summary>
        private object missionObj = "NooseMod.Mission";

        /// <summary>
        /// Gets the name of a mission.
        /// </summary>
		public string Name
		{
			get
			{
				return this.name;
			}
		}

        /// <summary>
        /// Gets the coordinates of the mission location.
        /// </summary>
		public Vector3 Location
		{
			get
			{
				return this.location;
			}
		}

        /// <summary>
        /// Gets the location and number of hostages to be spawned.
        /// </summary>
		public List<Vector3> HostageLocations
		{
			get
			{
				return this.hostageLocations;
			}
		}

        /// <summary>
        /// Gets the location and number of suspects (terrorists) to be spawned.
        /// </summary>
		public List<Vector3> SuspectLocations
		{
			get
			{
				return this.suspectLocations;
			}
		}

        /// <summary>
        /// Gets whether the mission time is specified.
        /// </summary>
		public bool IsMissionTimeSpecified
		{
			get
			{
				return this.isMissionTimeSpecified;
			}
		}

        /// <summary>
        /// Gets the mission time.
        /// </summary>
		public TimeSpan MissionTime
		{
			get
			{
				return this.missionTime;
			}
		}

        /// <summary>
        /// Creates a new instance of <see cref="Mission"/> class.
        /// The new instance will read a mission text file and stores information of it.
        /// </summary>
        /// <param name="file">File name</param>
		public Mission(string file)
		{
			StreamReader streamReader = new StreamReader(file);
			this.name = streamReader.ReadLine();
			string text = streamReader.ReadLine();
			if (!string.IsNullOrEmpty(text))
			{
				this.missionTime = TimeSpan.Parse(text);
				this.isMissionTimeSpecified = true;
			}
			this.location = this.ParseVector3(streamReader.ReadLine());
			bool flag = false;
			while (!streamReader.EndOfStream)
			{
				string text2 = streamReader.ReadLine();
				if (text2 == "hostages")
				{
					flag = true;
				}
				else
				{
					if (flag)
					{
						this.hostageLocations.Add(this.ParseVector3(text2));
					}
					else
					{
						this.suspectLocations.Add(this.ParseVector3(text2));
					}
				}
			}
			streamReader.Close();
		}

        /// <summary>
        /// Reads the current mission in play and returns an information of an object.
        /// This includes: mission name, entry point + location name in GTA, number of suspects, and number of hostages.
        /// </summary>
        /// <returns>A <see cref="string"/> representation of object</returns>
		public override string ToString()
		{
			return string.Concat(new string[]
			{
                // Mission name
				this.name,
				" (Entry: ",
                // Map coords and area name (LCPDFR feature)
				this.location.ToString() + " [" + Functions.GetAreaStringFromPosition(this.location) + "]",
				"): ",
                // Number of terrorists/suspects
				this.suspectLocations.Count.ToString(),
				" suspect(s), ",
                // Number of hostages
				this.hostageLocations.Count.ToString(),
				" hostage(s)"
			});
		}

        /// <summary>
        /// Parses the contained string (text) into <see cref="GTA.Vector3"/> format (coordinates in X, Y, and Z).
        /// </summary>
        /// <param name="str">The string of the contained text</param>
        /// <returns><see cref="GTA.Vector3"/> format</returns>
		private Vector3 ParseVector3(string str)
		{
			string[] array = str.Split(new char[] { ';' });

            // Changed unequal to less than, should further-proof skipping through array more than 3
            // Nice try, _hax!
			if (array.Length < 3)
            {
                Log.Error("ParseVector3(string str): Invalid Vector3 format", missionObj);
				throw new Exception("Invalid Vector3 format");
			}

                // Method will not stop parsing if detected more than 3 arrays (typo?).
                // This is improved from the original NooseMod so that even though 4 "splits" defined per line,
                //  method will only read the first three rather than skipping it (throwing Exception).
                // 4 "splits" can be defined into Vector4 format, which the fourth "split" will be direction (heading).
            else if (array.Length > 3)
                Log.Warning("ParseVector3(string str): Detected more than 3 arrays in text - parsed only 3", missionObj);
			return new Vector3(this.Str2Float(array[0]), this.Str2Float(array[1]), this.Str2Float(array[2]));
		}

        /// <summary>
        /// Converts <see cref="System.Single"/> value to a <see cref="System.String"/> format
        /// </summary>
        /// <param name="f"><see cref="System.Single"/> value</param>
        /// <returns><see cref="System.String"/> value</returns>
		private string Float2Str(float f)
		{
			return f.ToString(CultureInfo.GetCultureInfo("en-US"));
		}

        /// <summary>
        /// Converts <see cref="System.String"/> value to a <see cref="System.Single"/> format
        /// </summary>
        /// <param name="str"><see cref="System.String"/> (text)</param>
        /// <returns><see cref="System.Single"/> value</returns>
		private float Str2Float(string str)
		{
			return float.Parse(str, CultureInfo.GetCultureInfo("en-US"));
		}
	}
}
