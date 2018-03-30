#pragma once
#include "IUnit.h"

class IControllableUnit : public IUnit
{
	public:				
		void StartCharging() {};
		void Fire() {};
		void SetLeftControl(double control) {};
		void SetRightControl(double control) {};
};