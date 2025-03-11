#include "Common.h"
#include <stdint.h>
#include <stdio.h>
#include <time.h>
#include <windows.h>

inline struct timespec GetWin32TimeSpec()
{
	FILETIME ft;
	GetSystemTimeAsFileTime(&ft);

	ULARGE_INTEGER uli = {.LowPart = ft.dwLowDateTime, .HighPart = ft.dwHighDateTime};
	struct timespec returnValue = {.tv_sec = (uli.QuadPart / 10000000ULL) - 11644473600ULL, .tv_nsec = (uli.QuadPart % 10000000ULL) * 100};

	return returnValue;
}

inline const char *GetLocalDateTime()
{
	static const char CURRENT_TIME_FORMAT_STRING[] = "%s.%09ld";

	struct timespec ts = GetWin32TimeSpec();

	char tempISO8601Buffer[20];
	strftime(tempISO8601Buffer, sizeof tempISO8601Buffer, "%F %T", localtime(&ts.tv_sec));

	int returnSize = snprintf(NULL, 0, CURRENT_TIME_FORMAT_STRING, tempISO8601Buffer, ts.tv_nsec);
	char *returnValue = (char *)malloc(returnSize + 1);
	snprintf(returnValue, returnSize + 1, CURRENT_TIME_FORMAT_STRING, tempISO8601Buffer, ts.tv_nsec);

	return returnValue;
}

inline const char *GetUTCDateTime()
{
	static const char CURRENT_TIME_FORMAT_STRING[] = "%s.%09ld";

	struct timespec ts = GetWin32TimeSpec();

	char tempISO8601Buffer[20];
	strftime(tempISO8601Buffer, sizeof tempISO8601Buffer, "%F %T", gmtime(&ts.tv_sec));

	int returnSize = snprintf(NULL, 0, CURRENT_TIME_FORMAT_STRING, tempISO8601Buffer, ts.tv_nsec);
	char *returnValue = (char *)malloc(returnSize + 1);
	snprintf(returnValue, returnSize + 1, CURRENT_TIME_FORMAT_STRING, tempISO8601Buffer, ts.tv_nsec);

	return returnValue;
}
