# FarmGame

## Controls
* Tap menu buttons on the bottom
* Drag and drop plants from the right onto the field
* Tap plants while in watering / harvest mode
* Optional PC hotkeys for switching build modes: B, N, M, ESC

## Asset credits
### Skybox and Textures
* https://freestylized.com
### UI
* https://penzilla.itch.io/basic-gui-bundle
* https://openmoji.org
### Raw sounds used to create sfx
* https://freesound.org/people/klankbeeld/sounds/816665
* https://freesound.org/people/SunixMuz/sounds/767851
* https://freesound.org/people/Divoljud/sounds/547620
* https://freesound.org/people/D.S.G./sounds/244652
* https://freesound.org/people/YuriNikolai/sounds/577355
* https://freesound.org/people/Valenspire/sounds/699491
* https://freesound.org/people/YannSauvin/sounds/816748
* https://freesound.org/people/derjuli/sounds/467227

## Design considerations
* Textures have been downscaled and compressed for low-end mobile targets
* Most lighting features and post-processing have been disabled for the same reason
* I've manually reworked all crop models, their UV editing and textures in blender so they are clean, repaired and only use one model/material each
* The crop models have been stripped of their animation to use less expensive mesh renderers instead of skinned mesh renderers for performance and are animated on the gpu instead as well as with a simple manipulation of their transform.scale over time
* Crops are drawn with a custom shader to force gpu instancing to reduce them to a single draw call
* Ingame crop need icons are rendered using a single draw call as well
* The Update function has been avoided entirely; Instead I used coroutines with a performance setting to reduce their call frequency where appropriate
* I fixed the worker animations in blender and unity and added an idle animation for the gardener
* I decided to not use root motion or the rotation animations because of the way I move workers in code
* I sped up the interaction animations because they took too long and stall the gameplay

* I've noted down future changes for long term scalability below
* Some foundations for scalability have been already layed down for example with the VFX manager and general system classes

## Future changes
### Audio
* Implement pooling for audio sources to avoid potential cutoffs

### UI
* UI crop buttons and stats should be generated based on a centralized crop database and prefabs instead of created manually for scalability
* Hardcoded strings should be centralized for localization support
* Add coin particles on crop harvest that fly to UI element
* Fully animate UI for responsiveness and better game feel -> JUICE

### Workers
* Change garbage man model to a farmer with harvesting tool
* Waiting for the interaction animations still takes too long; They should be redone or another gameplay feature added

### Refactoring
* The grid manager ended up doing a lot at once; The class should be refactored and responsibilities further split -> SOLID

### Additional features
* Saving, loading
* Etc.

### (Playtesting and value adjustments)

