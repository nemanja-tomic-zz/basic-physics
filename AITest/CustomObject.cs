using System;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;

namespace AITest
{
	enum SB
	{
		None,
		Seek,
		Flee,
		Arrive,
		Pursuit,
		Evade,
		Wander,
		PathFollowing,
		Cohesion,
		Alignment,
		Separation,
		CF,
		FCAS,
		FCS,
		CS,
		CA,
		CAS
	}

	public struct Obstacles
	{
		public PointF Location;
		public int Size;
		public Obstacles(PointF location, int size)
		{
			Location = location;
			Size = size;
		}
	}

	internal class CustomObject
	{
		private static CustomObject[] _allObjects;

		//=========================================================//
		//general variables
		public static bool enableSpecialPath = false;
		private bool targetChanged;
		private float _mass;
		private int max_speed, max_force, _vehicleNo;
		private Point _currentCoordinates;
		private Vector2 velocity, acceleration, heading, steerForce, targetPosition;
		private Brush brushBack, brushWhite, brushCar;
		private Pen whitePen;
		private readonly Graphics _graphic;
		private static Random _random;
		private Size _clientSize;
		private SB SB;
		public static Obstacles[] obstacles = new Obstacles[10];    //not used
		private bool _xDirectionPositive;
		private bool _yDirectionPositive;
		//=========================================================//

		//=========================================================//
		//wander specific variables
		private Vector2 wanderTarget;
		public static float WanderRadius = 40, WanderDistance = 80;
		public static int WanderJitter = 10;
		//=========================================================//

		//=========================================================//
		//arrive specific variables
		public static int ArriveRadius = 100;
		//=========================================================//

		//=========================================================//
		//pathfollowing specific varibles
		Point[] specialPathPoints, pathPoints, sP1, sP2;
		int currentPathPoint, maxSpecialPathPoints, maxPathPoints;

		//=========================================================//
		//cohesion specific variables
		public static int CohesionRadius = 100;
		//=========================================================//

		//=========================================================//
		//flee specific variables
		public static int FOV = 100;    //field of view
		private Rectangle _currentPosition;
		//=========================================================//

		public CustomObject(Graphics graphicsObject, Size clientSize, SB SteeringBehavior, int vehicleNumber)
		{
			//=========================================================//
			//general initialization
			_vehicleNo = vehicleNumber;
			_random = new Random(GetSeed());
			_xDirectionPositive = Convert.ToBoolean(_random.Next(0, 2));
			_yDirectionPositive = Convert.ToBoolean(_random.Next(0, 2));
			_graphic = graphicsObject;
			_clientSize = clientSize;
			_mass = _random.Next(20, 80);
			max_force = 20;
			max_speed = 10;
			velocity = new Vector2(_random.Next(0, 10), _random.Next(0, 10));
			acceleration = new Vector2(0, 0);
			heading = Vector2.Normalize(velocity);
			_currentCoordinates = new Point(_random.Next(0, _clientSize.Width), _random.Next(0, _clientSize.Height));
			steerForce = new Vector2(0);
			//targetPosition = SBC.targetPosition;
			brushBack = Brushes.Black;
			brushWhite = Brushes.White;
			brushCar = Brushes.Blue;
			whitePen = Pens.White;
			targetChanged = false;
			SB = SteeringBehavior;
			//=========================================================//

			//=========================================================//
			//wander specific variables
			wanderTarget = new Vector2(0);
			//=========================================================//

			//=========================================================//

			//=========================================================//
			//pathfollowing s
			maxSpecialPathPoints = 40;
			maxPathPoints = 10;
			currentPathPoint = 0;
			specialPathPoints = new Point[maxSpecialPathPoints];
			pathPoints = new Point[maxPathPoints];
			sP1 = new Point[22];
			sP2 = new Point[18];
			NewPosition();
		}

		public static void SetCarsData(ref CustomObject[] cars)
		{
			_allObjects = cars;
		}

		public void Move()
		{
			var xRightIntersectingCar = _allObjects.FirstOrDefault(x => x._currentPosition.Left == _currentPosition.Right && HeightIntersects(x._currentPosition));
			var xLeftIntersectingCar = _allObjects.FirstOrDefault(x => x._currentPosition.Right == _currentPosition.Left && HeightIntersects(x._currentPosition));
			if (_currentPosition.Right >= _clientSize.Width || xRightIntersectingCar != null)
			{
				if (xRightIntersectingCar != null)
					xRightIntersectingCar._xDirectionPositive = true;
				_xDirectionPositive = false;
			}
			if (_currentPosition.X <= 0 || xLeftIntersectingCar != null)
			{
				if (xLeftIntersectingCar != null)
					xLeftIntersectingCar._xDirectionPositive = false;
				_xDirectionPositive = true;
			}

			var yBottomIntersectingCar = _allObjects.FirstOrDefault(x => x._currentPosition.Top == _currentPosition.Bottom && WidthIntersects(x._currentPosition));
			var yTopIntersectingCar = _allObjects.FirstOrDefault(x => x._currentPosition.Bottom == _currentPosition.Top && WidthIntersects(x._currentPosition));
			if (_currentPosition.Bottom >= _clientSize.Height || yBottomIntersectingCar != null)
			{
				if (yBottomIntersectingCar != null)
					yBottomIntersectingCar._yDirectionPositive = true;
				_yDirectionPositive = false;
			}
			if (_currentPosition.Y <= 0 || yTopIntersectingCar != null)
			{
				if (yTopIntersectingCar != null)
					yTopIntersectingCar._yDirectionPositive = false;
				_yDirectionPositive = true;
			}

			if (_xDirectionPositive)
				_currentPosition.X++;
			else
				_currentPosition.X--;

			if (_yDirectionPositive)
				_currentPosition.Y++;
			else
				_currentPosition.Y--;

			Draw();
		}

		private bool WidthIntersects(Rectangle otherCar)
		{
			if (otherCar.Right >= _currentPosition.Left && otherCar.Right <= _currentPosition.Right)
				return true;
			return otherCar.Left >= _currentPosition.Left && otherCar.Left <= _currentPosition.Right;
		}

		private bool HeightIntersects(Rectangle otherCar)
		{
			if (otherCar.Bottom >= _currentPosition.Top && otherCar.Bottom <= _currentPosition.Bottom)
				return true;
			return otherCar.Top >= _currentPosition.Top && otherCar.Top <= _currentPosition.Bottom;
		}

		private void NewPosition()
		{
			_currentPosition = new Rectangle(_currentCoordinates.X, _currentCoordinates.Y, 10, 10);
		}

		private void Draw()
		{
			_graphic.FillRectangle(_vehicleNo == 1 ? Brushes.Red : brushWhite, _currentPosition);
		}

		private int GetSeed()
		{
			var newGuid = Guid.NewGuid();
			return (int)((Convert.ToInt64(string.Join("", Regex.Replace(newGuid.ToString(), @"[a-zA-Z-]+", string.Empty).Take(4).ToArray())) * _vehicleNo) % 500);
		}
	}
}