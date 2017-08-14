using System;
namespace NooseMod_LCPDFR.Mission_Controller
{
    /// <summary>
    /// The status (state) of a mission
    /// </summary>
    [Flags]
    internal enum MissionState
    {
        /// <summary>
        /// Off: No callouts or at disabled state
        /// </summary>
        Off = 0,

        /// <summary>
        /// On Duty: Ready to accept terrorist activity callout
        /// </summary>
        OnDuty = 2^0,

        /// <summary>
        /// Going to Mission Location: Callout accepted, Player then dispatched to target location, Spawn necessities
        /// </summary>
        GoingToMissionLocation = 2^1,

        /// <summary>
        /// Wait for Team Insertion: When near the crime scene, NOOSE and a backup are dispatched.
        /// </summary>
        WaitForTeamInsertion = 2^2,

        /// <summary>
        /// Initialize: Register Pursuit
        /// </summary>
        Initialize = 2^3,

        /// <summary>
        /// During Mission: Shootout between NOOSE and terrorists
        /// </summary>
        DuringMission = 2^4
    }
}
