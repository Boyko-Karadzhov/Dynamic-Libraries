
__declspec(dllexport) double reduction_sum(double* a, int length);

double reduction_sum(double* a, int length)
{
	int i;
	double sum = 0;
	
	// I want to check if we can pass NULL pointers successfully.
	if (0 == a)
	{
		return 0;
	}
	
	for (i = 0; i < length; i++)
		sum += a[i];
		
	return sum;
}