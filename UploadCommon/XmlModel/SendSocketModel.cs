using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UploadCommon.XmlModel
{
    public class SendSocketModel
    {
        /// <summary>
        /// 类型 0获取配置信息 1获取长度 2发送字节流 3文件未找到 4未知错误
        /// </summary>
        public string type { get; set; }
        public object socketdata { get; set; }
        /// <summary>
        /// 服务器的下载地址
        /// </summary>
        public string filepath { get; set; }
    }
}
