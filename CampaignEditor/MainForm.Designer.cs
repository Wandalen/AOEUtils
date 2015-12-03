
namespace CampaignEditor
{
	partial class MainForm
	{
		/// <summary>
		/// Designer variable used to keep track of non-visual components.
		/// </summary>
		private System.ComponentModel.IContainer components = null;
		
		/// <summary>
		/// Disposes resources used by the form.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing) {
				if (components != null) {
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}
		
		/// <summary>
		/// This method is required for Windows Forms designer support.
		/// Do not change the method contents inside the source code editor. The Forms designer might
		/// not be able to load this method if it was changed manually.
		/// </summary>
		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
			this.scenariosTable = new System.Windows.Forms.DataGridView();
			this.NameCol = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.Column2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.nameLabel = new System.Windows.Forms.Label();
			this.nameTextbox = new System.Windows.Forms.TextBox();
			this.menuBar = new System.Windows.Forms.ToolStrip();
			this.loadButton = new System.Windows.Forms.ToolStripButton();
			this.saveButton = new System.Windows.Forms.ToolStripButton();
			this.extractAllButton = new System.Windows.Forms.ToolStripButton();
			this.separator1 = new System.Windows.Forms.ToolStripSeparator();
			this.moveUpButton = new System.Windows.Forms.ToolStripButton();
			this.moveDownButton = new System.Windows.Forms.ToolStripButton();
			this.deleteButton = new System.Windows.Forms.ToolStripButton();
			this.addButton = new System.Windows.Forms.ToolStripButton();
			((System.ComponentModel.ISupportInitialize)(this.scenariosTable)).BeginInit();
			this.menuBar.SuspendLayout();
			this.SuspendLayout();
			// 
			// scenariosTable
			// 
			this.scenariosTable.AllowUserToDeleteRows = false;
			this.scenariosTable.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
			this.scenariosTable.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllHeaders;
			this.scenariosTable.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.scenariosTable.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
									this.NameCol,
									this.Column2});
			this.scenariosTable.Location = new System.Drawing.Point(0, 54);
			this.scenariosTable.Name = "scenariosTable";
			this.scenariosTable.Size = new System.Drawing.Size(252, 217);
			this.scenariosTable.TabIndex = 0;
			// 
			// NameCol
			// 
			this.NameCol.HeaderText = "Name";
			this.NameCol.Name = "NameCol";
			// 
			// Column2
			// 
			this.Column2.HeaderText = "Scenarios";
			this.Column2.Name = "Column2";
			this.Column2.ReadOnly = true;
			// 
			// nameLabel
			// 
			this.nameLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.nameLabel.Location = new System.Drawing.Point(3, 24);
			this.nameLabel.Name = "nameLabel";
			this.nameLabel.Size = new System.Drawing.Size(89, 20);
			this.nameLabel.TabIndex = 1;
			this.nameLabel.Text = "Campaign Name:";
			this.nameLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// nameTextbox
			// 
			this.nameTextbox.Location = new System.Drawing.Point(98, 24);
			this.nameTextbox.Name = "nameTextbox";
			this.nameTextbox.Size = new System.Drawing.Size(146, 20);
			this.nameTextbox.TabIndex = 2;
			this.nameTextbox.TextChanged += new System.EventHandler(this.NameTextboxTextChanged);
			// 
			// menuBar
			// 
			this.menuBar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
									this.loadButton,
									this.saveButton,
									this.extractAllButton,
									this.separator1,
									this.moveUpButton,
									this.moveDownButton,
									this.deleteButton,
									this.addButton});
			this.menuBar.Location = new System.Drawing.Point(0, 0);
			this.menuBar.Name = "menuBar";
			this.menuBar.Size = new System.Drawing.Size(256, 25);
			this.menuBar.TabIndex = 3;
			this.menuBar.Text = "toolStrip1";
			// 
			// loadButton
			// 
			this.loadButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.loadButton.Image = ((System.Drawing.Image)(resources.GetObject("loadButton.Image")));
			this.loadButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.loadButton.Name = "loadButton";
			this.loadButton.Size = new System.Drawing.Size(23, 22);
			this.loadButton.Text = "Load";
			this.loadButton.ToolTipText = "Load campaign";
			this.loadButton.Click += new System.EventHandler(this.LoadButtonClick);
			// 
			// saveButton
			// 
			this.saveButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.saveButton.Image = ((System.Drawing.Image)(resources.GetObject("saveButton.Image")));
			this.saveButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.saveButton.Name = "saveButton";
			this.saveButton.Size = new System.Drawing.Size(23, 22);
			this.saveButton.Text = "Save";
			this.saveButton.ToolTipText = "Save campaign";
			this.saveButton.Click += new System.EventHandler(this.SaveButtonClick);
			// 
			// extractAllButton
			// 
			this.extractAllButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.extractAllButton.Image = ((System.Drawing.Image)(resources.GetObject("extractAllButton.Image")));
			this.extractAllButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.extractAllButton.Name = "extractAllButton";
			this.extractAllButton.Size = new System.Drawing.Size(23, 22);
			this.extractAllButton.Text = "Extract all";
			this.extractAllButton.ToolTipText = "Extract all scenarios";
			this.extractAllButton.Click += new System.EventHandler(this.ExtractAllButtonClick);
			// 
			// separator1
			// 
			this.separator1.Name = "separator1";
			this.separator1.Size = new System.Drawing.Size(6, 25);
			// 
			// moveUpButton
			// 
			this.moveUpButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.moveUpButton.Image = ((System.Drawing.Image)(resources.GetObject("moveUpButton.Image")));
			this.moveUpButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.moveUpButton.Name = "moveUpButton";
			this.moveUpButton.Size = new System.Drawing.Size(23, 22);
			this.moveUpButton.Text = "Move up";
			this.moveUpButton.ToolTipText = "Move selected scenario up";
			this.moveUpButton.Click += new System.EventHandler(this.MoveUpButtonClick);
			// 
			// moveDownButton
			// 
			this.moveDownButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.moveDownButton.Image = ((System.Drawing.Image)(resources.GetObject("moveDownButton.Image")));
			this.moveDownButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.moveDownButton.Name = "moveDownButton";
			this.moveDownButton.Size = new System.Drawing.Size(23, 22);
			this.moveDownButton.Text = "Move down";
			this.moveDownButton.ToolTipText = "Move selected scenario down";
			this.moveDownButton.Click += new System.EventHandler(this.MoveDownButtonClick);
			// 
			// deleteButton
			// 
			this.deleteButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.deleteButton.Image = ((System.Drawing.Image)(resources.GetObject("deleteButton.Image")));
			this.deleteButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.deleteButton.Name = "deleteButton";
			this.deleteButton.Size = new System.Drawing.Size(23, 22);
			this.deleteButton.Text = "Delete";
			this.deleteButton.ToolTipText = "Remove the selected scenario";
			this.deleteButton.Click += new System.EventHandler(this.DeleteButtonClick);
			// 
			// addButton
			// 
			this.addButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.addButton.Image = ((System.Drawing.Image)(resources.GetObject("addButton.Image")));
			this.addButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.addButton.Name = "addButton";
			this.addButton.Size = new System.Drawing.Size(23, 22);
			this.addButton.Text = "Add";
			this.addButton.ToolTipText = "Add scenario";
			this.addButton.Click += new System.EventHandler(this.AddButtonClick);
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(256, 273);
			this.Controls.Add(this.menuBar);
			this.Controls.Add(this.nameTextbox);
			this.Controls.Add(this.nameLabel);
			this.Controls.Add(this.scenariosTable);
			this.Name = "MainForm";
			this.Text = "CampaignEditor";
			((System.ComponentModel.ISupportInitialize)(this.scenariosTable)).EndInit();
			this.menuBar.ResumeLayout(false);
			this.menuBar.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
		private System.Windows.Forms.ToolStripButton addButton;
		private System.Windows.Forms.ToolStripButton deleteButton;
		
		private System.Windows.Forms.ToolStripSeparator separator1;
		private System.Windows.Forms.ToolStripButton saveButton;
		private System.Windows.Forms.ToolStripButton moveDownButton;
		private System.Windows.Forms.ToolStripButton moveUpButton;
		private System.Windows.Forms.DataGridViewTextBoxColumn NameCol;
		private System.Windows.Forms.ToolStripButton extractAllButton;
		private System.Windows.Forms.ToolStripButton loadButton;
		private System.Windows.Forms.ToolStrip menuBar;
		private System.Windows.Forms.TextBox nameTextbox;
		private System.Windows.Forms.Label nameLabel;
		private System.Windows.Forms.DataGridViewTextBoxColumn Column2;
		private System.Windows.Forms.DataGridView scenariosTable;
	}
}
