using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UploadCommon.XmlModel
{
    [Serializable]
    public class UpLoad
    {
        /// <summary>
        /// 文件版本号
        /// </summary>
        public string Version { get; set; }
        /// <summary>
        /// 文件名称
        /// </summary>
        public string FileName { get; set; }
    }
}
