/*
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
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml;
using Inventor;
using System.Xml.Serialization;
using System.Linq;
using System.Globalization;

namespace URDF
{
    #region Link
    /// <summary>
    /// Defines the URDF Link model.
    /// </summary>
    [Serializable, XmlRoot("link")]
    public class Link : ICloneable, ComponentOccurrence
    {
        [XmlElement("inertial")]
        public Inertial inertial { get; set; }
        [XmlElement("visual")]
        public Visual visual { get; set; }
        [XmlElement("collision")]
        public Collision collision { get; set; }
        public List<Collision> CollisionGroup { get; set; }

        private string safename;

        [XmlIgnore]
        public dynamic inventorComponent;

        /// <summary>
        /// 
        /// </summary>
        public class Geometry
        {
            public Geometry() { }

            public Geometry(Shape Shape) { shape = Shape; }

            public override string ToString()
            {
                return "shape: " + shape.ToString();
            }

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

        /// <summary>
        /// Link inertial properties.
        /// </summary>
        [Serializable]
        public class Inertial
        {
            [XmlElement("origin")]
            public Origin Origin { get; set; }

            [XmlElement("mass")]
            public Mass mass;
            [XmlIgnore]
            public double[,] InertiaMatrix { get; private set; }
            [XmlElement("inertia")]
            public InertiaVector inertiaVector;

            public struct Mass
            {
                [XmlAttribute]
                public double value;
            }
            
            public struct InertiaVector
            {
                [XmlAttribute]
                public double ixx;
                [XmlAttribute]
                public double ixy;
                [XmlAttribute]
                public double ixz;
                [XmlAttribute]
                public double iyy;
                [XmlAttribute]
                public double iyz;
                [XmlAttribute]
                public double izz;
            }

            public override string ToString()
            {
                return "mass: " + mass.value + " | MoI: " + inertiaVector.ixx + "," + inertiaVector.ixy + "," + inertiaVector.ixz + "," + inertiaVector.iyy + "," + inertiaVector.iyz + "," + inertiaVector.izz;
            }

            public Inertial() { }

            /// <summary>
            /// Set link's mass and moment of inertia.
            /// </summary>
            /// <param name="mass">Link mass (Kg).</param>
            /// <param name="inertiaMatrix">3x3 element moment of inertia matrix (Kg*m^2) [Ixx Ixy Ixz; Ixy Iyy Iyz; Ixz Iyz Izz]</param>
            public Inertial(double massin, double[,] inertiaMatrix)
            {
                mass.value = massin;
                InertiaMatrix = inertiaMatrix;
                inertiaVector.ixx = inertiaMatrix[0, 0];
                inertiaVector.ixy = inertiaMatrix[0, 1];
                inertiaVector.ixz = inertiaMatrix[0, 2];
                inertiaVector.iyy = inertiaMatrix[1, 1];
                inertiaVector.iyz = inertiaMatrix[1, 2];
                inertiaVector.izz = inertiaMatrix[2, 2];
            }

            /// <summary>
            /// Set link's mass and moment of inertia.
            /// </summary>
            /// <param name="mass">Link mass (Kg).</param>
            /// <param name="inertiaVector">1x6 vector of principal moments and products of inertia (Kg*m^2) [Ixx Ixy Ixz Iyy Iyz Izz]</param>
            public Inertial(double massin, double[] inertiaVin)
            {
                mass.value = massin;
                inertiaVector.ixx = inertiaVin[0];
                inertiaVector.ixy = inertiaVin[1];
                inertiaVector.ixz = inertiaVin[2];
                inertiaVector.iyy = inertiaVin[3];
                inertiaVector.iyz = inertiaVin[4];
                inertiaVector.izz = inertiaVin[5];
                InertiaMatrix = new double[,] {
                { inertiaVin[0], inertiaVin[1], inertiaVin[2] },
                { inertiaVin[1], inertiaVin[3], inertiaVin[4] },
                { inertiaVin[2], inertiaVin[4], inertiaVin[5] } };
            }

            public Inertial(MassProperties massProperties)
            {
                mass.value = Math.Round(massProperties.Mass, 4);
                // Get mass properties for each link.
                double[] iXYZ = new double[6];
                massProperties.XYZMomentsOfInertia(out iXYZ[0], out iXYZ[3], out iXYZ[5], out iXYZ[1], out iXYZ[4], out iXYZ[2]); // Ixx, Iyy, Izz, Ixy, Iyz, Ixz -> Ixx, Ixy, Ixz, Iyy, Iyz, Izz

                iXYZ = iXYZ.Select(x => Math.Round(Math.Round(x) * 0.0001,7)).ToArray();

                inertiaVector.ixx = iXYZ[0];
                inertiaVector.ixy = iXYZ[1];
                inertiaVector.ixz = iXYZ[2];
                inertiaVector.iyy = iXYZ[3];
                inertiaVector.iyz = iXYZ[4];
                inertiaVector.izz = iXYZ[5];
                InertiaMatrix = new double[,] {
                { iXYZ[0], iXYZ[1], iXYZ[2] },
                { iXYZ[1], iXYZ[3], iXYZ[4] },
                { iXYZ[2], iXYZ[4], iXYZ[5] } };

                Origin = new Origin();
                Origin.XYZ = new double[] { massProperties.CenterOfMass.X * 0.01, massProperties.CenterOfMass.Y * 0.01, massProperties.CenterOfMass.Z * 0.01 };
            }

        }

        /// <summary>
        /// Link visual properties.
        /// </summary>
        [Serializable, XmlRoot("geometry")]
        public class Visual
        {
            [XmlElement("origin")]
            public Origin Origin { get; set; }

            [XmlElement("geometry")]
            public Geometry Geometry { get; set; }
            public Material Material { get; set; }

            public Visual() { }

            public Visual(Geometry geometry)
            {
                Geometry = geometry;
            }

            public Visual(Geometry geometry, Material material)
            {
                Geometry = geometry;
                Material = material;
            }

            public override string ToString()
            {
                return "geometry: " + Geometry.ToString();
            }

        }

        /// <summary>
        /// Link material properties.
        /// </summary>
        [Serializable]
        public class Material
        {
            public string Name { get; set; }
            public double[] ColorRGBA { get; set; }

            public Material() { }

            public Material(string name, double[] colorRGBA)
            {
                Name = name;
                ColorRGBA = colorRGBA;
            }

            public override string ToString()
            {
                return "name: " + Name.ToString();
            }

        }

        /// <summary>
        /// Link collision properties.
        /// </summary>
        [Serializable]
        [XmlRoot("collision")]
        public class Collision
        {
            [XmlElement("origin")]
            public Origin Origin { get; set; }

            [XmlElement("geometry")]
            public Geometry geometry { get; set; }

            public Collision() { }

            public Collision(Geometry Geometry)
            {
                geometry = Geometry;
            }

            public override string ToString()
            {
                return "origin: " + Origin.ToString() + " | geometry: " + geometry.ToString();
            }

        }

        public Link() { }

        public Link(WorkPoint oCompOccur)
        {
            inventorComponent = oCompOccur;
        }

        public Link(ComponentOccurrence oCompOccur)
        {
            inventorComponent = oCompOccur;

            //double[] tmatrix = new double[16];

            //inventorComponent.Transformation.GetMatrixData(ref tmatrix);

            dynamic objDefinition = null;

            switch (oCompOccur.Definition.Type)
            {
                case ObjectTypeEnum.kPartComponentDefinitionObject:
                    objDefinition = (PartComponentDefinition)oCompOccur.Definition;
                    break;
                case ObjectTypeEnum.kAssemblyComponentDefinitionObject:
                    objDefinition = (AssemblyComponentDefinition)oCompOccur.Definition;
                    break;
                case ObjectTypeEnum.kWorkPointObject:
                    objDefinition = (WorkPoint)oCompOccur.Definition;
                    break;
                default:
                    Console.WriteLine("Unknown object type");
                    break;
            }

            inertial = new Inertial(objDefinition.MassProperties);           
        }

        [XmlAttribute("name")]
        public string Name { get => safename != null ? safename : inventorComponent.Name; set => safename = value; }

        public override string ToString()
        {
            return Name;
        }

        /// <summary>
        /// Clones the Link object into a new object.
        /// </summary>
        /// <returns>Cloned Link object.</returns>
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
        
        public void SetTransformWithoutConstraints(Matrix Matrix)
        {
            inventorComponent.SetTransformWithoutConstraints(Matrix);
        }

        public void CreateGeometryProxy(object Geometry, out object Result)
        {
            inventorComponent.CreateGeometryProxy(Geometry, out Result);
        }

        public void Delete()
        {
            inventorComponent.Delete();
        }

        public void Replace(string FileName, bool ReplaceAll)
        {
            inventorComponent.Replace(FileName, ReplaceAll);
        }

        public RenderStyle GetRenderStyle(out StyleSourceTypeEnum StyleSourceType)
        {
            return inventorComponent.GetRenderStyle(out StyleSourceType);
        }

        public void SetRenderStyle(StyleSourceTypeEnum StyleSourceType, object RenderStyle)
        {
            inventorComponent.SetRenderStyle(StyleSourceType, RenderStyle);
        }

        public void GetReferenceKey(ref byte[] ReferenceKey, int KeyContext = 0)
        {
            inventorComponent.GetReferenceKey(ref ReferenceKey, KeyContext);
        }

        public void ChangeRowOfiPartMember(object NewRow, object CustomInput)
        {
            inventorComponent.ChangeRowOfiPartMember(NewRow, CustomInput);
        }

        public void Edit()
        {
            inventorComponent.Edit();
        }

        public DisplayModeEnum GetDisplayMode(out DisplayModeSourceTypeEnum DisplayModeSourceType)
        {
            return inventorComponent.GetDisplayMode(out DisplayModeSourceType);
        }

        public void SetDisplayMode(DisplayModeSourceTypeEnum DisplayModeSourceType, object DisplayMode)
        {
            inventorComponent.SetDisplayMode(DisplayModeSourceType, DisplayMode);
        }

        public void SetLevelOfDetailRepresentation(string Representation, bool SkipDocumentSave = false)
        {
            inventorComponent.SetLevelOfDetailRepresentation(Representation, SkipDocumentSave);
        }

        public void SetDesignViewRepresentation(string Representation, string Reserved = "", bool Associative = false)
        {
            inventorComponent.SetDesignViewRepresentation(Representation, Reserved, Associative);
        }

        public void ChangeRowOfiAssemblyMember(object NewRow, object Options)
        {
            inventorComponent.ChangeRowOfiAssemblyMember(NewRow, Options);
        }

        public void Suppress(bool SkipDocumentSave = false)
        {
            inventorComponent.Suppress(SkipDocumentSave);
        }

        public void Unsuppress()
        {
            inventorComponent.Unsuppress();
        }

        public void ExitEdit(ExitTypeEnum ExitTo)
        {
            inventorComponent.ExitEdit(ExitTo);
        }

        public void GetDegreesOfFreedom(out int TranslationDegreesCount, out ObjectsEnumerator TranslationDegreesVectors, out int RotationDegreesCount, out ObjectsEnumerator RotationDegreesVectors, out Point DOFCenter)
        {
            inventorComponent.GetDegreesOfFreedom(out TranslationDegreesCount, out TranslationDegreesVectors, out RotationDegreesCount, out RotationDegreesVectors, out DOFCenter);
        }

        public void _Replace(string FileName, bool ReplaceAll)
        {
            inventorComponent._Replace(FileName, ReplaceAll);
        }

        public void _GetDisplayMode(out DisplayModeSourceTypeEnum DisplayModeSourceType)
        {
            inventorComponent._GetDisplayMode(out DisplayModeSourceType);
        }

        public void ShowRelationships()
        {
            inventorComponent.ShowRelationships();
        }
        [XmlIgnore]
        public ObjectTypeEnum Type => inventorComponent.Type;
        [XmlIgnore]
        public dynamic Application => inventorComponent.Application;
        [XmlIgnore]
        public AssemblyComponentDefinition parent => inventorComponent.parent;
        [XmlIgnore]
        public bool HasBodyOverride => inventorComponent.HasBodyOverride;
        [XmlIgnore]
        public SurfaceBodies SurfaceBodies => inventorComponent.SurfaceBodies;
        [XmlIgnore]
        public ComponentDefinition ContextDefinition => inventorComponent.ContextDefinition;
        [XmlIgnore]
        public ComponentDefinition Definition => inventorComponent.Definition;
        [XmlIgnore]
        public DocumentTypeEnum DefinitionDocumentType => inventorComponent.DefinitionDocumentType;
        [XmlIgnore]
        public ComponentOccurrence parentOccurrence => inventorComponent.parentOccurrence;
        [XmlIgnore]
        public ComponentOccurrencesEnumerator OccurrencePath => inventorComponent.OccurrencePath;
        [XmlIgnore]
        public ComponentOccurrencesEnumerator SubOccurrences => inventorComponent.SubOccurrences;

        [XmlIgnore]
        public Matrix Transformation { get => inventorComponent.Transformation; set => inventorComponent.Transformation = value; }
        [XmlIgnore]
        public AssemblyConstraintsEnumerator Constraints => inventorComponent.Constraints;
        [XmlIgnore]
        public bool Grounded { get => inventorComponent.Grounded; set => inventorComponent.Grounded = value; }
        [XmlIgnore]
        public bool Visible { get => inventorComponent.Visible; set => inventorComponent.Visible = value; }
        [XmlIgnore]
        public bool Adaptive { get => inventorComponent.Adaptive; set => inventorComponent.Adaptive = value; }
        [XmlIgnore]
        public bool Enabled { get => inventorComponent.Enabled; set => inventorComponent.Enabled = value; }
        [XmlIgnore]
        public RenderStyle RenderStyle { get => inventorComponent.RenderStyle; set => inventorComponent.RenderStyle = value; }
        [XmlIgnore]
        public MassProperties MassProperties => inventorComponent.MassProperties;
        [XmlIgnore]
        public OccurrencePatternElement PatternElement => inventorComponent.PatternElement;
        [XmlIgnore]
        public AttributeSets AttributeSets => inventorComponent.AttributeSets;
        [XmlIgnore]
        public bool IsiPartMember => inventorComponent.IsiPartMember;
        [XmlIgnore]
        public iMateDefinitionsEnumerator iMateDefinitions => inventorComponent.iMateDefinitions;
        [XmlIgnore]
        public ActionTypeEnum DisabledActionTypes { get => inventorComponent.DisabledActionTypes; set => inventorComponent.DisabledActionTypes = value; }
        [XmlIgnore]
        public Inventor.Box RangeBox => inventorComponent.RangeBox;
        [XmlIgnore]
        public string ActivePositionalState { get => inventorComponent.ActivePositionalState; set => inventorComponent.ActivePositionalState = value; }
        [XmlIgnore]
        public BOMStructureEnum BOMStructure { get => inventorComponent.BOMStructure; set => inventorComponent.BOMStructure = value; }
        [XmlIgnore]
        public bool Flexible { get => inventorComponent.Flexible; set => inventorComponent.Flexible = value; }
        [XmlIgnore]
        public DocumentDescriptor ReferencedDocumentDescriptor => inventorComponent.ReferencedDocumentDescriptor;
        [XmlIgnore]
        public string ActivePositionalRepresentation { get => inventorComponent.ActivePositionalRepresentation; set => inventorComponent.ActivePositionalRepresentation = value; }
        [XmlIgnore]
        public string ActiveLevelOfDetailRepresentation => inventorComponent.ActiveLevelOfDetailRepresentation;
        [XmlIgnore]
        public string ActiveDesignViewRepresentation => inventorComponent.ActiveDesignViewRepresentation;
        [XmlIgnore]
        public bool IsiAssemblyMember => inventorComponent.IsiAssemblyMember;
        [XmlIgnore]
        public bool CustomAdaptive { get => inventorComponent.CustomAdaptive; set => inventorComponent.CustomAdaptive = value; }
        [XmlIgnore]
        public dynamic InterchangeableComponents { get => inventorComponent.InterchangeableComponents; set => inventorComponent.InterchangeableComponents = value; }
        [XmlIgnore]
        public bool Suppressed => inventorComponent.Suppressed;
        [XmlIgnore]
        public double OverrideOpacity { get => inventorComponent.OverrideOpacity; set => inventorComponent.OverrideOpacity = value; }
        [XmlIgnore]
        public bool ShowDegreesOfFreedom { get => inventorComponent.ShowDegreesOfFreedom; set => inventorComponent.ShowDegreesOfFreedom = value; }
        [XmlIgnore]
        public bool IsSubstituteOccurrence => inventorComponent.IsSubstituteOccurrence;
        [XmlIgnore]
        public bool ContactSet { get => inventorComponent.ContactSet; set => inventorComponent.ContactSet = value; }
        [XmlIgnore]
        public bool Excluded { get => inventorComponent.Excluded; set => inventorComponent.Excluded = value; }
        [XmlIgnore]
        public ReferencedFileDescriptor ReferencedFileDescriptor => inventorComponent.ReferencedFileDescriptor;
        [XmlIgnore]
        public string _DisplayName => inventorComponent._DisplayName;
        [XmlIgnore]
        public ComponentDefinitionReference DefinitionReference => inventorComponent.DefinitionReference;
        [XmlIgnore]
        public bool Reference { get => inventorComponent.Reference; set => inventorComponent.Reference = value; }
        [XmlIgnore]
        public bool LocalAdaptive { get => inventorComponent.LocalAdaptive; set => inventorComponent.LocalAdaptive = value; }
        [XmlIgnore]
        public bool Edited => inventorComponent.Edited;
        [XmlIgnore]
        public bool IsPatternElement => inventorComponent.IsPatternElement;
        [XmlIgnore]
        public bool _IsSimulationOccurrence => inventorComponent._IsSimulationOccurrence;
        [XmlIgnore]
        public bool IsAssociativeToDesignViewRepresentation { get => inventorComponent.IsAssociativeToDesignViewRepresentation; set => inventorComponent.IsAssociativeToDesignViewRepresentation = value; }
        [XmlIgnore]
        public Asset Appearance { get => inventorComponent.Appearance; set => inventorComponent.Appearance = value; }
        [XmlIgnore]
        public AppearanceSourceTypeEnum AppearanceSourceType { get => inventorComponent.AppearanceSourceType; set => inventorComponent.AppearanceSourceType = value; }
        [XmlIgnore]
        public AssemblyJointsEnumerator Joints => inventorComponent.Joints;
        [XmlIgnore]
        public bool IsAssociativelyImported => inventorComponent.IsAssociativelyImported;
        [XmlIgnore]
        public bool HasAssociativeImportedComponent => inventorComponent.HasAssociativeImportedComponent;
        [XmlIgnore]
        public ImportedComponent ImportedComponent => inventorComponent.ImportedComponent;
        [XmlIgnore]
        public bool Transparent { get => inventorComponent.Transparent; set => inventorComponent.Transparent = value; }
        [XmlIgnore]
        public string AssociativeForeignFilename => inventorComponent.AssociativeForeignFilename;

        public void Delete2(bool SkipDocumentSave = false)
        {
            parentOccurrence.Delete2(SkipDocumentSave);
        }

        public AssemblyComponentDefinition Parent => parentOccurrence.Parent;

        public ComponentOccurrence ParentOccurrence => parentOccurrence.ParentOccurrence;
    }
}

#endregion