using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using AoeUtils;

namespace CampaignEditor {

	public partial class MainForm : Form {
		
		public MainForm() {
			InitializeComponent();
			scenariosTable.AllowUserToAddRows = false;
			ResizeEnd += ResizeEnded;
			
			ToolTip nameToolTip = new ToolTip();
			nameToolTip.SetToolTip( nameTextbox, "Displayed campaign name" );
			
			ToolTip nameToolTip2 = new ToolTip();
			nameToolTip2.SetToolTip( nameLabel, "Displayed campaign name" );
		}
		
		Campaign campaign;
		string file = "";
		
		void LoadButtonClick( object sender, EventArgs e ) {
			using( OpenFileDialog dialog = new OpenFileDialog() ) {
				dialog.CheckPathExists = true;
				dialog.CheckFileExists = true;
				dialog.Filter = "AoE II campaigns (*.cpn, *.cpx)|*.cpn;*.cpx";
				dialog.Title = "Load campaign..";
				DialogResult result = dialog.ShowDialog();
				
				if( result == DialogResult.OK ) {
					file = dialog.FileName;
					using( FileStream fs = File.OpenRead( dialog.FileName ) ) {
						campaign = Campaign.FromStream( fs );
						nameTextbox.Text = campaign.CampaignName;
						scenariosTable.Rows.Clear();
						List<CampaignScenario> scenarios = campaign.Scenarios;
						scenariosTable.Rows.Add( scenarios.Count );
						
						for( int i = 0; i < scenarios.Count; i++ ) {
							DataGridViewCell cell = scenariosTable.Rows[i].Cells[0];
							cell.Value = scenarios[i].Name;
						}
					}
				}
			}
		}
		
		void MoveUpButtonClick( object sender, EventArgs e ) {
			DataGridViewRow currentRow = scenariosTable.CurrentRow;
			if( currentRow == null ) return; // No row currently selected.
			
			int currentIndex = scenariosTable.CurrentRow.Index;
			if( currentIndex == 0 ) return; // Avoid pointlessly updating the data.
			int newIndex = currentIndex - 1;
			
			scenariosTable.Rows.RemoveAt( currentIndex );
			scenariosTable.Rows.Insert( newIndex, currentRow );
			scenariosTable.CurrentCell = currentRow.Cells[0];
			
			List<CampaignScenario> scenarios = campaign.Scenarios;
			CampaignScenario currentScenario = campaign.Scenarios[currentIndex];
			scenarios.RemoveAt( currentIndex );
			scenarios.Insert( newIndex, currentScenario );
		}
		
		void MoveDownButtonClick( object sender, EventArgs e ) {
			DataGridViewRow currentRow = scenariosTable.CurrentRow;
			if( currentRow == null ) return;
			
			int currentIndex = scenariosTable.CurrentRow.Index;
			int count = scenariosTable.Rows.Count;
			if( currentIndex == count - 1 ) return;
			int newIndex = currentIndex + 1;
			
			scenariosTable.Rows.RemoveAt( currentIndex );
			scenariosTable.Rows.Insert( newIndex, currentRow );
			scenariosTable.CurrentCell = currentRow.Cells[0];
			
			List<CampaignScenario> scenarios = campaign.Scenarios;
			CampaignScenario currentScenario = campaign.Scenarios[currentIndex];
			scenarios.RemoveAt( currentIndex );
			scenarios.Insert( newIndex, currentScenario );
		}
		
		void SaveButtonClick( object sender, EventArgs e ) {
			using( SaveFileDialog dialog = new SaveFileDialog() ) {
				dialog.Filter = "AoE II campaigns (*.cpn, *.cpx)|*.cpn;*.cpx";
				dialog.Title = "Save campaign..";
				dialog.FileName = file;
				DialogResult result = dialog.ShowDialog();
				
				if( result == DialogResult.OK ) {
					using( FileStream fs = File.Create( dialog.FileName ) ) {
						campaign.WriteToStream( fs );
					}
				}
			}
		}
		
		void ResizeEnded( object sender, EventArgs e ) {
			scenariosTable.Width = Width;
		}
		
		void DeleteButtonClick( object sender, EventArgs e ) {
			DataGridViewRow currentRow = scenariosTable.CurrentRow;
			if( currentRow == null ) return;
			
			int currentIndex = scenariosTable.CurrentRow.Index;
			scenariosTable.Rows.RemoveAt( currentIndex );
			campaign.Scenarios.RemoveAt( currentIndex );
		}
		
		void NameTextboxTextChanged( object sender, EventArgs e ) {
			try {
				campaign.CampaignName = nameTextbox.Text;
			} catch( ArgumentOutOfRangeException ) {
				MessageBox.Show( "Campaign name is too long." );
			}
		}
		
		void AddButtonClick( object sender, EventArgs e ) {
			using( OpenFileDialog dialog = new OpenFileDialog() ) {
				dialog.CheckPathExists = true;
				dialog.CheckFileExists = true;
				dialog.Filter = "AoE II scenarios (*.scn, *.scx)|*.scn;*.scx";
				dialog.Title = "Add scenario..";
				DialogResult result = dialog.ShowDialog();
				
				if( result == DialogResult.OK ) {
					string scenarioName = Path.GetFileNameWithoutExtension( dialog.FileName );
					string scenarioFilename = Path.GetFileName( dialog.FileName );
					using( FileStream fs = File.OpenRead( dialog.FileName ) ) {
						int length = (int)fs.Length;
						byte[] data = new byte[length];
						fs.Read( data, 0, data.Length );
						
						CampaignScenario scenario = new CampaignScenario();
						scenario.Data = data;
						scenario.Name = scenarioName;
						scenario.Filename = scenarioFilename;
						campaign.Scenarios.Add( scenario );
						int index = scenariosTable.Rows.Add();
						scenariosTable.Rows[index].Cells[0].Value = scenarioName;
					}
				}
			}
		}
		
		void OverwriteHandler( ref bool applyToAll, ref bool overwrite, string path, byte[] data ) {
			if( applyToAll ) {
				if( overwrite ) {
					File.WriteAllBytes( path, data );
				}
			} else {
				DialogResult overwriteResult = MessageBox.Show( "File already exists. Overwrite?", "File already exists",
				                                       MessageBoxButtons.YesNo );
				overwrite = overwriteResult == DialogResult.Yes;
				
				DialogResult applyResult = MessageBox.Show( "Do you want to apply this setting to all future file conflicts for this extraction?", "File already exists",
				                                       MessageBoxButtons.YesNo );
				applyToAll = applyResult == DialogResult.Yes;
				if( overwrite ) {
					File.WriteAllBytes( path, data );
				}
			}
		}
		
		void ExtractAllButtonClick( object sender, EventArgs e ) {
			using( FolderBrowserDialog dialog = new FolderBrowserDialog() ) {
				dialog.Description = "Select output directory..";
				
				dialog.SelectedPath = Path.GetDirectoryName( file ) ?? "";
				DialogResult result = dialog.ShowDialog();
				bool applyToAll = false;
				bool overwrite = false;
				
				if( result == DialogResult.OK ) {
					string directory = dialog.SelectedPath;
					foreach( CampaignScenario scenario in campaign.Scenarios ) {
						string path = Path.Combine( directory, scenario.Filename );
						try {
							using( FileStream fs = new FileStream( path, FileMode.CreateNew, FileAccess.Write ) ) {
								fs.Write( scenario.Data, 0, scenario.Data.Length );
							}
						} catch( IOException ) {
							if( File.Exists( path ) ) {
								OverwriteHandler( ref applyToAll, ref overwrite, path, scenario.Data );
							} else {
								throw;
							}
						}
					}
				}
			}
		}
	}
}