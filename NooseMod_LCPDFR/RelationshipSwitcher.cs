//    NooseMod LCPDFR Plugin with Database System
//    Relationship Switcher: Changes Partner's Relationship when you are assigned as a NOOSE/SWAT member
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
using LCPD_First_Response.Engine.Scripting.Plugins;
using LCPD_First_Response.LCPDFR.API;
using NooseMod_LCPDFR.Global_Controller;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#endregion

namespace NooseMod_LCPDFR
{
    /// <summary>
    /// Changes Partner's Relationship when you are assigned as a NOOSE/SWAT member
    /// </summary>
    [PluginInfo("NooseMod.RelationshipSwitcher", false, true)]
    public class RelationshipSwitcher : Plugin
    {
        #region Initialization
        /// <summary>
        /// Partners you have
        /// </summary>
        private LPed[] partners;

        /// <summary>
        /// Record of handle objects
        /// </summary>
        private bool[] Record = new Boolean[(long)2 ^ 32];

        /// <summary>
        /// Random number for <see cref="LPed.ComplianceChance"/>
        /// </summary>
        private Random random = new Random((int)2 ^ 8);
        #endregion

        /// <summary>
        /// Called when the plugin has been created successfully.
        /// </summary>
        public override void Initialize()
        {
            Log.Info("Relationship Switcher: Ready", this);
        }

        /// <summary>
        /// Called every tick to process all plugin logic.
        /// </summary>
        public override void Process()
        {
            // Detects if player is on duty and is using specified model used in NooseMod
            if (LPlayer.LocalPlayer.IsOnDuty)
                if (LPlayer.LocalPlayer.Skin.Model == new Model("M_Y_SWAT") || LPlayer.LocalPlayer.Skin.Model == new Model("M_Y_NHELIPILOT")) try
                    {
                        // Get Partners
                        LHandle partnerManager = Functions.GetCurrentPartner();
                        partners = Functions.GetPartnerPeds(partnerManager);
                        //Log.Debug("Partner Handle: " + partnerManager.ToString(), this);

                        // Make sure it is not null when processed
                        if (partners != null) foreach (LPed myped in partners)
                            {
                                //if (myped.Exists() && Record[myped.GetHashCode()] == false)
                                if (ValidityCheck.isObjectValid(myped) && Record[myped.GetHashCode()] == false)
                                {
                                    // Store the record so it won't be processed twice or more
                                    Record[myped.GetHashCode()] = true;

                                    // Change Relationship!
                                    myped.ChangeRelationship(RelationshipGroup.Player, Relationship.Companion);
                                    myped.ChangeRelationship(RelationshipGroup.Cop, Relationship.Companion);
                                    myped.ChangeRelationship(RelationshipGroup.Civillian_Male, Relationship.Like);
                                    myped.ChangeRelationship(RelationshipGroup.Civillian_Female, Relationship.Like);
                                    myped.ChangeRelationship(RelationshipGroup.Criminal, Relationship.Hate);
                                    myped.ChangeRelationship(RelationshipGroup.Fireman, Relationship.Like);
                                    myped.ChangeRelationship(RelationshipGroup.Medic, Relationship.Respect);
                                    myped.ChangeRelationship(RelationshipGroup.Dealer, Relationship.Dislike);
                                    myped.ChangeRelationship(RelationshipGroup.Gang_AfricanAmerican, Relationship.Hate);
                                    myped.ChangeRelationship(RelationshipGroup.Gang_Albanian, Relationship.Hate);
                                    myped.ChangeRelationship(RelationshipGroup.Gang_Biker1, Relationship.Hate);
                                    myped.ChangeRelationship(RelationshipGroup.Gang_Biker2, Relationship.Hate);
                                    myped.ChangeRelationship(RelationshipGroup.Gang_ChineseJapanese, Relationship.Hate);
                                    myped.ChangeRelationship(RelationshipGroup.Gang_Irish, Relationship.Hate);
                                    myped.ChangeRelationship(RelationshipGroup.Gang_Italian, Relationship.Hate);
                                    myped.ChangeRelationship(RelationshipGroup.Gang_Jamaican, Relationship.Hate);
                                    myped.ChangeRelationship(RelationshipGroup.Gang_Korean, Relationship.Hate);
                                    myped.ChangeRelationship(RelationshipGroup.Gang_PuertoRican, Relationship.Hate);
                                    myped.ChangeRelationship(RelationshipGroup.Gang_Russian1, Relationship.Hate);
                                    myped.ChangeRelationship(RelationshipGroup.Gang_Russian2, Relationship.Hate);
                                    myped.ChangeRelationship(RelationshipGroup.Special, Relationship.Hate);
                                    myped.ChangeRelationship(RelationshipGroup.Prostitute, Relationship.Neutral);

                                    // Make your partner can switch weapons from time to time (like ContactAction ASI plugin)
                                    myped.CanSwitchWeapons = true;

                                    // Allow drive-by when being shot at so they can return fire
                                    myped.WillDoDrivebys = true;

                                    // Should the partners have a chance to comply like suspects?
                                    // TODO: Remove me
                                    try
                                    {
                                        myped.ComplianceChance = random.Next(100);
                                    }
                                    catch (ArgumentOutOfRangeException)
                                    {
                                        Log.Error("Value is out of range, defaulting...", this);
                                        myped.ComplianceChance = 50;
                                    }

                                    Log.Info("Partner's custom relationship set!", this);
                                }
                            }
                    }
                    catch (Exception ex)
                    {
                        Log.Error("Failed setting up relationship data with partner: " + ex, this);
                        Functions.PrintHelp("Relationship Switcher encountered an error, check LCPDFR.log for details.");
                    }
                else return;
            else return;
        }

        /// <summary>
        /// Called when the plugin is being disposed, e.g. because an unhandled exception occured in Process. Free all resources here!
        /// </summary>
        public override void Finally()
        {
        }
    }
}
