using System;
using System.Drawing;
using System.IO;

namespace IsometricTests {

	public static class TerrainHelper {
		
		public static readonly string[] AokTerrainNames = new string[] {
			"Grass 1", "Shallow water", "Beach", "Dirt 3", "Shallows",
			"Leaves", "Dirt 1", "Farm", "Dead farm", "Grass 3", "Forest",
			"Dirt 2", "Grass 2", "Palm forest", "Desert", "Shoreless water",
			"Old grass", "Jungle", "Bamboo", "Pine forest", "Oak forest",
			"Snow pine forest", "Deep water", "Medium water", "Road",
			"Broken road", "Sailable ice", "Shoreless dirt 2", "Walkable water",
			"Unplanted farm", "Small planted farm", "Large planted farm",
			"Snow", "Snow dirt", "Snow grass", "Shoreless snow dirt",
			"Ice beach", "Snow road", "Fungus road", "KOH flag ground",
		};
		
		public static FastColour GetColour( byte terrainId ) {
			if( terrainId >= AokTerrainColours.Length ) return new FastColour( 0, 255, 0 );
			return AokTerrainColours[terrainId];
		}
		
		static RectangleF invalidTerrainTexRec;
		public static RectangleF GetTexRec( byte terrainId ) {
			if( terrainId >= AokTerrainLocations.Length ) return invalidTerrainTexRec;
			return AokTerrainLocations[terrainId];
		}
		
		static RectangleF[] AokTerrainLocations;
		
		public static readonly FastColour[] AokTerrainColours = new FastColour[] {
			new FastColour( 85, 119, 52 ),   // 0,  Grass 1
			new FastColour( 0, 139, 210 ),   // 1,  Shallow water
			new FastColour( 235, 202, 181 ), // 2,  Beach
			new FastColour( 228, 162, 82 ),  // 3,  Dirt 3,
			new FastColour( 0, 128, 128 ),     // 4,  Shallows
			new FastColour( 255, 80, 21 ),    // 5,  Leaves
			new FastColour( 196, 128, 88 ),  // 6,  Dirt 1
			new FastColour( 0, 255, 0 ),     // 7,  Farm
			new FastColour( 0, 255, 0 ),     // 8,  Dead farm
			new FastColour( 206, 187, 128 ), // 9,  Grass 3
			new FastColour( 0, 64, 0 ),     // 10, Forest
			new FastColour( 255, 201, 121 ), // 11, Dirt 2
			new FastColour( 165, 196, 108 ), // 12, Grass 2
			new FastColour( 0, 255, 0 ),     // 13, Palm forest
			new FastColour( 248, 201, 138 ), // 14, Desert
			new FastColour( 0, 90, 184 ),    // 15, Shoreless medium water
			new FastColour( 85, 119, 52 ),   // 16, Old grass
			new FastColour( 0, 64, 0 ),     // 17, Jungle
			new FastColour( 0, 64, 0 ),     // 18, Bamboo
			new FastColour( 0, 64, 0 ),     // 19, Pine forest
			new FastColour( 0, 64, 0 ),     // 20, Oak forest
			new FastColour( 0, 255, 0 ),     // 21, Snow pine forest
			new FastColour( 0, 53, 135 ),    // 22, Deep water
			new FastColour( 0, 90, 184 ),    // 23, Medium water
			new FastColour( 215, 186, 155 ), // 24, Road
			new FastColour( 167, 135, 102 ), // 25, Broken road
			new FastColour( 189, 209, 253 ), // 26, Sailable ice
			new FastColour( 255, 201, 121 ), // 27, Shoreless dirt 2
			new FastColour( 0, 90, 184 ),    // 28, Walkable water
			new FastColour( 0, 255, 0 ),     // 29, Farm unseeded
			new FastColour( 0, 255, 0 ),     // 30, Farm small plants
			new FastColour( 0, 255, 0 ),     // 31, Farm large plants
			new FastColour( 223, 234, 255 ), // 32, Snow
			new FastColour( 185, 185, 185 ), // 33, Snow dirt
			new FastColour( 156, 197, 217 ), // 34, Snow grass
			new FastColour( 189, 209, 253 ), // 35, Ice
			new FastColour( 185, 185, 185 ), // 36, Shoreless snow dirt
			new FastColour( 189, 209, 253 ), // 37, Ice beach	
			/*
38. Snow road
39. Fungus road
40. KOH flag ground ( like a road but you can't build on it )*/
		};
		
		const int xWidth = 97;
		const int yWidth = 49;
		public static int MakeTerrainTexture() {
			// 28 images
			// 4 x 7
			const int horElements = 28;
			const int verElements = 1;
			
			Console.WriteLine( "MAX:" + GLUtils.MaxTextureDimensions );
			
			Bitmap textureAtlas = new Bitmap( xWidth * horElements, yWidth * verElements );
			using( Graphics g = Graphics.FromImage( textureAtlas ) ) {
				//DrawPart( g, "5555.png", xWidth * 0, 0 );
				DrawPart( g, "grass1.gif", xWidth * 0, 0 );
				DrawPart( g, "watershallow.gif", xWidth * 1, 0 );
				DrawPart( g, "beach.gif", xWidth * 2, 0 );
				DrawPart( g, "dirt3.gif", xWidth * 3, 0 );
				
				DrawPart( g, "shallows.gif", xWidth * 4, 0 );
				DrawPart( g, "leaves.gif", xWidth * 5, 0 );
				DrawPart( g, "dirt1.gif", xWidth * 6, 0 );
				DrawPart( g, "farm.gif", xWidth * 7, 0 );
				
				DrawPart( g, "deadfarm.gif", xWidth * 8, 0 );
				DrawPart( g, "grass3.gif", xWidth * 9, 0 );
				DrawPart( g, "dirt2.gif", xWidth * 10, 0 );
				DrawPart( g, "grass2.gif", xWidth * 11, 0 );
				
				DrawPart( g, "desert.gif", xWidth * 12, 0 );
				DrawPart( g, "deepwater.gif", xWidth * 13, 0 );
				DrawPart( g, "mediumwater.gif", xWidth * 14, 0 );
				DrawPart( g, "road.gif", xWidth * 15, 0 );
				
				DrawPart( g, "roadbroken.gif", xWidth * 16, 0 );
				DrawPart( g, "newfarm.gif", xWidth * 17, 0 );
				DrawPart( g, "farmsmallplants.gif", xWidth * 18, 0 );
				DrawPart( g, "farmlargeplants.gif", xWidth * 19, 0 );
				
				DrawPart( g, "snow.gif", xWidth * 20, 0 );
				DrawPart( g, "snowdirt.gif", xWidth * 21, 0 );
				DrawPart( g, "snowgrass.gif", xWidth * 22, 0 );
				DrawPart( g, "ice.gif", xWidth * 23, 0 );
				
				DrawPart( g, "snowroad.gif", xWidth * 24, 0 );
				DrawPart( g, "roadfungus.gif", xWidth * 25, 0 );
				
				//DrawPart( g, "shallows.gif", 0, 49 );
				//DrawPart( g, "leaves.gif", 97, 49 );
				//DrawPart( g, "dirt1.gif", 194, 49 );
				//DrawPart( g, "farm.gif", 291, 49 );
			}
			
			Bitmap bmp = textureAtlas;
			/*for( int i = 0; i < bmp.Width; i++ ) {
				for( int j = 0; j < bmp.Height; j++ ) {
					if( bmp.GetPixel( i, j ) == Color.FromArgb( 255, 0, 0, 0 ) ) {
						Console.WriteLine( "GGGG" );
						bmp.SetPixel( i, j, Color.FromArgb( 0, 0, 0, 0 ) );
					}
				}
			}*/
			
			AokTerrainLocations = new RectangleF[41];
			//for( int i = 0; i < AokTerrainLocations.Length; i++ ) {
			//	AokTerrainLocations[i] = TextureCoords2d( 0, horElements, verElements );
			//}
			AokTerrainLocations[0] = TextureCoords2d( 0, horElements, verElements );
			AokTerrainLocations[1] = TextureCoords2d( 1, horElements, verElements );
			AokTerrainLocations[2] = TextureCoords2d( 2, horElements, verElements );
			AokTerrainLocations[3] = TextureCoords2d( 3, horElements, verElements );
			AokTerrainLocations[4] = TextureCoords2d( 4, horElements, verElements );
			AokTerrainLocations[5] = TextureCoords2d( 5, horElements, verElements );
			AokTerrainLocations[6] = TextureCoords2d( 6, horElements, verElements );
			AokTerrainLocations[7] = TextureCoords2d( 7, horElements, verElements );
			AokTerrainLocations[8] = TextureCoords2d( 8, horElements, verElements );
			AokTerrainLocations[9] = TextureCoords2d( 9, horElements, verElements );
			AokTerrainLocations[10] = TextureCoords2d( 5, horElements, verElements );
			AokTerrainLocations[11] = TextureCoords2d( 10, horElements, verElements );
			AokTerrainLocations[12] = TextureCoords2d( 11, horElements, verElements );
			AokTerrainLocations[13] = TextureCoords2d( 5, horElements, verElements );
			AokTerrainLocations[14] = TextureCoords2d( 12, horElements, verElements );
			AokTerrainLocations[15] = TextureCoords2d( 14, horElements, verElements );
			AokTerrainLocations[16] = TextureCoords2d( 0, horElements, verElements );
			AokTerrainLocations[17] = TextureCoords2d( 5, horElements, verElements );
			AokTerrainLocations[18] = TextureCoords2d( 5, horElements, verElements );
			AokTerrainLocations[19] = TextureCoords2d( 5, horElements, verElements );
			AokTerrainLocations[20] = TextureCoords2d( 5, horElements, verElements );
			AokTerrainLocations[21] = TextureCoords2d( 22, horElements, verElements );
			AokTerrainLocations[22] = TextureCoords2d( 13, horElements, verElements );
			AokTerrainLocations[23] = TextureCoords2d( 14, horElements, verElements );
			AokTerrainLocations[24] = TextureCoords2d( 15, horElements, verElements );
			AokTerrainLocations[25] = TextureCoords2d( 16, horElements, verElements );
			AokTerrainLocations[26] = TextureCoords2d( 23, horElements, verElements );
			AokTerrainLocations[27] = TextureCoords2d( 10, horElements, verElements );
			AokTerrainLocations[28] = TextureCoords2d( 14, horElements, verElements );
			AokTerrainLocations[29] = TextureCoords2d( 17, horElements, verElements );
			AokTerrainLocations[30] = TextureCoords2d( 18, horElements, verElements );
			AokTerrainLocations[31] = TextureCoords2d( 19, horElements, verElements );
			AokTerrainLocations[32] = TextureCoords2d( 20, horElements, verElements );
			AokTerrainLocations[33] = TextureCoords2d( 21, horElements, verElements );
			AokTerrainLocations[34] = TextureCoords2d( 22, horElements, verElements );
			AokTerrainLocations[35] = TextureCoords2d( 23, horElements, verElements );
			AokTerrainLocations[36] = TextureCoords2d( 21, horElements, verElements );
			AokTerrainLocations[37] = TextureCoords2d( 23, horElements, verElements );
			AokTerrainLocations[38] = TextureCoords2d( 24, horElements, verElements );
			AokTerrainLocations[39] = TextureCoords2d( 25, horElements, verElements );
			AokTerrainLocations[20] = TextureCoords2d( 15, horElements, verElements );
			invalidTerrainTexRec = TextureCoords2d( 27, horElements, verElements );
			
			//Bitmap bmp = new Bitmap( bmpWidth, bmpHeight );
			//using( Graphics g = Graphics.FromImage( bmp ) ) {
			//}
			return GLUtils.LoadTexture( textureAtlas );
		}
		
		static void DrawPart( Graphics g, string texture, int x, int y ) {
			using( Bitmap bmp = new Bitmap( Path.Combine( "textures", texture ) ) ) {
				g.DrawImage( bmp, x, y, xWidth, yWidth );
			}
		}
		
		static RectangleF TextureCoords2d( int textureId, int horTexturesPacked, int verTexturesPacked ) {
			RectangleF rec = new RectangleF();
			//rec.Y = 1f / verTexturesPacked * ( textureId % verTexturesPacked );
			//rec.Y = 1f / horTexturesPacked * (int)( textureId / horTexturesPacked );
			
			rec.X = 1f / horTexturesPacked * ( textureId % horTexturesPacked );
			rec.Width = 1f / horTexturesPacked;
			rec.Height = 1f / verTexturesPacked;
			//rec.Width = 1;
			//rec.Height = 1;
			return rec;
		}
	}
}
