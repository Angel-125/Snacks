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
using UnityEngine;
using System.Text;

namespace Snacks
{
    /// <summary>
    /// This class represents a resource that's tied to individual kerbals instead of a part. One example is Stress, an abstracted habitation mechanic.
    /// </summary>
    public class SnacksRosterResource
    {
        #region Constants
        public const string RosterResourceNode = "SNACKS_ROSTER_RESOURCE";
        const string RosterResourceAmount = "amount";
        const string RosterResourceMaxAmount = "maxAmount";
        const string RosterResourceName = "resourceName";
        const string RosterResourceDisplayName = "displayName";
        const string RosterResourceBonusAmount = "experienceBonusAmount";
        const string RosterResourceBonusMaxAmount = "experienceBonusMaxAmount";
        const string RosterResourceDisplayFormat = "statusFormat";
        const string RosterResourceShowInSnapshot = "showInSnapshot";
        const string AmountFormat = "<<amount>>";
        const string MaxAmountFormat = "<<maxAmount>>";
        const string PercentFormat = "<<percent>>";
        #endregion

        #region Housekeeping
        /// <summary>
        /// The name of the roster resource.
        /// </summary>
        public string resourceName = string.Empty;

        /// <summary>
        /// Public display name of the resource.
        /// </summary>
        public string displayName = string.Empty;

        /// <summary>
        /// Flag to indicate whether or not to show the resource in the Snapshots window.
        /// Default: true
        /// </summary>
        public bool showInSnapshot = true;

        /// <summary>
        /// The amount of resource available.
        /// </summary>
        public double amount;

        /// <summary>
        /// The maximum amount of resource allowed.
        /// </summary>
        public double maxAmount;

        /// <summary>
        /// A customized format for the status. The following parameters are all optional:
        /// <<percent>>: amount divided by maxAmount.
        /// <<amount>>: Current amount of roster resource.
        /// <<maxAmount>>: Max amount of roster resource.
        /// </summary>
        public string statusFormat = "<<percent>> (<<amount>>/<<maxAmount>> days)";

        /// <summary>
        /// The amount of resource to add when the kerbal levels up.
        /// </summary>
        public double experienceBonusAmount;

        /// <summary>
        /// The maximum amount of resource to add when the kerbal levels up.
        /// </summary>
        public double experienceBonusMaxAmount;
        #endregion

        #region Constructors
        public SnacksRosterResource()
        {

        }

        public SnacksRosterResource(ConfigNode node)
        {
            if (node.HasValue(RosterResourceName))
                resourceName = node.GetValue(RosterResourceName);

            if (node.HasValue(RosterResourceDisplayName))
                displayName = node.GetValue(RosterResourceDisplayName);

            if (node.HasValue(RosterResourceDisplayFormat))
                statusFormat = node.GetValue(RosterResourceDisplayFormat);

            if (node.HasValue(RosterResourceAmount))
                double.TryParse(node.GetValue(RosterResourceAmount), out amount);

            if (node.HasValue(RosterResourceMaxAmount))
                double.TryParse(node.GetValue(RosterResourceMaxAmount), out maxAmount);

            if (node.HasValue(RosterResourceBonusAmount))
                double.TryParse(node.GetValue(RosterResourceBonusAmount), out experienceBonusAmount);

            if (node.HasValue(RosterResourceBonusMaxAmount))
                double.TryParse(node.GetValue(RosterResourceBonusMaxAmount), out experienceBonusMaxAmount);

            if (node.HasValue(RosterResourceShowInSnapshot))
                bool.TryParse(node.GetValue(RosterResourceShowInSnapshot), out showInSnapshot);
        }
        #endregion

        #region Load and Save
        public static Dictionary<string, SnacksRosterResource> LoadRosterResources()
        {
            Dictionary<string, SnacksRosterResource> rosterResources = new Dictionary<string, SnacksRosterResource>();

            ConfigNode[] resourceNodes = GameDatabase.Instance.GetConfigNodes(RosterResourceNode);
            SnacksRosterResource resource;

            for (int index = 0; index < resourceNodes.Length; index++)
            {
                resource = new SnacksRosterResource(resourceNodes[index]);
                rosterResources.Add(resource.resourceName, resource);
            }

            return rosterResources;
        }

        public static Dictionary<string, SnacksRosterResource> LoadFromAstronautData(ConfigNode node)
        {
            Dictionary<string, SnacksRosterResource> rosterResources = new Dictionary<string, SnacksRosterResource>();

            ConfigNode[] resourceNodes = node.GetNodes(RosterResourceNode);
            double value = 0;
            SnacksRosterResource resource;
            SnacksRosterResource resourceDef;
            string resourceName;

            for (int index = 0; index < resourceNodes.Length; index++)
            {
                resource = new SnacksRosterResource();

                resourceName = resourceNodes[index].GetValue(RosterResourceName);
                resource.resourceName = resourceName;

                if (SnacksScenario.Instance.rosterResources.ContainsKey(resourceName))
                {
                    resourceDef = SnacksScenario.Instance.rosterResources[resourceName];
                    resource.displayName = resourceDef.displayName;
                    resource.statusFormat = resourceDef.statusFormat;
                    resource.showInSnapshot = resourceDef.showInSnapshot;
                }

                double.TryParse(resourceNodes[index].GetValue(RosterResourceAmount), out value);
                resource.amount = value;

                double.TryParse(resourceNodes[index].GetValue(RosterResourceMaxAmount), out value);
                resource.maxAmount = value;

                rosterResources.Add(resourceName, resource);
            }

            return rosterResources;
        }

        public static void SaveToAstronautData(Dictionary<string, SnacksRosterResource> rosterResources, ConfigNode node)
        {
            string[] keys = rosterResources.Keys.ToArray();
            ConfigNode configNode;

            for (int index = 0; index < keys.Length; index++)
            {
                configNode = new ConfigNode(RosterResourceNode);
                configNode.AddValue(RosterResourceName, keys[index]);
                configNode.AddValue(RosterResourceAmount, rosterResources[keys[index]].amount);
                configNode.AddValue(RosterResourceMaxAmount, rosterResources[keys[index]].maxAmount);
                node.AddNode(configNode);
            }

        }
        #endregion

        #region Game Events
        /// <summary>
        /// Handles the kerbal level up event
        /// </summary>
        /// <param name="astronaut">The ProtoCrewMember that has leveled up.</param>
        public virtual void onKerbalLevelUp(ProtoCrewMember astronaut)
        {
            AstronautData astronautData = SnacksScenario.Instance.GetAstronautData(astronaut);
            SnacksRosterResource astronautResource;

            if (astronautData.rosterResources.ContainsKey(resourceName))
            {
                astronautResource = astronautData.rosterResources[resourceName];

                astronautResource.amount += (astronautResource.experienceBonusAmount * astronaut.experienceTrait.CrewMemberExperienceLevel());
                astronautResource.maxAmount += (astronautResource.experienceBonusMaxAmount * astronaut.experienceTrait.CrewMemberExperienceLevel());

                astronautData.rosterResources[resourceName] = astronautResource;
            }
        }

        /// <summary>
        /// Handles the crew boarded event. The resource is removed from the kerbal and added to the vessel.
        /// </summary>
        /// <param name="evaKerbal">The kerbal that is returning from EVA</param>
        /// <param name="boardedPart">The part that the kerbal boarded</param>
        public virtual void onCrewBoardedVessel(Part evaKerbal, Part boardedPart)
        {

        }

        /// <summary>
        /// Handles the crew eva event. The kerbal gains the EVA resource and the vessel loses a corresponding amount.
        /// </summary>
        /// <param name="evaKerbal">The kerbal that went on EVA</param>
        /// <param name="partExited">The part that the kerbal exited</param>
        public virtual void onCrewEVA(Part evaKerbal, Part partExited)
        {

        }

        /// <summary>
        /// Adds the roster resource to the kerbal if needed
        /// </summary>
        /// <param name="astronaut">The ProtoCrewMember to check.</param>
        public virtual void addResourceIfNeeded(ProtoCrewMember astronaut)
        {
            SnacksRosterResource resource;
            AstronautData astronautData = SnacksScenario.Instance.GetAstronautData(astronaut);

            if (!astronautData.rosterResources.ContainsKey(resourceName))
            {
                resource = new SnacksRosterResource();
                resource.displayName = displayName;
                resource.resourceName = resourceName;
                resource.showInSnapshot = showInSnapshot;
                resource.amount = amount;
                resource.maxAmount = maxAmount;

                astronautData.rosterResources.Add(resourceName, resource);
                SnacksScenario.Instance.SetAstronautData(astronautData);
            }
        }

        /// <summary>
        /// Adds the roster resource to the vessel's kerbal if needed
        /// </summary>
        /// <param name="vessel">The Vessel whose crew to check</param>
        public virtual void addResourceIfNeeded(Vessel vessel)
        {
            ProtoCrewMember[] astronauts = null;

            if (vessel.loaded)
                astronauts = vessel.GetVesselCrew().ToArray();
            else
                astronauts = vessel.protoVessel.GetVesselCrew().ToArray();

            for (int index = 0; index < astronauts.Length; index++)
                addResourceIfNeeded(astronauts[index]);
        }
        #endregion

        #region API
        public string GetStatusDisplay()
        {
            StringBuilder status = new StringBuilder();
            double percent = amount / maxAmount;
            string statusDisplay = statusFormat;

            if (percent < 0.0001)
                return string.Empty;

            status.Append("<color=white> - ");
            status.Append(displayName != string.Empty ? displayName : resourceName);
            status.Append(": ");

            if (percent > 1.0f)
                percent = 100.0f;
            else
                percent *= 100.0f;
            if (statusDisplay.Contains(PercentFormat))
                statusDisplay = statusDisplay.Replace(PercentFormat, string.Format("{0:f0}%", percent));

            if (statusDisplay.Contains(AmountFormat))
                statusDisplay = statusDisplay.Replace(AmountFormat, string.Format("{0:f1}", amount));

            if (statusDisplay.Contains(MaxAmountFormat))
                statusDisplay = statusDisplay.Replace(MaxAmountFormat, string.Format("{0:f1}", maxAmount));

            status.Append(statusDisplay);

            status.Append("</color>");

            return status.ToString();
        }
        #endregion
    }
}
