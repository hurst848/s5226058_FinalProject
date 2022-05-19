using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class DVec3
{
    public DVec3() { x = 0; y = 0; z = 0; }
    public DVec3(double _x, double _y, double _z) { x = _x; y = _y; z = _z; }
    public DVec3(Vector3 _inp) { x = _inp.x; y = _inp.y; z = _inp.z; }

    public static double Distance(DVec3 _a, DVec3 _b)
    {
        double a = (_a.x - _b.x) * (_a.x - _b.x);
        double b = (_a.y - _b.y) * (_a.y - _b.y);
        double c = (_a.z - _b.z) * (_a.z - _b.z);

        return Mathf.Sqrt((float)(a + b + c));
    }

    public static DVec3 operator +(DVec3 a, DVec3 b) => new DVec3(a.x + b.x, a.y + b.y, a.z + b.z);
    public static DVec3 operator +(DVec3 a, Vector3 b) => new DVec3(a.x + b.x, a.y + b.y, a.z + b.z);

    [SerializeField]
    public double x;
    [SerializeField]
    public double y;
    [SerializeField]
    public double z;

}
