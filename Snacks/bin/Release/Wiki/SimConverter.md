            
This class is a simulated ModuleResourceConverter. It processes inputs, produces outputs, and when the time's up, generates yield resources. For the sake of simplicity, all vessel resources are available.
        
## Methods


### ProcessResources(System.Collections.Generic.Dictionary{System.String,Snacks.SimResource},System.Double)
Processes resources, consuming inputs, producing outputs, and when time expires, producing yield resources. For the purposes of simulation, we assume dumpExcess = true, yield resources always suceed, no heat generation, and no crew bonuses.
> #### Parameters
> **resources:** The map of vessel resources to process.

> **secondsPerSimulatorCycle:** The number of seconds per simulator cycle.


