#pragma once

#include "Platform.h"
#include "IUnit.h"

namespace GameWorld
{
	namespace Strategy
	{
		class IControllableUnit : public IUnit
		{
			public:				
				INTERFACE_MEMBER(void StartCharging());
				INTERFACE_MEMBER(void Fire());
				INTERFACE_MEMBER(void SetLeftControl(double control));
				INTERFACE_MEMBER(void SetRightControl(double control));
		};
	}
}