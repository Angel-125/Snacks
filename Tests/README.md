Run `powershell -ExecutionPolicy Bypass -File Tests\Run-StressRegression.ps1` from the repository root on Windows with .NET Framework installed.

The harness compiles the production `StressProcessor`, `OnStrikePenalty`, and `SnacksRosterResource` source against deterministic KSP/Unity substitutes. It checks independent daily timers for loaded and unloaded vessels, timer persistence and legacy saves, recovery with overlapping conditions, crew boarding, multiple-day catch-up, restored resource identity, and updates without a vessel at KSC. It does not verify KSP's actual experience effects, Unity lifecycle, or background converter execution.

Build output goes to `Snacks/obj/StressValidation`. No game files or configs are changed.
