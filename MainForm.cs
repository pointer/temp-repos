using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Threading;
using ServiceHostUtility;

namespace DCCLocomotiveFactory
{
    public partial class MainForm : Form
    {
        const int LocoDictSize = 10;

        class ServerStatus
        {
            public const string
                Stopped = "Stopped",
                Running = "Running";
        }

        DataSet m_locomotives = new DataSet();
        Dictionary<string, ThreadedServiceHost<DCCLocomotiveService, IDCCLocomotiveContract>> m_locoDict =
            new Dictionary<string, ThreadedServiceHost<DCCLocomotiveService, IDCCLocomotiveContract>>(LocoDictSize);

        public MainForm()
        {
            InitializeComponent();

            btnPilotLoco.Enabled = false;

            try
            {
                m_locomotives.ReadXml(Properties.Settings.Default.LocoListFileb);

                DataTable locoTable = m_locomotives.Tables["Locomotive"];
                DataRowCollection dataRows = locoTable.Rows;
                for (int nI = 0; nI < dataRows.Count; nI++)
                {
                    LocoDataRow row = new LocoDataRow(dataRows[nI]);

                    locoListBox.Items.Add(row);
                }
            }
            catch (Exception ex)
            {
                toolStripStatusLabel.Text = ex.Message;
            }
        }


        class LocoDataRow 
        {
            DataRow m_row;

            public LocoDataRow(DataRow theRow)
            {
                m_row = theRow;
            }

            public override string ToString()
            {
                return m_row["Name"].ToString();
            }

            public object   this[string name]
            {
                get
                {
                    return m_row[name];
                }

                set
                {
                    m_row["Name"] = value;
                }
            }

            public DataRow Row
            {
                get
                {
                    return m_row;
                }
            }
        }

        private void locoListBox_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            try
            {
                // 1 - Get the data for that loco
                LocoDataRow locoData = (LocoDataRow)locoListBox.Items[e.Index];

                if (e.NewValue == CheckState.Checked)
                {
                    // Start the server for the given loco
                    StartLocoServer(locoData.Row);
                }
                else
                {
                    // Stop the server for the given loco
                    StopLocoServer(locoData.Row);
                }
            }
            catch (Exception ex)
            {
                toolStripStatusLabel.Text = ex.Message;
            }
        }


        private void StartLocoServer(DataRow locoData)
        {
            // Get the data
            string name = locoData[Constants.ColumnName.Name].ToString();;
            string address = locoData[Constants.ColumnName.Address].ToString();

            // Create the Host
            string serviceAddress = string.Format(Constants.LocoServerBaseAddress, address);
            ThreadedServiceHost<DCCLocomotiveService, IDCCLocomotiveContract> locoHost =
                new ThreadedServiceHost<DCCLocomotiveService, IDCCLocomotiveContract>(serviceAddress, address, new NetTcpBinding());

            // Add to dictionary of servers
            m_locoDict.Add(address, locoHost);

            SetLocoServiceInfo(locoData);
        }


        private void SetLocoServiceInfo(DataRow locoData)
        {
            string address = locoData[Constants.ColumnName.Address].ToString();
            textBoxAddress.Text = address;
            textBoxName.Text = locoData[Constants.ColumnName.Name].ToString();

            // Try to get the Service host
            if (m_locoDict.ContainsKey(address))
            {
                // Access to service to get current info
                textBoxStatus.Text = ServerStatus.Running;
                textBoxStatus.BackColor = Color.PaleGreen;
            }
            else
            {
                textBoxStatus.Text = ServerStatus.Stopped;
                textBoxStatus.BackColor = Color.LightPink;
            }

            toolStripStatusLabel.Text = string.Empty;
        }


        private void StopLocoServer(DataRow locodata)
        {
            // Get the loco server host,close and remove from dictionary
            string address = locodata[Constants.ColumnName.Address].ToString();
            m_locoDict[address].Stop();

            // Remove from dictionary servers
            m_locoDict.Remove(address);

            SetLocoServiceInfo(locodata);
        }

        private void btnPilotLoco_Click(object sender, EventArgs e)
        {
            try
            {
                LocoDataRow locoDataRow = (LocoDataRow)locoListBox.Items[locoListBox.SelectedIndex];
                LocoControlForm locoForm = new LocoControlForm(
                    locoDataRow.Row[Constants.ColumnName.Address].ToString(),
                    locoDataRow.Row[Constants.ColumnName.Name].ToString(),
                    true);

                locoForm.Show();
            }
            catch (Exception ex)
            {
                toolStripStatusLabel.Text = ex.Message;
            }
        }

        private void locoListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            int idx = locoListBox.SelectedIndex;

            btnPilotLoco.Enabled = locoListBox.GetItemChecked(idx);

            LocoDataRow locoDataRow = (LocoDataRow)locoListBox.Items[idx];

            SetLocoServiceInfo(locoDataRow.Row);
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            // Stop all the running Loco servers
            foreach (string key in m_locoDict.Keys)
            {
                try
                {
                    m_locoDict[key].Stop();
                }
                catch (Exception)
                {
                }
            }
        }
    }
}
