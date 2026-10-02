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

namespace Snacks
{
    #region Enums
    /// <summary>
    /// Enumerator with the type of results to check.
    /// </summary>
    public enum CheckResultTypes
    {
        /// <summary>
        /// Check for a successful consumption
        /// </summary>
        resultConsumptionSuccess,

        /// <summary>
        /// Check for a consumption failure
        /// </summary>
        resultConsumptionFailure,

        /// <summary>
        /// Check for a production success
        /// </summary>
        resultProductionSuccess,

        /// <summary>
        /// Check for a production failure
        /// </summary>
        resultProductionFailure
    }
    #endregion

    /// <summary>
    /// This precondition checks the specified processor for desired results.
    /// Example definition:
    /// PRECONDITION 
    /// {
    ///     name  = CheckProcessorResult
    ///     type = resultConsumptionFailure
    ///     processorName = Snacks!
    ///     resourceName = Snacks
    ///     cyclesRequired = 1
    /// }
    /// </summary> 
    public class CheckProcessorResult: BasePrecondition
    {
        #region Constants
        public const string ResultType = "type";
        public const string ResultProcessorName = "processorName";
        public const string ResultCyclesRequired = "cyclesRequired";
        #endregion

        #region Housekeeping
        /// <summary>
        /// The type of result to check
        /// </summary>
        public CheckResultTypes resultType;

        /// <summary>
        /// The name of the processor to check
        /// </summary>
        public string processorName = string.Empty;

        /// <summary>
        /// The name of the resource to check
        /// </summary>
        public string resourceName = string.Empty;

        /// <summary>
        /// The number of process cycles to check
        /// </summary>
        public int cyclesRequired;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Snacks.CheckProcessorResult"/> class.
        /// </summary>
        /// <param name="node">A ConfigNode containing initialization parameters. parameters from the 
        /// <see cref="T:Snacks.BasePrecondition"/> class also apply.</param>
        public CheckProcessorResult(ConfigNode node): base(node)
        {
            if (node.HasValue(ResultType))
                resultType = (CheckResultTypes)Enum.Parse(typeof(CheckResultTypes), node.GetValue(ResultType));

            if (node.HasValue(ResultProcessorName))
                processorName = node.GetValue(ResultProcessorName);

            if (node.HasValue(ValueResourceName))
                resourceName = node.GetValue(ValueResourceName);

            if (node.HasValue(ResultCyclesRequired))
                int.TryParse(node.GetValue(ResultCyclesRequired), out cyclesRequired);
        }
        #endregion

        #region Overrides
        public override bool IsValid(ProtoCrewMember astronaut, Vessel vessel)
        {
            return IsValid(astronaut);
        }

        public override bool IsValid(ProtoCrewMember astronaut)
        {
            if (!base.IsValid(astronaut))
                return false;

            AstronautData astronautData = SnacksScenario.Instance.GetAstronautData(astronaut);
            if (astronautData == null)
                return false;

            //Get the processor
            BaseResourceProcessor processor = null;
            int count = SnacksScenario.Instance.resourceProcessors.Count;
            for (int index = 0; index < count; index++)
            {
                processor = SnacksScenario.Instance.resourceProcessors[index];
                if (!string.IsNullOrEmpty(processorName) && processor.name != processorName)
                {
                    processor = null;
                    continue;
                }
                if (resultType == CheckResultTypes.resultConsumptionFailure || resultType == CheckResultTypes.resultConsumptionSuccess)
                {
                    if (processor.consumptionResults.ContainsKey(resourceName))
                        break;
                }
                else
                {
                    if (processor.productionResults.ContainsKey(resourceName))
                        break;
                }
                processor = null;
            }
            if (processor == null)
                return false;

            switch (resultType)
            {
                case CheckResultTypes.resultConsumptionFailure:
                    //Processor needs at least one failure
                    if (processor.consumptionResults[resourceName].completedSuccessfully)
                        return false;

                    if (astronautData.processedResourceFailures.ContainsKey(resourceName))
                        return astronautData.processedResourceFailures[resourceName] >= cyclesRequired;
                    else
                        return true;

                case CheckResultTypes.resultConsumptionSuccess:
                    //Processor needs at least one success
                    if (!processor.consumptionResults[resourceName].completedSuccessfully)
                        return false;

                    if (astronautData.processedResourceSuccesses.ContainsKey(resourceName))
                        return astronautData.processedResourceSuccesses[resourceName] >= cyclesRequired;
                    else
                        return true;

                case CheckResultTypes.resultProductionFailure:
                    //Processor needs at least one failure
                    if (processor.productionResults[resourceName].completedSuccessfully)
                        return false;

                    if (astronautData.processedResourceFailures.ContainsKey(resourceName))
                        return astronautData.processedResourceFailures[resourceName] >= cyclesRequired;
                    else
                        return true;

                case CheckResultTypes.resultProductionSuccess:
                    //Processor needs at least one success
                    if (!processor.productionResults[resourceName].completedSuccessfully)
                        return false;

                    if (astronautData.processedResourceSuccesses.ContainsKey(resourceName))
                        return astronautData.processedResourceSuccesses[resourceName] >= cyclesRequired;
                    else
                        return true;
            }

            return true;
        }
        #endregion

        #region Helpers
        #endregion
    }
}
