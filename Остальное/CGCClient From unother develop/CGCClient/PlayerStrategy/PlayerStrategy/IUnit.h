#pragma once

#include "IObject.h"

class IUnit: public IObject
{
	public:
		int GetHP() { return 0; }
		int GetMaxHP() { return 0; }

		int ChargingStatus() { return 0; }

		unsigned int GetTeam() { return 0; }
};