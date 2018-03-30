#pragma once

#include "Platform.h"
#include <cmath>

namespace GameWorld
{
	namespace Strategy
	{
		class IObject
		{
		public:
			virtual double GetX() = 0;
			virtual double GetY() = 0;
			virtual double GetAngle() = 0;

			virtual double GetRadius() = 0;

			virtual unsigned int GetId() = 0;

			double AngleTo(double x, double y)
			{
				double dirx = cos(GetAngle());
				double diry = sin(GetAngle());

				double normx = -diry;
				double normy = dirx;

				double deltax = x - GetX();
				double deltay = y - GetY();

				return atan2(deltax * normx + deltay * normy, deltax * dirx + deltay * diry);
			}

			double AngleTo(IObject *to)
			{
				return AngleTo(to->GetX(), to->GetY());
			}

			double DistanceTo(double x, double y)
			{
				double deltax = x - GetX();
				double deltay = y - GetY();

				return sqrt(deltax * deltax + deltay * deltay);
			}

			double DistanceTo(IObject *to)
			{
				return DistanceTo(to->GetX(), to->GetY());
			}
		};
	}
}