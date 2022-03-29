Snacks!

Original mod by: Troy Gruetzmacher (tgruetzm)
Continuation by: Angel-125

Snacks was originally published by Troy Gruetzmacher (tgruetzm) in August of 2014. It offered a novel and lightweight solution to life support for those that didn't want the complexity of more sophisticated mods like TAC Life Support. Two years later, the game advanced and while the original author moved on, it was time to give Snacks an update.

Features:
- Friendly, lightweight life support system
- Highly configurable to support your play style
- Optional consequences that won't brick your mission

You can configure things like snacks consumed per meal, meals per day, enable/disable recycling, recycling efficiency, and various penalties for hungry kerbals including reputation loss, fines, and partial loss of vehicle control. You can even enable/disable random snacking if desired. Just like with the stock CommNet, the penalties won't brick your mission. All of these options are found in the Game Difficulty screen. And if you're new to Snacks, please consult the KSPedia.

LICENSE
Source code: The MIT License (MIT)
Snack Tin artwork by SQUAD/Porkjet: CC-BY-NC 3.0
Portions of this codebase are CC-BY-NC 3.0 and derived from Taranis Elsu's Window class.

INSTALLATION
Delete any previous instances in GameData/Snacks
Copy the files in the zip folder over to GameData/Snacks

REVISION HISTORY

1.27.4
- Minor Sandcastle compatibility fix.

1.27.3
- Recompiled for KSP 1.12.2.
- Fixed issue where Snacks events weren't being processed correctly.

1.27.2
- Added new SKILL_LOSS_CONDITION named Gardening to the list of conditions that will cause skill loss when a part with an active SnacksConverter is running, it lists Gardening as one of its skill loss conditions, and a kerbal resides inside the part. For example, a kerbal can be working in a greenhouse.
Hint: The greenhouse could also reduce a kerbal's Stress while it is running.
- Fixed issue where too many Snacks were being added to kerbals sitting in external command seats.
- Fixed issue where kerbals sitting in external command seats upon vessel launch weren't receiving EVA resources like FreshAir.
- Fixed NRE generated while opening the Snacks window when a vessel has kerbals in external command seats.
- Fixed issue where crew capacity was incorrectly calculated when kerbals sat in external command seats. Yay for circumventing a KSP bug!

1.27.1
- Fixed issue where Snacks weren't being added to crewed parts.

1.27

- Added the ability to store the SOCS Oxium Candle as a cargo part.
- SNACKS_PART_RESOURCE now supports unitsPerDay and daysLifeSupport. They specify the number of units per day that is consumed (per kerbal), and the number of days of life support to add to the part, respectively. If they're specified, then amount and maxAmount are ignored.
- Fixed missing resource infos in the editor's part display window.

1.26.1

- Fixed issue where the PAW would flicker whenever adjusting settings on another part.
- Fixed issue where players could not copy parts with Snacks resources in them.

1.26.0

Changes

- If a kerbal on EVA consumes a resource and it dips below minimum safe levels, then the player will receive a warning message.
- In Debug mode, the Snack Time button will become available in the Snacks flight app window.
- Fixed Snacks App Window not updating resource values properly after snack time.
- Fixed NRE generated when opening the Snacks window and there is a kerbal on EVA.
- Fixed issue where EVA resources were consumed when the jetpack is used. NOTE: Use the in-flight Snacks window to see the EVA kerbal's resources, they're no longer
visible in the Resources window due to the stock EVA resource consumption bug.
- Fixed crash issue that occurs when placing parts with crew capacity in symmetry.
- Fixed issue in Snacks App Window where kerbals on EVA would display their crew count.

1.25.2
- Fix Stresstimator showing up in Space Center window when it shouldn't.
- Fix corrected density of Hydrazine.

1.25.1
- Fix SnacksConverter not restoring skills when the converter is activated and a kerbal transfers to another part.
- You can now estimate a vessel's max Stress capacity in the VAB/SPH. Requires Stress to be enabled (rename the Stress.txt file in LifeSupportResources folder to Stress.cfg).

New Feature
- Stresstimator: If you have Stress enabled, then you'll get a new button in the in-flight Snacks window to open the Stresstimator. This window helps you estimate the max Stress that your crew can take based on the crewable parts that you select. Since kerbals can get Stressed Out when moving from vessels with a lot of available crew capacity to vessels without much available crew capacity, the Stresstimator helps you avoid kerbals getting Stressed Out if they have accumulated Stress, you move them to a docked vessel, and then undock the vessel. Given the game design, it's very difficult to do the Stress estimate automagically, so the Stresstimator is better than nothing...

1.25.0
- Fix background converters not respecting locked resources for unloaded vessels.
- Fix resource processors not respecting locked resources for unloaded vessels.
- Fix condition summary not showing Stressed Out condition.
- Fix Stress not recalculated when a vessel docks or undocks.
- Fix SOCS Cannister explosion check not being checked.
- Fix SOCS Cannister automatically shuts down when it runs out of SOCS Fuel.
- Fix SOCS Cannister user messages are more appropriate now.
- If kerbals get Stressed Out they might start stress eating. If you run out of Snacks they'll get even more Stressed Out.
- Kerbals wait to get Stressed Out before they start consuming "Hydrazine."
- You can now disable email notifications of converters running out of resources or storage space via the Snacks Settings menu.
- Background processors no longer consume ElectricCharge for simplicity; it's either that or bog the game down with finding and running power generators.
- You can now customize the SnacksConverter's criticalSuccessMessage, successMessage, failMessage, and criticalFailMessage displayed during yielded resource checks.

1.24.5
- Bug fixes

1.24.4
- Snacks (and other life support resources) will now be added to vessels that are loaded into the VAB/SPH.

1.24.3
- Compatibility update

1.24.1
- Updated for KSP 1.8

1.23.2
- Reduced timewarp ElectricCharge cap to 3x.
- New game settings: you can turn on/off ElectricCharge consumption for Snacks converters during background processing. Default is ON.
 
1.23.1
- Fixed issue with kerbals not suffering any penalties when a ship processed in the background has locked snack tins.
- Experienced kerbals can now process inputs and outputs without affecting ElectricCharge consumption.

1.23.0
- You can now disable Snacks/Soil resource processing if desired. Just rename Snacks.cfg to Snacks.txt in the LifeSupportResources folder.

1.22.2
- Fixed flow mode issues for ElectricCharge consumption.

1.22.1
- Removed test code from the simulator.

1.22
- Added support for Dynamic Batteries.
- Added ability to interrupt the resource consumption simulator. NOTE: duration estimates will be unavailable.
- Fix Inability to view vessels not on or around the current world.
- Fix Missing roster resource names when added to the kerbal.

1.21
- CheckResources can now check resource percent levels.
- Fix Simulator window not running simulations after closing and reopening the window in the same scene.
- Fix NRE generated by FaintPenalty.
- Fix estimated time remaining display showing 1 hour even when out of resources.
- Fix Snacks converters and their derivatives give players a break on ElectricCharge consumption past 100x timewarp.
- Fix background converters honor infinite electricity and infinite propellant debug settings.
- Fix "Hydrazine" Vodka display name.

1.20.3
- Fix snack tin symmetry issues.
- Fix NREs when changing resources.

1.20.1
- Small update to support the new Hydrazine tutorial.

1.20 Air and Stress

When I started reworking Snacks to add in the penalty system, I tried to follow the same design philosophies that Squad did when making KerbNet: make it a challenge but don't brick your game or save. I've kept that philosophy and stuck to the original concept as a lightweight life support system as I've made improvements over the years. This update is the collmination of weeks of work that keeps the simplistic life support out of the box but opens the doors to so much more. All it needs is a bit of legwork on your part, but there are plenty of examples.

I'm happy to say that Snacks is feature complete!

Custom Life Support Resources: Snacks now has the ability to define custom life support resources besides just Snacks! All it takes is a config file. With this feature you can:
  - Define your own life support resource to consume and/or produce.
  - Optionally track its status in the vessel snapshot window- with support in the multithreaded simulator!
  - Optionally apply one or more outcomes (like penalties) if the consumed resource runs out, or if space for the produced resource is full.

As an example of a tracked resource, check out the LifeSupportResources folder for the FreshAir.txt file. Rename it with the .cfg extension to enable it.
NOTE: You can make FreshAir using the stock ISRU and mini ISRU, and all the stock crew cabins have Air Scrubber converters to turn StaleAir into FreshAir.

New Part: SOCS! Similar to the real-world Solid Fuel Oxygen Generator, the Solid Oxium Candle System burns a solid fuel to produce Fresh Air. Once started it can't be stopped and it might explode... It's available at the Survivability tech tree node, and only available if you enable Air.

Roster Resource: Roster resources can now be defined via the SNACKS_ROSTER_RESOURCE config node. A roster resource is a characteristic of a kerbal as opposed to a resource stored in a part. Better yet, the SnacksConverter can work with roster resources- with background processing!

New penalty: The OnStrikePenalty removes a kerbal's skill set without turning him or her into a tourist. That way, should you uninstall the mod for some reason, you won't brick your mission or game.

Stress: You now have an optional resource to keep track of: Stress! Stress reflects the difficulties of living in confined spaces for extended periods of time. The more space available, reflected in a vessel's crew capacity, the longer a kerbal can live and work without getting stressed out. Events like a lack of food and FreshAir, arguments with other crew members, and low gravity can also cause Stress. And when a kerbal gets stressed out, they'll stop using their skills. You can reduce Stress by letting the kerbal hang out in the Cupola, but you won't gain use of their skills while they're relaxing. To enable Stress, just rename the LifeSupportResources/Stress.txt with the .cfg extension and restart your game. If you already have Kerbal Health then you won't need Stress, but it can serve as an example for how to use roster resources.

And if you have BARIS installed, part failures and staging failures will cause Stress!

Events: Random snacking is now reworked into an event system to add flavor to the game. Kerbals might get the munchies, or eat bad food. Maybe a crew member snores and it causes Stress. With a host of preconditions and outcomes, you can make a number of different and entertaining events. Check out the Events.cfg file in the Snacks folder for a couple of examples.

Wiki: updates describe all the preconditions, outcomes and events along with other API objects.

1.16.4
- Fix for kerbals taking too many snacks with them when going on EVA.

1.16.3
- Fixed issue where Snacks was being consumed by occupants of vessel that's part of a rescue contract. NOTE: Once the craft is loaded into the scene, they will be tracked by Snacks.

1.16.2
- Fixed issue where parts weren't receiving their correct allotment of Snacks for parts where the crew capacity is greater than one kerbal.

1.16.1
- Updated Snacks Trip Planner in the Docs folder - new Timewarp Calcs tab.
- SnacksConverter now has new options:
  requiresHomeConnection - Requires connection to homeworld to operate.
  minimumCrew - Minimum number of crew that must be in the host part in order to run the converter.
NOTE: Snacks Processor and Soil Recycler don't use these.
- Fixed NREs generated when converters are added in the editor.
- Fixed issue where Snacks wasn't being added to parts in the editor.
- Fixed debug info in SnacksProcessor.
- Fixed issue where Snack supply window wasn't being updated when the window is opened and snack time happens.

1.16
- Added new celestial bodies filter to the snacks supply window.

- Snacks will now run simulations on a vessel's supplies and converters to estimate how long the vessel's snacks will last.
NOTE: For the simulator to work properly, be sure to visit all your in-flight vessels that have crews aboard after installing this update.
NOTE: The simulator cannot simulate drill operations.

- Made some improvements to background converter processing. As a bonus, power production & consumption are also run in the background- with Kopernicus support for solar arrays.

- SnacksConverter now lists the yield resources in the part info window, and shows yield production time remaining in the PAW.

- Snacks and other resources consumed per day are now calculated based on the solar day length of the homeworld instead of set to the stock 6hrs/24hrs. I'm looking at you, JNSQ...

1.15
- Fixed issue where vessels spawned in game for rescue contracts lacked Snacks.
- Fixed integration issue with WalkAbout.
- The Soil Recycler now uses the Converter Skill from Engineers instead of the Science Skill. Yup, Scientists make Snacks from rocks and (sanitation) Engineers recycle Soil into Snacks.
- The converter and recycler won't automatically shut down if they lack an input resource or an output resource is full. Instead they'll wait until they get what they need.
- Updated the recycler/processor info view in the editor's part description window.

1.14
- Fixed restock whitelist

1.13
- Fixed recycler and processor efficiency calculations.
- Other bug fixes.

1.12
- Updated for KSP 1.7
- Bug fixes

1.11.5
- Fixed empty mass for the radial snack tin to be in line with similar parts.
- Fixed snack resources being locked by default when Snacks are added to in-flight vessels.
- Fixed efficiency processing; processors and recyclers output efficiency should now be 10% to 100% efficient based on their efficiency setting.
- Fixed issue where converters and processors wouldn't run in the background without at least one Kerbal aboard.
- Fixed issue where the Snacks Processor and Soil Recycler wouldn't produce the proper amount of resources while the vessel is in physics range.
- Reduced the amount of Snacks per day produced by the stock Mobile Processing Lab. It was a bit OP...
- Snack tins now tell you what their resource options are in the part info view.
- Added new SnacksConverter part module. It serves as the basis for the existing Snacks Processor and Soil Recycler. It can also produce "YIELD_RESOURCE" units over time just like the greenhouse from Wild Blue Tools. You can even assign effects to the converter!
NOTE: Snacks won't be getting a greenhouse of its own; it's intended to be a lightweight life support system, but I recognize that some players want more sophisticated capabilities. So the tools are there for others to expand upon...

1.11.1
- Recompiled for KSP 1.6

1.11.0
- Updated for KSP 1.5.X
- The Snack Processor and Soil Recycler will now automatically shut down if the vessel's ElectricCharge reserves drop below 5%.

1.10.0
- Fixed NRE causing the Settings menu to not appear.
- Kerbals can now die from a lack of Snacks! This penalty is trned OFF by default, and you can change the number of skipped meals before a kerbal dies in the settings menu. Kerbals listed as exempt will never starve to death.

1.9.0
- Recompiled for KSP 1.4.1

1.8.7
- Fixed NRE and production issues with the SnackProcessor.

1.8.6
- Snack consumption now honors resource locks.
- Retextured radial snack tin - Thanks JadeOfMaar! :)
- Removed unneeded catch-all - Thanks JadeOfMaar! :)
- Fixed bulkhead profiles and tags on inline snack tins - Thanks JadeOfMaar! :)
- Add parts to CCK LS category - Thanks JadeOfMaar! :)

1.8.5
- Fixed background processing of snacks and soil issues with WBI mods (Pathfinder, Buffalo, etc.).
NOTE: Be sure to visit your spacecraft at least once to ensure that the changes take effect.
- Updated to KSP 1.3.1.

1.8.0
- Time estimates are now measured in years and days; months, though accurate, was getting too confusing.
- Snack processors and soil recyclers now run in the background when vessels aren't loaded.

1.7.0

- Adjusted snack production rates for the snack grinder (found on the Hitchhiker).
- Added hooks for Snacks Plus. You can download Snacks Plus from: 
- Revised the Snacks Trip Planner spreadsheet.

1.6.5
- Added a radial snack tin. It holds 150 snacks, 150 Soil, or 75 Snacks and 75 Soil.

1.6
- Plugin renamed to SnacksUtils to alleviate issues with ModuleManager.
- When kerbals faint due to lack of snacks, you now choose from 1 minute, 5 minutes, 10 minutes, 30 minutes, an hour, 2 hours, or a day.
- Snacks now supports 24-hour days in addition to 6-hour days. Snack frequency is calculated accordingly.

1.5.7
- Fixed snacks calculations and minor GUI update. Thanks for the patch, bounty123! :)

1.5.6
- Fixed NRE's that happen in the editor (VAB/SPH)
- Snacking frequency is correctly calculated now.
- Updated to KSP 1.2.1
- Added recyclers to the Mk3 shuttle cockpit and the Mk2 crew cabin.

1.5.5
- Fixed some NREs.
- Fixed a situation where the ModuleManager patch wasn't adding snacks to crewed parts; Snacks can now dynamically add them when adding parts to vessels in the VAB/SPH.

1.5.3
- When kerbals go EVA, they take one day's worth of snacks with them.
- More code cleanup.
- Bug Fixes

1.5.1
- Temporarily disable the partial vessel control penalty.
- Added additional checks for vessels created through rescue contracts; any crew listed as "Unowned" will be ignored.

1.5.0
- ISnacksPenalty now has a RemovePenalty method. Snacks will call this each time kerbals don't miss any meals.
- ISnacksPenalty now has a GameSettingsApplied method. This is called at startup and when the player changes game settings.
- The partial control loss penalty should work now.
- New penalty: kerbals can pass out if they miss too many meals.
- Updated the KSPedia to improve clarity and to add the new penalty option.

New events
onBeforeSnackTime: Called before snacking begins.
onSnackTime: Called after snacking.
onSnackTick: called during fixed update right after updating the vessel snapshot.
onConsumeSnacks: Called right after calculating snack consumption but before applying any penalties. Gives you to the ability to alter the snack consumption.
onKerbalsMissedMeal: Called when a vessel with kerbals have missed a meal.

1.4.5
- Fixed an issue with snack tins not showing up.
- A single kerbal can now consume up to 12 snacks per meal and up to 12 snacks per day.
- By default, a single kerbal consumes 1 snack per meal and 3 meals per day.
- Reduced Soil storage in the Hitchhiker to 200. This only applies to new vessels.
- Reduced Snacks per crew capacity in non-command pods to 200 per crewmember. This only applies to new vessels.
- Added the SnacksForExistingSaves.cfg file to specify number of Snacks per command pod and snacks per non-command pod. These are used when installing Snacks into existing saves for vessels already in flight.
- Added new ISnacksPenalty interface for mods to use when implementing new penalties. One of the options is to always apply the penalty even with random penalties turned off. Of course the implementation can decide to honor random penalties...
- Added a Snacks Trip Planner Excel spreadsheet. You'll find it in the Docs folder. An in-game planner is in the works.

1.4.0

- Adjusted Snack production in the MPL; it was way too high. Ore -> Snacks is now 1:10 with mass conservation. A 1.25m Small Holding Tank (holds 300 Ore) now produce 3,000 Snacks.
- Added display field to Snack Processor that tells you how the max amount of snacks per day that it can produce.
- Moved Snack Tins to the Payload tab.
- Added option to show time remaining in days.
- When kerbals go hungry, added the option to randomly choose one penalty from the enabled penalties, or to apply all enabled penalties.
- Added lab data/experiment data loss as an optional penalty.
- You can now register/unregister your own custom penalties. This is particularly useful for addons to Snacks.
- Cleaned up some KSPedia issues.
- Fixed an issue with adding Snacks to existing saves.
- Fixed an issue with vessels spawned from rescue contracts incuring penalties due to being out of Snacks.

1.3.0

- Snacks now have mass and volume. One unit of Snacks takes up 1 liter and masses 0.001 metric tons. 
- Adjusted the MPL's Snack Processor's Ore to Snacks output to account for mass.
- The Snack Processor's efficiency can be improved by those with the Science skill.
- Added several configurable options to KSP's Game Difficulty screen. 
- If recycling is enabled, then kerbals produce Soil when consuming Snacks. Soil is a 1-liter resource that masses 0.001 metric tons. Apparently Soil was part of tgruetzm's original design...
- If recycling is enabled, then the Hitchhiker can convert Soil into Snacks. You can configure recycling efficiency in the Game Difficulty screen.
- New consequences for missing meals:
  * Pay fines per kerbal
  * Lose partial control of the vessel
- Added three sizes of Snack Tins. They can be switched between storing Snacks, Soil, or both. Models and textures courtesy of SQUAD/Porkjet.
- Added KSPedia entries for Snacks!

1.2.0
- Pre-release for KSP 1.2 pre-release.

1.1.6
- Minor updates to the MM patch to help with customization

1.1.5
- Updated to KSP 1.1.2

1.1.4
- Fixed an issue where snacks weren't provided to non-command crewed parts.
- Rebalananced Snack amounts for non-command modules to 400 per crewmember.
NOTE: This will only apply to new vessels. For existing vessles, temporarily rename patch.cfg to patch.txt,
and rename rebalance.txt to rebalance.cfg. Start your game, load your vessels, and then exit the game and rename
the files back to rebalance.txt and patch.cfg.

1.1.3
- Updated to KSP 1.1.1
- Fixed name in versioning file

1.1.2
- Fixed NREs
- Cleaned up the Module Manager patch. Thanks for the hints, Badsector! :)

1.1.1
- Re-added missing Snack Grinder
- Module Manager patch fixed to add Snacks to parts with up to 16 crewmembers
- Snacks won't be added to parts that already have Snacks
- Added MiniAVC support

1.1
- Updated for KSP 1.1
- Removed the need for the ModuleManager patch to equip crewed pods with Snacks.
