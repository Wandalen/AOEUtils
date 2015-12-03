using System;
using OpenTK;

namespace IsometricTests {
	//This is from Mark Morley's tutorial on frustum culling.
	//http://www.crownandcutlass.com/features/technicaldetails/frustum.html
	public sealed class FrustumCulling {
		public IsometricTest Window;
		
		float frustum00, frustum01, frustum02, frustum03;

		float frustum10, frustum11, frustum12, frustum13;

		float frustum20, frustum21, frustum22, frustum23;

		float frustum30, frustum31, frustum32, frustum33;

		float frustum40, frustum41, frustum42, frustum43;

		float frustum50, frustum51, frustum52, frustum53;
		
		public bool SphereInFrustum( float x, float y, float z, float radius ) {
			float d = 0;

			d = frustum00 * x + frustum01 * y + frustum02 * z + frustum03;
			if( d <= -radius )
				return false;
			d = frustum10 * x + frustum11 * y + frustum12 * z + frustum13;
			if( d <= -radius )
				return false;
			d = frustum20 * x + frustum21 * y + frustum22 * z + frustum23;
			if( d <= -radius )
				return false;
			d = frustum30 * x + frustum31 * y + frustum32 * z + frustum33;
			if( d <= -radius )
				return false;
			d = frustum40 * x + frustum41 * y + frustum42 * z + frustum43;
			if( d <= -radius )
				return false;
			d = frustum50 * x + frustum51 * y + frustum52 * z + frustum53;
			if( d <= -radius )
				return false;

			return true;
		}
		
		/// <summary> Calculates the frustum planes. </summary>
		/// <remarks>
		/// From the current OpenGL modelview and projection matrices,
		/// calculate the frustum plane equations (Ax+By+Cz+D=0, n=(A,B,C))
		/// The equations can then be used to see on which side points are.
		/// </remarks>
		public unsafe void CalcFrustumEquations() {
			float t;

			// Retrieve matrices from OpenGL
			Matrix4 modelView = Window.ModelView;
			Matrix4 projection = Window.Projection;
			Matrix4 frustum = projection;
			Matrix4.Mult( ref modelView, ref projection, out frustum );

			float* clip1 = (float*)&frustum;
			
			// Extract the numbers for the RIGHT plane
			frustum00 = clip1[3] - clip1[0];
			frustum01 = clip1[7] - clip1[4];
			frustum02 = clip1[11] - clip1[8];
			frustum03 = clip1[15] - clip1[12];

			// Normalize the result
			t = (float)Math.Sqrt(frustum00 * frustum00 + frustum01 * frustum01 + frustum02 * frustum02);
			frustum00 /= t;
			frustum01 /= t;
			frustum02 /= t;
			frustum03 /= t;

			// Extract the numbers for the LEFT plane
			frustum10 = clip1[3] + clip1[0];
			frustum11 = clip1[7] + clip1[4];
			frustum12 = clip1[11] + clip1[8];
			frustum13 = clip1[15] + clip1[12];

			// Normalize the result
			t = (float)Math.Sqrt(frustum10 * frustum10 + frustum11 * frustum11 + frustum12 * frustum12);
			frustum10 /= t;
			frustum11 /= t;
			frustum12 /= t;
			frustum13 /= t;

			// Extract the BOTTOM plane
			frustum20 = clip1[3] + clip1[1];
			frustum21 = clip1[7] + clip1[5];
			frustum22 = clip1[11] + clip1[9];
			frustum23 = clip1[15] + clip1[13];

			// Normalize the result
			t = (float)Math.Sqrt(frustum20 * frustum20 + frustum21 * frustum21 + frustum22 * frustum22);
			frustum20 /= t;
			frustum21 /= t;
			frustum22 /= t;
			frustum23 /= t;

			// Extract the TOP plane
			frustum30 = clip1[3] - clip1[1];
			frustum31 = clip1[7] - clip1[5];
			frustum32 = clip1[11] - clip1[9];
			frustum33 = clip1[15] - clip1[13];

			// Normalize the result
			t = (float)Math.Sqrt(frustum30 * frustum30 + frustum31 * frustum31 + frustum32 * frustum32);
			frustum30 /= t;
			frustum31 /= t;
			frustum32 /= t;
			frustum33 /= t;

			// Extract the FAR plane
			frustum40 = clip1[3] - clip1[2];
			frustum41 = clip1[7] - clip1[6];
			frustum42 = clip1[11] - clip1[10];
			frustum43 = clip1[15] - clip1[14];

			// Normalize the result
			t = (float)Math.Sqrt(frustum40 * frustum40 + frustum41 * frustum41 + frustum42 * frustum42);
			frustum40 /= t;
			frustum41 /= t;
			frustum42 /= t;
			frustum43 /= t;

			// Extract the NEAR plane
			frustum50 = clip1[3] + clip1[2];
			frustum51 = clip1[7] + clip1[6];
			frustum52 = clip1[11] + clip1[10];
			frustum53 = clip1[15] + clip1[14];

			// Normalize the result
			t = (float)Math.Sqrt(frustum50 * frustum50 + frustum51 * frustum51 + frustum52 * frustum52);
			frustum50 /= t;
			frustum51 /= t;
			frustum52 /= t;
			frustum53 /= t;
		}
	}
}