#pragma once

#if NATIVE_EXPORTS
	#define DllExport __declspec(dllexport)  
#else
	#define DllExport __declspec(dllimport)  
#endif

extern "C"
{
	DllExport int    sum(int* ptr, size_t size);

	DllExport void*  stream_new(size_t length);
	DllExport void   stream_delete(void* ptr);
	DllExport size_t stream_getsize(void* ptr);
	DllExport size_t stream_getposition(void* ptr);
	DllExport size_t stream_read(void* ptr, int* buffer, size_t size);
	DllExport void   stream_reset(void* ptr);
}
