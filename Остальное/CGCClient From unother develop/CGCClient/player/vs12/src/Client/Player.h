#pragma once
#include "Common/IWorld.h"

using namespace GameWorld::Strategy;

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

