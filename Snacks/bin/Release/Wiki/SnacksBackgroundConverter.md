            
This class runs active converters in the background, consuming inputs, producing outputs, and yielding resources.
        
## Fields

### ConverterName
Name of the converter
### moduleName
Name of the converter part module.
### IsActivated
Flag indicating that the converter is active.
### isMissingResources
Flag indicating that the converter is missing resources.
### isContainerFull
Flag indicating that the container is full.
### inputEfficiency
The input efficiency.
### outputEfficiency
The output efficiency.
## Methods


### GetBackgroundConverters
Parses a vessel to find active converters to run in the background.
> #### Return value
> A map keyed by Vessel that has a list of running converters to run in the background.

### Constructor
Initializes a new instance of the class.
> #### Parameters
> **protoPart:** The ProtPartSnapshot that hosts the converter.

> **protoModule:** The ProtoPartModuleSnapshot representing the converter.

> **moduleIndex:** The module index.


### Constructor
Initializes a new instance of the class.

### CheckRequiredResources(ProtoVessel,System.Double)
Checks to be sure the vessel has the required resources.
> #### Parameters
> **vessel:** The Vessel to check.

> **elapsedTime:** The seconds that have elapsed.


### ConsumeInputResources(ProtoVessel,System.Double)
Consumes the input resources.
> #### Parameters
> **vessel:** The Vessel to consume resources from.

> **elapsedTime:** Converter elapsed time.


### ProduceOutputResources(ProtoVessel,System.Double)
Produces the output resources.
> #### Parameters
> **vessel:** The Vessel to add resources to.

> **elapsedTime:** Converter elapsed time.


### ProduceyieldsList(ProtoVessel)
Produces the yield resources
> #### Parameters
> **vessel:** The Vessel to add resources to.


### PrepareToProcess(ProtoVessel)
Prepares the converter to process.
> #### Parameters
> **vessel:** The Vessel to check for preparations.


### PostProcess(ProtoVessel)
Handles post process tasks for the converter.
> #### Parameters
> **vessel:** The Vessel to update.


