            
This precondition rolls a random die between a minimum and maximum value and compares it to a target number. If the roll meets or exceeds the target number then the precondition passes. Example definition: PRECONDITION { name = CheckRandomChance dieRollMin = 1 dieRollMax = 1000 targetNumber = 999 } 
        
## Fields

### dieRollMin
Minimum value on the die roll
### dieRollMax
Maximum value on the die roll
### targetNumber
Target number required to declare the precondition valid.
## Methods


### Constructor
Initializes a new instance of the class.
> #### Parameters
> **node:** A ConfigNode containing initialization parameters. parameters from the class also apply.


