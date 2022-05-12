using GameEngine.Graphics;
using SharpDX;
using System;
using System.Collections.Generic;

namespace GameEngine.Collisions
{
    public abstract class ObjectCollision
    {
        private static Dictionary<Type, Dictionary<Type, Func<object, object, bool>>> _strategies;

        static ObjectCollision()
        {
            _strategies = new Dictionary<Type, Dictionary<Type, Func<object, object, bool>>>
            {
                {
                    typeof(BoundingBox),
                    new Dictionary<Type, Func<object, object, bool>>
                    {
                        { typeof(BoundingBox), BoxIntersectsBox },
                        { typeof(BoundingSphere), BoxIntersectsSphere },
                        { typeof(Plane), BoxIntersectsPlane },
                        { typeof(Ray), BoxIntersectsRay }
                    }
                },
                {
                    typeof(BoundingSphere),
                    new Dictionary<Type, Func<object, object, bool>>
                    {
                        { typeof(BoundingSphere), SphereIntersectsSphere },
                        { typeof(Plane), SphereIntersectsPlane },
                        { typeof(Ray), SphereIntersectsRay }
                    }
                },
                {
                    typeof(Plane),
                    new Dictionary<Type, Func<object, object, bool>>
                    {
                        { typeof(Plane), PlaneIntersectsPlane },
                        { typeof(Ray), PlaneIntersectsRay }
                    }
                },
                {
                    typeof(Ray),
                    new Dictionary<Type, Func<object, object, bool>>
                    {
                        { typeof(Ray), RayIntersectsRay }
                    }
                },
            };
        }

        public static bool Intersects(ObjectCollision left, ObjectCollision right)
        {
            if (left == null || right == null) return false;

            var leftCollision = left.GetCollision();
            var rightCollision = right.GetCollision();
            var leftType = leftCollision.GetType();
            var rightType = rightCollision.GetType();

            if (_strategies.ContainsKey(leftType))
            {
                var strategies = _strategies[leftType];
                if (strategies.ContainsKey(rightType))
                {
                    return strategies[rightType].Invoke(leftCollision, rightCollision);
                }
                else if (_strategies.ContainsKey(rightType))
                {
                    strategies = _strategies[rightType];
                    if (strategies.ContainsKey(leftType))
                    {
                        return strategies[leftType].Invoke(rightCollision, leftCollision);
                    }
                }
            }
            return false;
        }

        public Game3DObject GameObject { get; set; }

        protected abstract object GetCollision();

        public abstract MeshObject GetMesh();

        public abstract ObjectCollision GetCopy();

        private static bool BoxIntersectsBox(object left, object right)
        {
            return ((BoundingBox)left).Intersects((BoundingBox)right);
        }
        private static bool BoxIntersectsSphere(object left, object right)
        {
            return ((BoundingBox)left).Intersects((BoundingSphere)right);
        }
        private static bool BoxIntersectsPlane(object left, object right)
        {
            Plane plane = (Plane)right;
            PlaneIntersectionType type = ((BoundingBox)left).Intersects(ref plane);
            return type == PlaneIntersectionType.Intersecting;
        }
        private static bool BoxIntersectsRay(object left, object right)
        {
            Ray ray = (Ray)right;
            return ((BoundingBox)left).Intersects(ref ray);
        }
        private static bool SphereIntersectsSphere(object left, object right)
        {
            return ((BoundingSphere)left).Intersects((BoundingSphere)right);
        }
        private static bool SphereIntersectsPlane(object left, object right)
        {
            Plane plane = (Plane)right;
            PlaneIntersectionType type = ((BoundingSphere)left).Intersects(ref plane);
            return type == PlaneIntersectionType.Intersecting;
        }
        private static bool SphereIntersectsRay(object left, object right)
        {
            Ray ray = (Ray)right;
            return ((BoundingSphere)left).Intersects(ref ray);
        }
        private static bool PlaneIntersectsPlane(object left, object right)
        {
            Plane plane = (Plane)right;
            return ((Plane)left).Intersects(ref plane);
        }
        private static bool PlaneIntersectsRay(object left, object right)
        {
            Ray ray = (Ray)right;
            return ((Plane)left).Intersects(ref ray);

        }
        private static bool RayIntersectsRay(object left, object right)
        {
            Ray ray = (Ray)right;
            return ((Ray)left).Intersects(ref ray);
        }
    }
}
