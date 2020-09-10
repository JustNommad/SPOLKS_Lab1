using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace SPOLKS_Lab1
{
    [Serializable]
    public class DataInfo
    {
        public string ClientName { get; set; }
        public string FileName { get; set; }
        public int FileSize { get; set; }
        public string ProtocolType { get; set; }
        public string DestinationName { get; set; }

        public DataInfo() { }
        public DataInfo(byte[] data)
        {
            DataInfo d = FromArray(data);
            ClientName = d.ClientName;
            FileName = d.ClientName;
            FileSize = d.FileSize;
            DestinationName = d.DestinationName;
        }
        public static DataInfo FromArray(byte[] data)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            using (MemoryStream stream = new MemoryStream(data))
            {
                stream.Position = 0;
                return (DataInfo)formatter.Deserialize(stream);
            }
        }
        public byte[] ToArray()
        {
            BinaryFormatter formatter = new BinaryFormatter();
            using (MemoryStream stream = new MemoryStream())
            {
                formatter.Serialize(stream, this);

                return stream.ToArray();
            }
        }
    }
}
