function(set_project_compile_definitions project_name)
    set(CLANG_COMPILE_DEFINITIONS
    $<$<OR:$<CONFIG:RELEASE>,$<CONFIG:MINSIZEREL>,$<CONFIG:RELWITHDEBINFO>>:RELEASE>
    $<$<CONFIG:DEBUG>:DEBUG>
    UNICODE
    _UNICODE
    WIN32_LEAN_AND_MEAN
    )

    target_compile_definitions(${project_name} PRIVATE ${CLANG_COMPILE_DEFINITIONS} ${ARGN})
endfunction()
