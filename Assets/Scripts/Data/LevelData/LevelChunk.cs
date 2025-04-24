using ProtoBuf;
using System.Collections.Generic;

namespace Tag.NutSort
{
    [ProtoContract]
    public class LevelChunk
    {
        [ProtoMember(1)] public List<LevelData> Levels;

        public LevelChunk()
        {
            Levels = new List<LevelData>();
        }
    }
}
