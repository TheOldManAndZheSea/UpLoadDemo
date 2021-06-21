using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace UploadCommon.Unitity
{
    public class XmlSerializeHelper
    {

        /// <summary>
        /// 保存实体对象到xml文件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="xmlPath"></param>
        public static void Serialize<T>(T obj,string xmlPath)
        {
            try
            {

                if (obj == null)
                    throw new ArgumentNullException("obj");

                var ser = new XmlSerializer(obj.GetType());
                StreamWriter streamWriter = new StreamWriter(xmlPath);
                ser.Serialize(streamWriter,obj);
                streamWriter.Close();
                //using (var ms = new MemoryStream())
                //{
                //    using (var writer = new XmlTextWriter(ms, Encoding.UTF8))
                //    {
                //        writer.Formatting = Formatting.Indented;
                //        ser.Serialize(writer, obj);
                //    }
                //    var xml = Encoding.UTF8.GetString(ms.ToArray());
                //    xml = xml.Replace("xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"", "");
                //    xml = xml.Replace("xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"", "");
                //    return xml;
                //}
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 反序列化xml字符为对象，默认为Utf-8编码
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="xml"></param>
        /// <returns></returns>
        public static T DeSerialize<T>(string xml)
            where T : new()
        {
            return DeSerialize<T>(xml, Encoding.UTF8);
        }

        /// <summary>
        /// 反序列化xml字符为对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="xml"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static T DeSerialize<T>(string xml, Encoding encoding)
            where T : new()
        {
            try
            {
                var mySerializer = new XmlSerializer(typeof(T));
                using (var ms = new MemoryStream(encoding.GetBytes(xml)))
                {
                    using (var sr = new StreamReader(ms, encoding))
                    {
                        return (T)mySerializer.Deserialize(sr);
                    }
                }
            }
            catch (Exception e)
            {
                return default(T);
            }

        }
    }
}
