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
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml;
using System.Globalization;
using Inventor;
using System.Xml.Serialization;

namespace URDF
{
    #region Joint
    /// <summary>
    /// Defines the URDF Joint model.
    /// </summary>
    [Serializable, XmlRoot("joint")]
    public class Joint : AssemblyJoint
    {
        /// <summary>
        /// 
        /// </summary>
        [Serializable]
        public enum JointType
        {
            [XmlEnum("revolute")]
            Revolute,
            [XmlEnum("continuous")]
            Continuous,
            [XmlEnum("prismatic")]
            Prismatic,
            [XmlEnum("fixed")]
            Fixed,
            [XmlEnum("floating")]
            Floating,
            [XmlEnum("planar")]
            Planar
        }

        public class Reference
        {
            [XmlIgnore]
            public Link linkreference;
            [XmlAttribute]
            public string link { get => linkreference.Name; set => linkreference.Name = value; }

            public override string ToString() { return "link: " + link; }
        } 

        /// <summary>
        /// 
        /// </summary>
        [Serializable]
        public class Limit
        {
            [XmlAttribute("effort")]
            public double Effort { get; set; }
            [XmlAttribute("velocity")]
            public double Velocity { get; set; }
            [XmlAttribute("lower")]
            public double Lower { get; set; }
            [XmlAttribute("upper")]
            public double Upper { get; set; }

            public Limit() { } 

            public Limit(double effort, double velocity, double lower, double upper)
            {
                Effort = effort;
                Velocity = velocity;
                Lower = lower;
                Upper = upper;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [Serializable]
        public class Dynamics
        {
            public double Damping { get; set; }
            public double Friction { get; set; }

            public Dynamics() { }

            public Dynamics(double damping, double friction)
            {
                Damping = damping;
                Friction = friction;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [Serializable]
        public class SafetyController
        {
            public double SoftLowerLimit { get; set; }
            public double SoftUpperLimit { get; set; }
            public double KPosition { get; set; }
            public double KVelocity { get; set; }

            public SafetyController() { }

            public SafetyController(double softLowerLimit, double softUpperLimit, double kPosition, double kVelocity)
            {
                SoftLowerLimit = softLowerLimit;
                SoftUpperLimit = softUpperLimit;
                KPosition = kPosition;
                KVelocity = kVelocity;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        [Serializable]
        public sealed class Calibration
        {
            public static readonly Calibration rising = new Calibration("rising", 0.0);
            public static readonly Calibration falling = new Calibration("falling", 0.0);

            private Calibration() { }

            private Calibration(string type, double value)
            {
                Type = type;
                Value = value;
            }

            public string Type { get; set; }
            public double Value { get; set; }
        }
        #endregion
        [XmlAttribute("type")]
        public JointType jointType { get; set; }
        [XmlElement("parent")]
        public Reference parent { get; set; }
        [XmlElement("child")]
        public Reference child { get; set; }

        public Limit limit { get; set; }

        [XmlElement("axis")]
        public Origin axis { get; set; }

        [XmlElement("origin")]
        public Origin origin { get; set; }

        public Calibration calibration { get; set; }
        public Dynamics dynamics { get; set; }
        public SafetyController safetyController { get; set; }

        private string safename;

        [XmlAttribute("name")]
        public string Name { get => assemblyJoint == null ? safename : assemblyJoint.Name; set => safename = value; }

        public override string ToString()
        {
            return Name;
        }


        private AssemblyJoint assemblyJoint;

        public Joint() { }
        public Joint(string name, JointType jointTypeIn)
        {
            Name = name;
            jointType = jointTypeIn;
            if (jointType == JointType.Revolute || jointType == JointType.Prismatic)
            {
                // Default values for limit that can be modified later.
                limit = new Limit(1.0, 30.0, 0.0, 180.0);
            }

            parent = new Reference();
            child = new Reference();
        }
        public Joint(string name, JointType jointTypeIn, Link parentin, Link childin)
        {
            Name = name;
            jointType = jointTypeIn;
            if (jointType == JointType.Revolute || jointType == JointType.Prismatic)
            {
                // Default values for limit that can be modified later.
                limit = new Limit(1.0, 30.0, 0.0, 180.0);
            }
            //origin = new Origin() { XYZ = new double[] { . });
            Point ost = parentin.inventorComponent.Definition.Point();
            origin = new Origin() { XYZ = new double[] { ost.X, ost.Y, ost.Z } };
            parent = new Reference();
            child = new Reference();

            parent.linkreference = parentin;
            child.linkreference = childin;
        }

       




        public Joint(AssemblyJoint ajoint, List<Link> links)
        {
            assemblyJoint = ajoint;
            parent = new Reference();
            child = new Reference();

            parent.linkreference = links.Find(x => x.Name == ajoint.OccurrenceTwo.Name);
            child.linkreference = links.Find(x => x.Name == ajoint.OccurrenceOne.Name);

            origin = new Origin();

            switch (ajoint.Definition.JointType)
            {
                case AssemblyJointTypeEnum.kRigidJointType:
                    jointType = JointType.Fixed;
                    origin = TransformConverter.CalculateRotationAngles(ajoint.OccurrenceOne.Transformation, (Application)ajoint.Application);

                    origin.RPY[0] = Math.Abs(origin.RPY[0]) > 0.01 ? origin.RPY[0] : 0;
                    origin.RPY[1] = Math.Abs(origin.RPY[1]) > 0.01 ? origin.RPY[1] : 0;
                    origin.RPY[2] = Math.Abs(origin.RPY[2]) > 0.01 ? origin.RPY[2] : 0;

                    break;
                case AssemblyJointTypeEnum.kRotationalJointType:
                    try
                    {
                        dynamic obj1 = null;
                        if (ajoint.Definition.OriginTwo != null)
                        {
                            dynamic geometry = ajoint.Definition.OriginTwo.Geometry;
                            try
                            { 
                                obj1 = geometry.Geometry.AxisVector;
                            }
                            catch
                            {
                                try { obj1 = geometry.Geometry.Normal; } catch { }
                            }
                        }
                        else if (ajoint.Definition.OriginOne != null)
                        {
                            dynamic geometry = ajoint.Definition.OriginOne.Geometry;
                            try
                            {
                                obj1 = geometry.Geometry.AxisVector;
                            }
                            catch
                            {
                                try { obj1 = geometry.Geometry.Normal; } catch { }
                            }
                        }
                        else
                        {
                            Console.WriteLine("Could not find axis of rotation for " + Name);
                        }
                        double x = Math.Abs(obj1.X) > 0.035 ? obj1.X : 0;
                        double y = Math.Abs(obj1.Y) > 0.035 ? obj1.Y : 0;
                        double z = Math.Abs(obj1.Z) > 0.035 ? obj1.Z : 0;

                        axis = new Origin() { XYZ = new double[] { x, y, z } };
                    }
                    catch(Exception e)
                    {
                        Console.WriteLine("Could not find axis of rotation for " + Name + "\n Exception: " + e.Message);
                    }
                   

                    if (ajoint.Definition.HasAngularPositionLimits)
                    {
                        //Make revolute joint
                        jointType = JointType.Revolute;
                        dynamic limit1 = ajoint.Definition.AngularPositionStartLimit;
                        dynamic limit2 = ajoint.Definition.AngularPositionEndLimit;
                        limit = new Limit(1, 30, (double)limit1.Value, (double)limit2.Value);
                    }
                    else
                    {
                        //Make continous joint
                        jointType = JointType.Continuous;
                    }
                    break;
                default:
                    jointType = JointType.Floating;
                    break;
            }
            Vector frame1 = ajoint.OccurrenceOne.Transformation.Translation;
            Vector frame2 = ajoint.OccurrenceTwo.Transformation.Translation;

            frame1.SubtractVector(frame2);
            origin.XYZ = new double[] { frame1.X * 0.01, frame1.Y * 0.01, frame1.Z * 0.01 };        
        }

        /// <summary>
        /// Clones the Joint object into a new object.
        /// </summary>
        /// <returns>Cloned Joint object.</returns>
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

        public void Delete()
        {
            assemblyJoint.Delete();
        }

        public void GetReferenceKey(ref byte[] ReferenceKey, int KeyContext = 0)
        {
            assemblyJoint.GetReferenceKey(ref ReferenceKey, KeyContext);
        }

        [XmlIgnore]
        public ComponentOccurrence AffectedOccurrenceOne => assemblyJoint.AffectedOccurrenceOne;
        [XmlIgnore]
        public ComponentOccurrence AffectedOccurrenceTwo => assemblyJoint.AffectedOccurrenceTwo;
        [XmlIgnore]
        public dynamic Application => assemblyJoint.Application;
        [XmlIgnore]
        public AttributeSets AttributeSets => assemblyJoint.AttributeSets;

        [XmlIgnore]
        public AssemblyJointDefinition Definition { get => assemblyJoint.Definition; set => assemblyJoint.Definition = value; }
        [XmlIgnore]
        public DriveSettings DriveSettings => assemblyJoint.DriveSettings;
        [XmlIgnore]
        public HealthStatusEnum HealthStatus => assemblyJoint.HealthStatus;

        [XmlIgnore]
        public bool Locked { get => assemblyJoint.Locked; set => assemblyJoint.Locked = value; }
        [XmlIgnore]
        public bool Protected { get => assemblyJoint.Protected; set => assemblyJoint.Protected = value; }
        [XmlIgnore]
        public ComponentOccurrence OccurrenceOne => assemblyJoint.OccurrenceOne;
        [XmlIgnore]
        public ComponentOccurrence OccurrenceTwo => assemblyJoint.OccurrenceTwo;
        [XmlIgnore]
        AssemblyComponentDefinition AssemblyJoint.Parent => assemblyJoint.Parent;
        [XmlIgnore]
        public ObjectTypeEnum Type => assemblyJoint.Type;

        [XmlIgnore]
        public bool Visible { get => assemblyJoint.Visible; set => assemblyJoint.Visible = value; }
        [XmlIgnore]
        public bool Suppressed { get => assemblyJoint.Suppressed; set => assemblyJoint.Suppressed = value; }

        public AssemblyJointDefinition Copy()
        {
            return Definition.Copy();
        }

        public void SetOriginOneAsInfer()
        {
            Definition.SetOriginOneAsInfer();
        }

        public void SetOriginOneAsOffset(object XOffset, object YOffset)
        {
            Definition.SetOriginOneAsOffset(XOffset, YOffset);
        }

        public void SetOriginOneAsBetweenTwoFaces(FaceCollection ReferencedFaces)
        {
            Definition.SetOriginOneAsBetweenTwoFaces(ReferencedFaces);
        }

        public void SetOriginTwoAsInfer()
        {
            Definition.SetOriginTwoAsInfer();
        }

        public void SetOriginTwoAsOffset(object XOffset, object YOffset)
        {
            Definition.SetOriginTwoAsOffset(XOffset, YOffset);
        }

        public void SetOriginTwoAsBetweenTwoFaces(FaceCollection ReferencedFaces)
        {
            Definition.SetOriginTwoAsBetweenTwoFaces(ReferencedFaces);
        }
    }

}
