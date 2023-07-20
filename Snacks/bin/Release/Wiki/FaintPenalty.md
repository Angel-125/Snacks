            
This outcome causes affected kerbals to faint. Example definition: OUTCOME { name = FaintPenalty resourceName = Snacks cyclesBeforeFainting = 3 faintDurationSeconds = 180 }   
        
## Fields

### resourceName
The name of the resource to check for failed processor cycles.
### cyclesBeforeFainting
The number of cycles that must fail before the kerbal faints.
### faintDurationSeconds
The number of seconds that the kerbal will faint for.
## Methods


### Constructor
Initializes a new instance of the class.
> #### Parameters
> **node:** A ConfigNode containing initialization parameters. Parameters in the class also apply.


### Constructor
Initializes a new instance of the class.
> #### Parameters
> **resourceName:** The name of the resource to check. If none of the resource processors have failed cycles containing the resource then the outcome is invalidated.

> **cyclesBeforeFainting:** The number of failed cycles required before applying the outcome..

> **faintDurationSeconds:** Faint duration seconds.

> **playerMessage:** A string containing the bad news for the player.


