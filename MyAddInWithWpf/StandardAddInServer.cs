using Inventor;
using System;
using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace InvAddIn
{
    /// <summary>
    /// This is the primary AddIn Server class that implements the ApplicationAddInServer interface
    /// that all Inventor AddIns are required to implement. The communication between Inventor and
    /// the AddIn is via the methods on this interface.
    /// </summary>
    [GuidAttribute("880b4435-f9c2-4c5e-b234-d543a20b5c36")]
    public class StandardAddInServer : ApplicationAddInServer
    {

        // Inventor application object.
        private Application m_inventorApplication;
        private ButtonDefinition m_btnDef;
        private DockableWindow myDockableWindow;

        private PluginWindow wpfWindow;

        public StandardAddInServer()
        {
        }

        #region ApplicationAddInServer Members

        // ribbon panel
        RibbonPanel m_partSketchSlotRibbonPanel;

        public void Activate(Inventor.ApplicationAddInSite addInSiteObject, bool firstTime)
        {
            // This method is called by Inventor when it loads the addin.
            // The AddInSiteObject provides access to the Inventor Application object.
            // The FirstTime flag indicates if the addin is loaded for the first time.

            // Initialize AddIn members.
            m_inventorApplication = addInSiteObject.Application;

            // TODO: Add ApplicationAddInServer.Activate implementation.
            // e.g. event initialization, command creation etc.
            var cmdMgr = m_inventorApplication.CommandManager;

            m_btnDef = cmdMgr.ControlDefinitions.AddButtonDefinition(
              "Show URDF converter", "Show URDF converter", CommandTypesEnum.kQueryOnlyCmdType);
            m_btnDef.OnExecute += CtrlDef_OnExecute;

            Inventor.Ribbons ribbons;
            ribbons = m_inventorApplication.UserInterfaceManager.Ribbons;

            Inventor.Ribbon partRibbon;
            partRibbon = ribbons["Assembly"]; //replace with Assembly for final version

            //get the tabls associated with part ribbon
            RibbonTabs ribbonTabs;
            ribbonTabs = partRibbon.RibbonTabs;

            RibbonTab partSketchRibbonTab;
            partSketchRibbonTab = ribbonTabs.Add("URDF", "id_URDF", "{880b4435-f9c2-4c5e-b234-d543a20b5c36}");// ["id_TabSketch"];

            //create a new panel with the tab
            RibbonPanels ribbonPanels;
            ribbonPanels = partSketchRibbonTab.RibbonPanels;

            m_partSketchSlotRibbonPanel = ribbonPanels.Add("URDF", "Autodesk:URDFConverter:SlotRibbonPanel", "{880b4435-f9c2-4c5e-b234-d543a20b5c36}", "", false);

            //add controls to the slot panel
            CommandControls partSketchSlotRibbonPanelCtrls;
            partSketchSlotRibbonPanelCtrls = m_partSketchSlotRibbonPanel.CommandControls;

            //add the buttons to the ribbon panel
            CommandControl drawSlotCmdBtnCmdCtrl;
            drawSlotCmdBtnCmdCtrl = partSketchSlotRibbonPanelCtrls.AddButton(m_btnDef, false, true, "", false);

            //CtrlDef_OnExecute(null);
        }

        void CtrlDef_OnExecute(NameValueMap Context)
        {
            wpfWindow = new PluginWindow(m_inventorApplication, "{880b4435-f9c2-4c5e-b234-d543a20b5c36}");

            // Could be a good idea to set the owner for this window
            // especially if it was modeless as mentioned in this article:
            var helper = new WindowInteropHelper(wpfWindow);
            helper.EnsureHandle();
            //helper.Owner = new IntPtr(m_inventorApplication.MainFrameHWND);
            //wpfWindow.Show();

            var uimanager = m_inventorApplication.UserInterfaceManager;

            //Create window if missing
            if(myDockableWindow == null)
            {
                myDockableWindow = uimanager.DockableWindows.Add("{880b4435-f9c2-4c5e-b234-d543a20b5c36}", "URDF", "URDF converter");
            }

            if (!myDockableWindow.IsCustomized)
            {
                myDockableWindow.DockingState = DockingStateEnum.kFloat;
                myDockableWindow.Move(0, 0, myDockableWindow.Height, myDockableWindow.Width);
            }

            myDockableWindow.AddChild(helper.Handle);

            wpfWindow.Show();
            myDockableWindow.Visible = true; 
            //wpfWindow.Show(); 
        }

        public void Deactivate()
        {
            // This method is called by Inventor when the AddIn is unloaded.
            // The AddIn will be unloaded either manually by the user or
            // when the Inventor session is terminated

            // TODO: Add ApplicationAddInServer.Deactivate implementation

            // Release objects.
            wpfWindow.Close();
            m_inventorApplication = null;

            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        public void ExecuteCommand(int commandID)
        {
            // Note:this method is now obsolete, you should use the 
            // ControlDefinition functionality for implementing commands.
        }

        public object Automation
        {
            // This property is provided to allow the AddIn to expose an API 
            // of its own to other programs. Typically, this  would be done by
            // implementing the AddIn's API interface in a class and returning 
            // that class object through this property.

            get
            {
                // TODO: Add ApplicationAddInServer.Automation getter implementation
                return null;
            }
        }

        #endregion

    }
}
