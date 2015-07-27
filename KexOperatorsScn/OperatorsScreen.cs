using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ListBoxTest
{
    public partial class OperatorsScreen : Form
    {
        //MySql Interface
        MySqlInterface mySqlInterface = new MySqlInterface();
        String mySQLServer = KeX_C_Aq_MacroAdmin.Properties.Settings.Default.DBServer;

        //Column locations
        private int cxColumnID;
        private int dxColumnID;
        private int cxColumnObjects;
        private int dxColumnObjects;
        private int cxColumnPar01;
        private int dxColumnPar01;
        private int cxColumnPar02;
        private int dxColumnPar02;
        private int cxColumnPar03;
        private int dxColumnPar03;
        private int cxColumnPar04;
        private int dxColumnPar04;
        private int cxColumnPar05;
        private int dxColumnPar05;
        private int cxColumnPar06;
        private int dxColumnPar06;
        private int cxColumnPar07;
        private int dxColumnPar07;
        private int cxColumnPar08;
        private int dxColumnPar08;

        //All Entries
        List<EntryData> allEntries = new List<EntryData>();

        //All textboxes for the parameterFields
        //TextBox[] parameterFields = null;
        ComboBox[] parameterFields = null;
        Label[] parameterTitles = null;
        Label[] parameterUnits = null;

        //Filler Entries so that everything starts at 1 instead of 0
        private Label filler00_name = null;
        //private TextBox filler00_ID = null;
        private ComboBox filler00_ID = null;
        private Label filler00_unit = null;
        private EntryData filler00_entry = null;

        //This string will contain all the errors the user has in the fields
        String userErrors = "";

        /*userEntryError is a variable used to check if any of the parameters the user entered is 
        * not conforming to the format of the field */
        Boolean userEntryError = false;

        /*insertSelected is a variable used to check if the user wishes to insert elements */
        Boolean insertSelected = false;

        //Global variable to store step number when user wants to change an entry
        private string STEPNUM_CHANGE;

        //Global vaiable used to check if user has Clicked the seq combobox
        private bool refreshSeq = false;

        //This stores the previous dataSet
        private DataSet oldFunctionData = null;

        public OperatorsScreen()
        {
            InitializeComponent();
        }

        /* RefreshCombobox
         * 
         * This will refresh each combobox depending on the data sent to it.
         * */
        private void refreshChComboBox(DataSet channelData, String columnName)
        {
            chNames.DisplayMember = columnName;
            chNames.DataSource = channelData.Tables[0];
        }

        private void refreshFuncComboBox(DataSet functionData, String columnName)
        {
            funcNames.DisplayMember = columnName;
            funcNames.DataSource = functionData.Tables[0];
        }

        private void refreshObjComboBox(DataSet objectData, String columnName)
        {
            objNames.DisplayMember = columnName;
            objNames.DataSource = objectData.Tables[0];         
        }

        private void refreshsqComboBox(DataSet sequenceData, String columnName)
        {
            sqNames.DisplayMember = columnName;
            sqNames.DataSource = sequenceData.Tables[0];
        }

        /* Selected Index Changed for Objects Combobox
         * 
         * If you change the index for the Objects, the Functions should alter
         * to reflect the functions for that specific combobox. First, it gets
         * the correct ID for the object by using the object name in the combobox
         * and then uses that ID to search for the relevant functions.
         * 
         * However, it shouldn't refresh the functions if the same function exists
         * in previous object
         * 
         * */
        void objNames_SelectedIndexChanged(object sender, EventArgs e)
        {
            //Needed to find out the object ID for the object name selected
            String columnObjID = "ID";
            DataSet newObject = getObjID();

            //Get FunctionData
            String tableName = "kex.macro_viewobjectfunction";
            String columnNameFunc = "function_name";
            String columnNameObj = "object_ID";

            DataSet functionData = mySqlInterface.Query("SELECT " + columnNameFunc + " FROM " + tableName +
                         " WHERE " + columnNameObj + "=" + "'" + newObject.Tables[0].Rows[0][columnObjID].ToString() + "'");

            Boolean isSame = true;

            //You are entering the objSelectedIndexChanged for the first time
            if (oldFunctionData == null)
            {
                oldFunctionData = functionData;
                refreshFuncComboBox(functionData, "function_name");
            }
            //You are re-entering this function
            else
            {
                //If both the old and new object have the same number of functions
                if (oldFunctionData.Tables[0].Rows.Count == functionData.Tables[0].Rows.Count)
                {
                    //Check to see if all functions are the same. If so, then do not refresh
                    //If they are different, then refresh. Even if they have the same number
                    //of functions, refresh

                    //Iterate over all functions
                    for (int i = 0; i < oldFunctionData.Tables[0].Rows.Count; i++)
                    {
                        //Check if function string differs. If any differ, break out of
                        //loop and refresh
                        if (oldFunctionData.Tables[0].Rows[i].ItemArray[0].ToString() != functionData.Tables[0].Rows[i].ItemArray[0].ToString())
                        {
                            isSame = false;
                            break;
                        }
                    }
                    if (isSame == false)
                    {
                        isSame = true;
                        oldFunctionData = functionData;
                        refreshFuncComboBox(functionData, "function_name");
                    }
                }
                else
                {
                    oldFunctionData = functionData;
                    refreshFuncComboBox(functionData, "function_name");
                }
            }
        }

        /* The getObjID() function gets the object ID for
         * the object currently in view on the Combobox
         */
        private DataSet getObjID()
        {
            //Check if it is connected
            checkConnect();

            //Needed to find out the object ID for the object name selected
            String tableName = "kex.macro_viewobject";
            String columnIDObj = "ID";
            String columnNameObj = "name";
            DataSet objectID = mySqlInterface.Query("SELECT " + columnIDObj + " FROM " + tableName +
                                                    " WHERE " + columnNameObj + "='" + objNames.Text + "'");

            return objectID;
        }

        /* The getFuncID() function gets the function ID for
         * the function currently in view on the Combobox
         */
        private DataSet getFuncID()
        {
            //Check if it is connected
            checkConnect();

            //Need to find out the function ID for the function name selected
            String tableName = "kex.macro_viewfunction";
            String columnNameFunc = "name";
            String columnIDFunc = "ID";
            DataSet functionID = mySqlInterface.Query("SELECT " + columnIDFunc + " FROM " + tableName +
                                                    " WHERE " + columnNameFunc + "='" + funcNames.Text + "'");

            return functionID;
        }


        /* Selected Index Changed for Function Combobox
         * 
         * If you change the index for the Functions, the parameters should alter
         * to reflect the parameters for that specific function. So, specific 
         * textboxes and labels should become visible when they are needed.
         * 
         * First, it gets the object ID for the object name chosen in the combobox 
         * Then it gets the function ID for the function name chosen in the combobox
         * 
         * Then it gets a row that corresponds to the object ID and function ID
         * to see what parameters that specific function ID and object ID can be altered
         * */
        //This runs everytime the function combobox changes
        void funcNames_SelectedIndexChanged(object sender, EventArgs e)
        {
            //Need to find out the object ID for the object name selected
            String columnIDObj = "ID";
            DataSet objectID = getObjID();

            //Need to find out the function ID for the function name selected
            String columnIDFunc = "ID";
            DataSet functionID = getFuncID();

            //Get the row for the function in question
            String tableName = "kex.macro_viewfunction";
            String columnNameFunc = "ID";
            String columnNameObj = "Object_ID";

            DataSet functionData = mySqlInterface.Query("SELECT * FROM " + tableName + " WHERE "
                                    + columnNameFunc + "='" + functionID.Tables[0].Rows[0][columnIDFunc] + "'" + "and " +
                                    columnNameObj + "='" + objectID.Tables[0].Rows[0][columnIDObj].ToString() + "'");

            //Choose to display or not display textboxes, titles, and unit based on ID
            displayParameterInformation(functionData);

            //Null out values so next time a user enters something, the old text is not there
            nullBoxes();
        }
        
        /* This just nulls out all entries in the textbox fields.
         * It is meant to be used after the user has entered all the information
         * so the previous data isn't retained in the textbox fields.
         * */
        private void nullBoxes()
        {
            for (int i = 1; i < parameterFields.GetLength(0); i++)
            {
                //Null out values so next time a user enters something, the old text is not there
                parameterFields[i].Text = "";
            }
        }

        /* DisplayParameterInformation
         * 
         * This chooses to display the fields (ie. textboxes, labels, etc.) when the user has 
         * selected a specific function as the functions are tied to specific parameters.
         * 
         * It iterates over all parameters (parameterFields.GetLength(0)) and checks if the 
         * "para0#_ID" for that specific parameter is null. Each function has 8 "para0#_ID",
         * para01_ID, para02_ID, etc.. If one of them is not null, then check if the format
         * dictates that it should be a combobox. If so, fill the combobox. Else, display
         * a textbox.
         * 
         * */
        private void displayParameterInformation(DataSet functionData)
        {
            DataRow funcDetails = functionData.Tables[0].Rows[0];

            for (int i = 1; i < parameterFields.GetLength(0) ; i++)
            {
                String curID = "para0" + i.ToString() + "_ID";
                String curName = "para0" + i.ToString() + "_name";
                String curUnit = "para0" + i.ToString() + "_unit";
                String curFormat = "para0" + i.ToString() + "_format";

                if (funcDetails[curID].ToString() != "")
                {
                    //Make the label and textfield visible
                    parameterFields[i].Visible = true;
                    parameterTitles[i].Visible = true;
                    parameterTitles[i].Text = funcDetails[curName].ToString();

                    //Null entry
                    if (funcDetails[curUnit].ToString() == "")
                    {
                        parameterUnits[i].Text = "N/A";
                    }
                    //any other entry
                    else
                    {
                        //Sets the Unit label 
                        parameterUnits[i].Text = funcDetails[curUnit].ToString();
                    }
                    parameterUnits[i].Visible = true;

                    //Check the format to see if it should be a combobox
                    //or a regular text box
                    if (funcDetails[curFormat].ToString() == "channellist")
                    {
                        //Turn parameter into combobox
                        parameterFields[i].DropDownStyle = ComboBoxStyle.DropDownList;

                        //Populate the parameters
                        //get Dataset for channelData
                        String tableName = "kex.macro_viewchannel";
                        /* 20150514 by Bx.
                         * String columnNameCh = "name";
                        */
                        String columnNameCh = "ID";
                        DataSet channelData = mySqlInterface.Query("SELECT " + columnNameCh + " FROM " + tableName);

                        //Fill Channels with channelData
                        /* 20150514 by Bx.
                         * parameterFields[i].DisplayMember = "name";
                         */
                        parameterFields[i].DisplayMember = "ID";
                        parameterFields[i].DataSource = channelData.Tables[0];

                        //Set Event Handler
                        this.parameterFields[i].SelectedIndexChanged +=
                                new System.EventHandler(parameters_channellist_SelectedIndexChanged);

                    }
                    else if (funcDetails[curFormat].ToString() == "triggerlist")
                    {
                        //Turn parameter into combobox
                        parameterFields[i].DropDownStyle = ComboBoxStyle.DropDownList;

                        parameterFields[i].Items.Clear();

                        //Get Sequence ID
                        String columnNameID = "ID";
                        String columnseqNameID = "sequence_ID";
                        DataSet seqID = getSeqID();
                        
                        //Get Channel ID
                        String columnchNameID = "channel_ID";
                        String tableName = "kex.macro_viewchannel";
                        /*  20150514 by Bx.
                         * String columnNameCh = "name";
                         */
                        String columnNameCh = "ID";

                        DataSet chID = mySqlInterface.Query("SELECT " + columnNameID + " FROM " + tableName +
                                                " WHERE " + columnNameCh + "=" + "'" +
                                                parameterFields[1].Text + "'");

                        //Tablename
                        tableName = "kex.macro_viewusedtrigger";

                        /*
                         *SELECT * FROM kex.macro_viewusedtrigger WHERE sequence_ID='7' AND channel_ID='1'; 
                         * For the channel_ID, find the parameter in the combobox 
                         */
                        DataSet trigData = mySqlInterface.Query("SELECT * FROM " + tableName + " WHERE " + 
                                                                  columnseqNameID + "='" + seqID.Tables[0].Rows[0][columnNameID] +
                                                                 "' AND " + columnchNameID + "='" + chID.Tables[0].Rows[0][columnNameID]
                                                                 + "'");

                        //Iterate through all rows that are triggers 
                        foreach (DataRow row in trigData.Tables[0].Rows)
                        {
                            String trigColName = "trigger";

                            //Add it to combobox
                            parameterFields[i].Items.Add(row[trigColName].ToString());
                        }
                    }
                    else if (funcDetails[curFormat].ToString() == "labellist")
                    {
                        //Turn it into a combobox
                        parameterFields[i].DropDownStyle = ComboBoxStyle.DropDownList;

                        //Clear previous
                        parameterFields[i].Items.Clear();

                        //Get Sequence ID
                        String columnNameID = "ID";
                        String columnseqNameID = "sequence_ID";
                        DataSet seqID = getSeqID();

                        //Get Channel ID
                        String columnchNameID = "channel_ID";
                        String tableName = "kex.macro_viewchannel";

                        DataSet chID = getChID();

                        tableName = "kex.macro_viewusedlabel";

                        DataSet labelData = mySqlInterface.Query("SELECT * FROM " + tableName + " WHERE " +
                                          columnseqNameID + "='" + seqID.Tables[0].Rows[0][columnNameID] +
                                         "' AND " + columnchNameID + "='" + chID.Tables[0].Rows[0][columnNameID]
                                         + "'");

                        //Iterate through all rows that are labels
                        foreach (DataRow row in labelData.Tables[0].Rows)
                        {
                            String labelColName = "label";

                            //Add it to combobox
                            parameterFields[i].Items.Add(row[labelColName].ToString());
                        }
                    }
                    else
                    {
                        parameterFields[i].DropDownStyle = ComboBoxStyle.Simple;
                    }

                }
                else
                {
                    parameterFields[i].Visible = false;
                    parameterTitles[i].Visible = false;
                    parameterUnits[i].Visible = false;
                }
            }
        }

        /*This triggers if a parameter of type "channelist" is changed
         * 
         * 1. If a 'channellist' format changes, that means a 'triggerlist' format needs to be updated
         *    because 'channellist' and 'triggerlist' are part of the same function
         * 
         */ 
        private void parameters_channellist_SelectedIndexChanged(object sender, EventArgs e)
        {
            //The second parameter is the trigger - change later to search for trigger
            //rather than making it the second parameter
            parameterFields[2].Items.Clear();

            //Get Sequence ID
            String columnNameID = "ID";
            String columnseqNameID = "sequence_ID";
            DataSet seqID = getSeqID();

            //Get Channel ID
            String columnchNameID = "channel_ID";
            String tableName = "kex.macro_viewchannel";
            /* 20150514 By Bx.
             * String columnNameCh = "name";
             */

            String columnNameCh = "ID";

            //Get channel name from parameterFields[1].Text (which is the combobox for channellist) and
            //get the ID for that channel
            DataSet chID = mySqlInterface.Query("SELECT " + columnNameID + " FROM " + tableName +
                                    " WHERE " + columnNameCh + "=" + "'" +
                                    parameterFields[1].Text + "'");

            //Tablename
            tableName = "kex.macro_viewusedtrigger";
    
             /* For the channel_ID, find the parameter in the combobox 
              * 
             */
            DataSet trigData = mySqlInterface.Query("SELECT * FROM " + tableName + " WHERE " +
                                                      columnseqNameID + "='" + seqID.Tables[0].Rows[0][columnNameID] +
                                                     "' AND " + columnchNameID + "='" + chID.Tables[0].Rows[0][columnNameID]
                                                     + "'");


            //Iterate through all rows that are triggers 
            foreach (DataRow row in trigData.Tables[0].Rows)
            {
                String trigColName = "trigger";

                //Add it to combobox
                parameterFields[2].Items.Add(row[trigColName].ToString());
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //Init ServerList
            serverList.Items.Clear();
            serverList.Items.Add("KeX-KP10-TS");
            serverList.Items.Add("KeX-KP11-TS");
            serverList.Items.Add("localhost");
            serverList.Text = mySQLServer;

            displayVersion();
            
            //Tie Enter Button to "Change"
            AcceptButton = change;

            int dxBorder = SystemInformation.Border3DSize.Width;

            parameterFields = new ComboBox[]
            {
                filler00_ID, //Added this filler just because parameters start at 01
                para01_ID,
                para02_ID,
                para03_ID,
                para04_ID,
                para05_ID,
                para06_ID,
                para07_ID,
                para08_ID
            };

            parameterTitles = new Label[]
            {
                filler00_name, //Added this filler just because parameters start at 01
                para01_name,
                para02_name,
                para03_name,
                para04_name,
                para05_name,
                para06_name,
                para07_name,
                para08_name
            };

            parameterUnits = new Label[]
            {
                filler00_unit, //Added this filler just because parameters start at 01
                para01_unit,
                para02_unit,
                para03_unit,
                para04_unit,
                para05_unit,
                para06_unit,
                para07_unit,
                para08_unit
            };

            //Set up XY Coordinates
            cxColumnID = title_ID.Left - dxBorder;
            dxColumnID = title_ID.Width;
            cxColumnObjects = title_Object.Left - dxBorder;
            dxColumnObjects = title_Object.Width;
            cxColumnPar01 = title_Para01.Left - dxBorder;
            dxColumnPar01 = title_Para01.Width;
            cxColumnPar02 = title_Para02.Left - dxBorder;
            dxColumnPar02 = title_Para02.Width;
            cxColumnPar03 = title_Para03.Left - dxBorder;
            dxColumnPar03 = title_Para03.Width;
            cxColumnPar04 = title_Para04.Left - dxBorder;
            dxColumnPar04 = title_Para04.Width;
            cxColumnPar05 = title_Para05.Left - dxBorder;
            dxColumnPar05 = title_Para05.Width;
            cxColumnPar06 = title_Para06.Left - dxBorder;
            dxColumnPar06 = title_Para06.Width;
            cxColumnPar07 = title_Para07.Left - dxBorder;
            dxColumnPar07 = title_Para07.Width;
            cxColumnPar08 = title_Para08.Left - dxBorder;
            dxColumnPar08 = title_Para08.Width;

            //Remove all selector arrows from combobox
            foreach (var combobox in parameterFields)
            {
                if (combobox != null) combobox.DropDownStyle = ComboBoxStyle.Simple;
            }

            //Event handler that activates when the Object name changes
            this.objNames.SelectedIndexChanged +=
                new System.EventHandler(objNames_SelectedIndexChanged);

            //Event handler that activates when Function name changes
            this.funcNames.SelectedIndexChanged +=
                new System.EventHandler(funcNames_SelectedIndexChanged);

            //Event handler that activates when Channel Number changes
            this.chNames.SelectedIndexChanged +=
                new System.EventHandler(chNames_SelectedIndexChanged);

            //Event handler that activates when Sequence Changes
            this.sqNames.SelectedIndexChanged +=
                new System.EventHandler(chNames_SelectedIndexChanged);

            //Connect to Server
            mySqlInterface.Connect(KeX_C_Aq_MacroAdmin.Properties.Settings.Default.DBServer,
                    KeX_C_Aq_MacroAdmin.Properties.Settings.Default.DBShema,
                    KeX_C_Aq_MacroAdmin.Properties.Settings.Default.DBUser,
                    KeX_C_Aq_MacroAdmin.Properties.Settings.Default.DBPassword);

            //Check if the mySqlInterface is connected
            checkConnect();

            //get Dataset for objectData
            String tableName = "kex.macro_viewobject";
            DataSet objectData = mySqlInterface.Query("SELECT * FROM " + tableName);

            //Fill Combobox with objectData
            refreshObjComboBox(objectData, "name");

            //Need to find out the object ID for the object name selected
            String columnIDObj = "ID";
            DataSet objectID = getObjID();

            //Get Function name (not Function ID) for object ID chosen
            tableName = "kex.macro_viewobjectfunction";
            String columnNameFunc = "function_name";
            String columnNameObj = "object_ID";
            DataSet functionData = mySqlInterface.Query("SELECT " + columnNameFunc + " FROM " +
                                    tableName + " WHERE " + columnNameObj + "=" + "'" +
                                    objectID.Tables[0].Rows[0][columnIDObj].ToString() +
                                    "'");

            //get Dataset for sequenceData
            tableName = "kex.macro_viewsequence";
            String columnNameSeq = "name";
            DataSet sequenceData = mySqlInterface.Query("SELECT " + columnNameSeq + " FROM " + tableName);

            //Fill Sequences with sequenceData
            refreshsqComboBox(sequenceData, "name");

            //get Dataset for channelData
            tableName = "kex.macro_viewchannel";
            String columnNameCh = "name";
            DataSet channelData = mySqlInterface.Query("SELECT " + columnNameCh + " FROM " + tableName);

            //Fill Channels with channelData
            refreshChComboBox(channelData, "name");
        }

        private void displayVersion()
        {
            //Display Product Version
            string productVersion = "";
            if (System.Deployment.Application.ApplicationDeployment.IsNetworkDeployed)  //not @ debug mode
            {
                productVersion = System.Deployment.Application.ApplicationDeployment.CurrentDeployment.CurrentVersion.Major.ToString() + "."
                + System.Deployment.Application.ApplicationDeployment.CurrentDeployment.CurrentVersion.Minor.ToString() + "."
                + System.Deployment.Application.ApplicationDeployment.CurrentDeployment.CurrentVersion.Build.ToString() + "."
                + System.Deployment.Application.ApplicationDeployment.CurrentDeployment.CurrentVersion.Revision.ToString();
            }
            else
            {
                productVersion = "Debugging";
            }


            this.Text = "DIP/KPF Sequence Admin"
                + " V" + productVersion
                + " - " + mySQLServer
                + " (" + KeX_C_Aq_MacroAdmin.Properties.Settings.Default.DBUser + ")";
        }

        /* CheckConnect
         * 
         * This is meant to just check the mySql connection and if it is not connected,
         * then try reconnecting.
         * 
         * Might need more code here for error checking.
         * 
         * */
        private void checkConnect()
        {
            while (!mySqlInterface.isConnected)
            {
                try
                {
                    mySqlInterface.Connect(mySQLServer,
                    KeX_C_Aq_MacroAdmin.Properties.Settings.Default.DBShema,
                    KeX_C_Aq_MacroAdmin.Properties.Settings.Default.DBUser,
                    KeX_C_Aq_MacroAdmin.Properties.Settings.Default.DBPassword);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.GetBaseException().ToString(), "Error",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                    mySQLServer = "<Error>";
                }
            }
        }

        /* Selected Index Changed for Channel Combobox
         * 
         * You can change the index of a channel for two reasons. 
         * 1. It's becauase you want to change to another channel
         * 2. Because you want to refresh the sequence list
         * 
         * In #2's scenario, you don't want to refresh the array because
         * there is no need for it.
         * 
         * Also,if there is a change in channel you have to refresh the parameters
         * because if the user has selected a "Back@Counter", the interface should
         * refresh to reflect that
         * */
        private void chNames_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (refreshSeq == false)
            {
                //Check if it is connected
                checkConnect();

                String tableName = "kex.macro_viewsequencestep";
                String columnNameSeq = "sequence_name";
                String columnNameCh = "channel_name";

                DataSet seqData = mySqlInterface.Query("SELECT * FROM " + tableName + " WHERE " +
                             columnNameSeq + "=" + "'" + sqNames.Text + "'" + " and " +
                             columnNameCh + "=" + "'" + chNames.Text + "'");

                //Add entries to Array and display on screen
                addtoArray(seqData);

                /* Refresh Labels if needed
                 * 
                 * This was only needed because if the user selected "Back@Counter", there should be
                 * a refresh of the combobox that displays all the Labels the user can jump to
                 * 
                 */
                
                //Need to find out the object ID for the object name selected
                String columnIDObj = "ID";
                DataSet objectID = getObjID();

                //Need to find out the function ID for the function name selected
                String columnIDFunc = "ID";
                DataSet functionID = getFuncID();

                //Get the row for the function in question
                tableName = "kex.macro_viewfunction";
                String columnNameFunc = "ID";
                String columnNameObj = "Object_ID";

                DataSet functionData = mySqlInterface.Query("SELECT * FROM " + tableName + " WHERE "
                                        + columnNameFunc + "='" + functionID.Tables[0].Rows[0][columnIDFunc] + "'" + "and " +
                                        columnNameObj + "='" + objectID.Tables[0].Rows[0][columnIDObj].ToString() + "'");

                //Choose to display or not display textboxes, titles, and unit based on ID
                displayParameterInformation(functionData);
            }
            else
            {
                refreshSeq = false;
            }
        }

        //This shows the dataset onto the listbox
        private void addtoArray(DataSet seqData)
        {
            
            allEntries.Clear();

            String curID, curName, curValue = null;

            foreach (DataRow row in seqData.Tables[0].Rows)
            {
                EntryData newEntry = new EntryData();

                //20150518 By Bx.
                newEntry.backColor = Color.LightBlue;

                newEntry.IDnum = row["stepnumber"].ToString();

                for (int i = 1; i < parameterFields.GetLength(0); i++)
                {
                    curID = "para0" + i.ToString() + "_ID";
                    //An entry exists
                    if (row["function_name"].ToString() != "")
                    {
                        curName = "para0" + i.ToString() + "_name";
                        curValue = "para0" + i.ToString();

                        //Populate Text
                        newEntry.parameterVals[i] = row[curValue].ToString();

                        //Populate Name
                        newEntry.parameterNames[i] = row[curName].ToString();

                        //Populate Object
                        newEntry.objName = row["object_name"].ToString();

                        //Populate Function
                        newEntry.funcName = row["function_name"].ToString();
                    }
                }
                //Add Entry to Array
                allEntries.Add(newEntry);
            }

            //Refresh the ListBox
            listBox1.BeginUpdate();
            listBox1.Items.Clear();
            listBox1.Items.AddRange(allEntries.ToArray());
            listBox1.EndUpdate();

        }

        private void listBox1_DrawItem(object sender, DrawItemEventArgs e)
        {
            ListBox listBox1 = sender as ListBox;

            if ((listBox1 != null) && (e.Index >= 0))
            {
                EntryData newEntry = listBox1.Items[e.Index] as EntryData;

                if (newEntry != null && newEntry.checkNull != "NULLENTRY")
                {
                    Brush foreBrush = Brushes.Black;
                    Brush backBrush = Brushes.White;

                    if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
                    {
                        foreBrush = Brushes.Black;
                        backBrush = Brushes.LightSkyBlue;
                    }
                    else
                    {
                        switch (newEntry.funcName)
                        {
                            //Settings
                            case "Set Simulation":
                            case "Set TimeFactor":
                                foreBrush = Brushes.DarkGray;
                                backBrush = Brushes.LightGray;
                                break;

                            //Body
                            case "Init":
                            case "End":
                                foreBrush = Brushes.Gray;
                                backBrush = Brushes.LightGray;
                                break;

                            //Action
                            case "Activate":
                            case "Deactivate":
                            case "Open":
                            case "Close":
                                foreBrush = Brushes.Black;
                                backBrush = Brushes.LightBlue;
                                break;

                            //Comment
                            case "Comment":
                                foreBrush = Brushes.Black;
                                backBrush = Brushes.White;
                                break;

                            //Coordination
                            case "Set Trigger":
                            case "Jump@Trigger":
                            case "Wait Trigger":
                            case "TimeSync":
                            case "Execute":
                            case "(!) StopSequence":
                                foreBrush = Brushes.Black;
                                backBrush = Brushes.BlanchedAlmond;
                                break;

                            //Loop
                            case "Set Label":
                            case "Jump@Counter":
                            case "(!) Jump Event":
                            case "Jump@Value":
                                foreBrush = Brushes.Black;
                                backBrush = Brushes.LightGoldenrodYellow;
                                break;

                            //Set
                            case "Set":
                            case "Ramp":
                            case "Set Command":
                            case "Increment":
                            case "Decrement":
                                foreBrush = Brushes.Black;
                                backBrush = Brushes.LightBlue;
                                break;

                            //Datalog
                            case "Start DataLog":
                            case "Stop DataLog":
                            case "Scope Record":
                            case "Scope Save":
                                foreBrush = Brushes.Black;
                                backBrush = Brushes.Gold;
                                break;

                            //Wait
                            case "Wait Diag":
                            case "Wait State":
                            case "Wait Value":
                                foreBrush = Brushes.Black;
                                backBrush = Brushes.LightGreen;
                                break;

                            //Wait
                            case "Wait Time":
                                foreBrush = Brushes.Black;
                                backBrush = Brushes.LightSalmon;
                                break;

                            default:
                                foreBrush = Brushes.Black;
                                backBrush = Brushes.White;
                                break;
                        }

                    }

                    e.Graphics.FillRectangle(backBrush, e.Bounds);

                    StringFormat sFormat = new StringFormat();
                    sFormat.Alignment = StringAlignment.Far;

                    //Overall Rectangle
                    Rectangle drawRect = new Rectangle(cxColumnID, e.Bounds.Top, listBox1.Width, e.Bounds.Height);
                    //ControlPaint.DrawBorder3D(e.Graphics, drawRect, Border3DStyle.Flat);

                    drawRect = new Rectangle(cxColumnID, e.Bounds.Top, dxColumnID, e.Bounds.Height);
                    e.Graphics.DrawString(newEntry.IDnum.ToString(), e.Font, foreBrush, drawRect, sFormat);
                    //ControlPaint.DrawBorder3D(e.Graphics, drawRect, Border3DStyle.Flat);

                    drawRect = new Rectangle(cxColumnObjects, e.Bounds.Top, dxColumnObjects, e.Bounds.Height / 2);
                    e.Graphics.DrawString(newEntry.funcName.ToString(), e.Font, foreBrush, drawRect, sFormat);
                    //ControlPaint.DrawBorder3D(e.Graphics, drawRect, Border3DStyle.Flat);

                    drawRect = new Rectangle(cxColumnObjects, e.Bounds.Top + e.Bounds.Height / 2, dxColumnObjects, e.Bounds.Height / 2);
                    e.Graphics.DrawString(newEntry.objName.ToString(), new Font("Segoe UI", 9, FontStyle.Bold), foreBrush, drawRect, sFormat);
                    //ControlPaint.DrawBorder3D(e.Graphics, drawRect, Border3DStyle.Flat);

                    Font drawFontBold = new Font("Segoe UI", 9, FontStyle.Bold);

                    if (newEntry.parameterNames[1] != null)
                    {
                        drawRect = new Rectangle(cxColumnPar01, e.Bounds.Top, dxColumnPar01, e.Bounds.Height / 2);
                        e.Graphics.DrawString(newEntry.parameterNames[1], e.Font, foreBrush, drawRect, sFormat);
                        //ControlPaint.DrawBorder3D(e.Graphics, drawRect, Border3DStyle.Flat);

                        drawRect = new Rectangle(cxColumnPar01, e.Bounds.Top + e.Bounds.Height / 2, dxColumnPar01, e.Bounds.Height / 2);
                        e.Graphics.DrawString(newEntry.parameterVals[1], new Font("Segoe UI", 9, FontStyle.Bold), foreBrush, drawRect, sFormat);
                        //ControlPaint.DrawBorder3D(e.Graphics, drawRect, Border3DStyle.Flat);
                    }

                    if (newEntry.parameterNames[2] != null)
                    {
                        drawRect = new Rectangle(cxColumnPar02, e.Bounds.Top, dxColumnPar02, e.Bounds.Height / 2);
                        e.Graphics.DrawString(newEntry.parameterNames[2], e.Font, foreBrush, drawRect, sFormat);
                        //ControlPaint.DrawBorder3D(e.Graphics, drawRect, Border3DStyle.Flat);

                        drawRect = new Rectangle(cxColumnPar02, e.Bounds.Top + e.Bounds.Height / 2, dxColumnPar02, e.Bounds.Height / 2);
                        e.Graphics.DrawString(newEntry.parameterVals[2], new Font("Segoe UI", 9, FontStyle.Bold), foreBrush, drawRect, sFormat);
                        //ControlPaint.DrawBorder3D(e.Graphics, drawRect, Border3DStyle.Flat);
                    }

                    if (newEntry.parameterNames[3] != null)
                    {
                        drawRect = new Rectangle(cxColumnPar03, e.Bounds.Top, dxColumnPar03, e.Bounds.Height / 2);
                        e.Graphics.DrawString(newEntry.parameterNames[3], e.Font, foreBrush, drawRect, sFormat);
                        //ControlPaint.DrawBorder3D(e.Graphics, drawRect, Border3DStyle.Flat);

                        drawRect = new Rectangle(cxColumnPar03, e.Bounds.Top + e.Bounds.Height / 2, dxColumnPar03, e.Bounds.Height / 2);
                        e.Graphics.DrawString(newEntry.parameterVals[3], new Font("Segoe UI", 9, FontStyle.Bold), foreBrush, drawRect, sFormat);
                        //ControlPaint.DrawBorder3D(e.Graphics, drawRect, Border3DStyle.Flat);
                    }

                    if (newEntry.parameterNames[4] != null)
                    {
                        drawRect = new Rectangle(cxColumnPar04, e.Bounds.Top, dxColumnPar04, e.Bounds.Height / 2);
                        e.Graphics.DrawString(newEntry.parameterNames[4], e.Font, foreBrush, drawRect, sFormat);
                        //ControlPaint.DrawBorder3D(e.Graphics, drawRect, Border3DStyle.Flat);

                        drawRect = new Rectangle(cxColumnPar04, e.Bounds.Top + e.Bounds.Height / 2, dxColumnPar04, e.Bounds.Height / 2);
                        e.Graphics.DrawString(newEntry.parameterVals[4], new Font("Segoe UI", 9, FontStyle.Bold), foreBrush, drawRect, sFormat);
                        //ControlPaint.DrawBorder3D(e.Graphics, drawRect, Border3DStyle.Flat);
                    }

                    if (newEntry.parameterNames[5] != null)
                    {
                        drawRect = new Rectangle(cxColumnPar05, e.Bounds.Top, dxColumnPar05, e.Bounds.Height / 2);
                        e.Graphics.DrawString(newEntry.parameterNames[5], e.Font, foreBrush, drawRect, sFormat);
                        //ControlPaint.DrawBorder3D(e.Graphics, drawRect, Border3DStyle.Flat);

                        drawRect = new Rectangle(cxColumnPar05, e.Bounds.Top + e.Bounds.Height / 2, dxColumnPar05, e.Bounds.Height / 2);
                        e.Graphics.DrawString(newEntry.parameterVals[5], new Font("Segoe UI", 9, FontStyle.Bold), foreBrush, drawRect, sFormat);
                        //ControlPaint.DrawBorder3D(e.Graphics, drawRect, Border3DStyle.Flat);
                    }

                    if (newEntry.parameterNames[6] != null)
                    {
                        drawRect = new Rectangle(cxColumnPar06, e.Bounds.Top, dxColumnPar06, e.Bounds.Height / 2);
                        e.Graphics.DrawString(newEntry.parameterNames[6], e.Font, foreBrush, drawRect, sFormat);
                        //ControlPaint.DrawBorder3D(e.Graphics, drawRect, Border3DStyle.Flat);

                        drawRect = new Rectangle(cxColumnPar06, e.Bounds.Top + e.Bounds.Height / 2, dxColumnPar06, e.Bounds.Height / 2);
                        e.Graphics.DrawString(newEntry.parameterVals[6], new Font("Segoe UI", 9, FontStyle.Bold), foreBrush, drawRect, sFormat);
                        //ControlPaint.DrawBorder3D(e.Graphics, drawRect, Border3DStyle.Flat);
                    }

                    if (newEntry.parameterNames[7] != null)
                    {
                        drawRect = new Rectangle(cxColumnPar07, e.Bounds.Top, dxColumnPar07, e.Bounds.Height / 2);
                        e.Graphics.DrawString(newEntry.parameterNames[7], e.Font, foreBrush, drawRect, sFormat);
                        //ControlPaint.DrawBorder3D(e.Graphics, drawRect, Border3DStyle.Flat);

                        drawRect = new Rectangle(cxColumnPar07, e.Bounds.Top + e.Bounds.Height / 2, dxColumnPar07, e.Bounds.Height / 2);
                        e.Graphics.DrawString(newEntry.parameterVals[7], new Font("Segoe UI", 9, FontStyle.Bold), foreBrush, drawRect, sFormat);
                        //ControlPaint.DrawBorder3D(e.Graphics, drawRect, Border3DStyle.Flat);
                    }

                    if (newEntry.parameterNames[8] != null)
                    {
                        drawRect = new Rectangle(cxColumnPar08, e.Bounds.Top, dxColumnPar08, e.Bounds.Height / 2);
                        e.Graphics.DrawString(newEntry.parameterNames[8], e.Font, foreBrush, drawRect, sFormat);
                        //ControlPaint.DrawBorder3D(e.Graphics, drawRect, Border3DStyle.Flat);

                        drawRect = new Rectangle(cxColumnPar08, e.Bounds.Top + e.Bounds.Height / 2, dxColumnPar08, e.Bounds.Height / 2);
                        e.Graphics.DrawString(newEntry.parameterVals[8], new Font("Segoe UI", 9, FontStyle.Bold), foreBrush, drawRect, sFormat);
                        //ControlPaint.DrawBorder3D(e.Graphics, drawRect, Border3DStyle.Flat);
                    }
                }
            }
        }

        //This is only used for displaying the data
        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        /*There are 2 reasons why you would be in this:
         * 1 - You have selected something to view the data
         * 2 - You want to insert data
       */
        private void listBox1_Click(object sender, EventArgs e)
        {
            if (!(listBox1.SelectedItem == null))
            {
                //You are in here for the purposes of changing something
                if (insertSelected == false)
                {
                    //Gets data in the entry selected
                    EntryData entryData = new EntryData();
                    entryData = listBox1.SelectedItem as EntryData;

                    //Store the IDnum to be used when user clicks the "Change" button
                    STEPNUM_CHANGE = entryData.IDnum;

                    //Change Object
                    objNames.Text = entryData.objName;

                    //Change Function
                    funcNames.Text = entryData.funcName;

                    //Show values selected
                    for (int i = 1; i < parameterFields.GetLength(0); i++)
                    {
                        if (parameterFields[i].Visible == true)
                        {
                            parameterFields[i].Text = entryData.parameterVals[i];
                        }
                    }
                }
                //You want to insert something
                else
                {
                    int selectedIndex = listBox1.SelectedIndex;

                    if (selectedIndex != -1)
                    {

                        //This is the data that the user enters
                        EntryData entryData = new EntryData();

                        //This is the position that the user has clicked - with this, we can get the
                        //Stepnumber of the point the user wants to insert at
                        EntryData chosenInsertPoint = new EntryData();
                        chosenInsertPoint = listBox1.SelectedItem as EntryData;

                        //Check if it is connected
                        checkConnect();

                        //Need to find out the object ID for the object name selected
                        String columnIDObj = "ID";
                        DataSet objectID = getObjID();

                        //Need to find out the function ID for the function name selected
                        String columnIDFunc = "ID";
                        DataSet functionID = getFuncID();

                        //Get the functionRow
                        String tableName = "kex.macro_viewfunction";
                        String columnNameFunc = "ID";
                        String columnNameObj = "Object_ID";
                        DataSet functionData = mySqlInterface.Query("SELECT * FROM " + tableName + " WHERE "
                                                + columnNameFunc + "='" + functionID.Tables[0].Rows[0][columnIDFunc] +
                                                "'" + "and " + columnNameObj + "='" + objectID.Tables[0].Rows[0][columnIDObj].ToString() + "'");

                        //Get Data for the relevant function row
                        DataRow functionRow = functionData.Tables[0].Rows[0];

                        for (int i = 1; i < parameterFields.GetLength(0); i++)
                        {
                            if (parameterFields[i].Visible == true)
                            {
                                /*   Error Checking   */
                                String curFormat = "para0" + i.ToString() + "_format";
                                String curminLim = "para0" + i.ToString() + "_minlimit";
                                String curmaxLim = "para0" + i.ToString() + "_maxlimit";

                                submit_ErrorChecking(functionRow[curFormat].ToString(), parameterFields[i].Text,
                                    parameterTitles[i].Text, functionRow[curminLim].ToString(), functionRow[curmaxLim].ToString(), i);


                                //All error checking done for para0# and can be added to EntryData
                                if (userEntryError == false)
                                {
                                    //Populate Text
                                    entryData.parameterVals[i] = parameterFields[i].Text;

                                    //Populate Name
                                    entryData.parameterNames[i] = parameterTitles[i].Text;
                                }

                                //Reset userEntryError to check for the next parameter
                                userEntryError = false;
                            }
                            //Populate Object
                            entryData.objName = objNames.Text;

                            //Populate Function
                            entryData.funcName = funcNames.Text;
                        }

                        //No errors have occurred - thus the string for the errors is empty
                        if (userErrors == "")
                        {
                            DialogResult confirmation = MessageBox.Show("Are you sure you want to insert at Step " + chosenInsertPoint.IDnum + "?",
                                "Confirmation", MessageBoxButtons.YesNo);

                            if (confirmation == System.Windows.Forms.DialogResult.Yes)
                            {
                                //get TopIndex to stop moving cursor
                                int tempTopIndex = listBox1.TopIndex;

                                checkConnect();

                                String columnNameID = "ID";
                                String columnNameSeq = "name";
                                tableName = "kex.macro_viewsequence";

                                DataSet seqID = mySqlInterface.Query("SELECT " + columnNameID + " FROM " + tableName +
                                                                        " WHERE " + columnNameSeq + "=" + "'" +
                                                                        sqNames.Text + "'");

                                tableName = "kex.macro_viewchannel";
                                DataSet tableID = mySqlInterface.Query("SELECT " + columnNameID + " FROM " + tableName +
                                                        " WHERE " + columnNameSeq + "=" + "'" +
                                                        chNames.Text + "'");

                                //Need to find out the object ID for the object name selected
                                columnIDObj = "ID";
                                objectID = getObjID();

                                //Need to find out the function ID for the function name selected
                                String columnIDFuncNew = "ID";
                                DataSet functionIDNew = getFuncID();

                                mySqlInterface.runProcedure(
                                    null,
                                    "kex.macro_insertSequenceStep",
                                    "sequence_ID_in", seqID.Tables[0].Rows[0][columnNameID],
                                    "channel_ID_in", tableID.Tables[0].Rows[0][columnNameID],
                                    "stepnumber_in", Convert.ToInt32(chosenInsertPoint.IDnum),
                                    "function_ID_in", functionID.Tables[0].Rows[0][columnIDFuncNew],
                                    "object_ID_in", objectID.Tables[0].Rows[0][columnIDObj],
                                    "para01_in", entryData.parameterVals[1],
                                    "para02_in", entryData.parameterVals[2],
                                    "para03_in", entryData.parameterVals[3],
                                    "para04_in", entryData.parameterVals[4],
                                    "para05_in", entryData.parameterVals[5],
                                    "para06_in", entryData.parameterVals[6],
                                    "para07_in", entryData.parameterVals[7],
                                    "para08_in", entryData.parameterVals[8],
                                    "memo_in", "Insert Here");

                                tableName = "kex.macro_viewsequencestep";
                                columnNameSeq = "sequence_name";
                                String columnNameCh = "channel_name";

                                DataSet seqData = mySqlInterface.Query("SELECT * FROM " + tableName + " WHERE " +
                                             columnNameSeq + "=" + "'" + sqNames.Text + "'" + " and " +
                                             columnNameCh + "=" + "'" + chNames.Text + "'");

                                //Add entries to Array and display on screen
                                addtoArray(seqData);

                                //Null out values so next time a user enters something, the old text is not there
                                nullBoxes();

                                //Move cursor - doesn't select but just moves the viewing area
                                listBox1.TopIndex = tempTopIndex;
                            }
                        }
                        else
                        {
                            //Show the errors user has
                            MessageBox.Show("You have errors in your input. Please see below: \n" + userErrors);
                        }

                        //Reset userErrors String
                        userErrors = "";

                        //InsertSelected back to false
                        insertSelected = false;

                        //Enable Fields
                        foreach (var combobox in parameterFields)
                        {
                            //if (textbox != null) textbox.ReadOnly = false;
                            if (combobox != null) combobox.Enabled = true;
                        }
                        objNames.Enabled = true;
                        funcNames.Enabled = true;
                        chNames.Enabled = true;
                        sqNames.Enabled = true;
                        msg_box.Text = "";
                    }
                    else
                    {
                        MessageBox.Show("Please select an entry to insert at.");

                        //Reset userErrors String
                        userErrors = "";

                        //InsertSelected back to false
                        insertSelected = false;

                        //Enable Fields
                        foreach (var combobox in parameterFields)
                        {
                            //if (textbox != null) textbox.ReadOnly = false;
                            if (combobox != null) combobox.Enabled = true;
                        }
                        objNames.Enabled = true;
                        funcNames.Enabled = true;
                        chNames.Enabled = true;
                        sqNames.Enabled = true;
                        msg_box.Text = "";
                    }
                }
            }
        }

        /* Checks to see if the string entered is empty
         * */
        private Boolean checkifEmpty(String parameter, String parameterTitle)
        {
            if (parameter == "")
            {
                return true;
            }
            else return false;
        }

        /* Checks to see if the string entered is a string - not used anymore
         * */
        private void checkifString(String parameter, String parameterTitle)
        {

        }

        /* Checks to see if the string entered is a number
         * Also checks to see if the number is within the allowable limits
         * */
        private void checkifNumber(String parameter, String parameterTitle, String minLimit,
            String maxLimit)
        {
            double Doubnum;
            bool isDouble = double.TryParse(parameter, out Doubnum);

            if (isDouble == true)
            {
                if (minLimit != "" && maxLimit != "")
                {
                    double minLimitDouble = double.Parse(minLimit);
                    double maxLimitDouble = double.Parse(maxLimit);
                    double parameterDouble = double.Parse(parameter);

                    if ((parameterDouble > maxLimitDouble) ||
                        (parameterDouble < minLimitDouble))
                    {
                        userErrors = userErrors + parameterTitle +
                        " has a minlimit of " + minLimit + " and a maxlimit of " + maxLimit
                        + ". \n";
                        userEntryError = true;
                    }
                }
            }
            else
            {
                userErrors = userErrors + parameterTitle +
                " only uses a number format. Please enter a number for " +
                parameterTitle + ". \n";
                userEntryError = true;
            }
        }

        /* Checks to see if the time entered is valid -
         * The allowable variations are:
         * XXh, XX.XXh, XXm, XX.XXm, XXs, XX.XXs, XXms, XX.XXms where X represents a number
         * */
        private void checkifTime(String parameter, String parameterTitle)
        {
            if (!(Regex.IsMatch(parameter, @"\d\d[.]?(\d\d)?h")) &&
                !(Regex.IsMatch(parameter, @"\d\d[.]?(\d\d)?s")) &&
                !(Regex.IsMatch(parameter, @"\d\d[.]?(\d\d)?m")) &&
                !(Regex.IsMatch(parameter, @"\d\d[.]?(\d\d)?ms")))
            {
                userErrors = userErrors + parameterTitle +
                           " only uses a time format. Please enter a time for " +
                                parameterTitle + "." +
                                "An example is 04h03m02s03ms for 4 hours, 3 minutes, 2 seconds, " +
                                "2 milliseconds. \n";
                userEntryError = true;
            }
        }

        /* The following checks for the parameter type and does error checking 
         * for each entry 
         * */
        private void submit_ErrorChecking(String parameterType, String parameterVal,
            String parameterTitle, String minLimit, String maxLimit, int parmno)
        {
            //format is time
            if (parameterType == "time")
            {
                //Checks if field is empty
                if (!checkifEmpty(parameterVal, parameterTitle))
                {
                    //Checks if field is time
                    checkifTime(parameterVal, parameterTitle);
                }
            }
            //format of para0# is string
            else if (parameterType == "string")
            {
                //Checks if field is empty
                if (!checkifEmpty(parameterVal, parameterTitle))
                {
                    //Checks if field is string
                    checkifString(parameterVal, parameterTitle);
                }
            }
            //format of para0# is function
            else if (parameterType == "function")
            {
                //Checks if field is empty
                checkifEmpty(parameterVal, parameterTitle);
            }
            //format of para0# is number
            else if (parameterType == "number")
            {
                //Checks if field is empty
                if (!checkifEmpty(parameterVal, parameterTitle))
                {
                    //Checks if field is number
                    checkifNumber(parameterVal, parameterTitle, minLimit, maxLimit);
                }
            }
            //format of para0# is prefixnumber
            else if (parameterType == "prefixnumber")
            {
                //Checks if field is empty
                if (!checkifEmpty(parameterVal, parameterTitle))
                {
                    //Checks if field is number
                    checkifPrefixNumber(parameterVal, parameterTitle);
                }
            }
            //format of para0# is state
            else if (parameterType == "state")
            {
                //Checks if field is empty
                checkifEmpty(parameterVal, parameterTitle);
            }
            else
            {
                //Checks if field is empty
                checkifEmpty(parameterVal, parameterTitle);
            }
        }

        /* Checks to see if the string entered is of type "PrefixNumber"
         * The allowable types for this is:
         * <X, <-X, <X.XX, <-X.XX, >X, >-X, >X.XX, >-X.XX
         * <>X, <>-X, <>X.XX, <>-X.XX
         * */
        private void checkifPrefixNumber(string parameterVal, string parameterTitle)
        {
            if (!(Regex.IsMatch(parameterVal, @"^[<>]?(<>)?[-]?[+]?(\d+\,\d+)?(\d+)?$")))
            {
                userErrors = userErrors + parameterTitle +
                                " only uses a Prefixed Number format. Please enter a prefixed number for " +
                                parameterTitle + ". \n";
                userEntryError = true;
            }
        }

        /* Gets the channel ID by using the current channel name,
         * which is stored in chNames.Text
         * */
        private DataSet getChID()
        {
            String columnNameID = "ID";
            String tableName = "kex.macro_viewchannel";
            String columnNameSeq = "name";

            DataSet chID = mySqlInterface.Query("SELECT " + columnNameID + " FROM " + tableName +
                                    " WHERE " + columnNameSeq + "=" + "'" +
                                    chNames.Text + "'");

            return chID;
        }

        /* Gets the sequence ID by using the sequence name, which is stored in
         * sqNames.Text
         * */
        private DataSet getSeqID()
        {
            checkConnect();
            
            String columnNameID = "ID";
            String columnNameSeq = "name";
            String tableName = "kex.macro_viewsequence";
            
            DataSet seqID = mySqlInterface.Query("SELECT " + columnNameID + " FROM " + tableName +
		                                        " WHERE " + columnNameSeq + "=" + "'" +
		                                        sqNames.Text + "'");
		
            return seqID;
        }

        /* submit_Click - runs if user has selected the Submit button
         * 1) Verify if the user entered parameters conform to the parameter format
         *    of each parameter
         * 3) If there are no errors, run procedure to add to the database 
         * 4) Then, run a command to get the current sequence from database (after changes
         *    have been applied) and display it
         */
        private void submit_Click(object sender, EventArgs e)
        {
            //Check if it is connected
            checkConnect();

            //Need to find out the object ID for the object name selected
            String columnIDObj = "ID";
            DataSet objectID = getObjID();

            //Need to find out the function ID for the function name selected
            String columnIDFunc = "ID";
            DataSet functionID = getFuncID();

            String tableName = "kex.macro_viewfunction";
            String columnNameFunc = "ID";
            String columnNameObj = "Object_ID";

            DataSet functionData = mySqlInterface.Query("SELECT * FROM " + tableName + " WHERE "
                                    + columnNameFunc + "='" + functionID.Tables[0].Rows[0][columnIDFunc].ToString() + "'" + "and " +
                                    columnNameObj + "='" + objectID.Tables[0].Rows[0][columnIDObj].ToString() + "'");

            //Create new EntryData
            EntryData entryData = new EntryData();

            //Get Data for the relevant function row
            DataRow functionRow = functionData.Tables[0].Rows[0];

            //Iterate over all fields 
            for (int i = 1; i < parameterFields.GetLength(0); i++)
            {
                //Only do bottom tests if it is visible 
                if (parameterFields[i].Visible == true)
                {
                    String curFormat = "para0" + i.ToString() + "_format";
                    String curminLim = "para0" + i.ToString() + "_minlimit";
                    String curmaxLim = "para0" + i.ToString() + "_maxlimit";

                    submit_ErrorChecking(functionRow[curFormat].ToString(), parameterFields[i].Text,
                        parameterTitles[i].Text, functionRow[curminLim].ToString(), functionRow[curmaxLim].ToString(), i);
                    
                    //All error checking done for para0# and can be added to EntryData
                    if (userEntryError == false)
                    {
                        //Populate Text
                        entryData.parameterVals[i] = parameterFields[i].Text;

                        //Populate Name
                        entryData.parameterNames[i] = parameterTitles[i].Text;

                        //Populate Object
                        entryData.objName = objNames.Text;

                        //Populate Function
                        entryData.funcName = funcNames.Text;
                    }

                    //Reset userEntryError to check for the next parameter
                    userEntryError = false;
                }
            }

            //No errors have occurred - thus the string for the errors is empty
            if (userErrors == "")
            {
                //Set the ID Number
                entryData.IDnum = (allEntries.Count + 1).ToString();

                checkConnect();

                //Get the sequence ID
                String columnNameID = "ID";
                DataSet seqID = getSeqID();

                //Get the Channel ID
                DataSet chID = getChID();

                //Need to find out the object ID for the object name selected
                columnIDObj = "ID";
                objectID = getObjID();

                //Need to find out the function ID for the function name selected
                columnIDFunc = "ID";
                functionID = getFuncID();

                mySqlInterface.runProcedure(
                    null,
                    "kex.macro_insertSequenceStep",
                    "sequence_ID_in", seqID.Tables[0].Rows[0][columnNameID],
                    "channel_ID_in", chID.Tables[0].Rows[0][columnNameID],
                    "stepnumber_in", -1,
                    "function_ID_in", functionID.Tables[0].Rows[0][columnIDFunc].ToString(),
                    "object_ID_in", objectID.Tables[0].Rows[0][columnIDObj].ToString(),
                    "para01_in", entryData.parameterVals[1],
                    "para02_in", entryData.parameterVals[2],
                    "para03_in", entryData.parameterVals[3],
                    "para04_in", entryData.parameterVals[4],
                    "para05_in", entryData.parameterVals[5],
                    "para06_in", entryData.parameterVals[6],
                    "para07_in", entryData.parameterVals[7],
                    "para08_in", entryData.parameterVals[8],
                    "memo_in", "Insert Here");

                tableName = "kex.macro_viewsequencestep";
                String columnNameSeq = "sequence_name";
                String columnNameCh = "channel_name";

                DataSet seqData = mySqlInterface.Query("SELECT * FROM " + tableName + " WHERE " +
                             columnNameSeq + "=" + "'" + sqNames.Text + "'" + " and " +
                             columnNameCh + "=" + "'" + chNames.Text + "'");

                //Add entries to Array and display on screen
                addtoArray(seqData);

                //Null out values so next time a user enters something, the old text is not there
                nullBoxes();

                //Move cursor
                listBox1.SelectedIndex = allEntries.Count - 1;
            }
            else
            {
                //Show the errors user has
                MessageBox.Show("You have errors in your input. Please see below: \n" + userErrors);
            }

            //Reset userErrors String
            userErrors = "";
        }

        /* If user has clicked the change button, do a couple of things
         * 1) First checks if user has selected anything
         * 2) Then verify if the user entered parameters conform to the parameter format
         *    of each parameter
         * 3) If there are no errors, run procedure to update the database 
         * 4) Then, run a command to get the current sequence from database (after changes
         *    have been applied) and display it
         * */
        private void change_Click(object sender, EventArgs e)
        {
            //Get Selected index
            int currentIndex = listBox1.SelectedIndex;

            //Check if something is selected
            if (currentIndex == -1)
            {
                DialogResult confirmation = MessageBox.Show("Please Select a Step");
            }
            else
            {
                //Need to find out the object ID for the object name selected
                String columnIDObj = "ID";
                DataSet objectID = getObjID();

                //Need to find out the function ID for the function name selected
                String columnIDFunc = "ID";
                DataSet functionID = getFuncID();

                String tableName = "kex.macro_viewfunction";
                String columnNameFunc = "ID";
                String columnNameObj = "Object_ID";

                DataSet functionData = mySqlInterface.Query("SELECT * FROM " + tableName + " WHERE "
                                        + columnNameFunc + "='" + functionID.Tables[0].Rows[0][columnIDFunc].ToString() + "'" + "and " +
                                        columnNameObj + "='" + objectID.Tables[0].Rows[0][columnIDObj].ToString() + "'");

                //Get Data for the relevant function row
                DataRow functionRow = functionData.Tables[0].Rows[0];

                //Add a new entryData
                EntryData entryData = new EntryData();

                //Iterate over all parameters
                for (int i = 1; i < parameterFields.GetLength(0); i++)
                {
                    //Only if field is visible
                    if (parameterFields[i].Visible == true)
                    {
                        String curFormat = "para0" + i.ToString() + "_format";
                        String curminLim = "para0" + i.ToString() + "_minlimit";
                        String curmaxLim = "para0" + i.ToString() + "_maxlimit";

                        submit_ErrorChecking(functionRow[curFormat].ToString(), parameterFields[i].Text,
                            parameterTitles[i].Text, functionRow[curminLim].ToString(), functionRow[curmaxLim].ToString(), i);

                        if (userEntryError == false)
                        {
                            //Edit that particular entry in the entryData array
                            entryData.parameterVals[i] = parameterFields[i].Text;
                        }

                        //Reset userEntryError to check for the next parameter
                        userEntryError = false;
                    }
                }

                //No Errors
                if (userErrors == "")
                {
                    checkConnect();

                    //Get the sequence ID
                    String columnNameID = "ID";
                    String columnNameSeq = "name";
                    tableName = "kex.macro_viewsequence";
                    DataSet seqID = mySqlInterface.Query("SELECT " + columnNameID + " FROM " + tableName +
                                                            " WHERE " + columnNameSeq + "=" + "'" +
                                                            sqNames.Text + "'");

                    //Get the Channel ID
                    tableName = "kex.macro_viewchannel";
                    DataSet tableID = mySqlInterface.Query("SELECT " + columnNameID + " FROM " + tableName +
                                            " WHERE " + columnNameSeq + "=" + "'" +
                                            chNames.Text + "'");

                    //Need to find out the object ID for the object name selected
                    columnIDObj = "ID";
                    objectID = getObjID();

                    //Need to find out the function ID for the function name selected
                    columnIDFunc = "ID";
                    functionID = getFuncID();

                    //Run Procedure
                    mySqlInterface.runProcedure(
                        null,
                        "kex.macro_updateSequenceStep",
                        "sequence_ID_in", seqID.Tables[0].Rows[0][columnNameID],
                        "channel_ID_in", tableID.Tables[0].Rows[0][columnNameID],
                        "stepnumber_in", Convert.ToInt32(STEPNUM_CHANGE),
                        "function_ID_in", functionID.Tables[0].Rows[0][columnIDFunc],
                        "object_ID_in", objectID.Tables[0].Rows[0][columnIDObj],
                        "para01_in", entryData.parameterVals[1],
                        "para02_in", entryData.parameterVals[2],
                        "para03_in", entryData.parameterVals[3],
                        "para04_in", entryData.parameterVals[4],
                        "para05_in", entryData.parameterVals[5],
                        "para06_in", entryData.parameterVals[6],
                        "para07_in", entryData.parameterVals[7],
                        "para08_in", entryData.parameterVals[8],
                        "memo_in", "Insert Here");

                    tableName = "kex.macro_viewsequencestep";
                    columnNameSeq = "sequence_name";
                    String columnNameCh = "channel_name";

                    DataSet seqData = mySqlInterface.Query("SELECT * FROM " + tableName + " WHERE " +
                                 columnNameSeq + "=" + "'" + sqNames.Text + "'" + " and " +
                                 columnNameCh + "=" + "'" + chNames.Text + "'");

                    //Add entries to Array and display on screen
                    addtoArray(seqData);

                    //Refocus cursor
                    listBox1.SelectedIndex = currentIndex;
                }
                else
                {
                    //Show the errors user has
                    MessageBox.Show("You have errors in your input. Please see below: \n" + userErrors);
                }

                //Reset userErrors String
                userErrors = "";
            }
        }

        /* remove_Click - run when user clicks the "Remove" button
         * 1) First, check if user has selected anything
         * 2) Then, get the current sequence ID and channel ID 
         * 3) Run procedure to delete step number from specific sequence and specific channel
         * 4) Then, run a command to get the current sequence from database (after changes
         *    have been applied) and display it
         * */
        private void remove_Click(object sender, EventArgs e)
        {
            //Get Selected index
            int currentIndex = listBox1.SelectedIndex;

            if (currentIndex != -1)
            {
                int tempTopIndex = listBox1.TopIndex;

                DialogResult confirmation = MessageBox.Show("Are you sure you want to remove Step " + (allEntries[currentIndex].IDnum).ToString() + "?",
                            "Confirmation", MessageBoxButtons.YesNo);

                if (confirmation == System.Windows.Forms.DialogResult.Yes)
                {
                    checkConnect();

                    //Get the sequence ID
                    String columnNameID = "ID";
                    String columnNameSeq = "name";
                    String tableName = "kex.macro_viewsequence";
                    DataSet seqID = mySqlInterface.Query("SELECT " + columnNameID + " FROM " + tableName +
                                                            " WHERE " + columnNameSeq + "=" + "'" +
                                                            sqNames.Text + "'");

                    //Get the Channel ID
                    tableName = "kex.macro_viewchannel";
                    DataSet tableID = mySqlInterface.Query("SELECT " + columnNameID + " FROM " + tableName +
                                            " WHERE " + columnNameSeq + "=" + "'" +
                                            chNames.Text + "'");

                    mySqlInterface.runProcedure(
                        null,
                        "kex.macro_deleteSequenceStep",
                        "sequence_ID_in", seqID.Tables[0].Rows[0][columnNameID],
                        "channel_ID_in", tableID.Tables[0].Rows[0][columnNameID],
                        "stepnumber_in", Convert.ToInt32(allEntries[currentIndex].IDnum));

                    tableName = "kex.macro_viewsequencestep";
                    columnNameSeq = "sequence_name";
                    String columnNameCh = "channel_name";

                    DataSet seqData = mySqlInterface.Query("SELECT * FROM " + tableName + " WHERE " +
                                 columnNameSeq + "=" + "'" + sqNames.Text + "'" + " and " +
                                 columnNameCh + "=" + "'" + chNames.Text + "'");

                    //Add entries to Array and display on screen
                    addtoArray(seqData);

                    //Refocus cursor
                    listBox1.TopIndex = tempTopIndex;
                }
            }
            else
            {
                MessageBox.Show("Please select an entry to remove.");
            }
        }

        /* insert_Click - runs when user clicks the "Insert" button
         * 1) Make all fields false and then ask user to select position
         *    to insert
         * 2) This will then redirect to the listBox1_Click function where 
         *    everything else is checked. I use the flag "insertSelected" to
         *    check whether or not user wants to insert something when clicking
         *    an entry or if user just wants to view data
         */
        private void insert_Click(object sender, EventArgs e)
        {
            //Disable Fields
            foreach (var combobox in parameterFields)
            {
                //if (textbox != null) textbox.ReadOnly = true;
                if (combobox != null) combobox.Enabled = false;
            }
            objNames.Enabled = false;
            funcNames.Enabled = false;
            chNames.Enabled = false;
            sqNames.Enabled = false;

            insertSelected = true;

            //Display Message
            msg_box.Font = new Font(msg_box.Font, FontStyle.Bold);
            msg_box.Text = "Please Select Insert Position";
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (mySqlInterface != null)
            {
                mySqlInterface.Close();
            }
        }

        /* This runs when user clicks the "Activate" button - it will
         * run a procedure to set the current sequence to the "active"
         * sequence and be ready to run.
         */
        private void activateSeq_Click(object sender, EventArgs e)
        {
            //Get the sequence ID
            String columnNameID = "ID";
            DataSet seqID = getSeqID();

            checkConnect();

            DialogResult confirmation = MessageBox.Show("Are you sure you want to activate sequence " + sqNames.Text + "?",
                            "Confirmation", MessageBoxButtons.YesNo);

            if (confirmation == System.Windows.Forms.DialogResult.Yes)
            {
                mySqlInterface.runProcedure(
                    null,
                    "kex.macro_activateSequence",
                    "sequence_ID_in", seqID.Tables[0].Rows[0][columnNameID]);
            }
        }

        /* This refreshes the sequence combobox if the combobox is clicked
         * In turn, this also refreshes the chart
         */ 
        private void sqNames_Click(object sender, EventArgs e)
        {
            String currentSq = sqNames.Text;

            refreshSeq = true;

            checkConnect();

            //get Dataset for sequenceData
            String tableName = "kex.macro_viewsequence";
            String columnNameSeq = "name";
            DataSet sequenceData = mySqlInterface.Query("SELECT " + columnNameSeq + " FROM " + tableName);

            //Fill Sequences with sequenceData
            refreshsqComboBox(sequenceData, "name");

            sqNames.Text = currentSq;

        }

        private void newSeq_Click(object sender, EventArgs e)
        {
            DialogResult confirmation = MessageBox.Show("Use Sequence '" + sqNames.Text + "' as Template?",
                        "Confirmation", MessageBoxButtons.YesNo);

            if (confirmation == System.Windows.Forms.DialogResult.Yes)
            {
                String newSeqName = sqNames.Text + "-Copy";
                if ((InputBox("Create new Sequence", "Input new Sequence Name", ref newSeqName) == DialogResult.OK)
                    && (newSeqName != ""))
                {
                    //Get the sequence ID
                    String columnNameID = "ID";
                    String columnNameSeq = "name";
                    String tableName = "kex.macro_viewsequence";
                    DataSet seqID = mySqlInterface.Query("SELECT " + columnNameID + " FROM " + tableName +
                                                            " WHERE " + columnNameSeq + "=" + "'" +
                                                            sqNames.Text + "'");

                    mySqlInterface.runProcedure(
                        null,
                        "kex.macro_createSequence",
                        "sequence_name", newSeqName,
                        "template_ID", seqID.Tables[0].Rows[0][columnNameID]);
                }
            }
            else
            {
                String newSeqName = "New Sequence";
                if ((InputBox("Create new Sequence", "Input new Sequence Name", ref newSeqName) == DialogResult.OK)
                    && (newSeqName != ""))
                {
                    checkConnect();

                    //Get the sequence ID
                    String columnNameID = "ID";
                    String columnNameSeq = "name";
                    String tableName = "kex.macro_viewsequence";
                    DataSet seqID = mySqlInterface.Query("SELECT " + columnNameID + " FROM " + tableName +
                                                            " WHERE " + columnNameSeq + "=" + "'" +
                                                            sqNames.Text + "'");

                    mySqlInterface.runProcedure(
                        null,
                        "kex.macro_createSequence",
                        "sequence_name", newSeqName,
                        "template_ID", null);
                }
            }
        }

        public static DialogResult InputBox(string title, string promptText, ref string value)
        {
          Form form = new Form();
          Label label = new Label();
          TextBox textBox = new TextBox();
          Button buttonOk = new Button();
          Button buttonCancel = new Button();

          form.Text = title;
          label.Text = promptText;
          textBox.Text = value;

          buttonOk.Text = "OK";
          buttonCancel.Text = "Cancel";
          buttonOk.DialogResult = DialogResult.OK;
          buttonCancel.DialogResult = DialogResult.Cancel;

          label.SetBounds(9, 20, 372, 13);
          textBox.SetBounds(12, 36, 372, 20);
          buttonOk.SetBounds(228, 72, 75, 23);
          buttonCancel.SetBounds(309, 72, 75, 23);

          label.AutoSize = true;
          textBox.Anchor = textBox.Anchor | AnchorStyles.Right;
          buttonOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
          buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

          form.ClientSize = new Size(396, 107);
          form.Controls.AddRange(new Control[] { label, textBox, buttonOk, buttonCancel });
          form.ClientSize = new Size(Math.Max(300, label.Right + 10), form.ClientSize.Height);
          form.FormBorderStyle = FormBorderStyle.FixedDialog;
          form.StartPosition = FormStartPosition.CenterScreen;
          form.MinimizeBox = false;
          form.MaximizeBox = false;
          form.AcceptButton = buttonOk;
          form.CancelButton = buttonCancel;

          DialogResult dialogResult = form.ShowDialog();
          value = textBox.Text;
          return dialogResult;
        }

        private void serverList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (mySQLServer != serverList.Text)
            {
                try
                {
                    mySqlInterface.Close();
                    mySQLServer = serverList.Text;
                    checkConnect();
                    displayVersion();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.GetBaseException().ToString(), "Error",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                    mySQLServer = "<Error>";
                }
            }
        }
    }
}
