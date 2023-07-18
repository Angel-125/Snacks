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

namespace Snacks
{
    /// <summary>
    /// Represents a snapshot of the current and max units of a particular resource that is displayed in the Snapshots window.
    /// </summary>
    public class Snackshot
    {
        /// <summary>
        /// Name of the resource
        /// </summary>
        public string resourceName;

        /// <summary>
        /// Current amount in the vessel
        /// </summary>
        public double amount;

        /// <summary>
        /// Max amount in the vessel
        /// </summary>
        public double maxAmount;

        /// <summary>
        /// Flag to indicate whether to include the time remaining estimate in the display.
        /// </summary>
        public bool showTimeRemaining;

        /// <summary>
        /// Flag to indicate whether or not simulator is running.
        /// </summary>
        public bool isSimulatorRunning = true;

        /// <summary>
        /// Estimated time remaining in seconds.
        /// </summary>
        public double estimatedTimeRemaining;

        /// <summary>
        /// Flag to indicate whether or not the simulator was interrupted.
        /// </summary>
        public bool simulatorInterrupted = false;

        public virtual string GetStatusDisplay()
        {
            StringBuilder status = new StringBuilder();
            string colorTag = string.Empty;
            string endTag = string.Empty;
            double percentRemaining = amount / maxAmount;
            PartResourceDefinitionList definitions = PartResourceLibrary.Instance.resourceDefinitions;

            //Determine color tag
            if (estimatedTimeRemaining < 0 || percentRemaining > 0.5)
            {
                colorTag = string.Empty;
                endTag = string.Empty;
            }
            else if (percentRemaining > 0.25)
            {
                colorTag = "<color=yellow>";
                endTag = "</color>";
            }
            else
            {
                colorTag = "<color=red><b>";
                endTag = "</b></color>";
            }

            //Resource and amount / maxAmount
            status.Append(colorTag + definitions[resourceName].displayName + ": ");
            status.AppendFormat("{0:f2}/{1:f2}", amount, maxAmount);
            status.AppendLine(endTag);

            //Duration
            if (simulatorInterrupted)
            {
                status.AppendLine(Localizer.Format("#LOC_DURATION1"));//#LOC_DURATION1 = <color=white>Duration: Unavailable</color>
            }
            else if (estimatedTimeRemaining < 0)
            {
                status.AppendLine(Localizer.Format("#LOC_DURATION2"));//#LOC_DURATION2 = <color=white>Duration: Indefinite</color>
            }
            else if (isSimulatorRunning)
            {
                status.AppendLine(Localizer.Format("#LOC_DURATION3"));//#LOC_DURATION3 = <color=white>Duration: Calculating...</color>
            }
            else
            {
                string timeString = SnacksScenario.FormatTime(estimatedTimeRemaining);
                if (amount < 0.0001)
                    timeString = "0 Seconds";
                status.AppendLine(colorTag + Localizer.Format("#LOC_DURATION4") + timeString + endTag);//#LOC_DURATION4 = Duration:
            }

            return status.ToString();
        }
    }
}
