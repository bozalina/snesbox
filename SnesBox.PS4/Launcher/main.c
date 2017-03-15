#include <assert.h>
#include <scebase.h>
#include <stdio.h>
#include <kernel.h>

#if _DEBUG
#include "../../aot/Debug/mono-aot.h"
#else
#include "../../aot/Release/mono-aot.h"
#endif

unsigned int sceLibcHeapDelayedAlloc = 1;
unsigned int sceLibcHeapExtentededAlloc = 1;
size_t sceLibcHeapSize = 200 * 1024 * 1024; //0xffffffffffffffffUL; // SCE_LIBC_HEAP_SIZE_EXTENDED_ALLOC_NO_LIMIT
size_t sceLibcHeapInitialSize = 200 * 1024 * 1024; // Initial heap is 200Mb

int main (int argc, char *argv[])
{
	MonoAssembly *assembly;

	g_setenv ("MONO_USE_DIRECT_MEMORY", "1", 1);
	g_setenv ("MONO_PATH", "/app0/", 1); /*FIXME this doesn't affect the location in main_assembly_name */
	//g_setenv ("MONO_LOG_LEVEL", "debug", 1);
	g_setenv ("MONO_GC_PARAMS", "nursery-size=1m", 1);
	//g_setenv ("MONO_GC_DEBUG", "2", 1);
#if _DEBUG
	g_setenv("MONO_IGNORE_SUSPEND_TIMEOUT", "0", 1);
#endif

	orbis_setup ();
	orbis_register_modules ();
	mono_jit_set_aot_only (1);
	mono_jit_init_version ("ORBIS", "v2.0.50727");
	assembly = mono_assembly_open (main_assembly_name, NULL);

	// Set the main game code to run on the first four cores, leaving
	// the last two for parallel A/V tasks
	ScePthread mainThread = scePthreadSelf();
	int ret = scePthreadSetaffinity(mainThread, (1 << 0) | (1 << 1) | (1 << 2) | (1 << 3));
	assert(ret == SCE_OK);

	mono_jit_exec (mono_domain_get (), assembly, argc, argv);

	return 0;
}