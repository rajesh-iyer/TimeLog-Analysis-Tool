using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ServiceCenter.Framework
{
    public static class SerializationUtil
    {
        public static string Serialize<T>(this T toSerialize)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(toSerialize.GetType());
            StringWriter textWriter = new StringWriter();

            xmlSerializer.Serialize(textWriter, toSerialize);
            return textWriter.ToString();
        }

        public static T Deserialize<T>(this string xml)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            using (TextReader reader = new StringReader(xml))
            {
                return (T)serializer.Deserialize(reader);
            }
        }

        public static T DeserializeFile<T>(this string filename)
        {
            XmlSerializer _xmlSerializer = new XmlSerializer(typeof(T));
            using (Stream stream = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                var result = (T)_xmlSerializer.Deserialize(stream);
                return result;
            }
        }
    }
}
