//    NooseMod LCPDFR Plugin with Database System - Object Validity Check
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

using GTA;
using LCPD_First_Response.LCPDFR.API;

namespace NooseMod_LCPDFR.Global_Controller
{
    /// <summary>
    /// Class that handles object validity check
    /// </summary>
    internal static class ValidityCheck
    {
        // Big regards to LtFlash for extension tricks (consider leaving him some cookies soon...)
        /// <summary>
        /// Checks if the specific object still exists in the world
        /// </summary>
        /// <param name="ped">GTA Ped</param>
        /// <returns>True if exist, otherwise false</returns>
        internal static bool isObjectValid(this LPed ped)
        {
            return ped != null && ped.Exists();
        }

        /// <summary>
        /// Checks if the specific object still exists in the world
        /// </summary>
        /// <param name="ped">LCPDFR Ped</param>
        /// <returns>True if exist, otherwise false</returns>
        internal static bool isObjectValid(this Ped ped)
        {
            return ped != null && ped.Exists();
        }

        /// <summary>
        /// Checks if the specific object still exists in the world
        /// </summary>
        /// <param name="ped">LCPDFR Vehicle</param>
        /// <returns>True if exist, otherwise false</returns>
        internal static bool isObjectValid(this LVehicle veh)
        {
            return veh != null && veh.Exists();
        }

        /// <summary>
        /// Checks if the specific object still exists in the world
        /// </summary>
        /// <param name="ped">GTA Vehicle</param>
        /// <returns>True if exist, otherwise false</returns>
        internal static bool isObjectValid(this Vehicle veh)
        {
            return veh != null && veh.Exists();
        }

        /// <summary>
        /// Checks if the specific object still exists in the world
        /// </summary>
        /// <param name="ped">GTA Object</param>
        /// <returns>True if exist, otherwise false</returns>
        internal static bool isObjectValid(this GTA.@base.Object obj)
        {
            return obj != null && obj.Exists();
        }

        /// <summary>
        /// Checks if the specific object still exists in the world
        /// </summary>
        /// <param name="ped">System Object?</param>
        /// <returns>True if exist, otherwise false</returns>
        internal static bool isObjectValid(this System.Object obj)
        {
            return obj != null;
        }

        /// <summary>
        /// Checks if the specific object still exists in the world
        /// </summary>
        /// <param name="ped">GTA Blip</param>
        /// <returns>True if exist, otherwise false</returns>
        internal static bool isObjectValid(this GTA.Blip blip)
        {
            return blip != null && blip.Exists();
        }
    }
}
