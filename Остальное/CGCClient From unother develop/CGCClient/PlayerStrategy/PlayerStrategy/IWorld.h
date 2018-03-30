#pragma once

#include "IUnit.h"
#include "IObject.h"
#include "IControllableUnit.h"
#include <vector>

class IWorld
{
	public:
		std::vector<IUnit*> GetEnemies() { return std::vector<IUnit*>(); }
		std::vector<IControllableUnit*> GetOwnUnits() { return std::vector<IControllableUnit*>(); }
		std::vector<IObject*> GetObstacles() { return std::vector<IObject*>(); }
		double GetWidth() { return 0; }
		double GetHeight() { return 0; }
		unsigned int GetTick() { return 0; }
		void SetName(const std::string name) { }
		int GetCurrentTeam() { return 0; }
};