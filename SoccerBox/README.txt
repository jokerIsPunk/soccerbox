Getting set up:

1. Drag and drop the SoccerBox prefab into your scene. DO NOT resize the prefab itself. Place it roughly where you want your rectangular play area.

2. Move and resize child gameobject "Play Area" if desired. The white box mesh shows the area (this mesh will not appear in the World; it's tagged EditorOnly).

3. Place the control panel gameobject "Settings UI" as desired.

- Adding other game mechanics and environment is up to you. Try adding a wall for simple wall-ball.
--------------------------------------------------------------

other notes:
- in the original "SoccerBox" prefab, hitting or kicking objects ONLY functions inside the rectangular play area. This is to allow World creators to control and limit the effects of the prefab, and to save performance.
	If you instead want collisions to work everywhere, then use the "SoccerBox (no nets)" variant and ignore the deactivated Play Area.

- hold Ctrl+Shift and drag the pivot while moving the prefab's structures (like play area and control panel). I've set up the pivot points so that they drag nicely along the ground.

- the Play Area uses multiple trigger colliders on the MirrorReflection layer. Beware that this might generate collisions with other functioning elements in your World with unpredictable consequences. You can use Physics.IgnoreCollision() to work around that, and send me a report if it's an issue.

- there are more miscellaneous config settings on the scripts within the prefab. Check the hover tooltips for descriptions.