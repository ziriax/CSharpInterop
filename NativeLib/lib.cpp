// dllmain.cpp : Defines the entry point for the DLL application.
#include "pch.h"


class Counter
{
public:
	typedef void ChangedCallback(int value);
	typedef std::function<ChangedCallback> ChangedFunctor;

	void onChange(ChangedFunctor&& callback)
	{
		_changed = callback;
	}


	void increment()
	{
		++_value;
		if (_changed)
			_changed(_value);
	}

private:
	int _value = 0;

	ChangedFunctor _changed;


};

#define ABI __declspec(dllexport)

extern "C"
{
	ABI Counter* counter_create()
	{
		return new Counter();
	}

	ABI void counter_delete(Counter const* counter)
	{
		return delete counter;
	}

	ABI void counter_increment(Counter* counter)
	{
		counter->increment();
	}

	ABI void counter_on_change(Counter* counter, Counter::ChangedCallback callback)
	{
		counter->onChange(callback);
	}
}


BOOL APIENTRY DllMain(HMODULE hModule,
	DWORD  ul_reason_for_call,
	LPVOID lpReserved
)
{
	switch (ul_reason_for_call)
	{
	case DLL_PROCESS_ATTACH:
	case DLL_THREAD_ATTACH:
	case DLL_THREAD_DETACH:
	case DLL_PROCESS_DETACH:
		break;
	}
	return TRUE;
}

