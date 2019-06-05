﻿/**
The MIT License (MIT)
Copyright (c) 2014-2019 by Michael Billard
Original concept by Troy Gruetzmacher

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
    /// When a part with crew capacity is loaded in the editor and it lacks this resource, or when a vessel is loaded into the scene and its parts with crew capacity lack this resource, 
    /// add it to the part. Doesn’t apply to kerbals going on EVA. Use SNACKS_EVA_RESOURCE for that. Use the SNACKS_PART_RESOURCE to define resources to add.
    /// </summary>
    public class SnacksPartResource
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
        /// Parts with at least one of the modules on this list affect the part's capacity to store the resource (their equipment takes up additional space, for instance).
        /// </summary>
        public string capacityAffectingModules = string.Empty;

        /// <summary>
        /// If a part has at least one part module on the capacityAffectingModules list then multiply resource amount and max amount by this multiplier. Default is 1.0
        /// </summary>
        public float capacityMultiplier = 1.0f;

        /// <summary>
        /// If true (which is the default), then amount and maxAmount added are multiplied by the part's crew capacity.
        /// </summary>
        public bool isPerKerbal = true;
        #endregion

        #region API
        /// <summary>
        /// Loads the SNACKS_PART_RESOURCE config nodes, if any, and returns SnacksPartResource objects.
        /// </summary>
        /// <returns>A list of SnacksPartResource objects.</returns>
        public static List<SnacksPartResource> LoadPartResources()
        {
            ConfigNode[] partResourceNodes = GameDatabase.Instance.GetConfigNodes("SNACKS_PART_RESOURCE");
            ConfigNode node;
            List<SnacksPartResource> partResourceList = new List<SnacksPartResource>();
            SnacksPartResource partResource;
            bool addedSnacksResource = false;

            for (int index = 0; index < partResourceNodes.Length; index++)
            {
                node = partResourceNodes[index];

                partResource = new SnacksPartResource();
                if (node.HasValue("resourceName"))
                    partResource.resourceName = node.GetValue("resourceName");
                if (partResource.resourceName == SnacksProperties.SnacksResourceName)
                    addedSnacksResource = true;
                if (node.HasValue("amount"))
                    double.TryParse(node.GetValue("amount"), out partResource.amount);
                if (node.HasValue("maxAmount"))
                    double.TryParse(node.GetValue("maxAmount"), out partResource.maxAmount);
                if (node.HasValue("capacityAffectingModules"))
                    partResource.capacityAffectingModules = node.GetValue("capacityAffectingModules");
                if (node.HasValue("capacityMultiplier"))
                    float.TryParse(node.GetValue("capacityMultiplier"), out partResource.capacityMultiplier);
                if (node.HasValue("isPerKerbal"))
                    bool.TryParse(node.GetValue("isPerKerbal"), out partResource.isPerKerbal);

                partResourceList.Add(partResource);
            }

            //Failsafe: make sure we have a Snacks part resource. If not, add it.
            if (!addedSnacksResource)
            {
                partResource = new SnacksPartResource();
                partResource.resourceName = SnacksProperties.SnacksResourceName;
                partResource.amount = 200;
                partResource.maxAmount = 200;
                partResource.capacityAffectingModules = "ModuleCommand";
                partResource.capacityMultiplier = 0.25f;
                partResourceList.Add(partResource);
            }

            //Return the resources
            return partResourceList;
        }

        /// <summary>
        /// If the part with crew capacity doesn't have the resource, then add it.
        /// </summary>
        /// <param name="part"></param>
        public void addResourcesIfNeeded(Part part)
        {
            if (part.CrewCapacity <= 0)
                return;
            PartResourceDefinitionList definitions = PartResourceLibrary.Instance.resourceDefinitions;
            PartResourceDefinition def;
            double partCurrentAmount = 0;
            double partMaxAmount = 0;
            double unitsToAdd = 0;
            double maxUnitsToAdd = 0;
            int moduleCount;

            if (string.IsNullOrEmpty(resourceName))
                return;
            if (!definitions.Contains(resourceName))
                return;

            //Check the part to see if it has the resources already.
            def = definitions[resourceName];
            part.GetConnectedResourceTotals(def.id, out partCurrentAmount, out partMaxAmount, true);
            if (partMaxAmount > 0)
                return;

            //Now add the resource.
            //Determine how many units to add. If the part has any module on the capacity list, then we use the capacity modifier.
            unitsToAdd = amount;
            maxUnitsToAdd = maxAmount;
            if (!isPerKerbal)
            {
                unitsToAdd *= part.CrewCapacity;
                maxUnitsToAdd *= part.CrewCapacity;
            }

            //Next, account for capacity multiplier
            moduleCount = part.Modules.Count;
            for (int moduleIndex = 0; moduleIndex < moduleCount; moduleIndex++)
            {
                if (capacityAffectingModules.Contains(part.Modules[moduleIndex].moduleName))
                {
                    unitsToAdd *= capacityMultiplier;
                    maxUnitsToAdd *= capacityMultiplier;
                    break;
                }
            }

            //Add the resource.
            part.Resources.Add(def.name, unitsToAdd, maxUnitsToAdd, true, true, false, true, PartResource.FlowMode.Both);
        }

        /// <summary>
        /// If the loaded vessel's parts with crew capacity don't have the resource, then load it.
        /// </summary>
        /// <param name="vessel"></param>
        public void addResourcesIfNeeded(Vessel vessel)
        {
            if (vessel.GetCrewCapacity() <= 0)
                return;
            PartResourceDefinitionList definitions = PartResourceLibrary.Instance.resourceDefinitions;
            PartResourceDefinition def;
            double partCurrentAmount = 0;
            double partMaxAmount = 0;
            double unitsToAdd = 0;
            double maxUnitsToAdd = 0;
            int partCount;
            int moduleCount;
            Part part;

            if (string.IsNullOrEmpty(resourceName))
                return;
            if (!definitions.Contains(resourceName))
                return;

            //Check the vessel to see if it has the resources already.
            def = definitions[resourceName];
            vessel.rootPart.GetConnectedResourceTotals(def.id, out partCurrentAmount, out partMaxAmount, true);
            if (partMaxAmount > 0)
                return;

            //Now add the resource.
            partCount = vessel.parts.Count;
            for (int index = 0; index < partCount; index++)
            {
                part = vessel.parts[index];
                if (part.CrewCapacity <= 0)
                    continue;

                //Determine how many units to add. If the part has any module on the capacity list, then we use the capacity modifier.
                unitsToAdd = amount;
                maxUnitsToAdd = maxAmount;
                if (!isPerKerbal)
                {
                    unitsToAdd *= part.CrewCapacity;
                    maxUnitsToAdd *= part.CrewCapacity;
                }

                //Next, account for capacity multiplier
                moduleCount = part.Modules.Count;
                for (int moduleIndex = 0; moduleIndex < moduleCount; moduleIndex++)
                {
                    if (capacityAffectingModules.Contains(part.Modules[moduleIndex].moduleName))
                    {
                        unitsToAdd *= capacityMultiplier;
                        maxUnitsToAdd *= capacityMultiplier;
                        break;
                    }
                }

                //Add the resource.
                part.Resources.Add(def.name, unitsToAdd, maxUnitsToAdd, true, true, false, true, PartResource.FlowMode.Both);
            }
        }
        #endregion
    }
}
