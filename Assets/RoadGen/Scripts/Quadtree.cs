using UnityEngine;
using System.Collections.Generic;

namespace RoadGen
{
    public class Quadtree
    {
        int maxObjects;
        int maxLevels;
        int level;
        Rect bounds;
        List<AABB> objects;
        List<Quadtree> nodes;

        public Quadtree(Rect bounds, int maxObjects = 10, int maxLevels = 4, int level = 0)
        {
            this.maxObjects = maxObjects;
            this.maxLevels = maxLevels;
            this.level = level;
            this.bounds = bounds;
            objects = new List<AABB>();
            nodes = new List<Quadtree>();
        }

        public void Split()
        {
            int nextLevel = level + 1,
                subWidth = Mathf.RoundToInt(bounds.width / 2),
                subHeight = Mathf.RoundToInt(bounds.height / 2),
                x = Mathf.RoundToInt(bounds.x),
                y = Mathf.RoundToInt(bounds.y);

            //top right node
            nodes.Add(new Quadtree(new Rect(
                x + subWidth,
                y,
                subWidth,
                subHeight), maxObjects, maxLevels, nextLevel));

            //top left node
            nodes.Add(new Quadtree(new Rect(
                x,
                y,
                subWidth,
                subHeight), maxObjects, maxLevels, nextLevel));

            //bottom left node
            nodes.Add(new Quadtree(new Rect(
            x,
            y + subHeight,
            subWidth,
            subHeight), maxObjects, maxLevels, nextLevel));

            //bottom right node
            nodes.Add(new Quadtree(new Rect(
            x + subWidth,
            y + subHeight,
            subWidth,
            subHeight), maxObjects, maxLevels, nextLevel));
        }

        public int GetIndex(AABB obj)
        {
            int index = -1;
            float verticalMidpoint = bounds.x + (bounds.width * 0.5f);
            float horizontalMidpoint = bounds.y + (bounds.height * 0.5f);
            // obj can completely fit within the top quadrants
            bool topQuadrant = (obj.y < horizontalMidpoint && obj.y + obj.height < horizontalMidpoint);
            // obj can completely fit within the bottom quadrants
            bool bottomQuadrant = (obj.y > horizontalMidpoint);
            // obj can completely fit within the left quadrants
            if (obj.x < verticalMidpoint && obj.x + obj.width < verticalMidpoint)
            {
                if (topQuadrant)
                    index = 1;
                else if (bottomQuadrant)
                    index = 2;
            }
            //obj can completely fit within the right quadrants
            else if (obj.x > verticalMidpoint)
            {
                if (topQuadrant)
                    index = 0;
                else if (bottomQuadrant)
                    index = 3;
            }
            return index;
        }

        public void Insert(ICollidable collidable)
        {
            Insert(collidable.GetCollider().GetAABB());
        }

        public void Insert(AABB obj)
        {
            int i = 0, index;

            // if we have sub nodes, find in which to insert obj
            if (nodes.Count > 0)
            {
                index = GetIndex(obj);
                if (index != -1)
                {
                    nodes[index].Insert(obj);
                    return;
                }
            }

            objects.Add(obj);

            if (objects.Count > maxObjects && level < maxLevels)
            {
                // split if we don't already have sub nodes
                if (nodes.Count == 0)
                    Split();

                // add all objects to there corresponding sub nodes
                while (i < objects.Count)
                {
                    index = GetIndex(objects[i]);
                    if (index != -1)
                        nodes[index].Insert(objects.Splice(i, 1)[0]);
                    else
                        i++;
                }
            }
        }

        public List<AABB> Retrieve(ICollidable collidable)
        {
            return Retrieve(collidable.GetCollider().GetAABB());
        }

        public List<AABB> Retrieve(AABB obj)
        {
            int index = GetIndex(obj);
            List<AABB> returnObjects = new List<AABB>(objects);
            //if we have sub nodes...
            if (nodes.Count > 0)
            {
                // if obj fits into a sub node...
                if (index != -1)
                {
                    returnObjects.AddRange(nodes[index].Retrieve(obj));
                }
                // if obj does not fit into a sub node, check it against all sub nodes
                else
                {
                    for (var i = 0; i < nodes.Count; i++)
                        returnObjects.AddRange(nodes[i].Retrieve(obj));
                }
            }
            return returnObjects;
        }

        public void Clear()
        {
            objects = new List<AABB>();
            for (var i = 0; i < nodes.Count; i++)
            {
                nodes[i].Clear();
            }
            nodes = new List<Quadtree>();
        }

    }

}

