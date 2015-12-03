using System;

namespace IsometricTests {

	public interface IRenderTarget {
		float Zoom { get; }
		
		float HorizontalViewOffset { get; }
		
		float VerticalViewOffset { get; }
		
		float DisplayWidth { get; }
		
		float DisplayHeight { get; }
	}
}
