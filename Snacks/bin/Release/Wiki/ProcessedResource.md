            
This class represents resources consumed or produced by a SnacksResourceProcessor. Consumption and production is applied vessel-wide, or to individual kerbal roster entries depending on the configuration. If applied vessel-wide, the resource can be produced or consumed per kerbal. Finally, the resource can be displayed in the Snapshots view.
        
## Fields

### resourceName
Name of the consumed/produced resource
### dependencyResourceName
Name of the dependency resource if the resource to process depends upon the results of another resource's process result. E.G. 1 unit of Soil is produced for 1 unt of Snacks consumed.
### isRosterResource
Flag to indicate whether or not the resource is applied to roster entries instead of parts and vessels. if set to true, then appliedPerCrew is ignored. Default: false
### showInSnapshot
Flag to indicate whether or not to show the resource in the Snapshots window. Ignored if isRosterResource is set to true. Default: true
### failureResultAppliesOutcomes
Flag to indicate whether or not a failure result applies the processor's outcomes. Default: true
### amount
The amount of resource to consume or produce. If appliedPerCrew is true, then the amount consumed/produced is multiplied by the number of crew aboard the vessel. If isRosterResource is true, then each individual crew member's roster entry will be affected instead. Default: 0
### clearDataDuringRecovery
Flag to indicate that astronaut data should be cleared when a vessel is recovered. Default: true
## Methods


### Load(ConfigNode)
Loads the fields from the config node.
> #### Parameters
> **node:** A ConfigNode containing fields to load.


### Save
Saves current values to a ConfigNode.
> #### Return value
> A ConfigNode containing the field data.

### ConsumeResource(Vessel,System.Double,System.Int32,System.Int32)
Consumes the resource.
> #### Parameters
> **vessel:** The vessel to work on

> **elapsedTime:** Elapsed seconds

> **crewCount:** Current crew count

> **crewCapacity:** Current crew capacity

> #### Return value
> A SnacksConsumerResult containing the resuls of the consumption.

### ProduceResource(Vessel,System.Double,System.Int32,System.Int32,System.Collections.Generic.Dictionary{System.String,Snacks.SnacksProcessorResult})
Produces the resource
> #### Parameters
> **vessel:** The vessel to work on

> **elapsedTime:** Elapsed seconds

> **crewCount:** Current crew count

> **crewCapacity:** Current crew capacity

> **consumptionResults:** Results of resource consumption.

> #### Return value
> A SnacksConsumerResult containing the resuls of the production.

