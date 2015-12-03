using System;

namespace IsometricTests.Renderers {
	
	public abstract class TileRenderer : IDisposable {
		
		public virtual void Start( int terrainTexture ) {
		}
		
		public abstract void RenderTiles( TerrainMap map, ref int totalTriangles );
		
		public virtual void End() {
		}
		
		protected virtual void CleanupResources() {
		}
		
		public virtual void Dispose() {
			CleanupResources();
		}
	}
}