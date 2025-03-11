function(set_project_compile_options project_name)
    set(CLANG_COMPILE_OPTIONS
    -Wall
    -Wextra
    -Wpedantic
    -g -gcolumn-info -fuse-ld=lld
    $<$<OR:$<CONFIG:RELEASE>,$<CONFIG:MINSIZEREL>,$<CONFIG:RELWITHDEBINFO>>:-O3> # Optimize.
    #$<$<OR:$<CONFIG:RELEASE>,$<CONFIG:MINSIZEREL>>:-s> # Debug symbol stripping. [Requires GCC]
    $<$<CONFIG:DEBUG>:-O0> # No optimizations.
    )

    target_compile_options(${project_name} PRIVATE ${CLANG_COMPILE_OPTIONS} ${ARGN})
endfunction()
