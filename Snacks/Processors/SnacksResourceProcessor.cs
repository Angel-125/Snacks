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
    /// The SnacksResourceProcessor is a specialized version of the BaseResourceProcessor. It has the distict advantage of making use of the game settings for Snacks, whereas BaseResourceProcessor
    /// is entirely configured via config files.
    /// </summary>
    public class SnacksResourceProcessor: BaseResourceProcessor
    {
        public const string SnacksProcessorName = "Snacks!";

        #region Constructors
        public SnacksResourceProcessor()
        {
            name = SnacksProcessorName;

            //Get input list
            inputList = new List<ProcessedResource>();
            ProcessedResource resource = new ProcessedResource(SnacksProperties.SnacksResourceName, null, SnacksProperties.SnacksPerMeal);
            inputList.Add(resource);

            //Get output list
            outputList = new List<ProcessedResource>();

            //Setup the amounts and such based on current game settings
            OnGameSettingsApplied();
        }
        #endregion

        #region API
        public override void OnGameSettingsApplied()
        {
            //Update seconds per cycle
            secondsPerCycle = SnacksScenario.GetSecondsPerDay() / SnacksProperties.MealsPerDay;

            //Update input amounts
            inputList[0].amount = SnacksProperties.SnacksPerMeal;

            //Update output amounts
            outputList.Clear();
            if (SnacksProperties.RecyclersEnabled)
            {
                ProcessedResource resource = new ProcessedResource(SnacksProperties.SoilResourceName, SnacksProperties.SnacksResourceName, SnacksProperties.SnacksPerMeal, false, false, false);
                outputList.Add(resource);
            }

            //Outcomes
            outcomes.Clear();
            outcomes.Add(new FundingPenalty(true, SnacksText.Get("#LOC_Snacks_INFO_FUND", "Kerbals are hungry for snacks! You have been fined {0:N2} Funds"), SnacksProperties.FinePerKerbal));
            outcomes.Add(new RepPenalty(true, SnacksProperties.RepLostWhenHungry, SnacksText.Get("#LOC_Snacks_INFO_BADREP", "Kerbals are hungry for snacks! Your reputation has decreased by {0:N3}")));
            outcomes.Add(new SciencePenalty(true));
            if (SnacksProperties.CanStarveToDeath)
                outcomes.Add(new DeathPenalty(SnacksProperties.SnacksResourceName, SnacksProperties.MealsSkippedBeforeDeath, SnacksText.Get("#LOC_Snacks_INFO_DEATH", "has died from a lack of Snacks!")));
            if (SnacksProperties.FaintWhenHungry)
                outcomes.Add(new FaintPenalty(SnacksProperties.SnacksResourceName, SnacksProperties.MealsBeforeFainting, SnacksProperties.NapDurationSeconds, SnacksText.Get("#LOC_Snacks_INFO_FAINTE", "has fainted from a lack of Snacks!")));

            if (SnacksScenario.Instance.rosterResources.ContainsKey(StressProcessor.StressResourceName))
                outcomes.Add(new ProduceResource(StressProcessor.StressResourceName, 1.0, false, string.Empty));
        }
        #endregion

        #region Events
        #endregion

        #region Overrides
        public override void ProcessResources(Vessel vessel, double elapsedTime, int crewCount, int crewCapacity)
        {
            base.ProcessResources(vessel, elapsedTime, crewCount, crewCapacity);
        }

        protected override SnacksProcessorResult consumeInputResource(ProcessedResource resource, Vessel vessel,
            double elapsedTime, ProtoCrewMember[] astronauts, int crewCapacity)
        {
            if (resource.resourceName != SnacksProperties.SnacksResourceName)
                return base.consumeInputResource(resource, vessel, elapsedTime, astronauts, crewCapacity);
            return resource.ConsumeMeals(vessel, astronauts.Where(crew => crew.type != ProtoCrewMember.KerbalType.Unowned).ToArray(), crewCapacity);
        }

        protected override SnacksProcessorResult produceOutputResource(ProcessedResource resource, Vessel vessel,
            double elapsedTime, int crewCount, int crewCapacity)
        {
            SnacksProcessorResult meals;
            if (resource.resourceName == SnacksProperties.SoilResourceName)
            {
                double eaten = consumptionResults.TryGetValue(SnacksProperties.SnacksResourceName, out meals) ? meals.consumedAmount : 0;
                return resource.ProduceAmount(vessel, eaten, crewCount, crewCapacity);
            }
            return base.produceOutputResource(resource, vessel, elapsedTime, crewCount, crewCapacity);
        }

        public override void AddConsumedAndProducedResources(Vessel vessel, double secondsPerCycle, List<ResourceRatio> consumedResources, List<ResourceRatio> producedResources)
        {
            IEnumerable<ProtoCrewMember> crew = vessel.loaded ? vessel.GetVesselCrew() : vessel.protoVessel.GetVesselCrew();
            AddConsumedAndProducedResources(crew, secondsPerCycle, consumedResources, producedResources);
        }

        public void AddConsumedAndProducedResources(IEnumerable<ProtoCrewMember> crew, double secondsPerCycle,
            List<ResourceRatio> consumedResources, List<ResourceRatio> producedResources)
        {
            double portions = GetMealPortions(crew);
            addMealRates(portions, secondsPerCycle, consumedResources, producedResources);
        }

        public static double GetMealPortions(IEnumerable<ProtoCrewMember> crew)
        {
            double portions = 0;
            foreach (ProtoCrewMember astronaut in crew)
            {
                if (astronaut.type == ProtoCrewMember.KerbalType.Unowned)
                    continue;
                AstronautData data = SnacksScenario.Instance.GetAstronautData(astronaut);
                if (!data.isExempt)
                    portions += data.SnacksMultiplier;
            }
            return portions;
        }

        public override void AddConsumedAndProducedResources(int crewCount, double secondsPerCycle, List<ResourceRatio> consumedResources, List<ResourceRatio> producedResources)
        {
            // Callers supplying only a count have no individual crew identities.
            addMealRates(crewCount, secondsPerCycle, consumedResources, producedResources);
        }

        private void addMealRates(double portions, double secondsPerCycle, List<ResourceRatio> consumedResources, List<ResourceRatio> producedResources)
        {
            if (portions <= 0)
                return;

            ResourceRatio resourceRatio;

            //Calculate amount
            //Start with total snacks consumed per day, accounting for crew count.
            double amount = inputList[0].amount * portions * secondsPerCycle / this.secondsPerCycle;

            //Add snacks
            resourceRatio = new ResourceRatio();
            resourceRatio.ResourceName = SnacksProperties.SnacksResourceName;
            resourceRatio.Ratio = amount;
            consumedResources.Add(resourceRatio);

            //Add soil
            if (outputList.Any(resource => resource.resourceName == SnacksProperties.SoilResourceName))
            {
                resourceRatio = new ResourceRatio();
                resourceRatio.ResourceName = SnacksProperties.SoilResourceName;
                resourceRatio.Ratio = amount;
                producedResources.Add(resourceRatio);
            }
        }
        #endregion

        #region Helpers
        #endregion
    }
}
