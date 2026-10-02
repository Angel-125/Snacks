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
using KSP.IO;

namespace Snacks
{
    /// <summary>
    /// This outcome sets a condition on the affected kerbals. If that condition is defined in a
    /// SKILL_LOSS_CONDITION config node, then the kerbals' skills will be removed until the 
    /// condition is cleared.
    /// Example definition:
    /// OUTCOME 
    /// {
    ///     name  = ClearCondition
    ///     conditionSummary = Stressed Out
    /// }
    /// </summary>   

    public class OnStrikePenalty: BaseOutcome
    {
        #region Housekeeping
        /// <summary>
        /// The name of the condition to set. If defined in a SKILL_LOSS_CONDITION node then the affected kerbals
        /// will lose their skills until the condition is cleared.
        /// </summary>
        public string conditionName = "On Strike";
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Snacks.OnStrikePenalty"/> class.
        /// </summary>
        /// <param name="conditionName">The name of the condition to set. It must be added to a SKILL_LOSS_CONDITION
        /// config node in order for the kerbal to lose its skills.</param>
        /// <param name="canBeRandom">If set to <c>true</c> it can be randomly selected from the outcomes list.</param>
        /// <param name="playerMessage">A string containing the bad news.</param>
        public OnStrikePenalty(string conditionName, bool canBeRandom, string playerMessage) : base(canBeRandom, playerMessage)
        {
            this.conditionName = conditionName;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Snacks.OnStrikePenalty"/> class.
        /// </summary>
        /// <param name="node">A ConfigNode containing initialization parameters. Parameters in the
        /// <see cref="T:Snacks.BaseOutcome"/> class also apply.</param>
        public OnStrikePenalty(ConfigNode node) : base(node)
        {
            if (node.HasValue(ConditionName))
                conditionName = node.GetValue(ConditionName);
        }

        #endregion

        #region API
        public override void ApplyOutcome(Vessel vessel, SnacksProcessorResult result)
        {
            ProtoCrewMember[] astronauts = null;
            AstronautData astronautData = null;
            string message = string.Empty;

            //Get the crew manifest
            if (result.afftectedAstronauts != null)
                astronauts = result.afftectedAstronauts.ToArray();
            else if (vessel.loaded)
                astronauts = vessel.GetVesselCrew().ToArray();
            else
                astronauts = vessel.protoVessel.GetVesselCrew().ToArray();

            astronauts = astronauts.Where(crew => !SnacksScenario.Instance.GetAstronautData(crew).isExempt).ToArray();
            if (astronauts.Length == 0)
                return;

            //Select random crew if needed
            if (selectRandomCrew)
            {
                int randomIndex = UnityEngine.Random.Range(0, astronauts.Length);
                astronauts = new ProtoCrewMember[] { astronauts[randomIndex] };
            }

            //Go through each kerbal and set their condition
            for (int index = 0; index < astronauts.Length; index++)
            {
                astronautData = SnacksScenario.Instance.GetAstronautData(astronauts[index]);

                astronautData.SetCondition(conditionName);

                //If the vessel is loaded then remove skills
                if (vessel.loaded)
                    SnacksScenario.Instance.RemoveSkillsIfNeeded(astronauts[index]);

                //Inform player
                if (!string.IsNullOrEmpty(playerMessage))
                {
                    message = vessel.vesselName + ": " + astronauts[index].name + " " + playerMessage;
                    ScreenMessages.PostScreenMessage(message, 5.0f, ScreenMessageStyle.UPPER_LEFT);
                }
            }

            //Call the base class
            base.ApplyOutcome(vessel, result);
        }

        public override void RemoveOutcome(Vessel vessel, bool informPlayer)
        {
            ProtoCrewMember[] astronauts = null;
            AstronautData astronautData = null;

            //Get the crew manifest
            if (vessel.loaded)
                astronauts = vessel.GetVesselCrew().ToArray();
            else
                astronauts = vessel.protoVessel.GetVesselCrew().ToArray();

            //Go through each kerbal and set their condition
            for (int index = 0; index < astronauts.Length; index++)
            {
                astronautData = SnacksScenario.Instance.GetAstronautData(astronauts[index]);

                astronautData.ClearCondition(conditionName);

                //If the vessel is loaded then restore skills
                if (vessel.loaded)
                    SnacksScenario.Instance.RestoreSkillsIfNeeded(astronauts[index]);
            }

            //Call base class
            base.RemoveOutcome(vessel, informPlayer);
        }

        public override bool IsEnabled()
        {
            return true;
        }
        #endregion
    }
}
