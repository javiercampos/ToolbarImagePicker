using System.Windows.Forms;

namespace Jcl.Tools.WindowsForms.ToolbarImagePicker
{
	internal sealed class NonFlickeringListView : ListView
	{
		public NonFlickeringListView()
		{
			DoubleBuffered = true;
			SetStyle(ControlStyles.EnableNotifyMessage, true);
		}

		protected override void OnNotifyMessage(Message m)
		{
			if (m.Msg != 0x14)
			{
				base.OnNotifyMessage(m);
			}
		}
	}
}
