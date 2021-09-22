Getting set up:

1. Drag and drop the SoccerBox prefab into your scene. DO NOT resize the prefab itself. Place it roughly where you want your rectangular play area.

2. Move and resize child gameobject "Play Area" as desired. The white box mesh shows the area (this mesh will not appear in the World, it's tagged EditorOnly).

3. Place the control panel gameobject "Settings UI" as desired.

- Include any parts of your World that you want; SoccerBox simply adds a kickable soccer ball, but adding play mechanics and environment is up to you. An easy start is adding a wall for wall ball.
--------------------------------------------------------------

other notes:
- player body collider physics *only* work inside the play area. This is for performance reasons. The update loop does not run at all for a player not inside the area. This gives the World creator full control; if you want the physics to work everywhere, just make the play area cover the whole world.

- hold Ctrl+Shift and drag the pivot while moving the prefab's structures (like play area and control panel). I've set up the pivot points so that they drag nicely along the ground.

- there are more miscellaneous config settings on the scripts within the prefab. Check the hover tooltips for descriptions.