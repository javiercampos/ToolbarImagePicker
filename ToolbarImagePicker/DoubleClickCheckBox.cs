using System.Windows.Forms;

namespace Jcl.Tools.WindowsForms.ToolbarImagePicker
{
	public class DoubleClickCheckBox : CheckBox
	{
		public DoubleClickCheckBox()
		{
			SetStyle(ControlStyles.StandardClick | ControlStyles.StandardDoubleClick, true);
		}

		protected override void OnMouseUp(MouseEventArgs mevent)
		{
			ResetFlagsandPaint();
		}
	}
}