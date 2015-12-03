using System;
using System.Drawing;
using System.Windows.Forms;
using AoeUtils;

namespace IsometricGuiTests {
	
	public sealed class PlayersBottomRegion : BottomRegion {
		
		public PlayersBottomRegion( MainForm form ) : base( form ) {
		}
		
		void CreateComboboxPair( string name, string text, int x, int y, int width,
		                        ref ComboBox combobox, ref Label label ) {
			label = new Label();
			label.Location = new Point( x, y );
			label.Name = name + "Label";
			label.Size = new Size( width, 20 );
			label.Text = text;
			label.TextAlign = ContentAlignment.MiddleLeft;
			form.Controls.Add( label );
			
			combobox = new ComboBox();
			combobox.Location = new Point( x, y + 20 );
			combobox.Name = name + "Combobox";
			combobox.Size = new Size( width, 21 );
			combobox.DropDownStyle = ComboBoxStyle.DropDownList;
			form.Controls.Add( combobox );
		}
		
		void CreateTextboxPair( string name, string text, int x, int y, int width,
		                       int maxTextLength, ref TextBox textbox, ref Label label ) {
			label = new Label();
			label.Location = new Point( x, y );
			label.Name = name + "Label";
			label.Size = new Size( width, 20 );
			label.Text = text;
			label.TextAlign = ContentAlignment.MiddleLeft;
			form.Controls.Add( label );
			
			textbox = new TextBox();
			textbox.Location = new Point( x, y + 20 );
			textbox.Name = name + "Combobox";
			textbox.Size = new Size( width, 20 );
			form.Controls.Add( textbox );
		}
		
		ComboBox CreateCombobox( string name, int x, int y, int width ) {
			ComboBox combobox = new ComboBox();
			combobox.Location = new Point( x, y );
			combobox.Name = name;
			combobox.Size = new Size( width, 21 );
			combobox.DropDownStyle = ComboBoxStyle.DropDownList;
			form.Controls.Add( combobox );
			return combobox;
		}
		
		ComboBox activePlayer;
		
		Label playersCountLabel, startingAgeLabel;
		ComboBox playersCount, startingAge;
		
		Label foodLabel, stoneLabel, popLimitLabel, woodLabel, goldLabel, oreXLabel;
		TextBox food, stone, popLimit, wood, gold, oreX;
		
		Label playerNameLabel;
		TextBox playerName;
		
		Label personalityLabel, playerTypeLabel, civilizationLabel;
		ComboBox personality, playerType, civilization;
		
		public override void Display() {
			activePlayer = CreateCombobox( "activePlayerCombobox", 20, 680, 120 );
			SetComboboxItems( activePlayer, "Gaia", "Player 1", "Player 2", "Player 3",
			                 "Player 4", "Player 5", "Player 6", "Player 7", "Player 8" );
			
			CreateComboboxPair( "playersCount", "Number of players", 20, 720, 120, ref playersCount, ref playersCountLabel );
			SetComboboxItems( playersCount, "1 player", "2 players", "3 players", "4 players",
			                 "5 players", "6 players", "7 players", "8 players" );
			
			CreateComboboxPair( "startingAge", "Starting age", 20, 780, 120, ref startingAge, ref startingAgeLabel );
			SetComboboxItems( startingAge, "Dark age", "Feudal age", "Castle age", "Imperial age",
			                 "Post-Imperial age" );
			
			const int resourcesTextLength = 10;
			CreateTextboxPair( "food", "Food", 180, 680, 80, resourcesTextLength, ref food, ref foodLabel );
			CreateTextboxPair( "wood", "Wood", 180, 730, 80, resourcesTextLength, ref wood, ref woodLabel );
			CreateTextboxPair( "stone", "Stone", 280, 680, 80, resourcesTextLength, ref stone, ref stoneLabel );
			CreateTextboxPair( "gold", "Gold", 280, 730, 80, resourcesTextLength, ref gold, ref goldLabel );
			CreateTextboxPair( "popLimit", "Pop limit", 380, 680, 80, resourcesTextLength, ref popLimit, ref popLimitLabel );
			CreateTextboxPair( "orex", "OreX", 380, 730, 80, resourcesTextLength, ref oreX, ref oreXLabel );
			
			CreateTextboxPair( "playerName", "Player name", 500, 660, 120, 256, ref playerName, ref playerNameLabel );
			CreateComboboxPair( "playerType", "Player type", 660, 660, 120, ref playerType, ref playerTypeLabel );
			SetComboboxItems( playerType, "Computer", "Either" );
			CreateComboboxPair( "civ", "Civilization", 660, 720, 120, ref civilization, ref civilizationLabel );
			// TODO: Fill in combo box items
			//SetComboboxItems( civilization, null );
			CreateComboboxPair( "personality", "Personality", 500, 720, 120, ref personality, ref personalityLabel );
			// TODO: Fill in combo box items with class AiPersonality { string Name; string Text; AiType Type; }
			
			activePlayer.SelectedIndexChanged += ActivePlayerChanged;
			activePlayer.SelectedIndex = 1; // Player 1
			playersCount.SelectedIndex = form.LoadedScenario.PlayersCount - 1;
		}

		void ActivePlayerChanged( object sender, EventArgs e ) {
			int index = activePlayer.SelectedIndex;
			if( index > -1 ) {
				bool isGaia = index == 0;
				if( isGaia ) {
					index = 8;
				} else {
					index--;
				}
				
				PlayerInfo info = form.LoadedScenario.PlayersInfo[index];
				if( !isGaia ) {
					string name = form.LoadedScenario.PlayerNames[index];
					playerName.Enabled = true;
					playerName.Text = name;
				} else {
					playerName.Text = "";
					playerName.Enabled = false;
				}
				
				food.Text = info.Food.ToString();
				gold.Text = info.Gold.ToString();
				wood.Text = info.Wood.ToString();
				stone.Text = info.Stone.ToString();
				//popLimit.Text = info
				oreX.Text = info.OreX.ToString();
				playerType.SelectedIndex = info.Human ? 0 : 1;
				startingAge.SelectedIndex = (int)info.Age;
				//civ
			}
		}
		
		static void SetComboboxItems<T>( ComboBox combobox, params T[] items ) where T : class {
			combobox.Items.AddRange( items );
		}
		
		public override void Dispose() {
			DisposeElements( activePlayer, playersCount, playersCountLabel, foodLabel, stoneLabel,
			                popLimitLabel, goldLabel, oreXLabel, food, stone, popLimit,
			                wood, gold, oreX, playerNameLabel, woodLabel, playerName,
			                personalityLabel, personality, playerTypeLabel, playerType,
			                civilizationLabel, civilization, startingAge, startingAgeLabel );
		}
	}
}
