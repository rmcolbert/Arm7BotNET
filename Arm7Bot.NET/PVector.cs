using System;

namespace Arm7BotNET
{
    public class PVector
    {
        public double x, y, z;
        public PVector()
        {
            x = y = z = 0.0;
        }

        public PVector(double _x, double _y, double _z)
        {
            x = _x; y = _y; z = _z;
        }

        public void add(PVector p)
        {
            x += p.x;
            y += p.y;
            z += p.z;
        }

        public void normalize()
        {
            double l = Math.Sqrt(x * x + y * y + z * z);
            x /= l;
            y /= l;
            z /= l;
        }

        public double dot(PVector p)
        {
            return x * p.x + y * p.y + z * p.z;
        }

        public double dist(PVector p)
        {
            double dist_x = x - p.x;
            double dist_y = y - p.y;
            double dist_z = z - p.z;
            return Math.Sqrt(dist_x * dist_x + dist_y * dist_y + dist_z * dist_z);
        }

        public PVector mult(float num)
        {
            double x2 = x * num;
            double y2 = y * num;
            double z2 = z * num;
            return new PVector(x2, y2, z2);
        }


    }
}
