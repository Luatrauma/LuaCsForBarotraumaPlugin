using System;
using System.Collections.Generic;
using System.Diagnostics;
using Barotrauma.Items.Components;
using HarmonyLib;
using Microsoft.Xna.Framework;

namespace Barotrauma.LuaCs;

internal class UpdatePriorityService : ISystem
{
    public bool IsDisposed { get; private set; }

    private static LuaGame _game;
    private readonly Harmony _harmony;

    private static int _mapEntityUpdateInterval = 1;
    private static int _poweredUpdateInterval = 1;
    private static int _characterUpdateInterval = 1;

    public static int MapEntityUpdateInterval
    {
        get => _mapEntityUpdateInterval;
        set => _mapEntityUpdateInterval = Math.Max(value, 1);
    }

    public static int PoweredUpdateInterval
    {
        get => _poweredUpdateInterval;
        set => _poweredUpdateInterval = Math.Max(value, 1);
    }

    public static int CharacterUpdateInterval
    {
        get => _characterUpdateInterval;
        set => _characterUpdateInterval = Math.Max(value, 1);
    }

    private static int _mapEntityUpdateTick;
    private static int _characterUpdateTick = 1;

    public UpdatePriorityService(LuaGame game)
    {
        _game = game;
        _harmony = new Harmony("LuaCsForBarotrauma.UpdatePriority");
        _harmony.PatchAll(typeof(UpdatePriorityService));
    }

    public static void ResetIntervals()
    {
        MapEntityUpdateInterval = 1;
        PoweredUpdateInterval = 1;
        CharacterUpdateInterval = 1;
    }

    #region MapEntity

    [HarmonyPatch(typeof(MapEntity), nameof(MapEntity.UpdateAll)), HarmonyPrefix]
    private static bool MapEntity_UpdateAll_Pre(float deltaTime, Camera cam)
    {
        _mapEntityUpdateTick++;

        if (_mapEntityUpdateInterval == 1 && _poweredUpdateInterval == 1 && _game.UpdatePriorityItems.Count == 0)
        {
            return true;
        }

        bool updateMapEntities = _mapEntityUpdateTick % MapEntityUpdateInterval == 0;

#if CLIENT
        var sw = new System.Diagnostics.Stopwatch();
        sw.Start();
#endif
        if (updateMapEntities)
        {
            foreach (Hull hull in Hull.HullList)
            {
                hull.Update(deltaTime * MapEntityUpdateInterval, cam);
            }
#if CLIENT
            Hull.UpdateCheats(deltaTime * MapEntityUpdateInterval, cam);
#endif

            foreach (Structure structure in Structure.WallList)
            {
                structure.Update(deltaTime * MapEntityUpdateInterval, cam);
            }
        }

        foreach (Gap gap in Gap.GapList)
        {
            gap.ResetWaterFlowThisFrame();
        }

        //update gaps in random order, because otherwise in rooms with multiple gaps
        //the water/air will always tend to flow through the first gap in the list,
        //which may lead to weird behavior like water draining down only through
        //one gap in a room even if there are several
        List<Gap> gapUpdateList = MapEntity.gapUpdateList;
        gapUpdateList.Clear();
        gapUpdateList.AddRange(Gap.GapList);
        //Fisher-Yates shuffle
        for (int i = gapUpdateList.Count - 1; i > 0; i--)
        {
            int j = Rand.Int(i + 1);
            (gapUpdateList[i], gapUpdateList[j]) = (gapUpdateList[j], gapUpdateList[i]);
        }
        foreach (Gap gap in gapUpdateList)
        {
            gap.Update(deltaTime, cam);
        }

#if CLIENT
        sw.Stop();
        GameMain.PerformanceCounter.AddElapsedTicks("Update:MapEntity:Misc", sw.ElapsedTicks);
        sw.Restart();
#endif
        if (_mapEntityUpdateTick % PoweredUpdateInterval == 0)
        {
            Powered.UpdatePower(deltaTime * PoweredUpdateInterval);
        }
        Item.UpdatePendingConditionUpdates(deltaTime);
        if (updateMapEntities)
        {
            Item lastUpdatedItem = null;
            try
            {
                foreach (Item item in Item.ItemList)
                {
                    if (_game.UpdatePriorityItems.Contains(item)) { continue; }
                    lastUpdatedItem = item;
                    item.Update(deltaTime * MapEntityUpdateInterval, cam);
                }
            }
            catch (InvalidOperationException e)
            {
                GameAnalyticsManager.AddErrorEventOnce(
                    "MapEntity.UpdateAll:ItemUpdateInvalidOperation",
                    GameAnalyticsManager.ErrorSeverity.Critical,
                    $"Error while updating item {lastUpdatedItem?.Name ?? "null"}: {e.Message}");
                throw new InvalidOperationException($"Error while updating item {lastUpdatedItem?.Name ?? "null"}", innerException: e);
            }
        }

        foreach (Item item in _game.UpdatePriorityItems)
        {
            if (item.Removed) { continue; }

            item.Update(deltaTime, cam);
        }

#if CLIENT
        if (updateMapEntities)
        {
            MapEntity.UpdateAllProjSpecific(deltaTime * MapEntityUpdateInterval);
        }

        sw.Stop();
        GameMain.PerformanceCounter.AddElapsedTicks("Update:MapEntity:Items", sw.ElapsedTicks);
        sw.Restart();
#endif
        if (updateMapEntities)
        {
            Entity.Spawner?.Update();
        }

        return false;
    }

    #endregion

    #region Character

    [HarmonyPatch(typeof(Character), nameof(Character.UpdateAll)), HarmonyPrefix]
    private static bool Character_UpdateAll_Pre(float deltaTime, Camera cam)
    {
        _characterUpdateTick++;

        if (_characterUpdateInterval == 1 && _game.UpdatePriorityCharacters.Count == 0)
        {
            return true;
        }

        CharacterUpdateAll(deltaTime, cam);
        return false;
    }

    /// <summary>
    /// Copy of the vanilla <see cref="Character.UpdateAll"/> with the update interval and priority characters from the LuaCs sourcemod.
    /// </summary>
    private static void CharacterUpdateAll(float deltaTime, Camera cam)
    {
        if (GameMain.NetworkMember == null || !GameMain.NetworkMember.IsClient) // single player or server
        {
            foreach (Character c in Character.CharacterList)
            {
                // TODO: The logic below seems to be overly complicated and quite confusing
                if (c is not AICharacter && !c.IsRemotePlayer) { continue; } // confusing -> what this line is intended for? local player? But that's handled below...
                if (c.IsRemotePlayer)
                {
                    // Let the client tell when to enable the character. If we force it enabled here, it may e.g. get killed while still loading a round.
                    continue;
                }
                if (c.IsLocalPlayer || (c.IsBot && !c.IsDead))
                {
                    c.Enabled = true;
                }
                else if (GameMain.NetworkMember != null && GameMain.NetworkMember.IsServer) // mp server
                {
                    //disable AI characters that are far away from all clients and the host's character and not controlled by anyone
                    float closestPlayerDist = c.GetDistanceToClosestPlayer();
                    if (closestPlayerDist > c.Params.DisableDistance)
                    {
                        c.Enabled = false;
                        if (c.IsDead && c.AIController is EnemyAIController)
                        {
                            Entity.Spawner?.AddEntityToRemoveQueue(c);
                        }
                    }
                    else if (closestPlayerDist < c.Params.DisableDistance * 0.9f)
                    {
                        c.Enabled = true;
                    }
                }
                else if (Submarine.MainSub != null) // sp only?
                {
                    //disable AI characters that are far away from the sub and the controlled character
                    float distSqr = Vector2.DistanceSquared(Submarine.MainSub.WorldPosition, c.WorldPosition);
                    if (Character.Controlled != null)
                    {
                        distSqr = Math.Min(distSqr, Vector2.DistanceSquared(Character.Controlled.WorldPosition, c.WorldPosition));
                    }
                    else
                    {
                        distSqr = Math.Min(distSqr, Vector2.DistanceSquared(GameMain.GameScreen.Cam.GetPosition(), c.WorldPosition));
                    }

                    if (distSqr > MathUtils.Pow2(c.Params.DisableDistance))
                    {
                        c.Enabled = false;
                        if (c.IsDead && c.AIController is EnemyAIController)
                        {
                            Entity.Spawner?.AddEntityToRemoveQueue(c);
                        }
                    }
                    else if (distSqr < MathUtils.Pow2(c.Params.DisableDistance * 0.9f))
                    {
                        c.Enabled = true;
                    }
                }
            }
        }

        if (_characterUpdateTick % CharacterUpdateInterval == 0)
        {
            foreach (Character character in Character.CharacterList)
            {
                if (_game.UpdatePriorityCharacters.Contains(character)) { continue; }

                Debug.Assert(character is { Removed: false });
                character.Update(deltaTime * CharacterUpdateInterval, cam);
            }
        }

        foreach (Character character in _game.UpdatePriorityCharacters)
        {
            if (character.Removed) { continue; }

            character.Update(deltaTime, cam);
        }
    }
    #endregion

    public FluentResults.Result Reset()
    {
        return FluentResults.Result.Ok();
    }

    public void Dispose()
    {
        if (IsDisposed) { return; }
        IsDisposed = true;
        ResetIntervals();
        _harmony.UnpatchSelf();
    }
}
