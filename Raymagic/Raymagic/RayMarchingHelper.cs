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
        public static void RayMarch(Vector3 position, Vector3 dir, out float length, out Color color)
        {
            Map map = Map.instance;
            color = Color.Black;
            length = float.MaxValue;

            dir.Normalize();

            Vector3 testPos = position;
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


                bool sObj = true; // (is the best one static?)
                SDFout test;
                SDFout best = map.distanceMap[(int)Math.Abs(coords.X/map.distanceMapDetail),
                                              (int)Math.Abs(coords.Y/map.distanceMapDetail),
                                              (int)Math.Abs(coords.Z/map.distanceMapDetail)];

                Object bestDObj = null;
                test = map.BVH.Test(testPos, best.distance, out Object dObj);
                if(test.distance < best.distance)
                {
                    best = test;
                    bestDObj = dObj;
                    sObj = false;
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
                    /* float bestDst = float.MaxValue; */
                    /* Color bestColor = color; */
                    /* Object bestObj = null; */ 

                    if(sObj)
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
                    else
                    {
                        final = best;
                        finalObj = bestDObj;
                    }

                    Vector3 startPos;
                    float lightIntensity = 0;
                    foreach(Light light in map.lightList)
                    {
                        startPos = testPos+finalObj.SDF_normal(testPos)*2;
                        
                        lightIntensity += LightRayMarch(startPos, light);
                    }

                    lightIntensity = Math.Max(lightIntensity, 0.0001f); // try around something
                    color = new Color(final.color.R*lightIntensity,
                                      final.color.G*lightIntensity,
                                      final.color.B*lightIntensity);

                    return;
                }

                testPos += dir*best.distance;
            }
        }

        public static float LightRayMarch(Vector3 position, Light light)
        {
            Map map = Map.instance;
            Vector3 dir = (light.position - position);
            dir.Normalize();

            float length = 1f;
            /* float test; */
            float intensity = light.intensity;
            float k = 16f*intensity;
            while(length < (position - light.position).Length() - 0.1f)
            {
                Vector3 coords = position + dir*length - map.mapOrigin;

                SDFout test;
                SDFout best = map.distanceMap[(int)Math.Abs(coords.X/map.distanceMapDetail),
                                              (int)Math.Abs(coords.Y/map.distanceMapDetail),
                                              (int)Math.Abs(coords.Z/map.distanceMapDetail)];

                test = map.BVH.Test(position + dir*length, best.distance, out Object dObj);
                if(test.distance < best.distance)
                {
                    best = test;
                }

                float lightDst = light.DistanceFrom(position + dir*length);
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
            }

            length = (position - light.position).Length();
            return intensity/(length*length);
        }

        public static void PhysicsRayMarch(Vector3 position, Vector3 dir, int maxSteps, float stepMinSize, out float length, out Vector3 hit, out Object hitObj)
        {
            Map map = Map.instance;
            hit = position;
            length = float.MaxValue;
            hitObj = null;

            dir.Normalize();

            Vector3 testPos = position;

            SDFout test;

            for (int iter = 0; iter < maxSteps; iter++)
            {
                SDFout best = new SDFout(float.MaxValue, Color.Pink);

                foreach(Object obj in map.staticObjectList)
                {
                    test = obj.SDF(testPos, best.distance, physics:true);
                    if(test.distance < best.distance)
                    {
                        best = test;
                        hitObj = obj;
                    }
                }

                foreach(Object dObj in map.dynamicObjectList)
                {
                    test = dObj.SDF(testPos, best.distance, physics:true);
                    if(test.distance < best.distance)
                    {
                        best = test;
                        hitObj = dObj;
                    }
                }

                if(best.distance <= stepMinSize)
                {
                    if(best.distance < 0)
                        length = -1;
                    else
                        length = (position - testPos).Length(); 

                    hit = testPos;
                    return;
                }

                testPos += dir*best.distance;
            }

            length = (position - testPos).Length();
            hit = testPos;
        }
    }
}
