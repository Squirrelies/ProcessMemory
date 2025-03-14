#ifndef PROCESSMEMORY_NATIVE_SIGSCAN_H
#define PROCESSMEMORY_NATIVE_SIGSCAN_H

#include "Common.h"
#include <stdint.h>

LIBRARY_EXPORT_API typedef struct
{
	uintptr_t startAddress;
	uintptr_t endAddress;
	uint8_t alignment;
	DWORD pageProtectionFlags;
} SIGSCAN_OPTIONS_T;

LIBRARY_EXPORT_API typedef struct
{
	size_t elements;
	void **pointers;
} SIGSCAN_RESULTS_T;

LIBRARY_EXPORT_API SIGSCAN_RESULTS_T signature_scan(const HANDLE processHandle, const char *pattern_str, const SIGSCAN_OPTIONS_T *options);
LIBRARY_EXPORT_API void free_sigscan_results(SIGSCAN_RESULTS_T *results);

#endif
