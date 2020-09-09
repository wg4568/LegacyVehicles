# LegacyVehicles
Rust plugin. Adds Minicopter and Motorboat spawning back into the wild.

**Overview**
This plugin aims to implement mini and boat spawning into the latest version of Rust as closely as possible to pre-airwolf and fishing village updates.

**Config**
BoatMinimumDistance
_The minimum distance between boat spawns_

BoatToMapRatio
_MapSize / BoatToMapRatio = Max Boats_
_In other words, the lower this value, the more frequently boats will spawn_

MaxSpawnAttempts
_How many times will the plugin attempt to find a valid spawn location for an entity._
_Decrease this if you encounter heavy lag during spawn cycles, but note it may result in fewer overall spawns._

MiniMinimumDistance
_The minimum distance between minicopter spawns_

MiniToMapRatio
_MapSize / MiniToMapRatio = Max Minis_ 
_In other words, the lower this value, the more frequently minicopters will spawn_

SpawnLoopInterval
_The number of seconds between each spawn cycle._
_During each spawn cycle, the plugin will attempt to spawn enough minis and boats to meet the MaxBoats and MaxMinis values._

**Console Commands**
lv.spawnminis
_Manually trigger minicopter spawn cycle_

lv.spawnboats
_Manually trigger boat spawn cycle_

lv.despawnminis
_CAUTION! Despawns all minicopters on the map_

lv.despawnboats
_CAUTION! Despawns all boats on the map_
