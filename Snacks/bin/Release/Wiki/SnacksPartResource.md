            
When a part with crew capacity is loaded in the editor and it lacks this resource, or when a vessel is loaded into the scene and its parts with crew capacity lack this resource, add it to the part. Doesn’t apply to kerbals going on EVA. Use SNACKS_EVA_RESOURCE for that. Use the SNACKS_PART_RESOURCE to define resources to add.
        
## Fields

### resourceName
Name of the resource
### amount
Amount to add
### maxAmount
Max amount possible
### capacityAffectingModules
Parts with at least one of the modules on this list affect the part's capacity to store the resource (their equipment takes up additional space, for instance).
### capacityMultiplier
If a part has at least one part module on the capacityAffectingModules list then multiply resource amount and max amount by this multiplier. Default is 1.0
### isPerKerbal
If true (which is the default), then amount and maxAmount added are multiplied by the part's crew capacity.
## Methods


### LoadPartResources
Loads the SNACKS_PART_RESOURCE config nodes, if any, and returns SnacksPartResource objects.
> #### Return value
> A list of SnacksPartResource objects.

### addResourcesIfNeeded(Part)
If the part with crew capacity doesn't have the resource, then add it.
> #### Parameters
> **part:** 


### addResourcesIfNeeded(Vessel)
If the loaded vessel's parts with crew capacity don't have the resource, then load it.
> #### Parameters
> **vessel:** 


