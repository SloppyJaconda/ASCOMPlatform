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
using System.Xml;
using Microsoft.Win32;
using ASCOM.Utilities.CS.Interfaces;
using ASCOM.Utilities.CS.Exceptions;

namespace ASCOM.Utilities.CS
{
    // Class to read and write profile values in an XML format



    internal class XMLAccess : IAccess, IDisposable
    {
        private const int RETRY_MAX = 1; // Number of persistence failure retrys
        private const int RETRY_INTERVAL = 200; // Length between persistence failure retrys in milliseconds

        private IFileStoreProvider FileStore; // File store containing the new ASCOM XML profile 
        private bool disposedValue = false;        // To detect redundant calls to IDisposable

        private System.Threading.Mutex ProfileMutex;
        private bool GotMutex;

        private TraceLogger TL;

        private Stopwatch sw, swSupport;

        public XMLAccess() : this(false) // Create but respect any exceptions thrown
        {
        }

        public XMLAccess(string p_CallingComponent) : this(false)
        {
        }

        public XMLAccess(bool p_IgnoreTest)
        {
            string PlatformVersion;

            TL = new TraceLogger("", "XMLAccess"); // Create a new trace logger
            TL.Enabled = GetBool(TRACE_XMLACCESS, TRACE_XMLACCESS_DEFAULT); // Get enabled / disabled state from the user registry
            RunningVersions(TL); // Include version information

            sw = new Stopwatch(); // Create the stowatch instances
            swSupport = new Stopwatch();

            FileStore = new AllUsersFileSystemProvider();
            // FileStore = New IsolatedStorageFileStoreProvider

            ProfileMutex = new System.Threading.Mutex(false, PROFILE_MUTEX_NAME);

            // Bypass test for initial setup by MigrateProfile because the profile isn't yet set up
            if (!p_IgnoreTest)
            {
                try
                {
                    if (!FileStore.Exists(@"\" + VALUES_FILENAME))
                        throw new Exceptions.ProfileNotFoundException("Utilities Error Base key does not exist");
                    PlatformVersion = GetProfile(@"\", "PlatformVersion");
                }
                // OK, no exception so assume that we are initialised
                catch (Exception ex)
                {
                    TL.LogMessageCrLf("XMLAccess.New Unexpected exception:", ex.ToString());
                    throw;
                }
            }
        }

        // IDisposable
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                try
                {
                    FileStore = null; // Clean up the filestore and keycache
                    TL.Enabled = false; // Clean up the logger
                    TL.Dispose();
                    TL = null;
                    sw.Stop(); // Clean up the stopwatches
                    sw = null;
                    swSupport.Stop();
                    swSupport = null;
                    ProfileMutex.Close();
                    ProfileMutex = null;
                }
                catch (Exception ex)
                {
                    Interaction.MsgBox("XMLAccess:Dispose Exception - " + ex.ToString());
                }
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

        ~XMLAccess()
        {
            // Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
            Dispose(false);
            base.Finalize();
        }



        internal void CreateKey(string p_SubKeyName)
        {
            // Create a key
            Generic.SortedList<string, string> InitalValues = new Generic.SortedList<string, string>();
            string[] SubKeys;
            string SubKey;
            int i, j;

            try
            {
                GetProfileMutex("CreateKey", p_SubKeyName);
                sw.Reset(); sw.Start(); // Start timing this call
                TL.LogMessage("CreateKey", "SubKey: \"" + p_SubKeyName + "\"");

                p_SubKeyName = Strings.Trim(p_SubKeyName); // Normalise the string:
                SubKeys = Strings.Split(p_SubKeyName, @"\", Compare: Microsoft.VisualBasic.CompareMethod.Text); // Parse p_SubKeyName into its elements
                switch (p_SubKeyName)
                {
                    case ""  // Null path so do nothing
                   :
                        {
                            break;
                        }

                    case @"\" // Root node so just create this
           :
                        {
                            if (!FileStore.Exists(@"\" + VALUES_FILENAME))
                            {
                                TL.LogMessage("  CreateKey", @"  Creating root key ""\""");
                                InitalValues.Clear(); // Now add the file containing the contents of the key
                                InitalValues.Add(COLLECTION_DEFAULT_VALUE_NAME, COLLECTION_DEFAULT_UNSET_VALUE);
                                WriteValues(@"\", ref InitalValues, false); // Write the profile file, don't check if it already exists
                            }
                            else
                                TL.LogMessage("  CreateKey", "  Root key alread exists");
                            break;
                        }

                    default:
                        {
                            for (i = 0; i <= SubKeys.Length - 1; i++)
                            {
                                SubKey = "";
                                for (j = 0; j <= i; j++)
                                    SubKey = SubKey + @"\" + SubKeys[j];
                                if (!FileStore.Exists(SubKey + @"\" + VALUES_FILENAME))
                                {
                                    FileStore.CreateDirectory(SubKey, TL);  // It doesn't exist so create it
                                    InitalValues.Clear(); // Now add the file containing the contents of the key
                                    InitalValues.Add(COLLECTION_DEFAULT_VALUE_NAME, COLLECTION_DEFAULT_UNSET_VALUE);
                                    WriteValues(SubKey, ref InitalValues, false); // Write the profile file
                                }
                            }

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
                    FileStore.DeleteDirectory(p_SubKeyName);
                }
                catch (Exception ex)
                {
                } // Remove it if at all possible but don't throw any errors
                sw.Stop(); TL.LogMessage("  ElapsedTime", "  " + sw.ElapsedMilliseconds + " milliseconds");
            }
            finally
            {
                ProfileMutex.ReleaseMutex();
            }
        }

        internal void RenameKey(string CurrentSubKeyName, string NewSubKeyName)
        {
            try
            {
                GetProfileMutex("RenameKey", CurrentSubKeyName + " " + NewSubKeyName);
                sw.Reset(); sw.Start(); // Start timing this call
                TL.LogMessage("RenameKey", "Current SubKey: \"" + CurrentSubKeyName + "\"" + " New SubKey: \"" + NewSubKeyName + "\"");

                FileStore.RenameDirectory(CurrentSubKeyName, NewSubKeyName);
                sw.Stop(); TL.LogMessage("  ElapsedTime", "  " + sw.ElapsedMilliseconds + " milliseconds");
            }
            finally
            {
                ProfileMutex.ReleaseMutex();
            }
        }

        internal void DeleteProfile(string p_SubKeyName, string p_ValueName)
        {
            // Delete a value from a key
            Generic.SortedList<string, string> Values;

            try
            {
                GetProfileMutex("DeleteProfile", p_SubKeyName + " " + p_ValueName);
                sw.Reset(); sw.Start(); // Start timing this call
                TL.LogMessage("DeleteProfile", "SubKey: \"" + p_SubKeyName + "\" Name: \"" + p_ValueName + "\"");

                Values = ReadValues(p_SubKeyName); // Read current contents of key
                try // Remove value if it exists
                {
                    if (p_ValueName == "")
                    {
                        Values(COLLECTION_DEFAULT_VALUE_NAME) = COLLECTION_DEFAULT_UNSET_VALUE;
                        TL.LogMessage("DeleteProfile", "  Default name was changed to unset value");
                    }
                    else
                    {
                        Values.Remove(p_ValueName);
                        TL.LogMessage("DeleteProfile", "  Value was deleted");
                    }
                }
                catch
                {
                    TL.LogMessage("DeleteProfile", "  Value did not exist");
                }
                WriteValues(p_SubKeyName, ref Values); // Write the new list of values back to the key
                Values = null/* TODO Change to default(_) if this is not a reference type */;
                sw.Stop(); TL.LogMessage("  ElapsedTime", "  " + sw.ElapsedMilliseconds + " milliseconds");
            }
            finally
            {
                ProfileMutex.ReleaseMutex();
            }
        }

        internal System.Collections.Generic.SortedList<string, string> EnumKeys(string p_SubKeyName)
        {
            // Return a sorted list of subkeys
            Generic.SortedList<string, string> Values;
            Generic.SortedList<string, string> RetValues = new Generic.SortedList<string, string>();
            string[] Directories;
            string DefaultValue;

            try
            {
                GetProfileMutex("EnumKeys", p_SubKeyName);
                sw.Reset(); sw.Start(); // Start timing this call
                TL.LogMessage("EnumKeys", "SubKey: \"" + p_SubKeyName + "\"");

                Directories = FileStore.GetDirectoryNames(p_SubKeyName); // Get a list of the keys
                foreach (string Directory in Directories) // Process each key in trun
                {
                    try // If there is an error reading the data don't include in the returned list
                    {
                        Values = ReadValues(p_SubKeyName + @"\" + Directory); // Read the values of this key to find the default value
                        DefaultValue = Values.Item(COLLECTION_DEFAULT_VALUE_NAME); // Save the default value
                        if (DefaultValue == COLLECTION_DEFAULT_UNSET_VALUE)
                            DefaultValue = "";
                        RetValues.Add(Directory, DefaultValue); // Add the directory name and default value to the hashtable
                    }
                    catch
                    {
                    }
                    Values = null/* TODO Change to default(_) if this is not a reference type */;
                }
                sw.Stop(); TL.LogMessage("  ElapsedTime", "  " + sw.ElapsedMilliseconds + " milliseconds");
            }
            finally
            {
                ProfileMutex.ReleaseMutex();
            }
            return RetValues;
        }

        internal Generic.SortedList<string, string> EnumProfile(string p_SubKeyName)
        {
            // Returns a sorted list of key values
            Generic.SortedList<string, string> Values;
            Generic.SortedList<string, string> RetValues = new Generic.SortedList<string, string>();

            try
            {
                GetProfileMutex("EnumProfile", p_SubKeyName);
                sw.Reset(); sw.Start(); // Start timing this call
                TL.LogMessage("EnumProfile", "SubKey: \"" + p_SubKeyName + "\"");

                Values = ReadValues(p_SubKeyName); // Read values from profile XML file
                foreach (Generic.KeyValuePair<string, string> kvp in Values) // Retrieve each key/value  pair in turn
                {
                    if (kvp.Key == COLLECTION_DEFAULT_VALUE_NAME)
                    {
                        if (kvp.Value == COLLECTION_DEFAULT_UNSET_VALUE)
                        {
                        }
                        else
                            RetValues.Add("", kvp.Value);// Add any other value to the return value
                    }
                    else
                        RetValues.Add(kvp.Key, kvp.Value);
                }
                Values = null/* TODO Change to default(_) if this is not a reference type */;
                sw.Stop(); TL.LogMessage("  ElapsedTime", "  " + sw.ElapsedMilliseconds + " milliseconds");
            }
            finally
            {
                ProfileMutex.ReleaseMutex();
            }
            return RetValues;
        }

        internal new string GetProfile(string p_SubKeyName, string p_ValueName, string p_DefaultValue)
        {
            Generic.SortedList<string, string> Values;
            // Read a single value from a key
            string RetVal;

            try
            {
                GetProfileMutex("GetProfile", p_SubKeyName + " " + p_ValueName + " " + p_DefaultValue);
                sw.Reset(); sw.Start(); // Start timing this call
                TL.LogMessage("GetProfile", "SubKey: \"" + p_SubKeyName + "\" Name: \"" + p_ValueName + "\"" + "\" DefaultValue: \"" + p_DefaultValue + "\"");

                RetVal = ""; // Initialise return value to null string
                try
                {
                    Values = ReadValues(p_SubKeyName); // Read in the key values
                    if (p_ValueName == "")
                        RetVal = Values.Item(COLLECTION_DEFAULT_VALUE_NAME);
                    else
                        RetVal = Values.Item(p_ValueName);
                }
                catch (Generic.KeyNotFoundException ex)
                {
                    if (!(p_DefaultValue == null))
                    {
                        WriteProfile(p_SubKeyName, p_ValueName, p_DefaultValue);
                        RetVal = p_DefaultValue;
                        TL.LogMessage("GetProfile", "Value not yet set, returning supplied default value: " + p_DefaultValue);
                    }
                    else
                        TL.LogMessage("GetProfile", "Value not yet set and no default value supplied, returning null string");
                }
                catch (Exception ex)
                {
                    if (!(p_DefaultValue == null))
                    {
                        WriteProfile(p_SubKeyName, p_ValueName, p_DefaultValue);
                        RetVal = p_DefaultValue;
                        TL.LogMessage("GetProfile", "Key not yet set, returning supplied default value: " + p_DefaultValue);
                    }
                    else
                    {
                        TL.LogMessage("GetProfile", "Key not yet set and no default value supplied, throwing exception: " + ex.Message);
                        throw;
                    }
                }

                Values = null/* TODO Change to default(_) if this is not a reference type */;
                sw.Stop(); TL.LogMessage("  ElapsedTime", "  " + sw.ElapsedMilliseconds + " milliseconds");
            }
            finally
            {
                ProfileMutex.ReleaseMutex();
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
            Generic.SortedList<string, string> Values;

            try
            {
                GetProfileMutex("WriteProfile", p_SubKeyName + " " + p_ValueName + " " + p_ValueData);
                sw.Reset(); sw.Start(); // Start timing this call
                TL.LogMessage("WriteProfile", "SubKey: \"" + p_SubKeyName + "\" Name: \"" + p_ValueName + "\" Value: \"" + p_ValueData + "\"");

                // Check if the directory exists
                if (!FileStore.Exists(p_SubKeyName + @"\" + VALUES_FILENAME))
                    CreateKey(p_SubKeyName); // Create the subkey if it doesn't already exist
                Values = ReadValues(p_SubKeyName); // Read the key values

                if (p_ValueName == "")
                {
                    if (Values.ContainsKey(COLLECTION_DEFAULT_VALUE_NAME))
                        Values.Item(COLLECTION_DEFAULT_VALUE_NAME) = p_ValueData; // Update the existing value
                    else
                        Values.Add(COLLECTION_DEFAULT_VALUE_NAME, p_ValueData);// Add the new value
                    WriteValues(p_SubKeyName, ref Values); // Write the values back to the XML profile
                }
                else
                {
                    if (Values.ContainsKey(p_ValueName))
                        Values.Remove(p_ValueName); // Remove old value if it exists
                    Values.Add(p_ValueName, p_ValueData); // Add the new value
                    WriteValues(p_SubKeyName, ref Values); // Write the values back to the XML profile
                }
                Values = null/* TODO Change to default(_) if this is not a reference type */;
                sw.Stop(); TL.LogMessage("  ElapsedTime", "  " + sw.ElapsedMilliseconds + " milliseconds");
            }
            finally
            {
                ProfileMutex.ReleaseMutex();
            }
        }

        internal void SetSecurityACLs()
        {
            bool LogEnabled;
            try
            {
                GetProfileMutex("SetSecurityACLs", "");
                sw.Reset(); sw.Start(); // Start timing this call
                TL.LogMessage("SetSecurityACLs", "");

                // Force logging to be enabled for this...
                LogEnabled = TL.Enabled; // Save logging state
                TL.Enabled = true;
                RunningVersions(TL); // Capture date in case logging wasn't initially enabled

                // Set security ACLs on profile root directory
                TL.LogMessage("SetSecurityACLs", "Setting security ACLs on ASCOM root directory ");
                FileStore.SetSecurityACLs(TL);
                sw.Stop(); TL.LogMessage("  ElapsedTime", "  " + sw.ElapsedMilliseconds + " milliseconds");
                TL.Enabled = LogEnabled; // Restore logging state
            }
            catch (Exception ex)
            {
                TL.LogMessageCrLf("SetSecurityACLs", "Exception: " + ex.ToString());
                throw;
            }
        }

        internal void MigrateProfile(string CurrentPlatformVersion)
        {
            RegistryKey FromKey;
            bool LogEnabled;

            try
            {
                GetProfileMutex("MigrateProfile", "");
                sw.Reset(); sw.Start(); // Start timing this call
                TL.LogMessage("MigrateProfile", "");

                // Force logging to be enabled for this...
                LogEnabled = TL.Enabled; // Save logging state
                TL.Enabled = true;
                RunningVersions(TL); // Capture date in case logging wasn't initially enabled

                TL.LogMessage("MigrateProfile", "Migrating keys");
                // Create the root directory if it doesn't already exist
                if (!FileStore.Exists(@"\" + VALUES_FILENAME))
                {
                    FileStore.CreateDirectory(@"\", TL);
                    CreateKey(@"\"); // Create the root key
                    TL.LogMessage("MigrateProfile", "Successfully created root directory and root key");
                }
                else
                    TL.LogMessage("MigrateProfile", "Root directory already exists");

                // Set security ACLs on profile root directory
                TL.LogMessage("MigrateProfile", "Setting security ACLs on ASCOM root directory ");
                FileStore.SetSecurityACLs(TL);

                TL.LogMessage("MigrateProfile", "Copying Profile from Registry");
                // Get the registry root key depending. Success here depends on us running as 32bit as the Platform 5 registry 
                // is located under HKLM\Software\Wow6432Node!
                FromKey = Registry.LocalMachine.OpenSubKey(REGISTRY_ROOT_KEY_NAME); // Source to copy from 
                if (!FromKey == null)
                {
                    TL.LogMessage("MigrateProfile", "FromKey Opened OK: " + FromKey.Name + ", SubKeyCount: " + FromKey.SubKeyCount.ToString() + ", ValueCount: " + FromKey.ValueCount.ToString());
                    MigrateKey(FromKey, ""); // Use recursion to copy contents to new tree
                    TL.LogMessage("MigrateProfile", "Successfully migrated keys");
                    FromKey.Close();
                    // Restore original logging state
                    TL.Enabled = GetBool(TRACE_XMLACCESS, TRACE_XMLACCESS_DEFAULT); // Get enabled / disabled state from the user registry
                }
                else
                    throw new ProfileNotFoundException(@"Cannot find ASCOM Profile in HKLM\" + REGISTRY_ROOT_KEY_NAME + " Is Platform 5 installed?");
                sw.Stop(); TL.LogMessage("  ElapsedTime", "  " + sw.ElapsedMilliseconds + " milliseconds");
                TL.Enabled = LogEnabled; // Restore logging state
            }
            catch (Exception ex)
            {
                TL.LogMessageCrLf("MigrateProfile", "Exception: " + ex.ToString());
                throw;
            }
        }

        internal ASCOMProfile GetProfileXML(string DriverId)
        {
            throw new MethodNotImplementedException("XMLAccess:GetProfileXml");
        }
        internal void SetProfileXML(string DriverId, ASCOMProfile Profile)
        {
            throw new MethodNotImplementedException("XMLAccess:SetProfileXml");
        }



        private Generic.SortedList<string, string> ReadValues(string p_SubKeyName)
        {
            Generic.SortedList<string, string> Retval = new Generic.SortedList<string, string>();
            // Read all values in a key - SubKey has to be absolute from the profile store root
            XmlReaderSettings ReaderSettings;
            string LastElementName = "";
            string NextName = "";
            string ValueName = "";
            bool ReadOK = false;
            int RetryCount;
            bool ErrorOccurred = false;
            string ValuesFileName; // Name of the profile file from which to read
            bool ExistsValues, ExistsValuesOriginal, ExistsValuesNew;

            swSupport.Reset(); swSupport.Start(); // Start timing this call
            if (Strings.Left(p_SubKeyName, 1) != @"\")
                p_SubKeyName = @"\" + p_SubKeyName; // Condition to have leading \
            TL.LogMessage("  ReadValues", "  SubKeyName: " + p_SubKeyName);

            ValuesFileName = VALUES_FILENAME; // Initialise to the file holding current values
            RetryCount = -1; // Initialise to ensure we get RETRY_Max number of retrys

            // Determine what files exist and handle the case where this key has not yet been created
            ExistsValues = FileStore.Exists(p_SubKeyName + @"\" + VALUES_FILENAME);
            ExistsValuesOriginal = FileStore.Exists(p_SubKeyName + @"\" + VALUES_FILENAME_ORIGINAL);
            ExistsValuesNew = FileStore.Exists(p_SubKeyName + @"\" + VALUES_FILENAME_NEW);
            if (!ExistsValues & !ExistsValuesOriginal)
                throw new ProfileNotFoundException("No profile files exist for this key: " + p_SubKeyName);
            do
            {
                RetryCount += 1;
                try
                {
                    ReaderSettings = new XmlReaderSettings();
                    ReaderSettings.IgnoreWhitespace = true;
                    using (XmlReader Reader = XmlReader.Create(FileStore.FullPath(p_SubKeyName + @"\" + ValuesFileName), ReaderSettings))
                    {
                        Reader.Read();
                        Reader.Read();

                        // Start reading profile strings
                        while (Reader.Read())
                        {
                            switch (Reader.NodeType)
                            {
                                case XmlNodeType.Element:
                                    {
                                        switch (Reader.Name)
                                        {
                                            case object _ when DEFAULT_ELEMENT_NAME:
                                                {
                                                    Retval.Add(COLLECTION_DEFAULT_VALUE_NAME, Reader.GetAttribute(VALUE_ATTRIBUTE_NAME));
                                                    TL.LogMessage("    ReadValues", "    found " + COLLECTION_DEFAULT_VALUE_NAME + " = " + Retval.Item(COLLECTION_DEFAULT_VALUE_NAME));
                                                    break;
                                                }

                                            case object _ when VALUE_ELEMENT_NAME:
                                                {
                                                    ValueName = Reader.GetAttribute(NAME_ATTRIBUTE_NAME);
                                                    Retval.Add(ValueName, Reader.GetAttribute(VALUE_ATTRIBUTE_NAME));
                                                    TL.LogMessage("    ReadValues", "    found " + ValueName + " = " + Retval.Item(ValueName));
                                                    break;
                                                }

                                            default:
                                                {
                                                    TL.LogMessage("    ReadValues", "    ## Found unexpected Reader.Name: " + Reader.Name.ToString());
                                                    break;
                                                }
                                        }

                                        break;
                                    }

                                default:
                                    {
                                        break;
                                    }
                            }
                        }

                        Reader.Close();
                    }

                    swSupport.Stop();
                    TL.LogMessage("  ReadValues", "  added to cache - " + swSupport.ElapsedMilliseconds + " milliseconds");
                    ReadOK = true;
                }
                catch (Exception ex)
                {
                    ErrorOccurred = true;
                    if (RetryCount == RETRY_MAX)
                    {
                        if (ValuesFileName == VALUES_FILENAME)
                        {
                            ValuesFileName = VALUES_FILENAME_ORIGINAL;
                            RetryCount = -1;
                            LogEvent("XMLAccess:ReadValues", "Error reading profile on final retry - attempting recovery from previous version", EventLogEntryType.Warning, EventLogErrors.XMLAccessRecoveryPreviousVersion, ex.ToString());
                            TL.LogMessageCrLf("  ReadValues", "Final retry exception - attempting recovery from previous version: " + ex.ToString());
                        }
                        else
                        {
                            LogEvent("XMLAccess:ReadValues", "Error reading profile on final retry", EventLogEntryType.Error, EventLogErrors.XMLAccessReadError, ex.ToString());
                            TL.LogMessageCrLf("  ReadValues", "Final retry exception: " + ex.ToString());
                            throw new ProfilePersistenceException("XMLAccess Exception", ex);
                        }
                    }
                    else
                    {
                        LogEvent("XMLAccess:ReadValues", "Error reading profile - retry: " + RetryCount, EventLogEntryType.Warning, EventLogErrors.XMLAccessRecoveryPreviousVersion, ex.Message);
                        TL.LogMessageCrLf("  ReadValues", "Retry " + RetryCount + " exception: " + ex.ToString());
                    }
                }

                if (ErrorOccurred)
                    System.Threading.Thread.Sleep(RETRY_INTERVAL);
            }
            while (!ReadOK)// Get rid of the XML version string// Read in the Profile name tag// Found default value// Fount an element name// Close the IO readers// Set the exit flag here when a read has been successful // Wait if an error occurred
    ;
            if (ErrorOccurred)
            {
                LogEvent("XMLAccess:ReadValues", "Recovered from read error OK", EventLogEntryType.SuccessAudit, EventLogErrors.XMLAccessRecoveredOK, null/* TODO Change to default(_) if this is not a reference type */);
                TL.LogMessage("  ReadValues", "Recovered from read error OK");
            }

            return Retval;
        }

        private new void WriteValues(string p_SubKeyName, ref Generic.SortedList<string, string> p_KeyValuePairs)
        {
            // Make the general case check for existence of a current Profile.xml file. Most cases need this
            // The exception is the CreateKey where the Profile.xmldefinitlkey won't exist as we are about to create it for the first time
            WriteValues(p_SubKeyName, ref p_KeyValuePairs, true);
        }

        private new void WriteValues(string p_SubKeyName, ref Generic.SortedList<string, string> p_KeyValuePairs, bool p_CheckForCurrentProfileStore)
        {
            // Write  all key values to an XML file
            // SubKey has to be absolute from the profile store root
            XmlWriterSettings WriterSettings;
            string FName;
            int Ct;

            swSupport.Reset(); swSupport.Start(); // Start timing this call
            TL.LogMessage("  WriteValues", "  SubKeyName: " + p_SubKeyName);
            if (Strings.Left(p_SubKeyName, 1) != @"\")
                p_SubKeyName = @"\" + p_SubKeyName;

            try
            {
                Ct = 0;
                foreach (Generic.KeyValuePair<string, string> kvp in p_KeyValuePairs)
                {
                    Ct += 1;
                    TL.LogMessage("  WriteValues List", "  " + Ct.ToString() + " " + kvp.Key + " = " + kvp.Value);
                }

                WriterSettings = new XmlWriterSettings();
                WriterSettings.Indent = true;
                FName = FileStore.FullPath(p_SubKeyName + @"\" + VALUES_FILENAME_NEW);
                XmlWriter Writer;
                FileStream FStream;
                FStream = new FileStream(FName, FileMode.Create, FileAccess.Write, FileShare.None, 2048, FileOptions.WriteThrough);
                Writer = XmlWriter.Create(FStream, WriterSettings);
                // Writer = XmlWriter.Create(FName, WriterSettings)
                using (Writer)
                {
                    Writer.WriteStartDocument();
                    Writer.WriteStartElement(PROFILE_NAME); // Write the profile element
                    Writer.WriteStartElement(DEFAULT_ELEMENT_NAME); // Write the default element
                    Writer.WriteAttributeString(VALUE_ATTRIBUTE_NAME, p_KeyValuePairs.Item(COLLECTION_DEFAULT_VALUE_NAME)); // Write the default value
                    Writer.WriteEndElement();
                    Ct = 0;
                    foreach (Generic.KeyValuePair<string, string> kvp in p_KeyValuePairs) // Write each named value in turn
                    {
                        Ct += 1;
                        TL.LogMessage("  Writing Value", "  " + Ct.ToString() + " " + kvp.Key + " = " + kvp.Value);
                        if (kvp.Value == null)
                            TL.LogMessage("  Writing Value", "  WARNING - Suppplied Value is Nothing not empty string");
                        switch (kvp.Key)
                        {
                            case object _ when COLLECTION_DEFAULT_VALUE_NAME // Ignore the default value entry
                           :
                                {
                                    break;
                                }

                            default:
                                {
                                    Writer.WriteStartElement(VALUE_ELEMENT_NAME); // Write the element name
                                    Writer.WriteAttributeString(NAME_ATTRIBUTE_NAME, kvp.Key); // Write the name attribute
                                    Writer.WriteAttributeString(VALUE_ATTRIBUTE_NAME, kvp.Value); // Write the value attribute
                                    Writer.WriteEndElement(); // Close this element
                                    break;
                                }
                        }
                    }
                    Writer.WriteEndElement();

                    // Flush and close the writer object to complete writing of the XML file. 
                    Writer.Close(); // Actualy write the XML to a file
                }
                try
                {
                    FStream.Flush();
                    FStream.Close();
                    FStream.Dispose();
                    FStream = null;
                }
                catch (Exception ex)
                {
                }// Ensure no error occur from this tidying up

                Writer = null;
                try // New file successfully created so now rename the current file to original and rename the new file to current
                {
                    if (p_CheckForCurrentProfileStore)
                        FileStore.Rename(p_SubKeyName + @"\" + VALUES_FILENAME, p_SubKeyName + @"\" + VALUES_FILENAME_ORIGINAL);
                    try
                    {
                        FileStore.Rename(p_SubKeyName + @"\" + VALUES_FILENAME_NEW, p_SubKeyName + @"\" + VALUES_FILENAME);
                    }
                    catch (Exception ex2)
                    {
                        // Attempt to rename new file as current failed so try and restore the original file
                        TL.Enabled = true;
                        TL.LogMessage("XMLAccess:WriteValues", "Unable to rename new profile file to current - " + p_SubKeyName + @"\" + VALUES_FILENAME_NEW + "to " + p_SubKeyName + @"\" + VALUES_FILENAME + " " + ex2.ToString());
                        try
                        {
                            FileStore.Rename(p_SubKeyName + @"\" + VALUES_FILENAME_ORIGINAL, p_SubKeyName + @"\" + VALUES_FILENAME);
                        }
                        catch (Exception ex3)
                        {
                            // Restoration also failed so no clear recovery from this point
                            TL.Enabled = true;
                            TL.LogMessage("XMLAccess:WriteValues", "Unable to rename original profile file to current - " + p_SubKeyName + @"\" + VALUES_FILENAME_ORIGINAL + "to " + p_SubKeyName + @"\" + VALUES_FILENAME + " " + ex3.ToString());
                        }
                    }
                }
                catch (Exception ex1)
                {
                    // No clear remedial action as the current file rename failed so just leave as is
                    TL.Enabled = true;
                    TL.LogMessage("XMLAccess:WriteValues", "Unable to rename current profile file to original - " + p_SubKeyName + @"\" + VALUES_FILENAME + "to " + p_SubKeyName + @"\" + VALUES_FILENAME_ORIGINAL + " " + ex1.ToString());
                }

                WriterSettings = null;

                swSupport.Stop();
                TL.LogMessage("  WriteValues", "  Created cache entry " + p_SubKeyName + " - " + swSupport.ElapsedMilliseconds + " milliseconds");
            }
            catch (Exception ex)
            {
                TL.LogMessageCrLf("  WriteValues", "  Exception " + p_SubKeyName + " " + ex.ToString());
                Interaction.MsgBox("XMLAccess:Writevalues " + p_SubKeyName + " " + ex.ToString());
            }
        }

        private void MigrateKey(RegistryKey p_FromKey, string p_ToDir)
        {
            // Subroutine used for one off copy of registry profile to new XML profile
            string[] ValueNames, SubKeyNames;
            RegistryKey FromKey;
            Generic.SortedList<string, string> Values = new Generic.SortedList<string, string>();
            // Recusively copy contents from one key to the other
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
            TL.LogMessage("MigrateKeys " + RecurseDepth.ToString(), "To Directory: " + p_ToDir);
            try
            {
                TL.LogMessage("MigrateKeys" + RecurseDepth.ToString(), "From Key: " + p_FromKey.Name + ", SubKeyCount: " + p_FromKey.SubKeyCount.ToString() + ", ValueCount: " + p_FromKey.ValueCount.ToString());
            }
            catch (Exception ex)
            {
                TL.LogMessage("MigrateKeys", "Exception processing \"" + p_ToDir + "\": " + ex.ToString());
                TL.LogMessage("MigrateKeys", "Exception above: no action taken, continuing...");
            }

            // First copy values from the from key to the to key
            ValueNames = p_FromKey.GetValueNames();
            Values.Add(COLLECTION_DEFAULT_VALUE_NAME, COLLECTION_DEFAULT_UNSET_VALUE);
            foreach (string ValueName in ValueNames)
            {
                if (ValueName == "")
                {
                    Values.Remove(COLLECTION_DEFAULT_VALUE_NAME); // Remove the default unset value and replace with actual value
                    Values.Add(COLLECTION_DEFAULT_VALUE_NAME, p_FromKey.GetValue(ValueName).ToString());
                }
                else
                    Values.Add(ValueName, p_FromKey.GetValue(ValueName).ToString());
            }
            WriteValues(p_ToDir, ref Values); // Write values to XML file

            // Now process the keys
            SubKeyNames = p_FromKey.GetSubKeyNames();
            foreach (string SubKeyName in SubKeyNames)
            {
                FromKey = p_FromKey.OpenSubKey(SubKeyName); // Point at the source to copy to it
                CreateKey(p_ToDir + @"\" + SubKeyName);
                MigrateKey(FromKey, p_ToDir + @"\" + SubKeyName); // Recursively process each key
                FromKey.Close();
            }
            swLocal.Stop(); TL.LogMessage("  ElapsedTime " + RecurseDepth.ToString(), "  " + swLocal.ElapsedMilliseconds + " milliseconds, Completed Directory: " + p_ToDir);
            RecurseDepth -= 1; // Decrement the recursion depth counter
            swLocal = null;
        }

        private void GetProfileMutex(string Method, string Parameters)
        {
            // Get the profile mutex or log an error and throw an exception that will terminate this profile call and return to the calling application
            GotMutex = ProfileMutex.WaitOne(PROFILE_MUTEX_TIMEOUT, false);
            if (!GotMutex)
            {
                TL.LogMessage("GetProfileMutex", "***** WARNING ***** Timed out waiting for Profile mutex in " + Method + ", parameters: " + Parameters);
                LogEvent(Method, "Timed out waiting for Profile mutex in " + Method + ", parameters: " + Parameters, EventLogEntryType.Error, EventLogErrors.XMLProfileMutexTimeout, null/* TODO Change to default(_) if this is not a reference type */);
                throw new ProfilePersistenceException("Timed out waiting for Profile mutex in " + Method + ", parameters: " + Parameters);
            }
        }
    }

}
