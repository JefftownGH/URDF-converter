﻿/*
 * * MIT License
 * Copyright (c) 2018 Christian Mai
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.

 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 * 
 * Copyright (c) 2017 Richard Vallett
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml;
using Inventor;
using System.Xml.Serialization;

namespace URDF
{
    #region Robot
    /// <summary>
    /// Defines the URDF Robot model.
    /// </summary>
    [Serializable, XmlRoot("robot")]
    public class Robot : ICloneable
    {
        [XmlAttribute("name")]
        public string Name { get; set; }
        [XmlElement("link")]
        public List<Link> Links = new List<Link>();
        [XmlElement("joint")]
        public List<Joint> Joints = new List<Joint>();

        public Robot() { }

        public Robot(string name)
        {
            Name = name;
        }

        public Robot(string name, AssemblyComponentDefinition drawing)
        {
            Name = name;

            MakeLinks(drawing);
            MakeJoints(drawing);

        }

        /// <summary>
        /// Clones the Robot object into a new object.
        /// </summary>
        /// <returns>Cloned Robot object.</returns>
        public object Clone()
        {
            MemoryStream ms = new MemoryStream();
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(ms, this);
            ms.Position = 0;
            object obj = bf.Deserialize(ms);
            ms.Close();
            return obj;
        }


        private void MakeLinks(AssemblyComponentDefinition oAsmCompDef)
        {
            foreach (ComponentOccurrence oCompOccur in oAsmCompDef.Occurrences)
            {
                try
                {
                    Link temp = new Link(oCompOccur);
                    temp.visual = new Link.Visual(new Link.Geometry(new Link.Geometry.Shape.Mesh("package://" + Name + "_description/meshes/" + temp.Name + ".stl")));
                    if (oCompOccur.ContactSet)
                    {
                        dynamic doc = oCompOccur.Definition.Document;
                        dynamic test = doc.PropertySets["Inventor User Defined Properties"];

                        foreach (dynamic prop in test)
                        {
                            if (prop.Name == "collision")
                            {
                                XmlSerializer serializer = new XmlSerializer(typeof(Link.Collision));
                                using (TextReader reader = new StringReader("<" + prop.Name + ">" + prop.Value + "</" + prop.Name + ">"))
                                {
                                    temp.collision = (Link.Collision)serializer.Deserialize(reader);
                                }
                            }
                        }

                        if (temp.collision == null)
                        {
                            temp.collision = new Link.Collision(new Link.Geometry(new Link.Geometry.Shape.Mesh("package://" + Name + "_description/meshes/" + temp.Name + ".stl")));
                        }
                    }

                    Links.Add(temp);
                }
                catch
                {
                    System.Windows.MessageBox.Show("There was a problem adding link for component: " + oCompOccur.Name + ", please ensure the CAD assembly structure is correct");
                    Console.WriteLine("Link could not be created for component");
                }
            }
        }
        private void MakeJoints(AssemblyComponentDefinition oAsmCompDef)
        {
            foreach (AssemblyJoint ost in oAsmCompDef.Joints)
            {
                Joint temp = new Joint(ost, Links);
                Joints.Add(temp);
            }
        }

        public void WriteURDFFile(string filename)
        {
            //Create our own namespaces for the output
            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();

            //Add an empty namespace and empty value
            ns.Add("", "");

            XmlTextWriter URDFWriter = new XmlTextWriter(filename, null);
            URDFWriter.Formatting = Formatting.Indented;
            //URDFWriter.WriteStartDocument(false);
            URDFWriter.WriteComment(" Exported at " + DateTime.Now.ToString() + " ");
            //URDFWriter.WriteStartElement("robot");
            //URDFWriter.WriteAttributeString("name", Name);

            XmlSerializer xmlSerializer1 = new XmlSerializer(typeof(Robot));
            xmlSerializer1.Serialize(URDFWriter, this, ns);

            //Write the XML to file and close the writer
            URDFWriter.Flush();
            URDFWriter.Close();
            if (URDFWriter != null)
                URDFWriter.Close();
        }

        public void WriteSTLFiles(string outputfolder)
        {
            Inventor.Application _invApp = Links[0].Application;
            TranslatorAddIn stptrans = (TranslatorAddIn)_invApp.ApplicationAddIns.ItemById["{533E9A98-FC3B-11D4-8E7E-0010B541CD80}"];
            TranslationContext stpcontext = _invApp.TransientObjects.CreateTranslationContext();

            NameValueMap stpoptions = _invApp.TransientObjects.CreateNameValueMap();

            Links.Reverse();

            foreach (Link oAsmComp in Links.Skip(1))
            {
                dynamic test = oAsmComp.ReferencedDocumentDescriptor.ReferencedDocument;

                if (stptrans.HasSaveCopyAsOptions[test, stpcontext, stpoptions])
                {
                    stpoptions.Value["ExportUnits"] = 6;
                    stpoptions.Value["Resolution"] = 2;
                    stpcontext.Type = IOMechanismEnum.kFileBrowseIOMechanism;

                    DataMedium stpdata = _invApp.TransientObjects.CreateDataMedium();
                    stpdata.FileName = outputfolder + "\\" + oAsmComp.Name + ".stl";
                    stptrans.SaveCopyAs(test, stpcontext, stpoptions, stpdata);
                }
            }
        }
    }

#endregion
}


    
