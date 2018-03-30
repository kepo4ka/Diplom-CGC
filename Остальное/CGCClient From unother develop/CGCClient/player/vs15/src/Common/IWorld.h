#pragma once

#include "Platform.h"
#include "IUnit.h"
#include "IObject.h"
#include "IControllableUnit.h"
#include <vector>

namespace GameWorld
{
	namespace Strategy
	{
		class IWorld
		{
			public:
				virtual std::vector<IUnit*> GetEnemies() = 0;
				virtual std::vector<IControllableUnit*> GetOwnUnits() = 0;
				virtual std::vector<IObject*> GetObstacles() = 0;
				virtual double GetWidth() = 0;
				virtual double GetHeight() = 0;
				virtual unsigned int GetTick() = 0;
				virtual void SetName(const std::string name) = 0;
				virtual int GetCurrentTeam() = 0;
		};
	}
}