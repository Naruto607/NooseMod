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

using GTA;
using LCPD_First_Response.Engine;
using LCPD_First_Response.Engine.Scripting.Entities;
using LCPD_First_Response.LCPDFR.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// This script is currently incomplete and still contains errors. Many things are not yet implemented due to the quick learning.
// If you instate this in Functions.AddWorldEvent(typeof(ProvostPatrol), "Provost Patrol"); (removing commentary there in Main.cs), it may result in plugin error in game with LCPDFR running.
namespace NooseMod_LCPDFR.World_Events
{
    /// <summary>
    /// The purpose of this World Events Script is to add NOOSE cruisers and Patriots roaming around Liberty City.
    /// It is somewhat like NOOSE officers monitor the cops from doing illegal activities or the city from threats, having it similar to Provost in Indonesia.
    /// </summary>
    public class ProvostPatrol : WorldEvent
    {
        /// <summary>
        /// NOOSE members that will be created
        /// </summary>
        private LPed[] SWATMembers;

        // Using CModelInfo - look for model name and its hash here: https://www.gtamodding.com/wiki/List_of_models_hashes
        /// <summary>
        /// Model Information of NOOSE/SWAT trooper using LCPDFR basis
        /// </summary>
        private CModel SWATTrooper = new CModel(new CModelInfo("M_Y_SWAT", 3290204350, EModelFlags.IsNoose));

        /// <summary>
        /// The vehicle
        /// </summary>
        private LVehicle NooseVeh;

        /// <summary>
        /// The Player in LCPDFR game
        /// </summary>
        private LPlayer lcpdfrPlayer;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProvostPatrol"/> class. Don't put any logic here, but use <see cref="Initialize"/> instead.
        /// This is because the scenario manager will create instances of this class to call <see cref="CanStart"/>.
        /// </summary>
        public ProvostPatrol() : base("Provost Patrol") { }

        /// <summary>
        /// Locations where the Provost Patrol event starts
        /// </summary>
        private Vector3[] possibleNooseLocations = new Vector3[10] { };

        /// <summary>
        /// Called right after a world event was started.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            NooseVeh = new LVehicle(lcpdfrPlayer.Ped.GetOffsetPosition(new Vector3(90.0f, 90.0f, 0)), "NOOSE");
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
                SWATMembers[i].ChangeRelationship(RelationshipGroup.Cop, Relationship.Respect);
                SWATMembers[i].ChangeRelationship(RelationshipGroup.Criminal, Relationship.Dislike);
                SWATMembers[i].ChangeRelationship(RelationshipGroup.Gang_Biker1, Relationship.Dislike);
                SWATMembers[i].ChangeRelationship(RelationshipGroup.Gang_Biker2, Relationship.Dislike);
                SWATMembers[i].Accuracy = Common.GetRandomValue(75, 99);
                SWATMembers[i].Armor = 100;
                SWATMembers[i].Weapons.RemoveAll();
                SWATMembers[i].Weapons.FromType(Weapon.Melee_Knife);
                SWATMembers[i].Weapons.FromType(Weapon.Handgun_DesertEagle).Ammo = 999;
                SWATMembers[i].Weapons.FromType(Weapon.Shotgun_Baretta).Ammo = 999;
                SWATMembers[i].Weapons.FromType(Weapon.Rifle_M4).Ammo = 999;
                SWATMembers[i].Weapons.FromType(Weapon.Thrown_Grenade).Ammo = Common.GetRandomValue(1,3);
                SWATMembers[i].Weapons.Select(Weapon.Rifle_M4);
                Functions.SetPedIsOwnedByScript(SWATMembers[i], this, true);
            }
            NooseVeh.DisablePullover = true;
            NooseVeh.AllowSirenWithoutDriver = true;
        }

        
        /// <summary>
        /// Processes the main logic.
        /// </summary>
        public override void Process()
        {
            base.Process();

            // Grab current vehicle data from the first spawned NOOSE/SWAT member and assign it to cruise drive.
            if (NooseVeh.Exists() && NooseVeh.IsDriveable && SWATMembers[0].IsInVehicle(NooseVeh))
            {
                Vehicle currentVeh = SWATMembers[0].CurrentVehicle;
                SWATMembers[0].Task.CruiseWithVehicle(currentVeh, 40.0f, true);
            }
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
        /// Called when a world event should be disposed. This is also called when <see cref="WorldEvent.CanBeDisposedNow"/> returns false.
        /// </summary>
        public override void End()
        {
            base.End();

            foreach (LPed myped in SWATMembers)
            {
                myped.NoLongerNeeded();
                myped.Delete();
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
            // TODO: These personnel should appear in Bohan, southern Algonquin, airport, and Alderney
            //throw new NotImplementedException();
            bool AllSet = new bool();

            for (int i = 0; i <= possibleNooseLocations.Length; i++)
            {
                if (lcpdfrPlayer.Ped.Position.DistanceTo(possibleNooseLocations[i]) < 300.0f)
                {
                    AllSet = true;
                }
                else { AllSet = false; }
            }

            return AllSet;
        }

        /// <summary>
        /// Gets a string representation of this event. Call Base to get the string.
        /// </summary>
        /// <returns>A string that represents current object</returns>
        public override string ToString()
        {
            return base.ToString();
        }
    }
}
