using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Inventor;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Interop;

namespace URDFConverter
{
    /// <summary>
    /// Interaction logic for GUI.xaml
    /// </summary>
    public partial class GUI : Window
    {
        public GUI()
        {
            InitializeComponent();
        }

        public GUI(Inventor.Application _invApp, string addinCLS)
        {

            InitializeComponent();

            #region Get Inventor session
            //try
            //{
            //    _invApp = invApp;
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show(ex.ToString());
            //    MessageBox.Show("Unable to get or start Inventor");
            //}

            //Dockability
            //DockableWindow myDockableWindow = _invApp.UserInterfaceManager.DockableWindows.Add(addinCLS, "URDFConverter", "URDF converter");
            //myDockableWindow.AddChild(new WindowInteropHelper(this).Handle);

            //if (!myDockableWindow.IsCustomized)
            //{
            //    myDockableWindow.DockingState = DockingStateEnum.kFloat;
            //    myDockableWindow.Move(25, 25, myDockableWindow.Height, myDockableWindow.Width);
            //}

            //this.Activate();

            //myDockableWindow.Visible = true;
        }
    }
}
#endregion