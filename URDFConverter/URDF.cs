/*
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
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml;
using System.Xml.Schema;
using System.Globalization;
using Inventor;
using System.Xml.Serialization;

namespace URDF
{

    public static class StringUtil
    {
        public static string JoinFormat<T>(this IEnumerable<T> list, string separator,
                                   string formatString)
        {
            formatString = string.IsNullOrWhiteSpace(formatString) ? "{0}" : "{0:" + formatString + "}";
            return string.Join(separator,
                                 list.Select(item => string.Format(CultureInfo.InvariantCulture, formatString, item)));
        }
    }

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

            this.Links.Add(new Link(drawing.WorkPoints[1]));

            MakeLinks(drawing);
            MakeJoints(drawing);

            List<Link> roots = this.Links.Except(this.Joints.Select(x => x.Child.refff)).ToList();

            this.Joints.Add(new Joint("baselink", Joint.JointType.Fixed, roots[0], roots[1]));
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
                Link temp = new Link(oCompOccur);
                temp.visual = new Link.Visual(new Geometry(new Geometry.Shape.Mesh("package://" + Name + "/meshes/" + temp.Name + ".stl")));
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
                        temp.collision = new Link.Collision(new Geometry(new Geometry.Shape.Mesh("package://" + Name + "/meshes/" + temp.Name + ".stl")));
                    }
                }
                Links.Add(temp);
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
            //URDFWriter.WriteComment(" Exported at " + DateTime.Now.ToString() + " ");
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

    /// <summary>
    /// Link and Joint origin properties.
    /// </summary>
    [Serializable]
    public class Origin
    {
        [XmlIgnore]
        public double[] XYZ { get; set; }
        [XmlIgnore]
        public double[] RPY { get; set; }

        [XmlAttribute]
        public string xyz { get { return XYZ?.JoinFormat(" ", "0.###"); } set { XYZ = value.Split(' ').Select(x => double.Parse(x, CultureInfo.InvariantCulture)).ToArray(); } }
        [XmlAttribute]
        public string rpy { get { return RPY?.JoinFormat(" ", "0.###"); } set { RPY = value.Split(' ').Select(x => double.Parse(x, CultureInfo.InvariantCulture)).ToArray(); } }

        public override string ToString() { return " xyz:" + xyz + "| rpy:" + rpy; }
    }


    /// <summary>
    /// 
    /// </summary>
    public class Geometry
    {
        public Geometry() { }

        public Geometry(Shape Shape) { shape = Shape; }

        [XmlElement("box", Type = typeof(Shape.Box))]
        [XmlElement("cylinder", Type = typeof(Shape.Cylinder))]
        [XmlElement("sphere", Type = typeof(Shape.Sphere))]
        [XmlElement("mesh", Type = typeof(Shape.Mesh))]
        public Shape shape;

        [Serializable]
        [XmlInclude(typeof(Cylinder))]
        [XmlInclude(typeof(Box))]
        [XmlInclude(typeof(Sphere))]
        [XmlInclude(typeof(Mesh))]
        public class Shape
        {
            public Shape() { }

            /// <summary>
            /// 
            /// </summary>
            [Serializable, XmlRoot("box")]
            public class Box : Shape
            {
                [XmlAttribute("size")]
                public double[] Size = new double[3];

                public Box()
                {
                }
                /// <summary>
                /// 
                /// </summary>
                /// <param name="size">Extents of the box.</param>
                public Box(double[] size)
                {
                    Size = size;
                }
            }

            /// <summary>
            /// 
            /// </summary>
            [Serializable, XmlRoot("cylinder")]
            public class Cylinder : Shape
            {
                [XmlAttribute("radius")]
                public double Radius;
                [XmlAttribute("length")]
                public double Length;

                public Cylinder() { }

                public Cylinder(double radius, double length)
                {
                    Radius = radius;
                    Length = length;
                }
            }

            /// <summary>
            /// 
            /// </summary>
            [Serializable, XmlRoot("sphere")]
            public class Sphere : Shape
            {
                [XmlAttribute("radius")]
                public double Radius;

                public Sphere() { }

            }

            /// <summary>
            /// 
            /// </summary>
            [Serializable]
            public class Mesh : Shape
            {
                [XmlAttribute("filename")]
                public string Filename;

                public Mesh() { }

                public Mesh(string filename)
                {
                    Filename = filename;
                }

                public Mesh(string filename, double scalein)
                {
                    Filename = filename;
                }
            }
        }
    }
}


    
