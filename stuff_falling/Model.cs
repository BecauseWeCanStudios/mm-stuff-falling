using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace stuff_falling
{
    public static class Model {

        public enum Forces { Gravity, Archimedes, Drag, Viscosity }

        public struct Properties
        {
            public List<Forces> Forces;
            public double Height;
            public double Mass;
            public double EndTime;
            public double SegmentCount;
            public bool IsConstGravitationalAcceleration;
            public double Volume;
            public double ObjectDensity;
            public double EnviromentDensity;
            public double ObjectRadius;
            public double EnviromentViscosity;
            public double DragCoefficient;
        }

        public struct Result
        {
            public List<double> Height;
            public List<double> Speed;
            public List<double> Time;
        }

        public static void Calculate(Properties properties) {
            //do stuff
        }

    }
}
