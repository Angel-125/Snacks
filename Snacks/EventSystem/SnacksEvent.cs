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

namespace Snacks
{
    #region Enumerators
    /// <summary>
    /// Enumerator specifying the different types of events
    /// </summary>
    public enum SnacksEventCategories
    {
        /// <summary>
        /// Event is processed after the resource process cycle completes.
        /// </summary>
        categoryPostProcessCycle,

        /// <summary>
        /// The event is chosen at random once per process cycle.
        /// </summary>
        categoryEventCard,

        /// <summary>
        /// The event is processed when a kerbal levels up.
        /// </summary>
        categoryKerbalLevelUp
    }

    /// <summary>
    /// Enumerator specifying which kerbals are affected by the preconditions.
    /// </summary>
    public enum KerbalsAffectedTypes
    {
        /// <summary>
        /// A single available kerbal is chosen at random.
        /// </summary>
        affectsRandomAvailable,

        /// <summary>
        /// A single assigned kerbal is chosen at random.
        /// </summary>
        affectsRandomAssigned,

        /// <summary>
        /// All available kerbals are affected.
        /// </summary>
        affectsAllAvailable,

        /// <summary>
        /// All assigned kerbals are affected.
        /// </summary>
        affectsAllAssigned,

        /// <summary>
        /// A single random kerbal is chosesn amongst each crewed vessel.
        /// </summary>
        affectsRandomCrewPerVessel
    }
    #endregion

    /// <summary>
    /// This class represents an "event" in Snacks. Events consist of one or more preconditions and one or more outcomes. Preconditions are things like random numbers, the pressence of specific conditions, and the like.
    /// All preconditions must be met before the event outcomes can be applied. The outcomes include all the Snacks penalties as well as other things such as setting conditions.
    /// </summary>
    public class SnacksEvent
    {
        #region Constants
        public const string SNACKS_EVENT = "SNACKS_EVENT";
        public const string SnacksEventName = "name";
        public const string SnacksEventCategory = "eventCategory";
        public const string SnacksEventAffectedKerbals = "kerbalsAffected";
        public const string SnacksEventPlayerMessage = "playerMessage";
        public const string SnacksEventSecondsBetweenChecks = "secondsBetweenChecks";
        public const string SnacksEventDaysBetweenChecks = "daysBetweenChecks";
        public const string SnacksEventElapsedTime = "elapsedTime";
        #endregion

        #region Housekeeping
        /// <summary>
        /// The event's category
        /// </summary>
        public SnacksEventCategories eventCategory;

        /// <summary>
        /// The type of kerbals affected by the event.
        /// </summary>
        public KerbalsAffectedTypes affectedKerbals;

        /// <summary>
        /// Number of seconds that must pass before the event can be checked.
        /// </summary>
        public double secondsBetweenChecks;

        /// <summary>
        /// The number of day that must pass before the event can be checked. Overrides secondsBetweenChecks.
        /// </summary>
        public double daysBetweenChecks;

        /// <summary>
        /// Player-friendly message to display when outcomes are going to be applied.
        /// </summary>
        public string playerMessage = string.Empty;

        /// <summary>
        /// Name of the event
        /// </summary>
        public string name = string.Empty;

        protected List<BaseOutcome> outcomes;
        protected List<BasePrecondition> preconditions;
        protected double elapsedTime;
        protected SnacksProcessorResult result;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Snacks.SnacksEvent"/> class.
        /// </summary>
        public SnacksEvent()
        {
            outcomes = new List<BaseOutcome>();
            preconditions = new List<BasePrecondition>();

            result = new SnacksProcessorResult();
            result.affectedKerbalCount = 1;
            result.crewCapacity = 1;
            result.crewCount = 1;
            result.appliedPerCrew = true;
            result.afftectedAstronauts = new List<ProtoCrewMember>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Snacks.SnacksEvent"/> class.
        /// </summary>
        /// <param name="node">A ConfigNode specifying the initialization parameters.</param>
        public SnacksEvent(ConfigNode node) : base ()
        {
            if (!node.HasValue(SnacksEventName))
                return;

            result = new SnacksProcessorResult();
            result.affectedKerbalCount = 1;
            result.crewCapacity = 1;
            result.crewCount = 1;
            result.appliedPerCrew = true;
            result.afftectedAstronauts = new List<ProtoCrewMember>();

            name = node.GetValue(SnacksEventName);

            if (node.HasValue(SnacksEventCategory))
                eventCategory = (SnacksEventCategories)Enum.Parse(typeof(SnacksEventCategories), node.GetValue(SnacksEventCategory));

            if (node.HasValue(SnacksEventAffectedKerbals))
                affectedKerbals = (KerbalsAffectedTypes)Enum.Parse(typeof(KerbalsAffectedTypes), node.GetValue(SnacksEventAffectedKerbals));

            if (node.HasValue(SnacksEventPlayerMessage))
                playerMessage = node.GetValue(SnacksEventPlayerMessage);

            if (node.HasValue(SnacksEventDaysBetweenChecks))
            {
                double.TryParse(node.GetValue(SnacksEventDaysBetweenChecks), out daysBetweenChecks);
                secondsBetweenChecks = daysBetweenChecks * SnacksScenario.GetSecondsPerDay();
            }
            else if (node.HasValue(SnacksEventSecondsBetweenChecks))
            {
                double.TryParse(node.GetValue(SnacksEventSecondsBetweenChecks), out secondsBetweenChecks);
            }

            //Preconditions
            preconditions = new List<BasePrecondition>();
            if (node.HasNode(BasePrecondition.PRECONDITION))
            {
                ConfigNode[] nodes = node.GetNodes(BasePrecondition.PRECONDITION);
                BasePrecondition precondition;

                for (int index = 0; index < nodes.Length; index++)
                {
                    precondition = SnacksScenario.Instance.CreatePrecondition(nodes[index]);
                    if (precondition != null)
                        preconditions.Add(precondition);
                }
            }

            //Outcomes
            outcomes = new List<BaseOutcome>();
            if (node.HasNode(BaseOutcome.OUTCOME))
            {
                ConfigNode[] nodes = node.GetNodes(BaseOutcome.OUTCOME);
                BaseOutcome outcome;

                for (int index = 0; index < nodes.Length; index++)
                {
                    outcome = SnacksScenario.Instance.CreateOutcome(nodes[index]);
                    if (outcome != null)
                        outcomes.Add(outcome);
                }
            }
        }
        #endregion

        #region API
        /// <summary>
        /// Applies outcomes to the supplied astronaut
        /// </summary>
        /// <param name="astronaut">The ProtoCrewMember to apply outcomes to.</param>
        /// <param name="vessel">The Vessel to check</param>
        public void ApplyOutcomes(ProtoCrewMember astronaut, Vessel vessel)
        {
            result.afftectedAstronauts.Clear();
            result.afftectedAstronauts.Add(astronaut);

            //Apply all non-random outcomes and build random list in the process
            int count = outcomes.Count;
            List<BaseOutcome> randomOutcomes = new List<BaseOutcome>();
            for (int index = 0; index < count; index++)
            {
                if (!outcomes[index].canBeRandom)
                    outcomes[index].ApplyOutcome(vessel, result);
                else
                    randomOutcomes.Add(outcomes[index]);
            }

            //Now apply one of the random outcomes
            if (randomOutcomes.Count > 0)
            {
                int randomIndex = UnityEngine.Random.Range(0, randomOutcomes.Count - 1);
                randomOutcomes[randomIndex].ApplyOutcome(vessel, result);
            }
        }

        /// <summary>
        /// Applies outcomes to the supplied astronaut
        /// </summary>
        /// <param name="astronaut">The ProtoCrewMember to apply outcomes to.</param>
        public void ApplyOutcomes(ProtoCrewMember astronaut)
        {
            result.afftectedAstronauts.Clear();
            result.afftectedAstronauts.Add(astronaut);

            //Apply all non-random outcomes and build random list in the process
            int count = outcomes.Count;
            List<BaseOutcome> randomOutcomes = new List<BaseOutcome>();
            for (int index = 0; index < count; index++)
            {
                if (!outcomes[index].canBeRandom)
                    outcomes[index].ApplyOutcome(null, result);
                else
                    randomOutcomes.Add(outcomes[index]);
            }


            //Now apply one of the random outcomes
            if (randomOutcomes.Count > 0)
            {
                int randomIndex = UnityEngine.Random.Range(0, randomOutcomes.Count - 1);
                randomOutcomes[randomIndex].ApplyOutcome(null, result);
            }
        }

        /// <summary>
        /// Checks all preconditions against the supplied astronaut
        /// </summary>
        /// <param name="astronaut">The ProtoCrewMember to check</param>
        /// <param name="vessel">The Vessel to check</param>
        /// <returns></returns>
        public bool PreconditionsValid(ProtoCrewMember astronaut, Vessel vessel)
        {
            if (preconditions.Count == 0)
                return true;

            int count = preconditions.Count;
            for (int index = 0; index < count; index++)
            {
                if (!preconditions[index].IsValid(astronaut, vessel))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Checks all preconditions against the supplied astronaut
        /// </summary>
        /// <param name="astronaut">The ProtoCrewMember to check</param>
        /// <returns></returns>
        public bool PreconditionsValid(ProtoCrewMember astronaut)
        {
            if (preconditions.Count == 0)
                return true;

            int count = preconditions.Count;
            for (int index = 0; index < count; index++)
            {
                if (!preconditions[index].IsValid(astronaut))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Determines if the event can be evaluated based on the supplied elapsed time.
        /// </summary>
        /// <param name="elapsedTime">The number of seconds that have passed since the last inquiry.</param>
        /// <returns>true if it's time to evaluate the event, false if not.</returns>
        public bool IsTimeToCheck(double elapsedTime)
        {
            this.elapsedTime += elapsedTime;

            if (this.elapsedTime >= secondsBetweenChecks)
            {
                if (secondsBetweenChecks > 0)
                    this.elapsedTime -= secondsBetweenChecks;
                else
                    this.elapsedTime = 0;
                return true;
            }
            else
            {
                return false;
            }
        }

        public double GetTimeUntilNextCheck()
        {
            return secondsBetweenChecks > 0 ? Math.Max(0, secondsBetweenChecks - elapsedTime) : double.PositiveInfinity;
        }

        /// <summary>
        /// Processes the event based on elapsed time, event type, and kerbals affected.
        /// </summary>
        /// <param name="elapsedTime">The elapsed time since the last process cycle, ignored for event cards.</param>
        public void ProcessEvent(double elapsedTime)
        {
            if (!IsTimeToCheck(elapsedTime))
                return;

            do
            {
                switch (affectedKerbals)
                {
                    case KerbalsAffectedTypes.affectsAllAssigned:
                        processAllAssignedEvent();
                        break;

                    case KerbalsAffectedTypes.affectsRandomAssigned:
                        processRandomAssignedEvent();
                        break;

                    case KerbalsAffectedTypes.affectsRandomCrewPerVessel:
                        processRandomCrewPerVesselEvent();
                        break;

                    case KerbalsAffectedTypes.affectsAllAvailable:
                        processAllAvailableEvent();
                        break;

                    case KerbalsAffectedTypes.affectsRandomAvailable:
                        processRandomAvailableEvent();
                        break;
                }
                //Replay missed scheduled checks while retaining the fractional remainder.
            } while (secondsBetweenChecks > 0 && IsTimeToCheck(0));
        }

        /// <summary>
        /// Loads the persistent data.
        /// </summary>
        /// <param name="node">A ConfigNode with persistent data.</param>
        public void Load(ConfigNode node)
        {
            if (node.HasValue(SnacksEventElapsedTime))
                double.TryParse(node.GetValue(SnacksEventElapsedTime), out elapsedTime);
        }

        /// <summary>
        /// Saves the persistent data.
        /// </summary>
        /// <returns>A ConfigNode with persistent data.</returns>
        public ConfigNode Save()
        {
            ConfigNode node = new ConfigNode(SNACKS_EVENT);

            node.AddValue(SnacksEventName, name);
            node.AddValue(SnacksEventCategory, eventCategory.ToString());
            node.AddValue(SnacksEventElapsedTime, elapsedTime.ToString());

            return node;
        }
        #endregion

        #region Helpers
        protected void processAllAssignedEvent()
        {
            //Go through all vessels and check conditions and apply outcomes.
            Vessel[] vessels = FlightGlobals.Vessels.ToArray();
            Vessel vessel;
            ProtoCrewMember[] astronauts;
            ProtoCrewMember astronaut;

            for (int index = 0; index < vessels.Length; index++)
            {
                //Get the vessel
                vessel = vessels[index];

                //Get the crew
                if (vessel.loaded)
                    astronauts = vessel.GetVesselCrew().ToArray();
                else
                    astronauts = vessel.protoVessel.GetVesselCrew().ToArray();
                if (astronauts.Length == 0)
                    continue;

                //Now check preconditions and apply outcomes, skipping unowned crew
                for (int astronautIndex = 0; astronautIndex < astronauts.Length; astronautIndex++)
                {
                    astronaut = astronauts[astronautIndex];
                    if (astronaut.type == ProtoCrewMember.KerbalType.Unowned)
                        continue;

                    if (PreconditionsValid(astronaut, vessel))
                        ApplyOutcomes(astronaut, vessel);
                }
            }

            //Finally, show player message
            if (!string.IsNullOrEmpty(playerMessage))
                ScreenMessages.PostScreenMessage(playerMessage, 5.0f, ScreenMessageStyle.UPPER_LEFT);
        }

        protected void processAllAvailableEvent()
        {
            KerbalRoster roster = HighLogic.CurrentGame.CrewRoster;
            int count = roster.Count;

            for (int index = 0; index < count; index++)
            {
                if (roster[index].rosterStatus != ProtoCrewMember.RosterStatus.Available)
                    continue;

                //Make sure all preconditions are valid before applying outcomes.
                if (PreconditionsValid(roster[index]))
                    ApplyOutcomes(roster[index]);
            }

            //Finally, show player message
            if (!string.IsNullOrEmpty(playerMessage))
                ScreenMessages.PostScreenMessage(playerMessage, 5.0f, ScreenMessageStyle.UPPER_LEFT);
        }

        protected void processRandomAssignedEvent()
        {
            Vessel[] vessels = FlightGlobals.Vessels.ToArray();
            Vessel vessel;
            ProtoCrewMember[] astronauts;
            ProtoCrewMember astronaut;
            List<ProtoCrewMember> randomAssignedCandidates = new List<ProtoCrewMember>();
            List<Vessel> randomAssignedVessels = new List<Vessel>();

            for (int index = 0; index < vessels.Length; index++)
            {
                //Get the vessel
                vessel = vessels[index];

                //Get the crew
                if (vessel.loaded)
                    astronauts = vessel.GetVesselCrew().ToArray();
                else
                    astronauts = vessel.protoVessel.GetVesselCrew().ToArray();
                if (astronauts.Length == 0)
                    continue;

                //Now add astronaut and vessel to our lists
                for (int astronautIndex = 0; astronautIndex < astronauts.Length; astronautIndex++)
                {
                    astronaut = astronauts[astronautIndex];
                    if (astronaut.type == ProtoCrewMember.KerbalType.Unowned)
                        continue;

                    randomAssignedCandidates.Add(astronaut);
                    randomAssignedVessels.Add(vessel);
                }
            }
            if (randomAssignedCandidates.Count == 0)
                return;

            //Choose a random kerbal
            int randomIndex = UnityEngine.Random.Range(0, randomAssignedCandidates.Count - 1);

            //Make sure all preconditions are valid before applying outcomes.
            if (PreconditionsValid(randomAssignedCandidates[randomIndex], randomAssignedVessels[randomIndex]))
            {
                ApplyOutcomes(randomAssignedCandidates[randomIndex], randomAssignedVessels[randomIndex]);

                //Finally, show player message
                if (!string.IsNullOrEmpty(playerMessage))
                {
                    string message = playerMessage;
                    message = message.Replace("<<KerbalName>>", randomAssignedCandidates[randomIndex].name);
                    ScreenMessages.PostScreenMessage(message, 5.0f, ScreenMessageStyle.UPPER_LEFT);
                }
            }
        }

        protected void processRandomAvailableEvent()
        {
            KerbalRoster roster = HighLogic.CurrentGame.CrewRoster;
            int count = roster.Count;
            List<ProtoCrewMember> availableCrews = new List<ProtoCrewMember>();

            for (int index = 0; index < count; index++)
            {
                if (roster[index].rosterStatus == ProtoCrewMember.RosterStatus.Available)
                    availableCrews.Add(roster[index]);
            }
            if (availableCrews.Count == 0)
                return;

            //Now select one at random
            int randomIndex = UnityEngine.Random.Range(0, availableCrews.Count - 1);

            //Make sure all preconditions are valid before applying outcomes.
            if (PreconditionsValid(roster[randomIndex]))
            {
                ApplyOutcomes(roster[randomIndex]);

                //Finally, show player message
                if (!string.IsNullOrEmpty(playerMessage))
                {
                    string message = playerMessage;
                    message = message.Replace("<<KerbalName>>", roster[randomIndex].name);
                    ScreenMessages.PostScreenMessage(message, 5.0f, ScreenMessageStyle.UPPER_LEFT);
                }
            }
        }

        protected void processRandomCrewPerVesselEvent()
        {
            //Go through all vessels and check conditions and apply outcomes.
            Vessel[] vessels = FlightGlobals.Vessels.ToArray();
            Vessel vessel;
            ProtoCrewMember[] astronauts;
            ProtoCrewMember astronaut;
            List<ProtoCrewMember> crewList = new List<ProtoCrewMember>();

            for (int index = 0; index < vessels.Length; index++)
            {
                //Get the vessel
                vessel = vessels[index];

                //Get the crew
                if (vessel.loaded)
                    astronauts = vessel.GetVesselCrew().ToArray();
                else
                    astronauts = vessel.protoVessel.GetVesselCrew().ToArray();
                if (astronauts.Length == 0)
                    continue;

                //Get all assigned astronauts, skipping any unowned ones.
                crewList.Clear();
                for (int astronautIndex = 0; astronautIndex < astronauts.Length; astronautIndex++)
                {
                    astronaut = astronauts[astronautIndex];
                    if (astronaut.type == ProtoCrewMember.KerbalType.Unowned)
                        continue;

                    crewList.Add(astronaut);
                }
                if (crewList.Count == 0)
                    continue;

                //Now select one at random
                int randomIndex = UnityEngine.Random.Range(0, astronauts.Length - 1);

                if (PreconditionsValid(crewList[randomIndex], vessel))
                {
                    ApplyOutcomes(crewList[randomIndex], vessel);

                    //Finally, show player message
                    if (!string.IsNullOrEmpty(playerMessage))
                    {
                        string message = playerMessage;
                        message = message.Replace("<<KerbalName>>", crewList[randomIndex].name);
                        ScreenMessages.PostScreenMessage(message, 5.0f, ScreenMessageStyle.UPPER_LEFT);
                    }
                }
            }
        }
        #endregion
    }
}
