using System;
using System.Net;

namespace ASCOM.Utilities.CS
{
/// <summary>

/// ''' Data class for items that are added to the Chooser display combo box

/// ''' </summary>
public class ChooserItem : IComparable // Provides support for the device list combo box to sort items into a display order
{

    /// <summary>
    ///     ''' Base initialiser, sets all properties to default values
    ///     ''' </summary>
    public ChooserItem()
    {
        ChooserID = Guid.NewGuid();
        IsComDriver = false;
        ProgID = "";
        Name = "";
        DeviceNumber = 0;
        HostName = IPAddress.Loopback.ToString();
        Port = 0;
    }

    /// <summary>
    ///     ''' Initialiser called to create an item for a COM driver
    ///     ''' </summary>
    ///     ''' <param name="progId">The driver's ProgID</param>
    ///     ''' <param name="name">The driver's display name</param>
    public ChooserItem(string progId, string name) : this()
    {
        this.ProgID = progId;
        this.Name = name;
        this.IsComDriver = true;
    }

    /// <summary>
    ///     ''' Initialiser called to create an item for a new Alpaca driver i.e. one that is not already fronted by a COM driver 
    ///     ''' </summary>
    ///     ''' <param name="deviceNumber">The Alpaca device access number</param>
    ///     ''' <param name="hostName">The host name (or IP address) used to access the Alpaca device</param>
    ///     ''' <param name="port">The Alpaca port number</param>
    ///     ''' <param name="name">The device's display name</param>
    public ChooserItem(string deviceUniqueId, int deviceNumber, string hostName, int port, string name) : this()
    {
        this.DeviceUniqueID = deviceUniqueId;
        this.DeviceNumber = deviceNumber;
        this.HostName = hostName;
        this.Port = port;
        this.Name = name;
        this.IsComDriver = false;
    }

    /// <summary>
    ///     ''' ID that is unique within this list of Chooser items, just used to ensure that drivers that have the same display name appear differently
    ///     ''' </summary>
    ///     ''' <returns>The Chooser item's unique ID</returns>
    public Guid ChooserID { get; set; }

    /// <summary>
    ///     ''' ID that is globally unique for this Alpaca device
    ///     ''' </summary>
    ///     ''' <returns>ASCOM device's unique ID</returns>
    public string DeviceUniqueID { get; set; }

    /// <summary>
    ///     ''' Flag indicating whether this is a COM or new Alpaca driver
    ///     ''' </summary>
    ///     ''' <returns>True if the item is a new Alpaca driver, False if the item is an existing COM driver</returns>
    ///     ''' <remarks>Pre-existing COM drivers that front Alpaca devices are flagged as COM drivers. Only newly discovered Alpaca devices are flagged as such</remarks>
    public bool IsComDriver { get; }

    /// <summary>
    ///     ''' The COM ProgID
    ///     ''' </summary>
    ///     ''' <returns></returns>
    public string ProgID { get; set; }

    /// <summary>
    ///     ''' The device's display name
    ///     ''' </summary>
    ///     ''' <returns></returns>
    public string Name { get; set; }

    /// <summary>
    ///     ''' The Alpaca device access number
    ///     ''' </summary>
    ///     ''' <returns></returns>
    public int DeviceNumber { get; set; }

    /// <summary>
    ///     ''' The host name or IP address of the Alpaca device
    ///     ''' </summary>
    ///     ''' <returns></returns>
    public string HostName { get; set; }

    /// <summary>
    ///     ''' The Alpaca IP port through which the device can be accessed
    ///     ''' </summary>
    ///     ''' <returns></returns>
    public int Port { get; set; }

    /// <summary>
    ///     ''' Compares two ChooserItems items based on the Name field concatenated with a unique GUID
    ///     ''' </summary>
    ///     ''' <param name="otherChooserItemAsObject"></param>
    ///     ''' <returns>Less than zero if this instance precedes the other item in the sort order or
    ///     '''          Zero if the items occupy the same position in the sort order or 
    ///     '''          Greater than zero if this instance comes after the other item in the sort order</returns>
    ///     ''' <remarks>The concatenation is used to ensure that tow entries with identical descriptive names can be seen as distinct devices.</remarks>
    public int CompareTo(object otherChooserItemAsObject)
    {
        string myNameId, otherNameId;

        ChooserItem otherChooserItem = (ChooserItem)otherChooserItemAsObject;

        if (this.IsComDriver)
        {
            myNameId = $"{this.Name}{this.ProgID}";
            otherNameId = $"{otherChooserItem.Name}{otherChooserItem.ProgID}";
        }
        else
        {
            myNameId = $"{this.Name}{this.ChooserID}";
            otherNameId = $"{otherChooserItem.Name}{otherChooserItem.ChooserID}";
        }

        return myNameId.CompareTo(otherNameId);
    }
}
}