using System;
using System.Drawing;

namespace AITest
{
	internal class SteeringEngine
	{
		private readonly Graphics _graphics;
		private readonly Size _clientSize;
		private CustomObject[] _customObjects;

		public SteeringEngine(Graphics graphics, Size clientSize, int vehicles)
		{
			_graphics = graphics;
			_clientSize = clientSize;

			InitCars(vehicles);
			
			CustomObject.SetCarsData(ref _customObjects);
		}

		private void InitCars(int carCount)
		{
			_customObjects = new CustomObject[carCount];
			for (var i = 0; i < carCount; i++)
			{
				var car = new CustomObject(_graphics, _clientSize, SB.Seek, i);
				_customObjects[i] = car;
			}
			CustomObject.SetCarsData(ref _customObjects);
		}

		public void Step()
		{
			foreach (var vehicle in _customObjects)
			{
				vehicle.Move();
			}
		}
	}

	internal class SteeringBehaviors
	{
		private static readonly Random Random = new Random();

		public static Vector2 Wander(Graphics g, ref Vector2 wanderTarget, ref Vector2 currentPosition, ref Vector2 Velocity, ref Vector2 heading, float wanderRadius, float wanderDistance, int wanderJitter)
		{
			heading = Vector2.Normalize(Velocity);
			wanderTarget += new Vector2(Random.Next(-wanderJitter, wanderJitter), Random.Next(-wanderJitter, wanderJitter));
			wanderTarget = Vector2.Normalize(wanderTarget);
			wanderTarget *= wanderRadius / 2;
			var circleCenterG = new PointF((heading.X * wanderDistance + currentPosition.X) - wanderRadius / 2, (heading.Y * wanderDistance + currentPosition.Y) - wanderRadius / 2);
			var circleCenterM = new Vector2((heading.X * wanderDistance) + currentPosition.X, (heading.Y * wanderDistance) + currentPosition.Y);
			var pointOnCircle = new Vector2(circleCenterM.X + wanderTarget.X, circleCenterM.Y + wanderTarget.Y);
			g.DrawEllipse(Pens.LightGreen, new RectangleF(new PointF(circleCenterG.X, circleCenterG.Y), new SizeF(wanderRadius, wanderRadius)));
			g.FillEllipse(Brushes.LightYellow, new RectangleF(new PointF(pointOnCircle.X - 4, pointOnCircle.Y - 4), new SizeF(8, 8)));
			return Vector2.Subtract(pointOnCircle, currentPosition);

		}
	}
}