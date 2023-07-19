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
using System.Collections;
using System.Linq;
using System.Text;
using System.IO;
using UnityEngine;
using KSP.IO;
using KSP.UI.Screens;
using KSP.Localization;

namespace Snacks
{
    #region ISnacksPenalty
    /// <summary>
    /// Interface for creating and running penalties when a processor resource runs out or has too much aboard the vessel or kerbal.
    /// </summary>
    public interface ISnacksPenalty
    {
        /// <summary>
        /// Indicates whether or not the penalty is enabled.
        /// </summary>
        /// <returns>true if inabled, false if not.</returns>
        bool IsEnabled();

        /// <summary>
        /// Indicates whether or not the penalty is always applied instead of randomly chosen.
        /// </summary>
        /// <returns>true if the penalty should always be applied, false if not.</returns>
        bool AlwaysApply();

        /// <summary>
        /// Applies the penalty to the affected kerbals
        /// </summary>
        /// <param name="affectedKerbals">An int containing the number of kerbals affected by the penalty.</param>
        /// <param name="vessel">The vessel to apply the penalty to.</param>
        void ApplyPenalty(int affectedKerbals, Vessel vessel);

        /// <summary>
        /// Removes penalty effects.
        /// </summary>
        /// <param name="vessel">The vessel to remove the penalt effects from.</param>
        void RemovePenalty(Vessel vessel);

        /// <summary>
        /// Handles changes in game settings, if any.
        /// </summary>
        void GameSettingsApplied();
    }
    #endregion

    /// <summary>
    /// The SnacksScenario class is the heart of Snacks. It runs all the processes.
    /// </summary>
    [KSPScenario(ScenarioCreationOptions.AddToAllGames, GameScenes.SPACECENTER, GameScenes.EDITOR, GameScenes.FLIGHT, GameScenes.TRACKSTATION)]
    public class SnacksScenario : ScenarioModule
    {
        #region Constants
        public double secondsPerCycle = 3600.0;

        const string SkillLossConditionNode = "SKILL_LOSS_CONDITION";
        const string SkillLossConditionName = "name";
        #endregion

        #region Snacks Events
        /// <summary>
        /// Tells listeners that snapshots were created.
        /// </summary>
        public static EventData<Vessel, Snackshot> onSnapshotsUpdated = new EventData<Vessel, Snackshot>("onSnapshotsUpdated");

        /// <summary>
        /// Tells listeners that a simulator was created. Gives mods a chance to add custom converters not covered by Snacks.
        /// </summary>
        public static EventData<SimSnacks, SimulatorContext> onSimulatorCreated = new EventData<SimSnacks, SimulatorContext>("onSimulatorCreated");

        /// <summary>
        /// Tells listeners that background converters were created. Gives mods a chance to add custom converters not covered by Snacks.
        /// </summary>
        public static EventData<Vessel> onBackgroundConvertersCreated = new EventData<Vessel>("onBackgroundConvertersCreated");

        /// <summary>
        /// Signifies that snacking has occurred.
        /// </summary>
        public static EventVoid onSnackTime = new EventVoid("onSnackTime");

        /// <summary>
        /// Signifies that the roster resource has been updated
        /// </summary>
        public static EventData<Vessel, SnacksRosterResource, AstronautData, ProtoCrewMember> onRosterResourceUpdated = new EventData<Vessel, SnacksRosterResource, AstronautData, ProtoCrewMember>("onRosterResourceUpdated");
        #endregion

        #region Housekeeping
        /// <summary>
        /// Instance of the scenario.
        /// </summary>
        public static SnacksScenario Instance;

        /// <summary>
        /// Flag indicating whether or not logging is enabled.
        /// </summary>
        public static bool LoggingEnabled;
        private static double secondsPerDay = 0;
        private static double secondsPerYear = 0;

        /// <summary>
        /// Map of sciecnce penalties sorted by vessel.
        /// </summary>
        public DictionaryValueList<string, int> sciencePenalties;

        /// <summary>
        /// Map of astronaut data, keyed by astronaut name.
        /// </summary>
        public DictionaryValueList<string, AstronautData> crewData;

        /// <summary>
        /// List of kerbals that are exempt from outcome effects.
        /// </summary>
        public string exemptKerbals = string.Empty;

        /// <summary>
        /// Last time the processing cycle started.
        /// </summary>
        public double cycleStartTime;

        /// <summary>
        /// Map of the background conveters list, keyed by vessel.
        /// </summary>
        public Dictionary<Vessel, List<SnacksBackgroundConverter>> backgroundConverters;

        /// <summary>
        /// List of resource processors that handle life support consumption and waste production.
        /// </summary>
        public List<BaseResourceProcessor> resourceProcessors;

        /// <summary>
        /// List of resources that will be added to parts as they are created or loaded.
        /// </summary>
        public List<SnacksPartResource> snacksPartResources;

        /// <summary>
        /// List of resources that are added to kerbals when they go on EVA.
        /// </summary>
        public List<SnacksEVAResource> snacksEVAResources;

        /// <summary>
        /// Map of snapshots, keyed by vessel, that give a status of each vessel's visible life support resources and crew status.
        /// </summary>
        public Dictionary<Vessel, VesselSnackshot> snapshotMap;

        /// <summary>
        /// Helper that gives a count, by celestial body id, of how many vessels are on or around the celestial body.
        /// </summary>
        public Dictionary<int, int> bodyVesselCountMap;

        /// <summary>
        /// Map of all roster resources to add to kerbals as they are created.
        /// </summary>
        public Dictionary<string, SnacksRosterResource> rosterResources;

        /// <summary>
        /// List of conditions that will cause a skill loss. These conditions are defined via SKILL_LOSS_CONDITION nodes.
        /// </summary>
        public List<String> lossOfSkillConditions;
        public SnackSimThreadPool threadPool = null;

        /// <summary>
        /// List of converters to watch for when creating snapshot simulations.
        /// </summary>
        public string converterWatchlist = "SnacksConverter;SnacksProcessor;SoilRecycler;WBIResourceConverter;WBIModuleResourceConverterFX;ModuleResourceConverter;ModuleFusionReactor";

        /// <summary>
        /// How many simulated seconds pass per simulator cycle.
        /// </summary>
        public double simulatorSecondsPerCycle = 3600;

        /// <summary>
        /// Maximum number of simulator cycles to run.
        /// </summary>
        public int maxSimulatorCycles = 10000;

        /// <summary>
        /// Max number of simulator threads to create.
        /// </summary>
        public int maxThreads = 4;

        private double elapsedTime;
        private string introStreensDisplayed = string.Empty;

        private Dictionary<string, SnacksEvent> postProcessEvents;
        private Dictionary<string, SnacksEvent> levelUpEvents;
        private Dictionary<string, SnacksEvent> eventCards;
        private int vesselsBeingProcessed;
        private bool snackCycleStarted;
        #endregion

        #region Snapshots
        /// <summary>
        /// Updates the resource snapshots for each vessel in the game that isn't Debris, a Flag, a SpaceObject, or Unknown.
        /// </summary>
        public void UpdateSnapshots()
        {
            if (threadPool == null)
                threadPool = new SnackSimThreadPool();

            //Clear the snapshot map
            snapshotMap.Clear();
            bodyVesselCountMap.Clear();

            StartCoroutine(updateSnapshots());
        }

        protected IEnumerator<YieldInstruction> updateSnapshots()
        {
            int count = FlightGlobals.Vessels.Count;
            Vessel vessel;

            for (int index = 0; index < count; index++)
            {
                vessel = FlightGlobals.Vessels[index];

                //Create the vessel snackshot
                VesselSnackshot vesselSnackshot = createVesselSnackshot(vessel);
                if (vesselSnackshot == null)
                    continue;

                //Get simulator data: complete resource list and all the converters
                SimSnacks simSnacks = SimSnacks.CreateSimulator(vessel);
                yield return new WaitForFixedUpdate();

                //Give mods a chance to add converters that aren't covered by Snacks.
                SimulatorContext context = new SimulatorContext();
                context.vessel = vessel;
                context.simulatedVesselType = vessel.loaded ? SimulatedVesselTypes.simVesselLoaded : SimulatedVesselTypes.simVesselUnloaded;
                onSimulatorCreated.Fire(simSnacks, context);
                yield return new WaitForFixedUpdate();

                //Create resource snackshots
                createResourceSnackshots(vessel, vesselSnackshot, simSnacks);
                yield return new WaitForFixedUpdate();

                //Finally, add the simulator to the jobs list
                threadPool.AddSimulatorJob(simSnacks);
            }
            yield return new WaitForFixedUpdate();
        }

        protected void createResourceSnackshots(Vessel vessel, VesselSnackshot vesselSnackshot, SimSnacks simSnacks)
        {
            int processorCount = 0;
            int processorIndex = 0;
            int resourceCount = 0;
            int resourceIndex = 0;
            List<ProcessedResource> processedResources;
            Snackshot snackshot;
            string resourceName;

            processorCount = resourceProcessors.Count;
            for (processorIndex = 0; processorIndex < processorCount; processorIndex++)
            {
                //Get resources consumed and produced
                resourceProcessors[processorIndex].AddConsumedAndProducedResources(vessel, simSnacks.secondsPerCycle, simSnacks.consumedResources, simSnacks.producedResources);

                //First check input list for resources to add to the snapshots window
                processedResources = resourceProcessors[processorIndex].inputList;
                resourceCount = processedResources.Count;
                for (resourceIndex = 0; resourceIndex < resourceCount; resourceIndex++)
                {
                    resourceName = processedResources[resourceIndex].resourceName;

                    if (processedResources[resourceIndex].showInSnapshot && simSnacks.resources.ContainsKey(resourceName))
                    {
                        snackshot = new Snackshot();
                        snackshot.showTimeRemaining = true;
                        snackshot.resourceName = processedResources[resourceIndex].resourceName;
                        snackshot.amount = simSnacks.resources[resourceName].amount;
                        snackshot.maxAmount = simSnacks.resources[resourceName].maxAmount;

                        //Add to snackshots
                        vesselSnackshot.snackshots.Add(snackshot);
                    }
                }

                //Next check outputs
                processedResources = resourceProcessors[processorIndex].outputList;
                resourceCount = processedResources.Count;
                for (resourceIndex = 0; resourceIndex < resourceCount; resourceIndex++)
                {
                    resourceName = processedResources[resourceIndex].resourceName;

                    if (processedResources[resourceIndex].showInSnapshot && simSnacks.resources.ContainsKey(resourceName))
                    {
                        snackshot = new Snackshot();
                        snackshot.showTimeRemaining = true;
                        snackshot.resourceName = processedResources[resourceIndex].resourceName;
                        snackshot.amount = simSnacks.resources[resourceName].amount;
                        snackshot.maxAmount = simSnacks.resources[resourceName].maxAmount;

                        //Add to snackshots
                        vesselSnackshot.snackshots.Add(snackshot);
                    }
                }
            }
        }

        protected VesselSnackshot createVesselSnackshot(Vessel vessel)
        {
            VesselSnackshot vesselSnackshot = null;

            //Skip vessel types that we're not interested in.
            if (vessel.vesselType == VesselType.Debris ||
                vessel.vesselType == VesselType.Flag ||
                vessel.vesselType == VesselType.SpaceObject ||
                vessel.vesselType == VesselType.Unknown)
                return null;
            if (snapshotMap.ContainsKey(vessel))
                return null;

            //Get crew capacity & crew count. Skip vessels that have no crew capacity or crew count.
            int crewCapacity = GetCrewCapacity(vessel);
            if (crewCapacity <= 0)
                return null;
            int crewCount = 0;
            if (vessel.loaded)
                crewCount = vessel.GetCrewCount();
            else
                crewCount = vessel.protoVessel.GetVesselCrew().Count;
            if (crewCount <= 0)
                return null;

            vesselSnackshot = new VesselSnackshot();
            vesselSnackshot.vessel = vessel;
            vesselSnackshot.bodyID = vessel.mainBody.flightGlobalsIndex;
            vesselSnackshot.vesselName = vessel.vesselName;
            vesselSnackshot.crewCount = crewCount;
            vesselSnackshot.maxCrewCount = crewCapacity;
            snapshotMap.Add(vessel, vesselSnackshot);

            //Next, update body vessel count map
            if (!bodyVesselCountMap.ContainsKey(vessel.mainBody.flightGlobalsIndex))
                bodyVesselCountMap.Add(vessel.mainBody.flightGlobalsIndex, 0);
            bodyVesselCountMap[vessel.mainBody.flightGlobalsIndex] += 1;

            return vesselSnackshot;
        }

        /// <summary>
        /// Returns the crew capacity of the vessel
        /// </summary>
        /// <returns>The crew capacity.</returns>
        /// <param name="vessel">The Vessel to query.</param>
        public int GetCrewCapacity(Vessel vessel)
        {
            if (vessel.isEVA)
                return 1;

            // vessel.GetCrewCapacity() incorrectly reports the crew capacity when kerbals are seated in command seats.
            int crewCapacity = 0;
            if (vessel.loaded)
            {
                int count = vessel.Parts.Count;
                Part part;
                for (int index = 0; index < count; index++)
                {
                    part = vessel.Parts[index];
                    if (part.HasModuleImplementing<KerbalEVA>() || part.CrewCapacity <= 0)
                        continue;

                    crewCapacity += part.CrewCapacity;
                }

                return crewCapacity;
            }

            ProtoPartSnapshot[] protoParts = vessel.protoVessel.protoPartSnapshots.ToArray();
            for (int index = 0; index < protoParts.Length; index++)
                crewCapacity += protoParts[index].partInfo.partPrefab.CrewCapacity;

            return crewCapacity;
        }
        #endregion

        #region Fixed Update
        /// <summary>
        /// FixedUpdate handles all the processing tasks related to life support resources and event processing.
        /// </summary>
        void FixedUpdate()
        {
            //Record cycle start time if needed.
            if (cycleStartTime == 0f)
            {
                cycleStartTime = Planetarium.GetUniversalTime();
                return;
            }

            //Check to see if we've finished processing all the vessels.
            //If so, then process events.
            if (vesselsBeingProcessed == 0 && snackCycleStarted)
            {
                snackCycleStarted = false;
                StartCoroutine(processEvents(postProcessEvents));
                //Fire snack time event
                onSnackTime.Fire();
            }

            //To avoid hammering the game with updates, we only run background converters, processors, and events once per game hour.
            elapsedTime = Planetarium.GetUniversalTime() - cycleStartTime;

            if (elapsedTime < secondsPerCycle)
                return;
            cycleStartTime = Planetarium.GetUniversalTime();

            //Run the snacks cycle
            RunSnackCyleImmediately(elapsedTime);

            //Process event cards if needed.
            if (elapsedTime >= SnacksScenario.GetSecondsPerDay())
            {
                if (SnacksProperties.EnableRandomSnacking)
                    StartCoroutine(playEventCard());
            }
        }

        /// <summary>
        /// Runs the snack cyle immediately.
        /// </summary>
        /// <param name="secondsElapsed">Seconds elapsed.</param>
        public void RunSnackCyleImmediately(double secondsElapsed)
        {
            //Reset our process monitors
            vesselsBeingProcessed = 0;
            snackCycleStarted = true;

            //Go through all the vessels. For loaded vessels, run the processors and events.
            //For unloaded vessels, run the background converters, processors, and events.
            Vessel vessel;
            int count = FlightGlobals.Vessels.Count;
            for (int vesselIndex = 0; vesselIndex < count; vesselIndex++)
            {
                vessel = FlightGlobals.Vessels[vesselIndex];

                //Skip vessel types that we're not interested in.
                if (vessel.vesselType == VesselType.Debris ||
                    vessel.vesselType == VesselType.Flag ||
                    vessel.vesselType == VesselType.SpaceObject ||
                    vessel.vesselType == VesselType.Unknown)
                    continue;

                //Start snack cycle
                vesselsBeingProcessed += 1;
                StartCoroutine(runSnackCycle(vessel, secondsElapsed));
            }
        }
        #endregion

        #region Background Processing
        protected void runBackgroundConverters(Vessel vessel, double elapsedTime)
        {
            List<SnacksBackgroundConverter> converters;
            if (backgroundConverters.ContainsKey(vessel))
                converters = backgroundConverters[vessel];
            else
                return;
            int count = converters.Count;
            SnacksBackgroundConverter converter;

            for (int index = 0; index < count; index++)
            {
                converter = converters[index];

                if (converter.IsActivated && !converter.isMissingResources && !converter.isContainerFull)
                    runConverter(converter, elapsedTime, vessel.protoVessel);
            }
        }

        protected void runConverter(SnacksBackgroundConverter converter, double elapsedTime, ProtoVessel protoVessel)
        {
            //Get ready to process
            converter.PrepareToProcess(protoVessel);

            //Check required
            converter.CheckRequiredResources(protoVessel, elapsedTime);

            //Consume inputs
            converter.ConsumeInputResources(protoVessel, elapsedTime);

            //Produce outputs
            converter.ProduceOutputResources(protoVessel, elapsedTime);

            //Produce yields
            converter.ProduceyieldsList(protoVessel);

            //Post process
            converter.PostProcess(protoVessel);
        }
        #endregion

        #region Astronaut API
        /// <summary>
        /// Finds the vessel that the kerbal is residing in.
        /// </summary>
        /// <returns>The Vessel where the kerbal resides.</returns>
        /// <param name="astronaut">The astronaut to check.</param>
        public Vessel FindVessel(ProtoCrewMember astronaut)
        {
            int count = FlightGlobals.Vessels.Count;
            Vessel vessel;

            for (int index = 0; index < count; index++)
            {
                vessel = FlightGlobals.Vessels[index];
                if (vessel.loaded)
                {
                    if (vessel.GetVesselCrew().Contains(astronaut))
                        return vessel;
                }
                else
                {
                    if (vessel.protoVessel.GetVesselCrew().Contains(astronaut))
                        return vessel;
                }
            }

            return null;
        }

        /// <summary>
        /// Determines whether or not the kerbal's skills should be removed.
        /// </summary>
        /// <returns><c>true</c>, if remove skills should be removed, <c>false</c> otherwise.</returns>
        /// <param name="astronaut">the ProtoCrewMember to investigate.</param>
        public bool ShouldRemoveSkills(ProtoCrewMember astronaut)
        {
            if (astronaut.KerbalRef == null || astronaut.KerbalRef.InVessel == null)
                return false;
            else if (!astronaut.KerbalRef.InVessel.loaded)
                return false;
            AstronautData astronautData = GetAstronautData(astronaut);
            int count = lossOfSkillConditions.Count;

            //Special case: defer skill removal if the active vessel is flying or sub-orbital.
            if (astronaut.KerbalRef.InVessel == FlightGlobals.ActiveVessel && 
                (astronaut.KerbalRef.InVessel.situation == Vessel.Situations.FLYING || 
                astronaut.KerbalRef.InVessel.situation == Vessel.Situations.SUB_ORBITAL))
                return false;

            //See if the astronaut's condition summary has any of the conditions that
            //would result in a loss of skills.
            for (int index = 0; index < count; index++)
            {
                if (astronautData.conditionSummary.Contains(lossOfSkillConditions[index]))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Removes the skills if needed. The supplied kerbal must have at least one condition
        /// registered in a SKILL_LOSS_CONDITION config node in order to remove the skills.
        /// </summary>
        /// <param name="astronaut">The kerbal to check.</param>
        public void RemoveSkillsIfNeeded(ProtoCrewMember astronaut)
        {
            if (astronaut.KerbalRef == null || astronaut.KerbalRef.InVessel == null)
                return;
            else if (!astronaut.KerbalRef.InVessel.loaded)
                return;

            if (ShouldRemoveSkills(astronaut))
                RemoveSkills(astronaut);
        }

        /// <summary>
        /// Restores the skills if needed. The kerbal in question must not have any conditions that would result in a loss of skill.
        /// </summary>
        /// <param name="astronaut">The kerbal to query.</param>
        public void RestoreSkillsIfNeeded(ProtoCrewMember astronaut)
        {
            if (astronaut.KerbalRef == null || astronaut.KerbalRef.InVessel == null)
                return;
            else if (!astronaut.KerbalRef.InVessel.loaded)
                return;

            if (!ShouldRemoveSkills(astronaut))
                RestoreSkills(astronaut);
        }

        /// <summary>
        /// Removes skills from the desired kerbal. Does not check to see if they should be removed based on condition summary.
        /// </summary>
        /// <param name="astronaut">The ProtoCrewMember to remove skills from.</param>
        public void RemoveSkills(ProtoCrewMember astronaut)
        {
            if (astronaut.KerbalRef == null || astronaut.KerbalRef.InVessel == null)
                return;
            else if (!astronaut.KerbalRef.InVessel.loaded)
                return;

            //Central place to remove skills for potential integration with 3rd party mods.
            astronaut.UnregisterExperienceTraits(astronaut.KerbalRef.InPart);
            astronaut.experienceTrait.Effects.Clear();
            astronaut.KerbalRef.InVessel.CrewListSetDirty();
            Vessel.CrewWasModified(astronaut.KerbalRef.InVessel);
            FlightInputHandler.ResumeVesselCtrlState(astronaut.KerbalRef.InVessel);
        }

        /// <summary>
        /// Restores skills to the desired kerbal. Does not check to see if they can be restored based on condition summary.
        /// </summary>
        /// <param name="astronaut"></param>
        public void RestoreSkills(ProtoCrewMember astronaut)
        {
            if (astronaut.KerbalRef == null || astronaut.KerbalRef.InVessel == null)
                return;
            else if (!astronaut.KerbalRef.InVessel.loaded)
                return;

            Experience.ExperienceTraitConfig config = GameDatabase.Instance.ExperienceConfigs.GetExperienceTraitConfig(astronaut.trait);
            astronaut.experienceTrait = Experience.ExperienceTrait.Create(KerbalRoster.GetExperienceTraitType(astronaut.trait), config, astronaut);
            astronaut.RegisterExperienceTraits(astronaut.KerbalRef.InPart);
            astronaut.KerbalRef.InVessel.CrewListSetDirty();
            Vessel.CrewWasModified(astronaut.KerbalRef.InVessel);
            FlightInputHandler.ResumeVesselCtrlState(astronaut.KerbalRef.InVessel);
        }

        /// <summary>
        /// Adds the name of the kerbal to the exemptions list.
        /// </summary>
        /// <param name="exemptedCrew">The name of the kerbal to add to the list.</param>
        public void SetExemptCrew(string exemptedCrew)
        {
            if (string.IsNullOrEmpty(exemptedCrew))
                return;
            string[] exemptedKerbals = exemptKerbals.Split(new char[] { ';' });
            AstronautData[] astronautData = crewData.Values.ToArray();
            AstronautData data;

            for (int index = 0; index < astronautData.Length; index++)
            {
                data = astronautData[index];
                if (exemptedCrew.Contains(data.name))
                    data.isExempt = true;
                else
                    data.isExempt = false;
            }
        }

        /// <summary>
        /// Registers crew into the astronaut database.
        /// </summary>
        /// <param name="vessel">The vessel to search for crew.</param>
        public void RegisterCrew(Vessel vessel)
        {
            List<ProtoCrewMember> crewManifest;
            int crewCount = 0;

            if (vessel.loaded)
            {
                crewManifest = vessel.GetVesselCrew();
                crewCount = crewManifest.Count;
            }

            else
            {
                crewManifest = vessel.protoVessel.GetVesselCrew();
                crewCount = crewManifest.Count;
            }

            for (int index = 0; index < crewCount; index++)
                RegisterCrew(crewManifest[index]);

        }

        /// <summary>
        /// Unregisters the crew from the astronaut database.
        /// </summary>
        /// <param name="protoVessel">The vessel to search for crew to unregister.</param>
        public void UnregisterCrew(ProtoVessel protoVessel)
        {
            List<ProtoCrewMember> crewManifest;
            int crewCount = 0;

            crewManifest = protoVessel.GetVesselCrew();
            crewCount = crewManifest.Count;

            for (int index = 0; index < crewCount; index++)
                UnregisterCrew(crewManifest[index]);
        }

        /// <summary>
        /// Unregisters the crew from the astronaut database.
        /// </summary>
        /// <param name="vessel">The vessel to search for crew to unregister.</param>
        public void UnregisterCrew(Vessel vessel)
        {
            List<ProtoCrewMember> crewManifest;
            int crewCount = 0;

            if (vessel.loaded)
            {
                crewManifest = vessel.GetVesselCrew();
                crewCount = crewManifest.Count;
            }

            else
            {
                crewManifest = vessel.protoVessel.GetVesselCrew();
                crewCount = crewManifest.Count;
            }

            for (int index = 0; index < crewCount; index++)
                UnregisterCrew(crewManifest[index]);
        }

        /// <summary>
        /// Registers the astronaut into the astronaut database.
        /// </summary>
        /// <param name="astronaut">The astronaut to register.</param>
        public void RegisterCrew(ProtoCrewMember astronaut)
        {
            GetAstronautData(astronaut);
        }

        /// <summary>
        /// Unregisters the astronaut from the astronaut database.
        /// </summary>
        /// <param name="astronaut">The astronaut to unregister.</param>
        public void UnregisterCrew(ProtoCrewMember astronaut)
        {
            if (crewData.Contains(astronaut.name))
                crewData.Remove(astronaut.name);
        }

        /// <summary>
        /// Unregisters the astronaut data from the astronaut database.
        /// </summary>
        /// <param name="data">The astronaut data to unregister.</param>
        public void UnregisterCrew(AstronautData data)
        {
            if (crewData.Contains(data.name))
                crewData.Remove(data.name);
        }

        /// <summary>
        /// Returns the number of crew that aren't exempt.
        /// </summary>
        /// <param name="vessel">The vessel to query for crew.</param>
        /// <returns>The number of victims. Er, number of non-exempt crew.</returns>
        public int GetNonExemptCrewCount(Vessel vessel)
        {
            ProtoCrewMember[] astronauts;
            int nonExemptCrew = 0;
            AstronautData data;

            if (vessel.loaded)
                astronauts = vessel.GetVesselCrew().ToArray();
            else
                astronauts = vessel.protoVessel.GetVesselCrew().ToArray();

            for (int index = 0; index < astronauts.Length; index++)
            {
                data = SnacksScenario.Instance.GetAstronautData(astronauts[index]);
                if (data.isExempt == false)
                    nonExemptCrew = nonExemptCrew + 1;
            }
            
            return nonExemptCrew;
        }

        /// <summary>
        /// Returns the non-exempt crew in the vessel.
        /// </summary>
        /// <param name="vessel">The Vessel to query.</param>
        /// <returns>An array of ProtoCrewMember objects if there are non-exempt crew, or null if not.</returns>
        public ProtoCrewMember[] GetNonExemptCrew(Vessel vessel)
        {
            List<ProtoCrewMember> nonExemptCrew = new List<ProtoCrewMember>();
            ProtoCrewMember[] astronauts;

            if (vessel.loaded)
                astronauts = vessel.GetVesselCrew().ToArray();
            else
                astronauts = vessel.protoVessel.GetVesselCrew().ToArray();
            for (int index = 0; index < astronauts.Length; index++)
            {
                if (!SnacksScenario.Instance.exemptKerbals.Contains(astronauts[index].name))
                    nonExemptCrew.Add(astronauts[index]);
            }

            if (nonExemptCrew.Count == 0)
                return null;
            else
                return nonExemptCrew.ToArray();
        }

        /// <summary>
        /// Returns the astronaut data associated with the astronaut.
        /// </summary>
        /// <param name="astronaut">The ProtoCrewMember to check for astronaut data.</param>
        /// <returns>The AstronautData associated with the kerbal.</returns>
        public AstronautData GetAstronautData(ProtoCrewMember astronaut)
        {
            if (crewData.Contains(astronaut.name) == false)
            {
                AstronautData data = new AstronautData();
                data.name = astronaut.name;
                data.experienceTrait = astronaut.experienceTrait.Title;
                data.lastUpdated = Planetarium.GetUniversalTime();
                data.keyValuePairs = new DictionaryValueList<string, string>();

                //Don't forget about exemptions

                crewData.Add(data.name, data);
            }

            return crewData[astronaut.name];
        }

        /// <summary>
        /// Saves the astronaut data into the database.
        /// </summary>
        /// <param name="data">The AstronautData to save.</param>
        public void SetAstronautData(AstronautData data)
        {
            if (crewData.Contains(data.name))
                crewData[data.name] = data;
        }
        #endregion

        #region Overrides
        public override void OnAwake()
        {
            base.OnAwake();
            Instance = this;
            LoggingEnabled = SnacksProperties.DebugLoggingEnabled;

            //Get simulator settings
            ConfigNode[] simulatorSettingsNodes = GameDatabase.Instance.GetConfigNodes("SNACKS_SIMULATOR");
            if (simulatorSettingsNodes[0] != null)
            {
                if (simulatorSettingsNodes[0].HasValue("converterWatchlist"))
                    converterWatchlist = simulatorSettingsNodes[0].GetValue("converterWatchlist");

                if (simulatorSettingsNodes[0].HasValue("secondsPerCycle"))
                    double.TryParse(simulatorSettingsNodes[0].GetValue("secondsPerCycle"), out simulatorSecondsPerCycle);

                if (simulatorSettingsNodes[0].HasValue("maxSimulatorCycles"))
                    int.TryParse(simulatorSettingsNodes[0].GetValue("maxSimulatorCycles"), out maxSimulatorCycles);

                if (simulatorSettingsNodes[0].HasValue("maxThreads"))
                    int.TryParse(simulatorSettingsNodes[0].GetValue("maxThreads"), out maxThreads);
            }

            //Get seconds per day
            GetSecondsPerDay();

            //Game events
            GameEvents.onVesselLoaded.Add(onVesselLoaded);
            GameEvents.onCrewOnEva.Add(onCrewOnEva);
            GameEvents.onCrewBoardVessel.Add(onCrewBoardVessel);
            GameEvents.OnGameSettingsApplied.Add(onGameSettingsApplied);
            GameEvents.onVesselWasModified.Add(onVesselWasModified);
            GameEvents.onDockingComplete.Add(onDockingComplete);
            GameEvents.onVesselsUndocking.Add(onVesselsUndocking);
            GameEvents.onVesselWillDestroy.Add(onVesselWillDestroy);
            GameEvents.OnVesselRecoveryRequested.Add(onVesselRecoveryRequested);
            GameEvents.OnVesselRollout.Add(onVesselRollout);
            GameEvents.onLevelWasLoadedGUIReady.Add(onLevelLoaded);
            GameEvents.onVesselRecovered.Add(onVesselRecovered);
            GameEvents.onVesselTerminated.Add(onVesselTerminated);
            GameEvents.onKerbalAdded.Add(onKerbalAdded);
            GameEvents.onKerbalNameChanged.Add(onKerbalNameChanged);
            GameEvents.onKerbalRemoved.Add(onKerbalRemoved);
            GameEvents.OnCrewmemberHired.Add(OnCrewmemberHired);
            GameEvents.OnCrewmemberSacked.Add(OnCrewmemberSacked);
            GameEvents.OnCrewmemberLeftForDead.Add(OnCrewmemberLeftForDead);
            GameEvents.onKerbalLevelUp.Add(onKerbalLevelUp);
            GameEvents.onKerbalStatusChanged.Add(onKerbalStatusChanged);
            GameEvents.onCrewTransferred.Add(onCrewTransferred);
            GameEvents.onEditorPartPlaced.Add(onEditorPartPlaced);
            GameEvents.onEditorPartPicked.Add(onEditorPartPicked);
            GameEvents.onEditorPodPicked.Add(onEditorPartPicked);
            GameEvents.onEditorShipModified.Add(onEditorShipModified);
            GameEvents.onProtoCrewMemberLoad.Add(onProtoCrewMemberLoad);
            GameEvents.onVesselGoOffRails.Add(onVesselGoOffRails);
            GameEvents.onKerbalLevelUp.Add(onKerbalLevelUp);
            GameEvents.onVesselSituationChange.Add(onVesselSituationChange);
            GameEvents.onEditorLoad.Add(onEditorLoad);
            GameEvents.onEditorStarted.Add(onEditorStarted);
            GameEvents.onCommandSeatInteractionEnter.Add(onCommandSeatInteractionEnter);

            //Create skill loss conditions list
            lossOfSkillConditions = new List<string>();
            ConfigNode[] conditions = GameDatabase.Instance.GetConfigNodes(SkillLossConditionNode);
            for (int index = 0; index < conditions.Length; index++)
            {
                if (conditions[index].HasValue(SkillLossConditionName))
                    lossOfSkillConditions.Add(conditions[index].GetValue(SkillLossConditionName));
            }

            //Create housekeeping lists and such
            snacksPartResources = SnacksPartResource.LoadPartResources();
            snacksEVAResources = SnacksEVAResource.LoadEVAResources();
            rosterResources = SnacksRosterResource.LoadRosterResources();
            resourceProcessors = BaseResourceProcessor.LoadProcessors();
            sciencePenalties = new DictionaryValueList<string, int>();
            snapshotMap = new Dictionary<Vessel, VesselSnackshot>();
            bodyVesselCountMap = new Dictionary<int, int>();
            initializeEventLists();
            onGameSettingsApplied();
        }

        public void Start()
        {
            //Get background converters
            backgroundConverters = SnacksBackgroundConverter.GetBackgroundConverters();

            //Look for intro screens to display
            ConfigNode[] introNodes = GameDatabase.Instance.GetConfigNodes("SNACKS_RESOURCE_INTRO");
            ConfigNode introNode;
            string introName;
            string description;
            string title;
            SnacksIntroScreen introScreen;
            for (int index = 0; index < introNodes.Length; index++)
            {
                introNode = introNodes[index];

                if (introNode.HasValue("name") && introNode.HasValue("description"))
                {
                    introName = introNode.GetValue("name");
                    if (!introStreensDisplayed.Contains(introName))
                    {
                        description = introNode.GetValue("description");
                        description = description.Replace("<br>", "\n\n");

                        introStreensDisplayed += introName;
                        introScreen = new SnacksIntroScreen(description);

                        if (introNodes[index].HasValue("title"))
                        {
                            title = introNode.GetValue("title");
                            introScreen.WindowTitle = title;
                        }

                        introScreen.SetVisible(true);
                    }
                }
            }

            // Clear out data entries for astronauts that no longer exist.
            string[] keys = crewData.Keys.ToArray();
            KerbalRoster roster = HighLogic.CurrentGame.CrewRoster;
            for (int index = 0; index < keys.Length; index++)
            {
                //rosterResources
                if (!roster.Exists(keys[index]))
                {
                    crewData.Remove(keys[index]);
                    if (rosterResources.ContainsKey(keys[index]))
                        rosterResources.Remove(keys[index]);
                }
            }
        }

        public void OnDestroy()
        {
            int count = resourceProcessors.Count;
            for (int index = 0; index < count; index++)
                resourceProcessors[index].Destroy();

            GameEvents.onVesselLoaded.Remove(onVesselLoaded);
            GameEvents.onCrewOnEva.Remove(onCrewOnEva);
            GameEvents.onCrewBoardVessel.Remove(onCrewBoardVessel);
            GameEvents.OnGameSettingsApplied.Remove(onGameSettingsApplied);
            GameEvents.onVesselWasModified.Remove(onVesselWasModified);
            GameEvents.onDockingComplete.Add(onDockingComplete);
            GameEvents.onVesselsUndocking.Add(onVesselsUndocking);
            GameEvents.onVesselWillDestroy.Remove(onVesselWillDestroy);
            GameEvents.OnVesselRecoveryRequested.Remove(onVesselRecoveryRequested);
            GameEvents.OnVesselRollout.Remove(onVesselRollout);
            GameEvents.onLevelWasLoadedGUIReady.Remove(onLevelLoaded);
            GameEvents.onVesselRecovered.Remove(onVesselRecovered);
            GameEvents.onVesselTerminated.Remove(onVesselTerminated);
            GameEvents.onKerbalAdded.Remove(onKerbalAdded);
            GameEvents.onKerbalNameChanged.Remove(onKerbalNameChanged);
            GameEvents.onKerbalRemoved.Remove(onKerbalRemoved);
            GameEvents.OnCrewmemberHired.Add(OnCrewmemberHired);
            GameEvents.OnCrewmemberSacked.Add(OnCrewmemberSacked);
            GameEvents.OnCrewmemberLeftForDead.Add(OnCrewmemberLeftForDead);
            GameEvents.onKerbalLevelUp.Remove(onKerbalLevelUp);
            GameEvents.onKerbalStatusChanged.Remove(onKerbalStatusChanged);
            GameEvents.onCrewTransferred.Remove(onCrewTransferred);
            GameEvents.onEditorPartPlaced.Remove(onEditorPartPlaced);
            GameEvents.onEditorPartPicked.Remove(onEditorPartPicked);
            GameEvents.onEditorPodPicked.Remove(onEditorPartPicked);
            GameEvents.onEditorShipModified.Remove(onEditorShipModified);
            GameEvents.onProtoCrewMemberLoad.Remove(onProtoCrewMemberLoad);
            GameEvents.onVesselGoOffRails.Remove(onVesselGoOffRails);
            GameEvents.onKerbalLevelUp.Remove(onKerbalLevelUp);
            GameEvents.onVesselSituationChange.Remove(onVesselSituationChange);
            GameEvents.onEditorLoad.Remove(onEditorLoad);
            GameEvents.onEditorStarted.Remove(onEditorStarted);
            GameEvents.onCommandSeatInteractionEnter.Remove(onCommandSeatInteractionEnter);
        }

        public override void OnLoad(ConfigNode node)
        {
            base.OnLoad(node);

            if (node.HasValue("exemptkerbals"))
                exemptKerbals = node.GetValue("exemptKerbals");

            if (node.HasValue("introStreensDisplayed"))
                introStreensDisplayed = node.GetValue("introStreensDisplayed");

            ConfigNode[] penalties = node.GetNodes("SCIENCE_PENALTY");
            foreach (ConfigNode penaltyNode in penalties)
            {
                sciencePenalties.Add(penaltyNode.GetValue("vesselID"), int.Parse(penaltyNode.GetValue("amount")));
            }

            //Converters
            if (node.HasValue("cycleStartTime"))
                double.TryParse(node.GetValue("cycleStartTime"), out cycleStartTime);

            //Processors
            if (node.HasNode(BaseResourceProcessor.ProcessorNode) && resourceProcessors != null)
            {
                ConfigNode[] consumerNodes = node.GetNodes(BaseResourceProcessor.ProcessorNode);
                ConfigNode processorNode;
                BaseResourceProcessor processor;
                Dictionary<string, ConfigNode> persistenceMap = new Dictionary<string, ConfigNode>();
                int index;

                //Build persistence map
                for (index = 0; index < consumerNodes.Length; index++)
                {
                    processorNode = consumerNodes[index];
                    if (processorNode.HasValue(BaseResourceProcessor.ProcessorNodeName))
                        persistenceMap.Add(processorNode.GetValue(BaseResourceProcessor.ProcessorNodeName), processorNode);
                }

                //Load persistence data
                int consumerCount = resourceProcessors.Count;
                for (index = 0; index < consumerCount; index++)
                {
                    processor = resourceProcessors[index];

                    //Find the persistence node
                    if (persistenceMap.ContainsKey(processor.name))
                    {
                        processor.OnLoad(persistenceMap[processor.name]);
                    }
                }
            }

            //Load astronaut data
            crewData = AstronautData.Load(node);

            //Events
            loadPersistentEventData(node);
        }

        public override void OnSave(ConfigNode node)
        {
            base.OnSave(node);
            ConfigNode configNode;

            //Exempt kerbals
            if (string.IsNullOrEmpty(exemptKerbals) == false)
                node.AddValue("exemptKerbals", exemptKerbals);

            if (!string.IsNullOrEmpty("introStreensDisplayed"))
                node.AddValue("introStreensDisplayed", introStreensDisplayed);

            //Science penalties
            foreach (string key in sciencePenalties.Keys)
            {
                configNode = new ConfigNode("SCIENCE_PENALTY");
                configNode.AddValue("vesselID", key);
                configNode.AddValue("amount", sciencePenalties[key].ToString());
                node.AddNode(configNode);
            }

            //Save astronaut data
            AstronautData.Save(crewData, node);

            //Procssors
            int consumerCount = resourceProcessors.Count;
            for (int index = 0; index < consumerCount; index++)
            {
                configNode = resourceProcessors[index].OnSave();
                if (configNode != null)
                    node.AddNode(configNode);
            }

            //Events
            savePersistentEventData(node);
        }
        #endregion

        #region Game Events
        private void OnCrewmemberHired(ProtoCrewMember astronaut, int activeAstronautsCount)
        {
            onKerbalAdded(astronaut);
        }

        private void OnCrewmemberSacked(ProtoCrewMember astronaut, int activeAstronautsCount)
        {
            onKerbalRemoved(astronaut);
        }

        private void OnCrewmemberLeftForDead(ProtoCrewMember astronaut, int activeAstronautsCount)
        {
            onKerbalRemoved(astronaut);
        }

        private void onKerbalLevelUp(ProtoCrewMember astronaut)
        {
            //Update roster resources
            string[] keys = rosterResources.Keys.ToArray();
            for (int index = 0; index < keys.Length; index++)
            {
                rosterResources[keys[index]].onKerbalLevelUp(astronaut);
            }

            //Now handle level up events
            StartCoroutine(processEvents(levelUpEvents));
        }

        private void onKerbalStatusChanged(ProtoCrewMember astronaut, ProtoCrewMember.RosterStatus previousStatus, ProtoCrewMember.RosterStatus newStatus)
        {
            if (newStatus == ProtoCrewMember.RosterStatus.Dead)
                onKerbalRemoved(astronaut);
        }

        private void onKerbalAdded(ProtoCrewMember astronaut)
        {
            if (!crewData.Contains(astronaut.name))
            {
                AstronautData astronautData = GetAstronautData(astronaut);

                //Give processors a chance to add their data
                int count = resourceProcessors.Count;
                BaseResourceProcessor processor;
                for (int index = 0; index < count; index++)
                {
                    processor = resourceProcessors[index];
                    processor.onKerbalAdded(astronaut);
                }
            }
        }

        private void onKerbalRemoved(ProtoCrewMember astronaut)
        {
            if (crewData.Contains(astronaut.name))
            {
                crewData.Remove(astronaut.name);

                //Give processors a chance to remove their data if needed.
                int count = resourceProcessors.Count;
                BaseResourceProcessor processor;
                for (int index = 0; index < count; index++)
                {
                    processor = resourceProcessors[index];
                    processor.onKerbalRemoved(astronaut);
                }
            }
        }

        private void onKerbalNameChanged(ProtoCrewMember astronaut, string previousName, string newName)
        {
            AstronautData astronautData = null;

            if (crewData.Contains(astronaut.name))
            {
                astronautData = GetAstronautData(astronaut);
                crewData.Remove(astronaut.name);
                astronautData.name = newName;
                crewData.Add(newName, astronautData);

                //Give processors a chance to update their data if needed.
                int count = resourceProcessors.Count;
                BaseResourceProcessor processor;
                for (int index = 0; index < count; index++)
                {
                    processor = resourceProcessors[index];
                    processor.onKerbalNameChanged(astronaut, previousName, newName);
                }
            }
        }

        private void onProtoCrewMemberLoad(GameEvents.FromToAction<ProtoCrewMember, ConfigNode> data)
        {
        }

        private void onCrewTransferred(GameEvents.HostedFromToAction<ProtoCrewMember, Part> data)
        {
            ProtoCrewMember astronaut = data.host;
            Part fromPart = data.from;
            Part toPart = data.to;

            if (ShouldRemoveSkills(astronaut))
                RemoveSkills(astronaut);
        }

        private void onCrewBoardVessel(GameEvents.FromToAction<Part, Part> data)
        {
            try
            {
                Part evaKerbal = data.from;
                Part boardedPart = data.to;

                //Inform all eva resources
                int count = snacksEVAResources.Count;
                for (int index = 0; index < count; index++)
                    snacksEVAResources[index].onCrewBoardedVessel(evaKerbal, boardedPart);

                //Give processors a chance to update their data if needed.
                ProtoCrewMember[] astronauts = boardedPart.protoModuleCrew.ToArray();
                count = resourceProcessors.Count;
                BaseResourceProcessor processor;
                for (int astronautIndex = 0; astronautIndex < astronauts.Length; astronautIndex++)
                {
                    for (int index = 0; index < count; index++)
                    {
                        processor = resourceProcessors[index];
                        processor.onKerbalBoardedVessel(astronauts[astronautIndex], boardedPart);
                    }

                    //Remove skills if needed
                    if (ShouldRemoveSkills(astronauts[astronautIndex]))
                        RemoveSkills(astronauts[astronautIndex]);
                }
            }
            catch (Exception ex)
            {
                Debug.Log("[Snacks] - OnCrewBoardVessel " + ex.Message + ex.StackTrace);
            }
        }

        private void onCrewOnEva(GameEvents.FromToAction<Part, Part> data)
        {
            try
            {
                Part evaKerbal = data.to;
                Part partExited = data.from;

                //Update the snacks EVA resource to reflect current game settings.
                SnacksEVAResource.snacksEVAResource.amount = SnacksProperties.SnacksPerMeal;
                SnacksEVAResource.snacksEVAResource.maxAmount = SnacksProperties.SnacksPerMeal;

                //Inform all eva resources
                int count = snacksEVAResources.Count;
                for (int index = 0; index < count; index++)
                    snacksEVAResources[index].onCrewEVA(evaKerbal, partExited);

                //Give processors a chance to update their data if needed.
                ProtoCrewMember astronaut = evaKerbal.vessel.GetVesselCrew()[0];
                count = resourceProcessors.Count;
                BaseResourceProcessor processor;
                for (int index = 0; index < count; index++)
                {
                    processor = resourceProcessors[index];
                    processor.onKerbalEVA(astronaut, partExited);
                }

                //Remove skills if needed
                if (ShouldRemoveSkills(astronaut))
                    RemoveSkills(astronaut);
            }
            catch (Exception ex)
            {
                Debug.Log("[Snacks] - OnCrewOnEva " + ex.Message + ex.StackTrace);
            }
        }

        private void onEditorShipModified(ShipConstruct ship)
        {
        }

        private void onEditorPartPicked(Part part)
        {
        }

        private void onEditorPartPlaced(Part part)
        {
        }

        private void onVesselTerminated(ProtoVessel protoVessel)
        {
        }

        private void onVesselSituationChange(GameEvents.HostedFromToAction<Vessel, Vessel.Situations> action)
        {
            if (!HighLogic.LoadedSceneIsFlight)
                return;
            if (!action.host == FlightGlobals.ActiveVessel)
                return;

            ProtoCrewMember[] astronauts = action.host.GetVesselCrew().ToArray();
            if (astronauts.Length <= 0)
                return;

            for (int index = 0; index < astronauts.Length; index++)
                RemoveSkillsIfNeeded(astronauts[index]);
        }

        private void onVesselRecovered(ProtoVessel protoVessel, bool someBool)
        {
            //Give processors a chance to remove their data if needed.
            int count = resourceProcessors.Count;
            BaseResourceProcessor processor;
            for (int index = 0; index < count; index++)
            {
                processor = resourceProcessors[index];
                processor.onVesselRecovered(protoVessel);
            }
        }

        private void onLevelLoaded(GameScenes scene)
        {
        }

        private void onVesselGoOffRails(Vessel vessel)
        {
            //Inform processors
            int count = resourceProcessors.Count;
            for (int index = 0; index < count; index++)
                resourceProcessors[index].onVesselGoOffRails(vessel);

            ProtoCrewMember[] astronauts = vessel.GetVesselCrew().ToArray();

            for (int index = 0; index < astronauts.Length; index++)
            {
                if (ShouldRemoveSkills(astronauts[index]))
                    RemoveSkills(astronauts[index]);
            }
        }

        private void onVesselRollout(ShipConstruct ship)
        {
            Part part;
            int count = 0;
            string[] keys = rosterResources.Keys.ToArray();
            ProtoCrewMember[] astronauts;
            for (int index = 0; index < ship.parts.Count; index++)
            {
                part = ship.parts[index];

                count = snacksPartResources.Count;
                for (int resourceIndex = 0; resourceIndex < count; resourceIndex++)
                    snacksPartResources[resourceIndex].addResourcesIfNeeded(part);

                //Get the crew and add any roster resources needed.
                if (keys.Length > 0)
                {
                    astronauts = part.protoModuleCrew.ToArray();

                    for (int astronautIndex = 0; astronautIndex < astronauts.Length; astronautIndex++)
                    {
                        for (int resourceIndex = 0; resourceIndex < keys.Length; resourceIndex++)
                            rosterResources[keys[resourceIndex]].addResourceIfNeeded(astronauts[astronautIndex]);
                    }
                }
            }
        }

        private void onEditorStarted()
        {
            int count = PartLoader.LoadedPartsList.Count;
            Part partPrefab;
            for (int index = 0; index < count; index++)
            {
                int resourceCount = snacksPartResources.Count;

                for (int resourceIndex = 0; resourceIndex < resourceCount; resourceIndex++)
                {
                    partPrefab = PartLoader.LoadedPartsList[index].partPrefab;
                    if (partPrefab.isKerbalEVA() || partPrefab.isVesselEVA || partPrefab.CrewCapacity == 0 || PartLoader.LoadedPartsList[index].TechHidden)
                        continue;
                    snacksPartResources[resourceIndex].addResourcesIfNeeded(partPrefab, PartLoader.LoadedPartsList[index]);
                }
            }
        }

        private void onEditorLoad(ShipConstruct ship, CraftBrowserDialog.LoadType loadType)
        {
            Part part;
            int count = 0;
            string[] keys = rosterResources.Keys.ToArray();
            for (int index = 0; index < ship.parts.Count; index++)
            {
                part = ship.parts[index];

                count = snacksPartResources.Count;
                for (int resourceIndex = 0; resourceIndex < count; resourceIndex++)
                    snacksPartResources[resourceIndex].addResourcesIfNeeded(part);
            }
        }

        private void onVesselRecoveryRequested(Vessel vessel)
        {
            int count = resourceProcessors.Count;

            for (int index = 0; index < count; index++)
            {
                resourceProcessors[index].onVesselRecoveryRequested(vessel);
            }
        }

        private void onVesselWillDestroy(Vessel vessel)
        {
        }

        private void onVesselWasModified(Vessel vessel)
        {
            onVesselLoaded(vessel);
        }

        private void onDockingComplete(GameEvents.FromToAction<Part, Part> action)
        {
            //Inform resource processors
            Vessel vessel = action.from.vessel;
            int count = resourceProcessors.Count;
            for (int index = 0; index < count; index++)
            {
                resourceProcessors[index].onVesselDockUndock(vessel);
            }
        }

        private void onVesselsUndocking(Vessel oldVessel, Vessel newVessel)
        {
            //Inform resource processors
            int count = resourceProcessors.Count;
            for (int index = 0; index < count; index++)
            {
                resourceProcessors[index].onVesselDockUndock(oldVessel);
                resourceProcessors[index].onVesselDockUndock(newVessel);
            }
        }

        private void onGameSettingsApplied()
        {
            int count = resourceProcessors.Count;
            for (int index = 0; index < count; index++)
            {
                resourceProcessors[index].OnGameSettingsApplied();
            }

            count = snacksPartResources.Count;
            SnacksPartResource resource = null;
            for (int index = 0; index < count; index++)
            {
                resource = snacksPartResources[index];
                if (resource.resourceName == SnacksProperties.SnacksResourceName)
                    break;
            }
            if (resource != null && resource.unitsPerDay != 0 && resource.daysLifeSupport != 0)
            {
                resource.unitsPerDay = SnacksProperties.SnacksPerMeal * SnacksProperties.MealsPerDay;
            }
        }

        private void onCommandSeatInteractionEnter(KerbalEVA kerbalEVA, bool boardedSeat)
        {
            // Unfortunately we don't know what seat or what vessel we have boarded or left.
        }

        private void onVesselLoaded(Vessel vessel)
        {
            if (!HighLogic.LoadedSceneIsFlight)
                return;
            int count = 0;

            string objectName = vessel.name;

            if (vessel.isEVA || objectName.Contains("EVA"))
            {
                // A kerbal that just left a command seat or a kerbal that's already on EVA when the vessel is loaded will have a crew count.
                if (HighLogic.LoadedSceneIsFlight && vessel.GetVesselCrew().Count > 0)
                {
                    GameEvents.FromToAction<Part, Part> evaData = new GameEvents.FromToAction<Part, Part>();
                    evaData.to = vessel.rootPart;
                    onCrewOnEva(evaData);
                }
                return;
            }

            //Inform all part resources
            count = snacksPartResources.Count;
            for (int index = 0; index < count; index++)
                snacksPartResources[index].addResourcesIfNeeded(vessel);

            //Inform all roster resources
            count = rosterResources.Count;
            string[] keys = rosterResources.Keys.ToArray();
            SnacksRosterResource resource;
            for (int index = 0; index < keys.Length; index++)
            {
                resource = rosterResources[keys[index]];
                resource.addResourceIfNeeded(vessel);
            }

            //Inform resource processors
            count = resourceProcessors.Count;
            for (int index = 0; index < count; index++)
            {
                resourceProcessors[index].onVesselLoaded(vessel);
            }
        }
        #endregion

        #region Window Management
        List<IManagedSnackWindow> managedWindows = new List<IManagedSnackWindow>();

        public void OnGUI()
        {
            int totalWindows = managedWindows.Count;
            if (totalWindows == 0)
                return;
            IManagedSnackWindow managedWindow;

            for (int index = 0; index < totalWindows; index++)
            {
                managedWindow = managedWindows[index];
                if (managedWindow.IsVisible())
                    managedWindow.DrawWindow();
            }
        }

        public void RegisterWindow(IManagedSnackWindow managedWindow)
        {
            if (managedWindows.Contains(managedWindow) == false)
                managedWindows.Add(managedWindow);
        }

        public void UnregisterWindow(IManagedSnackWindow managedWindow)
        {
            if (managedWindows.Contains(managedWindow))
                managedWindows.Remove(managedWindow);
        }
        #endregion

        #region Static API
        /// <summary>
        /// Adds the stress to crew if Stress is enabled. This is primarily
        /// used by 3rd party mods like BARIS.
        /// </summary>
        /// <param name="vessel">The Vessel to query for crew.</param>
        /// <param name="stressAmount">The amount of Stress to add.</param>
        public static void AddStressToCrew(Vessel vessel, float stressAmount)
        {
            //Make sure that stress is enabled
            if (!Instance.rosterResources.ContainsKey(StressProcessor.StressResourceName))
                return;

            //Get the vessel crew
            ProtoCrewMember[] astronauts;
            AstronautData astronautData;

            //Get vessel crew
            if (vessel.loaded)
                astronauts = vessel.GetVesselCrew().ToArray();
            else
                astronauts = vessel.protoVessel.GetVesselCrew().ToArray();

            //Clear out exempt crew
            List<ProtoCrewMember> nonExemptCrew = new List<ProtoCrewMember>();
            for (int index = 0; index < astronauts.Length; index++)
            {
                astronautData = Instance.GetAstronautData(astronauts[index]);
                if (astronautData.isExempt)
                    continue;
                nonExemptCrew.Add(astronauts[index]);
            }
            if (nonExemptCrew.Count == 0)
                return;
            astronauts = nonExemptCrew.ToArray();

            //Now Add Stress
            SnacksRosterResource resource;
            for (int index = 0; index < astronauts.Length; index++)
            {
                //Get astronaut data
                astronautData = Instance.GetAstronautData(astronauts[index]);
                if (!astronautData.rosterResources.ContainsKey(StressProcessor.StressResourceName))
                    continue;

                //Increase Stress
                resource = astronautData.rosterResources[StressProcessor.StressResourceName];
                if (!astronauts[index].isBadass)
                    resource.amount += stressAmount;
                else
                    resource.amount += (stressAmount * 0.5f);
                if (resource.amount >= resource.maxAmount)
                    resource.amount = resource.maxAmount;
                astronautData.rosterResources[StressProcessor.StressResourceName] = resource;

                //Fire event
                onRosterResourceUpdated.Fire(vessel, resource, astronautData, astronauts[index]);
            }
        }

        /// <summary>
        /// Formats the supplied seconds into a string.
        /// </summary>
        /// <param name="secondsToFormat">The number of seconds to format.</param>
        /// <param name="showCompact">A flag to indicate whether or not to show the compact form.</param>
        /// <returns></returns>
        public static string FormatTime(double secondsToFormat, bool showCompact = false)
        {
            StringBuilder timeBuilder = new StringBuilder();
            double seconds = secondsToFormat;
            double years = 0;
            double days = 0;
            double hours = 0;
            double minutes = 0;

            //Make sure we have calculated our seconds per day
            GetSecondsPerDay();

            //Years
            years = Math.Floor(seconds / secondsPerYear);
            if (years >= 1.0)
            {
                seconds -= years * secondsPerYear;
            }
            else
            {
                years = 0;
            }

            //Days
            days = Math.Floor(seconds / secondsPerDay);
            if (days >= 1.0)
            {
                seconds -= days * secondsPerDay;
            }
            else
            {
                days = 0;
            }

            //Hours
            hours = Math.Floor(seconds / 3600);
            if (hours >= 1.0)
            {
                seconds -= hours * 3600;
            }
            else
            {
                seconds = 0;
            }

            //Minutes
            minutes = Math.Floor(seconds / 60);
            if (minutes >= 1.0)
            {
                seconds -= minutes * 60;
            }
            else
            {
                minutes = 0;
            }

            if (showCompact)
            {
                if (years > 0)
                    timeBuilder.Append(string.Format("Y{0:n2}:", years));
                if (days > 0)
                    timeBuilder.Append(string.Format("D{0:n2}:", days));
                if (hours > 0)
                    timeBuilder.Append(string.Format("H{0:n2}:", hours));
                if (minutes > 0)
                    timeBuilder.Append(string.Format("M{0:n2}:", minutes));
                if (seconds > 0.0001)
                    timeBuilder.Append(string.Format("S{0:n2}", seconds));
            }
            else
            {
                if (years > 0)
                    timeBuilder.Append(string.Format(Localizer.Format("#LOC_TIME1"), years));//#LOC_TIME1={0:n2} Years,
                if (days > 0)
                    timeBuilder.Append(string.Format(Localizer.Format("#LOC_TIME2"), days));//#LOC_TIME2={0:n2} Days,
                if (hours > 0)
                    timeBuilder.Append(string.Format(Localizer.Format("#LOC_TIME3"), hours));//#LOC_TIME3={0:n2} Hours,
                if (minutes > 0)
                    timeBuilder.Append(string.Format(Localizer.Format("#LOC_TIME4"), minutes));//#LOC_TIME4={0:n2} Minutes,
                if (seconds > 0.0001)
                    timeBuilder.Append(string.Format(Localizer.Format("#LOC_TIME5"), seconds));//#LOC_TIME5={0:n2} Seconds,
            }

            string timeDisplay = timeBuilder.ToString();
            char[] trimChars = { ',' };
            timeDisplay = timeDisplay.TrimEnd(trimChars);
            return timeDisplay;
        }

        /// <summary>
        /// Gets the number of seconds per day on the homeworld.
        /// </summary>
        /// <returns>The lenght of the solar day in seconds of the homeworld.</returns>
        public static double GetSecondsPerDay()
        {
            if (secondsPerDay > 0)
                return secondsPerDay;

            //Find homeworld
            int count = FlightGlobals.Bodies.Count;
            CelestialBody body = null;
            for (int index = 0; index < count; index++)
            {
                body = FlightGlobals.Bodies[index];
                if (body.isHomeWorld)
                    break;
                else
                    body = null;
            }
            if (body == null)
            {
                secondsPerYear = 21600 * 426.08;
                secondsPerDay = 21600;
                return secondsPerDay;
            }

            //Also get seconds per year
            secondsPerYear = body.orbit.period;

            //Return solar day length
            secondsPerDay = body.solarDayLength;
            return secondsPerDay;
        }

        /// <summary>
        /// Gets the solar flux based on vessel location.
        /// </summary>
        /// <param name="vessel">The vessel to query.</param>
        /// <returns>The level of solar flux at the vessel's location.</returns>
        public static double GetSolarFlux(Vessel vessel)
        {
            double solarFlux = 0;

            if (vessel.loaded)
            {
                //If the vessel is not on or orbiting a planet that's orbiting a star, then find the nearest parent planet that is.
                CelestialBody body = vessel.mainBody;
                while (body.referenceBody != null && body.referenceBody.scaledBody.GetComponentsInChildren<SunShaderController>(true).Length <= 0)
                    body = body.referenceBody;

                //If the vessel itself is orbiting a star then we'l use its orbit parameters instead.
                Orbit orbit = null;
                if (vessel.mainBody.scaledBody.GetComponentsInChildren<SunShaderController>(true).Length > 0)
                {
                    orbit = vessel.orbit;
                    body = vessel.mainBody;
                }
                else
                {
                    orbit = body.orbit;
                    body = body.referenceBody;
                }

                //Get solar luminosity from physics
                double solarLuminosity = PhysicsGlobals.SolarLuminosity;

                //Calculate solar luminosity for the star that the celestial body is orbiting if Kopernicus is installed.
                //solarLuminosity = Math.Pow(homeBody.orbit.semiMajorAxis, 2.0) * 4.0 * Math.PI * localStarLuminosityFromItsConfigFile
                ConfigNode[] bodyNodes = GameDatabase.Instance.GetConfigNodes("Kopernicus");
                if (bodyNodes.Length > 0 && bodyNodes[0].HasNode("Body"))
                {
                    ConfigNode node;
                    string bodyName;

                    bodyNodes = bodyNodes[0].GetNodes("Body");
                    for (int index = 0; index < bodyNodes.Length; index++)
                    {
                        node = bodyNodes[index];

                        //Find the config that matches the name of the celestial body.
                        if (!node.HasValue("name"))
                            continue;
                        bodyName = node.GetValue("name");
                        if (bodyName != body.name)
                            continue;

                        //Now get luminostiy
                        if (!node.HasNode("ScaledVersion"))
                            continue;
                        node = node.GetNode("ScaledVersion");

                        if (!node.HasNode("Light"))
                            continue;
                        node = node.GetNode("Light");

                        if (!node.HasValue("luminosity"))
                            continue;
                        double.TryParse(node.GetValue("luminosity"), out solarLuminosity);

                        //Find the homeworld. We need it's orbit parameters.
                        CelestialBody homeBody = null;
                        int bodyCount = FlightGlobals.Bodies.Count;
                        for (int bodyIndex = 0; bodyIndex < bodyCount; bodyIndex++)
                        {
                            homeBody = FlightGlobals.Bodies[bodyIndex];
                            if (homeBody.isHomeWorld)
                                break;
                        }

                        //Account for homeworlds that are moons of some planet orbiting some star.
                        while (homeBody.referenceBody != null && homeBody.referenceBody.scaledBody.GetComponentsInChildren<SunShaderController>(true).Length <= 0)
                            homeBody = homeBody.referenceBody;

                        //Now calculate solar luminosity based on homeworld's orbit
                        solarLuminosity = solarLuminosity * 4.0 * Math.PI * Math.Pow(homeBody.orbit.semiMajorAxis, 2.0);
                        break;
                    }
                }

                //Now calculate solar flux.
                solarFlux = solarLuminosity / (Math.PI * 4.0 * Math.Pow(orbit.semiMajorAxis, 2.0));
            }

            return solarFlux;
        }
        #endregion

        #region Processors API
        /// <summary>
        /// Creates a new precondition based on the config node data passed in.
        /// </summary>
        /// <param name="node">The ConfigNode containing data to parse.</param>
        /// <returns>A BasePrecondition containing the precondition object, or null if the config node couldn't be parsed.</returns>
        public BasePrecondition CreatePrecondition(ConfigNode node)
        {
            //Get the name
            string name = string.Empty;
            if (node.HasValue(BasePrecondition.PreconditionName))
                name = node.GetValue(BasePrecondition.PreconditionName);
            if (string.IsNullOrEmpty(name))
                return null;

            //Now create the precondition
            switch (name)
            {
                case "CheckProcessorResult":
                    return new CheckProcessorResult(node);

                case "CheckRandomChance":
                    return new CheckRandomChance(node);

                case "CheckCondition":
                    return new CheckCondition(node);

                case "CheckKeyValue":
                    return new CheckKeyValue(node);

                case "CheckGravityLevel":
                    return new CheckGravityLevel(node);

                case "CheckResource":
                    return new CheckResource(node);

                case "CheckCrewCount":
                    return new CheckCrewCount(node);

                case "CheckBadass":
                    return new CheckBadass(node);

                case "CheckHomeworldConnection":
                    return new CheckHomeworldConnection(node);

                case "CheckCourage":
                    return new CheckCourage(node);

                case "CheckStupidity":
                    return new CheckStupidity(node);

                case "CheckTrait":
                    return new CheckTrait(node);

                case "CheckSkill":
                    return new CheckSkill(node);

                case "CheckSkillLevel":
                    return new CheckSkillLevel(node);

                case "CheckBreathableAir":
                    return new CheckBreathableAir(node);

                default:
                    break;
            }

            return null;
        }

        /// <summary>
        /// Creates a new outcome based on the config node data passed in.
        /// </summary>
        /// <returns>The outcome corresponding to the desired config.</returns>
        /// <param name="node">The ConfigNode containing data to parse.</param>
        public BaseOutcome CreateOutcome(ConfigNode node)
        {
            //Get the name of the outcome
            string outcomeName = string.Empty;
            if (node.HasValue(BaseOutcome.OutcomeName))
                outcomeName = node.GetValue(BaseOutcome.OutcomeName);
            if (string.IsNullOrEmpty(outcomeName))
                return null;

            //Now create the appropriate outcome
            switch (outcomeName)
            {
                case "DeathPenalty":
                    return new DeathPenalty(node);

                case "FaintPenalty":
                    return new FaintPenalty(node);

                case "FundingPenalty":
                    return new FundingPenalty(node);

                case "RepPenalty":
                    return new RepPenalty(node);

                case "SciencePenalty":
                    return new SciencePenalty(node);

                case "ProduceResource":
                    return new ProduceResource(node);

                case "ConsumeResource":
                    return new ConsumeResource(node);

                case "SetCondition":
                    return new SetCondition(node);

                case "ClearCondition":
                    return new ClearCondition(node);

                case "SetKeyValue":
                    return new SetKeyValue(node);

                case "ClearKeyValue":
                    return new ClearKeyValue(node);

                default:
                    break;
            }
            return null;
        }
        #endregion

        #region Helpers
        private IEnumerator<YieldInstruction> runSnackCycle(Vessel vessel, double elapsedTime)
        {
            yield return new WaitForFixedUpdate();

            int crewCount = 0;
            int crewCapacity = 0;
            int count;
            List<SnacksBackgroundConverter> converters;
            SnacksBackgroundConverter converter;
            ProtoCrewMember[] astronauts;

            //Get crew count and crew capacity
            if (vessel.loaded)
            {
                crewCapacity = vessel.GetCrewCapacity();

                astronauts = vessel.GetVesselCrew().ToArray();
            }
            else
            {
                ProtoVessel protoVessel = vessel.protoVessel;
                ProtoPartSnapshot protoPart;
                ConfigNode node;
                int partCapacity = 0;

                astronauts = protoVessel.GetVesselCrew().ToArray();

                if (!vessel.isEVA)
                {
                    count = protoVessel.protoPartSnapshots.Count;
                    for (int index = 0; index < count; index++)
                    {
                        protoPart = protoVessel.protoPartSnapshots[index];
                        node = protoPart.partInfo.partConfig;
                        if (!node.HasValue("CrewCapacity"))
                            continue;
                        partCapacity = 0;
                        int.TryParse(node.GetValue("CrewCapacity"), out partCapacity);
                        crewCapacity += partCapacity;
                    }

                    yield return new WaitForFixedUpdate();
                }
                else
                {
                    crewCapacity = 1;
                }
            }

            //Count up the crew, but skip any that are unowned.
            for (int index = 0; index < astronauts.Length; index++)
            {
                if (astronauts[index].type != ProtoCrewMember.KerbalType.Unowned)
                    crewCount += 1;
            }

            //Continue processing if we have crew
            if (crewCount > 0)
            {
                //Run background converers
                if (backgroundConverters.ContainsKey(vessel) && !vessel.loaded)
                {
                    converters = backgroundConverters[vessel];
                    count = converters.Count;

                    for (int index = 0; index < count; index++)
                    {
                        converter = converters[index];

                        if (converter.IsActivated && !converter.isMissingResources && !converter.isContainerFull)
                        {
                            runConverter(converter, elapsedTime, vessel.protoVessel);
                            yield return new WaitForFixedUpdate();
                        }
                    }
                }
                yield return new WaitForFixedUpdate();

                //Run processors
                count = resourceProcessors.Count;
                for (int index = 0; index < count; index++)
                {
                    resourceProcessors[index].ProcessResources(vessel, elapsedTime, crewCount, crewCapacity);
                    yield return new WaitForFixedUpdate();
                }
            }

            vesselsBeingProcessed -= 1;
            yield return new WaitForFixedUpdate();
        }

        protected IEnumerator<YieldInstruction> processEvents(Dictionary<string, SnacksEvent> eventList)
        {
            string[] keys = eventList.Keys.ToArray();
            SnacksEvent snacksEvent;

            for (int index = 0; index < keys.Length; index++)
            {
                snacksEvent = eventList[keys[index]];
                snacksEvent.ProcessEvent(secondsPerCycle);
                yield return new WaitForFixedUpdate();
            }
            yield return new WaitForFixedUpdate();
        }

        protected IEnumerator<YieldInstruction> playEventCard()
        {
            yield return new WaitForFixedUpdate();
        }

        protected void initializeEventLists()
        {
            ConfigNode[] nodes = GameDatabase.Instance.GetConfigNodes(SnacksEvent.SNACKS_EVENT);
            SnacksEvent snacksEvent;

            postProcessEvents = new Dictionary<string, SnacksEvent>();
            levelUpEvents = new Dictionary<string, SnacksEvent>();
            eventCards = new Dictionary<string, SnacksEvent>();

            for (int index = 0; index < nodes.Length; index++)
            {
                snacksEvent = new SnacksEvent(nodes[index]);
                if (!string.IsNullOrEmpty(snacksEvent.name))
                {
                    switch (snacksEvent.eventCategory)
                    {
                        default:
                        case SnacksEventCategories.categoryPostProcessCycle:
                            postProcessEvents.Add(snacksEvent.name, snacksEvent);
                            break;

                        case SnacksEventCategories.categoryKerbalLevelUp:
                            levelUpEvents.Add(snacksEvent.name, snacksEvent);
                            break;

                        case SnacksEventCategories.categoryEventCard:
                            eventCards.Add(snacksEvent.name, snacksEvent);
                            break;
                    }
                }
            }
        }

        protected void savePersistentEventData(ConfigNode node)
        {
            string[] keys = postProcessEvents.Keys.ToArray();

            //Event persistent data
            for (int index = 0; index < keys.Length; index++)
                node.AddNode(postProcessEvents[keys[index]].Save());

            keys = levelUpEvents.Keys.ToArray();
            for (int index = 0; index < keys.Length; index++)
                node.AddNode(levelUpEvents[keys[index]].Save());

            keys = eventCards.Keys.ToArray();
            for (int index = 0; index < keys.Length; index++)
                node.AddNode(eventCards[keys[index]].Save());
        }

        protected void loadPersistentEventData(ConfigNode node)
        {
            ConfigNode[] nodes = node.GetNodes(SnacksEvent.SNACKS_EVENT);
            ConfigNode eventNode;
            SnacksEvent snacksEvent;
            string eventName;
            SnacksEventCategories eventCategory;

            //Event persistent data
            for (int index = 0; index < nodes.Length; index++)
            {
                eventNode = nodes[index];

                eventName = eventNode.GetValue(SnacksEvent.SnacksEventName);
                eventCategory = (SnacksEventCategories)Enum.Parse(typeof(SnacksEventCategories), eventNode.GetValue(SnacksEvent.SnacksEventCategory));

                switch (eventCategory)
                {
                    default:
                    case SnacksEventCategories.categoryPostProcessCycle:
                        if (postProcessEvents.ContainsKey(eventName))
                        {
                            snacksEvent = postProcessEvents[eventName];
                            snacksEvent.Load(nodes[index]);
                        }
                        break;

                    case SnacksEventCategories.categoryKerbalLevelUp:
                        if (levelUpEvents.ContainsKey(eventName))
                        {
                            snacksEvent = levelUpEvents[eventName];
                            snacksEvent.Load(nodes[index]);
                        }
                        break;

                    case SnacksEventCategories.categoryEventCard:
                        if (eventCards.ContainsKey(eventName))
                        {
                            snacksEvent = eventCards[eventName];
                            snacksEvent.Load(nodes[index]);
                        }
                        break;
                }
            }
        }
        #endregion
    }
}
