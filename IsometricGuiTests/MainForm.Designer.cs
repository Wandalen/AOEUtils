
namespace IsometricGuiTests
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
			this.mapButton = new System.Windows.Forms.Button();
			this.terrainButton = new System.Windows.Forms.Button();
			this.victoryButton = new System.Windows.Forms.Button();
			this.optionsButton = new System.Windows.Forms.Button();
			this.playersButton = new System.Windows.Forms.Button();
			this.unitsButton = new System.Windows.Forms.Button();
			this.messagesButton = new System.Windows.Forms.Button();
			this.triggersButton = new System.Windows.Forms.Button();
			this.diplomacyButton = new System.Windows.Forms.Button();
			this.cinematicsButton = new System.Windows.Forms.Button();
			this.glControl1 = new OpenTK.GLControl();
			this.saveButton = new System.Windows.Forms.Button();
			this.loadButton = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// mapButton
			// 
			this.mapButton.Location = new System.Drawing.Point(0, 0);
			this.mapButton.Name = "mapButton";
			this.mapButton.Size = new System.Drawing.Size(80, 30);
			this.mapButton.TabIndex = 0;
			this.mapButton.Text = "Map";
			this.mapButton.UseVisualStyleBackColor = true;
			this.mapButton.Click += new System.EventHandler(this.MapButtonClick);
			// 
			// terrainButton
			// 
			this.terrainButton.Location = new System.Drawing.Point(80, 0);
			this.terrainButton.Name = "terrainButton";
			this.terrainButton.Size = new System.Drawing.Size(80, 30);
			this.terrainButton.TabIndex = 1;
			this.terrainButton.Text = "Terrain";
			this.terrainButton.UseVisualStyleBackColor = true;
			this.terrainButton.Click += new System.EventHandler(this.TerrainButtonClick);
			// 
			// victoryButton
			// 
			this.victoryButton.Location = new System.Drawing.Point(0, 30);
			this.victoryButton.Name = "victoryButton";
			this.victoryButton.Size = new System.Drawing.Size(80, 30);
			this.victoryButton.TabIndex = 2;
			this.victoryButton.Text = "Global victory";
			this.victoryButton.UseVisualStyleBackColor = true;
			this.victoryButton.Click += new System.EventHandler(this.VictoryButtonClick);
			// 
			// optionsButton
			// 
			this.optionsButton.Location = new System.Drawing.Point(80, 30);
			this.optionsButton.Name = "optionsButton";
			this.optionsButton.Size = new System.Drawing.Size(80, 30);
			this.optionsButton.TabIndex = 3;
			this.optionsButton.Text = "Options";
			this.optionsButton.UseVisualStyleBackColor = true;
			this.optionsButton.Click += new System.EventHandler(this.OptionsButtonClick);
			// 
			// playersButton
			// 
			this.playersButton.Location = new System.Drawing.Point(160, 0);
			this.playersButton.Name = "playersButton";
			this.playersButton.Size = new System.Drawing.Size(80, 30);
			this.playersButton.TabIndex = 4;
			this.playersButton.Text = "Players";
			this.playersButton.UseVisualStyleBackColor = true;
			this.playersButton.Click += new System.EventHandler(this.PlayersButtonClick);
			// 
			// unitsButton
			// 
			this.unitsButton.Location = new System.Drawing.Point(240, 0);
			this.unitsButton.Name = "unitsButton";
			this.unitsButton.Size = new System.Drawing.Size(80, 30);
			this.unitsButton.TabIndex = 5;
			this.unitsButton.Text = "Units";
			this.unitsButton.UseVisualStyleBackColor = true;
			this.unitsButton.Click += new System.EventHandler(this.UnitsButtonClick);
			// 
			// messagesButton
			// 
			this.messagesButton.Location = new System.Drawing.Point(160, 30);
			this.messagesButton.Name = "messagesButton";
			this.messagesButton.Size = new System.Drawing.Size(80, 30);
			this.messagesButton.TabIndex = 6;
			this.messagesButton.Text = "Messages";
			this.messagesButton.UseVisualStyleBackColor = true;
			this.messagesButton.Click += new System.EventHandler(this.MessagesButtonClick);
			// 
			// triggersButton
			// 
			this.triggersButton.Location = new System.Drawing.Point(320, 30);
			this.triggersButton.Name = "triggersButton";
			this.triggersButton.Size = new System.Drawing.Size(80, 30);
			this.triggersButton.TabIndex = 7;
			this.triggersButton.Text = "Triggers";
			this.triggersButton.UseVisualStyleBackColor = true;
			this.triggersButton.Click += new System.EventHandler(this.TriggersButtonClick);
			// 
			// diplomacyButton
			// 
			this.diplomacyButton.Location = new System.Drawing.Point(320, 0);
			this.diplomacyButton.Name = "diplomacyButton";
			this.diplomacyButton.Size = new System.Drawing.Size(80, 30);
			this.diplomacyButton.TabIndex = 8;
			this.diplomacyButton.Text = "Diplomacy";
			this.diplomacyButton.UseVisualStyleBackColor = true;
			this.diplomacyButton.Click += new System.EventHandler(this.DiplomacyButtonClick);
			// 
			// cinematicsButton
			// 
			this.cinematicsButton.Location = new System.Drawing.Point(240, 30);
			this.cinematicsButton.Name = "cinematicsButton";
			this.cinematicsButton.Size = new System.Drawing.Size(80, 30);
			this.cinematicsButton.TabIndex = 9;
			this.cinematicsButton.Text = "Cinematics";
			this.cinematicsButton.UseVisualStyleBackColor = true;
			this.cinematicsButton.Click += new System.EventHandler(this.CinematicsButtonClick);
			// 
			// glControl1
			// 
			this.glControl1.BackColor = System.Drawing.Color.Black;
			this.glControl1.Location = new System.Drawing.Point(0, 60);
			this.glControl1.Name = "glControl1";
			this.glControl1.Size = new System.Drawing.Size(800, 600);
			this.glControl1.TabIndex = 10;
			this.glControl1.VSync = true;
			this.glControl1.Load += new System.EventHandler(this.GlControl1Load);
			this.glControl1.Paint += new System.Windows.Forms.PaintEventHandler(this.GlControl1Paint);
			this.glControl1.Resize += new System.EventHandler(this.GlControl1Resize);
			// 
			// saveButton
			// 
			this.saveButton.Location = new System.Drawing.Point(690, 5);
			this.saveButton.Name = "saveButton";
			this.saveButton.Size = new System.Drawing.Size(100, 20);
			this.saveButton.TabIndex = 11;
			this.saveButton.Text = "Save scenario";
			this.saveButton.UseVisualStyleBackColor = true;
			this.saveButton.Click += new System.EventHandler(this.SaveButtonClick);
			// 
			// loadButton
			// 
			this.loadButton.Location = new System.Drawing.Point(690, 35);
			this.loadButton.Name = "loadButton";
			this.loadButton.Size = new System.Drawing.Size(100, 20);
			this.loadButton.TabIndex = 12;
			this.loadButton.Text = "Load scenario";
			this.loadButton.UseVisualStyleBackColor = true;
			this.loadButton.Click += new System.EventHandler(this.LoadButtonClick);
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(792, 833);
			this.Controls.Add(this.loadButton);
			this.Controls.Add(this.saveButton);
			this.Controls.Add(this.glControl1);
			this.Controls.Add(this.cinematicsButton);
			this.Controls.Add(this.diplomacyButton);
			this.Controls.Add(this.triggersButton);
			this.Controls.Add(this.messagesButton);
			this.Controls.Add(this.unitsButton);
			this.Controls.Add(this.playersButton);
			this.Controls.Add(this.optionsButton);
			this.Controls.Add(this.victoryButton);
			this.Controls.Add(this.terrainButton);
			this.Controls.Add(this.mapButton);
			this.Name = "MainForm";
			this.Text = "IsometricGuiTests";
			this.ResumeLayout(false);
		}
		private System.Windows.Forms.Button loadButton;
		private System.Windows.Forms.Button saveButton;
		private OpenTK.GLControl glControl1;
		private System.Windows.Forms.Button cinematicsButton;
		private System.Windows.Forms.Button diplomacyButton;
		private System.Windows.Forms.Button triggersButton;
		private System.Windows.Forms.Button messagesButton;
		private System.Windows.Forms.Button unitsButton;
		private System.Windows.Forms.Button playersButton;
		private System.Windows.Forms.Button optionsButton;
		private System.Windows.Forms.Button victoryButton;
		private System.Windows.Forms.Button terrainButton;
		private System.Windows.Forms.Button mapButton;
	}
}
