using Inventor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using URDF;

namespace InvAddIn
{
  /// <summary>
  /// Interaction logic for MyWpfWindow.xaml
  /// </summary>
  public partial class PluginWindow : Window
  {
        Inventor.Application _invApp;
        UnitsOfMeasure oUOM;
        AssemblyDocument oAsmDoc;
        AssemblyComponentDefinition oAsmCompDef;
        RepresentationsManager repman;
        LevelOfDetailRepresentation lod_master;
        LevelOfDetailRepresentation lod_simple;

        Robot robot;
         
        public PluginWindow()
        {
            InitializeComponent();
        }

        public PluginWindow(Inventor.Application invApp, string addinCLS)
        {
            InitializeComponent();
            _invApp = invApp;
        }

        private void button3_Click(object sender, RoutedEventArgs e)
        {
            if (_invApp.Documents.Count == 0)
            {
                // Create an instance of the open file dialog box.
                OpenFileDialog openFileDialog1 = new OpenFileDialog();
                openFileDialog1.Filter = "Inventor assembly (.iam)|*.iam|All Files (*.*)|*.*";
                openFileDialog1.FilterIndex = 1;
                openFileDialog1.Multiselect = false;

                // Call the ShowDialog method to show the dialog box.


                // Process input if the user clicked OK.
                if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    _invApp.Documents.Open(openFileDialog1.FileName);
                }
                else
                {
                    _invApp.Documents.Open(Properties.Settings.Default.autoload_file);
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

            if (addbaselinkcheckbox.IsChecked == true)
            {
                addbaselink(ref robot, oAsmCompDef);
            }

            textBox.Text = oAsmDoc.DisplayName.TrimEnd(".iam".ToCharArray());

            dataGridLinks.DataContext = robot.Links;
            dataGridJoints.DataContext = robot.Joints;

            dataGridLinks.ItemsSource = robot.Links;
            dataGridJoints.ItemsSource = robot.Joints;
            dataGridLinks.Items.Refresh();
            dataGridJoints.Items.Refresh();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            if (robot.Name != textBox.Text)
            {
                lod_master.Activate();
                robot = new Robot(textBox.Text, oAsmCompDef);

                if (addbaselinkcheckbox.IsChecked == true)
                {
                    addbaselink(ref robot, oAsmCompDef);
                }
            }

            string folder = GetFolder();

            Directory.CreateDirectory(folder);
            Directory.CreateDirectory(folder + "\\urdf");
            robot.WriteURDFFile(folder + "\\urdf\\" + robot.Name + ".urdf");
        }

        private void addbaselink(ref Robot robot, AssemblyComponentDefinition oAsmCompDef)
        {
            Link base_link = new Link(oAsmCompDef.WorkPoints[1]);
            base_link.Name = "base_link";

            robot.Links.Add(base_link);

            List<Link> roots = robot.Links.Except(robot.Joints.Select(x => x.child.linkreference)).ToList();

            robot.Joints.Add(new Joint("base_link", Joint.JointType.Fixed, roots[1], roots[0]));
        }
      
        private void button_Copy_Click(object sender, RoutedEventArgs e)
        {
            //Change to simple version for meshes
            LevelOfDetailRepresentation lod_simple = repman.LevelOfDetailRepresentations["Collision"];
            lod_simple.Activate();

            string folder = GetFolder();

            Directory.CreateDirectory(folder);
            Directory.CreateDirectory(folder + "\\meshes");
            robot.WriteSTLFiles(folder + "\\meshes");

            lod_master.Activate();
        }

        private string GetFolder()
        {
            string assembly = oAsmDoc.FullFileName;
            string folder = new FileInfo(assembly).Directory.FullName;
            string package = folder + "\\" + robot.Name + "_description";

            return package;
        }
    }
}
