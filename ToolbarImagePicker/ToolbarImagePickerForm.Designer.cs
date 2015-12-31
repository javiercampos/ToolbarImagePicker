namespace Jcl.Tools.WindowsForms.ToolbarImagePicker
{
	sealed partial class ToolbarImagePickerForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Jcl.Tools.WindowsForms.ToolbarImagePicker.DoubleBufferPanel pnlContent;
			Jcl.Tools.WindowsForms.ToolbarImagePicker.DoubleBufferPanel pnlTop;
			Jcl.Tools.WindowsForms.ToolbarImagePicker.DoubleBufferPanel pnlBottom;
			this.tabControlMain = new System.Windows.Forms.TabControl();
			this.tabToolbarPicker = new System.Windows.Forms.TabPage();
			this.lvItems = new Jcl.Tools.WindowsForms.ToolbarImagePicker.NonFlickeringListView();
			this.splitter1 = new System.Windows.Forms.Splitter();
			this.panelChooser = new Jcl.Tools.WindowsForms.ToolbarImagePicker.DoubleBufferPanel();
			this.cbSelectSize = new System.Windows.Forms.ComboBox();
			this.teFilter = new Jcl.Tools.WindowsForms.ToolbarImagePicker.CueTextBox();
			this.cbSelectResource = new System.Windows.Forms.ComboBox();
			this.cbAllowSelectSmallLarge = new System.Windows.Forms.CheckBox();
			this.btnCancel = new System.Windows.Forms.Button();
			this.btnOk = new System.Windows.Forms.Button();
			this.tabVisualStudioPicker = new System.Windows.Forms.TabPage();
			pnlContent = new Jcl.Tools.WindowsForms.ToolbarImagePicker.DoubleBufferPanel();
			pnlTop = new Jcl.Tools.WindowsForms.ToolbarImagePicker.DoubleBufferPanel();
			pnlBottom = new Jcl.Tools.WindowsForms.ToolbarImagePicker.DoubleBufferPanel();
			this.tabControlMain.SuspendLayout();
			this.tabToolbarPicker.SuspendLayout();
			pnlContent.SuspendLayout();
			pnlTop.SuspendLayout();
			pnlBottom.SuspendLayout();
			this.SuspendLayout();
			// 
			// tabControlMain
			// 
			this.tabControlMain.Controls.Add(this.tabToolbarPicker);
			this.tabControlMain.Controls.Add(this.tabVisualStudioPicker);
			this.tabControlMain.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tabControlMain.Location = new System.Drawing.Point(0, 0);
			this.tabControlMain.Name = "tabControlMain";
			this.tabControlMain.SelectedIndex = 0;
			this.tabControlMain.Size = new System.Drawing.Size(872, 440);
			this.tabControlMain.TabIndex = 0;
			this.tabControlMain.SelectedIndexChanged += new System.EventHandler(this.tabControl_SelectedIndexChanged);
			// 
			// tabToolbarPicker
			// 
			this.tabToolbarPicker.Controls.Add(pnlContent);
			this.tabToolbarPicker.Controls.Add(pnlTop);
			this.tabToolbarPicker.Controls.Add(pnlBottom);
			this.tabToolbarPicker.Location = new System.Drawing.Point(4, 24);
			this.tabToolbarPicker.Name = "tabToolbarPicker";
			this.tabToolbarPicker.Padding = new System.Windows.Forms.Padding(3);
			this.tabToolbarPicker.Size = new System.Drawing.Size(864, 412);
			this.tabToolbarPicker.TabIndex = 0;
			this.tabToolbarPicker.Text = "Toolbar Picker";
			this.tabToolbarPicker.UseVisualStyleBackColor = true;
			// 
			// pnlContent
			// 
			pnlContent.Controls.Add(this.lvItems);
			pnlContent.Controls.Add(this.splitter1);
			pnlContent.Controls.Add(this.panelChooser);
			pnlContent.Dock = System.Windows.Forms.DockStyle.Fill;
			pnlContent.Location = new System.Drawing.Point(3, 33);
			pnlContent.Name = "pnlContent";
			pnlContent.Size = new System.Drawing.Size(858, 332);
			pnlContent.TabIndex = 4;
			// 
			// lvItems
			// 
			this.lvItems.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lvItems.HideSelection = false;
			this.lvItems.Location = new System.Drawing.Point(0, 0);
			this.lvItems.MultiSelect = false;
			this.lvItems.Name = "lvItems";
			this.lvItems.Size = new System.Drawing.Size(679, 332);
			this.lvItems.TabIndex = 1;
			this.lvItems.UseCompatibleStateImageBehavior = false;
			this.lvItems.SelectedIndexChanged += new System.EventHandler(this.lvItems_SelectedIndexChanged);
			this.lvItems.DoubleClick += new System.EventHandler(this.lvItems_DoubleClick);
			// 
			// splitter1
			// 
			this.splitter1.Dock = System.Windows.Forms.DockStyle.Right;
			this.splitter1.Location = new System.Drawing.Point(679, 0);
			this.splitter1.Name = "splitter1";
			this.splitter1.Size = new System.Drawing.Size(3, 332);
			this.splitter1.TabIndex = 0;
			this.splitter1.TabStop = false;
			// 
			// panelChooser
			// 
			this.panelChooser.Dock = System.Windows.Forms.DockStyle.Right;
			this.panelChooser.Location = new System.Drawing.Point(682, 0);
			this.panelChooser.Name = "panelChooser";
			this.panelChooser.Padding = new System.Windows.Forms.Padding(5);
			this.panelChooser.Size = new System.Drawing.Size(176, 332);
			this.panelChooser.TabIndex = 3;
			// 
			// pnlTop
			// 
			pnlTop.Controls.Add(this.cbSelectSize);
			pnlTop.Controls.Add(this.teFilter);
			pnlTop.Controls.Add(this.cbSelectResource);
			pnlTop.Dock = System.Windows.Forms.DockStyle.Top;
			pnlTop.Location = new System.Drawing.Point(3, 3);
			pnlTop.Name = "pnlTop";
			pnlTop.Size = new System.Drawing.Size(858, 30);
			pnlTop.TabIndex = 2;
			// 
			// cbSelectSize
			// 
			this.cbSelectSize.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cbSelectSize.FormattingEnabled = true;
			this.cbSelectSize.Items.AddRange(new object[] {
            "16x16",
            "32x32"});
			this.cbSelectSize.Location = new System.Drawing.Point(207, 3);
			this.cbSelectSize.Name = "cbSelectSize";
			this.cbSelectSize.Size = new System.Drawing.Size(111, 23);
			this.cbSelectSize.TabIndex = 7;
			this.cbSelectSize.SelectedIndexChanged += new System.EventHandler(this.cbSelectSize_SelectedIndexChanged);
			// 
			// teFilter
			// 
			this.teFilter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.teFilter.Cue = "Search...";
			this.teFilter.Location = new System.Drawing.Point(322, 3);
			this.teFilter.Name = "teFilter";
			this.teFilter.Size = new System.Drawing.Size(531, 23);
			this.teFilter.TabIndex = 6;
			this.teFilter.TextChanged += new System.EventHandler(this.teFilter_TextChanged);
			// 
			// cbSelectResource
			// 
			this.cbSelectResource.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cbSelectResource.FormattingEnabled = true;
			this.cbSelectResource.Location = new System.Drawing.Point(3, 3);
			this.cbSelectResource.Name = "cbSelectResource";
			this.cbSelectResource.Size = new System.Drawing.Size(198, 23);
			this.cbSelectResource.TabIndex = 2;
			// 
			// pnlBottom
			// 
			pnlBottom.Controls.Add(this.cbAllowSelectSmallLarge);
			pnlBottom.Controls.Add(this.btnCancel);
			pnlBottom.Controls.Add(this.btnOk);
			pnlBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
			pnlBottom.Location = new System.Drawing.Point(3, 365);
			pnlBottom.Name = "pnlBottom";
			pnlBottom.Size = new System.Drawing.Size(858, 44);
			pnlBottom.TabIndex = 3;
			// 
			// cbAllowSelectSmallLarge
			// 
			this.cbAllowSelectSmallLarge.AutoSize = true;
			this.cbAllowSelectSmallLarge.Location = new System.Drawing.Point(5, 16);
			this.cbAllowSelectSmallLarge.Name = "cbAllowSelectSmallLarge";
			this.cbAllowSelectSmallLarge.Size = new System.Drawing.Size(275, 19);
			this.cbAllowSelectSmallLarge.TabIndex = 9;
			this.cbAllowSelectSmallLarge.Text = "Set SmallImage/LargeImage to 16x16 and 32x32";
			this.cbAllowSelectSmallLarge.UseVisualStyleBackColor = true;
			// 
			// btnCancel
			// 
			this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.Location = new System.Drawing.Point(744, 10);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size(109, 28);
			this.btnCancel.TabIndex = 8;
			this.btnCancel.Text = "Cancel";
			this.btnCancel.UseVisualStyleBackColor = true;
			// 
			// btnOk
			// 
			this.btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnOk.Location = new System.Drawing.Point(625, 10);
			this.btnOk.Name = "btnOk";
			this.btnOk.Size = new System.Drawing.Size(113, 28);
			this.btnOk.TabIndex = 7;
			this.btnOk.Text = "OK";
			this.btnOk.UseVisualStyleBackColor = true;
			this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
			// 
			// tabVisualStudioPicker
			// 
			this.tabVisualStudioPicker.Location = new System.Drawing.Point(4, 24);
			this.tabVisualStudioPicker.Name = "tabVisualStudioPicker";
			this.tabVisualStudioPicker.Padding = new System.Windows.Forms.Padding(3);
			this.tabVisualStudioPicker.Size = new System.Drawing.Size(864, 412);
			this.tabVisualStudioPicker.TabIndex = 1;
			this.tabVisualStudioPicker.Text = "Visual Studio Resource Picker";
			this.tabVisualStudioPicker.UseVisualStyleBackColor = true;
			// 
			// ToolbarImagePickerForm
			// 
			this.AcceptButton = this.btnOk;
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.btnCancel;
			this.ClientSize = new System.Drawing.Size(872, 440);
			this.Controls.Add(this.tabControlMain);
			this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Name = "ToolbarImagePickerForm";
			this.Text = "Select Image Resource";
			this.tabControlMain.ResumeLayout(false);
			this.tabToolbarPicker.ResumeLayout(false);
			pnlContent.ResumeLayout(false);
			pnlTop.ResumeLayout(false);
			pnlTop.PerformLayout();
			pnlBottom.ResumeLayout(false);
			pnlBottom.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TabPage tabToolbarPicker;
		private System.Windows.Forms.TabPage tabVisualStudioPicker;
		private NonFlickeringListView lvItems;
		private System.Windows.Forms.ComboBox cbSelectResource;
		private Jcl.Tools.WindowsForms.ToolbarImagePicker.DoubleBufferPanel panelChooser;
		private System.Windows.Forms.TabControl tabControlMain;
		private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.Button btnOk;
		private CueTextBox teFilter;
		private System.Windows.Forms.ComboBox cbSelectSize;
		private System.Windows.Forms.CheckBox cbAllowSelectSmallLarge;
		private System.Windows.Forms.Splitter splitter1;
	}
}