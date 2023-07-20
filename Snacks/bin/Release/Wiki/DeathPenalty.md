            
This outcome causes affected kerbals to die. Example definition: OUTCOME { name = DeathPenalty resourceName = Snacks cyclesBeforeDeath = 10 }   
        
## Fields

### resourceName
The name of the resource to check for failed processor cycles.
### cyclesBeforeDeath
The number of cycles that must fail before the kerbal dies.
## Methods


### Constructor
Initializes a new instance of the class.
> #### Parameters
> **node:** A ConfigNode containing initialization parameters. Parameters in the class also apply.


### Constructor
Initializes a new instance of the class.
> #### Parameters
> **resourceName:** The name of the resource to check. If no processor has a failed cycle with the resource then the outcome is invalidated.

> **cyclesBeforeDeath:** The number of failed processor cycles required before applying the outcome.

> **playerMessage:** A string containing the bad news for the player.


