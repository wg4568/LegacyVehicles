Adds Minicopter and Motorboat spawning back into the wild. This plugin aims to implement mini and boat spawning into the latest version of Rust as closely as possible to pre-airwolf and fishing village updates.

## Configuration

* `BoatMinimumDistance`  
The minimum distance between boat spawns.

* `BoatToMapRatio`  
MapSize / BoatToMapRatio = Max Boats  
In other words, the lower this value, the more frequently boats will spawn.

* `MaxSpawnAttempts`  
How many times will the plugin attempt to find a valid spawn location for an entity.  
Decrease this if you encounter heavy lag during spawn cycles, but note it may result in fewer overall spawns.

* `MiniMinimumDistance`  
The minimum distance between minicopter spawns

* `MiniToMapRatio`  
MapSize / MiniToMapRatio = Max Minis  
In other words, the lower this value, the more frequently minicopters will spawn.

* `SpawnLoopInterval`  
The number of seconds between each spawn cycle.  
During each spawn cycle, the plugin will attempt to spawn enough minis and boats to meet the MaxBoats and MaxMinis values.

## Console Commands

* `lv.spawnminis`  
Manually trigger minicopter spawn cycle

* `lv.spawnboats`  
Manually trigger boat spawn cycle

* `lv.despawnminis`
**CAUTION**: Despawns all minicopters on the map

* `lv.despawnboats`
**CAUTION**: Despawns all boats on the map