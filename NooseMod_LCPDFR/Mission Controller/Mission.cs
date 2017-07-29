using GTA;
using LCPD_First_Response.Engine;
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
        /// Gets the name of a mission
        /// </summary>
		public string Name
		{
			get
			{
				return this.name;
			}
		}

        /// <summary>
        /// Gets the coordinates of the mission location
        /// </summary>
		public Vector3 Location
		{
			get
			{
				return this.location;
			}
		}

        /// <summary>
        /// Gets the location and number of hostages to be spawned
        /// </summary>
		public List<Vector3> HostageLocations
		{
			get
			{
				return this.hostageLocations;
			}
		}

        /// <summary>
        /// Gets the location and number of suspects (terrorists) to be spawned
        /// </summary>
		public List<Vector3> SuspectLocations
		{
			get
			{
				return this.suspectLocations;
			}
		}

        /// <summary>
        /// Gets whether the mission time is specified
        /// </summary>
		public bool IsMissionTimeSpecified
		{
			get
			{
				return this.isMissionTimeSpecified;
			}
		}

        /// <summary>
        /// Gets the mission time
        /// </summary>
		public TimeSpan MissionTime
		{
			get
			{
				return this.missionTime;
			}
		}

        /// <summary>
        /// Creates a new instance of <see cref="Mission"/> class
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
        /// Overrides <see cref="ToString"/> method into a readable text
        /// </summary>
        /// <returns>A <see cref="string"/> representation of the object</returns>
		public override string ToString()
		{
			return string.Concat(new string[]
			{
                // Mission name
				this.name,
				" (Entry: ",
                // Map coords and area name (LCPDFR feature)
				this.location.ToString() + " [" + LCPD_First_Response.LCPDFR.API.Functions.GetAreaStringFromPosition(this.location) + "]",
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
        /// Parses the contained string into <see cref="GTA.Vector3"/> format
        /// </summary>
        /// <param name="str">The string of the contained text</param>
        /// <returns><see cref="GTA.Vector3"/> format</returns>
		private Vector3 ParseVector3(string str)
		{
			string[] array = str.Split(new char[]
			{
				';'
			});
			if (array.Length != 3)
			{
				//throw new Exception("Invalid Vector3 format");
                Log.Error("Invalid Vector3 format", this);
			}
			return new Vector3(this.Str2Float(array[0]), this.Str2Float(array[1]), this.Str2Float(array[2]));
		}

        /// <summary>
        /// Converts Float number to a String
        /// </summary>
        /// <param name="f">The float number</param>
        /// <returns>A <see cref="string"/> value</returns>
		private string Float2Str(float f)
		{
			return f.ToString(CultureInfo.GetCultureInfo("en-US"));
		}

        /// <summary>
        /// Converts String to a Float number
        /// </summary>
        /// <param name="str">The string (text)</param>
        /// <returns><see cref="System.Single"/> value</returns>
		private float Str2Float(string str)
		{
			return float.Parse(str, CultureInfo.GetCultureInfo("en-US"));
		}
	}
}
