        /* public void RayMarch(Vector3 position, Vector3 dir, out float length, out Color color) */
        /* { */
        /*     color = Color.Black; */
        /*     length = float.MaxValue; */

        /*     dir.Normalize(); */

        /*     Vector3 testPos = position; */
        /*     const int maxSteps = 100; */
        /*     for (int iter = 0; iter < maxSteps; iter++) */
        /*     { */
        /*         bool sObj = true; */

        /*         Vector3 coords = testPos - map.mapOrigin; */

        /*         if((int)(coords.X/map.distanceMapDetail) >= map.distanceMap.GetLength(0) || */
        /*            (int)(coords.Y/map.distanceMapDetail) >= map.distanceMap.GetLength(1) || */
        /*            (int)(coords.Z/map.distanceMapDetail) >= map.distanceMap.GetLength(2) || */ 
        /*            (int)(coords.X/map.distanceMapDetail) < 0 || */
        /*            (int)(coords.Y/map.distanceMapDetail) < 0 || */
        /*            (int)(coords.Z/map.distanceMapDetail) < 0) */
        /*         { */
        /*             return; */
        /*         } */

        /*         float dst = map.distanceMap[(int)Math.Abs(coords.X/map.distanceMapDetail), */
        /*                                     (int)Math.Abs(coords.Y/map.distanceMapDetail), */
        /*                                     (int)Math.Abs(coords.Z/map.distanceMapDetail)]; */

        /*         Object bestDObj = null; */
        /*         float test = map.BVH.Test(testPos, dst, out Object dObj); */
        /*         if(test < dst) */
        /*         { */
        /*             dst = test; */
        /*             bestDObj = dObj; */
        /*             sObj = false; */
        /*         } */

        /*         foreach(Object iObj in map.infoObjectList) */
        /*         { */
        /*             test = iObj.SDF(testPos, dst); */
        /*             if(test < dst) */
        /*             { */
        /*                 dst = test; */
        /*                 if(dst < 0.1f) */
        /*                 { */
        /*                     color = Color.Red; */
        /*                     return; */
        /*                 } */
        /*             } */
        /*         } */

        /*         if(dst < 0.1f) */
        /*         { */
        /*             float bestDst = float.MaxValue; */
        /*             Color bestColor = color; */
        /*             Object bestObj = null; */ 

        /*             if(sObj) */
        /*             { */
        /*                 foreach(Object obj in map.staticObjectList) */
        /*                 { */
        /*                     test = obj.SDF(testPos, dst); */
        /*                     if(test <= bestDst) */
        /*                     { */
        /*                         bestDst = test; */
        /*                         bestColor = obj.Color; */
        /*                         bestObj = obj; */
        /*                     } */
        /*                 } */
        /*             } */
        /*             else */
        /*             { */
        /*                 bestDst = dst; */
        /*                 bestColor = bestDObj.Color; */
        /*                 bestObj = bestDObj; */
        /*             } */

        /*             Vector3 startPos; */
        /*             float lightIntensity = 0; */
        /*             foreach(Light light in map.lightList) */
        /*             { */
        /*                 startPos = testPos+bestObj.SDF_normal(testPos)*2; */
                        
        /*                 lightIntensity += LightRayMarch(startPos, light); */
        /*             } */

        /*             lightIntensity = Math.Max(lightIntensity, 0.0001f); // try around something */
        /*             color = new Color(bestColor.R*lightIntensity, */
        /*                               bestColor.G*lightIntensity, */
        /*                               bestColor.B*lightIntensity); */

        /*             return; */
        /*         } */

        /*         testPos += dir*dst; */
        /*     } */
        /* } */

        /* public float LightRayMarch(Vector3 position, Light light) */
        /* { */
        /*     Vector3 dir = (light.position - position); */
        /*     dir.Normalize(); */

        /*     float length = 1f; */
        /*     float test; */
        /*     float intensity = light.intensity; */
        /*     float k = 16f*intensity; */
        /*     while(length < (position - light.position).Length() - 0.1f) */
        /*     { */
        /*         Vector3 coords = position + dir*length - map.mapOrigin; */

        /*         float dst = map.distanceMap[(int)Math.Abs(coords.X/map.distanceMapDetail), */
        /*                                     (int)Math.Abs(coords.Y/map.distanceMapDetail), */
        /*                                     (int)Math.Abs(coords.Z/map.distanceMapDetail)]; */

        /*         test = map.BVH.Test(position + dir*length, dst, out Object dObj); */
        /*         if(test < dst) */
        /*         { */
        /*             dst = test; */
        /*         } */

        /*         test = light.SDF(position + dir*length); */
        /*         if(test < dst) */
        /*         { */
        /*             break; */
        /*         } */

        /*         if( dst<0.01f ) */
        /*             return 0.0f; */

        /*         intensity = Math.Min(intensity, k*dst/length); */
        /*         if(intensity < 0.001f) */
        /*             return 0.0f; */

        /*         length += dst; */
        /*     } */
        /*     length = (position - light.position).Length(); */
        /*     return intensity/(length*length); */
        /* } */

        /* public void PhysicsRayMarch(Vector3 position, Vector3 dir, int maxSteps, float stepMinSize, out float length, out Vector3 hit, out Object hitObj) */
        /* { */
        /*     hit = position; */
        /*     length = float.MaxValue; */
        /*     hitObj = null; */

        /*     dir.Normalize(); */

        /*     Vector3 testPos = position; */
        /*     float test; */
        /*     float dst; */
        /*     for (int iter = 0; iter < maxSteps; iter++) */
        /*     { */
        /*         dst = float.MaxValue; */
        /*         foreach(Object obj in map.staticObjectList) */
        /*         { */
        /*             test = obj.SDF(testPos, dst); */
        /*             if(test < dst) */
        /*             { */
        /*                 dst = test; */
        /*                 hitObj = obj; */
        /*             } */
        /*         } */

        /*         foreach(Object dObj in map.dynamicObjectList) */
        /*         { */
        /*             test = dObj.SDF(testPos, dst, physics:true); */
        /*             if(test < dst) */
        /*             { */
        /*                 dst = test; */
        /*                 hitObj = dObj; */
        /*             } */
        /*         } */

        /*         if(dst <= stepMinSize) */
        /*         { */
        /*             if(dst < 0) */
        /*                 length = -1; */
        /*             else */
        /*                 length = (position - testPos).Length(); */ 
        /*             hit = testPos; */
        /*             return; */
        /*         } */

        /*         testPos += dir*dst; */
        /*     } */

        /*     length = (position - testPos).Length(); */
        /*     hit = testPos; */
        /* } */
