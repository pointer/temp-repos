using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace DCCLocomotiveFactory
{
    public partial class LocoControlForm : Form
    {
        ChannelFactory<IDCCLocomotiveChannel> m_dccLocoFactory;
        IDCCLocomotiveChannel m_locoChannel = null;

        DCCLocomotiveService m_current = new DCCLocomotiveService();
        UpdateDisplayManager m_displayMgr = null;

        public LocoControlForm(string address, string name, bool canUpdate)
        {
            InitializeComponent();

            m_displayMgr = new UpdateDisplayManager(this, canUpdate);
           
            try
            {
                labelName.Text = name;
                labelAddress.Text = address;

                EndpointAddress endPoint = new EndpointAddress(
                    new Uri(string.Format(Constants.LocoServerBaseAddress, address) + address));
                System.ServiceModel.Channels.Binding binding = new NetTcpBinding();
                m_dccLocoFactory = new ChannelFactory<IDCCLocomotiveChannel>(binding, endPoint);
                m_dccLocoFactory.Open();

                m_locoChannel = m_dccLocoFactory.CreateChannel();

                m_locoChannel.Open();

                //UpdateLocoDisplay();
                m_displayMgr.Update();

                updateTimer.Start();
            }
            catch (Exception)
            {
                MessageBox.Show("Cannot connect to locomotive with address: " + address);
                this.Close();
            }
        }


        #region Accessors used by the UpdateDisplayManager class
        public IDCCLocomotiveContract CurrentLoco
        {
            get
            {
                return m_current;
            }
        }

        public IDCCLocomotiveContract RemoteLoco
        {
            get
            {
                return m_locoChannel;
            }
        }

        public NumericUpDown SpeedControl
        {
            get
            {
                return numericUpDownSpeed;
            }
        }

        public CheckBox LightControl
        {
            get
            {
                return checkBoxLight;
            }
        }

        public ComboBox DirectionControl
        {
            get
            {
                return comboBoxDirection;
            }
        }

        public Label SpeedLabel
        {
            get
            {
                return labelSpeed;
            }
        }

        public Label DirectionLabel
        {
            get
            {
                return labelDirection;
            }
        }

        public Label LightLabel
        {
            get
            {
                return labelLight;
            }
        }

        public Button UpdateButton
        {
            get
            {
                return btnChange;
            }
        }
        #endregion


        #region Event handlers
        protected override void OnClosed(EventArgs e)
        {
            try
            {

                if (m_locoChannel != null)
                    m_locoChannel.Close();

                if (m_locoChannel != null)
                    m_dccLocoFactory.Close();

                updateTimer.Stop();
            }
            catch (Exception)
            {
            }

            base.OnClosed(e);
        }

        private void checkBoxLight_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxLight.Checked)
                checkBoxLight.Text = "On";
            else
                checkBoxLight.Text = "Off";
        }

        private void btnChange_Click(object sender, EventArgs e)
        {
            try
            {
                m_locoChannel.SetSpeed((byte)numericUpDownSpeed.Value);
                m_locoChannel.SwitchLight(checkBoxLight.Checked);
                m_locoChannel.ChangeDirection(((string)comboBoxDirection.SelectedItem) == "Forward" ? Direction.Forward : Direction.Reverse);
            }
            catch (Exception)
            {
            }
        }

        private void updateTimer_Tick(object sender, EventArgs e)
        {
            // Check if anything changed in the loco
            try
            {
                if (NewLocoData(m_current, m_locoChannel))
                    m_displayMgr.Update();
                    //UpdateLocoDisplay();
            }
            catch (Exception)
            {
            }
        }
        #endregion

        private bool NewLocoData(IDCCLocomotiveContract current, IDCCLocomotiveContract newData)
        {
            bool bRet = false;

            if (current.GetSpeed() != newData.GetSpeed())
            {
                bRet = true;
            }
            else if (current.GetLightState() != newData.GetLightState())
            {
                bRet = true;
            }
            else if (current.GetDirection() != newData.GetDirection())
            {
                bRet = true;
            }

            return bRet;
        }
    }


    /// <summary>
    /// This class is used to update the display with values read
    /// from the IDCCLocomotiveChannel
    /// </summary>
    class UpdateDisplayManager
    {
        private bool m_canUpdate;
        private LocoControlForm m_form;

        public UpdateDisplayManager(LocoControlForm form, bool canUpdate)
        {
            m_canUpdate = canUpdate;
            m_form = form;

            if (!canUpdate)
                m_form.Text = "Locomotive Monitor";

            m_form.DirectionControl.Visible = canUpdate;
            m_form.DirectionLabel.Visible = !canUpdate;
            m_form.LightControl.Visible = canUpdate;
            m_form.LightLabel.Visible = !canUpdate;
            m_form.SpeedControl.Visible = canUpdate;
            m_form.SpeedLabel.Visible = !canUpdate;
            m_form.UpdateButton.Visible = canUpdate;
        }

        public void Update()
        {
            m_form.CurrentLoco.SwitchLight(m_form.RemoteLoco.GetLightState());
            m_form.CurrentLoco.SetSpeed(m_form.RemoteLoco.GetSpeed());
            m_form.CurrentLoco.ChangeDirection(m_form.RemoteLoco.GetDirection());

            string direction = m_form.CurrentLoco.GetDirection().ToString();

            if (m_canUpdate)
            {
                m_form.SpeedControl.Value = (decimal)m_form.CurrentLoco.GetSpeed();
                m_form.DirectionControl.SelectedItem = direction;
                m_form.LightControl.Checked = m_form.CurrentLoco.GetLightState();
            }
            else
            {
                m_form.SpeedLabel.Text = m_form.CurrentLoco.GetSpeed().ToString();
                m_form.DirectionLabel.Text = direction;
                m_form.LightLabel.Text = m_form.CurrentLoco.GetLightState() ? "On" : "Off";
            }
        }
    }

/// <summary>
/// Interface used to create the Proxy for IDCCLocomotiveContract
/// </summary>
interface IDCCLocomotiveChannel : IDCCLocomotiveContract, IClientChannel
{
}
}