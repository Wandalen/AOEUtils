﻿using System;
using System.Diagnostics;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace IsometricTests.Renderers {
	
	public class TileRendererLinesGLVbo : TileRenderer {
		
		const float tileWidth = 1;
		const float tileHeight = 1;
		
		float X, Y;
		float height;
		float z1, z2;
		
		public override void Start( int terrainTexture ) {
			GL.Color3( 0f, 1f, 0f );
			GL.PolygonMode( MaterialFace.FrontAndBack, PolygonMode.Line );
		}
		
		int triangles;
		int vboId = -1;
		
		public override void RenderTiles( TerrainMap map, ref int totalTriangles ) {
			if( vboId == -1 ) {
				vboId = GLUtils.GenBuffer();
				var sw = Stopwatch.StartNew();
				Vector3[] vertices = BuildTerrain( map, ref triangles );
				long elapsed = sw.ElapsedMilliseconds;
				sw.Stop();
				Console.WriteLine( "Took " + elapsed + " ms to generate the map." );
				int size = vertices.Length * 12;
				GL.Arb.BindBuffer( BufferTargetArb.ArrayBuffer, vboId );
				GL.Arb.BufferData( BufferTargetArb.ArrayBuffer, new IntPtr( size ), vertices, BufferUsageArb.StaticDraw );
				GL.Arb.BindBuffer( BufferTargetArb.ArrayBuffer, 0 );
			}
			
			GL.EnableClientState( ArrayCap.VertexArray );
			GL.Arb.BindBuffer( BufferTargetArb.ArrayBuffer, vboId );
			GL.VertexPointer( 3, VertexPointerType.Float, 12, new IntPtr( 0 ) );
			GL.DrawArrays( BeginMode.Triangles, 0, triangles * 3 );
			GL.Arb.BindBuffer( BufferTargetArb.ArrayBuffer, 0 );
			GL.DisableClientState( ArrayCap.VertexArray );
			totalTriangles = triangles;
		}
		
		Vector3[] BuildTerrain( TerrainMap map, ref int totalTriangles ) {
			TileDrawInfo drawInfo = new TileDrawInfo();
			
			int lengthX = map.Width;
			int lengthY = map.Length;
			Vector3[] vertices = new Vector3[lengthX * lengthY * 6];
			int index = 0;
			
			for( int x = 0; x < lengthX; x++ ) {
				for( int y = 0; y < lengthY; y++ ) {
					Tile tile = map[x, y];
					drawInfo.CurrentTile = tile;
					bool leftExists = x > 0;
					bool behindExists = y > 0;
					bool rightExists = x < lengthX - 1;
					bool frontExists = y < lengthY - 1;
					
					if( leftExists && behindExists ) {
						drawInfo.LeftBehind = map[x - 1, y - 1];
					}
					if( leftExists && frontExists ) {
						drawInfo.LeftFront = map[x - 1, y + 1];
					}
					if( rightExists && behindExists ) {
						drawInfo.RightBehind = map[x + 1, y - 1];
					}
					if( rightExists && frontExists ) {
						drawInfo.RightFront = map[x + 1, y + 1];
					}
					
					if( leftExists ) {
						drawInfo.Left = map[x - 1, y];
					}
					if( behindExists ) {
						drawInfo.Behind = map[x, y - 1];
					}
					if( rightExists ) {
						drawInfo.Right = map[x + 1, y];
					}
					if( frontExists ) {
						drawInfo.Front = map[x, y + 1];
					}
					
					drawInfo.MapX = x;
					drawInfo.MapY = y;
					RenderTile( drawInfo, vertices, index );
					index += 6;
				}
			} // end for
			totalTriangles = drawInfo.TotalTriangles;
			return vertices;
		}	
		
		public override void End() {
			GL.Color3( 1f, 1f, 1f );
			GL.PolygonMode( MaterialFace.FrontAndBack, PolygonMode.Fill );
		}
		
		public void RenderTile( TileDrawInfo drawInfo, Vector3[] vertices, int index ) {
			Tile tile = drawInfo.CurrentTile;
			X = tile.X;
			Y = tile.Y;
			height = tile.Height;
			z1 = height;
			z2 = height;
			
			Tile? left = drawInfo.Left;
			Tile? right = drawInfo.Right;
			Tile? back = drawInfo.Behind;
			Tile? front = drawInfo.Front;
			
			Tile? leftBehind = drawInfo.LeftBehind;
			Tile? leftFront = drawInfo.LeftFront;
			Tile? rightBehind = drawInfo.RightBehind;
			Tile? rightFront = drawInfo.RightFront;
			
			bool leftHigher = left != null && left.Value.Height > z2;
			bool rightHigher = right != null && right.Value.Height > z1;
			bool backHigher = back != null && back.Value.Height > z2;
			bool frontHigher = front != null && front.Value.Height > z1;
			
			bool leftBehindHigher = leftBehind != null && leftBehind.Value.Height > height;
			bool leftFrontHigher = leftFront != null && leftFront.Value.Height > height;
			bool rightBehindHigher = rightBehind != null && rightBehind.Value.Height > height;
			bool rightFrontHigher = rightFront != null && rightFront.Value.Height > height;
			
			if( leftBehindHigher && !leftHigher && !backHigher ) {
				z2 += tileHeight;
				RenderTopCornerFace( drawInfo, vertices, index );
				return;
			}
			
			if( leftFrontHigher && !leftHigher && !frontHigher ) {
				z2 += tileHeight;
				RenderTopCornerFaceRotated( drawInfo, vertices, index );
				return;
			}
			
			if( rightFrontHigher && !rightHigher && !frontHigher ) {
				z1 += tileHeight;
				RenderTopCornerFace( drawInfo, vertices, index );
				return;
			}
			
			if( rightBehindHigher && !rightHigher && !backHigher ) {
				z1 += tileHeight;
				RenderTopCornerFaceRotated( drawInfo, vertices, index );
				return;
			}
			
			if( leftHigher ) {
				z2 += tileHeight;
				RenderTopFace( drawInfo, vertices, index );
				return;
			}
			
			if( rightHigher ) {
				z1 += tileHeight;
				RenderTopFace( drawInfo, vertices, index );
				return;
			}
			
			if( backHigher ) {
				z1 += tileHeight;
				RenderTopFaceRotated( drawInfo, vertices, index );
				return;
			}
			
			if( frontHigher ) {
				z2 += tileHeight;
				RenderTopFaceRotated( drawInfo, vertices, index );
				return;
			}
			RenderTopFace( drawInfo, vertices, index );
		}

		void RenderTopFace( TileDrawInfo drawInfo, Vector3[] vertices, int index ) {
			vertices[index] = new Vector3( X, z2, Y + 0.5f );
			vertices[index + 1] = new Vector3( X + 0.5f, z1, Y );
			vertices[index + 2] = new Vector3( X + tileWidth, z1, Y + 0.5f );
			
			vertices[index + 3] = new Vector3( X + tileWidth, z1, Y + 0.5f );
			vertices[index + 4] = new Vector3( X + 0.5f, z2, Y + tileWidth );
			vertices[index + 5] = new Vector3( X, z2, Y + 0.5f );
			drawInfo.TotalTriangles += 2;
		}
		
		void RenderTopFaceRotated( TileDrawInfo drawInfo, Vector3[] vertices, int index ) {
			vertices[index] = new Vector3( X, z1, Y + 0.5f );
			vertices[index + 1] = new Vector3( X + 0.5f, z1, Y );
			vertices[index + 2] = new Vector3( X + tileWidth, z2, Y + 0.5f );
			
			vertices[index + 3] = new Vector3( X + tileWidth, z2, Y + 0.5f );
			vertices[index + 4] = new Vector3( X + 0.5f, z2, Y + tileWidth );
			vertices[index + 5] = new Vector3( X, z1, Y + 0.5f );
			drawInfo.TotalTriangles += 2;
		}
		
		void RenderTopCornerFaceRotated( TileDrawInfo drawInfo, Vector3[] vertices, int index ) {
			vertices[index] = new Vector3( X, height, Y + 0.5f );
			vertices[index + 1] = new Vector3( X + 0.5f, z1, Y );
			vertices[index + 2] = new Vector3( X + tileWidth, height, Y + 0.5f );

			vertices[index + 3] = new Vector3( X + tileWidth, height, Y + 0.5f );
			vertices[index + 4] = new Vector3( X + 0.5f, z2, Y + tileWidth );
			vertices[index + 5] = new Vector3( X, height, Y + 0.5f );
			drawInfo.TotalTriangles += 2;
		}
		
		void RenderTopCornerFace( TileDrawInfo drawInfo, Vector3[] vertices, int index ) {
			vertices[index] = new Vector3( X + 0.5f, height, Y );
			vertices[index + 1] = new Vector3( X, z2, Y + 0.5f );
			vertices[index + 2] = new Vector3( X + 0.5f, height, Y + tileWidth );
			
			vertices[index + 3] = new Vector3( X + 0.5f, height, Y );
			vertices[index + 4] = new Vector3( X + tileWidth, z1, Y + 0.5f );
			vertices[index + 5] = new Vector3( X + 0.5f, height, Y + tileWidth );
			drawInfo.TotalTriangles += 2;
		}
	}
}