using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;

namespace ServiceCenter.Framework
{
    public static class XslUtil
    {
        public static string TransformToTimeReporterFormat(this string xml, string transformXslFile)
        {
            XslTransform xslt = new XslTransform();
            xslt.Load(transformXslFile);
            XPathDocument xpath = new XPathDocument(XmlReader.Create(new StringReader(xml)));
            
            XmlReader xmlReader = xslt.Transform(xpath, null);
            if (xmlReader != null)
            {
                StringBuilder sb = new StringBuilder();

                while (xmlReader.Read())
                    sb.AppendLine(xmlReader.ReadOuterXml());

                return sb.ToString();
            }
            return string.Empty;
        }
    }
}
