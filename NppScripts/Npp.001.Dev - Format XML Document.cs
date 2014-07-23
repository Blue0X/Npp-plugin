using System;
using System.Windows.Form;
using System.IO;
using System.Linq;
using System.Xml;
using NppScripts;

public class Script : NppScript {

    public override void Run() {
        try {
            string text = Npp.GetAllText();
            Npp.SetAllText(FormatXML(text));
        }
        catch(Exception e) {
            MessageBox.Show("'Format XML Document' Error:\r\n" + e.Message);
        }
    }

    string FormatXML(string data) {
        var xmlDocument = new XmlDocument();
        xmlDocument.LoadXml(data);

        using(var stringWriter = new StringWriter())
        using (var xmlWriter = new XmlTextWriter(stringWriter) { Formatting = Formatting.Indented, Indentation = 4 })
        {
            xmlDocument.WriteTo(xmlWriter);
            return stringWriter.ToString();
        }
    }
}
