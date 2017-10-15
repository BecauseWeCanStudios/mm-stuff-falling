using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace stuff_falling
{
    public static class Model {

        public enum Forces { Archimedes, Drag, Viscosity }

        public class Parameters
        {
            public List<Forces> Forces = new List<Forces>();
            public double Height;
            public double Speed;
            public double EndTime;
            public double SegmentCount;
            public bool IsConstGravitationalAcceleration;
            public double SphereRadius;
            public double SphereMass;
            public double EnviromentDensity;
            public double EnviromentViscosity;
        }

        public class Result : EventArgs
        {
            public List<double> Height = new List<double>();
            public List<double> Speed = new List<double>();
            public List<double> Acceleration = new List<double>();
            public List<double> Time = new List<double>();
        }

        private static Func<double, double, double> GetFunc(Parameters parameters)
        {
            Func<double, double> gravity;
            if (parameters.IsConstGravitationalAcceleration)
                gravity = y => -9.81;
            else 
                gravity = y => -9.81 / Math.Pow(1 - y / 6371000, 2);
            Func<double, double> archimedes = (y) => 0;
            Func<double, double> drag = (v) => 0;
            double sphereVolume = 4 / 3 * Math.PI * Math.Pow(parameters.SphereRadius, 3);
            double sphereDensity = parameters.SphereMass / sphereVolume;
            foreach (var force in parameters.Forces)
            {
                switch(force)
                {
                    case Forces.Archimedes:
                        if (parameters.IsConstGravitationalAcceleration)
                            archimedes = y => parameters.EnviromentDensity / sphereDensity * 9.81;
                        else 
                            archimedes = y => parameters.EnviromentDensity / sphereDensity * 9.81 / Math.Pow(1 - y / 6371000, 2);
                        break;
                    case Forces.Drag:
                        drag = v => parameters.EnviromentViscosity / parameters.SphereMass * v * v;
                        break;
                    case Forces.Viscosity:
                        drag = v => parameters.EnviromentViscosity / parameters.SphereMass * v;
                        break;
                    default:
                        throw new Exception("How did you get here?!?!?!?!");
                }
            }
            return (y, v) => gravity(y) + archimedes(y) + drag(v);
        }

        public static event EventHandler<Result> CalculationCompleted;

        private static void Calculate(Parameters parameters)
        {
            Func<double, double, double> func = GetFunc(parameters);
            Result result = new Result();
            result.Height.Add(parameters.Height);
            result.Speed.Add(parameters.Speed);
            result.Time.Add(0);
            double dt = parameters.EndTime / parameters.SegmentCount;
            bool onGround = false;
            for (int i = 1; i <= parameters.SegmentCount; ++i)
            {
                result.Time.Add(i * dt);
                if (onGround)
                {
                    result.Speed.Add(0);
                    result.Height.Add(0);
                    result.Acceleration.Add(0);
                    continue;
                }
                result.Speed.Add(result.Speed.Last() + dt * func(result.Height.Last(), result.Speed.Last()));
                result.Height.Add(result.Height.Last() + dt * result.Speed.Last());
                result.Acceleration.Add((result.Speed.Last() - result.Speed[result.Speed.Count - 2]) / dt);
                if (result.Height.Last() <= 0)
                {
                    onGround = true;
                    result.Height[result.Height.Count - 1] = 0;
                    result.Speed[result.Speed.Count - 1] = 0;
                }
            }
            CalculationCompleted(null, result);        
        }

        public static void BeginCalculate(Parameters parameters) {
            Thread thred = new Thread(() => Calculate(parameters));
            thred.Start();
        }

    }
}
