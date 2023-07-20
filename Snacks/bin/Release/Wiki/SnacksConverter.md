            
An enhanced version of ModuleResourceConverter, the SnacksConverter offers a number of enhancements including producing resources after a set number of hours have elapsed (defined by YIELD_RESOURCES nodes), the ability to produce the yield resources based on the result of a random number generation, an optional flag that results in the part exploding as a result of a critical failure roll, an optional flag that can prevent the converter from being shut off, the ability to play effects, and the ability to be run in the background (when the vessel isn't loaded into the scene).
        
## Fields

### startEffect
Name of the effect to play when the converter starts.
### stopEffect
Name of the effect to play when the converter stops.
### runningEffect
Name of the effect to play while the converter is running.
### minimumVesselPercentEC
This is a threshold value to ensure that the converter will shut off if the vessel's ElectricCharge falls below the specified percentage. It is ignored if the converter doesn't use ElectricCharge.
### requiresHomeConnection
This flag tells the converter to check for a connection to the homeworld if set to true. If no connection is present, then the converter operations are suspended. It requires CommNet to be enabled.
### minimumCrew
This field specifies the minimum number of crew required to operate the converter. If the part lacks the minimum required crew, then operations are suspended.
### conditionSummary
This field specifies the condition summary to set when a kerbal enters the part and the converter is running. For example, the kerbal could be Relaxing. The condition summary appears in the kerbal's condition summary display. Certain conditions will result a loss of skills for the duration that the converter is running. For that to happen, be sure to define a SKILL_LOSS_CONDITION config node with the name of the condition.
### canBeShutdown
This field indicates whether or not the converter can be shut down. If set to false, then the converter will remove the shutdown and toggle actions and disable the shutdown button.
### ID
Unique ID of the converter. Used to identify it during background processing.
### dieRollMin
Minimum die roll
### dieRollMax
Maximum die roll
### minimumSuccess
On a roll of dieRollMin - dieRollMax, the minimum roll required to declare a successful resource yield. Set to 0 if you don't want to roll for success.
### criticalSuccess
On a roll of dieRollMin - dieRollMax, minimum roll for a resource yield to be declared a critical success.
### criticalFail
On a roll of dieRollMin - dieRollMax, the maximum roll for a resource yield to be declared a critical failure.
### hoursPerCycle
How many hours to wait before producing resources defined by YIELD_RESOURCE nodes.
### cycleStartTime
The time at which we started a new resource production cycle.
### progress
Current progress of the production cycle
### timeRemainingDisplay
Display field to show time remaining on the production cycle.
### lastAttempt
Results of the last production cycle attempt.
### criticalSuccessMultiplier
If the yield check is a critical success, multiply the units produced by this number. Default is 1.0.
### failureMultiplier
If the yield check is a failure, multiply the units produced by this number. Default is 1.0.
### explodeUponCriticalFail
Flag to indicate whether or not the part explodes if the yield roll critically fails.
### elapsedTime
The amount of time that has passed since the converter was last checked if it should produce yield resources.
### secondsPerCycle
The number of seconds per yield cycle.
### yieldsList
The list of resources to produce after the elapsedTime matches the secondsPerCycle.
### rosterInputList
Similar to an input list, this list contains the roster resources to consume during the converter's processing.
### rosterOutputList
Similar to an output list, this list contains the roster resources to produce during the converter's processing.
### missingResources
The converter is missing resources. If set to true then the converter's operations are suspended.
### crewEfficiencyBonus
The efficieny bonus of the crew.
## Methods


### PerformAnalysis
Performs the analysis roll to determine how many yield resources to produce. The roll must meet or exceed the minimumSuccess required in order to produce a nominal yield (the amount specified in a YIELD_RESOURCE's Ratio entry). If the roll fails, then a lower than normal yield is produced. If the roll exceeds the criticalSuccess number, then a higher than normal yield is produced. If the roll falls below the criticalFailure number, then no yield is produced, and the part will explode if the explodeUponCriticalFailure flag is set.

### CalculateProgress
Calculates and updates the progress of the yield production cycle.

### RemoveConditionIfNeeded
Removes the summaryCondition from all kerbals in the part if they have it set.

