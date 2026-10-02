using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using System.Text;
using System.Threading;
using UnityEngine;
using KSP.UI;
using Highlighting;

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
namespace Snacks
{
    public class SnackAppView : Window<SnackAppView>
    {
        public string exemptKerbals = string.Empty;

        private Vector2 scrollPos = new Vector2();
        private Vector2 scrollPosButtons = new Vector2();
        private int selectedBody = 0;
        private GUILayoutOption[] flightWindowLeftPaneOptions = new GUILayoutOption[] { GUILayout.Width(200) };
        private GUILayoutOption[] flightWindowRightPaneOptions = new GUILayoutOption[] { GUILayout.Width(300) };

        private int partCount = 0;
        private bool simulationComplete = false;
        private string simulationResults = string.Empty;
        private int previousCrewCount = 0;
        private int currentCrewCount = -1;
        private List<Snackshot> snackshots = null;
        private SnackSimThread snackThread = null;
        private bool convertersAssumedActive = false;
        private bool showCrewView = false;
        private bool showAvailableCrew = false;
        private bool showStresstimator = false;
        private int crewCapacity = 0;
        private int activeVesselCrewCount = 0;
        private List<Part> habitableParts = new List<Part>();
        private List<Part> selectedParts = new List<Part>();
        private StressProcessor stressProcessor = null;
        private bool showSnackMultipliers;
        private Vector2 multiplierScroll;
        private readonly Dictionary<string, string> multiplierEntries = new Dictionary<string, string>();
        private string previousCrewSelection = string.Empty;

        public SnackAppView() :
        base(SnacksText.Get("#LOC_Snacks_GUI_VESSELSTATUS", "Vessel Status"), 500, 500)
        {
            Resizable = false;
        }

        public override void SetVisible(bool newValue)
        {
            base.SetVisible(newValue);

            if (newValue)
            {
                multiplierEntries.Clear();
                partCount = 0;
                crewCapacity = 0;
                previousCrewCount = 0;
                currentCrewCount = -1;
                selectedBody = -1;

                if (HighLogic.LoadedSceneIsFlight || HighLogic.LoadedScene == GameScenes.SPACECENTER)
                {
                    SnacksScenario.Instance.UpdateSnapshots();
                    SnacksScenario.onSnackTime.Add(onSnackTime);
                }
                else if (HighLogic.LoadedSceneIsEditor)
                {
                    snackshots = new List<Snackshot>();

                    GameEvents.onEditorPartEvent.Add(onEditorPartEvent);
                    GameEvents.onPartResourceListChange.Add(onPartResourceListChange);
                    GameEvents.onEditorShipModified.Add(onEditorShipModified);

                    snackThread = new SnackSimThread(new Mutex(), new List<SimSnacks>());
                    snackThread.OnSimulationComplete = OnSimulationComplete;
                    snackThread.Start();
                }

                exemptKerbals = SnacksScenario.Instance.exemptKerbals;
                SnacksScenario.Instance.SetExemptCrew(exemptKerbals);
            }

            else
            {
                if (snackThread != null)
                    snackThread.Stop();

                if (SnacksScenario.Instance == null)
                    return;
                SnacksScenario.Instance.SetExemptCrew(exemptKerbals);
                if (HighLogic.LoadedSceneIsFlight || HighLogic.LoadedScene == GameScenes.SPACECENTER)
                {
                    SnacksScenario.onSnackTime.Remove(onSnackTime);

                    if (SnacksScenario.Instance != null && SnacksScenario.Instance.threadPool != null)
                        SnacksScenario.Instance.threadPool.StopAllJobs();
                }
                else if (HighLogic.LoadedSceneIsEditor)
                {
                    GameEvents.onEditorPartEvent.Remove(onEditorPartEvent);
                    GameEvents.onPartResourceListChange.Remove(onPartResourceListChange);
                    GameEvents.onEditorShipModified.Remove(onEditorShipModified);
                }
            }
        }

        private void onSnackTime()
        {
            SnacksScenario.Instance.UpdateSnapshots();
        }

        private void onEditorPartEvent(ConstructionEventType eventType, Part part)
        {
            partCount = 0;
        }

        private void onPartResourceListChange(Part part)
        {
            partCount = 0;
        }

        private void onEditorShipModified(ShipConstruct ship)
        {
            partCount = 0;
        }

        protected override void DrawWindowContents(int windowId)
        {
            if (HighLogic.LoadedScene == GameScenes.SPACECENTER)
                drawSpaceCenterWindow();
            else if (HighLogic.LoadedSceneIsEditor == false)
                drawFlightWindow();
            else
                drawEditorWindow();
        }

        public void drawEditorWindow()
        {
            //Rerun sim button
            if (GUILayout.Button(SnacksText.Get("#LOC_Snacks_GUI_RERUNSIMULATOR", "Rerun Simulator")))
            {
                //Reset crew count so that we'll trigger a rebuild of the simulator.
                currentCrewCount = -1;
            }

            scrollPos = GUILayout.BeginScrollView(scrollPos);

            //Setup simulator
            setupSimulatorIfNeeded();

            //Update status
            snackThread.mutex.WaitOne();
            if (simulationComplete)
                formatSimulationResults();
            snackThread.mutex.ReleaseMutex();
            GUILayout.Label(simulationResults);

            GUILayout.EndScrollView();
        }

        private void setupSimulatorIfNeeded()
        {
            //If the vessel parts have changed or crew count has changed then run a new simulation.
            ShipConstruct ship = EditorLogic.fetch.ship;
            VesselCrewManifest manifest = CrewAssignmentDialog.Instance.GetManifest();
            int consumerCount = SnacksScenario.Instance.resourceProcessors.Count;
            List<BaseResourceProcessor> resourceProcessors = SnacksScenario.Instance.resourceProcessors;
            List<ProcessedResource> consumerResources;
            int resourceCount;
            int resourceIndex;
            string resourceName;
            Snackshot snackshot;

            if (manifest != null)
                currentCrewCount = manifest.CrewCount;

            //Get crew capacity
            crewCapacity = 0;
            int partCrewCapacity = 0;
            for (int index = 0; index < ship.parts.Count; index++)
            {
                if (ship.parts[index].partInfo.partConfig.HasValue("CrewCapacity"))
                {
                    int.TryParse(ship.parts[index].partInfo.partConfig.GetValue("CrewCapacity"), out partCrewCapacity);
                    crewCapacity += partCrewCapacity;
                }
            }

            List<ProtoCrewMember> assignedCrew = manifest != null ? manifest.GetAllCrew(false) : new List<ProtoCrewMember>();
            string crewSelection = string.Join(";", assignedCrew.Select(crew => crew.name + ":" +
                SnacksScenario.Instance.GetAstronautData(crew).SnacksMultiplier.ToString("R", CultureInfo.InvariantCulture) + ":" +
                SnacksScenario.Instance.IsCrewExempt(crew.name)).ToArray());
            if (ship.parts.Count != partCount || currentCrewCount != previousCrewCount || crewSelection != previousCrewSelection)
            {
                previousCrewSelection = crewSelection;
                previousCrewCount = currentCrewCount;
                partCount = ship.parts.Count;

                //No parts? Nothing to do.
                if (partCount == 0)
                {
                    snackshots.Clear();
                    simulationResults = SnacksText.Get("#LOC_Snacks_GUI_SIMU1", "<color=yellow><b>Vessel has no crewed parts to simulate.</b></color>");
                    simulationComplete = false;
                }
                else if (currentCrewCount == 0)
                {
                    snackshots.Clear();
                    simulationResults = SnacksText.Get("#LOC_Snacks_GUI_SIMU2", "<color=yellow><b>Vessel needs crew to run simulation.</b></color>");
                    simulationComplete = false;
                }

                //Clear existing simulation if any
                snackThread.ClearJobs();

                SimSnacks simSnacks = SimSnacks.CreateSimulator(ship);
                if (simSnacks != null)
                {
                    simulationComplete = false;
                    simulationResults = SnacksText.Get("#LOC_Snacks_GUI_SIMU3", "<color=white><b>Simulation in progress, please wait...</b></color>");
                    snackshots.Clear();

                    //Get consumer resource lists
                    for (int consumerIndex = 0; consumerIndex < consumerCount; consumerIndex++)
                    {
                        SnacksResourceProcessor snacksProcessor = resourceProcessors[consumerIndex] as SnacksResourceProcessor;
                        if (snacksProcessor != null)
                            snacksProcessor.AddConsumedAndProducedResources(assignedCrew, simSnacks.secondsPerCycle, simSnacks.consumedResources, simSnacks.producedResources);
                        else
                            resourceProcessors[consumerIndex].AddConsumedAndProducedResources(currentCrewCount, simSnacks.secondsPerCycle, simSnacks.consumedResources, simSnacks.producedResources);

                        //First check input list for resources to add to the snapshots window
                        consumerResources = resourceProcessors[consumerIndex].inputList;
                        resourceCount = consumerResources.Count;
                        for (resourceIndex = 0; resourceIndex < resourceCount; resourceIndex++)
                        {
                            resourceName = consumerResources[resourceIndex].resourceName;

                            if (consumerResources[resourceIndex].showInSnapshot && simSnacks.resources.ContainsKey(resourceName))
                            {
                                snackshot = new Snackshot();
                                snackshot.showTimeRemaining = true;
                                snackshot.resourceName = consumerResources[resourceIndex].resourceName;
                                snackshot.amount = simSnacks.resources[resourceName].amount;
                                snackshot.maxAmount = simSnacks.resources[resourceName].maxAmount;

                                //Add to snackshots
                                snackshots.Add(snackshot);
                            }
                        }

                        //Next check outputs
                        consumerResources = resourceProcessors[consumerIndex].outputList;
                        resourceCount = consumerResources.Count;
                        for (resourceIndex = 0; resourceIndex < resourceCount; resourceIndex++)
                        {
                            resourceName = consumerResources[resourceIndex].resourceName;

                            if (consumerResources[resourceIndex].showInSnapshot && simSnacks.resources.ContainsKey(resourceName))
                            {
                                snackshot = new Snackshot();
                                snackshot.showTimeRemaining = true;
                                snackshot.resourceName = consumerResources[resourceIndex].resourceName;
                                snackshot.amount = simSnacks.resources[resourceName].amount;
                                snackshot.maxAmount = simSnacks.resources[resourceName].maxAmount;

                                //Add to snackshots
                                snackshots.Add(snackshot);
                            }
                        }
                    }

                    //Give mods a chance to add custom converters not already covered by Snacks.
                    SimulatorContext context = new SimulatorContext();
                    context.shipConstruct = ship;
                    context.simulatedVesselType = SimulatedVesselTypes.simEditor;
                    SnacksScenario.onSimulatorCreated.Fire(simSnacks, context);

                    //Now start the simulation
                    snackThread.AddJob(simSnacks);
                }
                else
                {
                    simulationResults = SnacksText.Get("#LOC_Snacks_GUI_SIMU1", "<color=yellow><b>Vessel has no crewed parts to simulate.</b></color>");
                }
            }
        }

        private void formatSimulationResults()
        {
            StringBuilder simResults = new StringBuilder();
            int count = snackshots.Count;
            Snackshot snackshot;

            //current/max crew
            simResults.AppendLine("<color=white>" + SnacksText.Get("#LOC_Snacks_GUI_CREW", "Crew: ") + currentCrewCount + "/" + crewCapacity + "</color>");

            //Snackshot list
            for (int index = 0; index < count; index++)
            {
                snackshot = snackshots[index];
                simResults.AppendLine(snackshot.GetStatusDisplay());
            }

            //Add roster resource estimates such as Stress
            List<BaseResourceProcessor> processors = SnacksScenario.Instance.resourceProcessors;
            ShipConstruct ship = EditorLogic.fetch.ship;
            count = processors.Count;
            for (int index = 0; index < count; index++)
                processors[index].GetRosterResourceEstimatesForEditor(currentCrewCount, crewCapacity, simResults, ship);

            //Converter assumption
            if (convertersAssumedActive)
                simResults.AppendLine(SnacksText.Get("#LOC_Snacks_GUI_ASSUMEACTIVE", "<color=orange>Assumes converters are active; be sure to turn them on.</color>"));

            simulationResults = simResults.ToString();
        }

        private void OnSimulationComplete(SimSnacks simSnacks)
        {
            simulationComplete = true;

            //Snackshot list
            int count = snackshots.Count;
            Snackshot snackshot;
            for (int index = 0; index < count; index++)
            {
                snackshot = snackshots[index];

                if (simSnacks.consumedResourceDurations.ContainsKey(snackshot.resourceName))
                {
                    snackshot.isSimulatorRunning = false;
                    snackshot.estimatedTimeRemaining = simSnacks.consumedResourceDurations[snackshot.resourceName];
                }
            }

            convertersAssumedActive = simSnacks.convertersAssumedActive;
        }

        public void drawSpaceCenterWindow()
        {
            GUILayout.Label(SnacksText.Get("#LOC_Snacks_GUI_EXEMPTKERB", "<color=white><b>Exempt Kerbals:</b> separate first or full names by semicolon</color>"));
            GUILayout.Label(SnacksText.Get("#LOC_Snacks_GUI_EXEMPTKERB_INTRO", "<color=yellow>These kerbals won't consume Snacks and won't suffer penalties from a lack of Snacks.</color>"));
            if (string.IsNullOrEmpty(exemptKerbals))
                exemptKerbals = string.Empty;
            string updatedExemptKerbals = GUILayout.TextField(exemptKerbals);
            if (updatedExemptKerbals != exemptKerbals)
            {
                exemptKerbals = updatedExemptKerbals;
                SnacksScenario.Instance.SetExemptCrew(exemptKerbals);
            }

            drawSnackMultipliers();

            if (SnacksProperties.DebugLoggingEnabled)
            {
                if (GUILayout.Button(SnacksText.Get("#LOC_Snacks_GUI_SNACKTIME", "Snack Time!")))
                {
                    SnacksScenario.Instance.RunSnackCyleImmediately(SnacksScenario.GetSecondsPerDay() / SnacksProperties.MealsPerDay);
                }
            }

            drawFlightWindow();
        }

        private void drawSnackMultipliers()
        {
            showSnackMultipliers = GUILayout.Toggle(showSnackMultipliers, SnacksText.Get("#LOC_Snacks_GUI_PORTIONS", "Kerbal Snacks portions"));
            if (!showSnackMultipliers)
                return;
            GUILayout.Label(SnacksText.Get("#LOC_Snacks_GUI_PORTIONS_HELP", "1 = normal portion, 2 = double. Values of 1 or higher are saved per Kerbal. Exemptions take precedence."));
            multiplierScroll = GUILayout.BeginScrollView(multiplierScroll, GUILayout.Height(160));
            KerbalRoster roster = HighLogic.CurrentGame.CrewRoster;
            for (int index = 0; index < roster.Count; index++)
            {
                ProtoCrewMember crew = roster[index];
                if (crew.type == ProtoCrewMember.KerbalType.Unowned || crew.rosterStatus == ProtoCrewMember.RosterStatus.Dead ||
                    crew.rosterStatus == ProtoCrewMember.RosterStatus.Missing)
                    continue;
                AstronautData data = SnacksScenario.Instance.GetAstronautData(crew);
                string entry;
                if (!multiplierEntries.TryGetValue(crew.name, out entry))
                    entry = data.SnacksMultiplier.ToString("0.###", CultureInfo.CurrentCulture);
                GUILayout.BeginHorizontal();
                GUILayout.Label(crew.name, GUILayout.Width(145));
                entry = GUILayout.TextField(entry, GUILayout.Width(65));
                multiplierEntries[crew.name] = entry;
                double multiplier;
                bool valid = (double.TryParse(entry, NumberStyles.Float, CultureInfo.CurrentCulture, out multiplier) ||
                    double.TryParse(entry, NumberStyles.Float, CultureInfo.InvariantCulture, out multiplier)) &&
                    !double.IsNaN(multiplier) && !double.IsInfinity(multiplier) && multiplier >= 1;
                if (GUILayout.Button(SnacksText.Get("#LOC_Snacks_GUI_SET", "Set"), GUILayout.Width(40)) && valid)
                {
                    data.SnacksMultiplier = multiplier;
                    SnacksScenario.Instance.UpdateSnapshots();
                }
                if (!valid)
                    GUILayout.Label("Enter a number >= 1");
                else if (data.isExempt)
                    GUILayout.Label(SnacksText.Get("#LOC_Snacks_GUI_PORTIONS_EXEMPT", "Exempt: 0 Snacks/homeworld day"));
                else
                    GUILayout.Label(string.Format(SnacksText.Get("#LOC_Snacks_GUI_PORTIONS_RATE", "{0:0.###}x: {1:0.###} Snacks/homeworld day"), data.SnacksMultiplier,
                        data.SnacksMultiplier * SnacksProperties.SnacksPerMeal * SnacksProperties.MealsPerDay));
                GUILayout.EndHorizontal();
            }
            GUILayout.EndScrollView();
        }

        public void drawFlightWindow()
        {
            Dictionary<Vessel, VesselSnackshot> snapshotMap = SnacksScenario.Instance.snapshotMap;
            VesselSnackshot vesselSnackshot;
            List<Vessel> keys = snapshotMap.Keys.ToList();
            int count = keys.Count;
            int snackShotCount = 0;
            Snackshot snackshot;

            //Update resource durations
            updateResourcesDurations();

            GUILayout.BeginHorizontal();

            //Draw left pane
            drawFlightLeftPane();

            //Draw right pane
            drawFlightRightPane();

            GUILayout.EndHorizontal();

            //Draw stop simulators button
            if (GUILayout.Button(SnacksText.Get("#LOC_Snacks_GUI_STOPSIM", "Stop simulators (Estimates will be unavailable)")))
            {
                SnacksScenario.Instance.threadPool.StopAllJobs();
                for (int index = 0; index < keys.Count; index++)
                {
                    vesselSnackshot = snapshotMap[keys[index]];
                    snackShotCount = vesselSnackshot.snackshots.Count;
                    for (int snackShotIndex = 0; snackShotIndex < snackShotCount; snackShotIndex++)
                    {
                        snackshot = vesselSnackshot.snackshots[snackShotIndex];
                        if (snackshot.isSimulatorRunning)
                        {
                            snackshot.estimatedTimeRemaining = 0;
                            snackshot.isSimulatorRunning = false;
                            snackshot.simulatorInterrupted = true;
                        }
                    }
                }
            }

            if (SnacksProperties.DebugLoggingEnabled && HighLogic.LoadedSceneIsFlight)
            {
                if (GUILayout.Button(SnacksText.Get("#LOC_Snacks_GUI_SNACKTIME", "Snack Time!")))
                {
                    SnacksScenario.Instance.RunSnackCyleImmediately(SnacksScenario.GetSecondsPerDay() / SnacksProperties.MealsPerDay);
                }
            }
        }

        private void updateResourcesDurations()
        {
            Dictionary<Vessel, VesselSnackshot> snapshotMap = SnacksScenario.Instance.snapshotMap;
            VesselSnackshot vesselSnackshot;
            List<Vessel> keys = snapshotMap.Keys.ToList();
            int count = keys.Count;
            int snackShotCount = 0;
            Snackshot snackshot;
            Dictionary<string, double> resourceDurations = null;
            bool convertersAssumedActive;

            SnacksScenario.Instance.threadPool.LockResourceDurations();
            for (int index = 0; index < keys.Count; index++)
            {
                resourceDurations = SnacksScenario.Instance.threadPool.GetVesselResourceDurations(keys[index]);
                if (resourceDurations != null)
                {
                    convertersAssumedActive = SnacksScenario.Instance.threadPool.ConvertersAssumedActive(keys[index]);

                    SnacksScenario.Instance.threadPool.RemoveVesselResourceDurations(keys[index]);

                    vesselSnackshot = snapshotMap[keys[index]];
                    vesselSnackshot.convertersAssumedActive = convertersAssumedActive;
                    snackShotCount = vesselSnackshot.snackshots.Count;
                    for (int snackShotIndex = 0; snackShotIndex < snackShotCount; snackShotIndex++)
                    {
                        snackshot = vesselSnackshot.snackshots[snackShotIndex];
                        if (resourceDurations.ContainsKey(snackshot.resourceName))
                        {
                            snackshot.estimatedTimeRemaining = resourceDurations[snackshot.resourceName];
                            snackshot.isSimulatorRunning = false;
                        }
                    }
                }
            }
            SnacksScenario.Instance.threadPool.UnlockResourceDurations();
        }

        private void drawFlightLeftPane()
        {
            List<CelestialBody> bodies = FlightGlobals.Bodies;

            GUILayout.BeginVertical();
            scrollPosButtons = GUILayout.BeginScrollView(scrollPosButtons, flightWindowLeftPaneOptions);
            if (selectedBody == -1)
                selectedBody = FlightGlobals.currentMainBody.flightGlobalsIndex;
            if (SnacksScenario.Instance.bodyVesselCountMap.Keys.Count > 0)
            {
                int bodyCount = bodies.Count;
                for (int bodyIndex = 0; bodyIndex < bodyCount; bodyIndex++)
                {
                    //Skip body if it has new crewed vessels
                    if (!SnacksScenario.Instance.bodyVesselCountMap.ContainsKey(bodyIndex) || SnacksScenario.Instance.bodyVesselCountMap[bodyIndex] == 0)
                        continue;

                    //Record the selected body index
                    if (GUILayout.Button(bodies[bodyIndex].bodyName))
                    {
                        selectedBody = bodyIndex;
                    }
                }
            }
            else
            {
                GUILayout.Label(SnacksText.Get("#LOC_Snacks_GUI_SIMU4", "<color=white>No crewed vessels found on or around any world or in solar orbit.</color>"));
            }

            GUILayout.EndScrollView();
            GUILayout.EndVertical();
        }

        private void drawFlightRightPane()
        {
            Dictionary<Vessel, VesselSnackshot> snapshotMap = SnacksScenario.Instance.snapshotMap;
            VesselSnackshot vesselSnackshot;
            List<Vessel> keys = snapshotMap.Keys.ToList();
            int count = keys.Count;
            List<CelestialBody> bodies = FlightGlobals.Bodies;
            bool isExempt = false;

            //Draw stresstimator?
            if (showStresstimator && HighLogic.LoadedSceneIsFlight)
            {
                drawStresstimator();
                return;
            }

            GUILayout.BeginVertical();
            if (SnacksScenario.Instance.rosterResources.Count > 0)
            {
                showCrewView = GUILayout.Toggle(showCrewView, SnacksText.Get("#LOC_Snacks_GUI_CREWVIEW", "Show Crew View"));
                if (showCrewView)
                    showAvailableCrew = GUILayout.Toggle(showAvailableCrew, SnacksText.Get("#LOC_Snacks_GUI_CREWVIEWA", "Show Available Crew"));

                //Stresstimator button
                if (SnacksScenario.Instance.rosterResources.ContainsKey(StressProcessor.StressResourceName) && HighLogic.LoadedSceneIsFlight)
                {
                    if (GUILayout.Button(SnacksText.Get("#LOC_Snacks_GUI_STRESSSIM", "Open Stresstimator")))
                    {
                        showStresstimator = true;
                        return;
                    }
                }
            }

            //Show vessel crew status
            if (!showAvailableCrew)
            {
                scrollPos = GUILayout.BeginScrollView(scrollPos, flightWindowRightPaneOptions);
                GUILayout.Label("<color=lightBlue><b>" + bodies[selectedBody].bodyName + "</b></color>");
                count = keys.Count;
                string statusDisplay;
                for (int index = 0; index < count; index++)
                {
                    vesselSnackshot = snapshotMap[keys[index]];

                    //Skip if vessel's planetary body doesn't match the filter.
                    if (vesselSnackshot.bodyID != selectedBody)
                        continue;

                    //Get status
                    statusDisplay = vesselSnackshot.GetStatusDisplay(showCrewView);
                    if (vesselSnackshot.convertersAssumedActive && !showCrewView)
                        statusDisplay = statusDisplay + SnacksText.Get("#LOC_Snacks_GUI_ASSUMEACTIVE", "<color=orange>Assumes converters are active; be sure to turn them on.</color>");

                    // Exempt
                    isExempt = SnacksScenario.Instance.isExemptFromProcessors(vesselSnackshot.vessel.id);
                    // Flip so that !isExempt will ENABLE the toggle if !isExempt = false.
                    isExempt = !isExempt;
                    isExempt = GUILayout.Toggle(isExempt, "Snacks enabled");
                    // Now flip it back
                    isExempt = !isExempt;
                    if (isExempt != SnacksScenario.Instance.isExemptFromProcessors(vesselSnackshot.vessel.id))
                    {
                        SnacksScenario.Instance.SetExemptVessel(vesselSnackshot.vessel, isExempt);
                    }

                    //Print status
                    GUILayout.Label(statusDisplay);
                }
            }

            //Show status of crews available to fly
            else
            {
                KerbalRoster roster = HighLogic.CurrentGame.CrewRoster;
                ProtoCrewMember astronaut;
                IEnumerator<ProtoCrewMember> enumerator = roster.Crew.GetEnumerator();
                AstronautData astronautData;
                StringBuilder status = new StringBuilder();
                string conditionSummary;

                scrollPos = GUILayout.BeginScrollView(scrollPos, flightWindowRightPaneOptions);

                while (enumerator.MoveNext())
                {
                    astronaut = enumerator.Current;
                    if (astronaut.rosterStatus != ProtoCrewMember.RosterStatus.Available)
                        continue;

                    if (SnacksScenario.Instance.crewData.ContainsKey(astronaut.name))
                    {
                        astronautData = SnacksScenario.Instance.GetAstronautData(astronaut);

                        status.AppendLine("<color=orange><i>" + astronaut.name + "</i></color>");
                        if (!string.IsNullOrEmpty(astronautData.conditionSummary))
                            conditionSummary = SnacksText.DisplayConditions(astronautData.conditionSummary);
                        else
                            conditionSummary = SnacksText.Get("#LOC_Snacks_GUI_COF", "Cleared for flight");
                        status.AppendLine("<color=white> " + SnacksText.Get("#LOC_Snacks_GUI_STATUS", "- Status: ") + conditionSummary + "</color>");

                        string[] rosterResourceKeys = astronautData.rosterResources.Keys.ToArray();
                        for (int rosterIndex = 0; rosterIndex < rosterResourceKeys.Length; rosterIndex++)
                        {
                            if (astronautData.rosterResources[rosterResourceKeys[rosterIndex]].showInSnapshot)
                                status.AppendLine(astronautData.rosterResources[rosterResourceKeys[rosterIndex]].GetStatusDisplay());
                        }
                    }
                    else
                    {
                        status.AppendLine("<color=orange><i>" + astronaut.name + "</i></color>");
                        conditionSummary = SnacksText.Get("#LOC_Snacks_GUI_COF", "Cleared for flight");
                        status.AppendLine("<color=white> " + SnacksText.Get("#LOC_Snacks_GUI_STATUS", "- Status: ") + conditionSummary + "</color>");
                    }
                }
                GUILayout.Label(status.ToString());
            }

            GUILayout.EndScrollView();
            GUILayout.EndVertical();
        }

        private void drawStresstimator()
        {
            //Find habitable parts
            findHabitableParts();
            if (habitableParts.Count == 0)
            {
                showStresstimator = false;
                return;
            }

            //Get the Stress processor
            findStressProcessor();
            if (stressProcessor == null)
            {
                unhighlightHabitableParts();
                showStresstimator = false;
                habitableParts.Clear();
                selectedParts.Clear();
                return;
            }

            GUILayout.BeginVertical();
            scrollPos = GUILayout.BeginScrollView(scrollPos, flightWindowRightPaneOptions);

            GUILayout.Label(SnacksText.Get("#LOC_Snacks_GUI_STRESSNOTE", "<color=white>Your kerbals might get Stressed Out if you transfer them to a docked vessel and then undock. Will that happen? Click on the habitable parts of the docked vessel to find out. Parts highlighted in blue will count towards the estimate.</color>"));

            //Highlight all habitable parts
            highlightHabitableParts();

            //Calculate and show the estimates
            drawStressEstimates();

            GUILayout.EndScrollView();

            if (GUILayout.Button(SnacksText.Get("#LOC_Snacks_GUI_STRESSCLOSE", "Close Stresstimator")))
            {
                unhighlightHabitableParts();
                showStresstimator = false;
                habitableParts.Clear();
                selectedParts.Clear();
            }

            GUILayout.EndVertical();
        }

        private void drawStressEstimates()
        {
            ProtoCrewMember[] astronauts = FlightGlobals.ActiveVessel.GetVesselCrew().ToArray(); ;
            AstronautData astronautData = null;

            //Show crew capacity
            GUILayout.Label(string.Format(SnacksText.Get("#LOC_Snacks_GUI_CREWCAPA", "<color=white>Estimated Crew Capacity: {0:n0}</color>"), crewCapacity));

            //Calculate the total space.
            float space = stressProcessor.CalculateSpace(activeVesselCrewCount, crewCapacity);

            //Get experience bonus
            double stressExperienceBonus = SnacksScenario.Instance.rosterResources[StressProcessor.StressResourceName].experienceBonusMaxAmount;

            //Show who will get stressed and who won't.
            SnacksRosterResource resource;
            double amount, maxAmount, bonusMaxStress = 0;
            int experienceLevel = 0;
            for (int index = 0; index < astronauts.Length; index++)
            {
                astronautData = SnacksScenario.Instance.GetAstronautData(astronauts[index]);
                if (astronautData == null)
                    continue;
                if (!astronautData.rosterResources.ContainsKey(StressProcessor.StressResourceName))
                    continue;

                resource = astronautData.rosterResources[StressProcessor.StressResourceName];
                experienceLevel = astronauts[index].experienceTrait.CrewMemberExperienceLevel();
                bonusMaxStress = stressExperienceBonus * experienceLevel;
                maxAmount = space + bonusMaxStress;
                amount = resource.amount;

                if (amount > maxAmount)
                    GUILayout.Label("<color=white>" + astronautData.name + string.Format(SnacksText.Get("#LOC_Snacks_GUI_STRESSES", "\n Estimated Stress: {0:n2}/{1:n2}"), amount, maxAmount) + "</color>" + SnacksText.Get("#LOC_Snacks_GUI_STRESSOUT", "\n<color=orange> Will likely get Stressed Out</color>"));
                else
                    GUILayout.Label("<color=white>" + astronautData.name + string.Format(SnacksText.Get("#LOC_Snacks_GUI_STRESSES", "\n Estimated Stress: {0:n2}/{1:n2}"), amount, maxAmount) + "</color>");
            }
        }

        private void findStressProcessor()
        {
            if (stressProcessor == null)
            {
                int count = SnacksScenario.Instance.resourceProcessors.Count;
                for (int index = 0; index < count; index++)
                {
                    if (SnacksScenario.Instance.resourceProcessors[index] is StressProcessor)
                    {
                        stressProcessor = (StressProcessor)SnacksScenario.Instance.resourceProcessors[index];
                        break;
                    }
                }
            }
        }

        private void findHabitableParts()
        {
            if (habitableParts.Count > 0)
                return;

            Vessel vessel = FlightGlobals.ActiveVessel;
            crewCapacity = 0;
            activeVesselCrewCount = vessel.GetCrewCount();

            int count = vessel.Parts.Count;
            Part part;
            for (int index = 0; index < count; index++)
            {
                part = vessel.Parts[index];

                if (part.CrewCapacity > 0)
                {
                    habitableParts.Add(part);
                    part.AddOnMouseDown(onPartMouseDown);
                }
            }
        }

        private void highlightHabitableParts()
        {
            if (habitableParts.Count == 0)
                return;

            int count = habitableParts.Count;
            for (int index = 0; index < count; index++)
            {
                if (habitableParts[index].HighlightActive)
                    continue;

                if (selectedParts.Contains(habitableParts[index]))
                    habitableParts[index].Highlight(Highlighter.colorPartTransferDestHighlight);
                else
                    habitableParts[index].Highlight(Highlighter.colorPartTransferSourceHighlight);
            }
        }

        private void unhighlightHabitableParts()
        {
            int count = habitableParts.Count;

            for (int index = 0; index < count; index++)
            {
                habitableParts[index].Highlight(false);
                habitableParts[index].SetHighlightDefault();
                habitableParts[index].RemoveOnMouseDown(onPartMouseDown);
            }
        }

        private void onPartMouseDown(Part partClicked)
        {
            if (!selectedParts.Contains(partClicked))
                selectedParts.Add(partClicked);

            else if (selectedParts.Contains(partClicked))
                selectedParts.Remove(partClicked);

            int count = selectedParts.Count;
            crewCapacity = 0;
            for (int index = 0; index < count; index++)
            {
                crewCapacity += selectedParts[index].CrewCapacity;
            }
        }
    }
}
