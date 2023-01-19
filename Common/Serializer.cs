using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace Order2VPos.Core.Common
{
    public class Serializer
    {
        public static T Load<T>(string fileName)
        {
            XmlSerializer xs = new XmlSerializer(typeof(T));
            using (FileStream xmlFileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (XmlReader xw = XmlReader.Create(xmlFileStream))
                {
                    T serializerObject = (T)(xs.Deserialize(xw));
                    return serializerObject;
                }
            }
        }

        public static void Save<T>(T serializerObject, string fileName)
        {
            XmlSerializer xs = new XmlSerializer(typeof(T));
            XmlWriterSettings settings = new XmlWriterSettings { NewLineHandling = NewLineHandling.Entitize, Indent = true, Encoding = Encoding.Unicode };

            using (XmlWriter sw = XmlWriter.Create(fileName, settings))
            {
                xs.Serialize(sw, serializerObject);
                sw.Flush();
            }
        }

    }
}
