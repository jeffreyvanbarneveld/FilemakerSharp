﻿using FilemakerSharp.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace FilemakerSharp.Commands
{
    public class EditRecordCommand : IEditRecordCommand
    {
        private Filemaker m_fm;
        private string m_layout;
        private int m_recordID;

        private Dictionary<string, string> m_values = new Dictionary<string, string>();

        private string m_afterScript = null;
        private string m_afterScriptParameter = null;

        internal EditRecordCommand(Filemaker fm, string layout, int recordID)
        {
            m_fm = fm;
            m_layout = layout;
            m_recordID = recordID;
        }

        /// <summary>
        /// Clear fields
        /// </summary>
        public void Clear()
        {
            m_values.Clear();
        }

        /// <summary>
        /// Add field to create
        /// </summary>
        /// <param name="name">Field name</param>
        /// <param name="value">Field value</param>
        public void AddField(string name, string value)
        {
            string ut;
            if (!m_values.TryGetValue(name, out ut))
                m_values.Add(name, value);
        }

        /// <summary>
        /// Add fields to create
        /// </summary>
        /// <param name="fields">The fields</param>
        public void AddFields(Dictionary<string, string> fields)
        {
            foreach (KeyValuePair<string, string> field in fields)
            {
                string ut;


                if (!m_values.TryGetValue(field.Key, out ut))
                    m_values.Add(field.Key, field.Value);
            }
        }

        /// <summary>
        /// Add script after execution
        /// </summary>
        /// <param name="name">Scriptname to execute</param>
        public void AddScript(string name)
        {
            m_afterScript = name;
        }

        /// <summary>
        /// Add script after execution with parameter
        /// </summary>
        /// <param name="name">Scriptname to execute</param>
        /// <param name="parameter">Script parameter</param>
        public void AddScript(string name, string parameter)
        {
            m_afterScript = name;
            m_afterScriptParameter = parameter;
        }

        private string GenerateUrl()
        {
            string extra = "";

            foreach (KeyValuePair<string, string> field in m_values)
                extra += "&" + Uri.EscapeDataString(field.Key == null ? "" : field.Key) + "=" + Uri.EscapeDataString(field.Value == null ? "" : field.Value);

            if (m_afterScript != null)
                extra += "&-script=" + Uri.EscapeDataString(m_afterScript);

            if (m_afterScriptParameter != null)
                extra += "&-script.param=" + Uri.EscapeDataString(m_afterScriptParameter);

            return "-lay=" + m_layout + "&-recid=" + m_recordID + "&-edit" + extra;
        }

        /// <summary>
        /// Execute :0
        /// </summary>
        public async Task<int> Execute()
        {

            XmlNode xmlResponse = await m_fm.Communicate(GenerateUrl(), true);

            int recordID = -1;


            foreach (XmlNode rootNode in xmlResponse.ChildNodes)
            {
                switch (rootNode.Name)
                {
                    case "error":
                        string theError = rootNode.Attributes.GetNamedItem("code").Value;
                        if (theError != "0")
                        {
                            throw new FilemakerException("FileMaker Server returned an error in retrieving the XML.  Error: " + theError);
                        }
                        break;

                    case "resultset":

                        foreach (XmlNode record in rootNode.ChildNodes)
                        {
                            foreach (XmlAttribute attribute in record.Attributes)
                            {
                                switch (attribute.Name)
                                {
                                    case "record-id":
                                        recordID = int.Parse(attribute.Value);
                                        break;
                                }
                            }
                        }
                        break;

                    case "metadata":
                        // TODO: parse meta data
                        break;
                }
            }

            return recordID;
        }


    }
}
