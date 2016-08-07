using UnityEngine;
using System.Collections.Generic;

namespace RoadGen
{
    public static class SettlementSpawner
    {
        static bool SpawnBuilding(Vector2 center, float direction, Quadtree quadtree, out Allotment allotment, List<Allotment> allotments)
        {
            allotment = new Allotment(default(Vector2),
                0,
                UnityEngine.Random.Range(Config.allotmentMinHalfDiagonal, Config.allotmentMaxHalfDiagonal),
                UnityEngine.Random.Range(Config.allotmentMinAspect, Config.allotmentMaxAspect));
            allotment.UpdateCenterAndDirection(center, direction);
            bool allow = false;
            for (int j = 0; j < Config.allotmentPlacementLoopLimit; j++)
            {
                int c = 0;
                Vector2 offset;
                var colliders = quadtree.Retrieve(allotment.GetCollider().GetAABB());
                for (int k = 0; k < colliders.Count && (c == 0 || j < Config.allotmentPlacementLoopLimit - 1); k++)
                {
                    if (allotment.GetCollider().Collide(((ICollidable)colliders[k].reference).GetCollider(), out offset))
                    {
                        c++;
                        allotment.Center = (allotment.Center + offset);
                    }
                }
                for (int k = 0; k < allotments.Count && (c == 0 || j < Config.allotmentPlacementLoopLimit - 1); k++)
                {
                    if (allotment.GetCollider().Collide(allotments[k].GetCollider(), out offset))
                    {
                        c++;
                        allotment.Center = (allotment.Center + offset);
                    }
                }
                if (c == 0)
                {
                    allow = true;
                    break;
                }
            }
            return allow;
        }

        public static void Spawn(Segment segment, int density, float radius, Quadtree quadtree, ref List<Allotment> newAllotments)
        {
            Vector2 segmentCenter = (segment.End + segment.Start) * 0.5f;
            for (int i = 0; i < density; i++)
            {
                float randomAngle = UnityEngine.Random.value * 2.0f * Mathf.PI;
                float randomRadius = UnityEngine.Random.value * radius;
                Vector2 center = new Vector2(
                    segmentCenter.x + randomRadius * Mathf.Sin(randomAngle),
                    segmentCenter.y + randomRadius * Mathf.Cos(randomAngle)
                );
                Allotment newAllotment;
                if (SpawnBuilding(center, segment.Direction, quadtree, out newAllotment, newAllotments))
                    newAllotments.Add(newAllotment);
            }
        }

    }

}