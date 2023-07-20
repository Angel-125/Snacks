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

using KSP.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Snacks
{
    /// <summary>
    /// This is a helper class to handle the unique conditions of a kerbal leveling up with the Stress resource.
    /// </summary>
    public class StressRosterResource: SnacksRosterResource
    {
        public override void onKerbalLevelUp(ProtoCrewMember astronaut)
        {
            //NOP
        }
    }

    /// <summary>
    /// The Stress processor is designed to work with the Stress roster resource.
    /// Essentially, Stress is an abstracted habitation mechanic that takes
    /// into account a variety of different events. The main thing that causes
    /// Stress is being aboard a vessel; you don't want to send kerbals to Jool
    /// in a Mk1 command pod! NASA allocates 25 m^3 of space per astronaut per
    /// year aboard the ISS, and Stress is based off that number. The larger the
    /// habitable volume, the greater a kerbal's maximum Stress becomes, and it's
    /// dynamically updated whenever a kerbal changes craft. Assuming no other
    /// events, a kerbal will accumulate 1 point of Stress per day, and when the
    /// kerbal reaches it's maximum Stress, bad things happen.
    /// </summary>
    public class StressProcessor : BaseResourceProcessor
    {
        #region Constants
        public const string StressResourceName = "Stress";

        const string StressConditionName = "Stressed Out";//#LOC_INFO_STRESSOUTNAME = Stressed Out
        const string StressPlayerMessage = "is stressed out and cannot work!";//#LOC_INFO_STRESSOUT = is stressed out and cannot work!
        const string StressRecoveryMessage = "recovered from stress.";//#LOC_INFO_STRESSOUTRECOVER = recovered from stress.

        /// <summary>
        /// The first N seats use the multiplier instead of the N^3 formula.
        /// </summary>
        const int MaxSeatsForMultiplier = 3;

        /// <summary>
        /// How much Space a single seat provides, assuming that the vessel's
        /// number of seats is less than or equal to MaxSeatsForMultiplier.
        /// </summary>
        const float SpacePerSeatMultiplier = 16.0f;
        #endregion

        #region Constructors
        public StressProcessor(): base()
        {
            name = StressResourceName;
        }
        #endregion

        #region Overrides
        public override void Initialize()
        {
            base.Initialize();

            //Replace the existing roster resource with a customized Stress version. We calculate max roster amount in the processor.
            SnacksRosterResource resource = SnacksScenario.Instance.rosterResources[StressResourceName];
            StressRosterResource stressResource = new StressRosterResource();
            stressResource.resourceName = resource.resourceName;
            stressResource.displayName = resource.displayName;
            stressResource.amount = 0;
            stressResource.maxAmount = 0;
            stressResource.experienceBonusAmount = 0;
            stressResource.experienceBonusMaxAmount = resource.experienceBonusMaxAmount;
            SnacksScenario.Instance.rosterResources[StressResourceName] = stressResource;
            stressResource.statusFormat = resource.statusFormat;

            secondsPerCycle = SnacksScenario.GetSecondsPerDay();

            outcomes.Add(new OnStrikePenalty(Localizer.Format("#LOC_INFO_STRESSOUTNAME"), false, Localizer.Format("#LOC_INFO_STRESSOUT")));//

            SnacksScenario.onRosterResourceUpdated.Add(onRosterResourceUpdated);
        }

        public override void Destroy()
        {
            base.Destroy();

            SnacksScenario.onRosterResourceUpdated.Remove(onRosterResourceUpdated);
        }

        public override void GetRosterResourceEstimatesForEditor(int currentCrewCount, int crewCapacity, StringBuilder results, ShipConstruct ship)
        {
            //Calculate the space available.
            float space = CalculateSpace(currentCrewCount, crewCapacity);

            results.AppendLine(Localizer.Format("#LOC_GUI_STRESSMAXES", currentCrewCount.ToString(),space));//#LOC_GUI_STRESSMAXES=Estimated Max Stress: A crew of <<1>> will be Stressed Out after <<2>> days.
            results.AppendLine(Localizer.Format("#LOC_GUI_STRESSMAXESNOTE"));//#LOC_GUI_STRESSMAXESNOTE=This is based on a kerbal with no experience. Experienced kerbals can last longer.
            results.AppendLine(" ");
        }

        public override void onKerbalBoardedVessel(ProtoCrewMember astronaut, Part part)
        {
            if (part == null || part.vessel == null)
                return;
            ProtoCrewMember[] astronauts = null;
            AstronautData astronautData = null;
            Vessel vessel = part.vessel;
            SnacksProcessorResult result = new SnacksProcessorResult();
            SnacksRosterResource resource;
            bool completedSuccessfully = true;

            //Get vessel crew
            astronauts = SnacksScenario.Instance.GetNonExemptCrew(vessel);
            if (astronauts == null)
                return;

            //Update max space
            updateMaxSpace(part.vessel);

            result.crewCount = astronauts.Length;
            result.crewCapacity = astronauts.Length;
            result.resourceName = StressResourceName;
            result.completedSuccessfully = true;

            //Now make sure kerbals aren't stressed out, or apply outcomes if they are.
            for (int index = 0; index < astronauts.Length; index++)
            {
                //Reset flag
                completedSuccessfully = true;

                //Get astronaut data
                astronautData = SnacksScenario.Instance.GetAstronautData(astronauts[index]);
                if (astronautData == null)
                    continue;
                if (!astronautData.rosterResources.ContainsKey(StressResourceName))
                    continue;

                //Get the Stress roster resource
                resource = astronautData.rosterResources[StressResourceName];

                //Check for failure conditions
                if (resource.amount >= resource.maxAmount)
                {
                    //Set the flags
                    completedSuccessfully = false;
                    result.completedSuccessfully = false;

                    //Incease affected kerbal count
                    result.affectedKerbalCount += 1;

                    //Add astronaut to the affected list
                    if (result.afftectedAstronauts == null)
                        result.afftectedAstronauts = new List<ProtoCrewMember>();
                    result.afftectedAstronauts.Add(astronauts[index]);
                }
            }

            //Process results
            if (!completedSuccessfully)
                applyFailureOutcomes(vessel, result);
            else
                removeFailureOutcomes(vessel);
        }

        public override void ProcessResources(Vessel vessel, double elapsedTime, int crewCount, int crewCapacity)
        {
            ProtoCrewMember[] astronauts = null;
            AstronautData astronautData = null;
            SnacksProcessorResult result = new SnacksProcessorResult();
            float stress = 0;

            remainingTime += elapsedTime;
            while (remainingTime >= secondsPerCycle)
            {
                //Update remaining time
                remainingTime -= secondsPerCycle;

                //Get the crew manifest
                astronauts = SnacksScenario.Instance.GetNonExemptCrew(vessel);
                if (astronauts == null)
                    return;
                if (astronauts.Length == 0)
                    return;

                //Setup result
                productionResults.Clear();
                result.crewCount = crewCount;
                result.crewCapacity = crewCapacity;
                result.resourceName = StressResourceName;
                result.completedSuccessfully = true;

                //Update max space
                updateMaxSpace(vessel);

                //Now increase stress in the vessel's crew.
                SnacksRosterResource resource;
                for (int index = 0; index < astronauts.Length; index++)
                {                    
                    //Get astronaut data
                    astronautData = SnacksScenario.Instance.GetAstronautData(astronauts[index]);
                    if (astronautData == null)
                        continue;
                    if (!astronautData.rosterResources.ContainsKey(StressResourceName))
                        continue;

                    //Get the Stress roster resource
                    resource = astronautData.rosterResources[StressResourceName];

                    //Increase stress; stupidity matters
                    stress = (1 - astronauts[index].stupidity);
                    stress = UnityEngine.Random.Range(stress/2.0f, stress);

                    //Is kerbal a badass? Then reduce acquired stress
                    if (astronauts[index].isBadass)
                        stress *= 0.5f;

                    //Account for homerworld or world with oxygen atmosphere
                    if (vessel.mainBody.isHomeWorld && vessel.LandedOrSplashed)
                        stress *= 0.25f;
                    else if (vessel.mainBody.atmosphere && vessel.mainBody.atmosphereContainsOxygen && vessel.LandedOrSplashed)
                        stress *= 0.75f;

                    resource.amount += stress;
                    astronautData.rosterResources[StressResourceName] = resource;

                    if (SnacksProperties.DebugLoggingEnabled)
                        Debug.Log("[" + name + "] - " + astronautData.name + ": Stress added: " + stress.ToString() + " Status: "+ resource.GetStatusDisplay());

                    //Check for failure conditions
                    if (resource.amount >= resource.maxAmount)
                    {
                        //Set the flag
                        result.completedSuccessfully = false;

                        //Incease affected kerbal count
                        result.affectedKerbalCount += 1;

                        //Add astronaut to the affected list
                        if (result.afftectedAstronauts == null)
                            result.afftectedAstronauts = new List<ProtoCrewMember>();
                        result.afftectedAstronauts.Add(astronauts[index]);
                    }
                }

                //Process results
                //First clear the failure outcomes, then apply to any who are affected.
                removeFailureOutcomes(vessel, false);
                if (!result.completedSuccessfully)
                    applyFailureOutcomes(vessel, result);

                //Record results
                productionResults.Add(StressResourceName, result);
            }
        }

        public override void onVesselLoaded(Vessel vessel)
        {
            ProtoCrewMember[] astronauts = null;
            AstronautData astronautData = null;
            SnacksProcessorResult result = new SnacksProcessorResult();

            //Get the crew manifest
            if (vessel.loaded)
                astronauts = vessel.GetVesselCrew().ToArray();
            else
                astronauts = vessel.protoVessel.GetVesselCrew().ToArray();
            if (astronauts.Length == 0)
                return;

            //Update max space
            updateMaxSpace(vessel);

            //Setup result
            productionResults.Clear();
            result.crewCount = vessel.GetCrewCount();
            result.crewCapacity = vessel.GetCrewCapacity();
            result.resourceName = StressResourceName;
            result.completedSuccessfully = true;

            SnacksRosterResource resource;
            for (int index = 0; index < astronauts.Length; index++)
            {
                //Get astronaut data
                astronautData = SnacksScenario.Instance.GetAstronautData(astronauts[index]);
                if (astronautData == null)
                    continue;
                if (!astronautData.rosterResources.ContainsKey(StressResourceName))
                    continue;

                //Get the Stress roster resource
                resource = astronautData.rosterResources[StressResourceName];

                //Check for failure conditions
                if (resource.amount >= resource.maxAmount)
                {
                    //Set the flag
                    result.completedSuccessfully = false;

                    //Incease affected kerbal count
                    result.affectedKerbalCount += 1;

                    //Add astronaut to the affected list
                    if (result.afftectedAstronauts == null)
                        result.afftectedAstronauts = new List<ProtoCrewMember>();
                    result.afftectedAstronauts.Add(astronauts[index]);
                }
            }

            //Process results
            //First clear the failure outcomes, then apply to any who are affected.
            removeFailureOutcomes(vessel, false);
            if (!result.completedSuccessfully)
                applyFailureOutcomes(vessel, result);
        }

        public override void onVesselGoOffRails(Vessel vessel)
        {
            updateMaxSpace(vessel);
        }

        #endregion

        #region Helpers
        protected void onRosterResourceUpdated(Vessel vessel, SnacksRosterResource rosterResource, AstronautData astronautData, ProtoCrewMember astronaut)
        {
            //Make sure it's a resource we're interested in.
            if (!rosterResource.resourceName.Contains(StressConditionName))
                return;

            //If the resource has gone down below max, then remove the stress condition.
            if (rosterResource.amount < rosterResource.maxAmount && astronautData.conditionSummary.Contains(StressConditionName))
            {
                astronautData.ClearCondition(StressConditionName);
                SnacksScenario.Instance.RestoreSkillsIfNeeded(astronaut);
                ScreenMessages.PostScreenMessage(astronaut.name + " " + Localizer.Format("#LOC_INFO_STRESSOUTRECOVER"), 5.0f, ScreenMessageStyle.UPPER_LEFT);
            }

            //If the resource has maxed out then add the stress condition
            else if (rosterResource.amount >= rosterResource.maxAmount && !astronautData.conditionSummary.Contains(StressConditionName))
            {
                astronautData.SetCondition(StressConditionName);
                SnacksScenario.Instance.RemoveSkillsIfNeeded(astronaut);
                ScreenMessages.PostScreenMessage(vessel.vesselName + ": " + astronaut.name + " " + StressPlayerMessage, 5.0f, ScreenMessageStyle.UPPER_LEFT);
            }
        }

        protected void updateMaxSpace(Vessel vessel)
        {
            ProtoCrewMember[] astronauts = null;
            AstronautData astronautData = null;

            //Get the crew manifest
            if (vessel.loaded)
                astronauts = vessel.GetVesselCrew().ToArray();
            else
                astronauts = vessel.protoVessel.GetVesselCrew().ToArray();
            if (astronauts.Length == 0)
                return;

            //Calculate how much Space the vessel has.
            float space = CalculateSpace(vessel);

            //Get experience bonus
            double stressExperienceBonus = SnacksScenario.Instance.rosterResources[StressResourceName].experienceBonusMaxAmount;

            //Go through the crew and update their max Stress.
            SnacksRosterResource resource;
            for (int index = 0; index < astronauts.Length; index++)
            {
                astronautData = SnacksScenario.Instance.GetAstronautData(astronauts[index]);
                if (astronautData == null)
                    continue;
                if (!astronautData.rosterResources.ContainsKey(StressResourceName))
                    continue;

                resource = astronautData.rosterResources[StressResourceName];
                resource.maxAmount = space + (stressExperienceBonus * astronauts[index].experienceTrait.CrewMemberExperienceLevel());
                astronautData.rosterResources[StressResourceName] = resource;
            }
        }

        /// <summary>
        /// Calculates how much Space a vessel has. It is a function of
        /// crew capacity and is influenced by the number of crew currently
        /// aboard.
        /// </summary>
        /// <returns>The amount of Space aboard the vessel.</returns>
        /// <param name="vessel">The Vessel to query.</param>
        public float CalculateSpace(Vessel vessel)
        {
            int crewCount = 0;
            int crewCapacity = 0;

            //Get the crew count
            if (vessel.loaded)
                crewCount = vessel.GetVesselCrew().Count;
            else
                crewCount = vessel.protoVessel.GetVesselCrew().Count;
            if (crewCount <= 0)
                return 0;

            //Get the crew capacity
            crewCapacity = SnacksScenario.Instance.GetCrewCapacity(vessel);

            return CalculateSpace(crewCount, crewCapacity);
        }

        /// <summary>
        /// Calculates how much Space a vessel has. It is a function of
        /// crew capacity and is influenced by the number of crew currently
        /// aboard.
        /// </summary>
        /// <param name="crewCount">Current crew count aboard the vessel</param>
        /// <param name="crewCapacity">Current crew capacity of the vessel</param>
        /// <returns>The amount of Space aboard the vessel.</returns>
        public float CalculateSpace(int crewCount, int crewCapacity)
        {
            //For a crewCount <= MaxSeatsForMultiplier, Space is calculated as 
            //(crewCapacity * MaxSeatsForMultiplier) / crewCount.
            //After that, it's (crewCount ^3) / crewCount.
            if (crewCapacity <= MaxSeatsForMultiplier)
                return (float)(crewCapacity * SpacePerSeatMultiplier) / (float)crewCount;
            else
                return (float)(crewCapacity * crewCapacity * crewCapacity) / (float)crewCount;

        }
        #endregion
    }
}
