using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UploadCommon.XmlModel
{
    [Serializable]
    public class ExeEntityModel
    {
        /// <summary>
        /// 序号
        /// </summary>
        public int Index { get; set; }
        /// <summary>
        /// 需要监听的更新程序的名称
        /// </summary>
        public string ExeName { get; set; }
        /// <summary>
        /// 需要监听的更新程序的路径
        /// </summary>
        public string ExePath { get; set; }
    }
}
