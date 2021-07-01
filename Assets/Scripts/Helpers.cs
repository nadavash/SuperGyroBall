using UnityEngine;

public static class Helpers
{
    public static Quaternion GyroToUnity(Quaternion q)
    {
        return new Quaternion(q.x, q.y, -q.z, -q.w);
    }

    public static Vector3 GyroToUnityVector(Vector3 v)
    {
        return new Vector3(v.x, v.y, -v.z);
    }

    public static Quaternion EliminateZRotation(Quaternion q)
    {
        return Quaternion.Euler(0, 0, -q.eulerAngles.z) * q;
    }
}
