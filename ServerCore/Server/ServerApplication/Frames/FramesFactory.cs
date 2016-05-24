using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace ServerApplication.Frames
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
        public static IFrame CreateObject(string xml)
        {

            var reader = new System.IO.StringReader(xml);
            var serializer = new XmlSerializer(typeof(IFrame));
            return serializer.Deserialize(reader) as IFrame;
        }

        /// <summary>
        /// Create serialized object as a single xml string line.
        /// </summary>
        /// <param name="frame"></param>
        /// <returns>
        /// Single string line.
        /// </returns>
        public static string CreateXmlMessage(IFrame frame)
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
