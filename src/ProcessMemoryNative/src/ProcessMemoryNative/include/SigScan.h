#ifndef PROCESSMEMORY_NATIVE_SIGSCAN_H
#define PROCESSMEMORY_NATIVE_SIGSCAN_H

#include "Common.h"
#include <ctype.h>
#include <stdint.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <windows.h>

#if __STDC_VERSION__ < 202311L
#include <stdbool.h>
#endif

typedef struct
{
	uintptr_t startAddress;
	uintptr_t endAddress;
	uint8_t alignment;
	DWORD pageProtectionFlags;
} SIGSCAN_OPTIONS_T;

typedef struct
{
	uint8_t *bytes;
	bool *wildcards;
	size_t length;
} SIGSCAN_PATTERN_T;

typedef struct
{
	size_t elements;
	void **pointers;
} SIGSCAN_RESULTS_T;

const SIGSCAN_OPTIONS_T DEFAULT_OPTIONS = {
    .startAddress = 0x0UL,
#if defined(ARCH_x86_64)
    .endAddress = 0x7FFFFFFFFFFFULL,
#elif defined(ARCH_x86_32)
    .endAddress = 0xFFFFFFFFUL,
#endif
    .alignment = 1,
    .pageProtectionFlags = PAGE_READWRITE | PAGE_EXECUTE_READWRITE};

SIGSCAN_PATTERN_T parse_pattern(const char *pattern_str);
bool check_pattern_match(const uint8_t *memory, size_t memory_size, const SIGSCAN_PATTERN_T *pattern, uint8_t alignment);
bool add_pointer(SIGSCAN_RESULTS_T *results, void *pointer);

LIBRARY_EXPORT_API SIGSCAN_RESULTS_T signature_scan(const HANDLE processHandle, const char *pattern_str, const SIGSCAN_OPTIONS_T *options);
LIBRARY_EXPORT_API SIGSCAN_RESULTS_T signature_scan_default(const HANDLE processHandle, const char *pattern);
LIBRARY_EXPORT_API void free_sigscan_results(SIGSCAN_RESULTS_T *results);

#endif
