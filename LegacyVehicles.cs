using System;
using System.Collections.Generic;
using System.Threading;
using ConVar;
using Newtonsoft.Json;
using Rust.Ai;
using Rust.Registry;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("LegacyVehicles", "Leap", "1.0.0")]
    [Description("Reverts Rust to legacy vehicle spawning behavior")]

    class LegacyVehicles : RustPlugin
    {
        private static readonly System.Random Random = new System.Random();
        private static Timer spawnTimer;

        protected override void LoadDefaultConfig()
        {
            Config["MaxSpawnAttempts"] = 500;
            Config["MiniMinimumDistance"] = 50;
            Config["BoatMinimumDistance"] = 100;
            Config["BoatToMapRatio"] = 50;
            Config["MiniToMapRatio"] = 100;
            Config["SpawnLoopInterval"] = 300;
        }

        private void Init()
        {
            spawnTimer = timer.Every((int) Config["SpawnLoopInterval"], () =>
            {
                BoatSpawnCycle();
                MiniSpawnCycle();
            });

            spawnTimer.Callback.Invoke();
        }

        private void Unload()
        {
            spawnTimer.Destroy();
        }

        private int GetMapSize()
        {
            return ConVar.Server.worldsize;
        }

        private Vector3 RandomWorldPosition()
        {
            var size = GetMapSize();
            var x = Random.Next(-size / 2, size / 2);
            var z = Random.Next(-size / 2, size / 2);
            var y = TerrainMeta.HeightMap.GetHeight(new Vector3(x, 0, z));
            return new Vector3(x, y, z);
        }

        #region Commands
        [ConsoleCommand("lv.spawnminis")]
        void SpawnMiniCommand(ConsoleSystem.Arg arg)
        {
            if (!arg.IsAdmin) return;

            int spawned = MiniSpawnCycle();
            String response = "Spawned " + spawned.ToString() + " mini(s)";

            if (arg.Connection != null)
            {
                BasePlayer player = arg.Connection.player as BasePlayer;
                player.ConsoleMessage(response);
            } else
            {
                Puts(response);
            }
        }

        [ConsoleCommand("lv.spawnboats")]
        void SpawnBoatCommand(ConsoleSystem.Arg arg)
        {
            if (!arg.IsAdmin) return;

            int spawned = BoatSpawnCycle();
            String response = "Spawned " + spawned.ToString() + " boats(s)";

            if (arg.Connection != null)
            {
                BasePlayer player = arg.Connection.player as BasePlayer;
                player.ConsoleMessage(response);
            }
            else
            {
                Puts(response);
            }
        }

        [ConsoleCommand("lv.despawnminis")]
        void DespawnMinisCommand(ConsoleSystem.Arg arg)
        {
            if (!arg.IsAdmin) return;

            int destroyed = DespawnMinis();
            String response = "Destroyed " + destroyed.ToString() + " mini(s)";

            if (arg.Connection != null)
            {
                BasePlayer player = arg.Connection.player as BasePlayer;
                player.ConsoleMessage(response);
            }
            else
            {
                Puts(response);
            }
        }

        [ConsoleCommand("lv.despawnboats")]
        void DespawnBoatsCommand(ConsoleSystem.Arg arg)
        {
            if (!arg.IsAdmin) return;

            int destroyed = DespawnBoats();
            String response = "Destroyed " + destroyed.ToString() + " boats(s)";

            if (arg.Connection != null)
            {
                BasePlayer player = arg.Connection.player as BasePlayer;
                player.ConsoleMessage(response);
            }
            else
            {
                Puts(response);
            }
        }
        #endregion

        #region Boats
        private int BoatSpawnCycle()
        {
            int totalSpawned = 0;
            int totalBoats = GameObject.FindObjectsOfType<MotorBoat>().Length;
            int needToSpawn = CalculateTargetBoats() - totalBoats;
            if (needToSpawn > 0)
            {
                for (int i = 0; i < needToSpawn; i++)
                {
                    bool success = SpawnBoat();
                    if (success) totalSpawned++;
                }
            }

            return totalSpawned;
        }

        private int CalculateTargetBoats()
        {
            float oceanBuffer = 600;
            return (int) ((float) (GetMapSize() - oceanBuffer) / (int) Config["BoatToMapRatio"]);
        }

        private bool CanSpawnBoat(Vector3 position)
        {
            if (position.y > 0.3) return false;
            var boats = new List<BaseBoat>();
            Vis.Entities(position, (int) Config["BoatMinimumDistance"], boats);
            if (boats.Count > 0) return false;

            bool isBeach = TerrainMeta.TopologyMap.GetTopology(position, TerrainTopology.BEACH);
            bool isRiver = TerrainMeta.TopologyMap.GetTopology(position, TerrainTopology.RIVER);
            bool isMonument = TerrainMeta.TopologyMap.GetTopology(position, TerrainTopology.MONUMENT);
            bool isBuilding = TerrainMeta.TopologyMap.GetTopology(position, TerrainTopology.BUILDING);
            bool isCliff = TerrainMeta.TopologyMap.GetTopology(position, TerrainTopology.CLIFF);

            if (isMonument || isBuilding || isCliff || isRiver) return false;
            if (!isBeach) return false;

            return true;
        }

        private bool SpawnBoat()
        {
            Vector3 spawnpoint;
            int spawnAttempts = 0;
            int maxSpawnAttempts = (int) Config["MaxSpawnAttempts"];
            do { spawnpoint = RandomWorldPosition(); spawnAttempts++; }
            while (!CanSpawnBoat(spawnpoint) && spawnAttempts < maxSpawnAttempts);

            if (spawnAttempts == maxSpawnAttempts) return false;

            Quaternion rotation = Quaternion.Euler(0, Random.Next(0, 360), 0);
            MotorBoat boat = GameManager.server.CreateEntity("assets/content/vehicles/boats/rowboat/rowboat.prefab", spawnpoint, rotation) as MotorBoat;
            if (boat == null) return false;

            boat.Spawn();
            return true;
        }

        private int DespawnBoats()
        {
            int destroyed = 0;
            foreach (var boat in GameObject.FindObjectsOfType<MotorBoat>())
            {
                if (boat == null || boat.IsDestroyed) continue;
                else boat.Kill();
                destroyed++;
            }

            return destroyed;
        }
        #endregion

        #region Minis
        private int MiniSpawnCycle()
        {
            int totalSpawned = 0;
            int totalMinis = GameObject.FindObjectsOfType<MiniCopter>().Length;
            int needToSpawn = CalculateTargetMinis() - totalMinis;
            if (needToSpawn > 0)
            {
                for (int i = 0; i < needToSpawn; i++)
                {
                    bool success = SpawnMini();
                    if (success) totalSpawned++;
                }
            }

            return totalSpawned;
        }

        private int CalculateTargetMinis()
        {
            return (int) ((float) GetMapSize() / (int) Config["MiniToMapRatio"]);
        }

        private bool CanSpawnMini(Vector3 position)
        {
            var minis = new List<MiniCopter>();
            Vis.Entities(position, (int) Config["MiniMinimumDistance"], minis);
            if (minis.Count > 0) return false;

            var cars = new List<ModularCar>();
            Vis.Entities(position, 2, cars);
            if (cars.Count > 0) return false;

            var junk = new List<JunkPile>();
            Vis.Entities(position, 1, junk);
            if (junk.Count > 0) return false;

            bool isRoad = TerrainMeta.TopologyMap.GetTopology(position, TerrainTopology.ROAD);
            bool isRoadside = TerrainMeta.TopologyMap.GetTopology(position, TerrainTopology.ROADSIDE);
            bool isMonument = TerrainMeta.TopologyMap.GetTopology(position, TerrainTopology.MONUMENT);
            bool isBuilding = TerrainMeta.TopologyMap.GetTopology(position, TerrainTopology.BUILDING);
            bool isPowerline = TerrainMeta.TopologyMap.GetTopology(position, TerrainTopology.POWERLINE);
            bool isCliff = TerrainMeta.TopologyMap.GetTopology(position, TerrainTopology.CLIFF);

            if (isMonument || isBuilding || isPowerline || isCliff) return false;
            if (!(isRoad || isRoadside)) return false;

            return true;
        }

        private bool SpawnMini()
        {
            Vector3 spawnpoint;
            int spawnAttempts = 0;
            int maxSpawnAttempts = (int) Config["MaxSpawnAttempts"];
            do { spawnpoint = RandomWorldPosition(); spawnAttempts++; }
            while (!CanSpawnMini(spawnpoint) && spawnAttempts < maxSpawnAttempts);

            if (spawnAttempts == maxSpawnAttempts) return false;

            Quaternion rotation = Quaternion.Euler(0, Random.Next(0, 360), 0);
            MiniCopter heli = GameManager.server.CreateEntity("assets/content/vehicles/minicopter/minicopter.entity.prefab", spawnpoint, rotation) as MiniCopter;
            if (heli == null) return false;

            heli.Spawn();
            return true;
        }

        private int DespawnMinis()
        {
            int destroyed = 0;
            foreach (var mini in GameObject.FindObjectsOfType<MiniCopter>())
            {
                if (mini == null || mini.IsDestroyed) continue;
                else mini.Kill();
                destroyed++;
            }

            return destroyed;
        }
        #endregion
    }
}
