using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace NodeEditor.Helpers
{
    public static class ObjectHelper
    {
        public static object DeepClone(this object obj)
        {
            object objResult = null;

            using (var ms = new MemoryStream())
            {
                var bf = new BinaryFormatter();
                bf.Serialize(ms, obj);

                ms.Position = 0;
                objResult = bf.Deserialize(ms);
            }

            return objResult;
        }
    }
}