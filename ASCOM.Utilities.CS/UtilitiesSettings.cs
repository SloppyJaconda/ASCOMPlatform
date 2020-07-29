using System;
using System.IO;
using Microsoft.Win32;

namespace ASCOM.Utilities.CS
{
    // Class to manage state storage for platform components e.g. profile tracing enabled/disabled

    // To add a new saved value:
    // 1) Decide on the variable name and its default value
    // 2) Create appropriately named constants similar to those below
    // 3) Create a property of the relevant type in the parameters section
    // 4) Create Get and Set code based on the patterns already implemented
    // 5) If the property is of a type not already handled,you will need to create a GetXXX function in the Utility code region

    internal class UtilitiesSettings : IDisposable
    {
        private RegistryKey m_HKCU, m_SettingsKey;
        private const string REGISTRY_CONFORM_FOLDER = @"Software\ASCOM\Utilities";

        // Constants used in the Parameters section
        private const string TRACE_XMLACCESS = "Trace XMLAccess"; // Enable XML Access tracing
        private const bool TRACE_XMLACCESS_DEFAULT = false;
        private const string TRACE_PROFILE = "Trace Profile"; // Enable Profile Tracing
        private const bool TRACE_PROFILE_DEFAULT = false;
        private const string PROFILE_ROOT_EDIT = "Profile Root Edit"; // Allow root editing in Profile Explorer
        private const bool PROFILE_ROOT_EDIT_DEFAULT = false;

        public UtilitiesSettings()
        {
            m_HKCU = Registry.CurrentUser;
            m_HKCU.CreateSubKey(REGISTRY_CONFORM_FOLDER);
            m_SettingsKey = m_HKCU.OpenSubKey(REGISTRY_CONFORM_FOLDER, true);
        }

        private bool disposedValue = false;        // To detect redundant calls

        // IDisposable
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                if (disposing)
                {
                }
                m_SettingsKey.Flush();
                m_SettingsKey.Close();
                m_SettingsKey = null;
                m_HKCU.Flush();
                m_HKCU.Close();
                m_HKCU = null;
            }
            this.disposedValue = true;
        }

        // This code added by Visual Basic to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~UtilitiesSettings()
        {
        }

        public bool TraceXMLAccess
        {
            get
            {
                return GetBool(TRACE_XMLACCESS, TRACE_XMLACCESS_DEFAULT);
            }
            set
            {
                SetName(m_SettingsKey, TRACE_XMLACCESS, value.ToString());
            }
        }

        public bool TraceProfile
        {
            get
            {
                return GetBool(TRACE_PROFILE, TRACE_PROFILE_DEFAULT);
            }
            set
            {
                SetName(m_SettingsKey, TRACE_PROFILE, value.ToString());
            }
        }

        public bool ProfileRootEdit
        {
            get
            {
                return GetBool(PROFILE_ROOT_EDIT, PROFILE_ROOT_EDIT_DEFAULT);
            }
            set
            {
                SetName(m_SettingsKey, PROFILE_ROOT_EDIT, value.ToString());
            }
        }


        private bool GetBool(string p_Name, bool p_DefaultValue)
        {
            bool l_Value;
            try
            {
                if (m_SettingsKey.GetValueKind(p_Name) == RegistryValueKind.String)
                    l_Value = System.Convert.ToBoolean(m_SettingsKey.GetValue(p_Name));
            }
            catch (IOException ex)
            {
                SetName(m_SettingsKey, p_Name, p_DefaultValue.ToString());
                l_Value = p_DefaultValue;
            }
            catch (Exception ex)
            {
                // LogMsg("GetBool", GlobalVarsAndCode.MessageLevel.msgError, "Unexpected exception: " & ex.ToString)
                l_Value = p_DefaultValue;
            }
            return l_Value;
        }
        private string GetString(string p_Name, string p_DefaultValue)
        {
            string l_Value;
            l_Value = "";
            try
            {
                if (m_SettingsKey.GetValueKind(p_Name) == RegistryValueKind.String)
                    l_Value = m_SettingsKey.GetValue(p_Name).ToString();
            }
            catch (IOException ex)
            {
                SetName(m_SettingsKey, p_Name, p_DefaultValue.ToString());
                l_Value = p_DefaultValue;
            }
            catch (Exception ex)
            {
                // LogMsg("GetString", GlobalVarsAndCode.MessageLevel.msgError, "Unexpected exception: " & ex.ToString)
                l_Value = p_DefaultValue;
            }
            return l_Value;
        }
        private double GetDouble(RegistryKey p_Key, string p_Name, double p_DefaultValue)
        {
            double l_Value;
            // LogMsg("GetDouble", GlobalVarsAndCode.MessageLevel.msgDebug, p_Name.ToString & " " & p_DefaultValue.ToString)
            try
            {
                if (p_Key.GetValueKind(p_Name) == RegistryValueKind.String)
                    l_Value = System.Convert.ToDouble(p_Key.GetValue(p_Name));
            }
            catch (IOException ex)
            {
                SetName(p_Key, p_Name, p_DefaultValue.ToString());
                l_Value = p_DefaultValue;
            }
            catch (Exception ex)
            {
                // LogMsg("GetDouble", GlobalVarsAndCode.MessageLevel.msgError, "Unexpected exception: " & ex.ToString)
                l_Value = p_DefaultValue;
            }
            return l_Value;
        }
        private DateTime GetDate(string p_Name, DateTime p_DefaultValue)
        {
            DateTime l_Value;
            try
            {
                if (m_SettingsKey.GetValueKind(p_Name) == RegistryValueKind.String)
                    l_Value = (DateTime)m_SettingsKey.GetValue(p_Name);
            }
            catch (IOException ex)
            {
                SetName(m_SettingsKey, p_Name, p_DefaultValue.ToString());
                l_Value = p_DefaultValue;
            }
            catch (Exception ex)
            {
                // LogMsg("GetDate", GlobalVarsAndCode.MessageLevel.msgError, "Unexpected exception: " & ex.ToString)
                l_Value = p_DefaultValue;
            }
            return l_Value;
        }
        private void SetName(RegistryKey p_Key, string p_Name, string p_Value)
        {
            p_Key.SetValue(p_Name, p_Value.ToString(), RegistryValueKind.String);
            p_Key.Flush();
        }
    }

}
