            
This outcome consumes the specified resource in the desired amount. It can be a vessel resource or a roster resource. Example definition: OUTCOME { name = ConsumeResource resourceName = Stress amount = 1 }
        
## Fields

### resourceName
Name of the resource to produce
### randomMin
Optional minimum value of random amount to produce.
### randomMax
Optional maximum value of random amount to produce.
### amount
Amount of resource to consume. Takes presedence over randomMin and randomMax
## Methods


### Constructor
Initializes a new instance of the class.
> #### Parameters
> **node:** A ConfigNode containing initialization parameters. Parameters in the class also apply.


