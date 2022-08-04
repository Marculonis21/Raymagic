using System;
using Microsoft.Xna.Framework;
using Extreme.Mathematics;
using Matrix = Extreme.Mathematics.Matrix;

namespace Raymagic
{
    public partial class Portal : Object
    {
        float testFieldSize = 50;

        protected List<IPortalable> objectWatcher = new List<IPortalable>();
        List<float> objectLastDot = new List<float>();

        public void OnFieldEnter()
        {
            foreach (IPortalable item in Map.instance.portalableObjectList)
            {
                if (objectWatcher.Contains(item)) continue;

                if ((this.Position - item.position).Length() < testFieldSize)
                {
                    /* if (otherPortal.objectWatcher.Contains(item)) */
                    /* { */
                    /*     throw new Exception("I thought this would ruin our day - and it did"); */
                    /* } */
                    objectWatcher.Add(item);
                    objectLastDot.Add(Vector3.Dot(Vector3.Normalize(this.Position - item.position), this.normal));
                    Console.WriteLine($"{this.color} obj added {item}");
                }
            }
        }

        public void OnFieldExit()
        {
            List<IPortalable> toRemove = new List<IPortalable>(); 

            foreach (IPortalable item in objectWatcher)
            {
                if ((this.Position - item.position).Length() > testFieldSize+5)
                {
                    toRemove.Add(item);
                    Console.WriteLine($"{this.color} obj removed {item}");
                }
            }

            foreach (var item in toRemove)
            {
                objectLastDot.RemoveAt(objectWatcher.IndexOf(item));
            }

            objectWatcher.RemoveAll(x => toRemove.Contains(x));
        }

        public void CheckTransfer()
        {
            if (this.cooldownCounter > 10)
            {
                this.portalState = State.READY;
                this.cooldownCounter = 0;
            }

            if (this.portalState == State.REACENTLYUSED) 
            {
                this.cooldownCounter += 1;
                return;
            }

            List<IPortalable> toRemove = new List<IPortalable>(); 

            for (int i = 0; i < objectWatcher.Count; i++)
            {
                var thisDot = Vector3.Dot(Vector3.Normalize(this.Position - objectWatcher[i].position), this.normal);
                if (i >= objectLastDot.Count)
                {
                    Console.WriteLine($"it happened {this.color}");
                    break;
                }

                if ((thisDot < 0 && objectLastDot[i] > 0) ||
                    (thisDot > 0 && objectLastDot[i] < 0))
                {
                    var lookDirK = objectWatcher[i].lookDir;
                    var rotB = this.baseChangeMatrixIn.Solve(Vector.Create<double>(lookDirK.X,lookDirK.Y,lookDirK.Z));
                    var newRotK = this.otherPortal.baseChangeMatrixInverse.Solve(rotB);

                    // dabble in momentum !!!
                    /* var velocityK = objectWatcher[i].velocity * this.normal*1.099f; */
                    var velocityK = objectWatcher[i].velocity * this.normal * Map.instance.portalMomentumConstant;
                    var velB = this.baseChangeMatrixIn.Solve(Vector.Create<double>(velocityK.X,velocityK.Y,velocityK.Z));
                    var newVelK = this.otherPortal.baseChangeMatrixInverse.Solve(velB);

                    var translateK = this.Position - objectWatcher[i].position;
                    var transB = this.baseChangeMatrixIn.Solve(Vector.Create<double>(translateK.X,translateK.Y,translateK.Z));
                    var newTransK = this.otherPortal.baseChangeMatrixInverse.Solve(transB);

                    objectWatcher[i].RotateAbsolute(newRotK.ToVector3());
                    objectWatcher[i].TranslateAbsolute(this.otherPortal.Position + this.otherPortal.normal*10 + objectWatcher[i].lookDir * 5 * Vector3.Dot(this.otherPortal.normal, objectWatcher[i].lookDir));
                    objectWatcher[i].SetVelocity(newVelK.ToVector3());

                    this.portalState = State.REACENTLYUSED;
                    this.otherPortal.portalState = State.REACENTLYUSED;

                    objectLastDot.RemoveAt(i);
                    toRemove.Add(objectWatcher[i]);

                    continue;
                }

                objectLastDot[i] = thisDot;
            }

            objectWatcher.RemoveAll(x => toRemove.Contains(x));
        }

        public static Ray TransferRay(Portal IN, Ray ray, Vector3 hit)
        {
            Portal OUT = IN.otherPortal;

            var lookDirK = ray.direction;
            var rotB = IN.baseChangeMatrixIn.Solve(Vector.Create<double>(lookDirK.X,lookDirK.Y,lookDirK.Z));
            var newRotK = OUT.baseChangeMatrixInverse.Solve(rotB);

            var translateK = IN.Position - hit;
            var transB = IN.baseChangeMatrixIn.Solve(Vector.Create<double>(translateK.X,translateK.Y,translateK.Z));
            var newTransK = OUT.baseChangeMatrixInverse.Solve(transB);

            Vector3 rayDir = newRotK.ToVector3();
            // maybe adjust distances, good for tonight
            Ray newRay = new Ray(OUT.Position - newTransK.ToVector3() + OUT.normal*10 + rayDir * 40, rayDir); 

            return newRay;
        }

        public static bool HitObjectIsActivePortal(Object hit)
        {
            return ((hit == Map.instance.portalList[0] && Map.instance.portalList[0].otherPortal != null) ||
                    (hit == Map.instance.portalList[1] && Map.instance.portalList[1].otherPortal != null));
        }

    }
}
