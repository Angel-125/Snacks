            
The Stress processor is designed to work with the Stress roster resource. Essentially, Stress is an abstracted habitation mechanic that takes into account a variety of different events. The main thing that causes Stress is being aboard a vessel; you don't want to send kerbals to Jool in a Mk1 command pod! NASA allocates 25 m^3 of space per astronaut per year aboard the ISS, and Stress is based off that number. The larger the habitable volume, the greater a kerbal's maximum Stress becomes, and it's dynamically updated whenever a kerbal changes craft. Assuming no other events, a kerbal will accumulate 1 point of Stress per day, and when the kerbal reaches it's maximum Stress, bad things happen.
        
## Fields

### MaxSeatsForMultiplier
The first N seats use the multiplier instead of the N^3 formula.
### SpacePerSeatMultiplier
How much Space a single seat provides, assuming that the vessel's number of seats is less than or equal to MaxSeatsForMultiplier.
## Methods


### CalculateSpace(Vessel)
Calculates how much Space a vessel has. It is a function of crew capacity and is influenced by the number of crew currently aboard.
> #### Parameters
> **vessel:** The Vessel to query.

> #### Return value
> The amount of Space aboard the vessel.

### CalculateSpace(System.Int32,System.Int32)
Calculates how much Space a vessel has. It is a function of crew capacity and is influenced by the number of crew currently aboard.
> #### Parameters
> **crewCount:** Current crew count aboard the vessel

> **crewCapacity:** Current crew capacity of the vessel

> #### Return value
> The amount of Space aboard the vessel.

