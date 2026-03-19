.section .text
.global ReturnBufferBridge
.type ReturnBufferBridge, %function

ReturnBufferBridge:
    // X0..X7 = Original Arguments (this, parent, etc.)
    // X8     = The Return Buffer Pointer
    // X16    = The address of the Managed Trampoline

    // 1. Save necessary registers on the stack
    stp x30, x16, [sp, #-16]!
    stp x6,  x7,  [sp, #-16]!
    stp x4,  x5,  [sp, #-16]!
    stp x2,  x3,  [sp, #-16]!
    stp x0,  x1,  [sp, #-16]!

    // 3. Pass the Return Buffer Pointer (X8) to the TLS storage for the current thread
    mov x0, x8
    bl  SetReturnBuffer

    // 3. Restore the saved registers from the stack
    ldp x0,  x1,  [sp], #16
    ldp x2,  x3,  [sp], #16
    ldp x4,  x5,  [sp], #16
    ldp x6,  x7,  [sp], #16
    ldp x30, x16, [sp], #16

    // 4. Jump to the managed trampoline address we stored in X16
    br  x16

