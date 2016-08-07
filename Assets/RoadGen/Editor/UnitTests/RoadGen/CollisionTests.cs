using UnityEngine;
using System.Collections;
using NUnit.Framework;

namespace RoadGen
{
    [TestFixture]
    public class CollisionTests
    {
        [Test]
        public void test_arm_points()
        {
            Vector2 p0 = new Vector2(2, 2);
            Vector2 p1 = new Vector2(0, 0);
            Vector2 p2 = new Vector2(-2, 2);
            float width = Mathf.Sqrt(2);

            // ---

            Vector2 elbow, joint;
            Collision.ArmPoints(p0, p1, p2, width, out elbow, out joint);

            // ---

            Assert.AreEqual(new Vector2(0, -1), elbow);
            Assert.AreEqual(new Vector2(0, 1), joint);
        }

    }

}