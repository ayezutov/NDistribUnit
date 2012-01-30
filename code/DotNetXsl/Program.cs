using System;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;

namespace DotNetXsl
{
    class Program
    {
        static void Main(string[] args)
        {

            if (args.Length != 3)
            {
                Console.WriteLine("You have not entered the correct parameters");
                return;
            }

            string xmlfile = args[0];
            string xslfile = args[1];
            string outfile = args[2];

            try
            {
                var doc = new XPathDocument(xmlfile);
                var transform = new XslCompiledTransform();
                var settings = new XsltSettings(true, true);
                transform.Load(xslfile, settings, new XmlUrlResolver());
                var writer = new XmlTextWriter(outfile, null);
                transform.Transform(doc, null, writer);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
        }
    }
}
