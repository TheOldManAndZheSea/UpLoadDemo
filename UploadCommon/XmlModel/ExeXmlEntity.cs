using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UploadCommon.XmlModel
{
    [Serializable]
    public class ExeXmlEntity
    {
        /// <summary>
        /// 监听端口
        /// </summary>
        public int ServerPort { get; set; }

        public List<ExeEntityModel> AllListenExes { get; set; }
    }
}
