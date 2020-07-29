using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic;
using System.Security.AccessControl;
using System.Security.Principal;
using Microsoft.Win32;
using ASCOM.Utilities.CS.Interfaces;
using ASCOM.Utilities.Exceptions;
using System.Xml;
using System.Xml.Serialization;
using System.Environment;
using System.Runtime.InteropServices;

namespace ASCOM.Utilities.CS
{
// Class to read and write profile values to the registry



internal class RegistryAccess : IAccess, IDisposable
    {
        private List<IntPtr> HandleList = new List<IntPtr>();

        private RegistryKey ProfileRegKey;

        private System.Threading.Mutex ProfileMutex;
        private bool GotMutex;

        private TraceLogger TL;
        private bool disposedValue = false;        // To detect redundant calls to IDisposable
        private bool DisableTLOnExit = true;

        private Stopwatch sw, swSupport;

        /// <summary>
        ///     ''' Enum containing all the possible registry access rights values. The built-in RegistryRights enum only has a partial collection
        ///     ''' and often returns values such as -1 or large positive and negative integer values when converted to a string
        ///     ''' The Flags attribute ensures that the ToString operation returns an aggregate list of discrete values
        ///     ''' </summary>
        [Flags()]
        enum AccessRights
        {
            Query = 1,
            SetKey = 2,
            CreateSubKey = 4,
            EnumSubkey = 8,
            Notify = 0x10,
            CreateLink = 0x20,
            Unknown40 = 0x40,
            Unknown80 = 0x80,
            Wow64_64Key = 0x100,
            Wow64_32Key = 0x200,
            Unknown400 = 0x400,
            Unknown800 = 0x800,
            Unknown1000 = 0x1000,
            Unknown2000 = 0x2000,
            Unknown4000 = 0x4000,
            Unknown8000 = 0x8000,
            StandardDelete = 0x10000,
            StandardReadControl = 0x20000,
            StandardWriteDAC = 0x40000,
            StandardWriteOwner = 0x80000,
            StandardSynchronize = 0x100000,
            Unknown200000 = 0x200000,
            Unknown400000 = 0x400000,
            AuditAccess = 0x800000,
            AccessSystemSecurity = 0x1000000,
            MaximumAllowed = 0x2000000,
            Unknown4000000 = 0x4000000,
            Unknown8000000 = 0x8000000,
            GenericAll = 0x10000000,
            GenericExecute = 0x20000000,
            GenericWrite = 0x40000000,
            GenericRead = 0x80000000
        }

        public RegistryAccess() : this(false) // Create but respect any exceptions thrown
        {
        }

        public RegistryAccess(TraceLogger TraceLoggerToUse)
        {
            // This initiator is called by UninstallASCOM in order to pass in its TraceLogger so that all output appears in one file
            TL = TraceLoggerToUse;
            DisableTLOnExit = false;

            RunningVersions(TL);

            sw = new Stopwatch(); // Create the stopwatch instances
            swSupport = new Stopwatch();
            ProfileMutex = new System.Threading.Mutex(false, PROFILE_MUTEX_NAME);

            ProfileRegKey = null;
        }

        public RegistryAccess(string p_CallingComponent)
        {
            if (p_CallingComponent.ToUpperInvariant() == "UNINSTALLASCOM")
            {
                TL = new TraceLogger("", "ProfileMigration"); // Create a new trace logger
                TL.Enabled = GetBool(TRACE_PROFILE, TRACE_PROFILE_DEFAULT); // Get enabled / disabled state from the user registry

                RunningVersions(TL);

                sw = new Stopwatch(); // Create the stopwatch instances
                swSupport = new Stopwatch();
                ProfileMutex = new System.Threading.Mutex(false, PROFILE_MUTEX_NAME);

                ProfileRegKey = null;
            }
            else
                NewCode(false);// Normal behaviour so call common code respecting exceptions
        }

        public RegistryAccess(bool p_IgnoreChecks)
        {
            NewCode(p_IgnoreChecks); // Call the common code with the appropriate ignore flag
        }

        /// <summary>
        ///     ''' Common code for the new method
        ///     ''' </summary>
        ///     ''' <param name="p_IgnoreChecks">If true, suppresses the exception normally thrown if a valid profile is not present</param>
        ///     ''' <remarks></remarks>
        public void NewCode(bool p_IgnoreChecks)
        {
            string PlatformVersion;

            TL = new TraceLogger("", "RegistryAccess"); // Create a new trace logger
            TL.Enabled = GetBool(TRACE_XMLACCESS, TRACE_XMLACCESS_DEFAULT); // Get enabled / disabled state from the user registry

            RunningVersions(TL); // Include version information

            sw = new Stopwatch(); // Create the stopwatch instances
            swSupport = new Stopwatch();
            ProfileMutex = new System.Threading.Mutex(false, PROFILE_MUTEX_NAME);

            try
            {
                ProfileRegKey = OpenSubKey3264(Registry.LocalMachine, REGISTRY_ROOT_KEY_NAME, true, RegWow64Options.KEY_WOW64_32KEY);
                PlatformVersion = GetProfile(@"\", "PlatformVersion");
            }
            // OK, no exception so assume that we are initialised
            catch (ComponentModel.Win32Exception ex)
            {
                if (p_IgnoreChecks)
                    ProfileRegKey = null;
                else
                {
                    TL.LogMessageCrLf(@"RegistryAccess.New - Profile not found in registry at HKLM\" + REGISTRY_ROOT_KEY_NAME, ex.ToString());
                    throw new ProfilePersistenceException(@"RegistryAccess.New - Profile not found in registry at HKLM\" + REGISTRY_ROOT_KEY_NAME, ex);
                }
            }
            catch (Exception ex)
            {
                if (p_IgnoreChecks)
                    TL.LogMessageCrLf("RegistryAccess.New IgnoreCheks is true so ignoring exception:", ex.ToString());
                else
                {
                    TL.LogMessageCrLf("RegistryAccess.New Unexpected exception:", ex.ToString());
                    throw new ProfilePersistenceException("RegistryAccess.New - Unexpected exception", ex);
                }
            }
        }

        // IDisposable
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                if (!(TL == null))
                {
                    try
                    {
                        TL.LogMessage("Dispose", $"RegistryAccess Dispose has been called with disposing = {disposing}.");
                    }
                    catch
                    {
                    }
                }
                try
                {
                    sw.Stop();
                }
                catch
                {
                } // Clean up the stopwatches
                try
                {
                    sw = null;
                }
                catch
                {
                }
                try
                {
                    swSupport.Stop();
                }
                catch
                {
                }
                try
                {
                    swSupport = null;
                }
                catch
                {
                }
                try
                {
                    ProfileMutex.Close();
                }
                catch
                {
                }
                try
                {
                    ProfileMutex = null;
                }
                catch
                {
                }
                try
                {
                    ProfileRegKey.Close();
                }
                catch
                {
                }

                try
                {
                    ProfileRegKey.Close();
                }
                catch
                {
                }
                try
                {
                    ProfileRegKey = null;
                }
                catch
                {
                }

                // Release the registry key OS handles
                foreach (IntPtr ptr in HandleList)
                {
                    try
                    {
                        if (!(TL == null))
                        {
                            try
                            {
                                TL.LogMessage("Dispose", $"Closing handle {ptr.ToString("X8")}");
                            }
                            catch
                            {
                            }
                        }
                        CloseHandle(ptr);
                    }
                    catch (Exception ex)
                    {
                        if (!(TL == null))
                        {
                            try
                            {
                                TL.LogMessageCrLf("Dispose", ex.ToString());
                            }
                            catch
                            {
                            }
                        }
                    }
                }

                if (DisableTLOnExit)
                {
                    try
                    {
                        TL.LogMessage("Dispose", "Cleaning up logger.");
                    }
                    catch
                    {
                    } // Clean up the logger
                    try
                    {
                        TL.Enabled = false;
                    }
                    catch
                    {
                    } // Clean up the logger
                    try
                    {
                        TL.Dispose();
                    }
                    catch
                    {
                    }
                    try
                    {
                        TL = null;
                    }
                    catch
                    {
                    }
                }
            }
            this.disposedValue = true;
        }

        // This code added by Visual Basic to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code.  Put clean-up code in Dispose(ByVal disposing As Boolean) above.
            Dispose(true);
        }

        // Protected Overrides Sub Finalize()
        // ' Do not change this code.  Put clean-up code in Dispose(ByVal disposing As Boolean) above.
        // Dispose(False)
        // MyBase.Finalize()
        // End Sub



        internal void CreateKey(string p_SubKeyName)
        {
            // Create a key

            try
            {
                GetProfileMutex("CreateKey", p_SubKeyName);
                sw.Reset(); sw.Start(); // Start timing this call
                TL.LogMessage("CreateKey", "SubKey: \"" + p_SubKeyName + "\"");

                p_SubKeyName = Strings.Trim(p_SubKeyName); // Normalise the string:
                switch (p_SubKeyName)
                {
                    case ""  // Null path so do nothing
                   :
                        {
                            break;
                        }

                    default:
                        {
                            ProfileRegKey.CreateSubKey(CleanSubKey(p_SubKeyName));
                            ProfileRegKey.Flush();
                            break;
                        }
                }
                sw.Stop(); TL.LogMessage("  ElapsedTime", "  " + sw.ElapsedMilliseconds + " milliseconds");
            }
            finally
            {
                ProfileMutex.ReleaseMutex();
            }
        }

        internal void DeleteKey(string p_SubKeyName)
        {
            // Delete a key
            try
            {
                GetProfileMutex("DeleteKey", p_SubKeyName);
                sw.Reset(); sw.Start(); // Start timing this call
                TL.LogMessage("DeleteKey", "SubKey: \"" + p_SubKeyName + "\"");

                try
                {
                    ProfileRegKey.DeleteSubKeyTree(CleanSubKey(p_SubKeyName)); ProfileRegKey.Flush();
                }
                catch
                {
                } // Remove it if at all possible but don't throw any errors
                sw.Stop(); TL.LogMessage("  ElapsedTime", "  " + sw.ElapsedMilliseconds + " milliseconds");
            }
            finally
            {
                ProfileMutex.ReleaseMutex();
            }
        }

        internal void RenameKey(string OriginalSubKeyName, string NewSubKeyName)
        {
            RegistryKey SubKey;
            // Rename a key by creating a copy of the original key with the new name then deleting the original key
            // Throw New MethodNotImplementedException("RegistryAccess:RenameKey " & OriginalSubKeyName & " to " & NewSubKeyName)
            Generic.SortedList<string, string> Values;
            SubKey = ProfileRegKey.OpenSubKey(CleanSubKey(NewSubKeyName));
            if (SubKey == null)
            {
                CreateKey(NewSubKeyName);
                Values = EnumProfile(OriginalSubKeyName);
                foreach (Generic.KeyValuePair<string, string> Value in Values)
                    WriteProfile(NewSubKeyName, Value.Key, Value.Value);
                DeleteKey(OriginalSubKeyName);
            }
            else
            {
                SubKey.Close();
                throw new ProfilePersistenceException("Key " + NewSubKeyName + " already exists");
            }
        }

        internal void DeleteProfile(string p_SubKeyName, string p_ValueName)
        {
            // Delete a value from a key

            try
            {
                GetProfileMutex("DeleteProfile", p_SubKeyName + " " + p_ValueName);
                sw.Reset(); sw.Start(); // Start timing this call
                TL.LogMessage("DeleteProfile", "SubKey: \"" + p_SubKeyName + "\" Name: \"" + p_ValueName + "\"");

                try // Remove value if it exists
                {
                    ProfileRegKey.OpenSubKey(CleanSubKey(p_SubKeyName), true).DeleteValue(p_ValueName);
                    ProfileRegKey.Flush();
                }
                catch
                {
                    TL.LogMessage("DeleteProfile", "  Value did not exist");
                }
                sw.Stop(); TL.LogMessage("  ElapsedTime", "  " + sw.ElapsedMilliseconds + " milliseconds");
            }
            finally
            {
                ReleaseProfileMutex("DeleteProfile");
            }
        }

        internal System.Collections.Generic.SortedList<string, string> EnumKeys(string p_SubKeyName)
        {
            // Return a sorted list of sub-keys
            Generic.SortedList<string, string> RetValues = new Generic.SortedList<string, string>();
            string[] SubKeys;
            string Value;
            try
            {
                GetProfileMutex("EnumKeys", p_SubKeyName);
                sw.Reset(); sw.Start(); // Start timing this call
                TL.LogMessage("EnumKeys", "SubKey: \"" + p_SubKeyName + "\"");

                SubKeys = ProfileRegKey.OpenSubKey(CleanSubKey(p_SubKeyName)).GetSubKeyNames();

                foreach (string SubKey in SubKeys) // Process each key in turn
                {
                    try // If there is an error reading the data don't include in the returned list
                    {
                        // Create the new subkey and get a handle to it
                        switch (p_SubKeyName)
                        {
                            case "":
                            case @"\":
                                {
                                    Value = ProfileRegKey.OpenSubKey(CleanSubKey(SubKey)).GetValue("", "").ToString();
                                    break;
                                }

                            default:
                                {
                                    Value = ProfileRegKey.OpenSubKey(CleanSubKey(p_SubKeyName) + @"\" + SubKey).GetValue("", "").ToString();
                                    break;
                                }
                        }
                        RetValues.Add(SubKey, Value); // Add the Key name and default value to the hash table
                    }
                    catch (Exception ex)
                    {
                        TL.LogMessageCrLf("", "Read exception: " + ex.ToString());
                        throw new ProfilePersistenceException("RegistryAccess.EnumKeys exception", ex);
                    }
                }
                sw.Stop(); TL.LogMessage("  ElapsedTime", "  " + sw.ElapsedMilliseconds + " milliseconds");
            }
            finally
            {
                ReleaseProfileMutex("EnumKeys");
            }
            foreach (Generic.KeyValuePair<string, string> kvp in RetValues)
                TL.LogMessage("", "Found: " + kvp.Key + " " + kvp.Value);
            return RetValues;
        }

        internal Generic.SortedList<string, string> EnumProfile(string p_SubKeyName)
        {
            // Returns a sorted list of key values
            Generic.SortedList<string, string> RetValues = new Generic.SortedList<string, string>();
            string[] Values;

            try
            {
                GetProfileMutex("EnumProfile", p_SubKeyName);
                sw.Reset(); sw.Start(); // Start timing this call
                TL.LogMessage("EnumProfile", "SubKey: \"" + p_SubKeyName + "\"");

                Values = ProfileRegKey.OpenSubKey(CleanSubKey(p_SubKeyName)).GetValueNames();
                foreach (string Value in Values)
                    RetValues.Add(Value, ProfileRegKey.OpenSubKey(CleanSubKey(p_SubKeyName)).GetValue(Value).ToString());// Add the Key name and default value to the hash table

                sw.Stop(); TL.LogMessage("  ElapsedTime", "  " + sw.ElapsedMilliseconds + " milliseconds");
            }
            finally
            {
                ReleaseProfileMutex("EnumProfile");
            }
            return RetValues;
        }

        internal new string GetProfile(string p_SubKeyName, string p_ValueName, string p_DefaultValue)
        {
            // Read a single value from a key
            string RetVal;

            try
            {
                GetProfileMutex("GetProfile", p_SubKeyName + " " + p_ValueName + " " + p_DefaultValue);
                sw.Reset(); sw.Start(); // Start timing this call
                TL.LogMessage("GetProfile", "SubKey: \"" + p_SubKeyName + "\" Name: \"" + p_ValueName + "\"" + "\" DefaultValue: \"" + p_DefaultValue + "\"");
                TL.LogMessage("  DefaultValue", "is nothing... " + (p_DefaultValue == null).ToString());
                RetVal = ""; // Initialise return value to null string
                try
                {
                    RetVal = ProfileRegKey.OpenSubKey(CleanSubKey(p_SubKeyName)).GetValue(p_ValueName).ToString();
                    TL.LogMessage("  Value", "\"" + RetVal + "\"");
                }
                catch (NullReferenceException ex)
                {
                    if (!(p_DefaultValue == null))
                    {
                        WriteProfile(p_SubKeyName, p_ValueName, p_DefaultValue);
                        RetVal = p_DefaultValue;
                        TL.LogMessage("  Value", "Value not yet set, returning supplied default value: " + p_DefaultValue);
                    }
                    else
                        TL.LogMessage("  Value", "Value not yet set and no default value supplied, returning null string");
                }
                catch (Exception ex)
                {
                    if (!(p_DefaultValue == null))
                    {
                        WriteProfile(CleanSubKey(p_SubKeyName), CleanSubKey(p_ValueName), p_DefaultValue);
                        RetVal = p_DefaultValue;
                        TL.LogMessage("  Value", "Key not yet set, returning supplied default value: " + p_DefaultValue);
                    }
                    else
                    {
                        TL.LogMessageCrLf("  Value", "Key not yet set and no default value supplied, throwing exception: " + ex.ToString());
                        throw new ProfilePersistenceException("GetProfile Exception", ex);
                    }
                }

                sw.Stop(); TL.LogMessage("  ElapsedTime", "  " + sw.ElapsedMilliseconds + " milliseconds");
            }
            finally
            {
                ReleaseProfileMutex("GetProfile");
            }

            return RetVal;
        }

        internal new string GetProfile(string p_SubKeyName, string p_ValueName)
        {
            return this.GetProfile(p_SubKeyName, p_ValueName, null);
        }

        internal void WriteProfile(string p_SubKeyName, string p_ValueName, string p_ValueData)
        {
            // Write a single value to a key

            try
            {
                GetProfileMutex("WriteProfile", p_SubKeyName + " " + p_ValueName + " " + p_ValueData);
                sw.Reset(); sw.Start(); // Start timing this call
                TL.LogMessage("WriteProfile", "SubKey: \"" + p_SubKeyName + "\" Name: \"" + p_ValueName + "\" Value: \"" + p_ValueData + "\"");

                if (p_SubKeyName == "")
                    ProfileRegKey.SetValue(p_ValueName, p_ValueData, RegistryValueKind.String);
                else
                    ProfileRegKey.CreateSubKey(CleanSubKey(p_SubKeyName)).SetValue(p_ValueName, p_ValueData, RegistryValueKind.String);
                ProfileRegKey.Flush();

                sw.Stop(); TL.LogMessage("  ElapsedTime", "  " + sw.ElapsedMilliseconds + " milliseconds");
            }
            catch (Exception ex)
            {
                TL.LogMessageCrLf("WriteProfile", "Exception: " + ex.ToString());
                throw new ProfilePersistenceException("RegistryAccess.WriteProfile exception", ex);
            }
            finally
            {
                ReleaseProfileMutex("WriteProfile");
            }
        }

        internal void BackupProfile(string CurrentPlatformVersion)
        {
            try
            {
                GetProfileMutex("BackupProfile", "");
                sw.Reset(); sw.Start(); // Start timing this call

                LogMessage("BackupProfile", "From platform: " + CurrentPlatformVersion + ", OS: " + Enum.GetName(typeof(Bitness), OSBits()));

                switch (CurrentPlatformVersion)
                {
                    case "" // New installation
                   :
                        {
                            LogMessage("BackupProfile", "New installation so nothing to migrate");
                            break;
                        }

                    case "4":
                    case "5" // Currently on Platform 4 or 5 so Profile is in 32bit registry
             :
                        {
                            // Profile just needs to be backed up 
                            LogMessage("BackupProfile", "Backing up Platform 5 Profile" + CurrentPlatformVersion);
                            Backup50(); // Take a backup copy to restore later
                            break;
                        }

                    case "5.5" // Currently on Platform 5.5 so Profile is in file system and there is some profile in the registry too
             :
                        {
                            // Backup old 5.0 Profile and Copy 5.5 Profile to registry
                            Backup50();
                            Backup55();
                            break;
                        }

                    default:
                        {
                            // Do nothing as Profile is already in the Registry
                            LogMessage("BackupProfile", "Profile reports previous Platform as " + CurrentPlatformVersion + " - no migration required");
                            break;
                        }
                }

                sw.Stop(); LogMessage("  ElapsedTime", "  " + sw.ElapsedMilliseconds + " milliseconds");
            }
            catch (Exception ex)
            {
                LogError("BackupProfile", "Exception: " + ex.ToString());
                throw new ProfilePersistenceException("RegistryAccess.BackupProfile exception", ex);
            }
            finally
            {
                ReleaseProfileMutex("BackupProfile");
            }
        }

        internal void RestoreProfile(string CurrentPlatformVersion)
        {
            try
            {
                GetProfileMutex("RestoreProfile", "");
                sw.Reset(); sw.Start(); // Start timing this call

                LogMessage("RestoreProfile", "From platform: " + CurrentPlatformVersion + ", OS: " + Enum.GetName(typeof(Bitness), OSBits()));

                switch (CurrentPlatformVersion)
                {
                    case "" // New installation
                   :
                        {
                            LogMessage("RestoreProfile", "New installation so nothing to migrate");
                            break;
                        }

                    case "4":
                    case "5" // Currently on Platform 4 or 5 so Profile is in 32bit registry
             :
                        {
                            // Profile just needs to be backed up 
                            LogMessage("RestoreProfile", "Restoring Platform 5 Profile" + CurrentPlatformVersion);
                            Restore50(); // Take a backup copy to restore later
                            break;
                        }

                    case "5.5" // Currently on Platform 5.5 so Profile is in file system and there is some profile in the registry too
             :
                        {
                            // Backup old 5.0 Profile and Copy 5.5 Profile to registry
                            Restore55();
                            break;
                        }

                    default:
                        {
                            // Do nothing as Profile is already in the Registry
                            LogMessage("RestoreProfile", "Profile reports previous Platform as " + CurrentPlatformVersion + " - no migration required");
                            break;
                        }
                }

                // Make sure we have a valid key now that we have migrated the profile to the registry
                ProfileRegKey = OpenSubKey3264(Registry.LocalMachine, REGISTRY_ROOT_KEY_NAME, true, RegWow64Options.KEY_WOW64_32KEY);

                sw.Stop(); LogMessage("  ElapsedTime", "  " + sw.ElapsedMilliseconds + " milliseconds");
            }
            catch (Exception ex)
            {
                LogError("RestoreProfile", "Exception: " + ex.ToString());
                throw new ProfilePersistenceException("RegistryAccess.BackupProfile exception", ex);
            }
            finally
            {
                ReleaseProfileMutex("RestoreProfile");
            }
        }

        internal new ASCOMProfile GetProfile(string p_SubKeyName)
        {
            ASCOMProfile ProfileContents = new ASCOMProfile();

            try
            {
                GetProfileMutex("GetProfile", p_SubKeyName);
                sw.Reset(); sw.Start(); // Start timing this call
                TL.LogMessage("GetProfile", "SubKey: \"" + p_SubKeyName + "\"");

                GetSubKey(p_SubKeyName, "", ref ProfileContents); // Read the requested profile into a ProfileKey object
                sw.Stop(); TL.LogMessage("  ElapsedTime", "  " + sw.ElapsedMilliseconds + " milliseconds ");
            }
            finally
            {
                ReleaseProfileMutex("GetProfile");
            }

            return ProfileContents;
        }

        internal void SetProfile(string p_SubKeyName, ASCOMProfile p_ProfileKey)
        {
            RegistryKey SKey;
            try
            {
                GetProfileMutex("SetProfile", p_SubKeyName);
                sw.Reset(); sw.Start(); // Start timing this call
                TL.LogMessage("SetProfile", "SubKey: \"" + p_SubKeyName + "\"");

                foreach (string ProfileSubkey in p_ProfileKey.ProfileValues.Keys)
                {
                    TL.LogMessage("SetProfile", "Received SubKey: " + ProfileSubkey);

                    foreach (string value in p_ProfileKey.ProfileValues.Item(ProfileSubkey).Keys)
                        TL.LogMessage("SetProfile", "  Received value: " + value + " = " + p_ProfileKey.ProfileValues.Item(ProfileSubkey).Item(value));
                }

                foreach (string SubKey in p_ProfileKey.ProfileValues.Keys)
                {
                    if (p_SubKeyName == "")
                    {
                        if (SubKey == "")
                            SKey = ProfileRegKey;
                        else
                            SKey = ProfileRegKey.CreateSubKey(CleanSubKey(SubKey));
                    }
                    else if (SubKey == "")
                        SKey = ProfileRegKey.CreateSubKey(p_SubKeyName);
                    else
                        SKey = ProfileRegKey.CreateSubKey(p_SubKeyName + @"\" + CleanSubKey(SubKey));

                    foreach (string value in p_ProfileKey.ProfileValues.Item(SubKey).Keys)
                        SKey.SetValue(value, p_ProfileKey.ProfileValues.Item(SubKey).Item(value), RegistryValueKind.String);
                    SKey.Flush();
                }
                ProfileRegKey.Flush();

                sw.Stop(); TL.LogMessage("  ElapsedTime", "  " + sw.ElapsedMilliseconds + " milliseconds ");
            }
            finally
            {
                try
                {
                    SKey.Close();
                }
                catch
                {
                }
                ReleaseProfileMutex("SetProfile");
            }
        }


        // log messages and send to screen when appropriate
        private void LogMessage(string section, string logMessage)
        {
            TL.LogMessageCrLf(section, logMessage); // The CrLf version is used in order properly to format exception messages
            EventLogCode.LogEvent(section, logMessage, EventLogEntryType.Information, GlobalConstants.EventLogErrors.MigrateProfileRegistryKey, "");
        }

        // log error messages and send to screen when appropriate
        private void LogError(string section, string logMessage)
        {
            TL.LogMessageCrLf(section, logMessage); // The CrLf version is used in order properly to format exception messages
            EventLogCode.LogEvent(section, "Exception", EventLogEntryType.Error, GlobalConstants.EventLogErrors.MigrateProfileRegistryKey, logMessage);
        }

        private void GetSubKey(string BaseSubKey, string SubKeyOffset, ref ASCOMProfile ProfileContents)
        {
            ASCOMProfile RetVal = new ASCOMProfile();
            string[] ValueNames, SubKeyNames;
            string Value;
            RegistryKey SKey;

            BaseSubKey = CleanSubKey(BaseSubKey);
            SubKeyOffset = CleanSubKey(SubKeyOffset);

            if (BaseSubKey == "")
            {
                if (SubKeyOffset == "")
                    SKey = ProfileRegKey;
                else
                    SKey = ProfileRegKey.OpenSubKey(SubKeyOffset);
            }
            else if (SubKeyOffset == "")
                SKey = ProfileRegKey.OpenSubKey(BaseSubKey);
            else
                SKey = ProfileRegKey.OpenSubKey(BaseSubKey + @"\" + SubKeyOffset);

            ValueNames = SKey.GetValueNames();

            if (ValueNames.Count() == 0)
                ProfileContents.SetValue("", "", SubKeyOffset);
            else
                foreach (string ValueName in ValueNames)
                {
                    Value = SKey.GetValue(ValueName).ToString();
                    ProfileContents.SetValue(ValueName, Value, SubKeyOffset);
                }

            SubKeyNames = SKey.GetSubKeyNames();

            foreach (string SubKeyName in SubKeyNames)
                GetSubKey(BaseSubKey, SubKeyOffset + @"\" + CleanSubKey(SubKeyName), ref ProfileContents);

            try
            {
                SKey.Close();
            }
            catch
            {
            }
        }

        private string CleanSubKey(string SubKey)
        {
            // Remove leading "\" if it exists as this is not legal in a subkey name. "\" in the middle of a subkey name is legal however
            if (Strings.Left(SubKey, 1) == @"\")
                return Strings.Mid(SubKey, 2);
            return SubKey;
        }

        private void CopyRegistry(RegistryKey FromKey, RegistryKey ToKey)
        {
            string Value;
            // Subroutine used to recursively copy a registry Profile from one place to another
            string[] ValueNames, SubKeys;
            // Dim swLocal As Stopwatch
            RegistryKey NewFromKey, NewToKey;
            ;/* Cannot convert LocalDeclarationStatementSyntax, System.NotSupportedException: StaticKeyword not supported!
   at ICSharpCode.CodeConverter.CSharp.SyntaxKindExtensions.ConvertToken(SyntaxKind t, TokenContext context)
   at ICSharpCode.CodeConverter.CSharp.CommonConversions.ConvertModifier(SyntaxToken m, TokenContext context)
   at ICSharpCode.CodeConverter.CSharp.CommonConversions.<ConvertModifiersCore>d__15.MoveNext()
   at System.Linq.Enumerable.WhereEnumerableIterator`1.MoveNext()
   at Microsoft.CodeAnalysis.SyntaxTokenList.CreateNode(IEnumerable`1 tokens)
   at ICSharpCode.CodeConverter.CSharp.CommonConversions.ConvertModifiers(IEnumerable`1 modifiers, TokenContext context, Boolean isVariableOrConst)
   at ICSharpCode.CodeConverter.CSharp.VisualBasicConverter.MethodBodyVisitor.VisitLocalDeclarationStatement(LocalDeclarationStatementSyntax node)
   at Microsoft.CodeAnalysis.VisualBasic.Syntax.LocalDeclarationStatementSyntax.Accept[TResult](VisualBasicSyntaxVisitor`1 visitor)
   at Microsoft.CodeAnalysis.VisualBasic.VisualBasicSyntaxVisitor`1.Visit(SyntaxNode node)
   at ICSharpCode.CodeConverter.CSharp.CommentConvertingMethodBodyVisitor.ConvertWithTrivia(SyntaxNode node)
   at ICSharpCode.CodeConverter.CSharp.CommentConvertingMethodBodyVisitor.DefaultVisit(SyntaxNode node)

Input: 
        Static RecurseDepth As Integer

 */
            RecurseDepth += 1; // Increment the recursion depth indicator

            // swLocal = Stopwatch.StartNew
            LogMessage("CopyRegistry " + RecurseDepth.ToString(), "Copy from: " + FromKey.Name + " to: " + ToKey.Name + " Number of values: " + FromKey.ValueCount.ToString() + ", number of sub-keys: " + FromKey.SubKeyCount.ToString());

            // First copy values from the from key to the to key
            ValueNames = FromKey.GetValueNames();
            foreach (string ValueName in ValueNames)
            {
                Value = FromKey.GetValue(ValueName, "").ToString();
                ToKey.SetValue(ValueName, Value);
                LogMessage("CopyRegistry", ToKey.Name + " - \"" + ValueName + "\"  \"" + Value + "\"");
            }

            // Now process the keys
            SubKeys = FromKey.GetSubKeyNames();
            foreach (string SubKey in SubKeys)
            {
                Value = FromKey.OpenSubKey(SubKey).GetValue("", "").ToString();
                // LogMessage("  CopyRegistry", "  Processing subkey: " & SubKey & " " & Value)
                NewFromKey = FromKey.OpenSubKey(SubKey); // Create the new subkey and get a handle to it
                NewToKey = ToKey.CreateSubKey(SubKey); // Create the new subkey and get a handle to it
                if (!Value == "")
                    NewToKey.SetValue("", Value); // Set the default value if present
                CopyRegistry(NewFromKey, NewToKey); // Recursively process each key
            }
            // swLocal.Stop() : LogMessage("  CopyRegistry", "  Completed subkey: " & FromKey.Name & " " & RecurseDepth.ToString & ",  Elapsed time: " & swLocal.ElapsedMilliseconds & " milliseconds")
            RecurseDepth -= 1; // Decrement the recursion depth counter
                               // swLocal = Nothing
            try
            {
                NewFromKey.Close();
            }
            catch
            {
            }
            try
            {
                NewToKey.Close();
            }
            catch
            {
            }
        }

        private void Backup50()
        {
            RegistryKey FromKey, ToKey;
            Stopwatch swLocal;
            string PlatformVersion;

            swLocal = Stopwatch.StartNew();

            // FromKey = Registry.LocalMachine.OpenSubKey(REGISTRY_ROOT_KEY_NAME, True)
            FromKey = OpenSubKey3264(Registry.LocalMachine, REGISTRY_ROOT_KEY_NAME, false, RegWow64Options.KEY_WOW64_32KEY);
            ToKey = Registry.CurrentUser.CreateSubKey(REGISTRY_ROOT_KEY_NAME + @"\" + REGISTRY_5_BACKUP_SUBKEY);
            PlatformVersion = ToKey.GetValue("PlatformVersion", "").ToString(); // Test whether we have already backed up the profile
            if (string.IsNullOrEmpty(PlatformVersion))
            {
                LogMessage("Backup50", "Backup PlatformVersion not found, backing up Profile 5 to " + REGISTRY_5_BACKUP_SUBKEY);
                CopyRegistry(FromKey, ToKey);
            }
            else
                LogMessage("Backup50", @"Platform 5 backup found at HKCU\" + REGISTRY_ROOT_KEY_NAME + @"\" + REGISTRY_5_BACKUP_SUBKEY + ", no further action taken.");

            FromKey.Close(); // Close the key after migration
            ToKey.Close();

            swLocal.Stop(); LogMessage("Backup50", "ElapsedTime " + swLocal.ElapsedMilliseconds + " milliseconds");
            swLocal = null;
        }

        internal void ListRegistryACLs(RegistryKey Key, string Description)
        {
            RegistrySecurity KeySec;
            AuthorizationRuleCollection RuleCollection;

            LogMessage("ListRegistryACLs", Description + ", Key: " + Key.Name);
            KeySec = Key.GetAccessControl(); // Get existing ACL rules on the key 

            RuleCollection = KeySec.GetAccessRules(true, true, typeof(NTAccount)); // Get the access rules

            foreach (RegistryAccessRule RegRule in RuleCollection) // Iterate over the rule set and list them
                LogMessage("ListRegistryACLs", RegRule.AccessControlType.ToString() + " " + RegRule.IdentityReference.ToString() + " " + (AccessRights)RegRule.RegistryRights.ToString() + " " + Interaction.IIf(RegRule.IsInherited, "Inherited", "NotInherited").ToString() + " " + RegRule.InheritanceFlags.ToString() + " " + RegRule.PropagationFlags.ToString());
            TL.BlankLine();
        }

        internal void SetRegistryACL() // ByVal CurrentPlatformVersion As String)
        {
            // Subroutine to control the migration of a Platform 5.5 profile to Platform 6
            Generic.SortedList<string, string> Values = new Generic.SortedList<string, string>();
            Stopwatch swLocal;
            RegistryKey Key;
            RegistrySecurity KeySec;
            RegistryAccessRule RegAccessRule;
            SecurityIdentifier DomainSid, Ident;
            AuthorizationRuleCollection RuleCollection;

            swLocal = Stopwatch.StartNew();

            // Set a security ACL on the ASCOM Profile key giving the Users group Full Control of the key

            LogMessage("SetRegistryACL", "Creating security identifier");
            DomainSid = new SecurityIdentifier("S-1-0-0"); // Create a starting point domain SID
            Ident = new SecurityIdentifier(WellKnownSidType.BuiltinUsersSid, DomainSid); // Create a security Identifier for the BuiltinUsers Group to be passed to the new access rule

            LogMessage("SetRegistryACL", "Creating FullControl ACL rule");
            RegAccessRule = new RegistryAccessRule(Ident, RegistryRights.FullControl, InheritanceFlags.ContainerInherit, PropagationFlags.None, AccessControlType.Allow); // Create the new access permission rule

            // List the ACLs on the standard registry keys before we do anything
            LogMessage("SetRegistryACL", "Listing base key ACLs");

            if (ApplicationBits() == Bitness.Bits64)
            {
                LogMessage("SetRegistryACL", "Listing base key ACLs in 64bit mode");
                ListRegistryACLs(Registry.ClassesRoot, "HKEY_CLASSES_ROOT");
                ListRegistryACLs(Registry.LocalMachine.OpenSubKey("SOFTWARE"), @"HKEY_LOCAL_MACHINE\SOFTWARE");
                ListRegistryACLs(Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft"), @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft");
                ListRegistryACLs(OpenSubKey3264(Registry.LocalMachine, "SOFTWARE", true, RegWow64Options.KEY_WOW64_64KEY), @"HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node");
                ListRegistryACLs(OpenSubKey3264(Registry.LocalMachine, @"SOFTWARE\Microsoft", true, RegWow64Options.KEY_WOW64_32KEY), @"HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Microsoft");
            }
            else
            {
                LogMessage("SetRegistryACL", "Listing base key ACLS in 32bit mode");
                ListRegistryACLs(Registry.ClassesRoot, "HKEY_CLASSES_ROOT");
                ListRegistryACLs(Registry.LocalMachine.OpenSubKey("SOFTWARE"), @"HKEY_LOCAL_MACHINE\SOFTWARE");
                ListRegistryACLs(Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft"), @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft");
            }

            LogMessage("SetRegistryACL", @"Creating root ASCOM key ""\""");
            Key = OpenSubKey3264(Registry.LocalMachine, REGISTRY_ROOT_KEY_NAME, true, RegWow64Options.KEY_WOW64_32KEY); // Always create the key in the 32bit portion of the registry for backward compatibility

            LogMessage("SetRegistryACL", "Retrieving ASCOM key ACL rule");
            TL.BlankLine();
            KeySec = Key.GetAccessControl(); // Get existing ACL rules on the key 

            RuleCollection = KeySec.GetAccessRules(true, true, typeof(NTAccount)); // Get the access rules
            foreach (RegistryAccessRule RegRule in RuleCollection) // Iterate over the rule set and list them
            {
                try
                {
                    LogMessage("SetRegistryACL Before", RegRule.AccessControlType.ToString() + " " + RegRule.IdentityReference.ToString() + " " + (AccessRights)RegRule.RegistryRights.ToString() + " " + Interaction.IIf(RegRule.IsInherited, "Inherited", "NotInherited").ToString() + " " + RegRule.InheritanceFlags.ToString() + " " + RegRule.PropagationFlags.ToString());
                }
                catch (Exception ex)
                {
                    LogMessage("SetRegistryACL Before", ex.ToString());
                }
            }
            // Ensure that the ACLs are canonical before writing them back.
            // key.GetAccessControl sometimes provides ACLs in a non-canonical format, which causes failure when we write them back - UGH!

            TL.BlankLine();
            if (KeySec.AreAccessRulesCanonical)
                LogMessage("SetRegistryACL", "Current access rules on the ASCOM profile key are canonical, no fix-up action required");
            else
            {
                LogMessage("SetRegistryACL", "***** Current access rules on the ASCOM profile key are NOT canonical, fixing them");
                CanonicalizeDacl(KeySec); // Ensure that the ACLs are canonical
                LogMessage("SetRegistryACL", "Access Rules are Canonical after fix: " + KeySec.AreAccessRulesCanonical);

                // List the rules post canonicalisation
                RuleCollection = KeySec.GetAccessRules(true, true, typeof(NTAccount)); // Get the access rules
                foreach (RegistryAccessRule RegRule in RuleCollection) // Iterate over the rule set and list them
                {
                    try
                    {
                        LogMessage("SetRegistryACL Canon", RegRule.AccessControlType.ToString() + " " + RegRule.IdentityReference.ToString() + " " + (AccessRights)RegRule.RegistryRights.ToString() + " " + Interaction.IIf(RegRule.IsInherited, "Inherited", "NotInherited").ToString() + " " + RegRule.InheritanceFlags.ToString() + " " + RegRule.PropagationFlags.ToString());
                    }
                    catch (Exception ex)
                    {
                        LogMessage("SetRegistryACL Canon", ex.ToString());
                    }
                }
            }
            TL.BlankLine();

            LogMessage("SetRegistryACL", "Adding new ACL rule");
            KeySec.AddAccessRule(RegAccessRule); // Add the new rule to the existing rules
            LogMessage("SetRegistryACL", "Access Rules are Canonical after adding full access rule: " + KeySec.AreAccessRulesCanonical);
            TL.BlankLine();
            RuleCollection = KeySec.GetAccessRules(true, true, typeof(NTAccount)); // Get the access rules after adding the new one
            foreach (RegistryAccessRule RegRule in RuleCollection) // Iterate over the rule set and list them
            {
                try
                {
                    LogMessage("SetRegistryACL After", RegRule.AccessControlType.ToString() + " " + RegRule.IdentityReference.ToString() + " " + (AccessRights)RegRule.RegistryRights.ToString() + " " + Interaction.IIf(RegRule.IsInherited, "Inherited", "NotInherited").ToString() + " " + RegRule.InheritanceFlags.ToString() + " " + RegRule.PropagationFlags.ToString());
                }
                catch (Exception ex)
                {
                    LogMessage("SetRegistryACL After", ex.ToString());
                }
            }

            TL.BlankLine();
            LogMessage("SetRegistryACL", "Applying new ACL rule to the Profile key");
            Key.SetAccessControl(KeySec); // Apply the new rules to the Profile key

            LogMessage("SetRegistryACL", "Flushing key");
            Key.Flush(); // Flush the key to make sure the permission is committed
            LogMessage("SetRegistryACL", "Closing key");
            Key.Close(); // Close the key after migration

            swLocal.Stop(); LogMessage("SetRegistryACL", "ElapsedTime " + swLocal.ElapsedMilliseconds + " milliseconds");
            swLocal = null;
        }

        internal void CanonicalizeDacl(RegistrySecurity objectSecurity)
        {

            // A canonical ACL must have ACES sorted according to the following order:
            // 1. Access-denied on the object
            // 2. Access-denied on a child or property
            // 3. Access-allowed on the object
            // 4. Access-allowed on a child or property
            // 5. All inherited ACEs 
            RawSecurityDescriptor descriptor = new RawSecurityDescriptor(objectSecurity.GetSecurityDescriptorSddlForm(AccessControlSections.Access));

            List<CommonAce> implicitDenyDacl = new List<CommonAce>();
            List<CommonAce> implicitDenyObjectDacl = new List<CommonAce>();
            List<CommonAce> inheritedDacl = new List<CommonAce>();
            List<CommonAce> implicitAllowDacl = new List<CommonAce>();
            List<CommonAce> implicitAllowObjectDacl = new List<CommonAce>();
            int aceIndex = 0;
            RawAcl newDacl = new RawAcl(descriptor.DiscretionaryAcl.Revision, descriptor.DiscretionaryAcl.Count);
            int count = 0; // Counter for ACEs as they are processed

            try
            {
                if (objectSecurity == null)
                    throw new ArgumentNullException("objectSecurity");
                if (objectSecurity.AreAccessRulesCanonical)
                {
                    LogMessage("CanonicalizeDacl", "Rules are already canonical, no action taken");
                    return;
                }

                TL.BlankLine();
                LogMessage("CanonicalizeDacl", "***** Rules are not canonical, restructuring them *****");
                TL.BlankLine();

                foreach (CommonAce ace in descriptor.DiscretionaryAcl)
                {
                    count += 1;
                    LogMessage("CanonicalizeDacl", "Processing ACE " + count);

                    if ((ace.AceFlags & AceFlags.Inherited) == AceFlags.Inherited)
                    {
                        try
                        {
                            LogMessage("CanonicalizeDacl", "Found Inherited Ace");
                            LogMessage("CanonicalizeDacl", "Found Inherited Ace,                  " + Interaction.IIf(ace.AceType == AceType.AccessAllowed, "Allow", "Deny").ToString() + ": " + ace.SecurityIdentifier.Translate(Type.GetType("System.Security.Principal.NTAccount")).ToString() + " " + (AccessRights)ace.AccessMask.ToString().ToString() + " " + ace.AceFlags.ToString());
                            inheritedDacl.Add(ace);
                        }
                        catch (Exception ex1)
                        {
                            LogError("CanonicalizeDacl", "IdentityNotMappedException exception, ignoring this ACE because it is an orphan");
                            LogError("CanonicalizeDacl", ex1.ToString());
                        }
                    }
                    else
                        switch (ace.AceType)
                        {
                            case AceType.AccessAllowed:
                                {
                                    try
                                    {
                                        LogMessage("CanonicalizeDacl", "Found NotInherited Ace, Allow");
                                        LogMessage("CanonicalizeDacl", "Found NotInherited Ace,               Allow: " + ace.SecurityIdentifier.Translate(Type.GetType("System.Security.Principal.NTAccount")).ToString() + " " + (AccessRights)ace.AccessMask.ToString().ToString() + " " + ace.AceFlags.ToString());
                                        implicitAllowDacl.Add(ace);
                                    }
                                    catch (IdentityNotMappedException ex1)
                                    {
                                        LogError("CanonicalizeDacl", "IdentityNotMappedException exception, ignoring this ACE because it is an orphan");
                                        LogError("CanonicalizeDacl", ex1.ToString());
                                    }
                                    break;
                                    break;
                                }

                            case AceType.AccessDenied:
                                {
                                    try
                                    {
                                        LogMessage("CanonicalizeDacl", "Found NotInherited Ace Deny");
                                        LogMessage("CanonicalizeDacl", "Found NotInherited Ace,                Deny: " + ace.SecurityIdentifier.Translate(Type.GetType("System.Security.Principal.NTAccount")).ToString() + " " + (AccessRights)ace.AccessMask.ToString().ToString() + " " + ace.AceFlags.ToString());
                                        implicitDenyDacl.Add(ace);
                                    }
                                    catch (IdentityNotMappedException ex1)
                                    {
                                        LogError("CanonicalizeDacl", "IdentityNotMappedException exception, ignoring this ACE because it is an orphan");
                                        LogError("CanonicalizeDacl", ex1.ToString());
                                    }
                                    break;
                                    break;
                                }

                            case AceType.AccessAllowedObject:
                                {
                                    try
                                    {
                                        LogMessage("CanonicalizeDacl", "Found NotInherited Ace, Object Allow");
                                        LogMessage("CanonicalizeDacl", "Found NotInherited Ace, Object        Allow:" + ace.SecurityIdentifier.Translate(Type.GetType("System.Security.Principal.NTAccount")).ToString() + " " + (AccessRights)ace.AccessMask.ToString().ToString() + " " + ace.AceFlags.ToString());
                                        implicitAllowObjectDacl.Add(ace);
                                    }
                                    catch (IdentityNotMappedException ex1)
                                    {
                                        LogError("CanonicalizeDacl", "IdentityNotMappedException exception, ignoring this ACE because it is an orphan");
                                        LogError("CanonicalizeDacl", ex1.ToString());
                                    }
                                    break;
                                    break;
                                }

                            case AceType.AccessDeniedObject:
                                {
                                    try
                                    {
                                        LogMessage("CanonicalizeDacl", "Found NotInherited Ace, Object Deny");
                                        LogMessage("CanonicalizeDacl", "Found NotInherited Ace, Object         Deny: " + ace.SecurityIdentifier.Translate(Type.GetType("System.Security.Principal.NTAccount")).ToString() + " " + (AccessRights)ace.AccessMask.ToString().ToString() + " " + ace.AceFlags.ToString());
                                        implicitDenyObjectDacl.Add(ace);
                                        break;
                                    }
                                    catch (IdentityNotMappedException ex1)
                                    {
                                        LogError("CanonicalizeDacl", "IdentityNotMappedException exception, ignoring this ACE because it is an orphan");
                                        LogError("CanonicalizeDacl", ex1.ToString());
                                    }

                                    break;
                                }
                        }
                }

                TL.BlankLine();
                LogMessage("CanonicalizeDacl", "Rebuilding in correct order...");
                TL.BlankLine();

                // List the number of entries in each category
                LogMessage("CanonicalizeDacl", string.Format("There are {0} Implicit Deny ACLs", implicitDenyDacl.Count));
                LogMessage("CanonicalizeDacl", string.Format("There are {0} Implicit Deny Object ACLs", implicitDenyObjectDacl.Count));
                LogMessage("CanonicalizeDacl", string.Format("There are {0} Implicit Allow ACLs", implicitAllowDacl.Count));
                LogMessage("CanonicalizeDacl", string.Format("There are {0} Implicit Allow Object ACLs", implicitAllowObjectDacl.Count));
                LogMessage("CanonicalizeDacl", string.Format("There are {0} Inherited ACLs", inheritedDacl.Count));
                TL.BlankLine();

                // Rebuild the access list in the correct order
                foreach (CommonAce ace in implicitDenyDacl)
                {
                    try
                    {
                        LogMessage("CanonicalizeDacl", "Adding NonInherited Implicit Deny Ace");
                        newDacl.InsertAce(aceIndex, ace);
                        aceIndex += 1;
                        LogMessage("CanonicalizeDacl", "Added NonInherited Implicit Deny Ace,         " + Interaction.IIf(ace.AceType == AceType.AccessAllowed, "Allow", " Deny").ToString() + ": " + ace.SecurityIdentifier.Translate(Type.GetType("System.Security.Principal.NTAccount")).ToString() + " " + (AccessRights)ace.AccessMask.ToString().ToString() + " " + ace.AceFlags.ToString());
                    }
                    catch (IdentityNotMappedException ex1)
                    {
                        LogError("CanonicalizeDacl", "IdentityNotMappedException exception, ignoring this error because it says that this is an orphan ACE");
                        LogError("CanonicalizeDacl", ex1.ToString());
                    }
                }

                foreach (CommonAce ace in implicitDenyObjectDacl)
                {
                    try
                    {
                        LogMessage("CanonicalizeDacl", "Adding NonInherited Implicit Deny Object Ace");
                        newDacl.InsertAce(aceIndex, ace);
                        aceIndex += 1;
                        LogMessage("CanonicalizeDacl", "Added NonInherited Implicit Deny Object Ace,  " + Interaction.IIf(ace.AceType == AceType.AccessAllowed, "Allow", " Deny").ToString() + ": " + ace.SecurityIdentifier.Translate(Type.GetType("System.Security.Principal.NTAccount")).ToString() + " " + (AccessRights)ace.AccessMask.ToString().ToString() + " " + ace.AceFlags.ToString());
                    }
                    catch (IdentityNotMappedException ex1)
                    {
                        LogError("CanonicalizeDacl", "IdentityNotMappedException exception, ignoring this error because it says that this is an orphan ACE");
                        LogError("CanonicalizeDacl", ex1.ToString());
                    }
                }

                foreach (CommonAce ace in implicitAllowDacl)
                {
                    try
                    {
                        LogMessage("CanonicalizeDacl", "Adding NonInherited Implicit Allow Ace");
                        newDacl.InsertAce(aceIndex, ace);
                        aceIndex += 1;
                        LogMessage("CanonicalizeDacl", "Added NonInherited Implicit Allow Ace,        " + Interaction.IIf(ace.AceType == AceType.AccessAllowed, "Allow", " Deny").ToString() + ": " + ace.SecurityIdentifier.Translate(Type.GetType("System.Security.Principal.NTAccount")).ToString() + " " + (AccessRights)ace.AccessMask.ToString().ToString() + " " + ace.AceFlags.ToString());
                    }
                    catch (IdentityNotMappedException ex1)
                    {
                        LogError("CanonicalizeDacl", "IdentityNotMappedException exception, ignoring this error because it says that this is an orphan ACE");
                        LogError("CanonicalizeDacl", ex1.ToString());
                    }
                }

                foreach (CommonAce ace in implicitAllowObjectDacl)
                {
                    try
                    {
                        LogMessage("CanonicalizeDacl", "Adding NonInherited Implicit Allow Object Ace");
                        newDacl.InsertAce(aceIndex, ace);
                        aceIndex += 1;
                        LogMessage("CanonicalizeDacl", "Added NonInherited Implicit Allow Object Ace, " + Interaction.IIf(ace.AceType == AceType.AccessAllowed, "Allow", " Deny").ToString() + ": " + ace.SecurityIdentifier.Translate(Type.GetType("System.Security.Principal.NTAccount")).ToString() + " " + (AccessRights)ace.AccessMask.ToString().ToString() + " " + ace.AceFlags.ToString());
                    }
                    catch (IdentityNotMappedException ex1)
                    {
                        LogError("CanonicalizeDacl", "IdentityNotMappedException exception, ignoring this error because it says that this is an orphan ACE");
                        LogError("CanonicalizeDacl", ex1.ToString());
                    }
                }

                foreach (CommonAce ace in inheritedDacl)
                {
                    try
                    {
                        LogMessage("CanonicalizeDacl", "Adding Inherited Ace");
                        newDacl.InsertAce(aceIndex, ace);
                        aceIndex += 1;
                        LogMessage("CanonicalizeDacl", "Added Inherited Ace,                 " + Interaction.IIf(ace.AceType == AceType.AccessAllowed, "Allow", " Deny").ToString() + ": " + ace.SecurityIdentifier.Translate(Type.GetType("System.Security.Principal.NTAccount")).ToString() + " " + (AccessRights)ace.AccessMask.ToString().ToString() + " " + ace.AceFlags.ToString());
                    }
                    catch (IdentityNotMappedException ex1)
                    {
                        LogError("CanonicalizeDacl", "IdentityNotMappedException exception, ignoring this error because it says that this is an orphan ACE");
                        LogError("CanonicalizeDacl", ex1.ToString());
                    }
                }

                // If aceIndex <> descriptor.DiscretionaryAcl.Count Then
                // System.Diagnostics.Debug.Fail("The DACL cannot be canonicalized since it would potentially result in a loss of information")
                // Return
                // End If

                descriptor.DiscretionaryAcl = newDacl;
                objectSecurity.SetSecurityDescriptorSddlForm(descriptor.GetSddlForm(AccessControlSections.Access), AccessControlSections.Access);
                LogMessage("CanonicalizeDacl", "New ACL successfully applied to registry key.");
            }
            catch (Exception ex2)
            {
                LogError("CanonicalizeDacl", "Unexpected exception");
                LogError("CanonicalizeDacl", ex2.ToString());
                throw;
            }
        }

        private void Backup55()
        {
            XMLAccess Prof55;
            RegistryKey Key;

            LogMessage("Backup55", "Creating Profile 5.5 XMLAccess object");
            Prof55 = new XMLAccess();

            LogMessage("Backup55", "Opening " + REGISTRY_ROOT_KEY_NAME + @"\" + REGISTRY_55_BACKUP_SUBKEY + " Registry Key");
            Key = Registry.CurrentUser.CreateSubKey(REGISTRY_ROOT_KEY_NAME + @"\" + REGISTRY_55_BACKUP_SUBKEY);

            LogMessage("Backup55", "Backing up Platform 5.5 Profile");
            Copy55("", Prof55, Key);

            // Clean up objects
            Key.Flush();
            Key.Close();
            Key = null;
            Prof55.Dispose();
            Prof55 = null/* TODO Change to default(_) if this is not a reference type */;

            LogMessage("Backup55", "Completed copying the Profile");
        }

        private void Copy55(string CurrentSubKey, XMLAccess Prof55, RegistryKey RegistryTarget)
        {
            // Subroutine used to recursively copy the 5.5 XML profile to new 64bit registry profile
            Generic.SortedList<string, string> Values, SubKeys;
            Stopwatch swLocal;
            ;/* Cannot convert LocalDeclarationStatementSyntax, System.NotSupportedException: StaticKeyword not supported!
   at ICSharpCode.CodeConverter.CSharp.SyntaxKindExtensions.ConvertToken(SyntaxKind t, TokenContext context)
   at ICSharpCode.CodeConverter.CSharp.CommonConversions.ConvertModifier(SyntaxToken m, TokenContext context)
   at ICSharpCode.CodeConverter.CSharp.CommonConversions.<ConvertModifiersCore>d__15.MoveNext()
   at System.Linq.Enumerable.WhereEnumerableIterator`1.MoveNext()
   at Microsoft.CodeAnalysis.SyntaxTokenList.CreateNode(IEnumerable`1 tokens)
   at ICSharpCode.CodeConverter.CSharp.CommonConversions.ConvertModifiers(IEnumerable`1 modifiers, TokenContext context, Boolean isVariableOrConst)
   at ICSharpCode.CodeConverter.CSharp.VisualBasicConverter.MethodBodyVisitor.VisitLocalDeclarationStatement(LocalDeclarationStatementSyntax node)
   at Microsoft.CodeAnalysis.VisualBasic.Syntax.LocalDeclarationStatementSyntax.Accept[TResult](VisualBasicSyntaxVisitor`1 visitor)
   at Microsoft.CodeAnalysis.VisualBasic.VisualBasicSyntaxVisitor`1.Visit(SyntaxNode node)
   at ICSharpCode.CodeConverter.CSharp.CommentConvertingMethodBodyVisitor.ConvertWithTrivia(SyntaxNode node)
   at ICSharpCode.CodeConverter.CSharp.CommentConvertingMethodBodyVisitor.DefaultVisit(SyntaxNode node)

Input: 
        Static RecurseDepth As Integer

 */
            RecurseDepth += 1; // Increment the recursion depth indicator

            swLocal = Stopwatch.StartNew();
            LogMessage("Copy55ToRegistry " + RecurseDepth.ToString(), "Starting key: " + CurrentSubKey);

            // First copy values from the from key to the to key
            Values = Prof55.EnumProfile(CurrentSubKey);
            foreach (Generic.KeyValuePair<string, string> Value in Values)
            {
                RegistryTarget.SetValue(Value.Key, Value.Value);
                LogMessage("  Copy55ToRegistry", "  Key: " + CurrentSubKey + " - \"" + Value.Key + "\"  \"" + Value.Value + "\"");
            }
            RegistryTarget.Flush();

            // Now process the keys
            SubKeys = Prof55.EnumKeys(CurrentSubKey);
            RegistryKey NewKey;
            foreach (Generic.KeyValuePair<string, string> SubKey in SubKeys)
            {
                LogMessage("  Copy55ToRegistry", "  Processing subkey: " + SubKey.Key + " " + SubKey.Value);
                NewKey = RegistryTarget.CreateSubKey(SubKey.Key); // Create the new subkey and get a handle to it
                if (!SubKey.Value == "")
                    NewKey.SetValue("", SubKey.Value); // Set the default value if present
                Copy55(CurrentSubKey + @"\" + SubKey.Key, Prof55, NewKey); // Recursively process each key
                NewKey.Flush();
                NewKey.Close();
                NewKey = null;
            }
            swLocal.Stop(); LogMessage("  Copy55ToRegistry", "  Completed subkey: " + CurrentSubKey + " " + RecurseDepth.ToString() + ",  Elapsed time: " + swLocal.ElapsedMilliseconds + " milliseconds");
            RecurseDepth -= 1; // Decrement the recursion depth counter
            swLocal = null;
        }

        private void Restore50()
        {
            RegistryKey FromKey, ToKey;
            Stopwatch swLocal;

            swLocal = Stopwatch.StartNew();

            FromKey = Registry.CurrentUser.CreateSubKey(REGISTRY_ROOT_KEY_NAME + @"\" + REGISTRY_5_BACKUP_SUBKEY);
            ToKey = OpenSubKey3264(Registry.LocalMachine, REGISTRY_ROOT_KEY_NAME, true, RegWow64Options.KEY_WOW64_32KEY);
            LogMessage("Restore50", "Restoring Profile 5 to " + ToKey.Name);
            CopyRegistry(FromKey, ToKey);
            FromKey.Close(); // Close the key after migration
            ToKey.Close();

            swLocal.Stop(); LogMessage("Restore50", "ElapsedTime " + swLocal.ElapsedMilliseconds + " milliseconds");
            swLocal = null;
        }

        private void Restore55()
        {
            RegistryKey FromKey, ToKey;
            Stopwatch swLocal;

            swLocal = Stopwatch.StartNew();

            FromKey = Registry.CurrentUser.OpenSubKey(REGISTRY_ROOT_KEY_NAME + @"\" + REGISTRY_55_BACKUP_SUBKEY);
            ToKey = OpenSubKey3264(Registry.LocalMachine, REGISTRY_ROOT_KEY_NAME, true, RegWow64Options.KEY_WOW64_32KEY);
            LogMessage("Restore55", "Restoring Profile 5.5 to " + ToKey.Name);
            CopyRegistry(FromKey, ToKey);
            FromKey.Close(); // Close the key after migration
            ToKey.Close();

            swLocal.Stop(); LogMessage("Restore55", "ElapsedTime " + swLocal.ElapsedMilliseconds + " milliseconds");
            swLocal = null;
        }

        private void GetProfileMutex(string Method, string Parameters)
        {
            // Get the profile mutex or log an error and throw an exception that will terminate this profile call and return to the calling application
            try
            {
                // Try to acquire the mutex
                GotMutex = ProfileMutex.WaitOne(PROFILE_MUTEX_TIMEOUT, false);
                TL.LogMessage("GetProfileMutex", "Got Profile Mutex for " + Method);
            }
            // Catch the AbandonedMutexException but not any others, these are passed to the calling routine
            catch (Threading.AbandonedMutexException ex)
            {
                // We've received this exception but it indicates an issue in a PREVIOUS thread not this one. Log it and we have also got the mutex; so continue!
                TL.LogMessage("GetProfileMutex", "***** WARNING ***** AbandonedMutexException in " + Method + ", parameters: " + Parameters);
                TL.LogMessageCrLf("AbandonedMutexException", ex.ToString());
                LogEvent("RegistryAccess", "AbandonedMutexException in " + Method + ", parameters: " + Parameters, EventLogEntryType.Error, EventLogErrors.RegistryProfileMutexAbandoned, ex.ToString());
                if (GetBool(ABANDONED_MUTEXT_TRACE, ABANDONED_MUTEX_TRACE_DEFAULT))
                {
                    TL.LogMessage("RegistryAccess", "Throwing exception to application");
                    LogEvent("RegistryAccess", "AbandonedMutexException in " + Method + ": Throwing exception to application", EventLogEntryType.Warning, EventLogErrors.RegistryProfileMutexAbandoned, null/* TODO Change to default(_) if this is not a reference type */);
                    throw; // Throw the exception in order to report it
                }
                else
                {
                    TL.LogMessage("RegistryAccess", "Absorbing exception, continuing normal execution");
                    LogEvent("RegistryAccess", "AbandonedMutexException in " + Method + ": Absorbing exception, continuing normal execution", EventLogEntryType.Warning, EventLogErrors.RegistryProfileMutexAbandoned, null/* TODO Change to default(_) if this is not a reference type */);
                    GotMutex = true; // Flag that we have got the mutex.
                }
            }

            // Check whether we have the mutex, throw an error if not
            if (!GotMutex)
            {
                TL.LogMessage("GetProfileMutex", "***** WARNING ***** Timed out waiting for Profile mutex in " + Method + ", parameters: " + Parameters);
                LogEvent(Method, "Timed out waiting for Profile mutex in " + Method + ", parameters: " + Parameters, EventLogEntryType.Error, EventLogErrors.RegistryProfileMutexTimeout, null/* TODO Change to default(_) if this is not a reference type */);
                throw new ProfilePersistenceException("Timed out waiting for Profile mutex in " + Method + ", parameters: " + Parameters);
            }
        }

        public void ReleaseProfileMutex(string Method)
        {
            try
            {
                ProfileMutex.ReleaseMutex();
                TL.LogMessage("ReleaseProfileMutex", "Released Profile Mutex for " + Method);
            }
            catch (Exception ex)
            {
                TL.LogMessage("ReleaseProfileMutex", "Exception: " + ex.ToString());
                if (GetBool(ABANDONED_MUTEXT_TRACE, ABANDONED_MUTEX_TRACE_DEFAULT))
                {
                    TL.LogMessage("ReleaseProfileMutex", "Release Mutex Exception in " + Method + ": Throwing exception to application");
                    LogEvent("RegistryAccess", "Release Mutex Exception in " + Method + ": Throwing exception to application", EventLogEntryType.Error, EventLogErrors.RegistryProfileMutexAbandoned, ex.ToString());
                    throw; // Throw the exception in order to report it
                }
                else
                {
                    TL.LogMessage("ReleaseProfileMutex", "Release Mutex Exception in " + Method + ": Absorbing exception, continuing normal execution");
                    LogEvent("RegistryAccess", "Release Mutex Exception in " + Method + ": Absorbing exception, continuing normal execution", EventLogEntryType.Error, EventLogErrors.RegistryProfileMutexAbandoned, ex.ToString());
                }
            }
        }

        // There is a better way to achieve a 64bit registry view in framework 4. 
        // Only OpenSubKey is used in the code, the rest of these routines are support for OpenSubKey.
        // OpenSubKey should be replaced with Microsoft.Win32.RegistryKey.OpenBaseKey method

        // <Obsolete("Replace with Microsoft.Win32.RegistryKey.OpenBaseKey method in Framework 4", False)> _
        internal RegistryKey OpenSubKey3264(RegistryKey ParentKey, string SubKeyName, bool Writeable, RegWow64Options Options)
        {
            int SubKeyHandle;
            int Result;

            if (ParentKey == null || GetRegistryKeyHandle(ParentKey).Equals(IntPtr.Zero))
                throw new ProfilePersistenceException("OpenSubKey: Parent key is not open");

            int Rights = RegistryRights.ReadKey; // Or RegistryRights.EnumerateSubKeys Or RegistryRights.QueryValues Or RegistryRights.Notify
            if (Writeable)
            {
                Rights = RegistryRights.WriteKey;
                // hKey                             SubKey      Res lpClass     dwOpts samDesired     SecAttr      Handle        Disp
                Result = RegCreateKeyEx(GetRegistryKeyHandle(ParentKey), SubKeyName, 0, IntPtr.Zero, 0, Rights | Options, IntPtr.Zero.ToInt32(), ref SubKeyHandle, IntPtr.Zero.ToInt32());
                HandleList.Add((IntPtr)SubKeyHandle);
            }
            else
            {
                Result = RegOpenKeyEx(GetRegistryKeyHandle(ParentKey), SubKeyName, 0, Rights | Options, ref SubKeyHandle);
                HandleList.Add((IntPtr)SubKeyHandle);
            }

            switch (Result)
            {
                case 0 // All OK so return result
               :
                    {
                        return PointerToRegistryKey((IntPtr)SubKeyHandle, Writeable, false, Options); // Now pass the options as well for Framework 4 compatibility
                    }

                case 2 // Key not found so return nothing
               :
                    {
                        throw new ProfilePersistenceException("Cannot open key " + SubKeyName + " as it does not exist - Result: 0x" + Conversion.Hex(Result));
                        break;
                    }

                default:
                    {
                        throw new ProfilePersistenceException("OpenSubKey: Exception encountered opening key - Result: 0x" + Conversion.Hex(Result));
                        break;
                    }
            }
        }

        private Microsoft.Win32.RegistryKey PointerToRegistryKey(System.IntPtr hKey, bool writable, bool ownsHandle, RegWow64Options options)
        {
            // Create a SafeHandles.SafeRegistryHandle from this pointer - this is a private class
            System.Reflection.BindingFlags privateConstructors, publicConstructors;
            System.Type safeRegistryHandleType;
            System.Type[] safeRegistryHandleConstructorTypes = new[] { typeof(System.IntPtr), typeof(System.Boolean) };
            System.Reflection.ConstructorInfo safeRegistryHandleConstructor;
            System.Object safeHandle;
            System.Type registryKeyType;
            System.Reflection.ConstructorInfo registryKeyConstructor;
            Microsoft.Win32.RegistryKey result;

            publicConstructors = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public;
            privateConstructors = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
            safeRegistryHandleType = typeof(Microsoft.Win32.SafeHandles.SafeHandleZeroOrMinusOneIsInvalid).Assembly.GetType("Microsoft.Win32.SafeHandles.SafeRegistryHandle");
            safeRegistryHandleConstructor = safeRegistryHandleType.GetConstructor(publicConstructors, null, safeRegistryHandleConstructorTypes, null);
            safeHandle = safeRegistryHandleConstructor.Invoke(new System.Object[] { hKey, ownsHandle });

            // Create a new Registry key using the private constructor using the safeHandle - this should then behave like a .NET natively opened handle and disposed of correctly
            if (System.Environment.Version.Major >= 4)
            {
                System.Type RegistryViewType = typeof(Microsoft.Win32.SafeHandles.SafeHandleZeroOrMinusOneIsInvalid).Assembly.GetType("Microsoft.Win32.RegistryView"); // This is the new parameter type
                System.Type[] registryKeyConstructorTypes = new[] { safeRegistryHandleType, typeof(System.Boolean), RegistryViewType }; // Add the extra parameter to the list of parameter types
                registryKeyType = typeof(Microsoft.Win32.RegistryKey);
                registryKeyConstructor = registryKeyType.GetConstructor(privateConstructors, null, registryKeyConstructorTypes, null);
                result = (Microsoft.Win32.RegistryKey)registryKeyConstructor.Invoke(new object[] { safeHandle, writable, (int)options }); // Version 4 and later
            }
            else
            {
                System.Type[] registryKeyConstructorTypes = new[] { safeRegistryHandleType, typeof(System.Boolean) };
                registryKeyType = typeof(Microsoft.Win32.RegistryKey);
                registryKeyConstructor = registryKeyType.GetConstructor(privateConstructors, null, registryKeyConstructorTypes, null);
                result = (Microsoft.Win32.RegistryKey)registryKeyConstructor.Invoke(new object[] { safeHandle, writable }); // Version 3.5 and earlier
            }
            return result;
        }

        private IntPtr GetRegistryKeyHandle(RegistryKey RegisteryKey)
        {
            // Basis from http://blogs.msdn.com/cumgranosalis/archive/2005/12/09/Win64RegistryPart1.aspx
            Type Type = Type.GetType("Microsoft.Win32.RegistryKey");
            System.Reflection.FieldInfo Info = Type.GetField("hkey", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            System.Runtime.InteropServices.SafeHandle Handle = (System.Runtime.InteropServices.SafeHandle)Info.GetValue(RegisteryKey);

            return Handle.DangerousGetHandle();
        }

        internal enum RegWow64Options : int
        {
            // Basis from: http://www.pinvoke.net/default.aspx/advapi32/RegOpenKeyEx.html
            None = 0,
            KEY_WOW64_64KEY = 0x100,
            KEY_WOW64_32KEY = 0x200
        }

        [System.Runtime.InteropServices.DllImport("advapi32.dll")]
        private static extern int RegOpenKeyEx(IntPtr hKey, string lpSubKey, int ulOptions, int samDesired, ref int phkResult);


        [System.Runtime.InteropServices.DllImport("advapi32.dll")]
        private static extern int RegCreateKeyEx(IntPtr hKey, string lpSubKey, int Reserved, IntPtr lpClass, int dwOptions, int samDesired, int lpSecurityAttributes, ref int phkResult, int lpdwDisposition);

        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        private static extern int CloseHandle(IntPtr hObject);
    }
}