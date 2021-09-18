using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Raymagic
{
    //copy from main game draw function
    public class IDEA
    {
        public void Update()
        {
            /* watch = new Stopwatch(); */
            /* watch.Start(); */
            /* const int SUBHARDLIMIT = 5; */
            /* DPPList.Clear(); */

            /* DrawPlanePart parent = new DrawPlanePart(new Point(0,0),winWidth); */
            /* DrawPlanePart[] subs = parent.Subdivide(); */
            /* for(int i = 0; i < subs.Length; i++) */
            /* { */
            /*     DrawPlanePart[] subs2 = subs[i].Subdivide(); */

            /*     for(int i2 = 0; i2 < subs2.Length; i2++) */
            /*     { */
            /*         DrawPlanePart[] subs3 = subs2[i2].Subdivide(); */

            /*         for(int i3 = 0; i3 < subs3.Length; i3++) */
            /*         { */
            /*             DrawPlanePart[] subs4 = subs3[i3].Subdivide(); */

            /*             for(int i4 = 0; i4 < subs4.Length; i4++) */
            /*             { */
            /*                 DrawPlanePart[] subs5 = subs4[i4].Subdivide(); */

            /*                 for(int i5 = 0; i5 < subs5.Length; i5++) */
            /*                 { */
            /*                     DrawPlanePart[] subs6 = subs5[i5].Subdivide(); */
            /*                     DPPList.AddRange(subs6); */
            /*                 } */
            /*             } */
            /*         } */
            /*     } */
            /* } */


            /* int passCount = 3; */
            /* int[] passRayCount = new int[passCount]; */
            /* long[] inDetailTime = new long[3*passCount]; */
            
            /* Stopwatch dSW = new Stopwatch(); */
            /* for (int iteration = 0; iteration < passCount; iteration++) */
            /* { */
            /*     dSW = new Stopwatch(); */
            /*     dSW.Start(); */
            /*     Parallel.For(0, DPPList.Count, i => */ 
            /*     { */
            /*         var part = DPPList[i]; */
            /*         if(part.isLeaf && !part.safe) */
            /*         { */
            /*             Vector3 rayDir = (player.position + playerLookDir*zoom + playerLookPerpenSIDE*part.centerPartPos.X + playerLookPerpenUP*part.centerPartPos.Y) - player.position; */

            /*             if(RayMarch(player.position, rayDir, out float length, out Color color)) */
            /*             { */
            /*                 part.color = color; */
            /*                 passRayCount[iteration]++; */
            /*             } */
            /*         } */
            /*     }); */
            /*     dSW.Stop(); */
            /*     inDetailTime[0 + 3*iteration] = dSW.ElapsedMilliseconds; */

            /*     dSW = new Stopwatch(); */
            /*     dSW.Start(); */
            /*     int iterCount = DPPList.Count; */
            /*     Parallel.For(0, iterCount, i => */ 
            /*     { */
            /*         var part = DPPList[i]; */

            /*         List<DrawPlanePart> neighborsList = new List<DrawPlanePart>(); */

            /*         List<DrawPlanePart> wNeighb = part.GetNeighbors("w"); */
            /*         foreach(var wPart in wNeighb) */
            /*         { */
            /*             if(wPart.color != Color.Black) */
            /*                 neighborsList.Add(wPart); */
            /*         } */
            /*         List<DrawPlanePart> sNeighb = part.GetNeighbors("s"); */
            /*         foreach(var sPart in sNeighb) */
            /*         { */
            /*             if(sPart.color != Color.Black) */
            /*                 neighborsList.Add(sPart); */
            /*         } */
            /*         List<DrawPlanePart> aNeighb = part.GetNeighbors("a"); */
            /*         foreach(var aPart in aNeighb) */
            /*         { */
            /*             if(aPart.color != Color.Black) */
            /*                 neighborsList.Add(aPart); */
            /*         } */
            /*         List<DrawPlanePart> dNeighb = part.GetNeighbors("d"); */
            /*         foreach(var dPart in dNeighb) */
            /*         { */
            /*             if(dPart.color != Color.Black) */
            /*                 neighborsList.Add(dPart); */
            /*         } */

            /*         if(neighborsList.Count > 0) */
            /*         { */
            /*             Vector3 colorAvg = new Vector3(); */
            /*             foreach(var neighb in neighborsList) */
            /*             { */
            /*                 colorAvg += new Vector3(neighb.color.R, */
            /*                                         neighb.color.G, */
            /*                                         neighb.color.B); */
            /*             } */
            /*             colorAvg /= neighborsList.Count; */

            /*             float avgDist = (Math.Abs(colorAvg.X-part.color.R) + Math.Abs(colorAvg.Y-part.color.G) + Math.Abs(colorAvg.Z-part.color.B)) / 3f; */
            /*             part.dst = avgDist; */

            /*             if(avgDist <= detailSize/40f || part.sizeWH < 8) */
            /*             { */
            /*                 part.safe = true; */
            /*             } */
            /*             else */
            /*             { */
            /*                 part.safe = false; */
            /*             } */
            /*         } */
            /*         else // black = fully in shadow */
            /*         { */
            /*             part.dst = 0; */
            /*             part.safe = true; */
            /*         } */
            /*     }); */
            /*     dSW.Stop(); */
            /*     inDetailTime[1 + 3*iteration] = dSW.ElapsedMilliseconds; */

            /*     if(iteration == passCount-1) break; */
            /*     dSW = new Stopwatch(); */
            /*     dSW.Start(); */
            /*     for(int i = 0; i < iterCount; i++) */
            /*     { */
            /*         var part = DPPList[i]; */
            /*         if(!part.safe) */
            /*         { */
            /*             DPPList.AddRange(part.Subdivide()); */
            /*         } */
            /*     }; */
            /*     dSW.Stop(); */
            /*     inDetailTime[2 + 3*iteration] = dSW.ElapsedMilliseconds; */
            /* } */

            /* watch.Stop(); */
            /* Informer.instance.AddInfo("debug quadtree", $" quadtree total phase: {watch.ElapsedMilliseconds}"); */
            /* Informer.instance.AddInfo("debug quadtree pass1", $" quadtree RM pass1: {passRayCount[0]}"); */
            /* Informer.instance.AddInfo("debug quadtree t1", $"  t1: {inDetailTime[0]}"); */
            /* Informer.instance.AddInfo("debug quadtree t2", $"  t2: {inDetailTime[1]}"); */
            /* Informer.instance.AddInfo("debug quadtree t3", $"  t3: {inDetailTime[2]}"); */
            /* Informer.instance.AddInfo("debug quadtree pass2", $" quadtree RM pass2: {passRayCount[1]}"); */
            /* Informer.instance.AddInfo("debug quadtree t12", $"  t1: {inDetailTime[3]}"); */
            /* Informer.instance.AddInfo("debug quadtree t22", $"  t2: {inDetailTime[4]}"); */
            /* Informer.instance.AddInfo("debug quadtree t32", $"  t3: {inDetailTime[5]}"); */
            /* Informer.instance.AddInfo("debug quadtree pass3", $" quadtree RM pass3: {passRayCount[2]}"); */
            /* Informer.instance.AddInfo("debug quadtree t13", $"  t1: {inDetailTime[6]}"); */
            /* Informer.instance.AddInfo("debug quadtree t23", $"  t2: {inDetailTime[7]}"); */
            /* Informer.instance.AddInfo("debug quadtree t33", $"  t3: {inDetailTime[8]}"); */


            /* watch = new Stopwatch(); */
            /* watch.Start(); */
            /* shapes.Begin(); */
            /* foreach(var part in DPPList) */
            /* { */
            /*     if(part.isLeaf) */
            /*     { */
            /*         shapes.DrawRectangle(new Point(winWidth/2 + part.centerPartPos.X - part.sizeWH/2, winHeight/2+part.centerPartPos.Y - part.sizeWH/2), */ 
            /*                              part.sizeWH,part.sizeWH, */ 
            /*                              part.color); */
            /*     } */
            /* } */

            /* shapes.End(); */
            /* shapes.Begin(); */
            /* foreach(var part in DPPList) */
            /* { */
            /*         /1* shapes.DrawText(".", this.font, new Vector2(winWidth/2 + part.centerPartPos.X,winHeight/2+part.centerPartPos.Y), Color.Red); *1/ */
            /*     if(Keyboard.GetState().IsKeyDown(Keys.Enter)) */
            /*     { */
            /*         if(part.safe) */
            /*             shapes.DrawBorder(new Point(winWidth/2, winHeight/2) + part.centerPartPos - new Point(part.sizeWH/2,part.sizeWH/2), part.sizeWH, part.sizeWH, 1, Color.LimeGreen); */
            /*         else */
            /*             shapes.DrawBorder(new Point(winWidth/2, winHeight/2) + part.centerPartPos - new Point(part.sizeWH/2,part.sizeWH/2), part.sizeWH, part.sizeWH, 1, Color.Red); */
            /*     } */

            /*     /1* if(part.isLeaf && !part.safe) *1/ */
            /*     /1* { *1/ */
            /*     /1*     shapes.DrawText(Math.Round(part.dst,3).ToString(), this.font, new Vector2(winWidth/2 + part.centerPartPos.X,winHeight/2+part.centerPartPos.Y), Color.Red); *1/ */
            /*     /1* } *1/ */
            /* } */
            /* shapes.End(); */

            /* watch.Stop(); */
            /* Informer.instance.AddInfo("debug quadtree draw", $" quadtree draw phase: {watch.ElapsedMilliseconds}"); */

            /* Informer.instance.AddInfo("count", DPPList.Count.ToString()); */
        }
    }
}
