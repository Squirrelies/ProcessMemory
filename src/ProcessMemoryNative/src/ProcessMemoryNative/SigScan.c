#include "SigScan.h"
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
	uint8_t *bytes;
	bool *wildcards;
	size_t length;
} SIGSCAN_PATTERN_T;

SIGSCAN_PATTERN_T parse_pattern(const char *pattern_str)
{
	SIGSCAN_PATTERN_T result = {0};

	size_t str_len = strlen(pattern_str);
	size_t pattern_len = 0;

	for (size_t i = 0; i < str_len; i++)
	{
		if (pattern_str[i] != ' ')
			pattern_len++;
	}

	pattern_len = pattern_len / 2;

	result.bytes = malloc(pattern_len);
	result.wildcards = malloc(pattern_len);
	result.length = pattern_len;

	if (!result.bytes || !result.wildcards)
	{
		free(result.bytes);
		free(result.wildcards);
		result.bytes = NULL;
		result.wildcards = NULL;
		result.length = 0;
		return result;
	}

	size_t byte_index = 0;
	for (size_t i = 0; i < str_len && byte_index < pattern_len;)
	{
		// Skip spaces
		if (pattern_str[i] == ' ')
		{
			i++;
			continue;
		}

		// Ensure we have at least 2 characters to parse
		if (i + 1 >= str_len)
		{
			break;
		}

		// Check for wildcards
		if (pattern_str[i] == '?' && pattern_str[i + 1] == '?')
		{
			result.bytes[byte_index] = 0;
			result.wildcards[byte_index] = true;
			i += 2;
		}
		else
		{
			// Parse hex byte
			char hex[3] = {pattern_str[i], pattern_str[i + 1], '\0'};

			// Handle potential wildcards in middle of hex
			if (hex[0] == '?' || hex[1] == '?')
			{
				result.bytes[byte_index] = 0;
				result.wildcards[byte_index] = true;
			}
			else
			{
				result.bytes[byte_index] = (uint8_t)strtol(hex, NULL, 16);
				result.wildcards[byte_index] = false;
			}
			i += 2;
		}

		byte_index++;
	}

	return result;
}

// Function to check if a memory region matches our pattern
bool check_pattern_match(
    const uint8_t *memory,
    size_t memory_size,
    const SIGSCAN_PATTERN_T *pattern,
    uint8_t alignment)
{
	// If we don't have enough memory to match the pattern, return false
	if (memory_size < pattern->length)
	{
		return false;
	}

	// Check for alignment if specified
	if (alignment > 1)
	{
		uintptr_t addr = (uintptr_t)memory;
		if (addr % alignment != 0)
		{
			return false;
		}
	}

	// Check if the memory matches our pattern
	for (size_t i = 0; i < pattern->length; i++)
	{
		if (!pattern->wildcards[i] && memory[i] != pattern->bytes[i])
		{
			return false;
		}
	}

	return true;
}

// Function to add a pointer to our results
bool add_pointer(SIGSCAN_RESULTS_T *results, void *pointer)
{
	// Allocate or reallocate memory for our results
	void **new_pointers = realloc(results->pointers,
	                              (results->elements + 1) * sizeof(void *));

	if (!new_pointers)
	{
		return false;
	}

	results->pointers = new_pointers;
	results->pointers[results->elements] = pointer;
	results->elements++;

	return true;
}

// Main signature scanning function
SIGSCAN_RESULTS_T signature_scan(
    const HANDLE processHandle,
    const char *pattern_str,
    const SIGSCAN_OPTIONS_T *options)
{
	SIGSCAN_RESULTS_T results = {0};

	if (!options)
	{
		printf("Required parameter `%s` not supplied.\n", NAMEOF(options));
		return results;
	}

	// Parse the pattern
	SIGSCAN_PATTERN_T pattern = parse_pattern(pattern_str);
	if (!pattern.bytes || !pattern.wildcards)
	{
		printf("Failed to parse pattern\n");
		return results;
	}

	// Debug pattern
	printf("Pattern bytes: ");
	for (size_t i = 0; i < pattern.length && i < 20; i++)
	{
		printf("%02X", pattern.bytes[i]);
	}
	printf("\n");

	const size_t BUFFER_SIZE = 64 * 1024;
	uint8_t *buffer = malloc(BUFFER_SIZE);
	if (!buffer)
	{
		printf("Failed to allocate buffer memory\n");
		free(pattern.bytes);
		free(pattern.wildcards);
		return results;
	}

	// Query memory regions
	MEMORY_BASIC_INFORMATION mbi;
	uintptr_t address = options->startAddress;

	uintptr_t alignment_mask = options->alignment > 0 ? (options->alignment - 1) : 0;

	while (address < options->endAddress &&
	       VirtualQueryEx(processHandle, (LPCVOID)address, &mbi, sizeof(mbi)))
	{
		// Check if this region should be scanned
		if ((mbi.State == MEM_COMMIT) && (mbi.Protect & options->pageProtectionFlags))
		{
			// Calculate region bounds
			uintptr_t region_start = (uintptr_t)mbi.BaseAddress;
			uintptr_t region_end = region_start + mbi.RegionSize;

			// Ensure we don't go past our end address
			if (region_end > options->endAddress)
			{
				region_end = options->endAddress;
			}

			// *** ADDED OVERLAP HANDLING LIKE C# ***
			size_t overlap = pattern.length - 1;

			// Process the region in chunks
			for (uintptr_t chunk = region_start;
			     chunk < region_end;
			     chunk += BUFFER_SIZE - overlap)
			{ // Subtract overlap

				size_t chunk_size = BUFFER_SIZE;
				if (chunk + chunk_size > region_end)
				{
					chunk_size = region_end - chunk;
				}

				// Skip chunks smaller than pattern length
				if (chunk_size < pattern.length)
				{
					continue;
				}

				// Read memory
				SIZE_T bytes_read;
				if (ReadProcessMemory(processHandle, (LPCVOID)chunk, buffer, chunk_size, &bytes_read))
				{
					// If we read less than pattern length, skip this chunk
					if (bytes_read < pattern.length)
					{
						continue;
					}

					// Calculate how many positions we can check
					size_t scan_size = bytes_read - pattern.length + 1;

					// Determine scan start - skip overlap area for non-first chunks
					size_t scan_start = 0;
					if (chunk > region_start && overlap > 0)
					{
						scan_start = overlap;
					}

					// If alignment is required, adjust scan start to next aligned position
					if (options->alignment > 0)
					{
						uintptr_t aligned_addr = chunk + scan_start;
						if ((aligned_addr & alignment_mask) != 0)
						{
							// Calculate bytes to add to reach next aligned address
							size_t alignment_adjustment = options->alignment -
							                              (aligned_addr & alignment_mask);
							scan_start += alignment_adjustment;
						}
					}

					// Skip if scan_start is beyond valid range
					if (scan_start >= scan_size)
					{
						continue;
					}

					// Scan for pattern matches
					for (size_t i = scan_start; i < scan_size;)
					{
						// Apply alignment if specified
						if (options->alignment > 0)
						{
							uintptr_t aligned_addr = chunk + i;

							// If not aligned, move to next aligned position
							if ((aligned_addr & alignment_mask) != 0)
							{
								i += options->alignment -
								     (aligned_addr & alignment_mask);
								continue;
							}
						}

						// Check for pattern match
						bool match = true;
						for (size_t j = 0; j < pattern.length; j++)
						{
							// Skip wildcards
							if (pattern.wildcards[j])
							{
								continue;
							}

							// Check if byte matches
							if (buffer[i + j] != pattern.bytes[j])
							{
								match = false;
								break;
							}
						}

						if (match)
						{
							// Found a match, add it to results
							void *match_addr = (void *)(chunk + i);
							if (!add_pointer(&results, match_addr))
							{
								// Handle allocation failure
								printf("Failed to add match to results\n");
								goto cleanup;
							}
						}

						// Move to next position - use alignment if specified
						i += options->alignment > 0 ? options->alignment : 1;
					}
				}
				else
				{
					DWORD error = GetLastError();
					// Only report significant read errors, not expected ones
					if (error != ERROR_PARTIAL_COPY && error != ERROR_NOACCESS)
					{
						printf("ReadProcessMemory failed at address 0x%llx with error %lu\n",
						       (unsigned long long)chunk, error);
					}
				}
			}
		}

		// Move to the next region
		address = (uintptr_t)mbi.BaseAddress + mbi.RegionSize;
	}

cleanup:
	free(buffer);
	free(pattern.bytes);
	free(pattern.wildcards);

	return results;
}

void free_sigscan_results(SIGSCAN_RESULTS_T *results)
{
	free(results->pointers);
	results->pointers = NULL;
	results->elements = 0;
}
