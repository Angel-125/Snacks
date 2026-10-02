using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using UnityEngine;
using Snacks;

// Runs pure snapshot/timer operations against the built mod and KSP assemblies.
// Unity components are allocated without invoking their native constructors.
class BackgroundRegression
{
    class TestConverter : SnacksBackgroundConverter
    {
        public bool UnlimitedElectricity;
        protected override bool HasUnlimitedElectricity() { return UnlimitedElectricity; }
    }
    class TestScenario : SnacksScenario
    {
        public int Failures;
        protected override void LogProcessingException(Exception exception) { Failures++; }
    }
    class TimelineScenario : TestScenario
    {
        public List<string> Trace;
        protected override Vessel[] GetProcessingVessels() { return new Vessel[0]; }
        protected override void RefreshBackgroundConverters() { Trace.Add("converters"); }
        protected override IEnumerator<YieldInstruction> processEvents(Dictionary<string, SnacksEvent> events, double elapsedSeconds = -1, bool checkUnscheduled = true)
        {
            Trace.Add("events:" + elapsedSeconds);
            yield return null;
        }
    }
    class OutputResource : ProcessedResource
    {
        public List<ProtoPartResourceSnapshot> Resources;
        protected override void getResourceTotals(Vessel vessel, out double amount, out double maximum, List<ProtoPartResourceSnapshot> list)
        {
            amount = 0; maximum = 0;
            foreach (ProtoPartResourceSnapshot resource in Resources)
                if (resource.flowState) { amount += resource.amount; maximum += resource.maxAmount; list.Add(resource); }
        }
    }
    class FailingWork : IEnumerator<YieldInstruction>
    {
        public bool Disposed;
        public YieldInstruction Current { get { return null; } }
        object System.Collections.IEnumerator.Current { get { return Current; } }
        public bool MoveNext() { throw new InvalidOperationException("Deliberate regression failure"); }
        public void Reset() { throw new NotSupportedException(); }
        public void Dispose() { Disposed = true; }
    }
    class ConsumptionResource : OutputResource
    {
        protected override double requestResource(Vessel vessel, double demand, List<ProtoPartResourceSnapshot> list)
        {
            double remaining = demand;
            foreach (var resource in list)
            {
                double taken = Math.Min(remaining, resource.amount);
                resource.amount -= taken; remaining -= taken;
            }
            return demand - remaining;
        }
    }
    class CrewProcessor : BaseResourceProcessor
    {
        public int FailedCrew;
        protected override void applyFailureOutcomes(Vessel vessel, SnacksProcessorResult result) { FailedCrew = result.affectedKerbalCount; }
    }
    class MealProcessor : SnacksResourceProcessor
    {
        public SnacksProcessorResult LastFailure;
        protected override void applyFailureOutcomes(Vessel vessel, SnacksProcessorResult result) { LastFailure = result; }
    }
    class MealOutput : OutputResource
    {
        protected override double addResource(Vessel vessel, double supply, List<ProtoPartResourceSnapshot> resources)
        {
            double remaining = supply;
            foreach (var resource in resources)
            {
                double added = Math.Min(remaining, resource.maxAmount - resource.amount);
                resource.amount += added; remaining -= added;
            }
            return supply - remaining;
        }
    }
    static string managedDirectory;
    static void Main(string[] args)
    {
        managedDirectory = args[0];
        AppDomain.CurrentDomain.AssemblyResolve += delegate(object sender, ResolveEventArgs request)
        {
            string path = System.IO.Path.Combine(managedDirectory, new AssemblyName(request.Name).Name + ".dll");
            return System.IO.File.Exists(path) ? Assembly.LoadFrom(path) : null;
        };
        Run();
        TestSimulatorWorkers();
        TestSolarDayPolicy();
    }
    static T Allocate<T>() { return (T)FormatterServices.GetUninitializedObject(typeof(T)); }

    static void TestSolarDayPolicy()
    {
        var dayField = typeof(SnacksScenario).GetField("secondsPerDay", BindingFlags.Static | BindingFlags.NonPublic);
        var oldDay = dayField.GetValue(null);
        try
        {
            double solarDay = SnacksTime.Day * 2 + 0.5;
            dayField.SetValue(null, solarDay);
            var nap = typeof(SnacksProperties).GetMethod("GetNapDurationSeconds", BindingFlags.Static | BindingFlags.NonPublic);
            Check((float)nap.Invoke(null, new object[] { FaintTime.OneDay }) == (float)solarDay, "One-day nap uses the solar day, with no calendar substitution or minute rounding");
            Check((float)nap.Invoke(null, new object[] { FaintTime.OneHour }) == 3600, "Hour naps retain physical seconds");
            Check(SnacksScenario.GetSecondsPerDay() == solarDay, "Processing day retains solar duration");
            Check(SnacksScenario.FormatTime(SnacksTime.Day, true).StartsWith("D1"), "Duration display uses a calendar day even when the solar day differs");
            string shortTime = SnacksScenario.FormatTime(SnacksTime.Minute + 15, true);
            Check(shortTime.Contains("M1") && shortTime.Contains("S15"), "Sub-hour display preserves minutes and seconds");
        }
        finally { dayField.SetValue(null, oldDay); }
        Console.WriteLine("PASS: solar-day naps/processing and calendar duration displays, including sub-hour time.");
    }

    static bool WaitUntil(Func<bool> predicate)
    {
        var timeout = DateTime.UtcNow.AddSeconds(3);
        while (DateTime.UtcNow < timeout)
        {
            if (predicate()) return true;
            System.Threading.Thread.Sleep(5);
        }
        return predicate();
    }

    static void TestSimulatorWorkers()
    {
        SnacksScenario.Instance.maxThreads = 1;
        var pool = new SnackSimThreadPool();
        var workers = (List<SnackSimThread>)Field(pool, "snackThreads").GetValue(pool);
        Check(workers.Count == 1, "Pool tracks its workers");
        var workerThread = (System.Threading.Thread)Field(workers[0], "jobThread").GetValue(workers[0]);
        try
        {
            Check(workerThread.IsBackground, "Simulator threads do not hold the process open");
            Check(WaitUntil(() => (workerThread.ThreadState & System.Threading.ThreadState.WaitSleepJoin) != 0), "Idle worker sleeps instead of busy spinning");
            var first = new SimSnacks { vessel = Allocate<Vessel>(), maxSimulatorCycles = 1 };
            pool.AddSimulatorJob(first);
            Check(WaitUntil(() => { pool.LockResourceDurations(); try { return pool.GetVesselResourceDurations(first.vessel) != null; } finally { pool.UnlockResourceDurations(); } }), "Worker consumes a queued job after sleeping");
            pool.StopAllJobs();
            Check(workerThread.IsAlive && workers[0].canRun, "Cancel estimates leaves workers available");
            var second = new SimSnacks { vessel = Allocate<Vessel>(), maxSimulatorCycles = 1 };
            pool.AddSimulatorJob(second);
            Check(WaitUntil(() => { pool.LockResourceDurations(); try { return pool.GetVesselResourceDurations(second.vessel) != null; } finally { pool.UnlockResourceDurations(); } }), "Workers accept new jobs after cancellation");
        }
        finally { pool.Stop(); }
        Check(!workerThread.IsAlive, "Stopping the pool joins idle workers");
        Console.WriteLine("PASS: simulator idle sleep, tracked background workers, cancellation/reuse, and shutdown.");
    }
    static void Check(bool valid, string message) { if (!valid) throw new Exception(message); }
    static FieldInfo Field(object target, string name)
    {
        for (Type type = target.GetType(); type != null; type = type.BaseType)
        {
            FieldInfo field = type.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (field != null) return field;
        }
        throw new Exception("Missing field: " + name);
    }
    static void Set(object target, string name, object value) { Field(target, name).SetValue(target, value); }
    static object Call(object target, string name, params object[] args)
    {
        for (Type type = target.GetType(); type != null; type = type.BaseType)
        {
            MethodInfo method = type.GetMethod(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (method != null) return method.Invoke(target, args);
        }
        throw new Exception("Missing method: " + name);
    }
    static ProtoPartResourceSnapshot Resource(string name, double amount, double max, bool unlocked)
    {
        ProtoPartResourceSnapshot resource = Allocate<ProtoPartResourceSnapshot>();
        resource.resourceName = name; resource.amount = amount; resource.maxAmount = max; resource.flowState = unlocked;
        return resource;
    }
    static void Run()
    {
        Vessel first = Allocate<Vessel>(), second = Allocate<Vessel>();
        first.id = Guid.NewGuid(); second.id = Guid.NewGuid();
        BaseResourceProcessor processor = new BaseResourceProcessor();
        Check((double)Call(processor, "AddVesselElapsedTime", first, 25.0) == 25, "First timer");
        Check((double)Call(processor, "AddVesselElapsedTime", second, 75.0) == 75, "Independent timer");
        ConfigNode saved = processor.OnSave();
        processor = new BaseResourceProcessor(); processor.OnLoad(saved);
        Check((double)Call(processor, "AddVesselElapsedTime", first, 10.0) == 35, "First timer reload");
        Check((double)Call(processor, "AddVesselElapsedTime", second, 10.0) == 85, "Second timer reload");
        ConfigNode legacy = new ConfigNode("SNACKS_RESOURCE_PROCESSOR"); legacy.AddValue("remainingTime", 50);
        processor.OnLoad(legacy);
        Check((double)Call(processor, "AddVesselElapsedTime", first, 10.0) == 60 &&
            (double)Call(processor, "AddVesselElapsedTime", second, 20.0) == 70, "Legacy timer phase");
        ConfigNode stressTimer = new ConfigNode("STRESS_VESSEL_TIMER"); stressTimer.AddValue("vesselID", first.id); stressTimer.AddValue("remainingTime", "12.5"); legacy.AddNode(stressTimer);
        processor.OnLoad(legacy);
        Check((double)Call(processor, "AddVesselElapsedTime", first, 1.0) == 13.5, "Earlier Stress save compatibility");

        SnacksEvent scheduled = new SnacksEvent { secondsBetweenChecks = 10, affectedKerbals = (KerbalsAffectedTypes)(-1) };
        scheduled.ProcessEvent(35);
        Check(scheduled.Save().GetValue("elapsedTime") == "5", "Event must catch up all three checks and retain remainder");
        SnacksEvent restored = new SnacksEvent { secondsBetweenChecks = 10, affectedKerbals = (KerbalsAffectedTypes)(-1) };
        restored.Load(scheduled.Save());
        Check(!restored.IsTimeToCheck(4) && restored.IsTimeToCheck(1), "Event remainder reload");
        scheduled.secondsBetweenChecks = 0; scheduled.ProcessEvent(100);
        Check(scheduled.Save().GetValue("elapsedTime") == "0", "Zero-interval events terminate");

        ProtoPartSnapshot part = Allocate<ProtoPartSnapshot>();
        part.resources = new List<ProtoPartResourceSnapshot> { Resource("ElectricCharge", 10, 20, true), Resource("ElectricCharge", 50, 100, false) };
        part.protoModuleCrew = new List<ProtoCrewMember>();
        ProtoVessel vessel = Allocate<ProtoVessel>();
        vessel.protoPartSnapshots = new List<ProtoPartSnapshot> { part };
        TestConverter converter = new TestConverter();
        Set(converter, "protoPart", part); converter.playerEmailed = true;
        converter.isMissingResources = true; converter.isContainerFull = true;
        converter.PrepareToProcess(vessel);
        Check(!converter.isMissingResources && !converter.isContainerFull, "Transient failures must reset for retry");
        Check((double)Call(converter, "getAmount", "ElectricCharge", ResourceFlowMode.ALL_VESSEL) == 10, "Locked input excluded");
        Check((double)Call(converter, "getAvailableSpace", "ElectricCharge", ResourceFlowMode.ALL_VESSEL) == 10, "Locked output excluded");
        Call(converter, "requestAmount", "ElectricCharge", 3.0, ResourceFlowMode.ALL_VESSEL);
        Check(part.resources[0].amount == 7 && part.resources[1].amount == 50, "Snapshot input mutation");
        Call(converter, "supplyAmount", "ElectricCharge", 2.0, ResourceFlowMode.NO_FLOW, false);
        Check(part.resources[0].amount == 9 && part.resources[1].amount == 50, "NO_FLOW output and resource locks");
        Set(converter, "minimumCrew", 1);
        converter.CheckRequiredResources(vessel, 1);
        Check(converter.isMissingResources, "Minimum crew blocks converter");
        converter.PrepareToProcess(vessel); Set(converter, "minimumCrew", 0);
        converter.CheckRequiredResources(vessel, 1);
        Check(!converter.isMissingResources, "Requirements retry after crew requirement changes");
        Set(converter, "outputList", new List<ResourceRatio> { new ResourceRatio { ResourceName = "ElectricCharge", FlowMode = ResourceFlowMode.ALL_VESSEL, Ratio = 20 } });
        converter.CheckRequiredResources(vessel, 1);
        Check(!converter.isContainerFull, "Partially empty storage must accept production");
        part.resources[0].amount = 20;
        converter.CheckRequiredResources(vessel, 1);
        Check(converter.isContainerFull && part.resources[0].amount == 20, "Full output capacity checked before input mutation");
        converter.PrepareToProcess(vessel);
        Set(converter, "outputList", new List<ResourceRatio> { new ResourceRatio { ResourceName = "ElectricCharge", FlowMode = ResourceFlowMode.ALL_VESSEL, Ratio = 20, DumpExcess = true } });
        converter.CheckRequiredResources(vessel, 1);
        Check(!converter.isContainerFull, "DumpExcess permits production");
        part.resources[0].amount = 9;
        Set(converter, "outputList", new List<ResourceRatio>());
        Set(converter, "inputList", new List<ResourceRatio> { new ResourceRatio { ResourceName = "ElectricCharge", FlowMode = ResourceFlowMode.ALL_VESSEL, Ratio = 2 } });
        converter.PrepareToProcess(vessel); converter.ConsumeInputResources(vessel, 1);
        Check(part.resources[0].amount == 7, "Background EC enabled consumes electricity");
        converter.UnlimitedElectricity = true;
        converter.ConsumeInputResources(vessel, 1);
        Check(part.resources[0].amount == 7, "Background EC disabled skips electricity");
        converter.UnlimitedElectricity = false;
        converter.PrepareToProcess(vessel); converter.ConsumeInputResources(vessel, 100);
        Check(converter.isMissingResources && part.resources[0].amount == 7, "Insufficient EC blocks conversion");
        converter.PrepareToProcess(vessel); part.resources[0].amount = 10;
        converter.ConsumeInputResources(vessel, 1);
        Check(!converter.isMissingResources && part.resources[0].amount == 8, "Conversion resumes after EC replenishment");
        TestRemainingFixes(first, vessel, part);
        Console.WriteLine("PASS: timers, event catch-up, EC/retries, snapshot locks, crew/storage requirements, multi-tank conservation, overflow, handoff timestamps, yield boundaries/remainders, and exception cleanup.");
    }

    static void TestRemainingFixes(Vessel ship, ProtoVessel vessel, ProtoPartSnapshot part)
    {
        var tanks = new List<ProtoPartResourceSnapshot> { Resource("Test", 6, 10, true), Resource("Test", 0, 100, true), Resource("Test", 0, 100, false) };
        double supplied = ProcessedResource.AddResource(ship, "Test", 10, tanks);
        Check(supplied == 10 && tanks[0].amount == 10 && tanks[1].amount == 6 && tanks[2].amount == 0, "Production must conserve resources across tanks");
        ProcessedResource resource = new ProcessedResource { resourceName = "Test" };
        supplied = (double)Call(resource, "addResource", ship, 1000.0, tanks);
        Check(supplied == 94 && tanks[1].amount == 100 && tanks[2].amount == 0, "Instance production clamps to unlocked capacity");
        OutputResource output = new OutputResource { resourceName = "Test", amount = 2, Resources = new List<ProtoPartResourceSnapshot> { Resource("Test", 5, 10, true) } };
        SnacksProcessorResult result = output.ProduceResource(ship, 1, 3, 3, new Dictionary<string, SnacksProcessorResult>());
        Check(!result.completedSuccessfully && result.affectedKerbalCount == 1 && result.currentAmount == 9 && output.Resources[0].amount == 9, "Overflow uses available capacity and reports unserved crew");

        TestConverter converter = new TestConverter(); Set(converter, "protoPart", part); converter.playerEmailed = true;
        ProtoPartModuleSnapshot module = Allocate<ProtoPartModuleSnapshot>(); module.moduleValues = new ConfigNode("MODULE");
        module.moduleValues.AddValue("lastUpdateTime", 3500); module.moduleValues.AddValue(SnacksBackgroundConverter.BackgroundUpdateTime, 3550);
        Set(converter, "moduleSnapshot", module);
        Call(converter, "ReadUpdateTime", module);
        Check(converter.GetElapsedTime(3600, 3600) == 50, "Handoff excludes previously loaded time");
        Set(converter, "isGenerator", true); converter.PostProcess(vessel, 3600);
        Check(converter.GetElapsedTime(60, 3600) == 0, "Repeated intervals are not counted twice");
        TestConverter restored = new TestConverter(); Call(restored, "ReadUpdateTime", module);
        Check(restored.GetElapsedTime(60, 3630) == 30, "Background handoff timestamp survives reconstruction");

        part.resources = new List<ProtoPartResourceSnapshot> { Resource("Yield", 0, 100, true) };
        converter.PrepareToProcess(vessel);
        Set(converter, "yieldsList", new List<ResourceRatio> { new ResourceRatio { ResourceName = "Yield", Ratio = 1, FlowMode = ResourceFlowMode.ALL_VESSEL } });
        Set(converter, "hoursPerCycle", 1.0); Set(converter, "cycleStartTime", 0.0); Set(converter, "yieldClockInitialized", true);
        converter.ProduceyieldsList(vessel, 3600);
        Check(part.resources[0].amount == 1, "Exact yield boundary produces one cycle");
        converter.ProduceyieldsList(vessel, 9360);
        Check(part.resources[0].amount == 2 && (double)Field(converter, "cycleStartTime").GetValue(converter) == 7200, "Yield rounds down and retains partial progress");
        converter.PauseYieldClock(3600);
        converter.ProduceyieldsList(vessel, 14400);
        Check(part.resources[0].amount == 3, "Paused time earns no extra yields");
        Set(converter, "hoursPerCycle", 0.0); converter.ProduceyieldsList(vessel, 20000);
        Check(part.resources[0].amount == 3, "Invalid yield duration cannot divide by zero");

        TestScenario scenario = Allocate<TestScenario>();
        FailingWork failing = new FailingWork();
        var safe = (IEnumerator<YieldInstruction>)Call(scenario, "executeSafely", failing);
        Check(!safe.MoveNext() && failing.Disposed && scenario.Failures == 1, "Failures must be logged and iterators disposed");
        Set(scenario, "snackCycleStarted", true);
        var batch = (IEnumerator<YieldInstruction>)Call(scenario, "runProcessingBatch", 1.0, 1.0);
        safe = (IEnumerator<YieldInstruction>)Call(scenario, "executeSafely", batch);
        Check(!safe.MoveNext() && !(bool)Field(scenario, "snackCycleStarted").GetValue(scenario), "Batch failure clears processing lock");

        TimelineScenario timeline = Allocate<TimelineScenario>(); timeline.Trace = new List<string>(); timeline.secondsPerCycle = 3600;
        timeline.resourceProcessors = new List<BaseResourceProcessor>();
        Set(timeline, "postProcessEvents", new Dictionary<string, SnacksEvent>());
        batch = (IEnumerator<YieldInstruction>)Call(timeline, "runProcessingBatch", 10800.0, 10800.0);
        while (batch.MoveNext()) { }
        Check(string.Join(",", timeline.Trace) == "converters,events:3600,converters,events:3600,converters,events:3600", "Catch-up interleaves converter refresh and events each hour");

        ship.protoVessel = vessel; ship.loaded = false;
        part.resources = new List<ProtoPartResourceSnapshot> { Resource("ElectricCharge", 0, 100, true), Resource("Processed", 0, 10000, true) };
        var generator = new TestConverter { IsActivated = true, playerEmailed = true };
        var consumer = new TestConverter { IsActivated = true, playerEmailed = true };
        Set(generator, "protoPart", part); Set(generator, "isGenerator", true);
        Set(generator, "outputList", new List<ResourceRatio> { new ResourceRatio { ResourceName = "ElectricCharge", Ratio = 1, FlowMode = ResourceFlowMode.ALL_VESSEL, DumpExcess = true } });
        Set(consumer, "protoPart", part);
        module = Allocate<ProtoPartModuleSnapshot>(); module.moduleValues = new ConfigNode("MODULE"); Set(consumer, "moduleSnapshot", module);
        Set(consumer, "inputList", new List<ResourceRatio> { new ResourceRatio { ResourceName = "ElectricCharge", Ratio = 0.4, FlowMode = ResourceFlowMode.ALL_VESSEL } });
        Set(consumer, "outputList", new List<ResourceRatio> { new ResourceRatio { ResourceName = "Processed", Ratio = 1, FlowMode = ResourceFlowMode.ALL_VESSEL } });
        Call(scenario, "runBackgroundConverterSteps", ship, new List<SnacksBackgroundConverter> { generator, consumer }, 3600.0, 3600.0);
        Check(part.resources[1].amount == 3600 && part.resources[0].amount >= 0 && part.resources[0].amount <= 100, "Continuous power processes one hour despite only a 100-EC battery");
        Check(scenario.Failures == 2, "Powered converter steps must not throw");

        SnacksScenario.Instance = scenario;
        var astronaut = Allocate<ProtoCrewMember>();
        foreach (FieldInfo crewField in typeof(ProtoCrewMember).GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            if (crewField.FieldType == typeof(string)) crewField.SetValue(astronaut, "Regression Kerbal");
        scenario.crewData = new DictionaryValueList<string, AstronautData>();
        scenario.crewData.Add(astronaut.name, new AstronautData { name = astronaut.name });
        var wrong = new BaseResourceProcessor { name = "Wrong" };
        wrong.productionResults["Test"] = new SnacksProcessorResult { completedSuccessfully = false };
        var right = new BaseResourceProcessor { name = "Right" };
        right.productionResults["Test"] = new SnacksProcessorResult { completedSuccessfully = true };
        scenario.resourceProcessors = new List<BaseResourceProcessor> { wrong, right };
        ConfigNode preconditionNode = new ConfigNode("PRECONDITION"); preconditionNode.AddValue("name", "CheckProcessorResult");
        preconditionNode.AddValue("type", "resultProductionSuccess"); preconditionNode.AddValue("resourceName", "Test"); preconditionNode.AddValue("processorName", "Right");
        var precondition = new CheckProcessorResult(preconditionNode);
        Check(precondition.IsValid(astronaut), "Production check uses production-only resource and named processor");
        precondition.resultType = CheckResultTypes.resultProductionFailure;
        Check(!precondition.IsValid(astronaut), "Successful production does not trigger failure event");

        TestExemptions(scenario, ship, part, astronaut);
        TestMeals(scenario, ship, part, astronaut);
    }
    static void TestExemptions(TestScenario scenario, Vessel ship, ProtoPartSnapshot part, ProtoCrewMember astronaut)
    {
        scenario.SetExemptCrew(" regression ; Valentina Kerman ;; ");
        Check(scenario.IsCrewExempt(astronaut.name), "Case-insensitive first-name match");
        Check(scenario.IsCrewExempt("Valentina Kerman") && !scenario.IsCrewExempt("Valentina Jones"), "Exact full-name match");
        Check(!scenario.IsCrewExempt("Reg Kerman") && !scenario.IsCrewExempt("Val Kerman"), "No substring matches");
        Check(scenario.GetAstronautData(astronaut).isExempt, "Existing crew flags updated");
        scenario.SetExemptCrew("");
        Check(!scenario.GetAstronautData(astronaut).isExempt, "Clearing list clears flags");
        scenario.SetExemptCrew(null);
        Check(!scenario.IsCrewExempt(astronaut.name), "Null list clears safely");
        scenario.exemptKerbals = "Regression";
        Check(scenario.GetAstronautData(astronaut).isExempt, "Direct API edits synchronize flags");
        scenario.crewData.Remove(astronaut.name);
        // Re-register without needing native experience effects or the game clock.
        // Matching is also exercised for an untracked name.
        Check(scenario.IsCrewExempt("Regression Kerman"), "Untracked crew inherit name matching");
        scenario.crewData.Add(astronaut.name, new AstronautData { name = astronaut.name });
        Check(scenario.GetAstronautData(astronaut).isExempt, "Newly tracked crew flags synchronize");

        ProtoCrewMember other = Allocate<ProtoCrewMember>();
        foreach (FieldInfo crewField in typeof(ProtoCrewMember).GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            if (crewField.FieldType == typeof(string)) crewField.SetValue(other, "Other Kerman");
        scenario.crewData.Add(other.name, new AstronautData { name = other.name });
        part.protoModuleCrew = new List<ProtoCrewMember> { astronaut, other };
        Set(ship.protoVessel, "crew", new List<ProtoCrewMember> { astronaut, other });
        Check(scenario.GetNonExemptCrewCount(ship) == 1 && scenario.GetNonExemptCrew(ship)[0] == other, "Unloaded crew list and count agree");
        var resource = new ConsumptionResource { resourceName = "Snacks", amount = 1,
            Resources = new List<ProtoPartResourceSnapshot> { Resource("Snacks", 10, 10, true) } };
        var processor = new CrewProcessor { secondsPerCycle = 10 };
        processor.inputList.Add(resource);
        processor.ProcessResources(ship, 20, 2, 2);
        Check(resource.Resources[0].amount == 8, "Mixed crew consume only for non-exempt crew on every catch-up cycle");
        Check(!scenario.GetAstronautData(astronaut).processedResourceSuccesses.ContainsKey("Snacks"), "Exempt crew do not record meal results");
        Check(scenario.GetAstronautData(other).processedResourceSuccesses["Snacks"] == 2, "Non-exempt meal results recorded");
        resource.Resources[0].amount = 0;
        processor.ProcessResources(ship, 10, 2, 2);
        Check(processor.FailedCrew == 1 && !scenario.GetAstronautData(astronaut).processedResourceFailures.ContainsKey("Snacks"), "Failures count only non-exempt crew");
        scenario.SetExemptCrew("Regression;Other");
        Check(scenario.GetNonExemptCrew(ship) == null && scenario.GetNonExemptCrewCount(ship) == 0, "All-exempt crew agree");
        resource.Resources[0].amount = 10;
        processor.ProcessResources(ship, 10, 2, 2);
        Check(resource.Resources[0].amount == 10, "All-exempt vessel consumes nothing");
        scenario.SetExemptCrew("");
        processor.ProcessResources(ship, 10, 2, 2);
        Check(resource.Resources[0].amount == 8, "Clearing exemptions resumes whole-crew consumption");
        ship.loaded = true;
        ship.parts = new List<Part>();
        Set(ship, "crew", new List<ProtoCrewMember> { astronaut, other });
        Set(ship, "crewCachedPartCount", 0);
        scenario.SetExemptCrew("Regression");
        Check(scenario.GetNonExemptCrewCount(ship) == 1, "Loaded crew filtering matches unloaded filtering");
        processor.ProcessResources(ship, 10, 2, 2);
        Check(resource.Resources[0].amount == 7, "Loaded mixed-crew consumption");
        ship.loaded = false;

        ConfigNode save = new ConfigNode("SCENARIO");
        save.AddValue("exemptKerbals", "Regression");
        scenario.GetAstronautData(astronaut).experienceTrait = "Pilot";
        scenario.GetAstronautData(other).experienceTrait = "Engineer";
        AstronautData.Save(scenario.crewData, save);
        TestScenario restored = Allocate<TestScenario>();
        restored.OnLoad(save);
        Check(restored.exemptKerbals == "Regression" && restored.crewData[astronaut.name].isExempt &&
            !restored.crewData[other.name].isExempt, "Scenario reload restores list and synchronizes saved flags");
        ConfigNode legacy = new ConfigNode("SCENARIO"); legacy.AddValue("exemptkerbals", "Other");
        restored.OnLoad(legacy);
        Check(restored.IsCrewExempt("Other Kerman"), "Legacy lowercase save key loads");
        restored.OnLoad(new ConfigNode("SCENARIO"));
        Check(!restored.IsCrewExempt("Other Kerman"), "Missing list does not retain previous-save exemptions");
        Console.WriteLine("PASS: exemption name matching, flag synchronization, clearing, mixed/all-exempt crew consumption, and failure accounting.");
    }

    static void TestMeals(TestScenario scenario, Vessel ship, ProtoPartSnapshot part, ProtoCrewMember first)
    {
        scenario.SetExemptCrew("");
        ProtoCrewMember second = part.protoModuleCrew[1];
        first.type = ProtoCrewMember.KerbalType.Crew; second.type = ProtoCrewMember.KerbalType.Crew;
        AstronautData firstData = scenario.GetAstronautData(first), secondData = scenario.GetAstronautData(second);
        Check(firstData.SnacksMultiplier == 1, "Default multiplier");
        firstData.SnacksMultiplier = 2; secondData.SnacksMultiplier = 1.5;
        foreach (double invalid in new double[] { 0, -1, double.NaN, double.PositiveInfinity, double.NegativeInfinity })
        {
            secondData.SnacksMultiplier = invalid;
            Check(secondData.SnacksMultiplier == 1, "Invalid multipliers fall back safely");
        }
        secondData.SnacksMultiplier = 1.5;
        var snack = new ConsumptionResource { resourceName = "Snacks", amount = 1,
            Resources = new List<ProtoPartResourceSnapshot> { Resource("Snacks", 30, 30, true) } };
        var soil = new MealOutput { resourceName = "Soil", amount = 1, failureResultAppliesOutcomes = false,
            Resources = new List<ProtoPartResourceSnapshot> { Resource("Soil", 0, 100, true) } };
        MealProcessor processor = Allocate<MealProcessor>();
        processor.secondsPerCycle = 10;
        processor.preconditions = new List<BasePrecondition>();
        processor.inputList = new List<ProcessedResource> { snack };
        processor.outputList = new List<ProcessedResource> { soil };
        processor.consumptionResults = new Dictionary<string, SnacksProcessorResult>();
        processor.productionResults = new Dictionary<string, SnacksProcessorResult>();
        Set(processor, "vesselRemainingTimes", new Dictionary<Guid, double>());
        ship.loaded = false; ship.vesselType = VesselType.Ship;
        processor.ProcessResources(ship, 20, 2, 2);
        Check(snack.Resources[0].amount == 23 && soil.Resources[0].amount == 7, "Unloaded catch-up consumes 2x and 1.5x meals and produces matching Soil");
        ship.loaded = true;
        processor.ProcessResources(ship, 10, 2, 2);
        Check(snack.Resources[0].amount == 19.5 && soil.Resources[0].amount == 10.5, "Loaded meals use identical multipliers");
        ship.loaded = false;
        snack.Resources[0].amount = 1.5;
        processor.ProcessResources(ship, 10, 2, 2);
        Check(processor.LastFailure.affectedKerbalCount == 1 && processor.LastFailure.afftectedAstronauts[0] == first, "A missing double portion counts as one hungry Kerbal");
        Check(firstData.processedResourceFailures["Snacks"] == 1 && !secondData.processedResourceFailures.ContainsKey("Snacks"), "Meal failure records identify only hungry crew");
        Check(soil.Resources[0].amount == 12 && processor.consumptionResults["Snacks"].consumedAmount == 1.5, "Soil on shortage matches the portion actually eaten");
        scenario.SetExemptCrew("Regression");
        snack.Resources[0].amount = 10;
        processor.ProcessResources(ship, 10, 2, 2);
        Check(snack.Resources[0].amount == 8.5 && SnacksResourceProcessor.GetMealPortions(part.protoModuleCrew) == 1.5, "Exemption wins over multiplier for consumption and estimates");
        scenario.SetExemptCrew("");
        var consumed = new List<ResourceRatio>(); var produced = new List<ResourceRatio>();
        processor.AddConsumedAndProducedResources(ship, 10, consumed, produced);
        Check(consumed[0].Ratio == 3.5 && produced[0].Ratio == 3.5, "Vessel estimates sum individual portions");
        consumed.Clear(); produced.Clear();
        processor.AddConsumedAndProducedResources(part.protoModuleCrew, 20, consumed, produced);
        Check(consumed[0].Ratio == 7, "Manifest estimates support identity and fractional multipliers");
        consumed.Clear(); produced.Clear();
        processor.AddConsumedAndProducedResources(2, 10, consumed, produced);
        Check(consumed[0].Ratio == 2, "Anonymous crew-count API preserves normal portions");

        // Run the actual snapshot withdrawal path, including a meal spanning tanks.
        part.resources = new List<ProtoPartResourceSnapshot> { Resource("Snacks", 1, 10, true),
            Resource("Snacks", 2.5, 10, true), Resource("Snacks", 9, 10, false) };
        var actualSnack = new ProcessedResource { resourceName = "Snacks", amount = 1 };
        SnacksProcessorResult actual = actualSnack.ConsumeMeals(ship, part.protoModuleCrew.ToArray(), 2);
        Check(actual.completedSuccessfully && actual.consumedAmount == 3.5 && part.resources[2].amount == 9,
            "Actual multi-tank withdrawal conserves portions and honors locks");
        part.resources = new List<ProtoPartResourceSnapshot> { Resource("Snacks", 0.3, 10, true) };
        firstData.SnacksMultiplier = 2; secondData.SnacksMultiplier = 1;
        actualSnack.amount = 0.1;
        actual = actualSnack.ConsumeMeals(ship, part.protoModuleCrew.ToArray(), 2);
        Check(actual.completedSuccessfully && Math.Abs(actual.consumedAmount - 0.3) < 1e-9, "Fractional meals tolerate floating-point roundoff");
        part.resources.Clear();
        actual = actualSnack.ConsumeMeals(ship, part.protoModuleCrew.ToArray(), 2);
        Check(actual.resultType == SnacksResultType.notApplicable, "Unvisited/no-storage vessels retain prior processing policy");
        secondData.SnacksMultiplier = 1.5;

        var save = new ConfigNode("SCENARIO");
        AstronautData.Save(scenario.crewData, save);
        var loaded = AstronautData.Load(save);
        Check(loaded[first.name].SnacksMultiplier == 2 && loaded[second.name].SnacksMultiplier == 1.5, "Multipliers survive save/reload");
        var previousCulture = System.Threading.Thread.CurrentThread.CurrentCulture;
        try
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("fr-FR");
            loaded = AstronautData.Load(save);
            Check(loaded[second.name].SnacksMultiplier == 1.5, "Saved multipliers use invariant culture");
        }
        finally { System.Threading.Thread.CurrentThread.CurrentCulture = previousCulture; }
        foreach (ConfigNode node in save.GetNodes("ASTRONAUT"))
            node.SetValue("snacksMultiplier", "NaN");
        loaded = AstronautData.Load(save);
        Check(loaded[first.name].SnacksMultiplier == 1, "Invalid saved multiplier falls back without losing crew data");
        foreach (ConfigNode node in save.GetNodes("ASTRONAUT"))
            node.RemoveValue("snacksMultiplier");
        loaded = AstronautData.Load(save);
        Check(loaded[first.name].SnacksMultiplier == 1 && loaded[second.name].SnacksMultiplier == 1, "Old saves default to normal portions");

        scenario.snacksEVAResources = new List<SnacksEVAResource>();
        Set(ship.protoVessel, "crew", new List<ProtoCrewMember> { first });
        part.protoModuleCrew = new List<ProtoCrewMember> { first };
        Set(ship, "crew", new List<ProtoCrewMember> { first });
        ship.vesselType = VesselType.EVA;
        firstData.SetResourceAmounts("Snacks", 4, 4);
        firstData.SetResourceAmounts("Soil", 0, 10);
        processor.ProcessResources(ship, 10, 1, 1);
        double remaining, maximum;
        firstData.GetResourceAmounts("Snacks", out remaining, out maximum);
        Check(remaining == 2 && processor.consumptionResults["Snacks"].consumedAmount == 2, "Unloaded EVA consumes scaled portion");
        ship.loaded = true;
        processor.ProcessResources(ship, 10, 1, 1);
        firstData.GetResourceAmounts("Snacks", out remaining, out maximum);
        Check(remaining == 0 && processor.consumptionResults["Snacks"].completedSuccessfully, "Loaded EVA consumes exact remaining portion");
        firstData.GetResourceAmounts("Soil", out remaining, out maximum);
        Check(remaining == 4, "EVA Soil matches consumption when a pack has Soil storage");
        processor.ProcessResources(ship, 10, 1, 1);
        Check(processor.LastFailure.affectedKerbalCount == 1 && firstData.processedResourceFailures["Snacks"] == 1, "EVA shortages identify one Kerbal");
        var pack = new SnacksEVAResource { resourceName = "Snacks", amount = 1 };
        Check(pack.GetPackAmount(firstData) == 2, "EVA pack portion scales");
        scenario.SetExemptCrew("Regression");
        Check(pack.GetPackAmount(firstData) == 0, "Exempt crew need no Snacks pack withdrawal");
        pack.resourceName = "FreshAir";
        Check(pack.GetPackAmount(firstData) == 1, "Other EVA resources do not scale with Snacks multiplier");
        scenario.SetExemptCrew("");
        SimSnacks simulator = SimSnacks.CreateSimulator(ship);
        Check(simulator.resources["Snacks"].amount == 0, "Loaded EVA estimates use roster pack resources");
        ship.loaded = false;
        simulator = SimSnacks.CreateSimulator(ship);
        Check(simulator.resources["Snacks"].amount == 0, "Unloaded EVA estimates use roster pack resources");
        var pool = Allocate<SnackSimThreadPool>();
        Set(pool, "mutex", new System.Threading.Mutex());
        Set(pool, "vesselResourceDurations", new Dictionary<Vessel, Dictionary<string, double>>());
        Set(pool, "convertersAssumedActive", new Dictionary<Vessel, bool>());
        Set(pool, "latestSimulators", new Dictionary<Vessel, SimSnacks>());
        pool.jobList = new List<SimSnacks>();
        Set(pool, "snackThreads", new List<SnackSimThread>());
        var oldEstimate = new SimSnacks { vessel = ship };
        var newEstimate = new SimSnacks { vessel = ship };
        oldEstimate.consumedResourceDurations["Snacks"] = 100;
        newEstimate.consumedResourceDurations["Snacks"] = 50;
        pool.AddSimulatorJob(oldEstimate); pool.AddSimulatorJob(newEstimate);
        Check(pool.jobList.Count == 1 && pool.jobList[0] == newEstimate, "New estimates replace queued jobs for the same vessel");
        Call(pool, "OnThreadSimulationComplete", oldEstimate);
        Check(pool.GetVesselResourceDurations(ship) == null, "Old estimate cannot overwrite new demand");
        Call(pool, "OnThreadSimulationComplete", newEstimate);
        Check(pool.GetVesselResourceDurations(ship)["Snacks"] == 50, "Current estimate publishes");
        Call(pool, "OnThreadSimulationComplete", oldEstimate);
        Check(pool.GetVesselResourceDurations(ship)["Snacks"] == 50, "Late stale completion cannot overwrite current estimate");
        pool.StopAllJobs();
        Check(pool.GetVesselResourceDurations(ship) == null, "Stopping estimates clears stale results");
        Console.WriteLine("PASS: saved/fractional multipliers, loaded/unloaded catch-up, individual hunger records, actual Soil production, exemptions, estimates, and EVA meals/packs.");
    }
}
