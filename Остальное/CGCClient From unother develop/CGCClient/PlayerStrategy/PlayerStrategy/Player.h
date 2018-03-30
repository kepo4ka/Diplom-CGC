#pragma once
#include "IWorld.h"

namespace Client
{
	namespace Strategy
	{
		class Player
		{
			public:				
				void Move(IWorld* world);
		};
	}
}

