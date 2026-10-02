// Compile the production Stress classes against deterministic KSP/Unity doubles.
// This checks processor logic without starting KSP; it does not test Unity integration.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Snacks;

class StressRegression
{
    static void Check(bool valid, string message) { if (!valid) throw new Exception(message); }
    static SnacksRosterResource Resource(ProtoCrewMember crew) { return SnacksScenario.Instance.GetAstronautData(crew).rosterResources["Stress"]; }
    static Vessel Ship(bool loaded, params ProtoCrewMember[] crew)
    {
        Vessel vessel = new Vessel { loaded = loaded, crew = crew.ToList() };
        vessel.protoVessel.crew = vessel.crew;
        foreach (ProtoCrewMember member in crew)
            SnacksScenario.Instance.GetAstronautData(member).rosterResources["Stress"] = new SnacksRosterResource { resourceName = "Stress", maxAmount = 16 };
        return vessel;
    }
    static void Tick(StressProcessor processor, Vessel vessel, double time) { processor.ProcessResources(vessel, time, vessel.crew.Count, vessel.capacity); }
    static void Main()
    {
        StressProcessor processor = new StressProcessor();
        SnacksScenario.Instance.rosterResources["Stress"] = new SnacksRosterResource { resourceName = "Stress" };
        processor.Initialize();
        ProtoCrewMember a = new ProtoCrewMember { name = "A" }, b = new ProtoCrewMember { name = "B" };
        Vessel first = Ship(true, a), second = Ship(false, b);
        Tick(processor, first, 50); Tick(processor, second, 50);
        Check(Resource(a).amount == 0 && Resource(b).amount == 0, "Vessels must not share daily time.");
        Tick(processor, second, 50); Tick(processor, first, 50);
        Check(Resource(a).amount == 1 && Resource(b).amount == 1, "Each loaded/unloaded vessel must process once per day.");

        Tick(processor, first, 25); Tick(processor, second, 75);
        ConfigNode saved = processor.OnSave();
        processor.Destroy(); processor = new StressProcessor(); processor.Initialize(); processor.OnLoad(saved);
        Tick(processor, first, 75); Tick(processor, second, 25);
        Check(Resource(a).amount == 2 && Resource(b).amount == 2, "Save/reload must retain each vessel's remainder.");

        ConfigNode oldSave = new ConfigNode("SNACKS_RESOURCE_PROCESSOR"); oldSave.AddValue("remainingTime", 50);
        processor.OnLoad(oldSave);
        Tick(processor, first, 50); Tick(processor, second, 50);
        Check(Resource(a).amount == 3 && Resource(b).amount == 3, "Legacy shared remainder must initialize both vessels.");

        AstronautData data = SnacksScenario.Instance.GetAstronautData(a);
        Resource(a).amount = 16;
        SnacksScenario.onRosterResourceUpdated.Fire(first, Resource(a), data, a);
        Check(data.conditionSummary.Contains("Stressed Out") && !a.skills, "Stress threshold must remove skills immediately.");
        data.SetCondition("Relaxing"); Resource(a).amount = 15;
        SnacksScenario.onRosterResourceUpdated.Fire(first, Resource(a), data, a);
        Check(!data.conditionSummary.Contains("Stressed Out") && !a.skills, "Recovery must retain skill loss from Relaxing.");
        data.ClearCondition("Relaxing"); data.SetCondition("Stressed Out");
        new OnStrikePenalty("Stressed Out", false, "").RemoveOutcome(first, false);
        Check(a.skills && !data.conditionSummary.Contains("Stressed Out"), "RemoveOutcome must clear the condition before restoring skills.");
        data.SetCondition("Stressed Out"); data.SetCondition("Relaxing"); a.skills = false;
        new OnStrikePenalty("Stressed Out", false, "").RemoveOutcome(first, false);
        Check(!a.skills && data.conditionSummary.Contains("Relaxing"), "Removing the penalty must respect other skill-loss conditions.");
        data.ClearCondition("Relaxing");

        ProtoCrewMember c = new ProtoCrewMember { name = "C" };
        Vessel mixed = Ship(true, a, c); mixed.capacity = 2;
        Resource(a).amount = 20; Resource(c).amount = 0;
        processor.onKerbalBoardedVessel(c, new Part { vessel = mixed });
        Check(data.conditionSummary.Contains("Stressed Out") && !SnacksScenario.Instance.GetAstronautData(c).conditionSummary.Contains("Stressed Out"), "Boarding must use all crew, not the last member.");
        Resource(a).amount = 0;
        processor.onKerbalBoardedVessel(a, new Part { vessel = mixed });
        Check(a.skills && !data.conditionSummary.Contains("Stressed Out"), "Boarding must clear stale stress penalties.");

        processor.OnLoad(new ConfigNode("SNACKS_RESOURCE_PROCESSOR"));
        Resource(a).amount = 20;
        Tick(processor, mixed, 300);
        SnacksProcessorResult result = processor.productionResults["Stress"];
        Check(result.affectedKerbalCount == 1 && result.afftectedAstronauts.Count == 1, "Catch-up cycles must not accumulate affected crew across days.");

        ConfigNode rosterSave = new ConfigNode("ASTRONAUT");
        SnacksRosterResource.SaveToAstronautData(data.rosterResources, rosterSave);
        data.rosterResources = SnacksRosterResource.LoadFromAstronautData(rosterSave);
        Check(Resource(a).resourceName == "Stress", "Roster load must restore resource identity.");
        Resource(a).amount = 0;
        SnacksScenario.onRosterResourceUpdated.Fire(null, Resource(a), data, a);
        Check(a.skills && !data.conditionSummary.Contains("Stressed Out"), "Restored roster must recover even without a vessel at KSC.");
        SnacksScenario.onRosterResourceUpdated.Fire(null, new SnacksRosterResource { resourceName = "Other", amount = 100 }, data, a);
        Check(!data.conditionSummary.Contains("Stressed Out"), "Other resources must not affect Stress conditions.");
        Resource(a).amount = Resource(a).maxAmount;
        SnacksScenario.onRosterResourceUpdated.Fire(null, Resource(a), data, a);
        Check(data.conditionSummary.Contains("Stressed Out"), "Threshold notifications must support a Kerbal without a vessel.");
        data.ClearCondition("Stressed Out"); a.skills = true; data.isExempt = true;
        SnacksScenario.onRosterResourceUpdated.Fire(null, Resource(a), data, a);
        Check(a.skills && !data.conditionSummary.Contains("Stressed Out"), "Exempt Kerbals do not receive Stress threshold penalties.");
        new OnStrikePenalty("Stressed Out", false, "").ApplyOutcome(first, new SnacksProcessorResult());
        Check(a.skills && !data.conditionSummary.Contains("Stressed Out"), "Strike penalties skip an all-exempt crew.");
        processor.onVesselLoaded(first);
        Check(!data.conditionSummary.Contains("Stressed Out"), "Loading a vessel does not penalize exempt stressed crew.");
        Console.WriteLine("PASS: independent vessel timers, loaded/unloaded processing, save/reload, legacy saves, immediate thresholds, recovery conditions, boarding, catch-up, roster identity, and KSC recovery.");
    }
}

// Minimal deterministic substitutes for the production classes' collaborators.
public class ConfigNode
{
    public string name; readonly Dictionary<string, string> values = new Dictionary<string, string>(); readonly List<ConfigNode> nodes = new List<ConfigNode>();
    public ConfigNode(string name) { this.name = name; }
    public bool HasValue(string key) { return values.ContainsKey(key); }
    public string GetValue(string key) { return HasValue(key) ? values[key] : null; }
    public void AddValue(string key, object value) { values[key] = Convert.ToString(value); }
    public void AddNode(ConfigNode node) { nodes.Add(node); }
    public ConfigNode[] GetNodes(string name) { return nodes.Where(n => n.name == name).ToArray(); }
}
public class GameDatabase { public static GameDatabase Instance = new GameDatabase(); public ConfigNode[] GetConfigNodes(string name) { return new ConfigNode[0]; } }
public class ExperienceTrait { public int CrewMemberExperienceLevel() { return 0; } }
public class ProtoCrewMember { public string name; public float stupidity; public bool isBadass; public bool skills = true; public ExperienceTrait experienceTrait = new ExperienceTrait(); }
public class CelestialBody { public bool isHomeWorld, atmosphere, atmosphereContainsOxygen; }
public class ProtoVessel { public List<ProtoCrewMember> crew = new List<ProtoCrewMember>(); public List<ProtoCrewMember> GetVesselCrew() { return crew; } }
public class Vessel
{
    public Guid id = Guid.NewGuid(); public bool loaded, LandedOrSplashed; public string vesselName = "Test"; public int capacity = 1;
    public List<ProtoCrewMember> crew = new List<ProtoCrewMember>(); public ProtoVessel protoVessel = new ProtoVessel(); public CelestialBody mainBody = new CelestialBody();
    public List<ProtoCrewMember> GetVesselCrew() { return crew; } public int GetCrewCount() { return crew.Count; } public int GetCrewCapacity() { return capacity; }
}
public class Part { public Vessel vessel; } public class ShipConstruct { }
public enum ScreenMessageStyle { UPPER_LEFT }
public static class ScreenMessages { public static void PostScreenMessage(string text, float time, ScreenMessageStyle style) { } }
namespace UnityEngine { public static class Debug { public static void Log(string text) { } } public static class Random { public static float Range(float min, float max) { return max; } public static int Range(int min, int max) { return min; } } }
namespace KSP.IO { }
namespace Snacks
{
    public class AstronautData
    {
        public bool isExempt;
        public string name, conditionSummary = ""; public Dictionary<string, SnacksRosterResource> rosterResources = new Dictionary<string, SnacksRosterResource>();
        public void SetCondition(string value) { if (!conditionSummary.Contains(value)) conditionSummary += value + ";"; }
        public void ClearCondition(string value) { conditionSummary = conditionSummary.Replace(value + ";", ""); }
    }
    public static class SnacksProperties { public static bool DebugLoggingEnabled; }
    public class RosterEvent
    {
        event Action<Vessel, SnacksRosterResource, AstronautData, ProtoCrewMember> handlers;
        public void Add(Action<Vessel, SnacksRosterResource, AstronautData, ProtoCrewMember> handler) { handlers += handler; }
        public void Remove(Action<Vessel, SnacksRosterResource, AstronautData, ProtoCrewMember> handler) { handlers -= handler; }
        public void Fire(Vessel vessel, SnacksRosterResource resource, AstronautData data, ProtoCrewMember crew) { if (handlers != null) handlers(vessel, resource, data, crew); }
    }
    public class SnacksScenario
    {
        public static SnacksScenario Instance = new SnacksScenario(); public static RosterEvent onRosterResourceUpdated = new RosterEvent();
        public Dictionary<string, SnacksRosterResource> rosterResources = new Dictionary<string, SnacksRosterResource>();
        readonly Dictionary<ProtoCrewMember, AstronautData> astronauts = new Dictionary<ProtoCrewMember, AstronautData>();
        public static double GetSecondsPerDay() { return 100; }
        public AstronautData GetAstronautData(ProtoCrewMember crew) { if (!astronauts.ContainsKey(crew)) astronauts[crew] = new AstronautData { name = crew.name }; return astronauts[crew]; }
        public void SetAstronautData(AstronautData data) { }
        public ProtoCrewMember[] GetNonExemptCrew(Vessel vessel) { var crew = vessel.crew.Where(member => !GetAstronautData(member).isExempt).ToArray(); return crew.Length == 0 ? null : crew; }
        public int GetCrewCapacity(Vessel vessel) { return vessel.capacity; }
        bool HasSkillLoss(ProtoCrewMember crew) { string conditions = GetAstronautData(crew).conditionSummary; return conditions.Contains("Stressed Out") || conditions.Contains("Relaxing"); }
        public void RemoveSkillsIfNeeded(ProtoCrewMember crew) { if (HasSkillLoss(crew)) crew.skills = false; }
        public void RestoreSkillsIfNeeded(ProtoCrewMember crew) { if (!HasSkillLoss(crew)) crew.skills = true; }
    }
    public struct SnacksProcessorResult { public string resourceName; public bool completedSuccessfully; public int crewCount, crewCapacity, affectedKerbalCount; public List<ProtoCrewMember> afftectedAstronauts; }
    public class BaseOutcome
    {
        protected const string ConditionName = "conditionName"; protected bool selectRandomCrew; protected string playerMessage;
        public BaseOutcome(bool random, string message) { playerMessage = message; } public BaseOutcome(ConfigNode node) { }
        public virtual void ApplyOutcome(Vessel vessel, SnacksProcessorResult result) { } public virtual void RemoveOutcome(Vessel vessel, bool informPlayer) { } public virtual bool IsEnabled() { return true; }
    }
    public class BaseResourceProcessor
    {
        public const string ProcessorNodeRemainingTime = "remainingTime"; public string name; public double secondsPerCycle = 100; protected double remainingTime;
        protected readonly Dictionary<Guid, double> vesselRemainingTimes = new Dictionary<Guid, double>();
        protected double AddVesselElapsedTime(Vessel vessel, double elapsed)
        {
            double time;
            if (!vesselRemainingTimes.TryGetValue(vessel.id, out time)) time = remainingTime;
            time += elapsed; vesselRemainingTimes[vessel.id] = time; return time;
        }
        public List<BaseOutcome> outcomes = new List<BaseOutcome>(); public Dictionary<string, SnacksProcessorResult> productionResults = new Dictionary<string, SnacksProcessorResult>();
        public virtual void Initialize() { } public virtual void Destroy() { }
        public virtual void OnLoad(ConfigNode node)
        {
            remainingTime = 0; if (node.HasValue(ProcessorNodeRemainingTime)) double.TryParse(node.GetValue(ProcessorNodeRemainingTime), out remainingTime);
            vesselRemainingTimes.Clear();
            foreach (ConfigNode timer in node.GetNodes("VESSEL_TIMER")) vesselRemainingTimes[Guid.Parse(timer.GetValue("vesselID"))] = double.Parse(timer.GetValue(ProcessorNodeRemainingTime));
        }
        public virtual ConfigNode OnSave()
        {
            ConfigNode node = new ConfigNode("SNACKS_RESOURCE_PROCESSOR"); node.AddValue(ProcessorNodeRemainingTime, remainingTime);
            foreach (KeyValuePair<Guid, double> timer in vesselRemainingTimes)
            {
                ConfigNode saved = new ConfigNode("VESSEL_TIMER"); saved.AddValue("vesselID", timer.Key); saved.AddValue(ProcessorNodeRemainingTime, timer.Value); node.AddNode(saved);
            }
            return node;
        }
        public virtual void GetRosterResourceEstimatesForEditor(int count, int capacity, StringBuilder results, ShipConstruct ship) { }
        public virtual void onKerbalBoardedVessel(ProtoCrewMember crew, Part part) { } public virtual void ProcessResources(Vessel vessel, double elapsed, int count, int capacity) { }
        public virtual void onVesselLoaded(Vessel vessel) { } public virtual void onVesselGoOffRails(Vessel vessel) { }
        protected void applyFailureOutcomes(Vessel vessel, SnacksProcessorResult result) { foreach (BaseOutcome outcome in outcomes) outcome.ApplyOutcome(vessel, result); }
        protected void removeFailureOutcomes(Vessel vessel, bool inform = true) { foreach (BaseOutcome outcome in outcomes) outcome.RemoveOutcome(vessel, inform); }
    }
}
