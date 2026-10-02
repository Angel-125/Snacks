/**
The MIT License (MIT)
Copyright (c) 2014-2019 by Michael Billard
 

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
 * */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Snacks
{
    /// <summary>
    /// When a kerbal goes on EVA, take this resource along and remove a corresponding amount from the vessel. Use the SNACKS_EVA_RESOURCE to define the resource to add.
    /// </summary>
    public class SnacksEVAResource
    {
        #region Fields
        /// <summary>
        /// Name of the resource
        /// </summary>
        public string resourceName = string.Empty;

        /// <summary>
        /// Amount to add
        /// </summary>
        public double amount;

        /// <summary>
        /// Max amount possible
        /// </summary>
        public double maxAmount;

        /// <summary>
        /// When the resource amount drops to or below this value, display the warning message.
        /// </summary>
        public double warningAmount;

        /// <summary>
        /// Message to display when the kerbal's resource has dropped to or below the warningAmount.
        /// </summary>
        public string warningMessage = string.Empty;
        #endregion

        #region Housekeeping
        /// <summary>
        /// The EVA resource that defines how many Snacks the kerbal gets. We track this so that we can update its amount and max amount based on game settings.
        /// </summary>
        static public SnacksEVAResource snacksEVAResource;
        #endregion

        #region API
        /// <summary>
        /// Loads the SNACKS_EVA_RESOURCE config nodes, if any, and returns SnacksEVAResource objects.
        /// </summary>
        /// <returns>A list of SnacksEVAResource objects.</returns>
        public static List<SnacksEVAResource> LoadEVAResources()
        {
            ConfigNode[] evaResourceNodes = GameDatabase.Instance.GetConfigNodes("SNACKS_EVA_RESOURCE");
            ConfigNode node;
            List<SnacksEVAResource> resourceList = new List<SnacksEVAResource>();
            SnacksEVAResource resource;

            for (int index = 0; index < evaResourceNodes.Length; index++)
            {
                node = evaResourceNodes[index];

                resource = new SnacksEVAResource();
                if (node.HasValue("resourceName"))
                    resource.resourceName = node.GetValue("resourceName");

                if (node.HasValue("warningAmount"))
                    double.TryParse(node.GetValue("warningAmount"), out resource.warningAmount);

                if (node.HasValue("warningMessage"))
                    resource.warningMessage = node.GetValue("warningMessage");

                //We use the game settings for Snacks.
                if (resource.resourceName == SnacksProperties.SnacksResourceName)
                {
                    resource.amount = SnacksProperties.SnacksPerMeal;
                    resource.maxAmount = SnacksProperties.SnacksPerMeal;
                    snacksEVAResource = resource;
                }
                else
                {
                    if (node.HasValue("amount"))
                        double.TryParse(node.GetValue("amount"), out resource.amount);

                    if (node.HasValue("maxAmount"))
                        double.TryParse(node.GetValue("maxAmount"), out resource.maxAmount);
                }

                resourceList.Add(resource);
            }

            return resourceList;
        }

        /// <summary>
        /// Handles the crew boarded event. The resource is removed from the kerbal and added to the vessel.
        /// </summary>
        /// <param name="evaKerbal">The kerbal that is returning from EVA</param>
        /// <param name="boardedPart">The part that the kerbal boarded</param>
        public void onCrewBoardedVessel(Part evaKerbal, Part boardedPart)
        {
        }

        /// <summary>
        /// Handles the crew eva event. The kerbal gains the EVA resource and the vessel loses a corresponding amount.
        /// </summary>
        /// <param name="evaKerbal">The kerbal that went on EVA</param>
        /// <param name="partExited">The part that the kerbal exited</param>
        public void onCrewEVA(Part evaKerbal, Part partExited)
        {
            // Get the astronaut's roster
            ProtoCrewMember astronaut = evaKerbal.vessel.GetVesselCrew()[0];
            AstronautData astronautData = SnacksScenario.Instance.GetAstronautData(astronaut);

            // Get the resource definition
            PartResourceDefinitionList definitions = PartResourceLibrary.Instance.resourceDefinitions;
            PartResourceDefinition def;

            if (string.IsNullOrEmpty(resourceName))
                return;
            if (!definitions.Contains(resourceName))
                return;
            def = definitions[resourceName];

            // See if we have the resource
            bool hasResource = astronautData.HasResource(resourceName);

            double packAmount = GetPackAmount(astronautData);
            if (packAmount <= 0)
                return;

            // Edge case: boardedPart is null, which indicates that the kerbal is already outside.
            // If we don't have the resource then we need to add it; kerbal gets a freebie here...
            if (partExited == null)
            {
                if (!hasResource)
                    astronautData.SetResourceAmounts(resourceName, packAmount, packAmount);
                return;
            }

            double astronautAmount = 0;
            double astronautMaxAmount = 0;
            if (hasResource)
                astronautData.GetResourceAmounts(resourceName, out astronautAmount, out astronautMaxAmount);
            double needed = Math.Max(0, packAmount - astronautAmount);
            double supplied = needed > 0 ? partExited.vessel.rootPart.RequestResource(def.id, needed) : 0;
            astronautData.SetResourceAmounts(resourceName, astronautAmount + supplied, Math.Max(packAmount, astronautAmount));
        }

        public double GetPackAmount(AstronautData astronautData)
        {
            if (resourceName != SnacksProperties.SnacksResourceName)
                return amount;
            return astronautData.isExempt ? 0 : amount * astronautData.SnacksMultiplier;
        }
        #endregion
    }
}
