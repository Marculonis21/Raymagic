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
        enum ObjHitType
        {
            Static,
            Dynamic,
        }
 
        /* public static bool gammaEnabled = false; */
        public static void RayMarch(Ray ray, out float length, out Color color, int depth=0)
        {
            Map map = Map.instance;
            color = Color.Black;
            Color transparentColor = Color.White;
            float transparentDepth = 0;
            float transparentStep = 0.25f;

            length = float.MaxValue;

            Vector3 testPos = ray.origin;
            const int maxSteps = 100;
            for (int iter = 0; iter < maxSteps; iter++)
            {
                Vector3 coords = testPos - map.mapOrigin;

                SDFout test;
                SDFout best = new SDFout(length, color);
                ObjHitType type = ObjHitType.Static;
                Object staticObject = null;

                if((int)(coords.X/map.distanceMapDetail) >= map.distanceMap.GetLength(0) ||
                   (int)(coords.Y/map.distanceMapDetail) >= map.distanceMap.GetLength(1) ||
                   (int)(coords.Z/map.distanceMapDetail) >= map.distanceMap.GetLength(2) || 
                   (int)(coords.X/map.distanceMapDetail) < 0 ||
                   (int)(coords.Y/map.distanceMapDetail) < 0 ||
                   (int)(coords.Z/map.distanceMapDetail) < 0)
                {
                }
                else
                {
                    DMValue dmValue = map.distanceMap[(int)Math.Abs(coords.X/map.distanceMapDetail),
                                                      (int)Math.Abs(coords.Y/map.distanceMapDetail),
                                                      (int)Math.Abs(coords.Z/map.distanceMapDetail)];

                    staticObject = map.staticObjectList[dmValue.objIndex];
                    best = dmValue.sdfValue;
                }

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
                test = map.BVH.Test(testPos, best.distance, false, out Object dObj);
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
                    if (obj.isTrigger) continue;

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
                            color = best.color;
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
                        final = best;
                        finalObj = staticObject;
                    }
                    else if(type == ObjHitType.Dynamic)
                    {
                        final = best;
                        finalObj = bestObj;
                    }
                    Vector3 startPos;
                    Vector3 objectNormal;

                    float lightIntensity = 0;

                    if (finalObj.IsTransparent)
                    {
                        if (transparentDepth == 0)
                        {
                            transparentColor = finalObj.Color;

                            foreach(Light light in map.lightList)
                            {
                                if (Vector3.Distance(light.position, testPos) > 750) continue;

                                objectNormal = finalObj.SDF_normal(testPos);
                                startPos = testPos+objectNormal*2;
                                
                                float addIntensity = LightRayMarch(startPos, light);
                                if(addIntensity > 0)
                                {
                                    addIntensity += 0.2f*(float)Math.Pow(SpecularHighlight(ray, testPos, objectNormal, light.position), 2);
                                }

                                lightIntensity += addIntensity;

                                lightIntensity = Math.Max(lightIntensity, 0.05f); // try around something
                                Color addColor = (final.color.ToVector3() * light.color.ToVector3() * lightIntensity).ToColor();

                                transparentColor = new Color(color.R+addColor.R,
                                                            color.G+addColor.G,
                                                            color.B+addColor.B);
                            }
                        }

                        testPos += ray.direction*transparentStep;
                        transparentDepth += 1;
                        iter -= 1;
                        continue;
                    }

                    foreach(Light light in map.lightList)
                    {
                        if (Vector3.Distance(light.position, testPos) > 750) continue;

                        objectNormal = finalObj.SDF_normal(testPos);
                        startPos = testPos+objectNormal*2;
                        
                        float addIntensity = LightRayMarch(startPos, light);
                        if(addIntensity > 0)
                        {
                            addIntensity += 0.2f*(float)Math.Pow(SpecularHighlight(ray, testPos, objectNormal, light.position), 2);
                        }

                        lightIntensity += addIntensity;

                        lightIntensity = Math.Max(lightIntensity, 0.05f); // try around something
                        Color addColor = (final.color.ToVector3() * light.color.ToVector3() * lightIntensity).ToColor();

                        color = new Color(color.R+addColor.R,
                                          color.G+addColor.G,
                                          color.B+addColor.B);

                        if (transparentDepth > 0)
                        {
                            Vector3 c = color.ToVector3();
                            float T = (float)Math.Exp(-(transparentStep*transparentDepth) * 0.075f); 
                            Vector3 _c = c * T + transparentColor.ToVector3() * (1-T);
                            color = _c.ToColor();
                        }

                        /* if (gammaEnabled) */
                        /* { */
                        /*     Vector3 v = color.ToVector3(); */
                        /*     Vector3 corrected = new Vector3((float)Math.Pow(v.X, 1f/2.2f), */
                        /*                                     (float)Math.Pow(v.Y, 1f/2.2f), */
                        /*                                     (float)Math.Pow(v.Z, 1f/2.2f)); */
                        /*     color = corrected.ToColor(); */
                        /* } */
                    }

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
            return Math.Max(Vector3.Dot(Vector3.Normalize(lightPos - objectHitPos), reflectionViewRay.direction), 0f);
        }

        /* float ambientOcclusion (float3 pos, float3 normal) */
        /* { */
        /*     float sum = 0; */
        /*     for (int i = 0; i < _AOSteps; i ++) */
        /*     { */
        /*         float3 p = pos + normal * (i+1) * _AOStepSize; */
        /*         sum    += map(p); */
        /*     } */
        /*     return sum / (_AOStep * _AOStepSize); */
        /* } */

        const int AOSteps = 10;
        /* const float AOStepSize = 5f; */
        private static float AmbientOcclusion(Vector3 pos, Vector3 normal)
        {
            float occ = 0f;
            float sca = 1f;
            for (int i = 0; i < AOSteps; i++)
            {
                float h = 2 + 2 * i/4;
                Vector3 test = pos + normal * h;
                PhysicsRayMarch(new Ray(test, test-pos), 1, -1, out float dist, out Vector3 _, out Object _);
                occ += (h-dist)*sca;
                sca *= 0.95f;
            }

            return Math.Clamp(1 - 3*occ, 0, 1);
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

                if((int)(coords.X/map.distanceMapDetail) >= map.distanceMap.GetLength(0) ||
                   (int)(coords.Y/map.distanceMapDetail) >= map.distanceMap.GetLength(1) ||
                   (int)(coords.Z/map.distanceMapDetail) >= map.distanceMap.GetLength(2) || 
                   (int)(coords.X/map.distanceMapDetail) < 0 ||
                   (int)(coords.Y/map.distanceMapDetail) < 0 ||
                   (int)(coords.Z/map.distanceMapDetail) < 0)
                {
                }
                else
                {
                    DMValue dmValue = map.distanceMap[(int)Math.Abs(coords.X/map.distanceMapDetail),
                                                      (int)Math.Abs(coords.Y/map.distanceMapDetail),
                                                      (int)Math.Abs(coords.Z/map.distanceMapDetail)];

                    if (!map.staticObjectList[dmValue.objIndex].IsTransparent)
                    {
                        best = dmValue.sdfValue;
                    }
                }

                test = map.BVH.Test(testPos, best.distance, false, out Object dObj);
                if(test.distance < best.distance &&  !dObj.IsTransparent)
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
                    if (obj.isTrigger || obj.IsTransparent) continue;

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

                // needed for precision
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

                test = map.BVH.Test(testPos, best.distance, true, out Object dObj);
                if(test.distance < best.distance)
                {
                    best = test;
                    hitObj = dObj;
                    portalHit = false;
                }

                foreach(PhysicsObject pObj in map.physicsObjectsList)
                {
                    if (pObj == caller || pObj.isTrigger) continue;

                    test = pObj.SDF(testPos, best.distance, physics:true);
                    if(test.distance < best.distance)
                    {
                        best = test;
                        hitObj = pObj;
                        portalHit = false;
                    }
                }

                if (Player.instance.model != caller)
                {
                    test = Player.instance.model.SDF(testPos, best.distance, physics:true);
                    if(test.distance < best.distance)
                    {
                        best = test;
                        hitObj = Player.instance.model;
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
