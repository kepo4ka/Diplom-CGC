#pragma once

#define INTERFACE_MEMBER(method) virtual method = 0
#define IMPLEMENT(method) virtual method override
#define BREAK __asm int 3