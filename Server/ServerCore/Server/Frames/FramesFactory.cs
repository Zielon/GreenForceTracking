﻿using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace Server.Frames
{
    public class FramesFactory
    {
        /// <summary>
        ///     Deserialize object.
        /// </summary>
        /// <param name="xml"></param>
        /// <returns>
        ///     Type of a template which implements an IFrame interface.
        /// </returns>
        public static T CreateObject<T>(string xml) where T : IFrame
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            using (TextReader reader = new StringReader(xml)) { return (T) serializer.Deserialize(reader); }
        }

        /// <summary>
        ///     Create an serialized object as a single xml string line.
        /// </summary>
        /// <param name="frame"></param>
        /// <returns>
        ///     Single string line.
        /// </returns>
        public static string CreateXmlMessage<T>(T frame) where T : IFrame
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

            return $"<Frame>{sr.ReadToEnd()}</Frame>";
        }
    }
}
