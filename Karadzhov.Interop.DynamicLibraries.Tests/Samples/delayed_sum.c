#include<windows.h>

__declspec(dllexport) int delayed_sum(int a, int b);

int delayed_sum(int a, int b)
{
	Sleep(3000);
	return a+b;
}