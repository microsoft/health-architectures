/* 
* 2020 Microsoft Corp
* 
* THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS “AS IS”
* AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO,
* THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
* ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE
* FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
* (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
* LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION)
* HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
* OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
* OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Threading.Tasks;
using System.IO;

namespace hl7ingest
{
    public class HL7ExtractedNameData
    {
        
        public HL7ExtractedNameData(string name,string repeat = "no")
        {
            this.ExtractedName = name.Trim();
            this.Repeats = (repeat.ToLower().Equals("no") ? false : true);
        }
        public string ExtractedName { get; set; }
        public bool Repeats { get; set; }
    }
    public static class HL7ToXmlConverter
    {

        // <span class="code-SummaryComment"><summary></span>
        /// Converts an HL7 message into a JOject Object from it's XML representation of the same message.
        /// <span class="code-SummaryComment"></summary></span>
        /// <span class="code-SummaryComment"><param name="sHL7">The HL7 to convert</param></span>
        /// <span class="code-SummaryComment"><returns>JObject with root of hl7message</returns></span>
        public static JObject ConvertToJObject(string sHL7, JObject hl7metadata = null)
        {
            var xmld = HL7ToXmlConverter.ConvertToXmlDocument(sHL7, hl7metadata);
            if (xmld != null)
            {
                string json = JsonConvert.SerializeXmlNode(xmld);
                JObject o = JObject.Parse(json);
                return o;
            }
            return null;
        }
        // <span class="code-SummaryComment"><summary></span>
        /// Converts a JObject hl7 message into a JSON String.
        /// <span class="code-SummaryComment"></summary></span>
        /// <span class="code-SummaryComment"><param name="o">The JObject with hl7message root to convert</param></span>
        /// <span class="code-SummaryComment"><returns></returns></span>
        public static string ConvertToJSON(JObject o)
        {
            return JsonConvert.SerializeObject(o["hl7message"]);
        }
        // <span class="code-SummaryComment"><summary></span>
        /// Converts an HL7 message into a JSON Object from it's XML representation of the same message.
        /// <span class="code-SummaryComment"></summary></span>
        /// <span class="code-SummaryComment"><param name="sHL7">The HL7 to convert</param></span>
        /// <span class="code-SummaryComment"><returns></returns></span>
        public static string ConvertToJSON(string sHL7, JObject hl7metadata = null)
        {
            JObject o = HL7ToXmlConverter.ConvertToJObject(sHL7, hl7metadata);
            if (o == null) return null;
            return JsonConvert.SerializeObject(o["hl7message"]);
        }
        /// <span class="code-SummaryComment"><summary></span>
        /// Converts an HL7 message into an XML representation of the same message.
        /// <span class="code-SummaryComment"></summary></span>
        /// <span class="code-SummaryComment"><param name="sHL7">The HL7 to convert</param></span>
        /// <span class="code-SummaryComment"><returns>XML String with root of hl7message</returns></span>
        public static string ConvertToXml(string sHL7, JObject hl7metadata = null)
        {
            return HL7ToXmlConverter.ConvertToXmlDocument(sHL7, hl7metadata).OuterXml;
        }
        private static HL7ExtractedNameData LookUpSegmentName(JObject metadata, string segment)
        {
            if (metadata == null || !Utilities.GetEnvironmentVariable("UseMetaDataFieldNames", "no").Equals("yes", StringComparison.InvariantCultureIgnoreCase)) return new HL7ExtractedNameData(segment);
            JToken fmd = metadata.SelectToken("..segments[?(@.id=='" + segment + "')]");
            return (fmd == null ? new HL7ExtractedNameData(segment) : new HL7ExtractedNameData((string)fmd["SegmentName"], (string)fmd["Repeat"]));
        }
        private static HL7ExtractedNameData LookUpFieldName(JObject metadata, string segment, string seq)
        {
            if (metadata == null || !Utilities.GetEnvironmentVariable("UseMetaDataFieldNames", "no").Equals("yes", StringComparison.InvariantCultureIgnoreCase)) return new HL7ExtractedNameData(segment + "." + seq);
            JToken fmd = metadata.SelectToken("..fields[?(@.Segment=='" + segment + "' && @.Sequence=='" + seq + "')]");
            return (fmd == null ? new HL7ExtractedNameData(segment + "." + seq) : new HL7ExtractedNameData((string)fmd["FieldName"], (string)fmd["Repeat"]));
        }
        /// <span class="code-SummaryComment"><summary></span>
        /// Converts an HL7 message into an XMLDocument representation of the same message.
        /// <span class="code-SummaryComment"></summary></span>
        /// <span class="code-SummaryComment"><param name="sHL7">The HL7 to convert</param></span>
        /// <span class="code-SummaryComment"><returns>XMLDocument with root of hl7message</returns></span>
        public static XmlDocument ConvertToXmlDocument(string sHL7, JObject metadata = null)
        {
            try
            {
                if (string.IsNullOrEmpty(sHL7) || !sHL7.StartsWith("MSH")) return null;
                XmlDocument _xmlDoc = null;
                // Go and create the base XML
                _xmlDoc = CreateXmlDoc();

                // HL7 message segments are terminated by carriage returns,
                // so to get an array of the message segments, split on carriage return
                string[] sHL7Lines = sHL7.Split('\r');

                // Now we want to replace any other unprintable control
                // characters with whitespace otherwise they'll break the XML
                for (int i = 0; i < sHL7Lines.Length; i++)
                {
                    sHL7Lines[i] = Regex.Replace(sHL7Lines[i], @"[^ -~]", "");
                }

                /// Go through each segment in the message
                /// and first get the fields, separated by pipe (|),
                /// then for each of those, get the field components,
                /// separated by carat (^), and check for
                /// repetition (~) and also check each component
                /// for subcomponents, and repetition within them too.
                for (int i = 0; i < sHL7Lines.Length; i++)
                {
                    string fs = null;
                    // Don't care about empty lines
                    if (sHL7Lines[i] != string.Empty)
                    {

                        // Get the line and get the line's segments
                        string sHL7Line = sHL7Lines[i];
                        if (fs == null) fs = GetFieldSeparator(sHL7Line);
                        string[] sFields = HL7ToXmlConverter.GetMessgeFields(sHL7Line, fs);
                        // Create a new element in the XML for the line
                        var segmentextract = LookUpSegmentName(metadata, sFields[0]);
                        XmlElement el = _xmlDoc.CreateElement(segmentextract.ExtractedName);
                        if (segmentextract.Repeats)
                        {
                            var attribute = _xmlDoc.CreateAttribute("json", "Array", "http://james.newtonking.com/projects/json");
                            attribute.InnerText = "true";
                            el.Attributes.Append(attribute);
                        }
                        _xmlDoc.DocumentElement.AppendChild(el);
                        XmlElement sq = _xmlDoc.CreateElement("msgseq");
                        sq.InnerText = (i + 1).ToString();
                        el.AppendChild(sq);
                        // For each field in the line of HL7
                        for (int a = 1; a < sFields.Length; a++)
                        {
                            var fieldextract = LookUpFieldName(metadata, sFields[0], a.ToString());
                            // Create a new element
                            XmlElement fieldEl = _xmlDoc.CreateElement(fieldextract.ExtractedName);
                            if (fieldextract.Repeats)
                            {
                                var attribute = _xmlDoc.CreateAttribute("json", "Array", "http://james.newtonking.com/projects/json");
                                attribute.InnerText = "true";
                                fieldEl.Attributes.Append(attribute);
                            }
                            /// Part of the HL7 specification is that part
                            /// of the message header defines which characters
                            /// are going to be used to delimit the message
                            /// and since we want to capture the field that
                            /// contains those characters we need
                            /// to just capture them and stick them in an element.
                            if (sFields[a] != @"^~\&")
                            {
                                /// Get the components within this field, separated by carats (^)
                                /// If there are more than one, go through and create an element for
                                /// each, then check for subcomponents, and repetition in both.
                                string[] sRepeatingComponent = HL7ToXmlConverter.GetRepetitions(sFields[a]);
                                int rc = 0;

                                foreach (string r in sRepeatingComponent)
                                {
                                    XmlElement repeatelement = null;
                                    if (sRepeatingComponent.Length > 1)
                                    {
                                        repeatelement = _xmlDoc.CreateElement(fieldextract.ExtractedName);

                                    }
                                    string[] sComponents = HL7ToXmlConverter.GetComponents(r);
                                    if (sComponents.Length > 1)
                                    {

                                        for (int b = 0; b < sComponents.Length; b++)
                                        {

                                            XmlElement componentEl = _xmlDoc.CreateElement(fieldextract.ExtractedName +
                                                    "." + (b + 1).ToString());



                                            string[] subComponents = GetSubComponents(sComponents[b]);
                                            if (subComponents.Length > 1)
                                            // There were subcomponents
                                            {
                                                for (int c = 0; c < subComponents.Length; c++)
                                                {
                                                    // Check for repetition
                                                    string[] subComponentRepetitions =
                                                            GetRepetitions(subComponents[c]);
                                                    if (subComponentRepetitions.Length > 1)
                                                    {
                                                        for (int d = 0;
                                                                d < subComponentRepetitions.Length;
                                                                d++)
                                                        {
                                                            XmlElement subComponentRepEl =
                                                                _xmlDoc.CreateElement(fieldextract.ExtractedName +
                                                                "." + (b + 1).ToString() +
                                                                "." + (c + 1).ToString() +
                                                                "." + (d + 1).ToString());
                                                            subComponentRepEl.InnerText =
                                                                    subComponentRepetitions[d].UnEscapeHL7();
                                                            componentEl.AppendChild(subComponentRepEl);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        XmlElement subComponentEl =
                                                            _xmlDoc.CreateElement(fieldextract.ExtractedName +
                                                            "." + (b + 1).ToString() + "." + (c + 1).ToString());
                                                        subComponentEl.InnerText = subComponents[c].UnEscapeHL7();
                                                        componentEl.AppendChild(subComponentEl);

                                                    }
                                                }
                                                if (sRepeatingComponent.Length > 1)
                                                {
                                                    repeatelement.AppendChild(componentEl);
                                                    fieldEl.AppendChild(repeatelement);

                                                }
                                                else
                                                {
                                                    fieldEl.AppendChild(componentEl);
                                                }
                                            }
                                            else // There were no subcomponents
                                            {
                                                string[] sRepetitions =
                                                   HL7ToXmlConverter.GetRepetitions(sComponents[b]);
                                                if (sRepetitions.Length > 1)
                                                {
                                                    XmlElement repetitionEl = null;
                                                    for (int c = 0; c < sRepetitions.Length; c++)
                                                    {
                                                        repetitionEl =
                                                          _xmlDoc.CreateElement(sFields[0] + "." + (sRepeatingComponent.Length > 1 ? "." + rc.ToString() : ".") +
                                                          a.ToString() + "." + (b + 1).ToString() +
                                                          "." + (c + 1).ToString());
                                                        repetitionEl.InnerText = sRepetitions[c].UnEscapeHL7();
                                                        componentEl.AppendChild(repetitionEl);
                                                    }
                                                    fieldEl.AppendChild(componentEl);
                                                    el.AppendChild(fieldEl);
                                                }
                                                else
                                                {

                                                    componentEl.InnerText = sComponents[b].UnEscapeHL7();
                                                    if (sRepeatingComponent.Length > 1)
                                                    {
                                                        repeatelement.AppendChild(componentEl);
                                                        el.AppendChild(repeatelement);
                                                    }
                                                    else
                                                    {
                                                        fieldEl.AppendChild(componentEl);
                                                        el.AppendChild(fieldEl);
                                                    }
                                                }
                                            }
                                        }

                                        //el.AppendChild(fieldEl);

                                    }
                                    else
                                    {
                                        if (sRepeatingComponent.Length > 1)
                                        {
                                            repeatelement.InnerText = sFields[a].UnEscapeHL7();
                                            el.AppendChild(repeatelement);
                                        }
                                        else
                                        {
                                            fieldEl.InnerText = sFields[a].UnEscapeHL7();
                                            el.AppendChild(fieldEl);
                                        }
                                    }
                                    rc++;
                                }
                            }
                            else
                            {
                                fieldEl.InnerText = sFields[a].UnEscapeHL7();
                                el.AppendChild(fieldEl);
                            }
                        }
                    }
                }

                return _xmlDoc;
            }
            catch (Exception exp)
            {
                Console.WriteLine(exp.Message);
                return null;
            }
        }
        private static string GetFieldSeparator(string s)
        {
            if (s.StartsWith("MSH") && s.Length > 3)
            {
                return s.Substring(3,1);
            }
            return "|";
        }
        /// <span class="code-SummaryComment"><summary></span>
        /// Split a line into its component parts based on pipe.
        /// <span class="code-SummaryComment"></summary></span>
        /// <span class="code-SummaryComment"><param name="s"></param></span>
        /// <span class="code-SummaryComment"><returns></returns></span>
        private static string[] GetMessgeFields(string s,string fs)
        {
            string[] s1 = s.Split(fs);
            if (s1[0].Equals("MSH", StringComparison.InvariantCultureIgnoreCase))
            {
                List<string> li = new List<string>(s1);
                li.Insert(0, s1[0]);
                li[1] = fs;
                return li.ToArray();
            }
            return s1;
        }

        /// <span class="code-SummaryComment"><summary></span>
        /// Get the components of a string by splitting based on carat.
        /// <span class="code-SummaryComment"></summary></span>
        /// <span class="code-SummaryComment"><param name="s"></param></span>
        /// <span class="code-SummaryComment"><returns></returns></span>
        private static string[] GetComponents(string s)
        {
            return s.Split('^');
        }

        /// <span class="code-SummaryComment"><summary></span>
        /// Get the subcomponents of a string by splitting on ampersand.
        /// <span class="code-SummaryComment"></summary></span>
        /// <span class="code-SummaryComment"><param name="s"></param></span>
        /// <span class="code-SummaryComment"><returns></returns></span>
        private static string[] GetSubComponents(string s)
        {
            return s.Split('&');
        }

        /// <span class="code-SummaryComment"><summary></span>
        /// Get the repetitions within a string based on tilde.
        /// <span class="code-SummaryComment"></summary></span>
        /// <span class="code-SummaryComment"><param name="s"></param></span>
        /// <span class="code-SummaryComment"><returns></returns></span>
        private static string[] GetRepetitions(string s)
        {
            return s.Split('~');
        }

        /// <span class="code-SummaryComment"><summary></span>
        /// Create the basic XML document that represents the HL7 message
        /// <span class="code-SummaryComment"></summary></span>
        /// <span class="code-SummaryComment"><returns></returns></span>
        private static XmlDocument CreateXmlDoc()
        {
            var rootxml = @"<hl7message xmlns:json='http://james.newtonking.com/projects/json'/>";
            XmlDocument output = new XmlDocument();
            output.LoadXml(rootxml);
            //XmlElement rootNode = output.CreateElement("hl7message");

            return output;
        }
        
    }
}


