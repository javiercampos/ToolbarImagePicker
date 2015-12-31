using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace Jcl.Tools.WindowsForms.ToolbarImagePicker
{
	public sealed class ToolbarImagePickerEditor : UITypeEditor
	{
		private static string _lastSelectedResource;

		private readonly Dictionary<string, IEnumerable<string>> _largeProperties = new Dictionary<string, IEnumerable<string>>()
		{
			{"LargeImage", new[]{ "SmallImage", "Image" }},
			{"LargeGlyph", new[]{ "Glyph", "SmallGlyph" } },
		};

		private readonly Dictionary<string, IEnumerable<string>> _smallProperties = new Dictionary<string, IEnumerable<string>>()
		{
			{"Image", new[]{ "LargeImage" }},
			{"SmallImage", new[]{ "LargeImage" }},
			{"Glyph", new[]{ "LargeGlyph"}},
			{"SmallGlyph", new[]{ "LargeGlyph"}},
		};

		private IWindowsFormsEditorService _editorService;

		private IEnumerable<string> _reversePropertyActualNames;
		private bool? _isSmall;

		private IEnumerable<PropertyInfo> GetAvailableReversePropertyNames(object instance, string propertyName)
		{
			// This should never happen in its current state, but it may happen on future versions and
			// I'll probably scratch my head and won't find this when it fails
			if (_reversePropertyActualNames == null)
				return Enumerable.Empty<PropertyInfo>();
			var pl = instance.GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).ToList();
			return pl.Where(x => _reversePropertyActualNames.Contains(x.Name) && x.Name != propertyName);
		}

		private ToolbarImagePickerForm GetEditForm(string propertyName, object instance)
		{
			var f = new ToolbarImagePickerForm(_lastSelectedResource);
			_reversePropertyActualNames = Enumerable.Empty<string>();
			if (_smallProperties.ContainsKey(propertyName))
			{
				_reversePropertyActualNames = _smallProperties[propertyName];
				_isSmall = true;
			}
			else if (_largeProperties.ContainsKey(propertyName))
			{
				_reversePropertyActualNames = _largeProperties[propertyName];
				_isSmall = false;
			}

			var actualPropertyNames = GetAvailableReversePropertyNames(instance, propertyName).Select(p => p.Name).ToList();
			f.AllowSelectSmallLarge(_isSmall.HasValue ? (_isSmall.Value ? ToolbarImagePickerForm.SmallSize : ToolbarImagePickerForm.LargeSize) : -1, propertyName, actualPropertyNames);

			return f;
		}


		#region UITypeEditor Overrides

		public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
		{
			if (provider == null || context == null)
				return base.EditValue(context, provider, value);

			try
			{
				_editorService = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
				if (_editorService != null && context.PropertyDescriptor != null)
				{
					var propName = context.PropertyDescriptor.Name;
					object returnValue;
					using (var frm = GetEditForm(propName, context.Instance))
					{
						frm.SetContext(context, provider, value);

						var dr = _editorService.ShowDialog(frm);

						// We make the default VS resource picker return DialogResult.Yes
						if (dr == DialogResult.Yes)
						{
							var x = frm.GetSelectedDefaultPickerValue();
							frm.EndResourcePicker();
							return x;
						}
						// Our own picker returns DialogResult.OK
						if (dr != DialogResult.OK)
							return base.EditValue(context, provider, value);

						returnValue = frm.GetSelectedImage();
						var returnImage = frm.GetSelectedResourceImage();

						if (frm.SetSmallAndLarge
							&& _isSmall.HasValue
							&& returnImage != null
							&& (
											 (returnImage.Size == ToolbarImagePickerForm.SmallSize && _isSmall.Value)
										|| (returnImage.Size == ToolbarImagePickerForm.LargeSize && !_isSmall.Value)
								 )
							)
						{
							foreach (var p in GetAvailableReversePropertyNames(context.Instance, propName))
								p.SetValue(context.Instance, frm.GetSelectedImage(_isSmall.Value ? ToolbarImagePickerForm.LargeSize : ToolbarImagePickerForm.SmallSize));
						}
						_lastSelectedResource = frm.LastSelectedResource;
					}
					return returnValue;
				}
			}
			catch (Exception e)
			{
				MessageBox.Show($"Fatal error{Environment.NewLine}Exception {e.GetType().FullName}{Environment.NewLine}{e.Message}", @"Fatal Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			return base.EditValue(context, provider, value);
		}

		public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
		{
			return context.PropertyDescriptor == null ? base.GetEditStyle(context) : UITypeEditorEditStyle.Modal;
		}

		public override bool GetPaintValueSupported(ITypeDescriptorContext context)
		{
			return true;
		}

		public override void PaintValue(PaintValueEventArgs e)
		{
			var image = e.Value as Image;
			if (image == null) return;
			var r = e.Bounds;
			r.Width--;
			r.Height--;
			e.Graphics.DrawRectangle(SystemPens.WindowFrame, r);
			e.Graphics.DrawImage(image, e.Bounds);
		}

		#endregion
	}
}