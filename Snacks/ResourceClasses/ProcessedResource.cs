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
using Unity;

namespace Snacks
{
    /// <summary>
    /// This class represents resources consumed or produced by a SnacksResourceProcessor. Consumption and production is applied vessel-wide, or to individual kerbal roster entries depending on the configuration.
    /// If applied vessel-wide, the resource can be produced or consumed per kerbal. Finally, the resource can be displayed in the Snapshots view.
    /// </summary>
    public class ProcessedResource
    {
        #region Constants
        public const string NodeName = "ProcessedResource";
        #endregion

        #region Fields
        /// <summary>
        /// Name of the consumed/produced resource
        /// </summary>
        public string resourceName = string.Empty;

        /// <summary>
        /// Name of the dependency resource if the resource to process depends upon the results of another resource's process result. E.G. 1 unit of Soil is produced for 1 unt of Snacks consumed.
        /// </summary>
        public string dependencyResourceName = string.Empty;

        /// <summary>
        /// Flag to indicate whether or not the resource is applied to roster entries instead of parts and vessels. if set to true, then appliedPerCrew is ignored.
        /// Default: false
        /// </summary>
        public bool isRosterResource = false;

        /// <summary>
        /// Flag to indicate whether or not to show the resource in the Snapshots window. Ignored if isRosterResource is set to true.
        /// Default: true
        /// </summary>
        public bool showInSnapshot = true;

        /// <summary>
        /// Flag to indicate whether or not a failure result applies the processor's outcomes.
        /// Default: true
        /// </summary>
        public bool failureResultAppliesOutcomes = true;

        /// <summary>
        /// The amount of resource to consume or produce. If appliedPerCrew is true, then the amount consumed/produced is multiplied by the number of crew aboard the vessel.
        /// If isRosterResource is true, then each individual crew member's roster entry will be affected instead.
        /// Default: 0
        /// </summary>
        public double amount = 0;

        /// <summary>
        /// Flag to indicate that astronaut data should be cleared when a vessel is recovered.
        /// Default: true
        /// </summary>
        public bool clearDataDuringRecovery = true;
        #endregion

        #region Housekeeping
        public ProcessedResource()
        {

        }

        public ProcessedResource(string resourceName, string dependencyResourceName, double amount, bool isRosterResource = false, bool showInSnapshot = true, bool failureResultAppliesOutcomes = true)
        {
            this.resourceName = resourceName;
            this.dependencyResourceName = dependencyResourceName;
            this.amount = amount;
            this.isRosterResource = isRosterResource;
            this.failureResultAppliesOutcomes = failureResultAppliesOutcomes;
            this.showInSnapshot = showInSnapshot;
        }

        /// <summary>
        /// Loads the fields from the config node.
        /// </summary>
        /// <param name="node">A ConfigNode containing fields to load.</param>
        public void Load(ConfigNode node)
        {
            if (node.HasValue("resourceName"))
                resourceName = node.GetValue("resourceName");
            if (node.HasValue("dependencyResourceName"))
                dependencyResourceName = node.GetValue("dependencyResourceName");
            if (node.HasValue("amount"))
                double.TryParse(node.GetValue("amount"), out amount);
            if (node.HasValue("showInSnapshot"))
                bool.TryParse(node.GetValue("showInSnapshot"), out showInSnapshot);
            if (node.HasValue("failureResultAppliesOutcomes"))
                bool.TryParse(node.GetValue("failureResultAppliesOutcomes"), out failureResultAppliesOutcomes);

            if (node.HasValue("isRosterResource"))
            {
                bool.TryParse(node.GetValue("isRosterResource"), out isRosterResource);
            }
            else
            {
                //See if we can figure it out.
                PartResourceDefinitionList definitions = PartResourceLibrary.Instance.resourceDefinitions;
                if (definitions.Contains(resourceName))
                    isRosterResource = false;
                else
                    isRosterResource = true;
            }
        }

        /// <summary>
        /// Saves current values to a ConfigNode.
        /// </summary>
        /// <returns>A ConfigNode containing the field data.</returns>
        public ConfigNode Save()
        {
            ConfigNode node = new ConfigNode(NodeName);

            node.AddValue("resourceName", resourceName);
            node.AddValue("amount", amount);
            node.AddValue("isRosterResource", isRosterResource);
            node.AddValue("showInSnapshot", showInSnapshot);
            node.AddValue("failureResultAppliesOutcomes", failureResultAppliesOutcomes);

            return node;
        }
        #endregion

        /// <summary>
        /// Consumes the resource.
        /// </summary>
        /// <param name="vessel">The vessel to work on</param>
        /// <param name="elapsedTime">Elapsed seconds</param>
        /// <param name="crewCount">Current crew count</param>
        /// <param name="crewCapacity">Current crew capacity</param>
        /// <returns>A SnacksConsumerResult containing the resuls of the consumption.</returns>
        public SnacksProcessorResult ConsumeResource(Vessel vessel, double elapsedTime, int crewCount, int crewCapacity)
        {
            List<ProtoPartResourceSnapshot> protoPartResources = new List<ProtoPartResourceSnapshot>();

            SnacksProcessorResult result = new SnacksProcessorResult();
            result.resourceName = resourceName;
            result.resultType = SnacksResultType.resultConsumption;
            result.crewCapacity = crewCapacity;
            result.crewCount = crewCount;

            if (!isRosterResource)
            {
                double vesselCurrentAmount = 0;
                double vesselMaxAmount = 0;
                double requestAmount = amount;

                // If the vessel is a kerbal on EVA, then consume the snack pack resources.
                if (vessel.isEVA)
                {
                    return ConsumeEVAResource(vessel, result);
                }

                //Get current totals
                getResourceTotals(vessel, out vesselCurrentAmount, out vesselMaxAmount, protoPartResources);

                //If the vessel has no resource at all then it hasn't been visited in-game yet.
                if (vesselMaxAmount <= 0)
                {
                    result.resultType = SnacksResultType.notApplicable;
                    return result;
                }

                //Multiply request amount by crew count
                requestAmount *= crewCount;

                //If we have enough to support the whole crew, then we're good.
                if ((vesselCurrentAmount / requestAmount) >= 0.999)
                {
                    //Request resource
                    requestResource(vessel, requestAmount, protoPartResources);

                    //Update results
                    result.affectedKerbalCount = crewCount;
                    result.completedSuccessfully = true;
                    result.currentAmount = vesselCurrentAmount - requestAmount;
                    result.maxAmount = vesselMaxAmount;
                }

                //We don't have enough to support the whole crew. Figure out how many we can support.
                else
                {
                    int totalServed = (int)Math.Floor(vesselCurrentAmount / amount);

                    requestAmount = totalServed * amount;
                    requestResource(vessel, requestAmount, protoPartResources);

                    result.completedSuccessfully = false;
                    result.currentAmount = vesselCurrentAmount - requestAmount;
                    result.maxAmount = vesselMaxAmount;
                    result.affectedKerbalCount = crewCount - totalServed;
                }
            }

            //Process the roster resource
            else
            {
                consumeRosterInputs(vessel, elapsedTime);
            }

            return result;
        }

        /// <summary>
        /// Produces the resource
        /// </summary>
        /// <param name="vessel">The vessel to work on</param>
        /// <param name="elapsedTime">Elapsed seconds</param>
        /// <param name="crewCount">Current crew count</param>
        /// <param name="crewCapacity">Current crew capacity</param>
        /// <param name="consumptionResults">Results of resource consumption.</param>
        /// <returns>A SnacksConsumerResult containing the resuls of the production.</returns>
        public SnacksProcessorResult ProduceResource(Vessel vessel, double elapsedTime, int crewCount, int crewCapacity, Dictionary<string, SnacksProcessorResult> consumptionResults)
        {
            SnacksProcessorResult dependencyConsumptionResult;
            int adjustedCrewCount = crewCount;
            List<ProtoPartResourceSnapshot> protoPartResources = new List<ProtoPartResourceSnapshot>();

            //If our output depends upon the results of a dependency resource, then retrieve the results.
            if (!string.IsNullOrEmpty(dependencyResourceName) && consumptionResults.ContainsKey(dependencyResourceName))
            {
                dependencyConsumptionResult = consumptionResults[dependencyResourceName];
                adjustedCrewCount = dependencyConsumptionResult.affectedKerbalCount;
            }

            SnacksProcessorResult result = new SnacksProcessorResult();
            result.resourceName = resourceName;
            result.resultType = SnacksResultType.resultProduction;
            result.crewCapacity = crewCapacity;
            result.crewCount = crewCount;

            if (!isRosterResource)
            {
                double vesselCurrentAmount = 0;
                double vesselMaxAmount = 0;
                double supplyAmount = amount;

                //Get current totals
                getResourceTotals(vessel, out vesselCurrentAmount, out vesselMaxAmount, protoPartResources);

                //Multiply supply amount by crew count
                supplyAmount *= adjustedCrewCount;

                //Make sure we have enough room
                if (vesselCurrentAmount + supplyAmount <= vesselMaxAmount)
                {
                    //Add resource
                    addResource(vessel, supplyAmount, protoPartResources);

                    //Update results
                    result.affectedKerbalCount = adjustedCrewCount;
                    result.completedSuccessfully = true;
                    result.currentAmount = vesselCurrentAmount + supplyAmount;
                    result.maxAmount = vesselMaxAmount;
                }
                else
                {
                    int totalServed = Math.Min(adjustedCrewCount, (int)Math.Floor((vesselMaxAmount - vesselCurrentAmount) / amount));
                    supplyAmount = totalServed * amount;

                    //Add resource
                    addResource(vessel, supplyAmount, protoPartResources);

                    //Update results
                    result.completedSuccessfully = false;
                    result.currentAmount = vesselCurrentAmount + supplyAmount;
                    result.maxAmount = vesselMaxAmount;
                    result.affectedKerbalCount = adjustedCrewCount - totalServed;
                }
            }
            else
            {
                produceRosterOutputs(vessel, elapsedTime);
            }

            return result;
        }

        #region Helpers
        /// <summary>Consumes full, individually sized Snacks portions in crew manifest order.</summary>
        public SnacksProcessorResult ConsumeMeals(Vessel vessel, ProtoCrewMember[] crew, int crewCapacity)
        {
            crew = crew.Where(astronaut => !SnacksScenario.Instance.GetAstronautData(astronaut).isExempt).ToArray();
            SnacksProcessorResult result = new SnacksProcessorResult
            {
                resourceName = resourceName, resultType = SnacksResultType.resultConsumption,
                crewCount = crew.Length, crewCapacity = crewCapacity, appliedPerCrew = true,
                completedSuccessfully = true, mealResults = new Dictionary<string, bool>(),
                afftectedAstronauts = new List<ProtoCrewMember>()
            };
            if (crew.Length == 0)
                return result;
            if (vessel.isEVA)
                return ConsumeEVAResource(vessel, result, amount * SnacksScenario.Instance.GetAstronautData(crew[0]).SnacksMultiplier);

            var resources = new List<ProtoPartResourceSnapshot>();
            double available, maximum;
            getResourceTotals(vessel, out available, out maximum, resources);
            result.currentAmount = available;
            result.maxAmount = maximum;
            if (maximum <= 0)
            {
                result.resultType = SnacksResultType.notApplicable;
                return result;
            }
            foreach (ProtoCrewMember astronaut in crew)
            {
                double demand = amount * SnacksScenario.Instance.GetAstronautData(astronaut).SnacksMultiplier;
                double consumed = 0;
                if (demand > 0 && !double.IsInfinity(demand) && !double.IsNaN(demand) && available >= demand * (1 - 1e-9))
                    consumed = Math.Max(0, Math.Min(demand, requestResource(vessel, Math.Min(demand, available), resources)));
                bool fed = demand == 0 || (demand > 0 && consumed >= demand * (1 - 1e-9));
                available -= consumed;
                result.consumedAmount += consumed;
                result.mealResults[astronaut.name] = fed;
                if (!fed)
                    result.afftectedAstronauts.Add(astronaut);
            }
            result.currentAmount = Math.Max(0, available);
            result.completedSuccessfully = result.afftectedAstronauts.Count == 0;
            result.affectedKerbalCount = result.completedSuccessfully ? crew.Length : result.afftectedAstronauts.Count;
            return result;
        }

        /// <summary>Produces an explicit total, used to match Soil to Snacks actually eaten.</summary>
        public SnacksProcessorResult ProduceAmount(Vessel vessel, double supply, int crewCount, int crewCapacity)
        {
            SnacksProcessorResult result = new SnacksProcessorResult
            {
                resourceName = resourceName, resultType = SnacksResultType.resultProduction,
                crewCount = crewCount, crewCapacity = crewCapacity, completedSuccessfully = true
            };
            if (supply <= 0)
                return result;
            if (vessel.isEVA)
            {
                ProtoCrewMember astronaut = vessel.loaded ? vessel.GetVesselCrew()[0] : vessel.protoVessel.GetVesselCrew()[0];
                AstronautData data = SnacksScenario.Instance.GetAstronautData(astronaut);
                double current, maximum;
                if (!data.GetResourceAmounts(resourceName, out current, out maximum))
                {
                    result.resultType = SnacksResultType.notApplicable;
                    return result;
                }
                result.currentAmount = Math.Min(maximum, current + supply);
                result.maxAmount = maximum;
                data.SetResourceAmounts(resourceName, result.currentAmount, maximum);
                return result;
            }
            var resources = new List<ProtoPartResourceSnapshot>();
            double available, capacity;
            getResourceTotals(vessel, out available, out capacity, resources);
            if (capacity <= 0)
            {
                result.resultType = SnacksResultType.notApplicable;
                return result;
            }
            double stored = addResource(vessel, Math.Min(supply, Math.Max(0, capacity - available)), resources);
            // KSP RequestResource returns a negative amount for production; snapshots return positive.
            stored = Math.Abs(stored);
            result.currentAmount = available + stored;
            result.maxAmount = capacity;
            result.completedSuccessfully = stored >= supply * (1 - 1e-9);
            return result;
        }

        public static double AddResource(Vessel vessel, string resourceName, double supply, List<ProtoPartResourceSnapshot> resourceList)
        {
            if (vessel.loaded)
            {
                int resourceID = PartResourceLibrary.Instance.resourceDefinitions[resourceName].id;
                return vessel.rootPart.RequestResource(resourceID, -supply);
            }
            return AddToSnapshots(supply, resourceList);
        }

        private static double AddToSnapshots(double supply, IEnumerable<ProtoPartResourceSnapshot> resources)
        {
            double remaining = Math.Max(0, supply);
            foreach (ProtoPartResourceSnapshot resource in resources)
            {
                if (!resource.flowState)
                    continue;
                double added = Math.Min(remaining, Math.Max(0, resource.maxAmount - resource.amount));
                resource.amount += added;
                remaining -= added;
                if (remaining <= 0)
                    break;
            }
            return Math.Max(0, supply) - remaining;
        }
        public static double RequestResource(Vessel vessel, string resourceName, double demand, List<ProtoPartResourceSnapshot> resourceList)
        {
            double amountObtained = 0;
            double demandRemaining = demand;
            int count = resourceList.Count;
            ProtoPartResourceSnapshot resource;

            if (vessel.loaded)
            {
                int resourceID = PartResourceLibrary.Instance.resourceDefinitions[resourceName].id;
                amountObtained = vessel.rootPart.RequestResource(resourceID, demand);
            }
            else
            {
                for (int index = 0; index < count; index++)
                {
                    resource = resourceList[index];
                    if (!resource.flowState)
                        continue;

                    if (resource.amount >= demandRemaining)
                    {
                        resource.amount -= demandRemaining;
                        amountObtained += demandRemaining;
                        break;
                    }
                    else
                    {
                        demandRemaining -= resource.amount;
                        amountObtained += resource.amount;
                        resource.amount = 0;
                    }
                }
            }

            return amountObtained;
        }

        public static void GetResourceTotals(Vessel vessel, string resourceName, out double amount, out double maxAmount, List<ProtoPartResourceSnapshot> resourceList)
        {
            amount = 0;
            maxAmount = 0;
            if (vessel.loaded)
            {
                int resourceID = PartResourceLibrary.Instance.resourceDefinitions[resourceName].id;
                vessel.rootPart.GetConnectedResourceTotals(resourceID, out amount, out maxAmount);
            }
            else
            {
                ProtoPartSnapshot[] protoPartSnapshots = vessel.protoVessel.protoPartSnapshots.ToArray();
                ProtoPartSnapshot partSnapshot;
                ProtoPartResourceSnapshot[] protoResources;
                ProtoPartResourceSnapshot resource;

                for (int partIndex = 0; partIndex < protoPartSnapshots.Length; partIndex++)
                {
                    partSnapshot = protoPartSnapshots[partIndex];

                    protoResources = partSnapshot.resources.ToArray();
                    for (int resourceIndex = 0; resourceIndex < protoResources.Length; resourceIndex++)
                    {
                        resource = protoResources[resourceIndex];
                        if (!resource.flowState)
                            continue;

                        if (resource.resourceName == resourceName && resource.flowState)
                        {
                            amount += resource.amount;
                            maxAmount += resource.maxAmount;
                            resourceList.Add(resource);
                        }
                    }
                }
            }
        }

        protected virtual void produceRosterOutputs(Vessel vessel, double elapsedTime)
        {
            AstronautData astronautData;
            SnacksRosterResource rosterResource;
            ProtoCrewMember[] astronauts;
            if (vessel.loaded)
                astronauts = vessel.GetVesselCrew().ToArray();
            else
                astronauts = vessel.protoVessel.GetVesselCrew().ToArray();
            if (astronauts.Length <= 0)
                return;

            for (int astronautIndex = 0; astronautIndex < astronauts.Length; astronautIndex++)
            {
                //Get astronaut data
                astronautData = SnacksScenario.Instance.GetAstronautData(astronauts[astronautIndex]);
                if (astronautData.isExempt || !astronautData.rosterResources.ContainsKey(resourceName))
                    continue;

                //Get roster resource
                rosterResource = astronautData.rosterResources[resourceName];

                //Process the input
                rosterResource.amount += amount * elapsedTime;
                if (rosterResource.amount >= rosterResource.maxAmount)
                    rosterResource.amount = rosterResource.maxAmount;
                astronautData.rosterResources[resourceName] = rosterResource;

                //Fire event
                SnacksScenario.onRosterResourceUpdated.Fire(vessel, rosterResource, astronautData, astronauts[astronautIndex]);
            }
        }

        protected virtual void consumeRosterInputs(Vessel vessel, double elapsedTime)
        {
            AstronautData astronautData;
            SnacksRosterResource rosterResource;
            ProtoCrewMember[] astronauts;
            if (vessel.loaded)
                astronauts = vessel.GetVesselCrew().ToArray();
            else
                astronauts = vessel.protoVessel.GetVesselCrew().ToArray();
            if (astronauts.Length <= 0)
                return;

            for (int astronautIndex = 0; astronautIndex < astronauts.Length; astronautIndex++)
            {
                //Get astronaut data
                astronautData = SnacksScenario.Instance.GetAstronautData(astronauts[astronautIndex]);
                if (astronautData.isExempt || !astronautData.rosterResources.ContainsKey(resourceName))
                    continue;

                //Get roster resource
                rosterResource = astronautData.rosterResources[resourceName];

                //Process the input
                rosterResource.amount -= amount * elapsedTime;
                if (rosterResource.amount <= 0)
                    rosterResource.amount = 0;
                astronautData.rosterResources[resourceName] = rosterResource;

                //Fire event
                SnacksScenario.onRosterResourceUpdated.Fire(vessel, rosterResource, astronautData, astronauts[astronautIndex]);
            }
        }

        protected virtual double addResource(Vessel vessel, double supply, List<ProtoPartResourceSnapshot> resourceList)
        {
            return AddResource(vessel, resourceName, supply, resourceList);
        }
        protected virtual double requestResource(Vessel vessel, double demand, List<ProtoPartResourceSnapshot> resourceList)
        {
            double amountObtained = 0;
            double demandRemaining = demand;
            int count = resourceList.Count;
            ProtoPartResourceSnapshot resource;

            if (vessel.loaded)
            {
                int resourceID = PartResourceLibrary.Instance.resourceDefinitions[resourceName].id;
                amountObtained = vessel.rootPart.RequestResource(resourceID, demand);
            }
            else
            {
                for (int index = 0; index < count; index++)
                {
                    resource = resourceList[index];
                    if (!resource.flowState)
                        continue;

                    if (resource.amount >= demandRemaining)
                    {
                        resource.amount -= demandRemaining;
                        amountObtained += demandRemaining;
                        break;
                    }
                    else
                    {
                        demandRemaining -= resource.amount;
                        amountObtained += resource.amount;
                        resource.amount = 0;
                    }
                }
            }

            return amountObtained;
        }

        protected virtual void getResourceTotals(Vessel vessel, out double amount, out double maxAmount, List<ProtoPartResourceSnapshot> resourceList)
        {
            amount = 0;
            maxAmount = 0;
            if (vessel.loaded)
            {
                int resourceID = PartResourceLibrary.Instance.resourceDefinitions[resourceName].id;
                vessel.rootPart.GetConnectedResourceTotals(resourceID, out amount, out maxAmount);
            }
            else
            {
                ProtoPartSnapshot[] protoPartSnapshots = vessel.protoVessel.protoPartSnapshots.ToArray();
                ProtoPartSnapshot partSnapshot;
                ProtoPartResourceSnapshot[] protoResources;
                ProtoPartResourceSnapshot resource;

                for (int partIndex = 0; partIndex < protoPartSnapshots.Length; partIndex++)
                {
                    partSnapshot = protoPartSnapshots[partIndex];

                    protoResources = partSnapshot.resources.ToArray();
                    for (int resourceIndex = 0; resourceIndex < protoResources.Length; resourceIndex++)
                    {
                        resource = protoResources[resourceIndex];
                        if (!resource.flowState)
                            continue;

                        if (resource.resourceName == resourceName)
                        {
                            if (resource.flowState)
                                amount += resource.amount;
                            maxAmount += resource.maxAmount;
                            resourceList.Add(resource);
                        }
                    }
                }
            }
        }

        protected virtual SnacksProcessorResult ConsumeEVAResource(Vessel vessel, SnacksProcessorResult result)
        {
            return ConsumeEVAResource(vessel, result, amount);
        }

        protected virtual SnacksProcessorResult ConsumeEVAResource(Vessel vessel, SnacksProcessorResult result, double mealAmount)
        {
            ProtoCrewMember astronaut = vessel.loaded ? vessel.GetVesselCrew()[0] : vessel.protoVessel.GetVesselCrew()[0];
            AstronautData astronautData = SnacksScenario.Instance.GetAstronautData(astronaut);
            double astronautAmount = 0;
            double astronautMaxAmount = 0;

            // Get the current and max amounts.
            if (!astronautData.HasResource(resourceName))
            {
                result.resultType = SnacksResultType.notApplicable;
                return result;
            }
            astronautData.GetResourceAmounts(resourceName, out astronautAmount, out astronautMaxAmount);
            if (double.IsNaN(astronautAmount) || double.IsInfinity(astronautAmount))
            {
                result.resultType = SnacksResultType.notApplicable;
                return result;
            }

            // Consume the resource and note the result.
            if (mealAmount >= 0 && !double.IsNaN(mealAmount) && !double.IsInfinity(mealAmount) && astronautAmount >= mealAmount * (1 - 1e-9))
            {
                result.consumedAmount = Math.Min(astronautAmount, mealAmount);
                astronautAmount = Math.Max(0, astronautAmount - mealAmount);
                result.completedSuccessfully = true;
            }
            else
            {
                result.completedSuccessfully = false;
            }

            // Finish up the results
            result.currentAmount = astronautAmount;
            result.maxAmount = astronautMaxAmount;
            result.affectedKerbalCount = 1;
            if (result.mealResults != null)
            {
                result.mealResults[astronaut.name] = result.completedSuccessfully;
                if (!result.completedSuccessfully)
                    result.afftectedAstronauts.Add(astronaut);
            }

            // Update the astronaut's resource amounts.
            astronautData.SetResourceAmounts(resourceName, astronautAmount, astronautMaxAmount);
            SnacksScenario.Instance.SetAstronautData(astronautData);

            // Get the EVA resource (if any)
            SnacksEVAResource[] evaResources = SnacksScenario.Instance.snacksEVAResources.ToArray();
            SnacksEVAResource evaResource = null;
            for (int index = 0; index < evaResources.Length; index++)
            {
                if (evaResources[index].resourceName == resourceName)
                {
                    evaResource = evaResources[index];
                    break;
                }
            }

            // Display warning message if there is one and we've crossed the threshold.
            double warningScale = resourceName == SnacksProperties.SnacksResourceName ? astronautData.SnacksMultiplier : 1;
            if (evaResource != null && !string.IsNullOrEmpty(evaResource.warningMessage) && astronautAmount <= evaResource.warningAmount * warningScale)
            {
                ScreenMessages.PostScreenMessage(astronaut.name + " (On EVA )- " + evaResource.warningMessage, 5.0f, ScreenMessageStyle.UPPER_CENTER);
            }
            

            // Retur the result
            return result;
        }
        #endregion
    }
}
