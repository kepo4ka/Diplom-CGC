#pragma once

class IObject
{
	public:
		double GetX() { return 0; }
		double GetY() { return 0; }
		double GetAngle() { return 0; }

		double GetRadius() { return 0; }

		unsigned int GetId() { return 0; }
		
		double AngleTo(IObject *to) { return 0; }
		double DistanceTo(IObject *to) { return 0; }

		double AngleTo(double x, double y) { return 0; }
		double DistanceTo(double x, double y) { return 0; }
};