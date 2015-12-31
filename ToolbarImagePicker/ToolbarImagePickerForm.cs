using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using EnvDTE;

namespace Jcl.Tools.WindowsForms.ToolbarImagePicker
{
	internal sealed partial class ToolbarImagePickerForm : Form
	{
		internal class ResourceImage
		{
			public ResourceImage(string resourceName, string imageName, int size, Image image, string resXName)
			{
				ResourceName = resourceName;
				Image = image;
				ImageName = imageName;
				ResXName = resXName;
				Size = size;
			}

			public Image Image { get; }
			public string ImageName { get; }
			public string ResourceName { get; }
			public string ResXName { get; }
			public int Size { get; }
		}

		private ITypeDescriptorContext _context;
		private CancellationTokenSource _ctsSource;
		private Project _currentProject;
		private object _defaultValue;
		private object _lastSelectedValue;
		private IEnumerable<ResourceImage> _loadedResourceImages;
		private IServiceProvider _provider;
		private dynamic _resourcePickerDialog;
		private Task<IEnumerable<ResourceImage>> _runningTask;
		private string _selectedResourceFile;
		private System.Threading.Timer _teTimer;
		private ResourceImage _selectedResourceImage;

		private const int SearchTimerTimeoutMilliseconds = 300;

		public const int SmallSize = 16;
		public const int LargeSize = 32;
		private const string SmallSizeSuffix = "_16";
		private const string LargeSizeSuffix = "_32";

		private ResourceImage SelectedResourceImage
		{
			get { return _selectedResourceImage; }
			set
			{
				_selectedResourceImage = value;
				if (!cbAllowSelectSmallLarge.Visible) return;
				if (	 (value == null)
						|| (value.Size == SmallSize && GetSelectedImage(LargeSize, false) == null)
						|| (value.Size == LargeSize && GetSelectedImage(SmallSize, false) == null)
						|| (value.Size != SmallSize && value.Size != LargeSize)
						)
					cbAllowSelectSmallLarge.Enabled = false;
				else
					cbAllowSelectSmallLarge.Enabled = true;
			}
		}

		public ToolbarImagePickerForm(string lastResourceFile)
		{
			_strings = new LocalizedStringResource();
			DoubleBuffered = true;
			InitializeComponent();
			cbSelectSize.SelectedIndex = 1;
			LastSelectedResource = lastResourceFile;
			lvItems.Focus();
		}


		public string LastSelectedResource { get; private set; }
		public bool SetSmallAndLarge => cbAllowSelectSmallLarge.Visible && cbAllowSelectSmallLarge.Enabled && cbAllowSelectSmallLarge.Checked;

		private bool CurrentIsSmall => cbAllowSelectSmallLarge.Tag is int && (int)cbAllowSelectSmallLarge.Tag == SmallSize;
		private bool HasPreferredSize => cbAllowSelectSmallLarge.Tag is int && (int)cbAllowSelectSmallLarge.Tag > 0;
		private int? PreferredSizeVal => cbAllowSelectSmallLarge.Tag as int?;

		private dynamic _strings;

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			Localize();
		}

		public void Localize()
		{
			Text = _strings.EditorTitle;
			tabToolbarPicker.Text = _strings.ToolbarPickerTabTitle;
			tabVisualStudioPicker.Text = _strings.ToolbarPickerResourcePicker;
			teFilter.Cue = _strings.SearchCue;
			btnOk.Text = _strings.ButtonOk;
			btnCancel.Text = _strings.ButtonCancel;
		}

		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);

			// Load Visual Studio resource picker and integrate it
			try
			{
				_resourcePickerDialog = DefaultResourcePickerPlug.GetPickerForm(_context, _provider, _defaultValue);
				if (_resourcePickerDialog.Object is Form)
				{
					DefaultResourcePickerPlug.SetOkAction(() => { DialogResult = DialogResult.Yes; });
					DefaultResourcePickerPlug.SetCancelAction(() => { DialogResult = DialogResult.Cancel; });
					_resourcePickerDialog.TopLevel = false;
					tabVisualStudioPicker.Controls.Add((Form) _resourcePickerDialog.Object);
					_resourcePickerDialog.FormBorderStyle = FormBorderStyle.None;
					_resourcePickerDialog.Dock = DockStyle.Fill;
					_resourcePickerDialog.Show();
				}
			}
			catch
			{
				_resourcePickerDialog = null;
				tabVisualStudioPicker.Enabled = false;
			}

			var resourceList = DteProjectResourceHelper.GetResourceFiles(_currentProject);
			LoadResourceList(resourceList);
			CreateListViewItemsForCurrentResource();
		}

		#region To be called by editor

		public void SetContext(ITypeDescriptorContext context, IServiceProvider provider, object defaultValue)
		{
			_context = context;
			_provider = provider;
			_defaultValue = _lastSelectedValue = defaultValue;
			if (_lastSelectedValue != null)
			{
				_selectedResourceFile = GetResourceNameFromImage(_lastSelectedValue);
			}
			var projectItem = _provider.GetService(typeof(ProjectItem)) as ProjectItem;
			if (projectItem != null) _currentProject = projectItem.ContainingProject;
			if (_lastSelectedValue != null)
				SelectLastEntry(_lastSelectedValue, true);
		}

		public void AllowSelectSmallLarge(int preferredSize, string currentProp, IEnumerable<string> reverseProperties)
		{
			Debug.Assert(reverseProperties != null);
			var reverseProp = reverseProperties.ToList();

			cbAllowSelectSmallLarge.Tag = preferredSize;
			if (reverseProp.Any() && (preferredSize == SmallSize || preferredSize == LargeSize))
			{
				cbAllowSelectSmallLarge.Text = string.Format(_strings.AllowSmallLarge,
					ToSingleString(reverseProp, ","),
					preferredSize == SmallSize ? _strings.AllowSmallLargeLarge : _strings.AllowSmallLargeSmall, reverseProp.Count == 1 ? _strings.AllowSmallLargePropertySingular : _strings.AllowSmallLargePropertyPlural);
				cbSelectSize.SelectedIndex = preferredSize == SmallSize ? 0 : 1;
				cbAllowSelectSmallLarge.Visible = cbAllowSelectSmallLarge.Checked = true;
			}
			else
			{
				cbAllowSelectSmallLarge.Visible = cbAllowSelectSmallLarge.Checked = false;
			}
		}

		public void EndResourcePicker()
		{
			if (_resourcePickerDialog != null)
				DefaultResourcePickerPlug.EndPickerForm(_resourcePickerDialog);
		}

		#endregion


		#region Editor return values

		public object GetSelectedDefaultPickerValue()
		{
			return DefaultResourcePickerPlug.GetPickerValue(_resourcePickerDialog);
		}

		internal ResourceImage GetSelectedResourceImage()
		{
			return SelectedResourceImage;
		}

		public object GetSelectedImage(bool alertIfNotFound = true)
		{
			if (SelectedResourceImage == null)
				return null;

			return SelectImage($"{SelectedResourceImage.ResXName}.{SelectedResourceImage.ResourceName}", alertIfNotFound);
		}

		public object GetSelectedImage(int size, bool alertIfNotFound = true)
		{
			if (SelectedResourceImage == null)
				return null;

			return SelectImage($"{SelectedResourceImage.ResXName}.{SelectedResourceImage.ImageName + "_" + size}", alertIfNotFound);
		}

		private object SelectImage(string resourceName, bool alertIfNotFound)
		{
			var found = LoadImageReference(resourceName);
			if (found == null && alertIfNotFound)
			{
				MessageBox.Show($"Fatal error{Environment.NewLine}Can't find resource: {resourceName}", @"Fatal Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return null;
			}
			return found;
		}

		#endregion

		private void CreateListViewItemsForCurrentResource()
		{
			if (lvItems.InvokeRequired)
			{
				lvItems.Invoke(new MethodInvoker(CreateListViewItemsForCurrentResource));
				return;
			}
			if (_runningTask != null)
			{
				_ctsSource.Cancel();
				_runningTask.Wait();
			}

			lvItems.Clear();
			lvItems.SmallImageList?.Dispose();
			lvItems.LargeImageList?.Dispose();
			lvItems.SmallImageList = null;
			lvItems.LargeImageList = null;
			_loadedResourceImages = null;

			if (cbSelectResource.SelectedItem == null)
				return;

			var currentResourceFile = (ResourceFile)cbSelectResource.SelectedItem;

			var rx = new ResXResourceReader(currentResourceFile.Path) { BasePath = Path.GetDirectoryName(currentResourceFile.Path) };
			var dict = rx.GetEnumerator();

			_ctsSource = new CancellationTokenSource();
			_runningTask = LoadImagesFromResourceAsync(currentResourceFile.ResourceObjectName, dict, _ctsSource.Token);

			var contTask = _runningTask.ContinueWith(
				t =>
				{
					_loadedResourceImages = t.Result;
					CreateView(t.Result);
				}, _ctsSource.Token, TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.Default);

			if (_lastSelectedValue != null)
			{
				var closureLastSelectedValue = _lastSelectedValue;
				_lastSelectedValue = null;
				contTask.ContinueWith(t =>
				{
					SelectLastEntry(closureLastSelectedValue);
				}, _ctsSource.Token, TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.Default);
			}

			_runningTask.Start();
		}

		private void LoadResourceList(IEnumerable<ResourceFile> resourceList)
		{
			cbSelectResource.SelectedIndexChanged -= cbSelectResource_SelectedIndexChanged;
			cbSelectResource.Items.Clear();
			if (resourceList != null) cbSelectResource.Items.AddRange(resourceList.OfType<object>().ToArray());
			if (cbSelectResource.Items.Count > 0)
				cbSelectResource.SelectedIndex = 0;

			if (_selectedResourceFile != null)
			{
				var sel = cbSelectResource.Items.OfType<ResourceFile>().FirstOrDefault(x => x.DisplayName == _selectedResourceFile);
				if (sel != null)
					cbSelectResource.SelectedItem = sel;
				_selectedResourceFile = null;
			}
			else
			{
				var sel = cbSelectResource.Items.OfType<ResourceFile>().FirstOrDefault(x => x.DisplayName == LastSelectedResource);
				if (sel != null)
					cbSelectResource.SelectedItem = sel;
			}
			cbSelectResource.SelectedIndexChanged += cbSelectResource_SelectedIndexChanged;
		}

		static Task<IEnumerable<ResourceImage>> LoadImagesFromResourceAsync(string resxname, IDictionaryEnumerator resourceDictionaryEnumerator, CancellationToken ctsToken)
		{
			var task = new Task<IEnumerable<ResourceImage>>(() =>
			{
				var lr = new List<ResourceImage>();
				while (resourceDictionaryEnumerator.MoveNext())
				{
					ctsToken.ThrowIfCancellationRequested();
					var image = resourceDictionaryEnumerator.Value as Image;
					if (image == null) continue;
					var resKey = Convert.ToString(resourceDictionaryEnumerator.Key);
					lr.Add(new ResourceImage(resKey, GetImageDisplayNameFromResourceName(resKey), GetImageSizeFromResourceName(resKey), image, resxname));
				}
				return lr;
			}, ctsToken);
			return task;
		}

		void CreateItemsOnListView(ListView view, IEnumerable<ResourceImage> resourceImages, string nameFilter)
		{
			var viewInt = view;
			var imagesInt = resourceImages.ToList();

			if (view.InvokeRequired)
			{
				view.Invoke(new MethodInvoker(() => CreateItemsOnListView(viewInt, imagesInt, nameFilter)));
				return;
			}

			var resImages = imagesInt.Where(
				x => string.IsNullOrWhiteSpace(nameFilter) || x.ImageName.Contains(nameFilter))
				.GroupBy(x => x.ImageName).Select(g => g.First());

			try
			{
				viewInt.BeginUpdate();
				viewInt.Items.Clear();
				foreach (var ri in resImages)
				{
					var lvi = viewInt.Items.Add(ri.ImageName, ri.ImageName, ri.ImageName);
					lvi.SubItems.Add($"{ri.ResXName}.{ri.ResourceName}");
					lvi.SubItems.Add($"{ri.ResXName}.{ri.ImageName}");
					lvi.Tag = ri;
				}
			}
			finally
			{
				viewInt.EndUpdate();
			}
		}

		private void CreateView(IEnumerable<ResourceImage> resourceImages)
		{
			if (lvItems.InvokeRequired)
			{
				lvItems.Invoke(new MethodInvoker(() => CreateView(resourceImages)));
				return;
			}
			var resIm = resourceImages.ToList();
			var imgListSmallSize = GenerateImageList(resIm, SmallSize, true, LargeSize);
			var imgListLargeSize = GenerateImageList(resIm, LargeSize, true, SmallSize);
			lvItems.SmallImageList = imgListSmallSize;
			lvItems.LargeImageList = imgListLargeSize;
			CreateItemsOnListView(lvItems, resIm, teFilter.Text);
		}

		private void SelectLastEntry(object selected, bool onlySize = false)
		{
			if (lvItems.InvokeRequired)
			{
				lvItems.Invoke(new MethodInvoker(() => SelectLastEntry(selected)));
				return;
			}

			ResourceImage resi = null;
			if (selected != null && _loadedResourceImages != null)
			{
				var refString = GetNameFromImage(selected);
				if (refString != null)
					resi = _loadedResourceImages.FirstOrDefault(x => refString == $"{x.ResXName}.{x.ResourceName}");
			}
			if (resi == null) return;
			switch (resi.Size)
			{
				case SmallSize:
					cbSelectSize.SelectedIndex = 0;
					break;
				case LargeSize:
					cbSelectSize.SelectedIndex = 1;
					break;
				default:
					// Find closest match
					cbSelectSize.SelectedIndex = Math.Abs(resi.Size - LargeSize) >= Math.Abs(resi.Size - SmallSize) ? 1 : 0;
					break;
			}
			if (onlySize)
				return;

			var listViewItem = lvItems.Items.OfType<ListViewItem>().FirstOrDefault(x => x.SubItems.Count > 2 && x.SubItems[2].Text == $"{resi.ResXName}.{resi.ImageName}");
			if (listViewItem != null)
			{
				listViewItem.Selected = true;
				lvItems.FocusedItem = listViewItem;
				lvItems.EnsureVisible(listViewItem.Index);
			}
			// By now, sub-images should be loaded
			var cbControls = panelChooser.Controls.OfType<CheckBox>().ToList();
			if (cbControls.Any())
			{
				var cbSelected =
					cbControls.FirstOrDefault(
						x =>
							x.Tag is ResourceImage && ((ResourceImage)x.Tag).ResXName == resi.ResXName &&
							((ResourceImage)x.Tag).ResourceName == resi.ResourceName);
				if (cbSelected != null && !cbSelected.Checked)
				{
					foreach (var rbl in cbControls)
					{
						rbl.Checked = rbl == cbSelected;
						SetCheckBoxDesign(rbl);
					}
				}
			}
		}

		static ImageList GenerateImageList(IEnumerable<ResourceImage> resourceImages, int imageListSize, bool includeNonMatching, params int[] includeSizes)
		{
			var imgList = new ImageList();
			var added = new List<string>();
			imgList.ImageSize = new Size(imageListSize, imageListSize);
			var allResources = resourceImages.ToList();

			foreach (var ri in allResources)
			{
				if (added.Contains(ri.ImageName))
					continue;

				// If this image is the correct and desired size
				if (ri.Size == imageListSize)
				{
					imgList.Images.Add(ri.ImageName, ri.Image);
					added.Add(ri.ImageName);
					continue;
				}
				// If not, try to find the correct image
				var correctImage = allResources.FirstOrDefault(x => x.ImageName == ri.ImageName && x.Size == imageListSize);
				if (correctImage != null)
				{
					imgList.Images.Add(correctImage.ImageName, correctImage.Image);
					added.Add(correctImage.ImageName);
					continue;
				}

				// Try to find another with any of the other sizes
				var partiallyCorrectImage = allResources.FirstOrDefault(x => x.ImageName == ri.ImageName && includeSizes.Contains(x.Size));
				if (partiallyCorrectImage != null)
				{
					imgList.Images.Add(partiallyCorrectImage.ImageName, partiallyCorrectImage.Image);
					added.Add(partiallyCorrectImage.ImageName);
					continue;
				}
				// If nothing works, include whatever image this is
				if (includeNonMatching)
				{
					imgList.Images.Add(ri.ResourceName, ri.Image);
					added.Add(ri.ResourceName);
				}
			}
			return imgList;
		}

		#region Global object references

		private static string GetImageDisplayNameFromResourceName(string resName)
		{
			if (resName.EndsWith(SmallSizeSuffix))
				return resName.Substring(0, resName.Length - SmallSizeSuffix.Length);
			if (resName.EndsWith(LargeSizeSuffix))
				return resName.Substring(0, resName.Length - LargeSizeSuffix.Length);
			return resName;
		}

		private static int GetImageSizeFromResourceName(string resName)
		{
			if (resName.EndsWith(SmallSizeSuffix))
				return SmallSize;
			if (resName.EndsWith(LargeSizeSuffix))
				return LargeSize;
			// Not managed
			return -1;
		}

		private string GetNameFromImage(object obj)
		{
			var referenceService = _provider.GetService(typeof(IReferenceService)) as IReferenceService;
			return referenceService?.GetName(obj);
		}

		private string GetResourceNameFromImage(object obj)
		{
			var name = GetNameFromImage(obj);
			return name?.Substring(0, name.IndexOf('.'));
		}

		private Image LoadImageReference(string resourceName)
		{
			var referenceService = _provider.GetService(typeof(IReferenceService)) as IReferenceService;
			return referenceService?.GetReference(resourceName) as Image;
		}

		#endregion
		private void SetCheckBoxDesign(CheckBox sender)
		{
			sender.FlatAppearance.BorderColor = sender.Checked ? SystemColors.Highlight : default(Color);
			sender.BackColor = sender.Checked ? SystemColors.Highlight : Color.Transparent;
			sender.ForeColor = sender.Checked ? SystemColors.HighlightText : SystemColors.ControlText;

		}


		#region Form control events

		private void btnOk_Click(object sender, EventArgs e)
		{
			if (SelectedResourceImage == null)
			{
				MessageBox.Show(_strings.SelectingNoImage, _strings.SelectingNoImageTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			if (SelectedResourceImage.Size == SmallSize && HasPreferredSize && (PreferredSizeVal ?? -1) == LargeSize)
			{
				var dr = MessageBox.Show(_strings.SelectingDifferentSmall, _strings.SelectingDifferentTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
				if (dr == DialogResult.No)
					return;
			}
			else if (SelectedResourceImage.Size == LargeSize && HasPreferredSize && (PreferredSizeVal ?? -1) == SmallSize)
			{
				var dr = MessageBox.Show(_strings.SelectingDifferentLarge, _strings.SelectingDifferentTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
				if (dr == DialogResult.No)
					return;
			}
			DialogResult = DialogResult.OK;
		}

		private void cbSelectResource_SelectedIndexChanged(object sender, EventArgs e)
		{
			LastSelectedResource = ((ResourceFile)cbSelectResource.SelectedItem).DisplayName;
			CreateListViewItemsForCurrentResource();
		}

		private void cbSelectSize_SelectedIndexChanged(object sender, EventArgs e)
		{
			var cb = sender as ComboBox;
			if (cb == null) return;
			lvItems.View = cb.SelectedIndex == 0 ? View.List : View.LargeIcon;
		}

		private void lvItems_DoubleClick(object sender, EventArgs e)
		{
			if (lvItems.SelectedItems.Count > 0)
				btnOk.PerformClick();
		}

		private void lvItems_SelectedIndexChanged(object sender, EventArgs e)
		{
			var lvi = sender as ListView;
			if (lvi == null) return;

			SelectedResourceImage = null;

			if (_loadedResourceImages == null)
				return;

			try
			{
				panelChooser.SuspendLayout();
				panelChooser.Controls.Clear();
				if (lvi.SelectedItems.Count == 0)
					return;
				var resSel = lvi.SelectedItems[0].Tag as ResourceImage;
				if (resSel == null)
					return;

				var anySelected = false;
				var resList = _loadedResourceImages.Where(x => x.ImageName == resSel.ImageName).ToList();
				var yposition = 0;

				foreach (var resourceImage in resList)
				{
					var shouldBeChecked =  (HasPreferredSize && CurrentIsSmall && resourceImage.Size == SmallSize)
															|| (HasPreferredSize && !CurrentIsSmall && resourceImage.Size == LargeSize)
															|| (!HasPreferredSize && cbSelectSize.SelectedIndex == 0 && resourceImage.Size == SmallSize)
															|| (!HasPreferredSize && cbSelectSize.SelectedIndex == 1 && resourceImage.Size == LargeSize);

					if (anySelected)
						shouldBeChecked = false;

					var rb = new DoubleClickCheckBox
					{
						Visible = true,
						Text = resourceImage.ResourceName,
						AutoCheck = false,
						Appearance = Appearance.Button,
						AutoSize = true,
						FlatStyle = FlatStyle.Flat,
						Location = new Point(0, yposition),
						Checked = shouldBeChecked,
						Margin = new Padding(10),
						Image = resourceImage.Image,
						TextImageRelation = TextImageRelation.ImageAboveText,
						Tag = resourceImage,
						Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
					};

					SetCheckBoxDesign(rb);
					panelChooser.Controls.Add(rb);
					rb.AutoSize = true;
					var loc = new Point(0, yposition);
					var size = new Size(panelChooser.ClientSize.Width, rb.Height + resourceImage.Image.Size.Height + 5);
					panelChooser.PerformLayout();
					rb.AutoSize = false;
					rb.Location = loc;
					rb.Size = size;
					if (rb.Checked)
					{
						anySelected = true;
						SelectedResourceImage = resourceImage;
					}
					yposition += rb.Height + 10;

					Action<object, EventArgs> clickAction = (o, ev) =>
					{
						var rbi = o as CheckBox;
						if (rbi == null)
							return;

						foreach (var rbl in panelChooser.Controls.OfType<CheckBox>())
						{
							rbl.Checked = rbl == rbi;
							SetCheckBoxDesign(rbl);
						}
					};

					rb.CheckedChanged += (o, ev) =>
					{
						var cb = o as CheckBox;
						if (cb == null)
							return;
						if (cb.Checked && SelectedResourceImage != cb.Tag as ResourceImage)
							SelectedResourceImage = cb.Tag as ResourceImage;
						else if (!cb.Checked && SelectedResourceImage == cb.Tag as ResourceImage)
							SelectedResourceImage = null;
					};
					rb.Click += new EventHandler(clickAction);
					rb.DoubleClick += (o, ev) =>
					{
						btnOk.PerformClick();
					};
				}
				var llist = panelChooser.Controls.OfType<CheckBox>().ToList();
				var forceCheck = true;
				if (llist.Any() && !llist.Any(x => x.Checked))
				{
					if (HasPreferredSize && PreferredSizeVal.HasValue)
					{
						var preferredSize = PreferredSizeVal.Value;
						var cb = llist.FirstOrDefault(x => x.Tag is ResourceImage && ((ResourceImage)x.Tag).Size == preferredSize);
						if (cb != null)
						{
							cb.Checked = true;
							SetCheckBoxDesign(cb);
							forceCheck = false;
						}
					}
					if (forceCheck)
					{
						var first = llist.First();
						first.Checked = true;
						SetCheckBoxDesign(first);
					}
				}

			}
			finally
			{
				panelChooser.ResumeLayout(true);
			}
		}

		private void tabControl_SelectedIndexChanged(object sender, EventArgs e)
		{
			var tab = sender as TabControl;
			if (tab == null) return;
			if (tab.SelectedIndex == 0)
			{
				AcceptButton = btnOk;
				CancelButton = btnCancel;
			}
			else
			{
				AcceptButton = _resourcePickerDialog?.okButton;
				CancelButton = _resourcePickerDialog?.cancelButton;
			}
		}

		#endregion

		private static string ToSingleString(IEnumerable<string> array, string separator = null)
		{
			var enumerable = array as IList<string> ?? array.ToList();
			if (array == null || !enumerable.Any()) return string.Empty;

			if (separator == null)
				separator = Environment.NewLine;

			var builder = new StringBuilder();
			var ar = enumerable.ToList();
			for (var i = 0; i < ar.Count; i++)
			{
				builder.Append(ar[i]);
				if (i < ar.Count - 1)
					builder.Append(separator);
			}
			return builder.ToString();
		}

		private void teFilter_TextChanged(object sender, EventArgs e)
		{
			if (_teTimer == null)
				_teTimer = new System.Threading.Timer(FilterTimerCallback, null, SearchTimerTimeoutMilliseconds, Timeout.Infinite);
			else
				_teTimer.Change(SearchTimerTimeoutMilliseconds, Timeout.Infinite);
		}

		private void FilterTimerCallback(object state)
		{
			CreateListViewItemsForCurrentResource();
		}
	}
}
