# Snacks open pull request review

Reviewed against the current working tree on 2026-10-01, including the prior Stress, background processing, exemptions, and per-Kerbal portions fixes. Config edits are confined to `C:\KSPDev\GameData\WildBlueIndustries\Snacks`. Primary project GameData files are unchanged. No Git commits, pushes, or PR merges were made.

Closed all seven PRs (#1, #5, #7, #8, #9, #10, and #11) without merging, using the signed-in maintainer's GitHub CLI after the connector refused writes. The user approved retaining solar-day processing for #5; its display fixes and solar-day naps are integrated and validated. Integrated work remains local and has not been published upstream.

| PR | Assessment | Integration |
| --- | --- | --- |
| [#1 Tank paths](https://github.com/Angel-125/Snacks/pull/1) | Already resolved. | Current tin configs use `WildBlueIndustries/Snacks/Parts/Payload`, and the referenced models exist. No additional path edits. |
| [#5 Nonstandard calendars](https://github.com/Angel-125/Snacks/pull/5) | Display fixes adapted; calendar-based processing declined after user review. The original nap patch has a seconds/minutes bug. | Dynamic formatter units for elapsed-duration displays; preserve minutes/seconds in sub-hour estimates. Meals, daily Stress, daily recovery, and one-day naps use the homeworld solar day. Nap penalties receive exact seconds while the existing minutes API is preserved. Daily-rate labels explicitly say homeworld day, and converter daily tooltip values use solar-day units. |
| [#7 Version URL](https://github.com/Angel-125/Snacks/pull/7) | Still valid. | Corrected the authoritative version URL to the raw GitHub file under `GameData/WildBlueIndustries/Snacks`. Verified that repository file exists. |
| [#8 Simulator CPU](https://github.com/Angel-125/Snacks/pull/8) | Still valid. | Sleep 20 ms when idle without delaying active cycles. Track background workers, preserve them when cancelling estimates, and stop/join them when the scenario is destroyed. |
| [#9 MM refactor](https://github.com/Angel-125/Snacks/pull/9) | Contains a valid Air syntax/label fix, plus unreviewed balance and coverage changes. | Corrected the misplaced MK1 Air scrubber closing brace and `Scribber` typo. Left rates, capacities, storage, affected parts, and BARIS values unchanged. Optional Air stays `.txt`. |
| [#10 Inventory/construction](https://github.com/Angel-125/Snacks/pull/10) | Still valid for stock KSP 1.12 cargo support. | Added `ModuleCargoPart` to the radial tin (50 L, quantity 1) and S-500 (920 L). Inventory storage depends on available inventory volume; 920 L is not construction-only. Resource quantities, mass, cost, and crew balance are unchanged. |
| [#11 Localization](https://github.com/Angel-125/Snacks/pull/11) | Useful translation, but unsuitable for direct merging. | Adapted English/Chinese catalogs and UI, part presentation, converter messages, resource options, and condition displays. Added namespaced tags, English fallbacks, consistent numeric templates, and updated exemption/portion help. Preserve deferred actual penalty totals. Credit Chinese contribution to akira17845. Excluded build artifacts, machine-specific project settings, optional-feature activation, and condition/resource/option identifier changes. |

## Changes requiring review before integration

PR #9 proposes adding recyclers and Air scrubbers to more parts, changing Soil storage from 200 total to 200 per seat, and raising BARIS quality from 65 to 75. On a four-seat cabin, its Soil change raises capacity from 200 to 800. The current installed stock patch no longer has those BARIS blocks. None of these balance changes were applied.

PR #11 proposes translating persisted/lookup condition identifiers, including `Stressed Out`, `Relaxing`, and `Sick`, while other skill-loss rules still refer to the English names. This can break recovery and save compatibility. These identifiers remain canonical; only their presentation is translated. No migration is proposed without explicit review.

The user reviewed #5 and chose the homeworld solar day (sunrise to the next sunrise) for processing. Calendar-based meal scheduling is not integrated. Calendar units are used only to express elapsed durations. One-day naps follow the same solar-day definition as meals and Stress. Fixed-duration minute/hour naps retain their physical-second durations.

## Validation and limits

- Release build passed with the post-build event disabled. The existing unused-variable warning in `SnacksPartResource` remains.
- Stress regression passed, including KSC recovery, overlapping conditions, identity, exemptions, save/load, and vessel timers.
- Background regression passed, including the prior processor/converter checks and new worker idle sleep, cancellation/reuse, queued-job processing, tracking, and shutdown tests.
- Utility regression passed for changing/custom/invalid calendar units, localization fallback and formatting, and presentation-only condition translation.
- Compiled regression passed with a solar day different from the calendar day: one-day naps retain exact solar-day seconds, processing retains the solar day, elapsed durations use the calendar day, and sub-hour displays retain minutes and seconds.
- Config validation passed for matching catalogs and numeric placeholders, referenced keys, balanced Air/stock/tin config nodes, existing model paths, cargo modules, preserved recycler balance, and unchanged Stress identifiers.
- Live stock inventory/construction, Chinese UI layout, Unity destruction events, and calendar-mod integration still require in-game verification. This request does not deploy a DLL or publish the local changes upstream.
