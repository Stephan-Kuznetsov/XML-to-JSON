using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Xml;

namespace XmlToJsonConverter
{
    class ConverterXmlToJson
    {
        private XmlDocument Source;
        public bool IsBroken = true;
        private int Nodes;
        private string JsonResult;
        private string SavePath = "";
        private string OriginalPath;

        public ConverterXmlToJson(string path)
        {
            OriginalPath = path;
        }
        public void Convert()
        {
            Source = new XmlDocument();
            System.IO.StreamReader sr = new System.IO.StreamReader(OriginalPath);
            try
            {
                Source.LoadXml(sr.ReadToEnd());
                sr.Close();
            }
            catch (XmlException e)
            {
                string mes = e.Message;
                IsBroken = true;
                sr.Close();
                return;
            }
            IsBroken = false;
            Nodes = Source.ChildNodes.Count;
            int ind = OriginalPath.LastIndexOf(".xml");
            SavePath = OriginalPath.Substring(0, ind) + "_converted.json";

            StringBuilder JsonFileMaker = new StringBuilder();
            JsonFileMaker.Append("{ ");
            XmlToJSONnode(JsonFileMaker, Source.DocumentElement, true);
            JsonFileMaker.Append("}");
            JsonResult = JsonFileMaker.ToString();
        }
        private void XmlToJSONnode(StringBuilder Jsonf, XmlElement node, bool showNodeName)
        {
            if (showNodeName)
                Jsonf.Append("\"" + node.Name + "\": ");
            Jsonf.Append("{");

            SortedList childNodeNames = new SortedList();

            if (node.HasAttributes)
            {
                foreach (XmlAttribute attr in node.Attributes)
                    AddToList(childNodeNames, attr.Name, attr.InnerText);
            }

            foreach (XmlNode cnode in node.ChildNodes)
            {
                if (cnode is XmlText)
                    AddToList(childNodeNames, "value", cnode.InnerText);
                else if (cnode is XmlElement)
                    AddToList(childNodeNames, cnode.Name, cnode);

            }

            foreach (string childname in childNodeNames.Keys)
            {
                ArrayList alChild = (ArrayList)childNodeNames[childname];
                if (alChild.Count == 1)
                    WriteConverted(childname, alChild[0], Jsonf, true);
                else
                {
                    Jsonf.Append(" \"" + CheckForWrongSigns(childname) + "\": [ ");
                    foreach (object Child in alChild)
                        WriteConverted(childname, Child, Jsonf, false);
                    Jsonf.Remove(Jsonf.Length - 2, 2);
                    Jsonf.Append(" ], ");
                }
            }
            Jsonf.Remove(Jsonf.Length - 2, 2);
            Jsonf.Append(" }");
        }

        private void AddToList(SortedList childNodeNames, string nodeName, object nodeValue)
        {
            // Pre-process contraction of XmlElement-s
            if (nodeValue is XmlElement)
            {
                // Convert  <aa></aa> into "aa":null
                //          <aa>xx</aa> into "aa":"xx"
                XmlNode cnode = (XmlNode)nodeValue;
                if (cnode.Attributes.Count == 0)
                {
                    XmlNodeList children = cnode.ChildNodes;
                    if (children.Count == 0)
                        nodeValue = null;
                    else if (children.Count == 1 && (children[0] is XmlText))
                        nodeValue = ((XmlText)(children[0])).InnerText;
                }
            }
            // Add nodeValue to ArrayList associated with each nodeName
            // If nodeName doesn't exist then add it
            ArrayList ValuesAL;

            if (!childNodeNames.Contains(nodeName))
            {
                ValuesAL = new ArrayList();
                childNodeNames.Add(nodeName, ValuesAL);
            }
            else
                ValuesAL = (ArrayList)childNodeNames[nodeName];
            ValuesAL.Add(nodeValue);
        }

        private void WriteConverted(string childname, object alChild, StringBuilder JsonFileMaker, bool showNodeName)
        {
            if (alChild == null)
            {
                if (showNodeName)
                    JsonFileMaker.Append("\"" + CheckForWrongSigns(childname) + "\": ");
                JsonFileMaker.Append("null");
            }
            else if (alChild is string)
            {
                if (showNodeName)
                    JsonFileMaker.Append("\"" + CheckForWrongSigns(childname) + "\": ");
                JsonFileMaker.Append("\"" + CheckForWrongSigns(alChild.ToString().Trim()) + "\"");
            }
            else
                XmlToJSONnode(JsonFileMaker, (XmlElement)alChild, showNodeName);
            JsonFileMaker.Append(", ");
        }

        private string CheckForWrongSigns(string s)
        {
            StringBuilder res = new StringBuilder(s.Length);
            foreach (char ch in s)
            {
                if (Char.IsControl(ch) || ch == '\'')
                {
                    int ich = (int)ch;
                    res.Append(@"\u" + ich.ToString("x4"));
                    continue;
                }
                else if (ch == '\"' || ch == '\\' || ch == '/')
                {
                    res.Append('\\');
                }
                res.Append(ch);
            }
            return res.ToString();
        }
        public string SaveResultInJson()
        {
            System.IO.StreamWriter sw = new System.IO.StreamWriter(SavePath);
            sw.Write(JsonResult);
            sw.Close();
            return SavePath;
        }
    }
}
