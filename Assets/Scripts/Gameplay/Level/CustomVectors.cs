using ProtoBuf;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Tag.NutSort
{
    [System.Serializable, ProtoContract]
    public struct CustomVector3
    {
        [HorizontalGroup]
        [ProtoMember(1)] public float x;
        [ProtoMember(2)] public float y;
        [ProtoMember(3)] public float z;

        public CustomVector3(Vector3 vector3)
        {
            x = vector3.x;
            y = vector3.y;
            z = vector3.z;
        }

        public Vector3 GetVector()
        {
            return new Vector3(x, y, z);
        }
    }

    [System.Serializable, ProtoContract]
    public struct CustomVector2
    {
        [HorizontalGroup]
        [ProtoMember(1)] public float x;
        [ProtoMember(2)] public float y;

        public CustomVector2(Vector2 vector2)
        {
            x = vector2.x;
            y = vector2.y;
        }

        public Vector2 GetVector()
        {
            return new Vector2(x, y);
        }
    }
}
