using System;
using Microsoft.Xna.Framework;

namespace Raymagic
{
    public struct Ray
    {
        public Vector3 origin {get; private set;}
        public Vector3 direction {get; private set;}

        public Ray(Vector3 origin, Vector3 direction)
        {
            this.origin = origin;
            this.direction = Vector3.Normalize(direction);
        }
    }

    public static class RayMarchingHelper
    {
        /* public static bool SpecularEnabled { get; set; } */
        enum ObjHitType
        {
            Static,
            Dynamic,
        }
 
        public static void RayMarch(Ray ray, out float length, out Color color, int depth=0)
        {
            Map map = Map.instance;
            color = Color.Black;
            length = float.MaxValue;

            Vector3 testPos = ray.origin;
            const int maxSteps = 100;
            for (int iter = 0; iter < maxSteps; iter++)
            {
                Vector3 coords = testPos - map.mapOrigin;

                if((int)(coords.X/map.distanceMapDetail) >= map.distanceMap.GetLength(0) ||
                   (int)(coords.Y/map.distanceMapDetail) >= map.distanceMap.GetLength(1) ||
                   (int)(coords.Z/map.distanceMapDetail) >= map.distanceMap.GetLength(2) || 
                   (int)(coords.X/map.distanceMapDetail) < 0 ||
                   (int)(coords.Y/map.distanceMapDetail) < 0 ||
                   (int)(coords.Z/map.distanceMapDetail) < 0)
                {
                    return;
                }

                ObjHitType type = ObjHitType.Static;
                SDFout test;
                SDFout best = map.distanceMap[(int)Math.Abs(coords.X/map.distanceMapDetail),
                                              (int)Math.Abs(coords.Y/map.distanceMapDetail),
                                              (int)Math.Abs(coords.Z/map.distanceMapDetail)];
                if (depth < 5)
                {
                    foreach(Portal portal in map.portalList)
                    {
                        if (portal == null) continue;

                        test = portal.PortalSDF(testPos, best.distance, ray, depth);
                        if(test.distance < best.distance)
                        {
                            best = test;
                            // cut sdf checks when portal is enough
                            if(best.distance < 0.1f)
                            {
                                color = test.color;
                                return;
                            }
                        }
                    }
                }

                Object bestObj = null;
                test = map.BVH.Test(testPos, best.distance, out Object dObj);
                if(test.distance < best.distance)
                {
                    best = test;
                    bestObj = dObj;
                    type = ObjHitType.Dynamic;
                }

                if (depth > 0 || Player.instance.GodMode) 
                {
                    test = Player.instance.model.SDF(testPos, best.distance);
                    if(test.distance < best.distance)
                    {
                        best = test;
                        bestObj = Player.instance.model;
                        type = ObjHitType.Dynamic;
                    }
                }

                foreach(var obj in map.physicsObjectsList)
                {
                    test = obj.SDF(testPos, best.distance);
                    if(test.distance < best.distance)
                    {
                        best = test;
                        bestObj = obj;
                        type = ObjHitType.Dynamic;
                    }
                }

                foreach(Object iObj in map.infoObjectList)
                {
                    test = iObj.SDF(testPos, best.distance);
                    if(test.distance < best.distance)
                    {
                        best = test;
                        if(best.distance < 0.1f)
                        {
                            color = Color.Red;
                            return;
                        }
                    }
                }

                if(best.distance < 0.1f)
                {
                    SDFout final = new SDFout(float.MaxValue, Color.Pink);
                    Object finalObj = null;

                    if(type == ObjHitType.Static)
                    {
                        foreach(Object obj in map.staticObjectList)
                        {
                            var _test = obj.SDF(testPos, best.distance+10);
                            if(_test.distance <= final.distance)
                            {
                                final = _test;
                                finalObj = obj;
                            }
                        }
                    }
                    else if(type == ObjHitType.Dynamic)
                    {
                        final = best;
                        finalObj = bestObj;
                    }

                    Vector3 startPos;
                    Vector3 objectNormal;

                    float lightIntensity = 0;
                    foreach(Light light in map.lightList)
                    {
                        objectNormal = finalObj.SDF_normal(testPos);
                        startPos = testPos+objectNormal*2;
                        
                        float addIntensity = LightRayMarch(startPos, light);
                        if(addIntensity > 0)
                        {
                            addIntensity += 0.001f*SpecularHighlight(ray, testPos, objectNormal, light.position);
                        }

                        lightIntensity += addIntensity;
                    }

                    lightIntensity = Math.Max(lightIntensity, 0.00025f); // try around something
                    color = new Color(final.color.R*lightIntensity,
                                      final.color.G*lightIntensity,
                                      final.color.B*lightIntensity);

                    return;
                }

                testPos += ray.direction*best.distance;
            }
        }

        private static float SpecularHighlight(Ray viewRay, Vector3 objectHitPos, Vector3 objectNormal, Vector3 lightPos)
        {
            Vector3 reflectionDir = viewRay.direction - 2*Vector3.Dot(viewRay.direction, objectNormal)*objectNormal;
            Ray reflectionViewRay = new Ray(objectHitPos, reflectionDir);

            // "= how close is the reflection vector to the light vector)"
            return Math.Max(Vector3.Dot(Vector3.Normalize(lightPos - objectHitPos), reflectionViewRay.direction), 0);
        }

        public static float LightRayMarch(Vector3 position, Light light)
        {
            Map map = Map.instance;
            Ray ray = new Ray(position, light.position - position);

            float length = 1f;

            float intensity = light.intensity;
            float k = 16f*intensity;

            Vector3 testPos = position + ray.direction*length;
            while(length < (position - light.position).Length() - 0.1f)
            {
                Vector3 coords = testPos - map.mapOrigin;

                SDFout test;
                SDFout best = new SDFout(float.MaxValue, Color.Pink);
                try
                {
                    best = map.distanceMap[(int)Math.Abs(coords.X/map.distanceMapDetail),
                                           (int)Math.Abs(coords.Y/map.distanceMapDetail),
                                           (int)Math.Abs(coords.Z/map.distanceMapDetail)];
                }
                catch (IndexOutOfRangeException e)
                {
                    Console.WriteLine("LIGHT ERROR");
                }

                test = map.BVH.Test(testPos, best.distance, out Object _);
                if(test.distance < best.distance)
                {
                    best = test;
                }

                test = Player.instance.model.SDF(testPos, best.distance);
                if(test.distance < best.distance)
                {
                    best = test;
                }

                foreach(var obj in map.physicsObjectsList)
                {
                    test = obj.SDF(testPos, best.distance);
                    if(test.distance < best.distance)
                    {
                        best = test;
                    }
                }

                float lightDst = light.DistanceFrom(testPos);
                if(lightDst < best.distance)
                {
                    break;
                }

                if(best.distance < 0.01f)
                    return 0.0f;

                intensity = Math.Min(intensity, k*best.distance/length);
                if(intensity < 0.001f)
                    return 0.0f;

                length += best.distance;
                testPos += ray.direction*best.distance;
            }

            return intensity/(Vector3.DistanceSquared(position, light.position));
        }

        public static void PhysicsRayMarch(Ray ray, int maxSteps, float stepMinSize, out float length, out Vector3 hit, out Object hitObj, Object caller = null)
        {
            Map map = Map.instance;
            hit = ray.origin;
            length = float.MaxValue;
            hitObj = null;

            SDFout test;
            Vector3 testPos = ray.origin;

            for (int iter = 0; iter < maxSteps; iter++)
            {
                bool portalHit = false;

                SDFout best = new SDFout(float.MaxValue, Color.Pink);

                foreach(Portal portal in map.portalList)
                {
                    if (portal == null) continue;

                    test = portal.PortalSDF(testPos, best.distance, ray, 0, physics:true);
                    if(test.distance < best.distance)
                    {
                        best = test;
                        hitObj = portal;
                        portalHit = true;
                    }
                }

                foreach(Object obj in map.staticObjectList)
                {
                    test = obj.SDF(testPos, best.distance, physics:true);
                    if(test.distance < best.distance)
                    {
                        best = test;
                        hitObj = obj;
                        portalHit = false;
                    }
                }

                foreach(Object dObj in map.dynamicObjectList)
                {
                    test = dObj.SDF(testPos, best.distance, physics:true);
                    if(test.distance < best.distance)
                    {
                        best = test;
                        hitObj = dObj;
                        portalHit = false;
                    }
                }

                foreach(PhysicsObject pObj in map.physicsObjectsList)
                {
                    if (pObj == caller) continue;

                    test = pObj.SDF(testPos, best.distance, physics:true);
                    if(test.distance < best.distance)
                    {
                        best = test;
                        hitObj = pObj;
                        portalHit = false;
                    }
                }

                if(best.distance <= stepMinSize)
                {
                    if(best.distance < 0)
                        length = -1;
                    else
                    {
                        length = (ray.origin - testPos).Length(); 
                        if (maxSteps == 1) // collision checks use only one step - diffferent length better
                        {
                            length = best.distance;
                        }
                    }

                    hit = testPos;
                    return;
                }

                testPos += ray.direction*best.distance;
            }

            length = (ray.origin - testPos).Length();
            hit = testPos;
        }
    }
}
