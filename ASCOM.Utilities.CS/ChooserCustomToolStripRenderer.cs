
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

namespace ASCOM.Utilities.CS
{
/// <summary>
/// ''' Custom renderer for the Chooser tool strip
/// ''' </summary>

public class ChooserCustomToolStripRenderer : ToolStripProfessionalRenderer
{

    /// <summary>
    ///     ''' Prevent "selected "colour changes when hovering over disabled menu items 
    ///     ''' </summary>
    ///     ''' <param name="e"></param>
    protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
    {
        if ((e.Item.Enabled))
            base.OnRenderMenuItemBackground(e);
    }

    /// <summary>
    ///     ''' Respect the BackBolor property set for labels - without this they always appear with a grey background.
    ///     ''' </summary>
    ///     ''' <param name="e"></param>
    protected override void OnRenderLabelBackground(ToolStripItemRenderEventArgs e)
    {
        if (!(e.Item.BackColor == Color.WhiteSmoke))
        {
            SolidBrush myBrush = new SolidBrush(e.Item.BackColor);
            e.Graphics.FillRectangle(myBrush, e.Item.ContentRectangle);
            myBrush.Dispose();
        }
    }
}
}