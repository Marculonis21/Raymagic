using System;
using Microsoft.Xna.Framework;
using Extreme.Mathematics;
using Matrix = Extreme.Mathematics.Matrix;

namespace Raymagic
{
    public partial class Portal : Object
    {
        float testFieldSize = 50;

        protected List<Object> objectWatcher = new List<Object>();
        List<float> objectLastDot = new List<float>();

        public void OnFieldEnter()
        {
            foreach (var item in Map.instance.dynamicObjectList)
            {
                if (objectWatcher.Contains(item)) continue;

                if ((this.Position - item.Position).Length() < testFieldSize)
                {
                    if (otherPortal.objectWatcher.Contains(item))
                    {
                        throw new Exception("I thought this would ruin our day - and it did");
                    }
                    objectWatcher.Add(item);
                    objectLastDot.Add(Vector3.Dot(Vector3.Normalize(this.Position - item.Position), this.normal));
                }
            }

            if (!(objectWatcher.Contains(Player.instance.model)))
            {
                if ((this.Position - Player.instance.position).Length() < testFieldSize)
                {
                    if (otherPortal.objectWatcher.Contains(Player.instance.model))
                    {
                        throw new Exception("I thought this would ruin our day - and it did");
                    }
                    objectWatcher.Add(Player.instance.model);
                    objectLastDot.Add(Vector3.Dot(Vector3.Normalize(this.Position - Player.instance.position), this.normal));
                }
            }
        }

        public void OnFieldExit()
        {
            List<Object> toRemove = new List<Object>(); 

            foreach (var item in objectWatcher)
            {
                if ((this.Position - item.Position).Length() > testFieldSize+5)
                {
                    toRemove.Add(item);
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
            List<Object> toRemove = new List<Object>(); 

            for (int i = 0; i < objectWatcher.Count; i++)
            {
                var thisDot = Vector3.Dot(Vector3.Normalize(this.Position - objectWatcher[i].Position), this.normal);
                if ((thisDot < 0 && objectLastDot[i] > 0) ||
                    (thisDot > 0 && objectLastDot[i] < 0))
                {
                    Console.WriteLine($"transfer - {this.type}");
                    if (objectWatcher[i] == Player.instance.model)
                    {
                        /* Player.instance.Rotate(new Vector2((float)(Math.PI),0)); */

                        var lookDirK = Player.instance.lookDir;
                        var rotB = this.baseChangeMatrixIn.Solve(Vector.Create<double>(lookDirK.X,lookDirK.Y,lookDirK.Z));
                        var newRotK = this.otherPortal.baseChangeMatrixInverse.Solve(rotB);
                        Console.WriteLine($"{lookDirK} -> {rotB} -> {newRotK}");

                        Player.instance.RotateAbsolute(newRotK.ToVector3());
                        Player.instance.position = this.otherPortal.Position + Player.instance.lookDir*5;

                    }
                    else
                    {
                        objectWatcher[i].TranslateAbsolute(this.otherPortal.Position + this.otherPortal.normal*20); 
                        objectWatcher[i].Rotate(180, "z");

                    }
                    objectLastDot.RemoveAt(i);
                    toRemove.Add(objectWatcher[i]);
                    continue;
                }

                objectLastDot[i] = thisDot;
            }

            objectWatcher.RemoveAll(x => toRemove.Contains(x));
        }
    }
}
