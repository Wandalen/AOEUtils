using System;

namespace aoetest {
	
	public abstract class Projectile {
		public double MotionX, MotionY, MotionZ;
		
		public double X, Y, Z;
		
		public abstract void PhysicsTick( double elapsed );
	}
	
	public sealed class TestProjectile : Projectile {
		
		
		static double CalculateMaxHeight( double theta, double range ) {
			// Breakdown of algorithm:
			
			// Assuming that the horizontal acceleration is 0,
			// d = ut + ( at^2 ) / 2 becomes d = ut
			// The variables known are d and theta.
			
			//     /|
			//   V/ |Vv
			//   /@_|
			//    Vh
			// tan( @ ) = Vv / Vh
			// Vh = Vv / tan( @ )
			// Therefore, d = ( Vv / tan( @ ) ) * t
			
			// v^2 = u^2 + 2ad
			// 0 = u^2 - 20d (assuming acceletation due to gravity = -10)
			// -u^2 = -20d
			// u = (20d)^0.5
			// Thefore, d = ( (20h)^0.5 / tan( @ ) ) * t
			
			// d = ut + ( at^2 ) / 2
			// (assuming that u is equal to 0, i.e. when falling from top of the parabolic curve path)
			// d = ( at^2 ) / 2
			// d = 5t^2
			// t = ( d / 5 )^0.5
			// The total time of flight will be twice the time to fall.
			
			// Therefore, d = ( (20h)^0.5 / tan( @ ) ) * 2( h / 5 )^0.5
			// Squaring everything gives:
			// d^2 = ( 20h * h * 4 ) / ( 5 * tan( @ )^2 )
			// d^2 = 80h^2 / ( 5 * tan( @ )^2 )
			// d^2 = 16h^2 / tan( @ )^2
			
			// Everything can there be multiplied by a power of 0.5 to give:
			// d = 4h / tan( @ )
			// d * tan( @ ) = 4h
			// h = ( d * tan( @ ) ) / 4
			return ( range * Math.Tan( theta ) ) / 4.0;
		}
		
		void CalculateMotion( double x1, double y1, double x2, double y2 ) {
			double yDist = y2 - y1;
			double xDist = x2 - x1;
			double horAngle = Math.Atan2( yDist, xDist );
			double verAngle = Math.PI / 4;
			
			double length = Math.Sqrt( xDist * xDist + yDist * yDist );
			
			double height = CalculateMaxHeight( verAngle, length );
			
			double ySpeed = Math.Sin( horAngle );
			double xSpeed = Math.Cos( horAngle );
		}
		
		
		
		public override void PhysicsTick( double elapsed ) {
		}
	}
}
