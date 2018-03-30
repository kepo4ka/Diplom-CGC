#pragma once

#include "IObject.h"
#include "Platform.h"

namespace GameWorld
{
	namespace Strategy
	{
		class IUnit: public IObject
		{
		public:
			virtual int GetHP() = 0;
			virtual int GetMaxHP() = 0;

			virtual int ChargingStatus() = 0;

			virtual unsigned int GetTeam() = 0;
		};
	}
}