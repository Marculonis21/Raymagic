using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Raymagic
{
    public class PhysicsSpace
    {
        List<PhysicsObject> objects;
        PhysicsSolver solver;

        public PhysicsSpace(List<PhysicsObject> spaceObjects)
        {
            this.objects = spaceObjects;
            this.solver = new PhysicsSolver();
        }

        public PhysicsSpace() : this(new List<PhysicsObject>()) 
        {
        }

        public void Update(float dt)
        {
            solver.Solve(dt, objects);
        }
    }
}
