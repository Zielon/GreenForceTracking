using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using ServerApplication.Common;

namespace ServerApplication.Frames.Factory
{
    public class FramesFactory
    {
        /// <summary>
        /// Deserialize object.
        /// </summary>
        /// <param name="xml"></param>
        /// <returns>
        /// Type of IFrame.
        /// </returns>
        public static T CreateObject<T>(string xml)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            T result;
            using (TextReader reader = new StringReader(xml))
            {
                result = (T)serializer.Deserialize(reader);
            }

            return result;
        }

        /// <summary>
        /// Create serialized object as a single xml string line.
        /// </summary>
        /// <param name="frame"></param>
        /// <returns>
        /// Single string line.
        /// </returns>
        public static string CreateXmlMessage<T>(T frame)
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.OmitXmlDeclaration = true;
            MemoryStream ms = new MemoryStream();
            XmlWriter writer = XmlWriter.Create(ms, settings);
            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            ns.Add(string.Empty, string.Empty);
            var serializer = new XmlSerializer(frame.GetType());
            serializer.Serialize(writer, frame, ns);
            ms.Flush();
            ms.Seek(0, SeekOrigin.Begin);
            StreamReader sr = new StreamReader(ms);

            return sr.ReadToEnd();
        }
    }
}
