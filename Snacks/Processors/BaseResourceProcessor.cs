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
    public enum SnacksResultType
    {
        resultConsumption,
        resultProduction,
        notApplicable
    }

    /// <summary>
    /// This is a result that has data regarding what happened during resource consumption or production.
    /// </summary>
    public struct SnacksProcessorResult
    {
        /// <summary>
        /// Name of the resource that was processed.
        /// </summary>
        public string resourceName;

        /// <summary>
        /// Type of result
        /// </summary>
        public SnacksResultType resultType;

        /// <summary>
        /// Flag to indicate whether or not the process completed successfully.
        /// </summary>
        public bool completedSuccessfully;

        /// <summary>
        /// Flag indicating if the process was applied per crew member.
        /// </summary>
        public bool appliedPerCrew;

        /// <summary>
        /// Number of kerbals affected by the process.
        /// </summary>
        public int affectedKerbalCount;

        /// <summary>
        /// Current amount of the resource in the vessel/kerbal.
        /// </summary>
        public double currentAmount;

        /// <summary>
        /// Max amount of the resource in the vessel/kerbal.
        /// </summary>
        public double maxAmount;

        /// <summary>
        /// Current number of crew aboard the vessel
        /// </summary>
        public int crewCount;

        /// <summary>
        /// Total crew capacity.
        /// </summary>
        public int crewCapacity;

        /// <summary>
        /// List of individual astronauts affected by the result.
        /// </summary>
        public List<ProtoCrewMember> afftectedAstronauts;
    }

    /// <summary>
    /// This is the base class for a resource processor. Similar to ModuleResourceConverter, the consumer will consume resources and produce resources, but it happens at the vessel level, not the part level.
    /// It's also designed to work with both loaded and unloaded vessels. Another important difference is that consumed/produced resources can occur on a per crewmember basis; a vessel with 5 crew will
    /// consume and/or produce 5 times the resources as a vessel with 1 crewmember. The configuration of a BaseResourceProcessor is done through config files.
    /// </summary>
    public class BaseResourceProcessor
    {
        #region Constants
        public const string ProcessorNode = "SNACKS_RESOURCE_PROCESSOR";
        public const string ProcessorNodeSecondsPerCycle = "secondsPerCycle";
        public const string ProcessorNodeName = "name";
        public const string ProcessorNodeRemainingTime = "remainingTime";

        public const string ConsumedResourceNode = "CONSUMED_RESOURCE";
        public const string ProducedResourceNode = "PRODUCED_RESOURCE";
        public const string OutcomeNode = "OUTCOME";
        #endregion

        #region Fields
        /// <summary>
        /// Name of the resource processor
        /// </summary>
        public string name = string.Empty;

        /// <summary>
        /// Number of seconds that must pass before running the consumer.
        /// </summary>
        public double secondsPerCycle = 3600.0f;

        /// <summary>
        /// Flag indicating whether or not the processor is enabled.
        /// </summary>
        public bool isActivated = true;
        #endregion

        #region Housekeeping
        public List<BasePrecondition> preconditions;
        public List<ProcessedResource> inputList;
        public List<ProcessedResource> outputList;
        public Dictionary<string, SnacksProcessorResult> consumptionResults;
        public Dictionary<string, SnacksProcessorResult> productionResults;
        public List<BaseOutcome> outcomes;

        protected double remainingTime = 0;

        public BaseResourceProcessor()
        {
            //Create lists
            preconditions = new List<BasePrecondition>();
            inputList = new List<ProcessedResource>();
            outputList = new List<ProcessedResource>();
            consumptionResults = new Dictionary<string, SnacksProcessorResult>();
            productionResults = new Dictionary<string, SnacksProcessorResult>();
            outcomes = new List<BaseOutcome>();
        }

        public BaseResourceProcessor(ConfigNode node)
        {
            if (node.HasValue(ProcessorNodeName))
                name = node.GetValue(ProcessorNodeName);

            if (node.HasValue(ProcessorNodeSecondsPerCycle))
                double.TryParse(node.GetValue(ProcessorNodeSecondsPerCycle), out secondsPerCycle);

            //Create lists
            preconditions = new List<BasePrecondition>();
            inputList = new List<ProcessedResource>();
            outputList = new List<ProcessedResource>();
            consumptionResults = new Dictionary<string, SnacksProcessorResult>();
            productionResults = new Dictionary<string, SnacksProcessorResult>();
            outcomes = new List<BaseOutcome>();

            //Add processed resources
            ConfigNode[] nodes;
            ConfigNode configNode;
            ProcessedResource resource;
            BaseOutcome outcome;
            if (node.HasNode(ConsumedResourceNode))
            {
                nodes = node.GetNodes(ConsumedResourceNode);
                for (int index = 0; index < nodes.Length; index++)
                {
                    configNode = nodes[index];
                    resource = new ProcessedResource();
                    resource.Load(configNode);
                    inputList.Add(resource);
                }
            }

            if (node.HasNode(ProducedResourceNode))
            {
                nodes = node.GetNodes(ProducedResourceNode);
                for (int index = 0; index < nodes.Length; index++)
                {
                    configNode = nodes[index];
                    resource = new ProcessedResource();
                    resource.Load(configNode);
                    outputList.Add(resource);
                }
            }

            //Add outcomes
            if (node.HasNode(OutcomeNode))
            {
                nodes = node.GetNodes(OutcomeNode);
                for (int index = 0; index < nodes.Length; index++)
                {
                    configNode = nodes[index];
                    outcome = SnacksScenario.Instance.CreateOutcome(configNode);
                    if (outcome != null)
                        outcomes.Add(outcome);
                }
            }

            //Add preconditions
            BasePrecondition precondition;
            if (node.HasNode(BasePrecondition.PRECONDITION))
            {
                nodes = node.GetNodes(BasePrecondition.PRECONDITION);
                for (int index = 0; index < nodes.Length; index++)
                {
                    configNode = nodes[index];
                    precondition = SnacksScenario.Instance.CreatePrecondition(configNode);
                    if (precondition != null)
                        preconditions.Add(precondition);
                }
            }

            if (node.HasValue("isActivated"))
                bool.TryParse(node.GetValue("isActivated"), out isActivated);
        }
        #endregion

        #region Event Handlers
        /// <summary>
        /// Handles the situation where the kerbal went on EVA.
        /// </summary>
        /// <param name="astronaut">The kerbal that went on EVA.</param>
        /// <param name="part">The part that the kerbal left.</param>
        public virtual void onKerbalEVA(ProtoCrewMember astronaut, Part part)
        {

        }

        /// <summary>
        /// Handles the situation where a kerbal boards a vessel.
        /// </summary>
        /// <param name="astronaut">The kerbal boarding a vessel.</param>
        /// <param name="part">The part boarded.</param>
        public virtual void onKerbalBoardedVessel(ProtoCrewMember astronaut, Part part)
        {

        }

        /// <summary>
        /// Handles adding of a new kerbal, giving the consumer a chance to add custom roster data.
        /// </summary>
        /// <param name="astronaut">The kerbal being added.</param>
        public virtual void onKerbalAdded(ProtoCrewMember astronaut)
        {
        }

        /// <summary>
        /// Handles removal of a kerbal, giving the consumer a chance to update custom data if needed.
        /// </summary>
        /// <param name="astronaut">The kerbal being removed.</param>
        public virtual void onKerbalRemoved(ProtoCrewMember astronaut)
        {

        }

        /// <summary>
        /// Handles a kerbal's name change.
        /// </summary>
        /// <param name="astronaut">The kerbal whose name has changed. Note that roster data is already being carried over, this event is used to give consumers a chance to update custom data kept outside of the roster.</param>
        /// <param name="previousName">The kerbal's previous name.</param>
        /// <param name="newName">The kerbal's new name.</param>
        public virtual void onKerbalNameChanged(ProtoCrewMember astronaut, string previousName, string newName)
        {

        }

        /// <summary>
        /// Handles vessel loaded event, for instance, adding resources that should be on the vessel.
        /// </summary>
        /// <param name="vessel">The vessel that was loaded.</param>
        public virtual void onVesselLoaded(Vessel vessel)
        {

        }

        /// <summary>
        /// Handles vessel dock/undock event.
        /// </summary>
        /// <param name="vessel">The vessel that was loaded.</param>
        public virtual void onVesselDockUndock(Vessel vessel)
        {

        }

        /// <summary>
        /// Handles the vessel recovery event
        /// </summary>
        /// <param name="protoVessel">The ProtoVessel being recovered</param>
        public virtual void onVesselRecovered(ProtoVessel protoVessel)
        {
            ProtoCrewMember[] astronauts = protoVessel.GetVesselCrew().ToArray();
            clearProcessResults(astronauts);
        }

        public virtual void onVesselRecoveryRequested(Vessel vessel)
        {
            ProtoCrewMember[] astronauts = vessel.GetVesselCrew().ToArray();
            clearProcessResults(astronauts);
        }

        /// <summary>
        /// Handles the situation where the vessel goes off rails.
        /// </summary>
        /// <param name="vessel">The Vessel going off rails</param>
        public virtual void onVesselGoOffRails(Vessel vessel)
        {

        }

        /// <summary>
        /// Handles changes to game settings.
        /// </summary>
        public virtual void OnGameSettingsApplied()
        {

        }
        #endregion

        #region API
        /// <summary>
        /// Retrieves the editor estimates for roster resources, if any.
        /// </summary>
        /// <param name="currentCrewCount">An int containing the current crew count.</param>
        /// <param name="crewCapacity">An int containing the vessel's crew capacity.</param>
        /// <param name="results">A StringBuilder that will hold the results.</param>
        /// <param name="ship">A ShipConstruct that is the current ship design in the editor.</param>
        public virtual void GetRosterResourceEstimatesForEditor(int currentCrewCount, int crewCapacity, StringBuilder results, ShipConstruct ship)
        {

        }

        /// <summary>
        /// Loads the SNACKS_RESOURCE_PROCESSOR config nodes and returns a list of processors.
        /// </summary>
        /// <returns>A list of resource processors.</returns>
        public static List<BaseResourceProcessor> LoadProcessors()
        {
            List<BaseResourceProcessor> resourceProcessors = new List<BaseResourceProcessor>();
            BaseResourceProcessor resourceProcessor;
            ConfigNode[] nodes = GameDatabase.Instance.GetConfigNodes(ProcessorNode);
            ConfigNode node;

            //Add the Stress processor if Stress is defined
            if (SnacksScenario.Instance.rosterResources.ContainsKey(StressProcessor.StressResourceName))
            {
                StressProcessor stressProcessor = new StressProcessor();
                stressProcessor.Initialize();
                resourceProcessors.Add(stressProcessor);
            }

            //Now go through all the config nodes and load them
            for (int index = 0; index < nodes.Length; index++)
            {
                node = nodes[index];
                if (!node.HasValue("name"))
                    continue;
                if (node.GetValue("name") == SnacksResourceProcessor.SnacksProcessorName)
                {
                    //Add the snacks processor
                    SnacksResourceProcessor snacksProcessor = new SnacksResourceProcessor();
                    snacksProcessor.Initialize();
                    resourceProcessors.Add(snacksProcessor);
                }
                else
                {
                    resourceProcessor = new BaseResourceProcessor(node);
                    resourceProcessor.Initialize();
                    resourceProcessors.Add(resourceProcessor);
                }
            }

            //Return all the processors
            return resourceProcessors;
        }

        /// <summary>
        /// Initializes the consumer
        /// </summary>
        public virtual void Initialize()
        {

        }

        /// <summary>
        /// Cleanup as processor is about to be destroyed
        /// </summary>
        public virtual void Destroy()
        {

        }

        /// <summary>
        /// De-serializes persistence data
        /// </summary>
        /// <param name="node">The ConfigNode with the persistence data</param>
        public virtual void OnLoad(ConfigNode node)
        {
            if (node.HasValue(ProcessorNodeRemainingTime))
                double.TryParse(node.GetValue(ProcessorNodeRemainingTime), out remainingTime);

            if (node.HasValue("isActivated"))
                bool.TryParse(node.GetValue("isActivated"), out isActivated);
        }

        /// <summary>
        /// Saves persistence data to a ConfigNode and returns it.
        /// </summary>
        /// <returns>A ConfigNode containing persistence data, if any.</returns>
        public virtual ConfigNode OnSave()
        {
            ConfigNode node = new ConfigNode(ProcessorNode);

            node.AddValue(ProcessorNodeName, name);
            node.AddValue(ProcessorNodeRemainingTime, remainingTime);
            node.AddValue("isActivated", isActivated);

            return node;
        }

        /// <summary>
        /// Used primarily for simulations, returns the consumed and produced resources for the given unit of time.
        /// </summary>
        /// <param name="vessel">The vessel to query for data.</param>
        /// <param name="secondsPerCycle">The number of seconds to calculate total inputs and outputs.</param>
        /// <param name="consumedResources">The list of consumed resources to add the inputs to.</param>
        /// <param name="producedResources">The list of produced resources to add the outputs to.</param>
        public virtual void AddConsumedAndProducedResources(Vessel vessel, double secondsPerCycle, List<ResourceRatio> consumedResources, List<ResourceRatio>  producedResources)
        {
            //Make sure we have crew
            int crewCount = 0;
            if (vessel.loaded)
                crewCount = vessel.GetCrewCount();
            else
                crewCount = vessel.protoVessel.GetVesselCrew().Count;
            if (crewCount <= 0)
                return;

            AddConsumedAndProducedResources(crewCount, secondsPerCycle, consumedResources, producedResources);
        }

        /// <summary>
        /// Used primarily for simulations, returns the consumed and produced resources for the given unit of time.
        /// </summary>
        /// <param name="crewCount">The number of crew to simulate.</param>
        /// <param name="secondsPerCycle">The number of seconds to calculate total inputs and outputs.</param>
        /// <param name="consumedResources">The list of consumed resources to add the inputs to.</param>
        /// <param name="producedResources">The list of produced resources to add the outputs to.</param>
        public virtual void AddConsumedAndProducedResources(int crewCount, double secondsPerCycle, List<ResourceRatio> consumedResources, List<ResourceRatio> producedResources)
        {
            if (crewCount <= 0)
                return;

            ResourceRatio resourceRatio;

            //Process input list
            int count = inputList.Count;
            for (int index = 0; index < count; index++)
            {
                resourceRatio = new ResourceRatio();
                resourceRatio.ResourceName = inputList[index].resourceName;
                resourceRatio.Ratio = inputList[index].amount * crewCount;
                consumedResources.Add(resourceRatio);
            }

            //Process output list
            count = outputList.Count;
            for (int index = 0; index < count; index++)
            {
                resourceRatio = new ResourceRatio();
                resourceRatio.ResourceName = outputList[index].resourceName;
                resourceRatio.Ratio = outputList[index].amount * crewCount;
                producedResources.Add(resourceRatio);
            }
        }

        /// <summary>
        /// Returns the amount and max amount of the desired resource in the unloaded vessel.
        /// </summary>
        /// <param name="protoVessel">The vessel to query for the resource totals.</param>
        /// <param name="resourceName">The name of the resource to query.</param>
        /// <param name="amount">The amount of the resource that the entire vessel has.</param>
        /// <param name="maxAmount">The max amount of the resource that the entire vessel has.</param>
        public virtual void GetUnloadedResourceTotals(ProtoVessel protoVessel, string resourceName, out double amount, out double maxAmount)
        {
            amount = 0;
            maxAmount = 0;

            ProtoPartSnapshot[] protoPartSnapshots = protoVessel.protoPartSnapshots.ToArray();
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
                    if (resource.resourceName == resourceName && resource.flowState)
                    {
                        amount += resource.amount;
                        maxAmount += resource.maxAmount;
                    }
                }
            }
        }
        #endregion

        #region Process Cycle
        /// <summary>
        /// Runs the processor, consuming input resources, producing output resources, and collating results.
        /// </summary>
        /// <param name="vessel">The vessel to run the consumer on.</param>
        /// <param name="elapsedTime">Number of seconds that have passed.</param>
        /// <param name="crewCount">Number of crew aboard the vessel.</param>
        /// <param name="crewCapacity">The vessel's total crew capacity.</param>
        public virtual void ProcessResources(Vessel vessel, double elapsedTime, int crewCount, int crewCapacity)
        {
            int count = 0;
            ProcessedResource resource;
            SnacksProcessorResult result;
            ProtoCrewMember[] astronauts;
            int adjustedCrewCount = crewCount;

            remainingTime += elapsedTime;
            while (remainingTime >= secondsPerCycle)
            {
                remainingTime -= secondsPerCycle;

                //Get vessel crew
                astronauts = SnacksScenario.Instance.GetNonExemptCrew(vessel);
                if (astronauts == null)
                    return;

                //Validate preconditions. Adjust crew count if the preconditions aren't valid for that crew member.
                count = preconditions.Count;
                for (int index = 0; index < count; index++)
                {
                    for (int astronautIndex = 0; astronautIndex < astronauts.Length; astronautIndex++)
                    {
                        if (!preconditions[index].IsValid(astronauts[astronautIndex], vessel))
                            adjustedCrewCount -= 1;
                    }
                }

                //If no crew member meets the preconditions then we're done.
                if (adjustedCrewCount <= 0)
                    return;

                //Consume inputs
                consumptionResults.Clear();
                count = inputList.Count;
                for (int index = 0; index < count; index++)
                {
                    resource = inputList[index];
                    result = resource.ConsumeResource(vessel, elapsedTime, adjustedCrewCount, crewCapacity);
                    if (result.resultType == SnacksResultType.notApplicable)
                        continue;
                    consumptionResults.Add(result.resourceName, result);

                    //Update astronaut data
                    updateAstronautData(vessel, resource, result);

                    //Handle outcomes
                    if (!result.completedSuccessfully && resource.failureResultAppliesOutcomes)
                        applyFailureOutcomes(vessel, result);
                }

                //Produce outputs
                productionResults.Clear();
                count = outputList.Count;
                for (int index = 0; index < count; index++)
                {
                    resource = outputList[index];
                    result = resource.ProduceResource(vessel, elapsedTime, adjustedCrewCount, crewCapacity, consumptionResults);
                    if (result.resultType == SnacksResultType.notApplicable)
                        continue;
                    productionResults.Add(result.resourceName, result);

                    //Update astronaut data
                    updateAstronautData(vessel, resource, result);

                    //Handle outcomes
                    if (!result.completedSuccessfully && resource.failureResultAppliesOutcomes)
                        applyFailureOutcomes(vessel, result);
                }
            }
        }
        #endregion

        #region Helpers
        protected void clearProcessResults(ProtoCrewMember[] astronauts)
        {
            AstronautData astronautData;
            ProcessedResource resource;
            int count;

            //Remove processing data for each resource if needed.
            for (int index = 0; index < astronauts.Length; index++)
            {
                astronautData = SnacksScenario.Instance.GetAstronautData(astronauts[index]);

                count = inputList.Count;
                for (int resourceIndex = 0; resourceIndex < count; resourceIndex++)
                {
                    resource = inputList[resourceIndex];
                    if (resource.clearDataDuringRecovery)
                    {
                        if (astronautData.processedResourceSuccesses.ContainsKey(resource.resourceName))
                            astronautData.processedResourceSuccesses.Remove(resource.resourceName);

                        if (astronautData.processedResourceFailures.ContainsKey(resource.resourceName))
                            astronautData.processedResourceFailures.Remove(resource.resourceName);
                    }
                }

                count = outputList.Count;
                for (int resourceIndex = 0; resourceIndex < count; resourceIndex++)
                {
                    resource = outputList[resourceIndex];
                    if (resource.clearDataDuringRecovery)
                    {
                        if (astronautData.processedResourceSuccesses.ContainsKey(resource.resourceName))
                            astronautData.processedResourceSuccesses.Remove(resource.resourceName);

                        if (astronautData.processedResourceFailures.ContainsKey(resource.resourceName))
                            astronautData.processedResourceFailures.Remove(resource.resourceName);
                    }
                }
            }
        }

        protected virtual void updateAstronautData(Vessel vessel, ProcessedResource resource, SnacksProcessorResult result)
        {
            ProtoCrewMember[] astronauts;
            AstronautData astronautData;

            //Get astronauts
            if (vessel.loaded)
                astronauts = vessel.GetVesselCrew().ToArray();
            else
                astronauts = vessel.protoVessel.GetVesselCrew().ToArray();

            for (int index = 0; index < astronauts.Length; index++)
            {
                //Get astronaut data
                astronautData = SnacksScenario.Instance.GetAstronautData(astronauts[index]);
                if (astronautData == null)
                    continue;

                //If the result was successful then remove failed count and increment success count.
                if (result.completedSuccessfully)
                {
                    //Increment success count
                    if (!astronautData.processedResourceSuccesses.ContainsKey(resource.resourceName))
                        astronautData.processedResourceSuccesses.Add(resource.resourceName, 0);
                    astronautData.processedResourceSuccesses[resource.resourceName] += 1;

                    //Remove failure count
                    if (astronautData.processedResourceFailures.ContainsKey(resource.resourceName))
                        astronautData.processedResourceFailures.Remove(resource.resourceName);
                }

                //Otherwise, remove success count and increment failed count.
                else
                {
                    //Increment failure count
                    if (!astronautData.processedResourceFailures.ContainsKey(resource.resourceName))
                        astronautData.processedResourceFailures.Add(resource.resourceName, 0);
                    astronautData.processedResourceFailures[resource.resourceName] += 1;

                    //Remove success count
                    if (astronautData.processedResourceSuccesses.ContainsKey(resource.resourceName))
                        astronautData.processedResourceSuccesses.Remove(resource.resourceName);
                }
            }
        }

        protected virtual void applyFailureOutcomes(Vessel vessel, SnacksProcessorResult result)
        {
            int count = outcomes.Count;
            List<BaseOutcome> enabledOutcomes = new List<BaseOutcome>();
            List<BaseOutcome> randomOutcomes = new List<BaseOutcome>();
            bool randomOutcomesEnabled = SnacksProperties.RandomPenaltiesEnabled;

            //Find the outcomes that are enabled and add them to the appropriate lists.
            for (int index = 0; index < count; index++)
            {
                if (outcomes[index].IsEnabled())
                {
                    if (outcomes[index].canBeRandom && randomOutcomesEnabled)
                        randomOutcomes.Add(outcomes[index]);
                    else
                        enabledOutcomes.Add(outcomes[index]);
                }
            }

            //Now go through and apply the outcomes (if any).
            count = enabledOutcomes.Count;
            for (int index = 0; index < count; index++)
            {
                enabledOutcomes[index].ApplyOutcome(vessel, result);
            }

            //Finally, pick a random outcome to apply (if any).
            if (randomOutcomes.Count > 0)
            {
                int randomIndex = UnityEngine.Random.Range(0, randomOutcomes.Count - 1);
                randomOutcomes[randomIndex].ApplyOutcome(vessel, result);
            }
        }

        protected virtual void removeFailureOutcomes(Vessel vessel, bool informPlayer = true)
        {
            int count = outcomes.Count;
            for (int index = 0; index < count; index++)
                outcomes[index].RemoveOutcome(vessel, informPlayer);
        }
        #endregion
    }
}
