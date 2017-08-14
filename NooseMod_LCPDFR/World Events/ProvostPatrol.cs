//    NooseMod LCPDFR Plugin with Database System
//    Provost Patrol World Event script: adds Indonesian-styled random world event
//          that adds NOOSE cruisers and Patriots roaming around Liberty City
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
using LCPD_First_Response.Engine;
using LCPD_First_Response.Engine.Scripting.Entities;
using LCPD_First_Response.LCPDFR.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#endregion

namespace NooseMod_LCPDFR.World_Events
{
    /// <summary>
    /// A random world event that adds NOOSE cruisers and Patriots roaming around Liberty City.
    /// It is somewhat like NOOSE officers monitor the cops from doing illegal activities or the city from threats, having it similar to Provost in Indonesia.
    /// </summary>
    internal class ProvostPatrol : WorldEvent
    {
        #region Initialization
        /// <summary>
        /// NOOSE members that will be created
        /// </summary>
        private LPed[] SWATMembers;

        // Using CModelInfo - look for model name and its hash here: https://www.gtamodding.com/wiki/List_of_models_hashes
        /// <summary>
        /// Model Information of NOOSE/SWAT trooper using LCPDFR basis
        /// </summary>
        private CModel SWATTrooper = new CModel(new CModelInfo("M_Y_SWAT", 3290204350, EModelFlags.IsNoose));

        // Add a regular police car or Enforcer here if you like a diverse variety
        /// <summary>
        /// List of vehicle models used by NOOSE
        /// </summary>
        private string[] NooseVehModels = { "NOOSE", "POLPATRIOT" };

        /// <summary>
        /// The vehicle
        /// </summary>
        private LVehicle NooseVeh;

        /// <summary>
        /// The Player in LCPDFR game
        /// </summary>
        private LPlayer lcpdfrPlayer = new LPlayer();

        /// <summary>
        /// Initializes a new instance of the <see cref="ProvostPatrol"/> class. Don't put any logic here, but use <see cref="Initialize"/> instead.
        /// This is because the scenario manager will create instances of this class to call <see cref="CanStart"/>.
        /// </summary>
        public ProvostPatrol() : base("Provost Patrol") { }

        /// <summary>
        /// Locations where the Provost Patrol event starts
        /// </summary>
        private Vector3[] possibleNooseLocations = new Vector3[] 
        {
            // These locations are hand-picked by myself using screenshots with Coordinates enabled (Native Trainer).
            // You can add or remove locations as you wish.
            // When creating new coordinates, make sure the value MUST BE added "f" (no quotes or spaces) in the end.

            // Alderney
            new Vector3(-1151.71F,-375.44F,2.47F), // ASCF (prisoners on riot will be a high attention)
            new Vector3(-1164.79F,380.07F,3.86F), // Normandy
            new Vector3(-870.48F,1322.05F,21.46F), // Leftwood PD
            // Algonquin
            new Vector3(-196.73F,942.30F,10.18F), // Middle Park
            new Vector3(-204.67F,261.11F,14.36F), // Star Junction
            new Vector3(57.55F,-458.7F,14.28F), // Chinatown
            new Vector3(89.52F,-699.02F,14.27F), // Lawyer's Office (post-"The Interview" mission)
            // Bohan
            new Vector3(494.23F,1532.21F,12.84F), // Fortside
            new Vector3(990.38F,1882.32F,23.38F), // Northern Gardens PD
            // Broker
            new Vector3(2249.75F,466.83F,5.42F), // FIA
            new Vector3(1189.49F,198.50F,31.94F), // Schottler Medical Center (after the first mission with P-X)
            new Vector3(900.56F,-350.65F,17.16F), // Hove Beach PD
            new Vector3(775.15F,123.32F,5.41F) // BOABO
        };
        #endregion

        /// <summary>
        /// Called right after a world event was started.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            // Create patrolling NOOSE Squad
            NooseVeh = new LVehicle(lcpdfrPlayer.Ped.GetOffsetPosition(new Vector3(90.0f, 90.0f, 0)), Common.GetRandomCollectionValue<string>(NooseVehModels));
            NooseVeh.PlaceOnNextStreetProperly();
            NooseVeh.Wash();
            SWATMembers = new LPed[4] {
                NooseVeh.CreatePedOnSeat(VehicleSeat.Driver,SWATTrooper, RelationshipGroup.Cop),
                NooseVeh.CreatePedOnSeat(VehicleSeat.AnyPassengerSeat,SWATTrooper,RelationshipGroup.Cop),
                NooseVeh.CreatePedOnSeat(VehicleSeat.AnyPassengerSeat,SWATTrooper,RelationshipGroup.Cop),
                NooseVeh.CreatePedOnSeat(VehicleSeat.AnyPassengerSeat,SWATTrooper,RelationshipGroup.Cop)
            };
            for (int i = 0; i <= SWATMembers.Length; i++)
            {
                // Adjust relationship with environment
                SWATMembers[i].ChangeRelationship(RelationshipGroup.Cop, Relationship.Respect);
                SWATMembers[i].ChangeRelationship(RelationshipGroup.Criminal, Relationship.Dislike);
                SWATMembers[i].ChangeRelationship(RelationshipGroup.Gang_Biker1, Relationship.Dislike);
                SWATMembers[i].ChangeRelationship(RelationshipGroup.Gang_Biker2, Relationship.Dislike);

                // Some officers cannot shoot accurately
                SWATMembers[i].Accuracy = Common.GetRandomValue(75, 100);

                // Equip armor
                SWATMembers[i].Armor = 100;

                // Assign weapons
                SWATMembers[i].Weapons.RemoveAll();
                SWATMembers[i].Weapons.FromType(Weapon.Melee_Knife);
                SWATMembers[i].Weapons.FromType(Weapon.Handgun_DesertEagle).Ammo = 999;
                SWATMembers[i].Weapons.FromType(Weapon.Shotgun_Baretta).Ammo = 999;
                SWATMembers[i].Weapons.FromType(Weapon.Rifle_M4).Ammo = 999;
                SWATMembers[i].Weapons.FromType(Weapon.Thrown_Grenade).Ammo = Common.GetRandomValue(1,3);
                SWATMembers[i].Weapons.Select(Weapon.Rifle_M4);

                // Set ownership
                Functions.SetPedIsOwnedByScript(SWATMembers[i], this, true);
            }
            NooseVeh.DisablePullover = true;
            NooseVeh.AllowSirenWithoutDriver = true;

            // Grab current vehicle data from the first spawned NOOSE/SWAT member and assign it to cruise drive.
            if (NooseVeh.Exists() && NooseVeh.IsDriveable && SWATMembers[0].IsInVehicle(NooseVeh))
            {
                SWATMembers[0].Task.CruiseWithVehicle(NooseVeh, 40.0f, true);
            }
        }

        
        /// <summary>
        /// Processes the main logic.
        /// </summary>
        public override void Process()
        {
            base.Process();
        }

        /// <summary>
        /// Checks whether the world event can be disposed now, most likely because player got too far away.
        /// Returning true from here will call <see cref="End"/>.
        /// </summary>
        /// <returns>True if yes, false otherwise.</returns>
        public override bool CanBeDisposedNow()
        {
            return lcpdfrPlayer.Ped.Position.DistanceTo(SWATMembers[0].Position) > 420;
        }

        /// <summary>
        /// Called when a world event should be disposed.
        /// </summary>
        public override void End()
        {
            base.End();

            foreach (LPed myped in SWATMembers)
            {
                // If the ped is still owned
                if (Functions.IsStillControlledByScript(myped, this))
                {
                    // Release ownership
                    Functions.SetPedIsOwnedByScript(myped, this, false);

                    // Clear and delete
                    myped.NoLongerNeeded();
                    myped.Delete();
                }
            }
            NooseVeh.NoLongerNeeded();
            NooseVeh.Delete();
        }

        
        /// <summary>
        /// Called when a ped assigned to the current script has left the script due to a more important action, such as being arrested by the player.
        /// This is invoked right before control is granted to the new script, so perform all necessary freeing actions right here.
        /// </summary>
        /// <param name="ped">The ped</param>
        public override void PedLeftScript(LPed ped)
        {
            base.PedLeftScript(ped);

            // Since they are law enforcers, they can only be left when died
            for (int i = 0; i <= SWATMembers.Length; i++)
            {
                if (ped == SWATMembers[i])
                {
                    SWATMembers[i].Task.ClearAll();
                    SWATMembers[i].NoLongerNeeded();
                }
                //NooseVeh.NoLongerNeeded();
            }
        }

        /// <summary>
        /// Checks whether the world event can start at the position depending on where the NOOSE personnel can be seen.
        /// </summary>
        /// <param name="position">
        /// The position.
        /// </param>
        /// <returns>
        /// True if yes, otherwise false.
        /// </returns>
        public override bool CanStart(Vector3 position)
        {
            bool AllSet = new Boolean();

            // NOOSE Personnel can only spawn when within 300 (meters?) range.
            if (Common.GetRandomCollectionValue<Vector3>(possibleNooseLocations).DistanceTo(position) < 300f)
                AllSet = true;
            else AllSet = false;

            return AllSet;
        }
    }
}
