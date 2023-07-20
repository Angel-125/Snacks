            
This class determines how long consumed resources like Snacks will last by simulating resource consumption and simulating running converters like soil recyclers and snacks processors. It is designed to allow for an arbitrary number of resource production chains and an arbitrary number of consumed resources. Conditions: The only inputs allowed into the system are those consumed by kerbals. Ex: kerbals eat Snacks and produce Soil. Resources aboard the vessel that aren't directly involved in resource consumption are fixed. Ex: Resource harvesters that produce Ore aren't accounted for. Running simulations is computationally expensive. This class should be run in a thread.
        
## Methods


### CreateSimulator(ShipConstruct)
Creates a simulator from the supplied ship construct
> #### Parameters
> **ship:** A ShipConstruct to simulate

> #### Return value
> A SimSnacks simulator

### CreateSimulator(ProtoVessel)
Creates a simulator from the proto vessel
> #### Parameters
> **protoVessel:** The unloaded vessel to query for resources and converters.

> #### Return value
> A SimSnacks simulator.

### CreateSimulator(Vessel)
Creates a simulator from a loaded vessel
> #### Parameters
> **vessel:** The Vessel object to query for resources and converters.

> #### Return value
> A SimSnacks simulator.

