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

using Inventor;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;
using URDF;

namespace URDFConverter
{
    public partial class Form1 : Form
    {
        Inventor.Application _invApp;
        bool _started = false;

        public Form1(Inventor.Application invApp, string addinCLS)
        {
            InitializeComponent();

            #region Get Inventor session
            try
            {
                _invApp = invApp;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                MessageBox.Show("Unable to get or start Inventor");
            }
        }

        public Form1()
        {
            InitializeComponent();
            
            #region Get Inventor session
            try
            {
                _invApp = (Inventor.Application)Marshal.GetActiveObject("Inventor.Application");
            }
            catch (Exception ex)
            {
                try
                {
                    Type invAppType = Type.GetTypeFromProgID("Inventor.Application");

                    _invApp = (Inventor.Application)System.Activator.CreateInstance(invAppType);
                    _invApp.Visible = true;

                    /* Note: if the Inventor session is left running after this
                     * form is closed, there will still an be and Inventor.exe 
                     * running. We will use this Boolean to test in Form1.Designer.cs 
                     * in the dispose method whether or not the Inventor App should
                     * be shut down when the form is closed.
                     */
                    _started = true;

                }
                catch (Exception ex2)
                {
                    MessageBox.Show(ex2.ToString());
                    MessageBox.Show("Unable to get or start Inventor");
                }
            }

            #endregion

            #region Test code
            /*
            // Define a new Robot, robot, with the name "HuboPlus"
            Robot robot = new Robot("HuboPlus");
            
            // Define a new Link, link1, with the name "link1".
            Link link1 = new Link("link1");
            
            // Set the Visual attributes, geometric and material, of the link.
            link1.Visual = new Visual(new Mesh("package://link1.stl"), 
                new URDF.Material("Red", new double[] { 255, 0, 0, 1.0 }));

            // Set the Collision attributes of the link.
            link1.Collision = new Collision(new URDF.Cylinder(1, 2));

            // Set the Inertial attributes of the link.
            link1.Inertial = new Inertial(5, new double[] { 1, 0, 0, 1, 0, 1 });

            // Add the link to the list of links within the robot.
            robot.Links.Add(link1);

            // Make a clone of link1 and add it to the robot model.
            robot.Links.Add((Link)link1.Clone());


            // Define a new Joint, joint1, with the name "joint1".
            Joint joint1 = new Joint("joint1", JointType.Prismatic, link1, link1);

            robot.Joints.Add(joint1);

            robot.Joints.Add((Joint)joint1.Clone());

            robot.WriteURDFToFile("hubo.xml");
            */

            #endregion

           
        }

        UnitsOfMeasure oUOM;
        AssemblyDocument oAsmDoc;
        AssemblyComponentDefinition oAsmCompDef;
        RepresentationsManager repman;
        LevelOfDetailRepresentation lod_master;
        LevelOfDetailRepresentation lod_simple;

        Robot robot;

        private void button1_Click(object sender, EventArgs e)
        {
            if (_invApp.Documents.Count == 0)
            {
                // Create an instance of the open file dialog box.
                OpenFileDialog openFileDialog1 = new OpenFileDialog();
                openFileDialog1.Filter = "Inventor assembly (.iam)|*.iam|All Files (*.*)|*.*";
                openFileDialog1.FilterIndex = 1;
                openFileDialog1.Multiselect = false;

                // Call the ShowDialog method to show the dialog box.
                DialogResult dialogResult = openFileDialog1.ShowDialog();

                // Process input if the user clicked OK.
                if (dialogResult == DialogResult.OK)
                {
                    _invApp.Documents.Open(openFileDialog1.FileName);
                }
                else
                {
                    _invApp.Documents.Open("D:\\Workspace\\AutoTurf4_CAD\\Version 4 - Komplet maskine.iam");
                }
            }

            oUOM = _invApp.ActiveDocument.UnitsOfMeasure;
            oAsmDoc = (AssemblyDocument)_invApp.ActiveDocument;
            oAsmCompDef = oAsmDoc.ComponentDefinition;

            //Change to master version for massproperties and joints
            repman = oAsmCompDef.RepresentationsManager;
            lod_master = repman.LevelOfDetailRepresentations["Master"];
            lod_simple = repman.LevelOfDetailRepresentations["Collision"];

            lod_master.Activate();
            robot = new Robot(oAsmDoc.DisplayName, oAsmCompDef);

            textBox1.Text = oAsmDoc.DisplayName;

            dataGridView1.AutoGenerateColumns = false;
            dataGridView2.AutoGenerateColumns = false;
            dataGridView1.DataSource = robot.Links;
            dataGridView1.Columns[0].DataPropertyName = "Name";
            dataGridView1.Columns[1].DataPropertyName = "Inertial";
            dataGridView2.DataSource = robot.Joints;
            dataGridView2.Columns[0].DataPropertyName = "Name";
            dataGridView2.Columns[1].DataPropertyName = "JointType";
            dataGridView2.Columns[2].DataPropertyName = "Origin";
            dataGridView2.Columns[3].DataPropertyName = "Axis";
            dataGridView2.Columns[4].DataPropertyName = "Parent";
            dataGridView2.Columns[5].DataPropertyName = "Child";

        }

        private void button2_Click(object sender, EventArgs e)
        {
            //Change to simple version for meshes
            LevelOfDetailRepresentation lod_simple = repman.LevelOfDetailRepresentations["Collision"];
            lod_simple.Activate();

            Directory.CreateDirectory(Directory.GetCurrentDirectory() + "\\meshes");
            robot.WriteSTLFiles(Directory.GetCurrentDirectory() + "\\meshes");

            lod_master.Activate();
        }

        private void button3_Click(object sender, EventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (robot.Name != textBox1.Text)
            {
                lod_master.Activate();
                robot = new Robot(textBox1.Text, oAsmCompDef);
            }
            Directory.CreateDirectory(Directory.GetCurrentDirectory() + "\\urdf");
            robot.WriteURDFFile(Directory.GetCurrentDirectory() + "\\urdf\\" + robot.Name + ".urdf");
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
#endregion