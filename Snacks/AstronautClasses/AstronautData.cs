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
using System.Globalization;
using UnityEngine;

namespace Snacks
{
    /// <summary>
    /// This class contains data related to a kerbal. Information includes
    /// roster resources (characteristics of the kerbal akin to Courage and Stupidity),
    /// a condition summary specifying what states the kerbal is in,
    /// a list of disqualified conditions that will auto-fail precondition checks,
    /// a list of processor successes and failures,
    /// a key-value map suitable for tracking states in the event system,
    /// an exempt flag that exempts the kerbal from all outcomes.
    /// </summary>
    public class AstronautData
    {
        #region Constants
        const string AstronautNode = "ASTRONAUT";
        const string AstronautNodeName = "name";
        const string AstronautNodeTrait = "experienceTrait";
        const string AstronautNodeUpdated = "lastUpdated";
        const string AstronautNodeExempt = "isExempt";
        const string AstronautNodeSnacksMultiplier = "snacksMultiplier";
        const string AstronautNodeCondition = "conditionSummary";
        const string AstronautNodeDisqualifiers = "disqualifiedPreconditions";

        const string ResourceCounterNode = "RESOURCE_COUNT";
        const string ResourceCounterIsSuccess = "isSuccess";
        const string ResourceCounterName = "resourceName";
        const string ResourceCounterValue = "count";
        const string ResourceNode = "RESOURCE";
        const string ResourceNodeName = "name";
        const string ResourceNodeAmount = "amount";
        const string ResourceNodeMaxAmount = "maxAmount";

        const string KeyValueNode = "KEYVALUE";
        const string KeyValuePairKey = "key";
        const string KeyValuePairValue = "value";

        public const string EVAStartTime = "EVAStartTime";
        #endregion

        #region Housekeeping
        /// <summary>
        /// Name of the kerbal.
        /// </summary>
        public string name;

        /// <summary>
        /// The kerba's current experience trait.
        /// </summary>
        public string experienceTrait;

        /// <summary>
        /// Timestamp of when the astronaut data was last update.
        /// </summary>
        public double lastUpdated;

        /// <summary>
        /// Flag to indicate that the kerbal is exempt from outcomes.
        /// </summary>
        public bool isExempt;

        private double snacksMultiplier = 1;
        /// <summary>Portion size relative to the game's Snacks per meal. Exemptions take precedence.</summary>
        public double SnacksMultiplier
        {
            get { return snacksMultiplier; }
            set { snacksMultiplier = !double.IsNaN(value) && !double.IsInfinity(value) && value >= 1 ? value : 1; }
        }

        /// <summary>
        /// Summary of all the conditions that the kerbal currently has. If a
        /// condition in the summary is defined in a SKILL_LOSS_CONDITION config node,
        /// then the kerbal will lose its skills until the condition is cleared.
        /// </summary>
        public string conditionSummary = string.Empty;

        /// <summary>
        /// A map of key-value pairs.
        /// </summary>
        public DictionaryValueList<string, string> keyValuePairs;

        /// <summary>
        /// Map of successful process cycles. The key is the name of the processor,
        /// the value is the number of successes.
        /// </summary>
        public Dictionary<string, int> processedResourceSuccesses;

        /// <summary>
        /// Map of unsuccessfull process cycles. The key is the name of the processor,
        /// the value is the number of failures.
        /// </summary>
        public Dictionary<string, int> processedResourceFailures;

        /// <summary>
        /// A map of roster resources (characteristics of the kerbal), similar to
        /// vessel resources.
        /// </summary>
        public Dictionary<string, SnacksRosterResource> rosterResources;

        /// <summary>
        /// Conditions that will automatically disqualify a precondition check.
        /// </summary>
        public string disqualifiedPreconditions = string.Empty;

        /// <summary>
        /// List of resources that the kerbal uses.
        /// </summary>
        public Dictionary<string, ConfigNode> resources;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Snacks.AstronautData"/> class.
        /// </summary>
        public AstronautData()
        {
            keyValuePairs = new DictionaryValueList<string, string>();
            processedResourceFailures = new Dictionary<string, int>();
            processedResourceSuccesses = new Dictionary<string, int>();
            rosterResources = new Dictionary<string, SnacksRosterResource>();
            resources = new Dictionary<string, ConfigNode>();
        }
        #endregion

        #region Load & Save
        protected static void Log(string message)
        {
            if (SnacksScenario.LoggingEnabled)
            {
                Debug.Log("[AstronautData] - " + message);
            }
        }

        /// <summary>
        /// Loads the astronaut data from the config node supplied.
        /// </summary>
        /// <returns>A map keyed kerbal name that contains astronaut data.</returns>
        /// <param name="node">The ConfigNode to read data from.</param>
        public static DictionaryValueList<string, AstronautData> Load(ConfigNode node)
        {
            DictionaryValueList<string, AstronautData> crewData = new DictionaryValueList<string, AstronautData>();
            ConfigNode[] astronautNodess = node.GetNodes(AstronautNode);

            foreach (ConfigNode astronautNode in astronautNodess)
            {
                try
                {
                    AstronautData astronautData = new AstronautData();

                    astronautData.name = astronautNode.GetValue(AstronautNodeName);
                    astronautData.experienceTrait = astronautNode.GetValue(AstronautNodeTrait);
                    astronautData.lastUpdated = double.Parse(astronautNode.GetValue(AstronautNodeUpdated));
                    astronautData.isExempt = bool.Parse(astronautNode.GetValue(AstronautNodeExempt));
                    double multiplier;
                    if (double.TryParse(astronautNode.GetValue(AstronautNodeSnacksMultiplier), NumberStyles.Float,
                        CultureInfo.InvariantCulture, out multiplier))
                        astronautData.SnacksMultiplier = multiplier;

                    if (astronautNode.HasValue(AstronautNodeCondition))
                        astronautData.conditionSummary = astronautNode.GetValue(AstronautNodeCondition);

                    if (astronautNode.HasValue(AstronautNodeDisqualifiers))
                        astronautData.disqualifiedPreconditions = astronautNode.GetValue(AstronautNodeDisqualifiers);

                    //Key value pairs
                    astronautData.keyValuePairs = new DictionaryValueList<string, string>();
                    if (astronautNode.HasNode(KeyValueNode))
                    {
                        ConfigNode[] keyValuePairs = astronautNode.GetNodes(KeyValueNode);
                        foreach (ConfigNode keyValue in keyValuePairs)
                            astronautData.keyValuePairs.Add(keyValue.GetValue(KeyValuePairKey), keyValue.GetValue(KeyValuePairValue));
                    }

                    //Success/fail counters
                    if (astronautNode.HasNode(ResourceCounterNode))
                    {
                        ConfigNode[] resourceCounterNodes = astronautNode.GetNodes(ResourceCounterNode);
                        ConfigNode counterNode;
                        string resourceName = string.Empty;
                        int count = 0;
                        bool isSuccess = false;
                        for (int index = 0; index < resourceCounterNodes.Length; index++)
                        {
                            counterNode = resourceCounterNodes[index];
                            resourceName = counterNode.GetValue(ResourceCounterName);
                            int.TryParse(counterNode.GetValue(ResourceCounterValue), out count);
                            isSuccess = false;
                            bool.TryParse(counterNode.GetValue(ResourceCounterIsSuccess), out isSuccess);

                            if (isSuccess)
                                astronautData.processedResourceSuccesses.Add(resourceName, count);
                            else
                                astronautData.processedResourceFailures.Add(resourceName, count);
                        }
                    }

                    //Roster resources
                    if (astronautNode.HasNode(SnacksRosterResource.RosterResourceNode))
                        astronautData.rosterResources = SnacksRosterResource.LoadFromAstronautData(astronautNode);

                    //Other resources
                    if (astronautNode.HasNode(ResourceNodeName))
                    {
                        ConfigNode[] resourceNodes = astronautNode.GetNodes(ResourceNodeName);
                        for (int index = 0; index < resourceNodes.Length; index++)
                            astronautData.resources.Add(resourceNodes[index].GetValue(ResourceNodeName), resourceNodes[index]);
                    }

                    crewData.Add(astronautData.name, astronautData);
                }
                catch (Exception ex)
                {
                    Log("error encountered: " + ex + " skipping kerbal.");
                    continue;
                }
            }

            return crewData;
        }

        /// <summary>
        /// Saves persistent astronaut data to the supplied config node.
        /// </summary>
        /// <param name="crewData">A map of astronaut data, keyed by kerbal name.</param>
        /// <param name="node">The ConfigNode to save the data to.</param>
        public static void Save(DictionaryValueList<string, AstronautData> crewData, ConfigNode node)
        {
            List<AstronautData>.Enumerator dataValues = crewData.GetListEnumerator();
            AstronautData astronautData;
            ConfigNode configNode;
            ConfigNode astronautNode;
            string[] keys;

            while (dataValues.MoveNext())
            {
                astronautData = dataValues.Current;
                astronautNode = new ConfigNode(AstronautNode);
                astronautNode.AddValue(AstronautNodeName, astronautData.name);
                astronautNode.AddValue(AstronautNodeTrait, astronautData.experienceTrait);
                astronautNode.AddValue(AstronautNodeUpdated, astronautData.lastUpdated);
                astronautNode.AddValue(AstronautNodeExempt, astronautData.isExempt);
                astronautNode.AddValue(AstronautNodeSnacksMultiplier, astronautData.SnacksMultiplier.ToString("R", CultureInfo.InvariantCulture));

                if (!string.IsNullOrEmpty(astronautData.conditionSummary))
                    astronautNode.AddValue(AstronautNodeCondition, astronautData.conditionSummary);

                if (!string.IsNullOrEmpty(astronautData.disqualifiedPreconditions))
                    astronautNode.AddValue(AstronautNodeDisqualifiers, astronautData.disqualifiedPreconditions);

                //Save keyvalue pairs
                keys = astronautData.keyValuePairs.Keys.ToArray();
                for (int index = 0; index < keys.Length; index++)
                {
                    configNode = new ConfigNode(KeyValueNode);
                    configNode.AddValue(KeyValuePairKey, keys[index]);
                    configNode.AddValue(KeyValuePairValue, astronautData.keyValuePairs[keys[index]]);
                    astronautNode.AddNode(configNode);
                }

                //Save resource process results
                keys = astronautData.processedResourceSuccesses.Keys.ToArray();
                for (int index = 0; index < keys.Length; index++)
                {
                    configNode = new ConfigNode(ResourceCounterNode);
                    configNode.AddValue(ResourceCounterName, keys[index]);
                    configNode.AddValue(ResourceCounterIsSuccess, true);
                    configNode.AddValue(ResourceCounterValue, astronautData.processedResourceSuccesses[keys[index]]);
                    astronautNode.AddNode(configNode);
                }

                keys = astronautData.processedResourceFailures.Keys.ToArray();
                for (int index = 0; index < keys.Length; index++)
                {
                    configNode = new ConfigNode(ResourceCounterNode);
                    configNode.AddValue(ResourceCounterName, keys[index]);
                    configNode.AddValue(ResourceCounterIsSuccess, false);
                    configNode.AddValue(ResourceCounterValue, astronautData.processedResourceFailures[keys[index]]);
                    astronautNode.AddNode(configNode);
                }

                //Save roster resources
                SnacksRosterResource.SaveToAstronautData(astronautData.rosterResources, astronautNode);

                //Save other resources
                keys = astronautData.resources.Keys.ToArray();
                for (int index = 0; index < keys.Length; index++)
                {
                    astronautNode.AddNode(astronautData.resources[keys[index]]);
                }

                node.AddNode(astronautNode);
            }
        }
        #endregion

        #region API
        /// <summary>
        /// Sets a disqualifier that will automatically fail a precondition check.
        /// </summary>
        /// <param name="disqualifier">The name of the disqualifier to set.</param>
        public void SetDisqualifier(string disqualifier)
        {
            if (string.IsNullOrEmpty(disqualifiedPreconditions))
                disqualifiedPreconditions = disqualifier;

            else if (!conditionSummary.Contains(disqualifier))
                disqualifiedPreconditions += ", " + disqualifier;

            SnacksScenario.Instance.SetAstronautData(this);
        }

        /// <summary>
        /// Clears a disqualifier that will no longer fail a precondition check.
        /// </summary>
        /// <param name="disqualifier">The name of the disqualifier to clear.</param>
        public void ClearDisqualifier(string disqualifier)
        {
            if (disqualifiedPreconditions == disqualifier)
                disqualifiedPreconditions = string.Empty;

            else if (conditionSummary.Contains(disqualifier))
            {
                disqualifiedPreconditions = disqualifiedPreconditions.Replace(", " + disqualifier, "");
                disqualifiedPreconditions = disqualifiedPreconditions.Replace(disqualifier + ", ", "");
            }

            SnacksScenario.Instance.SetAstronautData(this);
        }

        /// <summary>
        /// Sets a condition that could result in loss of skills if defined in a SKILL_LOSS_CONDITION config node.
        /// The condition will appear in the kerbal's condition summary in the status window.
        /// </summary>
        /// <param name="condition">The name of the condition to set.</param>
        public void SetCondition(string condition)
        {
            if (string.IsNullOrEmpty(conditionSummary))
                conditionSummary = condition;

            else if (!conditionSummary.Contains(condition))
                conditionSummary += ", " + condition;

            SnacksScenario.Instance.SetAstronautData(this);
        }

        /// <summary>
        /// Clears a condition, removing it from the condition summary display. If the condition is defined in
        /// a SKILL_LOSS_CONDITION config node, and the kerbal has no other conditions that result from skill loss,
        /// then the kerbal will regain its skills.
        /// </summary>
        /// <param name="condition">The name of the condition to clear.</param>
        public void ClearCondition(string condition)
        {
            if (conditionSummary == condition)
                conditionSummary = string.Empty;

            else if (conditionSummary.Contains(condition))
            {
                conditionSummary = conditionSummary.Replace(", " + condition, "");
                conditionSummary = conditionSummary.Replace(condition + ", ", "");
            }
            SnacksScenario.Instance.SetAstronautData(this);
        }

        /// <summary>
        /// Sets the key/value pair.
        /// </summary>
        /// <param name="key">The key to use.</param>
        /// <param name="value">The value of the key.</param>
        public void SetKeyValue(string key, string value)
        {
            if (keyValuePairs.ContainsKey(key))
                keyValuePairs[key] = value;
            else
                keyValuePairs.Add(key, value);
        }

        /// <summary>
        /// Returns the value for the desired key.
        /// </summary>
        /// <param name="key">A string containing the desired key.</param>
        /// <returns>A string with the value, or null if it doesn't exist.</returns>
        public string GetValueForKey(string key)
        {
            return keyValuePairs.Contains(key) ? keyValuePairs[key] : null;
        }

        /// <summary>
        /// Returns the value for the desired key.
        /// </summary>
        /// <param name="key">A string containing the desired key.</param>
        /// <returns>A double with the value, or NaN if it doesn't exist.</returns>
        public double GetDoubleValueForKey(string key)
        {
            string value = GetValueForKey(key);
            if (string.IsNullOrEmpty(value))
                return double.NaN;

            double doubleValue;
            if (double.TryParse(value, out doubleValue))
                return doubleValue;
            else
                return double.NaN;
        }

        /// <summary>
        /// Removes the key/value pair.
        /// </summary>
        /// <param name="key">A string containing the key/value key to remove.</param>
        public void RemoveKeyValue(string key)
        {
            if (keyValuePairs.ContainsKey(key))
                keyValuePairs.Remove(key);
        }

        /// <summary>
        /// Sets the resource to the desired amount and max amount.
        /// </summary>
        /// <param name="resourceName">A string containing the name of the resource.</param>
        /// <param name="amount">A double containing the resource amount.</param>
        /// <param name="maxAmount">A double containing the resource max amount.</param>
        public void SetResourceAmounts(string resourceName, double amount, double maxAmount)
        {
            ConfigNode node = new ConfigNode(ResourceNode);
            node.SetValue(ResourceNodeName, resourceName, true);
            node.SetValue(ResourceNodeAmount, amount.ToString(), true);
            node.SetValue(ResourceNodeMaxAmount, maxAmount.ToString(), true);

            if (resources.ContainsKey(resourceName))
                resources[resourceName] = node;
            else
                resources.Add(resourceName, node);
        }

        /// <summary>
        /// Retrieves the resource's amount and max amount if they exist.
        /// </summary>
        /// <param name="resourceName">A string containing the name of the resource.</param>
        /// <param name="amount">A double containing the amount of resource.</param>
        /// <param name="maxAmount">A double containing the max amount of the resource.</param>
        /// <returns>true if the resource was successfully retrieved, false if not.</returns>
        public bool GetResourceAmounts(string resourceName, out double amount, out double maxAmount)
        {
            amount = 0;
            maxAmount = 0;

            if (!resources.ContainsKey(resourceName))
                return false;

            ConfigNode node = resources[resourceName];
            double.TryParse(node.GetValue(ResourceNodeAmount), out amount);
            double.TryParse(node.GetValue(ResourceNodeMaxAmount), out maxAmount);

            return true;
        }

        /// <summary>
        /// Removes the desired resource from the map.
        /// </summary>
        /// <param name="resourceName">A string containing the name of the resource to remove.</param>
        public void RemoveResource(string resourceName)
        {
            if (resources.ContainsKey(resourceName))
                resources.Remove(resourceName);
        }

        /// <summary>
        /// Determines whether or not the resource exists.
        /// </summary>
        /// <param name="resourceName">A string containing the name of the resource.</param>
        /// <returns>true if the resource exists, false if not.</returns>
        public bool HasResource(string resourceName)
        {
            return resources.ContainsKey(resourceName);
        }
        #endregion
    }
}
