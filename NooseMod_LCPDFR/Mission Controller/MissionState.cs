using System;
namespace NooseMod_LCPDFR.Mission_Controller
{
    /// <summary>
    /// The status (state) of a mission
    /// </summary>
	internal enum MissionState
	{
        /// <summary>
        /// Off: No callouts or at disabled state
        /// </summary>
		Off,
        /// <summary>
        /// On Duty: Ready to accept terrorist activity callout
        /// </summary>
		OnDuty,
        /// <summary>
        /// Going to Mission Location: Callout accepted, Player then dispatched to target location, Spawn necessities
        /// </summary>
		GoingToMissionLocation,
        /// <summary>
        /// Wait for Team Insertion: When near the crime scene, NOOSE and a backup are dispatched.
        /// </summary>
        WaitForTeamInsertion,
        /// <summary>
        /// During Mission: Shootout between NOOSE and terrorists
        /// </summary>
		DuringMission
        /*
        /// <summary>
        /// Engage Remaining Fleeing Terrorists: extra feature to allow remaining terrorists to flee the crime scene
        /// when condition permits
        /// </summary>
        EngageRemainingFleeingTerrors
         */
	}
}
